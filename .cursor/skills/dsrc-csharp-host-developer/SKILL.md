---
name: dsrc-csharp-host-developer
description: Develops, debugs, and verifies DSRC/RSU/OBU features in WJ_ComprehensiveTest_Desk (WinForms, DesktopComm, lane trade). Use for new features, UI, DLL/serial, frame parsing, and pass/fail logic. Do not use for C++/MFC migration audits.
---

# DSRC C# 上位机开发

## 工作流程

### 第一阶段：需求分析与开发计划

1. 明确目标功能、页面或测试项、操作入口、输入/输出、日志/截图、取消/重试，以及本次不做的范围。
2. 盘点 sln、窗体、Designer、事件、服务、P/Invoke、INI、结果文件和类似流程；避开无关用户改动。
3. 用表格列出每步帧类型、方向、模板、动态字段来源、长度、等待、重试、是否等响应、EventReport 收尾。未知标待确认。
4. 分别写明 DLL 请求/响应码、是否收到回复、外层长度、APDU、业务比较、超时和整例通过条件。DLL 成功 ≠ 测试通过。
5. 计划至少含：需求与代码映射、Designer 改动、服务职责、帧时序、成败/超时/取消/恢复、验证安排、未决问题。
6. 展示计划并等待用户确认。确认前只读。

### 第二阶段：实施

7. 核对确认范围和受影响文件后再改。
8. 窗体：校验、UI 状态、取消令牌、调服务。服务：协议、设备、解析、判定。P/Invoke 留在 `Native/`。
9. 控件按 Designer → 显式事件 → 后置处理函数；禁止反射或隐式 Lambda。
10. 先把已确认帧流程写成清晰步骤；存完整下行/回复/DLL 码/时间再解析；动态字段只从已确认来源取。
11. DLL 失败、句柄无效、无响应、长度异常、错误状态字、取消、结果写入失败都要有明确结果；改过的台发/信道/自动更新在结束时恢复。
12. 临时兼容只覆盖明确用例或帧，写依据、`TODO`、删除条件。

### 第三阶段：验证与交付

13. 离线核对帧顺序、长度、动态字段、APDU 边界、状态字、超时、重试、恢复。
14. 失败路径：DLL 失败、无回复、错误响应、长度异常、取消、初始化失败、结果保存失败。
15. 构建 `Debug|x86`；界面查 Designer/DPI；有硬件再报实机。
16. 交付列修改文件、已验证、未执行、仍需实机确认。更新 `项目文件说明.md`。

## 参考资料

- 涉及 DSRC 帧、BST/VST、GetSecure、TransferChannel、SetMMI、EventReport、方向、偏移或长度时，读取 `references/送检指南--DSRC协议解析参考.pdf` 的相关章节。
- 涉及联网收费用户卡、ESAM、文件标识、文件结构、读写偏移或数据定义时，读取 `references/附件3-附录3全国高速公路电子不停车收费联网用户卡、ESAM文件结构与数据定义.pdf` 的相关章节。
- 文档、DLL 和实机数据冲突时保留原始数据并报告冲突；不自行解释未知字段。
