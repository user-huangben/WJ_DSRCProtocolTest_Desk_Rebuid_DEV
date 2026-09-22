---
name: operation-manual-docx
description: Generates a screenshot-annotated DOCX operation manual for the WinForms host. Use only when the user says 生成操作说明, 生成用户手册, or 生成 DOCX 操作说明.
disable-model-invocation: true
---

# 上位机 DOCX 操作说明

## 适用范围

根据当前工程已实现界面和测试流程，在 `.sln` 同级生成 `操作说明.docx`（当前为 `WJ_ComprehensiveTest_Desk_20260922/操作说明.docx`）。使用 `scripts/build_manual.py` 生成文档，截图及标注后的生成物放在 `WJ_ComprehensiveTest_Desk_20260922/artifacts/manual/`。只写代码能验证的功能，不把占位用例写成已实现。

## 内容要求

### 内容层级

1. 产品级一级 Tab。
2. 一级 Tab 下的二级页面或功能 Tab。
3. 页面中的分组、面板和区域。
4. 区域中每个可操作控件。
5. 最底层测试分组、子项和叶子用例。

每个控件至少说明：所在区域、用途、输入规则、操作方式、操作后效果、前置条件、成败提示，以及是否影响测试帧、日志或结果文件。测试用例还须说明执行顺序、关键判断、通过/失败表现和结果保存位置。

## 专项内容

### 透传界面

透传页必须逐项盘点台发设置、帧解析区、实时上下行帧、帧操作区、交易帧/IC 卡/ESAM/BUF、打开/保存/清空/上移/下移/删除/测试/修改等按钮和状态提示。写清按钮是改界面、维护待发列表、发设备帧还是只显示结果。

## 取证与隐私

先核对 Designer、后置文件、事件、测试树、服务入口、结果写入和构建输出，建立「控件—事件—服务—结果」对应。截图覆盖一级 Tab、已实现二级页、重要操作区和代表性用例。标注用副本；编号与说明表一一对应。密钥、密文、完整卡数据、正式配置或敏感日志必须遮盖。无界面证据标「待截图/待确认」。

## 输出格式

封面、适用版本、目录、启动与退出、一级 Tab、逐页控件说明、测试用例、结果/日志、异常处理、当前限制。用 Word 标题层级。控件密集区用「标号截图 + 说明表」。交付必须是 `.docx`。

## 工作流程与验收

创建或修改 DOCX 前，先读取 `references/document-generation-standard.md`，并执行一次：

`node scripts/mark_artifact_operation_started.mjs --operation-kind create --expected-output-count 1 --output-format docx`

使用 `python scripts/build_manual.py` 生成文档。写入后使用：

`python scripts/render_docx.py WJ_ComprehensiveTest_Desk_20260922/操作说明.docx --output_dir WJ_ComprehensiveTest_Desk_20260922/artifacts/manual/rendered`

将 DOCX 渲成逐页 PNG，在 100% 视图检查标题、截图、标注、表格分页、中文字体、页眉页脚和空白页。用户只要求新增 skill 时，不生成操作说明文件。
