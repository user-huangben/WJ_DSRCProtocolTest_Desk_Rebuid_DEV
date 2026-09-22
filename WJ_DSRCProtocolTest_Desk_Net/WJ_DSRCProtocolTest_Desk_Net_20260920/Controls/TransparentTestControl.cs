using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WJ_DSRCProtocolTest_Desk_Net.Services;

namespace WJ_DSRCProtocolTest_Desk_Net.Controls
{
    /// <summary>
    /// 提供与原上位机透传页对应的预置帧、帧序列编辑、文件读写、执行和上下行显示功能。
    /// </summary>
    public partial class TransparentTestControl : UserControl
    {
        private const int MaximumFrameBytes = 1024;
        private const int DefaultTimeoutMilliseconds = 500;
        private const string Transfer4FrameText = "00 03 08 00 00 01 40 F7 91 05 01 03 18 02 05 07 00 A4 00 00 02 3F 00 05 00 B0 81 00 05 07 00 A4 00 00 02 DF 01 0F 00 B4 00 00 0A 00 00 00 00 11 11 11 11 01 40 05 00 B0 8A 00 02";
        private DesktopCommService _desktopCommService;

        private readonly TransparentFrameTemplate[] _protocolTemplates =
        {
            new TransparentFrameTemplate("请选择交易帧", null),
            new TransparentFrameTemplate("ETC 联机 BST", "00 01 FF FF FF FF 50 03 91 C0 01 08 FE 01 53 0F 20 81 00 01 41 83 29 A0 1A 00 04 00 2B 00"),
            new TransparentFrameTemplate("浙江标识 BST", "00 01 FF FF FF FF 50 03 91 C0 02 00 9D 49 54 74 03 0A 01 01 42 87 2B 00 1A 02 32 33 00"),
            new TransparentFrameTemplate("GetSecure", "00 02 08 00 00 01 40 77 91 05 01 00 14 80 01 00 00 15 D4 7B D9 60 DA 47 DD 40 00 00"),
            new TransparentFrameTemplate("Transfer 1", "00 03 08 00 00 01 40 F7 91 05 01 03 18 01 02 05 00 B2 01 CC 2B 05 80 5C 00 02 04"),
            new TransparentFrameTemplate("Transfer 2", "00 03 08 00 00 01 40 77 91 05 01 03 18 01 02 10 80 50 03 02 0B 01 00 00 00 00 01 34 00 01 0F CF 30 80 DC AA C8 2B AA 29 00 34 01 08 FE 01 1A A1 DD 02 01 03 00 27 10 02 C0 B6 CB D5 41 4E 35 39 35 5A 00 00 00 00 34 01 00 00 01 1A A1 DD 02 00 00"),
            new TransparentFrameTemplate("Transfer 3", "00 03 08 00 00 01 40 F7 91 05 01 03 18 01 01 14 80 54 01 00 0F 00 02 79 55 20 14 02 27 19 24 50 E8 48 2E 6E"),
            new TransparentFrameTemplate("Transfer 4", Transfer4FrameText),
            new TransparentFrameTemplate("SetMMI", "00 04 08 00 00 01 40 77 99 05 01 04 1A 00"),
            new TransparentFrameTemplate("Event_report", "00 05 08 00 00 01 40 03 99 60 01 00")
        };

        private readonly TransparentFrameTemplate[] _icTemplates =
        {
            new TransparentFrameTemplate("请选择 IC 卡指令", null),
            new TransparentFrameTemplate("读 IC 卡 0015 文件", "00 03 08 00 00 01 40 F7 91 05 01 03 18 01 01 05 00 B0 95 00 2B"),
            new TransparentFrameTemplate("读 IC 卡 0012 文件", "00 03 08 00 00 01 40 F7 91 05 01 03 18 01 01 05 00 B0 92 00 28"),
            new TransparentFrameTemplate("读 IC 卡 0009 文件", "00 03 08 00 00 01 40 F7 91 05 01 03 18 01 01 05 00 B0 89 00 67")
        };

        private readonly TransparentFrameTemplate[] _esamTemplates =
        {
            new TransparentFrameTemplate("请选择 ESAM 指令", null),
            new TransparentFrameTemplate("选择 ESAM 3F00 目录", "00 03 08 00 00 01 40 F7 91 05 01 03 18 02 01 07 00 A4 00 00 02 3F 00"),
            new TransparentFrameTemplate("读取 ESAM 系统信息", "00 03 08 00 00 01 40 F7 91 05 01 03 18 02 01 05 00 B0 81 00 29"),
            new TransparentFrameTemplate("读取 ESAM 芯片序列号", "00 03 08 00 00 01 40 F7 91 05 01 03 18 02 01 05 80 F6 00 03 04")
        };

        /// <summary>
        /// 初始化透传测试控件及原上位机对应的交易帧、IC 卡和 ESAM 预置项。
        /// </summary>
        public TransparentTestControl()
        {
            // 构造阶段先建立 Designer 管理的静态控件，再装载仅用于选择的预置帧数据。
            InitializeComponent();
            BindTemplates(comboBoxProtocolTemplate, _protocolTemplates);
            BindTemplates(comboBoxIcTemplate, _icTemplates);
            BindTemplates(comboBoxEsamTemplate, _esamTemplates);
        }

        /// <summary>
        /// 绑定主窗体持有的台发通信服务，使透传测试复用同一 DLL 句柄且不自行打开设备。
        /// </summary>
        /// <param name="desktopCommService">主窗体已创建的台发服务；不能为 <see langword="null"/>。</param>
        /// <exception cref="ArgumentNullException">传入服务为空时抛出。</exception>
        internal void AttachService(DesktopCommService desktopCommService)
        {
            _desktopCommService = desktopCommService ?? throw new ArgumentNullException(nameof(desktopCommService));
        }

        /// <summary>
        /// 将指定模板集合绑定到下拉框，并默认选中提示项。
        /// </summary>
        /// <param name="comboBox">承载模板名称的下拉框。</param>
        /// <param name="templates">待绑定的模板数组，首项应为提示项。</param>
        private static void BindTemplates(ComboBox comboBox, IEnumerable<TransparentFrameTemplate> templates)
        {
            comboBox.Items.Clear();
            comboBox.Items.AddRange(templates.Cast<object>().ToArray());
            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// 把交易帧下拉框当前选中的预置帧加入待执行序列。
        /// </summary>
        /// <param name="sender">触发事件的“添加”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonAddProtocolTemplate_Click(object sender, EventArgs e)
        {
            // 交易帧入口统一复用模板校验和序列追加逻辑。
            AddSelectedTemplate(comboBoxProtocolTemplate);
        }

        /// <summary>
        /// 把 IC 卡下拉框当前选中的预置帧加入待执行序列。
        /// </summary>
        /// <param name="sender">触发事件的“添加”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonAddIcTemplate_Click(object sender, EventArgs e)
        {
            // IC 卡入口统一复用模板校验和序列追加逻辑。
            AddSelectedTemplate(comboBoxIcTemplate);
        }

        /// <summary>
        /// 把 ESAM 下拉框当前选中的预置帧加入待执行序列。
        /// </summary>
        /// <param name="sender">触发事件的“添加”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonAddEsamTemplate_Click(object sender, EventArgs e)
        {
            // ESAM 入口统一复用模板校验和序列追加逻辑。
            AddSelectedTemplate(comboBoxEsamTemplate);
        }

        /// <summary>
        /// 校验下拉框选择并将模板的规范化十六进制文本追加到序列。
        /// </summary>
        /// <param name="comboBox">当前模板来源下拉框。</param>
        private void AddSelectedTemplate(ComboBox comboBox)
        {
            TransparentFrameTemplate template = comboBox.SelectedItem as TransparentFrameTemplate;
            if (template == null || string.IsNullOrEmpty(template.FrameText))
            {
                SetExecutionStatus("请先选择要添加的预置帧。", true);
                return;
            }

            // 模板也经过与手工输入相同的解析，防止错误常量进入执行序列。
            if (!TryNormalizeFrame(template.FrameText, out string normalizedFrame, out string errorMessage))
            {
                SetExecutionStatus(errorMessage, true);
                return;
            }

            listBoxFrameSequence.Items.Add(normalizedFrame);
            listBoxFrameSequence.SelectedIndex = listBoxFrameSequence.Items.Count - 1;
            SetExecutionStatus($"已添加：{template.Name}。", false);
        }

        /// <summary>
        /// 清空手工帧输入区，不改变已经加入的序列。
        /// </summary>
        /// <param name="sender">触发事件的“清空 BUF”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonClearBuffer_Click(object sender, EventArgs e)
        {
            textBoxFrameBuffer.Clear();
        }

        /// <summary>
        /// 选中序列项时把带空格的帧文本载入 BUF，便于查看和修改。
        /// </summary>
        /// <param name="sender">发生选中项变化的帧序列列表。</param>
        /// <param name="e">选中项变化事件数据。</param>
        private void listBoxFrameSequence_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFrameSequence.SelectedItem != null)
            {
                textBoxFrameBuffer.Text = listBoxFrameSequence.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// 在用户点击实时上下行列表中的一帧时，将该帧的方向、时间、类型、长度和字段信息显示到左侧解析框。
        /// </summary>
        /// <param name="sender">发生选中项变化的实时上下行帧列表。</param>
        /// <param name="e">选中项变化事件数据。</param>
        private void listBoxLiveFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            TransparentFrameLogEntry entry = listBoxLiveFrames.SelectedItem as TransparentFrameLogEntry;
            if (entry != null)
            {
                // 实时帧选择入口只解析当前点击项，不改变人工待发送帧序列。
                ShowFrameAnalysis(entry);
            }
        }

        /// <summary>
        /// 用 BUF 中通过校验的帧替换当前序列项；空 BUF 不隐式删除序列项。
        /// </summary>
        /// <param name="sender">触发事件的“修改”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonSetFrame_Click(object sender, EventArgs e)
        {
            int index = listBoxFrameSequence.SelectedIndex;
            if (index < 0)
            {
                SetExecutionStatus("请先选择要修改的帧。", true);
                return;
            }

            // 修改入口使用统一十六进制校验，避免产生不可执行的序列项。
            if (!TryNormalizeFrame(textBoxFrameBuffer.Text, out string normalizedFrame, out string errorMessage))
            {
                SetExecutionStatus(errorMessage, true);
                return;
            }

            listBoxFrameSequence.Items[index] = normalizedFrame;
            SetExecutionStatus("帧修改成功。", false);
        }

        /// <summary>
        /// 将当前序列项向上移动一位并保持选中状态。
        /// </summary>
        /// <param name="sender">触发事件的“上移”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            // 上移入口交由通用移动函数处理边界和选中状态。
            MoveSelectedFrame(-1);
        }

        /// <summary>
        /// 将当前序列项向下移动一位并保持选中状态。
        /// </summary>
        /// <param name="sender">触发事件的“下移”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            // 下移入口交由通用移动函数处理边界和选中状态。
            MoveSelectedFrame(1);
        }

        /// <summary>
        /// 按指定偏移移动当前选中的序列项。
        /// </summary>
        /// <param name="offset">移动偏移，-1 表示上移，1 表示下移。</param>
        private void MoveSelectedFrame(int offset)
        {
            int sourceIndex = listBoxFrameSequence.SelectedIndex;
            int targetIndex = sourceIndex + offset;
            if (sourceIndex < 0 || targetIndex < 0 || targetIndex >= listBoxFrameSequence.Items.Count)
            {
                SetExecutionStatus("请选择可继续移动的帧。", true);
                return;
            }

            object frame = listBoxFrameSequence.Items[sourceIndex];
            listBoxFrameSequence.Items.RemoveAt(sourceIndex);
            listBoxFrameSequence.Items.Insert(targetIndex, frame);
            listBoxFrameSequence.SelectedIndex = targetIndex;
            SetExecutionStatus("帧顺序已调整。", false);
        }

        /// <summary>
        /// 删除当前选中的序列项。
        /// </summary>
        /// <param name="sender">触发事件的“删除”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonDeleteFrame_Click(object sender, EventArgs e)
        {
            int index = listBoxFrameSequence.SelectedIndex;
            if (index < 0)
            {
                SetExecutionStatus("请先选择要删除的帧。", true);
                return;
            }

            listBoxFrameSequence.Items.RemoveAt(index);
            textBoxFrameBuffer.Clear();
            SetExecutionStatus("帧已删除。", false);
        }

        /// <summary>
        /// 清空当前全部待执行帧和 BUF 编辑内容。
        /// </summary>
        /// <param name="sender">触发事件的“清空”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonClearSequence_Click(object sender, EventArgs e)
        {
            listBoxFrameSequence.Items.Clear();
            textBoxFrameBuffer.Clear();
            SetExecutionStatus("帧序列已清空。", false);
        }

        /// <summary>
        /// 从 UTF-8 文本文件读取此前保存的实时上下行帧记录，使其仍可点击并在左侧解析。
        /// </summary>
        /// <param name="sender">触发事件的“打开”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonOpenSequence_Click(object sender, EventArgs e)
        {
            if (openFileDialogSequence.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // 文件入口先完整校验所有实时记录，避免部分覆盖当前显示内容。
            string[] sourceLines = File.ReadAllLines(openFileDialogSequence.FileName, Encoding.UTF8);
            List<TransparentFrameLogEntry> entries = new List<TransparentFrameLogEntry>();
            foreach (string sourceLine in sourceLines.Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                // 每行按本控件的稳定存储格式恢复，确保打开后仍保留方向、时间和命令类型。
                if (!TransparentFrameLogEntry.TryParseStorageLine(sourceLine, out TransparentFrameLogEntry entry))
                {
                    SetExecutionStatus("打开失败：文件中包含无法识别的实时帧记录。", true);
                    return;
                }

                entries.Add(entry);
            }

            listBoxLiveFrames.Items.Clear();
            listBoxLiveFrames.Items.AddRange(entries.Cast<object>().ToArray());
            richTextBoxFrameAnalysis.Text = "点击右上方实时上下行帧进行解析。";
            SetExecutionStatus($"已打开 {entries.Count} 条实时帧记录。", false);
        }

        /// <summary>
        /// 将右上方实时上下行帧按可恢复方向、时间、类型和数据的格式保存为 UTF-8 文本文件。
        /// </summary>
        /// <param name="sender">触发事件的“保存”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonSaveSequence_Click(object sender, EventArgs e)
        {
            if (listBoxLiveFrames.Items.Count == 0)
            {
                SetExecutionStatus("当前没有可保存的实时帧。", true);
                return;
            }

            if (saveFileDialogSequence.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // 保存入口只序列化实时帧对象，人工待发送帧不随此按钮写入。
            string[] frames = listBoxLiveFrames.Items.Cast<TransparentFrameLogEntry>()
                .Select(entry => entry.ToStorageLine())
                .ToArray();
            File.WriteAllLines(saveFileDialogSequence.FileName, frames, new UTF8Encoding(false));
            SetExecutionStatus($"已保存 {frames.Length} 条实时帧记录。", false);
        }

        /// <summary>
        /// 按序执行全部透传帧，并将每帧的下行、上行和返回码实时追加到解析区。
        /// </summary>
        /// <param name="sender">触发事件的“开始测试”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private async void buttonRunTest_Click(object sender, EventArgs e)
        {
            if (_desktopCommService == null || !_desktopCommService.IsOpen)
            {
                SetExecutionStatus("请先在左侧打开通信端口。", true);
                return;
            }

            if (!_desktopCommService.IsInitialized)
            {
                SetExecutionStatus("请先在左侧初始化台发。", true);
                return;
            }

            if (listBoxFrameSequence.Items.Count == 0)
            {
                SetExecutionStatus("请先添加至少一条透传帧。", true);
                return;
            }

            string[] frameTexts = listBoxFrameSequence.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            bool originalLlcChange = _desktopCommService.CurrentLlcChange;
            if (originalLlcChange)
            {
                // 勾选 LLC 更新时先把列表转换成可见、可复核的逐帧交替值，避免只依赖 DLL 内部隐式修改。
                if (!TryApplyAutomaticLlcSequence(frameTexts, out string llcErrorMessage))
                {
                    SetExecutionStatus(llcErrorMessage, true);
                    return;
                }
                for (int index = 0; index < frameTexts.Length; index++)
                {
                    listBoxFrameSequence.Items[index] = frameTexts[index];
                }
            }
            int timeout = DefaultTimeoutMilliseconds;
            SetEditorEnabled(false);
            try
            {
                if (originalLlcChange)
                {
                    // LLC 已由透明测试按列表顺序写入，临时关闭 DLL 更新可避免实际发射前再次覆盖。
                    DesktopInitializationResult initialization = await Task.Run(() => _desktopCommService.ReinitializeLlcChange(false));
                    if (!initialization.IsSuccess)
                    {
                        SetExecutionStatus(
                            $"关闭 DLL LLC 自动更新失败：请求={initialization.RequestResult}，响应={(initialization.ResponseResult.HasValue ? initialization.ResponseResult.Value.ToString() : "未执行")}。",
                            true);
                        return;
                    }
                }

                for (int index = 0; index < frameTexts.Length; index++)
                {
                    // 每个序列项在进入 DLL 前再次解析，确保文件载入或编辑后的数据仍然有效。
                    if (!TryParseFrame(frameTexts[index], out byte[] commandFrame, out string errorMessage))
                    {
                        SetExecutionStatus($"第 {index + 1} 帧错误：{errorMessage}", true);
                        return;
                    }

                    SetExecutionStatus($"正在执行第 {index + 1}/{frameTexts.Length} 帧…", false);

                    // DLL 调用可能等待设备响应，在工作线程执行以保持 WinForms 界面可响应。
                    TransparentCommandResult result = await Task.Run(() => _desktopCommService.ExecuteTransparent(commandFrame, timeout));
                    // 显示服务层实际处理的请求，而非包含占位 MACID 的人工模板。
                    AppendFrameLog("↓", result.CommandName, result.RequestData);
                    if (result.ResponseData != null && result.ResponseData.Length > 0)
                    {
                        AppendFrameLog("↑", result.CommandName, result.ResponseData);
                    }

                    // 临时兼容：仅 Transfer 4 的 DLL 接收返回码 -100 放行，便于继续验证后续透明帧流程。
                    bool acceptTemporaryTransfer4Minus100 = IsTemporaryTransfer4Minus100Accepted(commandFrame, result);
                    if (acceptTemporaryTransfer4Minus100)
                    {
                        AppendFrameLog("!", "TransferChannel 临时兼容", Encoding.ASCII.GetBytes("DLL response -100 accepted"));
                    }

                    if (!result.IsSuccess && !acceptTemporaryTransfer4Minus100)
                    {
                        SetExecutionStatus($"第 {index + 1} 帧失败：{result.ErrorMessage}", true);
                        return;
                    }
                }

                SetExecutionStatus($"透传测试完成，共执行 {frameTexts.Length} 帧。", false);
            }
            catch (Exception exception)
            {
                SetExecutionStatus($"透传测试异常：{exception.Message}", true);
            }
            finally
            {
                if (originalLlcChange && _desktopCommService.IsOpen && !_desktopCommService.CurrentLlcChange)
                {
                    try
                    {
                        // 透明测试完成或异常退出后恢复用户原来的 LLC 自动更新配置。
                        await Task.Run(() => _desktopCommService.ReinitializeLlcChange(true));
                    }
                    catch (Exception exception)
                    {
                        SetExecutionStatus("恢复 LLC 自动更新配置失败：" + exception.Message, true);
                    }
                }
                SetEditorEnabled(true);
            }
        }

        /// <summary>
        /// 临时判断当前透明帧是否为固定 Transfer 4，并在 DLL 已成功发送但接收返回 -100 时允许序列继续。
        /// </summary>
        /// <param name="commandFrame">本轮从透明测试列表解析出的完整本地命令帧；允许 MACID 和 LLC 被会话逻辑动态替换。</param>
        /// <param name="result">DLL 请求/响应执行结果；仅请求返回 0 且响应返回 -100 时可能放行。</param>
        /// <returns>仅当帧类型和 Transfer 4 的 APDU 区完全一致、请求成功且响应为 -100 时返回 true。</returns>
        /// <remarks>TODO 临时实机兼容：确认 DLL 对多 APDU TransferChannel 的 -100 原因后删除本方法及调用点。</remarks>
        private static bool IsTemporaryTransfer4Minus100Accepted(byte[] commandFrame, TransparentCommandResult result)
        {
            if (commandFrame == null || result == null || result.RequestResult != 0 || result.ResponseResult != -100)
            {
                return false;
            }

            // 解析固定模板只用于限定临时兼容范围；前13字节含动态 MACID/LLC，故从 APDU 区开始逐字节核对。
            if (!TryParseFrame(Transfer4FrameText, out byte[] transfer4Frame, out _) || commandFrame.Length != transfer4Frame.Length)
            {
                return false;
            }

            const int apduOffset = 13;
            for (int index = apduOffset; index < commandFrame.Length; index++)
            {
                if (commandFrame[index] != transfer4Frame[index])
                {
                    return false;
                }
            }

            return commandFrame.Length > 1 && commandFrame[0] == 0x00 && commandFrame[1] == 0x03;
        }

        /// <summary>
        /// 按待发送顺序显式更新 DSRC 会话帧 LLC；BST 开始新会话，GetSecure、TransferChannel、SetMMI 依次交替。
        /// </summary>
        /// <param name="frameTexts">待执行帧文本数组；成功时在原数组中写回规范化结果。</param>
        /// <param name="errorMessage">失败时返回具体帧号和解析原因；成功时为空。</param>
        /// <returns>全部帧有效且 LLC 已完成更新时返回 true。</returns>
        private static bool TryApplyAutomaticLlcSequence(string[] frameTexts, out string errorMessage)
        {
            errorMessage = string.Empty;
            byte? previousLlc = null;
            for (int index = 0; index < frameTexts.Length; index++)
            {
                if (!TryParseFrame(frameTexts[index], out byte[] commandFrame, out string parseError))
                {
                    errorMessage = $"第 {index + 1} 帧错误：{parseError}";
                    return false;
                }

                byte commandType = commandFrame[1];
                if (commandType == 0x01)
                {
                    previousLlc = null;
                }
                else if (commandType >= 0x02 && commandType <= 0x04 && commandFrame.Length >= 8)
                {
                    if (previousLlc.HasValue)
                    {
                        commandFrame[7] = previousLlc.Value == 0xF7 ? (byte)0x77 : (byte)0xF7;
                    }
                    previousLlc = commandFrame[7];
                }

                frameTexts[index] = BitConverter.ToString(commandFrame).Replace("-", " ");
            }
            return true;
        }

        /// <summary>
        /// 清空上下行帧和解析信息显示区。
        /// </summary>
        /// <param name="sender">触发事件的“清空显示”按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            listBoxLiveFrames.Items.Clear();
            richTextBoxFrameAnalysis.Text = "点击右上方实时上下行帧进行解析。";
        }

        /// <summary>
        /// 将一条上下行帧按方向、命令名称、长度和十六进制数据追加到显示区。
        /// </summary>
        /// <param name="direction">方向标记；下行使用 ↓，上行使用 ↑。</param>
        /// <param name="commandName">根据本地类型字节识别出的命令名称。</param>
        /// <param name="data">要显示的数据字节，不含本地两字节类型标识。</param>
        private void AppendFrameLog(string direction, string commandName, byte[] data)
        {
            TransparentFrameLogEntry entry = new TransparentFrameLogEntry(DateTime.Now, direction, commandName, data);
            listBoxLiveFrames.Items.Add(entry);
            listBoxLiveFrames.TopIndex = listBoxLiveFrames.Items.Count - 1;
        }

        /// <summary>
        /// 将选中的实时帧解析为稳定的字段视图；不确定的协议字段只显示偏移和原始值，不作推测。
        /// </summary>
        /// <param name="entry">用户从实时上下行列表中选中的帧记录。</param>
        private void ShowFrameAnalysis(TransparentFrameLogEntry entry)
        {
            StringBuilder analysis = new StringBuilder();
            analysis.AppendLine($"时间：{entry.Timestamp:HH:mm:ss.fff}");
            analysis.AppendLine($"方向：{(entry.Direction == "↓" ? "下行（台发发送）" : "上行（台发接收）")}");
            analysis.AppendLine($"类型：{entry.CommandName}");
            analysis.AppendLine($"长度：{entry.Data.Length} 字节");
            // 按原版协议字段顺序解析选中的记录；截断字段保留明确提示。
            analysis.AppendLine(DecodeFields(entry.Data, entry.Direction == "↑", entry.CommandName));

            if (entry.Direction == "↓" && entry.CommandName == "BST / VST" && entry.Data.Length >= 12)
            {
                int beaconId = (entry.Data[9] << 16) | (entry.Data[10] << 8) | entry.Data[11];
                analysis.AppendLine($"路侧设备个体标识：{beaconId:X6}");
                if (entry.Data.Length >= 16)
                {
                    uint unixTime = ((uint)entry.Data[12] << 24)
                        | ((uint)entry.Data[13] << 16)
                        | ((uint)entry.Data[14] << 8)
                        | entry.Data[15];
                    analysis.AppendLine($"时间戳（秒）：{unixTime}");
                }
            }

            analysis.AppendLine();
            analysis.AppendLine("原始数据：");
            for (int offset = 0; offset < entry.Data.Length; offset += 16)
            {
                byte[] row = entry.Data.Skip(offset).Take(16).ToArray();
                analysis.AppendLine(BitConverter.ToString(row).Replace("-", " "));
            }

            richTextBoxFrameAnalysis.Text = analysis.ToString();
        }

        /// <summary>按原版 CODE_ENCODE 的字段顺序解释 DSRC 帧，遇到截断时停止，避免错误偏移扩散。</summary>
        /// <param name="data">不含本地类型前缀的完整数据；不能为 null。</param>
        /// <param name="up">true 表示 OBU 响应，false 表示请求。</param>
        /// <param name="kind">实时记录保存的命令名称。</param>
        /// <returns>按“中文描述：内容”排列的多行字段说明，不显示字节位置。</returns>
        private static string DecodeFields(byte[] data, bool up, string kind)
        {
            var output = new StringBuilder();
            int offset = 0;
            // 所有字段共用边界检查，防止短帧越界。
            Func<string, int, byte[]> field = (name, count) =>
            {
                if (count < 0 || offset + count > data.Length)
                    throw new FormatException("数据不完整，当前字段无法完整解析。");
                byte[] value = data.Skip(offset).Take(count).ToArray();
                // 英文协议标识后的中文说明作为界面标签，字段值保持原始字节顺序。
                int separator = name.IndexOf(" / ", StringComparison.Ordinal);
                string description = separator > 0 && name[0] < 128 ? name.Substring(separator + 3) : name;
                output.AppendLine(description + "：" + BitConverter.ToString(value).Replace("-", " "));
                offset += count;
                return value;
            };
            try
            {
                output.AppendLine("详细字段解析：");
                if (kind == "ICC / CPC")
                {
                    field("ICC/CPC 原始 APDU（无 DSRC MAC 头）", data.Length);
                    return output.ToString();
                }
                // MAC 和 LLC 为 DSRC 公共链路头。
                field("MACID / OBU 地址（FFFFFFFF 为广播）", 4);
                field("MAC_CONTROL / MAC 控制", 1);
                byte llc = field("LLC_CONTROL / LLC 控制", 1)[0];
                if (up && (kind != "BST / VST" || llc == 0xE0))
                    field("LLC_STATUS / 链路状态", 1);
                field("Head / 协议头", 1);
                byte sign = field("服务标识 / 可选项位图", 1)[0];
                if (kind == "BST / VST")
                {
                    if (!up)
                    {
                        field("BeaconID / 信标标识", 4);
                        field("UnixTime / 网络字节序时间", 4);
                        field("SupportConfig / 支持配置", 1);
                        field("MandApplications / 应用列表", 1);
                    }
                    else
                    {
                        field("SupportConfig / 支持配置", 1);
                        field("ApplicationList / 应用列表", 1);
                        byte privateInfoOption = field("ApplicationContextMark / 应用上下文可选项位图", 1)[0];
                        output.AppendLine($"  DID存在：{FormatPresence(privateInfoOption, 0x80)}；ApplicationParameter存在：{FormatPresence(privateInfoOption, 0x40)}；扩展位：0x{privateInfoOption & 0x3F:X2}");
                        if ((privateInfoOption & 0x80) != 0)
                            field("DID / DSRC应用标识", 1);
                        if ((privateInfoOption & 0x40) != 0)
                        {
                            byte applicationOption = field("ApplicationParameter / 应用参数可选项位图", 1)[0];
                            output.AppendLine($"  rndOBE：{FormatPresence(applicationOption, 0x80)}；privateInfo：{FormatPresence(applicationOption, 0x40)}；IC卡预读：{FormatPresence(applicationOption, 0x20)}；保留ESAM信息：{FormatPresence(applicationOption, 0x10)}；其余位：0x{applicationOption & 0x0F:X1}");

                            byte systemContainer = field("SysInfoFile / ESAM系统信息容器类型（应为20或27）", 1)[0];
                            int systemLength = ResolveVstSystemInformationLength(data, offset, applicationOption);
                            if (systemLength < 0)
                                throw new FormatException("无法依据VST可选项标志定位系统信息、IC预读区和尾部状态区。");

                            int systemEnd = offset + systemLength;
                            AppendAvailableVstField(field, "ContractProvider / 合同发行商标识", 8, ref offset, systemEnd);
                            AppendAvailableVstField(field, "ContractType / 合同类型", 1, ref offset, systemEnd);
                            AppendAvailableVstField(field, "ContractVersion / 合同版本", 1, ref offset, systemEnd);
                            AppendAvailableVstField(field, "ContractSerialNumber / 合同序列号", 8, ref offset, systemEnd);
                            AppendAvailableVstField(field, "ContractSignedDate / 合同签署日期（BCD YYYYMMDD）", 4, ref offset, systemEnd);
                            AppendAvailableVstField(field, "ContractExpiredDate / 合同到期日期（BCD YYYYMMDD）", 4, ref offset, systemEnd);
                            AppendAvailableVstField(field, "RemoveFlag / 拆卸标志", 1, ref offset, systemEnd);
                            if (offset < systemEnd)
                                field("DefaultData / ESAM系统信息保留数据", systemEnd - offset);
                            output.AppendLine($"  系统信息实际解析长度：{systemLength} 字节；容器类型：0x{systemContainer:X2}");

                            if ((applicationOption & 0x80) != 0)
                            {
                                byte randomContainer = field("rndOBE容器类型（应为1D）", 1)[0];
                                field("rndOBE / OBU随机数", 8);
                                if (randomContainer != 0x1D)
                                    output.AppendLine($"  警告：rndOBE容器类型为0x{randomContainer:X2}，与原上位机期望的1D不一致。");
                            }
                            if ((applicationOption & 0x40) != 0)
                                output.AppendLine("  提示：privateInfo置位，但旧上位机也未定义其长度，后续仅保留原始数据以避免错位。");
                            else
                            {
                                int reservedLength = (applicationOption & 0x10) != 0 ? 16 : 0;
                                int tailLength = 7;
                                if ((applicationOption & 0x20) != 0)
                                {
                                    byte iccContainer = field("gbICCInfo / IC卡预读容器类型（应为28）", 1)[0];
                                    int iccLength = data.Length - offset - reservedLength - tailLength;
                                    if (iccLength < 0)
                                        throw new FormatException("IC卡预读区长度与VST尾部长度不匹配。");
                                    field(iccLength == 43 ? "ICC0015 / IC卡0015预读数据" : "ICCInfo / IC卡预读数据（具体文件分段由BST配置决定）", iccLength);
                                    output.AppendLine($"  IC卡预读数据长度：{iccLength} 字节；容器类型：0x{iccContainer:X2}");
                                }
                                if (reservedLength > 0)
                                {
                                    field("ReservedInfo1容器类型", 1);
                                    field("ReservedInfo1 / ESAM保留信息", 15);
                                }
                                byte[] obuMac = field("ObuConfiguration MacID / OBU配置MAC", 4);
                                byte equipmentInfo = field("EquipmentInfo / 设备类别与硬件版本", 1)[0];
                                byte status1 = field("ObuStatus[0] / OBU状态", 1)[0];
                                field("ObuStatus[1] / OBU保留状态", 1);
                                output.AppendLine($"  MAC一致性：{(data.Take(4).SequenceEqual(obuMac) ? "一致" : "不一致")}");
                                output.AppendLine($"  EquipmentClass：0x{equipmentInfo >> 4:X1}；EquipmentVersion：0x{equipmentInfo & 0x0F:X1}");
                                output.AppendLine($"  IC卡存在：{FormatPresence(status1, 0x80)}；IC卡类型：{(status1 >> 4) & 0x07}；IC卡状态：{FormatBit(status1, 0x08)}；锁定：{FormatBit(status1, 0x04)}；防拆：{FormatBit(status1, 0x02)}；电池：{FormatBit(status1, 0x01)}");
                            }
                        }
                    }
                }
                else if (kind == "GetSecure" || kind == "TransferChannel" || kind == "SetMMI" || kind == "EventReport")
                {
                    field("DID / 应用标识", 1);
                    if (!up) field(kind == "EventReport" ? "EventType / 事件类型" : "ActionType / 操作类型", 1);
                    if (kind == "GetSecure")
                    {
                        if (!up && (sign & 8) != 0) field("AccessCredentials / 访问凭证", 8);
                        field(up ? "GetSecureRs / 响应容器 15" : "GetSecureRq / 请求容器 14", 1);
                        if (!up) field("KeyIdForEncryptOp / 加密密钥选项", 1);
                        field("FID / 文件标识", 1);
                        if (!up)
                        {
                            field("Offset / 文件读取偏移（大端）", 2);
                            field("Length / 请求读取长度", 1);
                            field("RndRSU / RSU 随机数", 8);
                            field("KeyIDforAuthen / 认证密钥标识", 1);
                            field("KeyIDforEncrypt / 加密密钥标识", 1);
                        }
                        else
                        {
                            field("File / 文件响应数据（含其编码）", data.Length - offset - 9);
                            field("Authenticator / 认证码", 8);
                            field("ReturnStatus / 返回状态", 1);
                        }
                    }
                    else if (kind == "TransferChannel")
                    {
                        field(up ? "ChannelRs / 通道响应" : "ChannelRq / 通道请求", 1);
                        field("ChannelID / 通道标识", 1);
                        int count = field(up ? "DATALIST / 响应数量" : "APDULIST / 指令数量", 1)[0];
                        for (int i = 0; i < count; i++)
                        {
                            int length = field($"第 {i + 1} 项长度", 1)[0];
                            byte[] apdu = field($"第 {i + 1} 项 APDU", length);
                            if (up && length >= 2)
                                output.AppendLine($"响应状态字：{apdu[length - 2]:X2}{apdu[length - 1]:X2}");
                            if (!up && length >= 4)
                                output.AppendLine($"指令类别：{apdu[0]:X2}\r\n指令代码：{apdu[1]:X2}\r\n指令参数一：{apdu[2]:X2}\r\n指令参数二：{apdu[3]:X2}");
                        }
                        if (up) field("ReturnStatus / 返回状态", 1);
                    }
                    else if (kind == "SetMMI")
                    {
                        field("SetMMI / 容器标识", 1);
                        field(up ? "ReturnStatus / 返回状态" : "Parameter / 0成功、1失败、2保留", 1);
                    }
                }
                if (offset < data.Length)
                    field("其余应用/可选字段（尚未细分，保留原始数据）", data.Length - offset);
            }
            catch (FormatException error)
            {
                output.AppendLine("解析提示：" + error.Message);
            }
            return output.ToString();
        }

        /// <summary>根据旧版VST解码顺序和可选容器标识推导本帧系统信息实际长度。</summary>
        /// <param name="data">完整VST响应数据。</param>
        /// <param name="systemOffset">系统信息正文起始位置。</param>
        /// <param name="applicationOption">ApplicationParameter可选项位图。</param>
        /// <returns>可可靠定位时返回系统信息长度，否则返回-1。</returns>
        private static int ResolveVstSystemInformationLength(byte[] data, int systemOffset, byte applicationOption)
        {
            int suffixBeforeTail = ((applicationOption & 0x80) != 0 ? 9 : 0) + ((applicationOption & 0x20) != 0 ? 1 : 0);
            int minimumLength = suffixBeforeTail + ((applicationOption & 0x10) != 0 ? 16 : 0) + 7;
            if ((applicationOption & 0x20) == 0 && (applicationOption & 0x40) == 0)
            {
                int exactLength = data.Length - systemOffset - minimumLength;
                if (exactLength < 0) return -1;
                if ((applicationOption & 0x80) != 0 && data[systemOffset + exactLength] != 0x1D) return -1;
                return exactLength;
            }
            int[] preferredLengths = { 26, 27, 32, 39 };
            foreach (int length in preferredLengths.Concat(Enumerable.Range(0, 73)))
            {
                int cursor = systemOffset + length;
                if (cursor + minimumLength > data.Length) continue;
                if ((applicationOption & 0x80) != 0 && data[cursor] != 0x1D) continue;
                if ((applicationOption & 0x80) != 0) cursor += 9;
                if ((applicationOption & 0x20) != 0 && data[cursor] != 0x28) continue;
                return length;
            }
            return -1;
        }

        /// <summary>在系统信息声明长度内追加一个字段；字段不足时追加剩余字节并停止该段细分。</summary>
        private static void AppendAvailableVstField(Func<string, int, byte[]> field, string name, int length, ref int offset, int end)
        {
            int available = end - offset;
            if (available <= 0) return;
            field(name, Math.Min(length, available));
        }

        /// <summary>将协议可选位显示为明确的“存在/不存在”。</summary>
        private static string FormatPresence(byte value, byte mask) => (value & mask) != 0 ? "存在" : "不存在";

        /// <summary>将单比特状态显示为0或1，避免对厂商状态值作额外推断。</summary>
        private static int FormatBit(byte value, byte mask) => (value & mask) != 0 ? 1 : 0;

        /// <summary>
        /// 启用或禁用会改变帧序列的控件，避免测试执行期间发生并发编辑或重复发送。
        /// </summary>
        /// <param name="enabled">为 <see langword="true"/> 时允许编辑和启动测试。</param>
        private void SetEditorEnabled(bool enabled)
        {
            groupBoxFrameSequence.Enabled = enabled;
            groupBoxFrameEditor.Enabled = enabled;
        }

        /// <summary>
        /// 更新透传页底部状态文字和错误/正常颜色。
        /// </summary>
        /// <param name="message">面向用户显示的状态说明。</param>
        /// <param name="isError">为 <see langword="true"/> 时使用错误颜色。</param>
        private void SetExecutionStatus(string message, bool isError)
        {
            labelExecutionStatus.Text = message;
            labelExecutionStatus.ForeColor = isError
                ? Color.FromArgb(181, 54, 54)
                : Color.FromArgb(44, 111, 76);
        }

        /// <summary>
        /// 把允许包含空格、换行和连字符的十六进制文本规范化为大写、逐字节空格分隔格式。
        /// </summary>
        /// <param name="source">待规范化的帧文本。</param>
        /// <param name="normalizedFrame">成功时返回规范化帧文本，失败时返回空字符串。</param>
        /// <param name="errorMessage">失败时返回可显示的校验原因，成功时返回空字符串。</param>
        /// <returns>文本能解析为包含两字节类型标识和至少一个数据字节的帧时返回 <see langword="true"/>。</returns>
        private static bool TryNormalizeFrame(string source, out string normalizedFrame, out string errorMessage)
        {
            // 规范化入口复用字节解析结果，保证显示格式和真实发送字节一致。
            if (!TryParseFrame(source, out byte[] bytes, out errorMessage))
            {
                normalizedFrame = string.Empty;
                return false;
            }

            normalizedFrame = BitConverter.ToString(bytes).Replace("-", " ");
            return true;
        }

        /// <summary>
        /// 将十六进制帧文本转换为字节数组，并校验长度、字符和本地命令类型范围。
        /// </summary>
        /// <param name="source">可包含空白和连字符的十六进制字符串。</param>
        /// <param name="bytes">成功时返回完整帧，其中前两字节为本地类型标识；失败时返回空数组。</param>
        /// <param name="errorMessage">失败时返回具体原因，成功时返回空字符串。</param>
        /// <returns>帧满足 3～1024 字节且类型为 0001～0007 时返回 <see langword="true"/>。</returns>
        private static bool TryParseFrame(string source, out byte[] bytes, out string errorMessage)
        {
            string compact = new string((source ?? string.Empty).Where(character => !char.IsWhiteSpace(character) && character != '-').ToArray());
            if (compact.Length < 6)
            {
                bytes = new byte[0];
                errorMessage = "帧至少需要 2 字节类型标识和 1 字节数据。";
                return false;
            }

            if ((compact.Length & 1) != 0)
            {
                bytes = new byte[0];
                errorMessage = "十六进制字符数量必须为偶数。";
                return false;
            }

            if (compact.Length / 2 > MaximumFrameBytes)
            {
                bytes = new byte[0];
                errorMessage = $"帧长度不能超过 {MaximumFrameBytes} 字节。";
                return false;
            }

            bytes = new byte[compact.Length / 2];
            for (int index = 0; index < bytes.Length; index++)
            {
                if (!byte.TryParse(compact.Substring(index * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[index]))
                {
                    bytes = new byte[0];
                    errorMessage = "帧中包含非十六进制字符。";
                    return false;
                }
            }

            if (bytes[0] != 0x00 || bytes[1] < 0x01 || bytes[1] > 0x07)
            {
                bytes = new byte[0];
                errorMessage = "前两字节必须是 0001～0007 的本地命令类型。";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// 描述一个可在下拉框中显示并追加到序列的预置透传帧。
        /// </summary>
        private sealed class TransparentFrameTemplate
        {
            /// <summary>
            /// 创建预置帧描述。
            /// </summary>
            /// <param name="name">下拉框显示名称。</param>
            /// <param name="frameText">完整十六进制帧；提示项允许为空。</param>
            internal TransparentFrameTemplate(string name, string frameText)
            {
                Name = name;
                FrameText = frameText;
            }

            internal string Name { get; }
            internal string FrameText { get; }

            /// <summary>
            /// 返回模板名称，供 WinForms 下拉框显示。
            /// </summary>
            /// <returns>当前模板的中文名称。</returns>
            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// 保存一次真实 DLL 请求或响应产生的实时帧，供右上列表显示并在左侧按点击项解析。
        /// </summary>
        private sealed class TransparentFrameLogEntry
        {
            /// <summary>
            /// 创建一条不可变的实时上下行帧记录，并复制数据以隔离 DLL 缓冲区后续变化。
            /// </summary>
            /// <param name="timestamp">帧进入显示层的本地时间。</param>
            /// <param name="direction">方向标记，↓ 表示下行，↑ 表示上行。</param>
            /// <param name="commandName">本地类型标识对应的命令名称。</param>
            /// <param name="data">不含本地两字节类型标识的真实收发数据。</param>
            internal TransparentFrameLogEntry(DateTime timestamp, string direction, string commandName, byte[] data)
            {
                Timestamp = timestamp;
                Direction = direction;
                CommandName = commandName;
                Data = data == null ? new byte[0] : data.ToArray();
            }

            internal DateTime Timestamp { get; }
            internal string Direction { get; }
            internal string CommandName { get; }
            internal byte[] Data { get; }

            /// <summary>
            /// 生成右上实时帧列表的一行摘要，不替代左侧详细解析。
            /// </summary>
            /// <returns>包含时间、方向、类型、长度和十六进制数据的单行文本。</returns>
            public override string ToString()
            {
                return $"[{Timestamp:HH:mm:ss.fff}] {Direction} {CommandName}  Len={Data.Length}  {BitConverter.ToString(Data).Replace("-", " ")}";
            }

            /// <summary>
            /// 将实时帧转换为可由“打开”按钮完整恢复的单行 UTF-8 存储格式。
            /// </summary>
            /// <returns>依次包含往返时间、方向、命令名称和连续十六进制数据的竖线分隔文本。</returns>
            internal string ToStorageLine()
            {
                return $"{Timestamp:O}|{Direction}|{CommandName}|{BitConverter.ToString(Data).Replace("-", string.Empty)}";
            }

            /// <summary>
            /// 从实时帧单行存储格式恢复可点击解析的帧对象。
            /// </summary>
            /// <param name="source">由 <see cref="ToStorageLine"/> 生成的竖线分隔文本。</param>
            /// <param name="entry">成功时返回恢复后的实时帧；失败时返回空。</param>
            /// <returns>时间、方向、名称和十六进制数据均有效时返回 <see langword="true"/>。</returns>
            internal static bool TryParseStorageLine(string source, out TransparentFrameLogEntry entry)
            {
                entry = null;
                string[] parts = (source ?? string.Empty).Split('|');
                if (parts.Length != 4
                    || !DateTime.TryParseExact(parts[0], "O", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime timestamp)
                    || (parts[1] != "↓" && parts[1] != "↑")
                    || string.IsNullOrWhiteSpace(parts[2])
                    || (parts[3].Length & 1) != 0)
                {
                    return false;
                }

                byte[] data = new byte[parts[3].Length / 2];
                for (int index = 0; index < data.Length; index++)
                {
                    if (!byte.TryParse(parts[3].Substring(index * 2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out data[index]))
                    {
                        return false;
                    }
                }

                entry = new TransparentFrameLogEntry(timestamp, parts[1], parts[2], data);
                return true;
            }
        }
    }
}
