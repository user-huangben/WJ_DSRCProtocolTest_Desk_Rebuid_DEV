using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using WJ_DSRCProtocolTest_Desk_Net.Services;

namespace WJ_DSRCProtocolTest_Desk_Net
{
    public partial class MainForm : Form
    {
        private readonly DesktopCommService _desktopCommService = new DesktopCommService();
        private string[] _knownPortNames = new string[0];

        /// <summary>
        /// 初始化主窗体，并把同一台发通信服务交给透传测试页复用。
        /// </summary>
        public MainForm()
        {
            // 先创建 Designer 中的全部静态控件，确保透传控件实例已经可用。
            InitializeComponent();
            // 透传页必须复用左侧端口建立的 DLL 句柄，不能创建第二套设备连接。
            transparentTestControl.AttachService(_desktopCommService);
        }

        /// <summary>绘制三个一级产品页签的场景颜色和选中状态。</summary>
        /// <param name="sender">Designer 绑定的一级产品页签控件。</param>
        /// <param name="e">包含待绘制页签边界和选中状态的事件数据。</param>
        private void tabControlProducts_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color[] normalColors =
            {
                Color.FromArgb(220, 230, 242),
                Color.FromArgb(220, 238, 221),
                Color.FromArgb(247, 228, 204)
            };
            Color[] selectedColors =
            {
                Color.FromArgb(47, 111, 178),
                Color.FromArgb(67, 140, 91),
                Color.FromArgb(181, 107, 44)
            };
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color background = isSelected ? selectedColors[e.Index] : normalColors[e.Index];
            Rectangle bounds = e.Bounds;
            using (SolidBrush backgroundBrush = new SolidBrush(background))
            using (SolidBrush textBrush = new SolidBrush(isSelected ? Color.White : Color.FromArgb(55, 65, 81)))
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.FillRectangle(backgroundBrush, bounds);
                e.Graphics.DrawString(tabControlProducts.TabPages[e.Index].Text, tabControlProducts.Font, textBrush, bounds, format);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshAvailablePorts(true);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerPortMonitor.Stop();

            if (!_desktopCommService.IsOpen)
            {
                return;
            }

            try
            {
                _desktopCommService.Close();
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                // The form must still be allowed to close if the native library is unavailable.
            }
        }

        private void buttonOpenClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (_desktopCommService.IsOpen)
                {
                    CloseCommunicationPort();
                }
                else
                {
                    OpenCommunicationPort();
                }
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                ShowNativeLibraryError(exception);
            }
        }

        private void buttonInitialize_Click(object sender, EventArgs e)
        {
            if (!_desktopCommService.IsOpen)
            {
                SetStatus("请先打开通信端口。", true);
                return;
            }

            if (!TryReadInitializationOptions(out DesktopInitializationOptions options))
            {
                return;
            }

            try
            {
                int logResult = _desktopCommService.SetLogEnabled(checkBoxEnableLog.Checked);
                string logWarning = DesktopCommService.IsLogSettingSuccessful(logResult)
                    ? null
                    : $"DLL 日志设置返回码：{logResult}";

                UseWaitCursor = true;
                buttonInitialize.Enabled = false;
                DesktopInitializationResult result = _desktopCommService.Initialize(options);
                if (result.RequestResult != 0)
                {
                    SetStatus($"发送台发初始化请求失败，返回码：{result.RequestResult}。", true);
                }
                else if (result.ResponseResult != 0)
                {
                    SetStatus($"接收台发初始化响应失败，返回码：{result.ResponseResult}。", true);
                }
                else
                {
                    string message = $"台发初始化成功，设备状态：{result.DesktopStatus}。";
                    if (logWarning != null)
                    {
                        message += $" {logWarning}。";
                    }

                    SetStatus(message, logWarning != null);
                }
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                ShowNativeLibraryError(exception);
            }
            finally
            {
                buttonInitialize.Enabled = _desktopCommService.IsOpen;
                UseWaitCursor = false;
            }
        }

        private void checkBoxEnableLog_CheckedChanged(object sender, EventArgs e)
        {
            if (!_desktopCommService.IsOpen)
            {
                return;
            }

            try
            {
                int result = _desktopCommService.SetLogEnabled(checkBoxEnableLog.Checked);
                SetStatus(
                    DesktopCommService.IsLogSettingSuccessful(result)
                        ? $"DLL 日志已{(checkBoxEnableLog.Checked ? "启用" : "关闭")}。"
                        : $"DLL 日志设置失败，返回码：{result}。",
                    !DesktopCommService.IsLogSettingSuccessful(result));
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                ShowNativeLibraryError(exception);
            }
        }

        private void OpenCommunicationPort()
        {
            string portName = comboBoxPort.Text.Trim();
            if (string.IsNullOrWhiteSpace(portName))
            {
                SetStatus("请选择通信端口。", true);
                return;
            }

            int result = _desktopCommService.Open(portName);
            if (result <= 0)
            {
                SetStatus($"打开 {portName} 失败，返回码：{result}。", true);
                return;
            }

            comboBoxPort.Enabled = false;
            buttonOpenClose.Text = "关闭端口";
            buttonInitialize.Enabled = true;
            SetStatus($"{portName} 已打开。", false);
        }

        private void CloseCommunicationPort()
        {
            int result = _desktopCommService.Close();
            if (result != 0)
            {
                SetStatus($"关闭通信端口失败，返回码：{result}。", true);
                return;
            }

            comboBoxPort.Enabled = true;
            buttonOpenClose.Text = "打开端口";
            buttonOpenClose.Enabled = comboBoxPort.Items.Count > 0;
            buttonInitialize.Enabled = false;
            SetStatus("通信端口已关闭。", false);
        }

        private void timerPortMonitor_Tick(object sender, EventArgs e)
        {
            RefreshAvailablePorts(false);
        }

        private void RefreshAvailablePorts(bool initialRefresh)
        {
            try
            {
                string[] currentPortNames = SerialPort.GetPortNames();
                Array.Sort(currentPortNames, StringComparer.OrdinalIgnoreCase);
                if (!initialRefresh && _knownPortNames.SequenceEqual(currentPortNames, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }

                string selectedPort = comboBoxPort.SelectedItem as string;
                string[] addedPorts = currentPortNames.Except(_knownPortNames, StringComparer.OrdinalIgnoreCase).ToArray();
                string[] removedPorts = _knownPortNames.Except(currentPortNames, StringComparer.OrdinalIgnoreCase).ToArray();

                comboBoxPort.BeginUpdate();
                comboBoxPort.Items.Clear();
                comboBoxPort.Items.AddRange(currentPortNames);
                if (!string.IsNullOrEmpty(selectedPort) && currentPortNames.Contains(selectedPort, StringComparer.OrdinalIgnoreCase))
                {
                    comboBoxPort.SelectedItem = selectedPort;
                }
                else if (comboBoxPort.Items.Count > 0)
                {
                    comboBoxPort.SelectedIndex = 0;
                }
                comboBoxPort.EndUpdate();

                _knownPortNames = currentPortNames;
                buttonOpenClose.Enabled = _desktopCommService.IsOpen || currentPortNames.Length > 0;

                if (initialRefresh)
                {
                    labelPortMonitorStatus.Text = currentPortNames.Length == 0
                        ? "未识别到串口，正在实时监测"
                        : $"已识别串口：{string.Join("、", currentPortNames)}";
                    return;
                }

                string changeMessage = BuildPortChangeMessage(addedPorts, removedPorts);
                labelPortMonitorStatus.Text = changeMessage;
                toolStripStatusLabel.Text = $"状态：{changeMessage}";
            }
            catch (Exception exception)
            {
                labelPortMonitorStatus.Text = $"串口监测异常：{exception.Message}";
            }
        }

        private static string BuildPortChangeMessage(string[] addedPorts, string[] removedPorts)
        {
            string added = addedPorts.Length > 0 ? $"新增 {string.Join("、", addedPorts)}" : null;
            string removed = removedPorts.Length > 0 ? $"移除 {string.Join("、", removedPorts)}" : null;
            if (added != null && removed != null)
            {
                return $"串口变动：{added}；{removed}";
            }

            return $"串口变动：{added ?? removed ?? "无"}";
        }

        private bool TryReadInitializationOptions(out DesktopInitializationOptions options)
        {
            options = null;
            if (!TryReadNonNegativeInt(textBoxBstInterval, "BST 间隔", out int bstInterval)
                || !TryReadNonNegativeInt(textBoxRetryInterval, "非 BST 帧间隔", out int retryInterval)
                || !TryReadNonNegativeInt(textBoxRetryTimes, "帧发送次数", out int retryTimes)
                || !TryReadNonNegativeInt(textBoxTimeout, "交易超时", out int timeout)
                || !TryReadNonNegativeInt(textBoxTxPower, "功率等级", out int txPower)
                || !TryReadNonNegativeInt(textBoxPhysicalChannel, "物理信道", out int physicalChannelId))
            {
                return false;
            }

            options = new DesktopInitializationOptions
            {
                BstInterval = bstInterval,
                RetryInterval = retryInterval,
                RetryTimes = retryTimes,
                Timeout = timeout,
                TxPower = txPower,
                PhysicalChannelId = physicalChannelId,
                BidChange = checkBoxBidChange.Checked,
                UnixTimeChange = checkBoxUnixTimeChange.Checked,
                LlcChange = checkBoxLlcChange.Checked,
                MacChange = checkBoxMacChange.Checked
            };
            return true;
        }

        private bool TryReadNonNegativeInt(TextBox textBox, string fieldName, out int value)
        {
            if (int.TryParse(textBox.Text.Trim(), out value) && value >= 0)
            {
                return true;
            }

            SetStatus($"{fieldName}必须是非负整数。", true);
            textBox.Focus();
            textBox.SelectAll();
            return false;
        }

        private void SetStatus(string message, bool isError)
        {
            labelDeviceStatus.Text = message;
            labelDeviceStatus.ForeColor = isError
                ? System.Drawing.Color.FromArgb(181, 54, 54)
                : System.Drawing.Color.FromArgb(44, 111, 76);
            toolStripStatusLabel.Text = $"状态：{message}";
        }

        private void ShowNativeLibraryError(Exception exception)
        {
            string message;
            if (exception is DllNotFoundException)
            {
                message = $"未找到 DeskTopComm.dll。请确认文件位于程序目录：{AppDomain.CurrentDomain.BaseDirectory}";
            }
            else if (exception is BadImageFormatException)
            {
                message = "DeskTopComm.dll 位数不匹配。当前程序和 DLL 均应为 x86。";
            }
            else if (exception is EntryPointNotFoundException)
            {
                message = "DeskTopComm.dll 缺少所需导出函数，请核对 DLL 版本。";
            }
            else
            {
                message = exception.Message;
            }

            SetStatus(message, true);
        }

        private static bool IsNativeInteropException(Exception exception)
        {
            return exception is DllNotFoundException
                || exception is BadImageFormatException
                || exception is EntryPointNotFoundException
                || exception is Win32Exception;
        }

        /// <summary>
        /// 打开万集 OBU 标准测试独立大框，并复用主窗体持有的台发服务。
        /// </summary>
        /// <param name="sender">万集 OBU 标准测试按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonWanjiObuStandard_Click(object sender, EventArgs e)
        {
            // OBU 入口必须打开包含已迁移用例树和版本号读取逻辑的独立测试窗体。
            using (WanjiStandardTestForm testForm = new WanjiStandardTestForm("万集 OBU 标准测试", _desktopCommService))
            {
                testForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 打开北京地标协议测试独立大框，并复用主窗体持有的台发服务。
        /// </summary>
        /// <param name="sender">北京地标协议测试按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonBeijingLocalStandard_Click(object sender, EventArgs e)
        {
            // 北京地标入口保持独立界面状态，但协议执行必须复用当前已打开和初始化的同一台发句柄。
            using (BeijingLocalStandardTestForm testForm = new BeijingLocalStandardTestForm(_desktopCommService))
            {
                testForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 打开万集 CPC 卡标准测试独立大框；具体测试项由后续需求接入。
        /// </summary>
        /// <param name="sender">万集 CPC 卡标准测试按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonWanjiCpcStandard_Click(object sender, EventArgs e)
        {
            // CPC 卡入口使用自己的窗体类型，保证后续卡片测试状态与其他大项隔离。
            using (WanjiCpcStandardTestForm testForm = new WanjiCpcStandardTestForm())
            {
                testForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 打开守望者播报测试独立大框；具体测试项由后续需求接入。
        /// </summary>
        /// <param name="sender">守望者播报测试按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonWatchmanBroadcast_Click(object sender, EventArgs e)
        {
            // 守望者入口使用自己的窗体类型，避免与其他标准测试大项共用控件实例。
            using (WatchmanBroadcastTestForm testForm = new WatchmanBroadcastTestForm())
            {
                testForm.ShowDialog(this);
            }
        }
    }
}
