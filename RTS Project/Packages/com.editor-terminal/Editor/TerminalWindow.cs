using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTerminal
{
    /// <summary>
    /// Unity Editor 内置终端窗口。
    /// Window → Editor Terminal 打开，可 dock 到任意面板区域。
    /// </summary>
    public class TerminalWindow : EditorWindow
    {
        private static readonly List<TerminalWindow> _instances = new();

        private TerminalProcess _process;

        private readonly List<string> _outputLines = new();
        private const int MaxLines = 5000;
        private string _inputText = "";
        private Vector2 _scrollPos;
        private bool _scrollToBottom = true;

        private GUIStyle _outputStyle;
        private GUIStyle _inputStyle;

        private bool _shouldFocusInput;

        [MenuItem("Window/Editor Terminal")]
        public static void Open()
        {
            var window = GetWindow<TerminalWindow>("终端", true);
            window.minSize = new Vector2(300, 150);
            window.Show();
        }

        private void OnEnable()
        {
            _instances.Add(this);
            InitStyles();
            StartShell();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            _instances.Remove(this);
            CleanupProcess();
        }

        private void CleanupProcess()
        {
            _process?.Dispose();
            _process = null;
        }

        private void OnEditorUpdate()
        {
            _process?.DrainToMainThread(line =>
            {
                if (_outputLines.Count >= MaxLines)
                    _outputLines.RemoveAt(0);
                _outputLines.Add(line);
                _scrollToBottom = true;
            });

            if (_process != null && _process.HasOutput)
                Repaint();
        }

        private void OnGUI()
        {
            EnsureStyles();

            DrawToolbar();
            DrawOutput();
            DrawInput();

            if (Event.current.control && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.L)
            {
                _outputLines.Clear();
                Repaint();
                Event.current.Use();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("终端", EditorStyles.miniLabel);

            if (GUILayout.Button("重启", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                RestartShell();
            }

            if (GUILayout.Button("清屏", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _outputLines.Clear();
                Repaint();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutput()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(
                _scrollPos,
                false, false,
                GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.ScrollWheel)
                _scrollToBottom = false;

            if (_outputLines.Count == 0)
            {
                EditorGUILayout.LabelField(
                    "终端已启动。输入命令后按 Enter 执行...",
                    new GUIStyle(EditorStyles.centeredGreyMiniLabel) { padding = new RectOffset(0, 0, 20, 0) });
            }
            else
            {
                foreach (string line in _outputLines)
                {
                    EditorGUILayout.LabelField(line, _outputStyle,
                        GUILayout.ExpandWidth(true),
                        GUILayout.MinHeight(EditorGUIUtility.singleLineHeight));
                }

                if (_scrollToBottom && Event.current.type == EventType.Repaint)
                {
                    _scrollPos.y = float.MaxValue;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawInput()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label(">", EditorStyles.miniLabel, GUILayout.Width(15));

            GUI.SetNextControlName("TerminalInput");
            _inputText = EditorGUILayout.TextField(_inputText, _inputStyle,
                GUILayout.ExpandWidth(true));

            if (_shouldFocusInput)
            {
                EditorGUI.FocusTextInControl("TerminalInput");
                _shouldFocusInput = false;
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl() == "TerminalInput"
                && !string.IsNullOrEmpty(_inputText))
            {
                string cmd = _inputText;
                _inputText = "";
                AppendEcho(cmd);
                _process?.Send(cmd);
                _scrollToBottom = true;
                Event.current.Use();
                Repaint();
            }

            if (GUILayout.Button("⏎", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                if (!string.IsNullOrEmpty(_inputText))
                {
                    string cmd = _inputText;
                    _inputText = "";
                    AppendEcho(cmd);
                    _process?.Send(cmd);
                    _scrollToBottom = true;
                    GUI.FocusControl(null);
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void StartShell()
        {
            CleanupProcess();
            _process = new TerminalProcess();
            _process.Start();
            _outputLines.Add("<color=grey>—— Editor Terminal v1.0 ——</color>");
            _outputLines.Add("<color=grey>提示: 输入命令后按 Enter 执行 | Ctrl+L 清屏 | 重启按钮重新连接</color>");
            _outputLines.Add("");
            _shouldFocusInput = true;
        }

        private void RestartShell()
        {
            _outputLines.Add("<color=yellow>—— 正在重启终端... ——</color>");
            StartShell();
        }

        private void AppendEcho(string cmd)
        {
            _outputLines.Add($"<color=cyan>> {cmd}</color>");
        }

        private void InitStyles()
        {
            Font monoFont = null;
            foreach (string fontName in new[] { "Consolas", "Courier New", "Source Code Pro", "Menlo" })
            {
                monoFont = Font.CreateDynamicFontFromOSFont(fontName, 13);
                if (monoFont != null) break;
            }

            _outputStyle = new GUIStyle(EditorStyles.label)
            {
                font = monoFont ?? EditorStyles.label.font,
                fontSize = 13,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(4, 4, 1, 1),
            };

            _inputStyle = new GUIStyle(EditorStyles.toolbarTextField)
            {
                font = monoFont ?? EditorStyles.label.font,
                fontSize = 13,
            };
        }

        private void EnsureStyles()
        {
            if (_outputStyle == null) InitStyles();
        }
    }
}
