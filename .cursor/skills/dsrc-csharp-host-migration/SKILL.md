---
name: dsrc-csharp-host-migration
description: Audits legacy C++/MFC DSRC, RSU, and OBU test flows and lands proven cases on the current C# WinForms host in WJ_ComprehensiveTest_Desk. Use for migration audits, equivalence mapping, and porting confirmed cases. Do not use for new C# features that are not a legacy-flow port.
---

# DSRC C# 上位机移植

## 工作流程

### 第一阶段：旧上位机流程审计

1. 建立证据表：定位旧界面入口、线程或调度、最终执行函数、初始化台发、配置/INI、密钥和算法来源。
2. 还原完整时序：记录 BST/VST、GetSecure、Transfer、SetMMI、EventReport、等待、重试、轮次、动态字段变化及各分支清理动作。
3. 还原判定：分别记录 DLL 返回码、是否收到回复、外层长度、APDU 状态、业务数据比较和最终通过条件。DLL 成功 ≠ 测试通过。
4. 逐字节校对：外层 TransferChannel、DataList/APDU 边界、CLA/INS/P1/P2/Lc/Data/Le、随机数、密钥索引和 MAC 输入；动态字段必须标注来源。
5. 对照旧组帧代码或模板、旧实机日志和新实测帧，列出帧顺序、字段、时序、判定和异常清理差异。
6. 无法证实的内容标记为待确认；只有证据充分的结论才能进入当前 C# 实现。旧行为与文档或实机冲突时保留冲突证据，不根据相邻用例补全字段。
7. 缺失信息若会实质改变协议字段、长度、算法、ABI、密钥或验收判定，停止并请求依据。展示流程/判定矩阵和帧差异记录并等待用户确认。确认前只读。

### 第二阶段：移植实施

8. 核对确认范围和受影响文件后再改。定位当前标准测试窗体、测试树节点、稳定用例路径和现有服务；确认是新增叶子、扩展现有处理器还是新增专用服务。
9. 为每个旧用例记录新用例名称、树路径、处理器入口、配置来源、帧序列、状态判定、结果文件和截图行为；未实现项保持不可执行，不创建伪造通过路径。
10. Designer 只维护静态控件和显式事件；窗体后置只做选择、状态、取消和服务调用；协议组帧/解析、DLL 调用、配置读取和结果判定放入职责明确的服务类。
11. 按旧流程把初始化、动态字段、每一帧请求/回复、LLC/MACID/BID/UnixTime/算法、等待/重试和 EventReport 映射到新服务；调用方显式传递参数，不依赖未说明的全局状态。
12. 为每个叶子提供清晰可跳转的处理器入口，复用现有父子勾选、顺序批量、统一取消令牌、日志、结果文件和汇总截图；未连接设备时必须明确失败或停在可诊断状态。
13. 双击用例只展示流程和判定依据，标明已迁移、待确认或原版未实现；不得在说明入口隐式启动通信。
14. 临时兼容判断限定到精确用例或帧，写明依据、`TODO` 和删除条件。仅影响格式或非关键 UI 的小差异可沿用当前项目风格并在交付中注明。

### 第三阶段：对照、验证与交付

15. 离线比较旧模板/日志与新组帧的帧顺序、长度、动态字段、APDU 边界、状态字、LLC 和 MAC 输入。
16. 失败路径：超时、无回复、错误状态、长度异常、DLL 返回失败、取消和 EventReport 收尾。
17. 构建 `Debug|x86`；受影响窗体查 Designer 能打开、事件能双击跳转；有硬件再按同一矩阵做实机。
18. 交付列出已完成映射、与原程序确认的差异、未实现/待确认项、构建结果和仍需实机确认的范围。更新 `项目文件说明.md`。

## 参考资料

- 涉及 DSRC 帧、BST/VST、GetSecure、TransferChannel、SetMMI、EventReport、方向、偏移或长度时，读取 `references/送检指南--DSRC协议解析参考.pdf` 的相关章节。
- 涉及联网收费用户卡、ESAM、文件标识、文件结构、读写偏移或数据定义时，读取 `references/附件3-附录3全国高速公路电子不停车收费联网用户卡、ESAM文件结构与数据定义.pdf` 的相关章节。
- 文档、DLL 和实机数据冲突时保留原始数据并报告冲突；不自行解释未知字段。
