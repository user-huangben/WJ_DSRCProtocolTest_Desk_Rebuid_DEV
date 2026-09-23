using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WJ_ComprehensiveTest_Desk.Services;

namespace WJ_ComprehensiveTest_Desk
{
    /// <summary>
    /// 承载北京地标协议测试目录、选择状态、顺序调度和后续协议用例注册入口。
    /// </summary>
    public partial class BeijingLocalStandardTestForm : Form
    {
        private readonly DesktopCommService _desktopCommService;
        private readonly Dictionary<string, Func<CancellationToken, Task>> _testCaseHandlers;
        private readonly BeijingRespondBroadcastService _respondBroadcastService = new BeijingRespondBroadcastService();
        private readonly BeijingRespondDifferentMacService _respondDifferentMacService = new BeijingRespondDifferentMacService();
        private readonly BeijingTypicalTransactionService _typicalTransactionService = new BeijingTypicalTransactionService();
        private CancellationTokenSource _testCancellation;
        private bool _updatingChecks;

        /// <summary>
        /// 初始化仅供 WinForms 设计器和兼容调用方使用的北京地标测试窗口。
        /// </summary>
        public BeijingLocalStandardTestForm() : this(null)
        {
        }

        /// <summary>初始化北京地标测试窗口并复用主窗体已持有的台发服务。</summary>
        /// <param name="desktopCommService">主窗体共享通信服务；真实用例注册后必须通过该实例访问设备。</param>
        internal BeijingLocalStandardTestForm(DesktopCommService desktopCommService)
        {
            _desktopCommService = desktopCommService;
            InitializeComponent();
            _testCaseHandlers = CreateTestCaseHandlers();
            PopulateTestTree();
            UpdateSelectionSummary();
        }

        /// <summary>按照原 ONECHIP_DIALOG 当前活动节点建立四类北京地标测试目录。</summary>
        private void PopulateTestTree()
        {
            treeViewTestItems.BeginUpdate();
            treeViewTestItems.Nodes.Clear();
            AddCatalogGroup("OBU基本测试", BasicTestCatalog);
            AddCatalogGroup("OBU入网测试", NetworkTestCatalog);
            AddCatalogGroup("OBU实验室检测", LaboratoryTestCatalog);
            AddCatalogGroup("单片式OBU测试（原版目录未接执行线程）", OnePieceTestCatalog, true);
            treeViewTestItems.EndUpdate();
            treeViewTestItems.Nodes[0].Expand();
            treeViewTestItems.SelectedNode = treeViewTestItems.Nodes[0];
        }

        /// <summary>建立测试路径到已迁移协议处理器的映射。</summary>
        /// <returns>已完成逐帧迁移的北京地标用例异步处理器映射。</returns>
        private Dictionary<string, Func<CancellationToken, Task>> CreateTestCaseHandlers()
        {
            return new Dictionary<string, Func<CancellationToken, Task>>(StringComparer.Ordinal)
            {
                { "OBU基本测试 > 1、交易过程中响应广播帧测试 > 测试用例1：获取车辆信息后发送BST", token => RunRespondBroadcastAsync(false, token) },
                { "OBU基本测试 > 1、交易过程中响应广播帧测试 > 测试用例2：消费初始化后发送BST", token => RunRespondBroadcastAsync(true, token) },
                { "OBU基本测试 > 2、交易过程中响应不同MAC测试 > 测试用例1：获取车辆信息后发送其他标签MAC的TransferChannel", token => RunRespondDifferentMacAsync(1, token) },
                { "OBU基本测试 > 2、交易过程中响应不同MAC测试 > 测试用例2：读取车辆信息之后发送全FF的TransferChannel", token => RunRespondDifferentMacAsync(2, token) },
                { "OBU基本测试 > 2、交易过程中响应不同MAC测试 > 测试用例3：读取车辆信息之后发送其他MAC的TransferChannel", token => RunRespondDifferentMacAsync(3, token) },
                { "OBU基本测试 > 3、典型交易测试", RunTypicalTransactionAsync }
            };
        }

        /// <summary>执行一个旧版错误 MAC 变体并保存完整交互结果。</summary>
        private async Task RunRespondDifferentMacAsync(int macVariant, CancellationToken cancellationToken)
        {
            DateTime startedAt = DateTime.Now;
            BeijingRespondDifferentMacResult result = await Task.Run(() => _respondDifferentMacService.Execute(_desktopCommService, macVariant, cancellationToken), cancellationToken);
            AppendInformation(result.Message + "\r\n");
            SaveTestArtifactsIfSelected("北京地标-交易过程中响应不同MAC测试-变体" + macVariant, result.IsSuccess, result.Message, result.Exchanges, startedAt);
        }

        /// <summary>执行旧 WJ_OneChip_RespondBroadcast 的完整两轮流程并保存结果帧。</summary>
        /// <param name="useSm4">消费初始化使用 SM4 参数 41；当前北京地标配置默认使用 3DES 参数 01。</param>
        /// <param name="cancellationToken">统一测试取消令牌。</param>
        private async Task RunRespondBroadcastAsync(bool afterPurchaseInitialization, CancellationToken cancellationToken)
        {
            DateTime startedAt = DateTime.Now;
            BeijingRespondBroadcastResult result = await Task.Run(() => _respondBroadcastService.Execute(_desktopCommService, afterPurchaseInitialization, false, cancellationToken), cancellationToken);
            AppendInformation(result.Message + "\r\n");
            SaveTestArtifactsIfSelected("北京地标-交易过程中响应广播帧测试", result.IsSuccess, result.Message, result.Exchanges, startedAt);
        }

        /// <summary>执行旧 <c>WJ_OneChip_typicaltransfer</c> 的固定六分支流程。</summary>
        /// <param name="cancellationToken">北京测试队列共用的停止令牌。</param>
        private async Task RunTypicalTransactionAsync(CancellationToken cancellationToken)
        {
            DateTime startedAt = DateTime.Now;
            bool validateVehicleInformation = checkBoxValidateVehicleInformation.Checked;
            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
            BeijingTypicalTransactionResult result = await Task.Run(
                () => _typicalTransactionService.Execute(
                    _desktopCommService,
                    configurationPath,
                    validateVehicleInformation,
                    cancellationToken,
                    AppendInformationFromWorker,
                    UpdateTypicalTransactionProgress),
                cancellationToken);
            AppendInformation(result.Message + "\r\n");
            SaveTestArtifactsIfSelected(
                "北京地标-OBU基本测试-典型交易测试",
                result.IsSuccess,
                result.Message,
                result.Exchanges,
                startedAt);
        }

        /// <summary>从后台协议流程安全追加测试信息。</summary>
        /// <param name="text">单条步骤信息。</param>
        private void AppendInformationFromWorker(string text)
        {
            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendInformationFromWorker), text);
                return;
            }

            AppendInformation(text);
        }

        /// <summary>显示典型交易六分支完成进度，不改变外层叶子队列进度条。</summary>
        /// <param name="completed">已成功分支数。</param><param name="total">固定分支总数。</param>
        private void UpdateTypicalTransactionProgress(int completed, int total)
        {
            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(new Action<int, int>(UpdateTypicalTransactionProgress), completed, total);
                return;
            }

            labelExecutionStatus.Text = "北京典型交易分支进度：" + completed + "/" + total;
        }

        /// <summary>按照“存储测试结果”选项保存当前用例的结果文件和截图。</summary>
        private void SaveTestArtifactsIfSelected(string testCaseName, bool isSuccess, string message, IList<ObuProtocolExchange> exchanges, DateTime startedAt)
        {
            AppendInformation("最终测试结果：" + (isSuccess ? "测试成功" : "测试失败") + "\r\n");
            if (!checkBoxStoreTestResult.Checked) return;
            try
            {
                DateTime completedAt = DateTime.Now;
                string path = TestResultFileWriter.WriteTestResult(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, completedAt);
                AppendInformation("测试结果文件：" + path + "\r\n");
                string screenshotPath = TestResultScreenshotWriter.CaptureTestForm(testCaseName, this, isSuccess, completedAt);
                AppendInformation("测试截图文件：" + screenshotPath + "\r\n");
            }
            catch (IOException exception)
            {
                AppendInformation("测试结果文件或截图写入失败：" + exception.Message + "\r\n");
            }
            catch (UnauthorizedAccessException exception)
            {
                AppendInformation("测试结果文件或截图写入失败：" + exception.Message + "\r\n");
            }
        }

        /// <summary>执行已勾选的全部叶子，未勾选时执行当前节点下的叶子。</summary>
        private async void buttonRun_Click(object sender, EventArgs e)
        {
            List<TreeNode> targets = GetLeafNodes(treeViewTestItems.Nodes.Cast<TreeNode>()).Where(node => node.Checked).ToList();
            if (targets.Count == 0 && treeViewTestItems.SelectedNode != null)
                targets = GetLeafNodes(new[] { treeViewTestItems.SelectedNode }).ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show(this, "请选择测试用例。", "北京地标协议测试", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _testCancellation = new CancellationTokenSource();
            SetRunning(true);
            progressBarExecution.Maximum = targets.Count;
            progressBarExecution.Value = 0;
            try
            {
                foreach (TreeNode node in targets)
                {
                    _testCancellation.Token.ThrowIfCancellationRequested();
                    string path = GetNodePath(node);
                    labelExecutionStatus.Text = "正在检查：" + path;
                    AppendInformation("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + path);
                    if (IsLegacyUnimplemented(node))
                    {
                        AppendInformation("原上位机未实现或未接执行线程，本次不调用设备。\r\n");
                    }
                    else if (!_testCaseHandlers.TryGetValue(path, out Func<CancellationToken, Task> handler))
                    {
                        AppendInformation("目录与原入口已登记，协议流程待后续逐项迁移，本次不调用设备。\r\n");
                    }
                    else
                    {
                        if (_desktopCommService == null || !_desktopCommService.IsOpen || !_desktopCommService.IsInitialized)
                            throw new InvalidOperationException("执行真实用例前必须打开串口并完成台发初始化。");
                        await handler(_testCancellation.Token);
                    }
                    progressBarExecution.Value++;
                }
                labelExecutionStatus.Text = "队列检查完成，共 " + targets.Count + " 个叶子用例。";
            }
            catch (OperationCanceledException)
            {
                labelExecutionStatus.Text = "测试已停止。";
                AppendInformation("测试已由用户停止。\r\n");
            }
            catch (Exception exception)
            {
                labelExecutionStatus.Text = "执行失败。";
                AppendInformation("执行失败：" + exception.Message + "\r\n");
            }
            finally
            {
                SetRunning(false);
                _testCancellation.Dispose();
                _testCancellation = null;
            }
        }

        /// <summary>取消当前北京测试队列，后续真实处理器共用该取消令牌。</summary>
        private void buttonStop_Click(object sender, EventArgs e)
        {
            _testCancellation?.Cancel();
            buttonStop.Enabled = false;
        }

        /// <summary>清空右侧测试信息，不改变测试项选择状态。</summary>
        private void buttonClear_Click(object sender, EventArgs e) => richTextBoxInformation.Clear();

        /// <summary>
        /// 在测试信息变化后将光标定位到末尾，确保最新的测试状态始终可见。
        /// </summary>
        /// <param name="sender">Designer 绑定的测试信息 RichTextBox。</param>
        /// <param name="e">文本变化事件数据；当前实现不使用其附加信息。</param>
        private void richTextBoxInformation_TextChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.SelectionStart = richTextBoxInformation.TextLength;
            richTextBoxInformation.SelectionLength = 0;
            // 测试信息变化入口把显示位置滚动到末尾，覆盖追加和整段替换两种显示方式。
            richTextBoxInformation.ScrollToCaret();
        }

        /// <summary>在全选与全部取消之间切换全部叶子用例。</summary>
        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            List<TreeNode> leaves = GetLeafNodes(treeViewTestItems.Nodes.Cast<TreeNode>()).ToList();
            bool check = leaves.Any(node => !node.Checked);
            _updatingChecks = true;
            foreach (TreeNode node in leaves) node.Checked = check;
            foreach (TreeNode root in treeViewTestItems.Nodes) root.Checked = check;
            _updatingChecks = false;
            UpdateSelectionSummary();
        }

        /// <summary>把父节点勾选状态递归应用到子节点，并回算祖先节点状态。</summary>
        private void treeViewTestItems_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_updatingChecks) return;
            _updatingChecks = true;
            SetChildrenChecked(e.Node, e.Node.Checked);
            UpdateParentChecked(e.Node.Parent);
            _updatingChecks = false;
            UpdateSelectionSummary();
        }

        /// <summary>显示所选测试项的原版实现状态和新上位机迁移状态。</summary>
        private void treeViewTestItems_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            string path = GetNodePath(e.Node);
            string state = IsLegacyUnimplemented(e.Node) ? "原上位机未实现或未接执行线程" : (_testCaseHandlers.ContainsKey(path) ? "已接入协议处理器" : "已完成目录审计，协议处理器待迁移");
            richTextBoxInformation.Text = "测试项：" + path + "\r\n状态：" + state + "\r\n\r\n逐项参数来源、流程和判断依据见《北京地标协议测试用例迁移对照.md》。";
        }

        /// <summary>窗体关闭时取消尚未完成的测试队列。</summary>
        private void BeijingLocalStandardTestForm_FormClosing(object sender, FormClosingEventArgs e) => _testCancellation?.Cancel();

        /// <summary>把一段审计目录文本转换为树中的一级类别、测试组和叶子节点。</summary>
        private void AddCatalogGroup(string groupName, string catalog, bool legacyUnimplemented = false)
        {
            TreeNode root = treeViewTestItems.Nodes.Add(groupName);
            if (legacyUnimplemented) root.Tag = "LegacyUnimplemented";
            TreeNode parent = null;
            foreach (string rawLine in catalog.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("- ", StringComparison.Ordinal))
                {
                    string leafText = line.Substring(2);
                    bool rootLeaf = leafText.Length > 0 && char.IsDigit(leafText[0]);
                    TreeNode leaf = (rootLeaf ? root : (parent ?? root)).Nodes.Add(leafText);
                    leaf.Tag = IsUnimplementedText(rootLeaf || parent == null ? leafText : parent.Text) ? "LegacyUnimplemented" : "PendingMigration";
                    if (rootLeaf) parent = null;
                }
                else
                {
                    parent = root.Nodes.Add(line);
                    if (IsUnimplementedText(line)) parent.Tag = "LegacyUnimplemented";
                }
            }
        }

        /// <summary>判断目录文字是否明确表示原程序没有可执行实现。</summary>
        private static bool IsUnimplementedText(string text) => text.Contains("原版未实现") || text.Contains("未接执行线程");

        /// <summary>沿节点祖先链判断当前叶子是否属于原版未实现范围。</summary>
        private static bool IsLegacyUnimplemented(TreeNode node)
        {
            for (TreeNode current = node; current != null; current = current.Parent)
                if (string.Equals(current.Tag as string, "LegacyUnimplemented", StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>按界面顺序递归枚举指定节点集合下的全部叶子。</summary>
        private static IEnumerable<TreeNode> GetLeafNodes(IEnumerable<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
                if (node.Nodes.Count == 0) yield return node;
                else foreach (TreeNode child in GetLeafNodes(node.Nodes.Cast<TreeNode>())) yield return child;
        }

        /// <summary>递归设置一个父节点下全部子节点的勾选状态。</summary>
        private static void SetChildrenChecked(TreeNode node, bool value)
        {
            foreach (TreeNode child in node.Nodes) { child.Checked = value; SetChildrenChecked(child, value); }
        }

        /// <summary>根据直接子节点状态向上更新全部祖先节点。</summary>
        private static void UpdateParentChecked(TreeNode node)
        {
            if (node == null) return;
            node.Checked = node.Nodes.Count > 0 && node.Nodes.Cast<TreeNode>().All(child => child.Checked);
            UpdateParentChecked(node.Parent);
        }

        /// <summary>生成用于显示和处理器注册的完整测试树路径。</summary>
        private static string GetNodePath(TreeNode node)
        {
            Stack<string> parts = new Stack<string>();
            for (TreeNode current = node; current != null; current = current.Parent) parts.Push(current.Text);
            return string.Join(" > ", parts);
        }

        /// <summary>统一切换执行期间按钮与测试树的启用状态。</summary>
        private void SetRunning(bool running)
        {
            buttonRun.Enabled = !running;
            buttonSelectAll.Enabled = !running;
            buttonStop.Enabled = running;
            treeViewTestItems.Enabled = !running;
            checkBoxValidateVehicleInformation.Enabled = !running;
        }

        /// <summary>统计已勾选叶子并刷新窗口底部状态。</summary>
        private void UpdateSelectionSummary()
        {
            int selected = GetLeafNodes(treeViewTestItems.Nodes.Cast<TreeNode>()).Count(node => node.Checked);
            labelExecutionStatus.Text = "已选择 " + selected + " 个叶子用例；当前为目录审计与调度框架阶段。";
        }

        /// <summary>追加测试信息并自动滚动到显示末尾。</summary>
        private void AppendInformation(string text)
        {
            richTextBoxInformation.AppendText(text + (text.EndsWith("\r\n", StringComparison.Ordinal) ? string.Empty : "\r\n"));
            richTextBoxInformation.SelectionStart = richTextBoxInformation.TextLength;
            richTextBoxInformation.ScrollToCaret();
        }

        private const string BasicTestCatalog = @"1、交易过程中响应广播帧测试
- 测试用例1：获取车辆信息后发送BST
- 测试用例2：消费初始化后发送BST
2、交易过程中响应不同MAC测试
- 测试用例1：获取车辆信息后发送其他标签MAC的TransferChannel
- 测试用例2：读取车辆信息之后发送全FF的TransferChannel
- 测试用例3：读取车辆信息之后发送其他MAC的TransferChannel
- 3、典型交易测试
- 4、速通255
- 5、BSTVST测试
- 6、路径标识流程
7、保留文件读写测试
- 测试用例1：读写ICC0009文件
8、OBU信道选择测试
- 测试用例1：0信道读取文件
- 测试用例2：1信道读取文件
- 9、BST测试
- 10、天线预读后重读19交易测试
11、预读信息测试（原版未实现）
- 测试用例1：预读0019文件、0002文件
- 测试用例2：预读0015文件
- 测试用例3：预读0012文件
- 12、取消省界收费站交易流程
- 13、单帧响应时间测试（原版未实现）
- 14、ESAM测试（原版未实现）
15、双通道切换测试
- 测试用例1：ESAM读取EF01，ICC读取0016
- 测试用例2：ICC读取0016，ESAM读取EF04
- 测试用例3：ESAM读取EF04，ICC读取0015
- 测试用例4：ICC读取0015，ESAM读取EF01
16、指令集测试
- 测试用例1：取4字节随机数
- 测试用例2：取8字节随机数
- 测试用例3：取16字节随机数
- 测试用例4：取9字节随机数
- 测试用例5：发送随机数P1参数不正确
- 测试用例6：发送随机数CLA参数不正确
- 测试用例7：发送随机数INS参数不正确
- 测试用例8：取4字节芯片序列号
- 测试用例9：发送取芯片序列号指令P1参数不正确
- 测试用例10：发送取芯片序列号指令长度不正确
- 测试用例11：取芯片序列号INS参数不正确
- 测试用例12：取芯片序列号INS参数不正确（原目录重复名称）
- 测试用例13：选择EF04文件
- 测试用例14：选择EF04文件参数不正确
- 测试用例15：选择EF04文件Lc参数不正确
- 测试用例16：选择EF04文件CLA不正确
- 测试用例17：读取0015文件测试
- 测试用例18：读取001A文件
- 测试用例19：取响应代码返回6982
- 测试用例20：响应代码6981不支持安全报文
- 测试用例21：响应代码6A83未找到记录
- 测试用例22：响应代码6984引用数据无效
- 测试用例23：响应代码6984未申请随机数
- 测试用例24：响应代码6986不满足命令执行
- 测试用例25：响应代码6988安全报文数据项不正确
- 测试用例26：响应代码6985不满足引用条件
- 测试用例27：响应代码6B00参数不正确，偏移地址超出EF
- 测试用例28：响应代码6F00判断不准确
17、SetMMI蜂鸣器测试
- 测试用例1：参数00
- 测试用例2：参数01
- 测试用例3：参数02
- 测试用例4：参数03
- 测试用例5：参数04
18、VST随机避让测试
- 测试用例1：无间隔发送BST
- 19、交易成功后4S冻结机制测试（非语音款）";

        private const string NetworkTestCatalog = @"1、预读信息测试
- 测试用例1：预读0019文件、0002文件
- 测试用例2：预读0015文件
- 测试用例3：预读0012文件
- 2、防碰撞测试
- 3、连续交易时间间隔测试（原版未实现）
- 4、ESAM芯片序列号测试
- 5、交易时间测试（原版未实现）
6、保留文件读写测试
- 测试用例1：读写ICC卡0009文件
7、休眠掉电机制测试（北京地标）
- 测试用例1：发行模式1S内未收到下行帧是否休眠
- 测试用例2：交易模式3S内未收到下行帧是否休眠
- 测试用例3：发行超时上限20S
- 8、255机制测试（原版未实现）";

        private const string LaboratoryTestCatalog = @"- 1、BST和VST（原版未实现）
2、GetSecure
- 测试用例1（原目录未命名）
- 测试用例2（原目录未命名）
- 测试用例3（原目录未命名）
- 测试用例4（原目录未命名）
3、TransferChannel
- 测试用例1：1条APDU指令读余额
- 测试用例2：2条相同APDU指令读余额
- 测试用例3：3条相同APDU指令读余额
- 测试用例4：4条相同APDU指令读余额
- 测试用例5：5条相同APDU指令读余额
- 测试用例6：6条相同APDU指令读余额
- 测试用例7：7条相同APDU指令读余额
- 测试用例8：1条APDU操作ESAM取4字节随机数
- 测试用例9：2条APDU操作ESAM取4字节随机数
- 测试用例10：3条APDU操作ESAM取4字节随机数
- 测试用例11：IC通道读余额、0015和0012
- 测试用例12：ESAM通道进3F00、读EF01和取芯片序列号
- 4、SetMMI（原版未实现）
- 5、封闭式入口、不带拼帧、复合消费交易（原版未实现）
- 6、封闭式入口、带拼帧、复合消费交易（原版未实现）
- 7、255计时测试（原版未实现）";

        private const string OnePieceTestCatalog = @"- 1、典型交易测试
- 2、BST测试
- 3、GetSecure测试
- 4、预读测试
- 5、ETC门架典型交易流程
- 6、ESAM测试
- 7、OBU发行信息测试
- 8、选OBE-ICC测试
- 9、OBU通道测试
- 10、指令集测试";
    }
}
