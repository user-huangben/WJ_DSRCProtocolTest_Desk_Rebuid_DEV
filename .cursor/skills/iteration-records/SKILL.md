---
name: iteration-records
description: Updates the current-version table and appends change points and involved files to 迭代记录.md beside the solution in WJ_ComprehensiveTest_Desk. Use only when the user says 生成迭代记录 or 生成变更记录. Do not use for 生成操作说明, ordinary development, builds, or tests.
disable-model-invocation: true
---

# DSRC C# 迭代记录

## 工作流程

### 第一阶段：确认范围与素材

1. 仅当用户明确说「生成迭代记录」或「生成变更记录」时进入本流程。
2. 用户说「生成操作说明」时，改用 `operation-manual-docx`；本技能不创建操作说明。
3. 普通开发、构建或测试不生成记录。
4. 定位 `WJ_ComprehensiveTest_Desk/WJ_ComprehensiveTest_Desk.sln`，记录写在其同级 `迭代记录.md`。
5. 读取并更新文首「当前版本」表。新条目只写变更点和涉及文件。不写验证结果、协议影响、未完成项、后续计划或交付说明。
6. 不记录密钥、密文、完整敏感卡数据、设备私密配置或未脱敏日志。

### 第二阶段：汇总并写入

7. 按现行工程事实改写「当前版本」各字段；不删该表。
8. 不覆盖已有历史条目；在「迭代记录」下追加新日期小节。标题与已有条目冲突时先提示。
9. 每个新条目只含两节：变更点、涉及文件。格式见参考资料。

### 第三阶段：核对与交付

10. 核对文首有当前版本表，新条目只有变更点和涉及文件，且没有敏感字段。
11. 交付写明写入路径和本次小节标题。

## 参考资料

```markdown
## 当前版本

- 上位机名称：WJ_ComprehensiveTest_Desk
- 版本日期：yyyy-MM-dd
- 程序集版本：1.0.0.0
- 目标框架：.NET Framework 4.8
- 构建平台：x86
- 工作区根目录：现行工作区根
- 解决方案：WJ_ComprehensiveTest_Desk/WJ_ComprehensiveTest_Desk.sln
- 项目文件：WJ_ComprehensiveTest_Desk/WJ_ComprehensiveTest_Desk/WJ_ComprehensiveTest_Desk.csproj
- 程序集与输出 EXE：WJ_ComprehensiveTest_Desk_20260922
- C# 命名空间：WJ_ComprehensiveTest_Desk

## 迭代记录

### yyyy-MM-dd 简短主题

#### 变更点

- 一条变更。

#### 涉及文件

- `相对路径`
```
