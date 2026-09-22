# AGENTS.md

## 项目描述

`WJ_ComprehensiveTest_Desk_20260922`：OBU综合测试上位机

### 技术栈

- 语言与 UI：C#、WinForms。
- 运行时：.NET Framework 4.8。
- 平台：x86（加载 32 位设备 DLL）。
- 程序集：`WJ_ComprehensiveTest_Desk_20260922`。

### 目录与分层

- 解决方案：`WJ_ComprehensiveTest_Desk_20260922/WJ_ComprehensiveTest_Desk_20260922.sln`。
- 源码目录：`WJ_ComprehensiveTest_Desk_20260922/WJ_ComprehensiveTest_Desk_20260922/`。
- 入口窗体：`MainForm`。
- 测试窗体：`WanjiStandardTestForm`（`WanjiStandardTestCaseHandlers`）、`BeijingLocalStandardTestForm`、`WanjiCpcStandardTestForm`、`WatchmanBroadcastTestForm`、`TestCaseLogicForm`；布局控件 `Controls/TransparentTestControl`。
- 服务：`DesktopCommService`、`LaneTransactionService`、`SoftTradeCryptoService`、`LegacyLaneDllService`、`OneChipIssueActivationService`、`BeijingRespondBroadcastService`、`BeijingRespondDifferentMacService`、`BeijingTypicalTransactionService`；结果 `TestResultFileWriter` / `TestResultScreenshotWriter`。
- P/Invoke：`Native/DesktopCommNativeMethods.cs`、`Native/LegacyLaneNativeMethods.cs`。
- 受控 DLL：`DeskTopComm.dll`、`mwCardReader.dll`、`wdcrwv.dll`、`gmssl.dll`。
- 受控 INI：`SetMe.ini`、`BST_Locked.ini`、`BST_Locked_Sutong.ini`。

### 构建与验证

- 常规配置：`Debug|x86`。
- 另有 `Release|x86`。
- 默认输出：项目下 `bin/Debug/`、`bin/Release/`。

### 代码约定

- 窗体后置：校验、UI 状态、调用服务。
- 协议、设备 DLL、结果文件：`Services/`。
- P/Invoke：`Native/`。

## 项目边界

- 用户当前指令优先；规则和 Skill 不得扩大授权。
- 只做当前明确要求；缺 ABI/字段/长度/算法/判定依据先问。
- DLL 未就绪必须失败；无真实 RSU/OBU/读卡器不得声称实机通过。
- 不擅自移动、替换、重编码受控 DLL/INI/密钥/协议资料；未要求不 git init、不提交推送、不删历史输出。
- 原 C++/MFC 只读核对指定外部行为。`.backups/`、`bin/`、`obj/`、`.vs/` 不作实现来源。
- 未经确认不得变更 .NET Framework 4.8、WinForms 和 x86 技术基线。
- 新代码和依赖必须与该基线兼容，不使用仅适用于 .NET Core 或 .NET 5+ 的 API。
- 不通过修改平台位数规避原生 DLL 加载错误。

## Cursor 配置

### 加载分工

| 路径 | Cursor 如何加载 |
| --- | --- |
| `AGENTS.md` | 根目录始终读取 |
| `.cursor/rules/project.mdc` | alwaysApply |
| `.cursor/rules/csharp-dsrc-host.mdc` | 编辑现行项目源码时按 glob 附加 |
| `.cursor/skills/` | 按 description 自动选用，或 `/` 点名 |

### Skill 路由

- 开发/排障：`dsrc-csharp-host-developer`。移植/对照旧程序：`dsrc-csharp-host-migration`。
- 「生成迭代记录」：`iteration-records`。「生成操作说明」：`operation-manual-docx`。
