---
name: operation-manual-docx
description: Generates a screenshot-annotated DOCX operation manual for the WinForms host in WJ_ComprehensiveTest_Desk. Use only when the user says 生成操作说明, 生成用户手册, or 生成 DOCX 操作说明. Do not use when the user only asks to add or edit a skill.
disable-model-invocation: true
---

# DSRC C# 操作说明

## 工作流程

### 第一阶段：确认范围与界面证据

1. 仅当用户说「生成操作说明」「生成用户手册」或「生成 DOCX 操作说明」时进入本流程。用户只要求新增或修改 skill 时，不生成操作说明文件。
2. 输出固定为 `.sln` 同级 `WJ_ComprehensiveTest_Desk/操作说明.docx`。截图及标注产物放在 `WJ_ComprehensiveTest_Desk/artifacts/manual/`。
3. 创建或修改 DOCX 前，读取 `references/document-generation-standard.md`。
4. 核对 Designer、后置文件、事件、测试树、服务入口、结果写入和构建输出，建立「控件—事件—服务—结果」对应。只写代码能验证的功能，不把占位用例写成已实现。
5. 按层级盘点：产品级一级 Tab；一级 Tab 下的二级页面或功能 Tab；页面中的分组、面板和区域；区域中每个可操作控件；最底层测试分组、子项和叶子用例。
6. 每个控件至少准备：所在区域、用途、输入规则、操作方式、操作后效果、前置条件、成败提示，以及是否影响测试帧、日志或结果文件。测试用例还须准备执行顺序、关键判断、通过/失败表现和结果保存位置。
7. 透传页逐项盘点台发设置、帧解析区、实时上下行帧、帧操作区、交易帧/IC 卡/ESAM/BUF、打开/保存/清空/上移/下移/删除/测试/修改等按钮和状态提示。写清按钮是改界面、维护待发列表、发设备帧还是只显示结果。
8. 截图覆盖一级 Tab、已实现二级页、重要操作区和代表性用例。标注用副本；编号与说明表一一对应。密钥、密文、完整卡数据、正式配置或敏感日志必须遮盖。无界面证据标「待截图/待确认」。

### 第二阶段：编写文档

9. 执行一次 `node scripts/mark_artifact_operation_started.mjs --operation-kind create --expected-output-count 1 --output-format docx`。
10. 使用 `python scripts/build_manual.py` 生成文档。
11. 文档含封面、适用版本、目录、启动与退出、一级 Tab、逐页控件说明、测试用例、结果/日志、异常处理、当前限制。用 Word 标题层级。控件密集区用「标号截图 + 说明表」。交付文件必须是 `.docx`。

### 第三阶段：渲染验收与交付

12. 写入后执行 `python scripts/render_docx.py WJ_ComprehensiveTest_Desk/操作说明.docx --output_dir WJ_ComprehensiveTest_Desk/artifacts/manual/rendered`。
13. 将 DOCX 渲成逐页 PNG，在 100% 视图检查标题、截图、标注、表格分页、中文字体、页眉页脚和空白页。不通过则改文档并重新渲染，直到页面通过。
14. 对外交付最终 `.docx`。渲染 PNG 只作验收产物。列出已写入页面、标为待截图/待确认的项，以及未写成已实现的占位用例。

## 参考资料

- 版式、字体、表格、截图编号和验收清单见 `references/document-generation-standard.md`。
- 生成脚本：`scripts/build_manual.py`。渲染脚本：`scripts/render_docx.py`。开始标记：`scripts/mark_artifact_operation_started.mjs`。
