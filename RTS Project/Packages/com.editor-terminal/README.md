# Editor Terminal

Unity Editor 内置终端面板——在编辑器内直接运行命令行。

## 功能

- 内嵌 PowerShell / zsh / bash 终端
- ANSI 颜色支持（自动转为 Unity RichText）
- 可 dock 到任意 Unity 面板区域
- 重启、清屏、Ctrl+L
- 自动滚动 / 手动上滚查看历史

## 安装

### 方式一：本地路径

1. 将 `com.editor-terminal/` 文件夹复制到你的 Unity 项目的 `Packages/` 目录下
2. Unity 会自动识别并编译

```
你的项目/
├── Assets/
├── Packages/
│   └── com.editor-terminal/   ← 放这里
├── ProjectSettings/
└── ...
```

### 方式二：Git URL

1. 将 package 推送到 GitHub
2. Unity → Window → Package Manager → `+` → "Add package from git URL"
3. 输入 `https://github.com/你的用户名/unity-editor-terminal.git`

### 方式三：Package Manager 本地引用

1. Unity → Window → Package Manager → `+` → "Add package from disk"
2. 选择 `com.editor-terminal/package.json`

## 使用

1. Unity 菜单栏 → **Window → Editor Terminal**
2. 在终端窗口底部的输入框输入命令，按 Enter 执行
3. 工具栏按钮：重启（重新连接 shell）、清屏
4. 快捷键：**Ctrl+L** 清屏

## 兼容性

- Unity 2021.3 LTS 及以上
- Windows (PowerShell)、macOS (zsh)、Linux (bash)

## 自定义 shell

修改 `TerminalProcess.cs` 中 `Start()` 的参数：

```csharp
// 使用 WSL
process.Start(shellPath: "wsl.exe");

// 使用 Git Bash
process.Start(shellPath: @"C:\Program Files\Git\bin\bash.exe", arguments: "--login");
```

## License

MIT
