using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EditorTerminal
{
    /// <summary>
    /// 管理一个外部 shell 进程，异步读取 stdout/stderr，通过 ConcurrentQueue 安全交付到主线程。
    /// </summary>
    public class TerminalProcess : IDisposable
    {
        private Process _process;
        private readonly ConcurrentQueue<string> _outputQueue = new();
        private readonly ConcurrentQueue<string> _errorQueue = new();

        /// <summary>是否有待读取的输出行</summary>
        public bool HasOutput => !_outputQueue.IsEmpty || !_errorQueue.IsEmpty;

        /// <summary>进程是否在运行</summary>
        public bool IsRunning => _process != null && !_process.HasExited;

        /// <summary>进程退出码（未退出时为 null）</summary>
        public int? ExitCode => _process?.HasExited == true ? _process.ExitCode : null;

        public static bool ConvertAnsiToRichText { get; set; } = true;

        private static readonly Regex SgrRegex = new(@"\x1b\[([\d;]*)m", RegexOptions.Compiled);

        private static readonly string[] ColorMap = {
            "black", "red", "lime", "yellow",
            "blue", "magenta", "cyan", "white",
        };

        private static readonly string[] BrightColorMap = {
            "grey", "#FF6666", "#66FF66", "#FFFF66",
            "#6666FF", "#FF66FF", "#66FFFF", "white",
        };

        /// <summary>
        /// 将包含 ANSI SGR 转义码的字符串转换为 Unity RichText。
        /// 示例: "\x1b[32mgreen\x1b[0m" → "&lt;color=lime&gt;green&lt;/color&gt;"
        /// </summary>
        public static string AnsiToUnityRichText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int fgColor = -1;
            bool bold = false;

            string result = SgrRegex.Replace(input, match =>
            {
                string[] codes = match.Groups[1].Value.Split(';');

                if (codes.Length == 1 && string.IsNullOrEmpty(codes[0]))
                {
                    fgColor = -1;
                    bold = false;
                    return "";
                }

                foreach (string c in codes)
                {
                    if (!int.TryParse(c, out int code)) continue;

                    switch (code)
                    {
                        case 0: fgColor = -1; bold = false; break;
                        case 1: bold = true; break;
                        case 22: bold = false; break;
                        case >= 30 and <= 37: fgColor = code - 30; break;
                        case >= 90 and <= 97: fgColor = code - 90 + 8; break;
                        case 39: fgColor = -1; break;
                    }
                }

                return "";
            });

            if (fgColor >= 0)
            {
                string[] map = fgColor < 8 ? ColorMap : BrightColorMap;
                string color = fgColor < 8 ? ColorMap[fgColor] : BrightColorMap[fgColor - 8];
                string tags = $"<color={color}>";
                if (bold) tags += "<b>";
                result = tags + result;
                if (bold) result += "</b>";
                result += "</color>";
            }
            else if (bold)
            {
                result = "<b>" + result + "</b>";
            }

            return result;
        }

        /// <summary>
        /// 启动 shell 进程。默认会自动检测系统可用 shell。
        /// </summary>
        /// <param name="shellPath">shell 可执行文件路径。留空则自动检测。</param>
        /// <param name="arguments">shell 启动参数。留空则使用默认。</param>
        public void Start(string shellPath = null, string arguments = null)
        {
            if (string.IsNullOrEmpty(shellPath))
                shellPath = DetectShell();

            if (string.IsNullOrEmpty(arguments))
                arguments = GetDefaultArgs(shellPath);

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shellPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8,
                },
                EnableRaisingEvents = true,
            };

            _process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    _outputQueue.Enqueue(e.Data);
            };
            _process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    _errorQueue.Enqueue(e.Data);
            };
            var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            _process.StartInfo.EnvironmentVariables["PATH"] = $"{userPath};{machinePath}";

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        /// <summary>向进程 stdin 发送一行文本</summary>
        public void Send(string input)
        {
            if (!IsRunning) return;
            try
            {
                _process.StandardInput.WriteLine(input);
                _process.StandardInput.Flush();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// 取出所有待处理的输出行（在主线程调用）。
        /// </summary>
        /// <param name="onLine">每行输出回调。如果启用了 ANSI 转换，行内容已经是 RichText 格式。</param>
        public void DrainToMainThread(Action<string> onLine)
        {
            while (_outputQueue.TryDequeue(out string line))
            {
                if (ConvertAnsiToRichText)
                    line = AnsiToUnityRichText(line);
                onLine(line);
            }
            while (_errorQueue.TryDequeue(out string line))
            {
                if (ConvertAnsiToRichText)
                    line = AnsiToUnityRichText(line);
                onLine($"<color=red>[ERR]</color> {line}");
            }
        }

        public void Dispose()
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit(1000);
                    _process.Dispose();
                }
            }
            catch { }
            finally
            {
                _process = null;
            }
        }

        private static string DetectShell()
        {
#if UNITY_EDITOR_WIN
            return "powershell.exe";
#elif UNITY_EDITOR_OSX
            return "/bin/zsh";
#elif UNITY_EDITOR_LINUX
            return "/usr/bin/bash";
#else
            return "powershell.exe";
#endif
        }

        private static string GetDefaultArgs(string shellPath)
        {
            string name = System.IO.Path.GetFileName(shellPath).ToLowerInvariant();
            if (name.Contains("powershell") || name.Contains("pwsh"))
                return "-NoLogo -NoExit";
            return "";
        }
    }
}
