---
name: iteration-records
description: Writes host version and change notes to 迭代记录.md next to the .sln. Use only when the user says 生成迭代记录. For 生成操作说明 use operation-manual-docx.
disable-model-invocation: true
---

# 上位机迭代记录

## 适用范围

记录文件必须与 `.sln` 同级（当前为 `WJ_DSRCProtocolTest_Desk_Net/迭代记录.md`），不得写入 `bin`、`obj` 或运行输出目录。

### 触发与边界

- 用户明确说「生成迭代记录」时，读取项目名称、程序集版本、最近确认的代码变更和验证结果，生成或追加迭代记录。
- 用户明确说「生成操作说明」时，转由 `operation-manual-docx`；本 skill 不创建操作说明。
- 普通开发、构建或测试不自动生成记录。
- 用户尚未确认记录格式时，只展示模板并等待确认。
- 不记录密钥、密文、完整敏感卡数据、设备私密配置或未脱敏日志。

## 输入与前置检查

1. 定位 `WJ_DSRCProtocolTest_Desk_Net/WJ_DSRCProtocolTest_Desk_Net_20260920.sln`，记录写在其同级目录。
2. 读取项目名称、程序集 `WJ_DSRCProtocolTest_Desk_Net_20260920`、目标框架、平台、版本后缀和日期。
3. 汇总变更文件、功能变化、协议/界面影响、验证命令和结果。
4. 区分「已实现」「待确认」「未执行实机验证」「后续计划」。
5. 不覆盖用户已有记录；已有文件追加新版本或先提示冲突。

## 输出与交付

`迭代记录.md` 每个版本一个一级标题，至少包含：版本、日期、变更摘要、详细变更、验证结果、未完成项和后续计划。
