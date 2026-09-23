# AGENTS.md

## 项目描述

`WJ_ComprehensiveTest_Desk`：独立 DSRC 协议测试上位机。

### 技术栈

- 语言与 UI：C#、WinForms。
- 运行时：.NET Framework 4.8。
- 平台：x86（加载 32 位设备 DLL）。
- 程序集与输出 exe：`WJ_ComprehensiveTest_Desk_20260922`（与解决方案/源码目录名不同，后者不带日期后缀）。
- C# 命名空间与 `RootNamespace`：`WJ_ComprehensiveTest_Desk`。

### 目录与分层

- 工作区根：本文件、`.cursor/`、`.gitignore`、`.editorconfig`。
- 解决方案：`WJ_ComprehensiveTest_Desk/WJ_ComprehensiveTest_Desk.sln`（同级 `项目文件说明.md`、`迭代记录.md`）。
- 源码目录：`WJ_ComprehensiveTest_Desk/WJ_ComprehensiveTest_Desk/`。
- 入口窗体：`MainForm`。
- 测试窗体：`WanjiStandardTestForm`（`WanjiStandardTestCaseHandlers`）、`BeijingLocalStandardTestForm`、`WanjiCpcStandardTestForm`、`WatchmanBroadcastTestForm`、`TestCaseLogicForm`；布局控件 `Controls/TransparentTestControl`。
- 服务：`DesktopCommService`、`LaneTransactionService`、`SoftTradeCryptoService`、`LegacyLaneDllService`、`OneChipIssueActivationService`、`BeijingRespondBroadcastService`、`BeijingRespondDifferentMacService`、`BeijingTypicalTransactionService`；结果 `TestResultFileWriter` / `TestResultScreenshotWriter`。
- P/Invoke：`Native/DesktopCommNativeMethods.cs`、`Native/LegacyLaneNativeMethods.cs`。
- 受控 DLL：`DeskTopComm.dll`、`mwCardReader.dll`、`wdcrwv.dll`、`gmssl.dll`。
- 受控 INI：`SetMe.ini`、`BST_Locked.ini`、`BST_Locked_Sutong.ini`。

### 构建

- 常规配置：`Debug|x86`。
- 另有 `Release|x86`。
- 默认输出：项目下 `bin/Debug/`、`bin/Release/`。

窗体后置、`Services/`、`Native/` 及 .NET 5+ API / 平台位数约束见 `.cursor/rules/csharp-dsrc-host.mdc`。

## 项目边界

- 用户当前指令优先；规则和 Skill 不得扩大授权。
- 只做当前明确要求；缺 ABI/字段/长度/算法/判定依据先问。
- DLL 未就绪必须失败；无真实 RSU/OBU/读卡器不得声称实机通过。
- 不擅自移动、替换、重编码受控 DLL/INI/密钥/协议资料；未要求不 git init、不提交推送、不删历史输出。
- 原 C++/MFC 只读核对指定外部行为。`.backups/`、`bin/`、`obj/`、`.vs/` 不作实现来源。
- 未经确认不得变更 .NET Framework 4.8、WinForms 和 x86 技术基线。
