# 杀戮尖塔 2 快速重开 Mod

这是一个给《杀戮尖塔 2》使用的快速重开 Mod。

它会在暂停菜单中加入一个“快速重开”按钮。点击后不会去模拟“保存并退出 -> 主界面 -> 继续游戏”的界面操作，而是直接读取游戏当前的自动存档，并走继续游戏对应的底层加载流程，把当前房间恢复到最近一次自动存档时的状态。

## 功能说明

- 在单人模式的暂停菜单中加入“快速重开”按钮
- 点击后重新载入当前自动存档
- 多人模式下自动禁用该按钮
- 当前没有可用自动存档时自动禁用该按钮
- 支持中文按钮文本

## 适用范围

这个 Mod 的目标是“回到当前房间最近一次自动存档的状态”。

如果你要的是“整局重新开始一把，回到选角色界面之前”，那不是这个 Mod 当前实现的功能。

## 工程结构

- `MainFile.cs`：Mod 初始化与 Harmony 补丁注册
- `PauseMenuPatches.cs`：暂停菜单按钮注入、按钮状态刷新、手柄/键盘焦点链重建
- `QuickRestartService.cs`：自动存档读取与重新加载逻辑
- `FastRestart.json`：Mod 清单文件
- `FastRestart/localization/zhs/gameplay_ui.json`：中文按钮文本

## 构建环境

需要准备以下环境：

- .NET 9 SDK
- 《杀戮尖塔 2》本体

当前工程已按“只装 DLL 即可运行”的方式配置，不依赖 `.pck` 资源包。

## 路径配置

工程默认会尝试自动发现 Steam 的游戏安装目录。

如果没有识别到，可以手动编辑 `Directory.Build.props`，填入你自己的游戏路径。例如：

```xml
<Project>
  <PropertyGroup>
    <GodotPath>C:/megadot/MegaDot_v4.5.1-stable_mono_win64.exe</GodotPath>
    <Sts2Path>D:/Steam/steamapps/common/Slay the Spire 2</Sts2Path>
  </PropertyGroup>
</Project>
```

说明：

- 当前版本不依赖 `.pck`，所以不配置 `GodotPath` 也能直接编译 DLL
- 如果你将来想扩展为带资源导出的版本，再补 `GodotPath` 即可

## 编译方法

在项目根目录执行：

```powershell
dotnet build -c Release
```

编译成功后，工程会自动把以下文件复制到游戏目录：

```text
<游戏目录>/mods/FastRestart/
```

生成的主要文件包括：

- `FastRestart.dll`
- `FastRestart.json`

## 安装方法

如果你是自己编译：

1. 执行 `dotnet build -c Release`
2. 确认游戏目录下出现 `mods/FastRestart/`
3. 确认其中包含 `FastRestart.dll` 和 `FastRestart.json`

如果你是发给别人使用：

只需要把下面两个文件打包发出去即可：

- `FastRestart.dll`
- `FastRestart.json`

## 使用方法

进入一局单人游戏后：

1. 按 `Esc` 打开暂停菜单
2. 点击“快速重开”
3. 当前房间会恢复到最近一次自动存档时的状态

## 发布到 GitHub

如果你要把这个项目传到 GitHub，可以在项目根目录执行：

```powershell
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/你的用户名/你的仓库名.git
git push -u origin main
```

如果还没有配置 Git 用户信息，先执行：

```powershell
git config --global user.name "你的GitHub名字"
git config --global user.email "你的邮箱"
```

## 许可证

当前仓库默认使用 `MIT License`。
