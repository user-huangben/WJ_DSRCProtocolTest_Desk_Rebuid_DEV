using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WJ_ComprehensiveTest_Desk.Services;

namespace WJ_ComprehensiveTest_Desk
{
    /// <summary>
    /// 承载单个万集标准测试类别的测试项界面，并提供测试逻辑查看和结果文件记录入口。
    /// </summary>
    public partial class WanjiStandardTestForm : Form
    {
        private const string ObuVersionTestCaseName = "0、OBU版本号读取";
        private const int VersionReadTimeoutMilliseconds = 500;
        private readonly DesktopCommService _desktopCommService;
        private readonly LaneTransactionService _laneTransactionService;
        private readonly OneChipIssueActivationService _oneChipIssueActivationService;
        private readonly Dictionary<string, Action> _testCaseHandlers;
        private CancellationTokenSource _currentTestCancellation;
        private Queue<TreeNode> _pendingTestCaseNodes;
        private bool _stopTestCaseSequenceRequested;
        private bool _isUpdatingTreeChecks;
        private bool _isBatchScreenshotMode;
        private bool _batchAllTestsSucceeded;
        private string _batchScreenshotTestName;

        /// <summary>
        /// 创建指定类别的大型测试项窗口，并将类别名称显示在窗口标题和页内标题中。
        /// </summary>
        /// <param name="categoryName">入口按钮对应的完整测试类别名称；不能为空或空白。</param>
        /// <exception cref="ArgumentException">类别名称为空或仅包含空白字符时抛出。</exception>
        public WanjiStandardTestForm(string categoryName)
            : this(categoryName, null)
        {
        }

        /// <summary>
        /// 创建指定类别的大型测试项窗口，并复用主窗体持有的台发通信服务。
        /// </summary>
        /// <param name="categoryName">入口按钮对应的完整测试类别名称；不能为空或空白。</param>
        /// <param name="desktopCommService">主窗体共享的台发服务；万集 OBU 标准测试执行版本号读取时必须提供。</param>
        /// <exception cref="ArgumentException">类别名称为空或仅包含空白字符时抛出。</exception>
        internal WanjiStandardTestForm(string categoryName, DesktopCommService desktopCommService)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentException("测试类别名称不能为空。", nameof(categoryName));
            }

            _desktopCommService = desktopCommService;
            // 车道交易软算法服务独立于界面创建，测试入口不打开读卡器或硬件 PSAM。
            SoftTradeCryptoService softTradeCryptoService = new SoftTradeCryptoService();
            _laneTransactionService = new LaneTransactionService(softTradeCryptoService);
            _oneChipIssueActivationService = new OneChipIssueActivationService(softTradeCryptoService);
            // 构造入口先建立 Designer 管理的控件，再填充当前选择的类别标题。
            InitializeComponent();
            Text = categoryName;
            labelCategoryTitle.Text = categoryName;
            // 构造入口完成控件初始化后建立“测试用例路径→独立函数”的分发表。
            _testCaseHandlers = CreateTestCaseHandlers();
        }

        /// <summary>
        /// 根据勾选的测试项或测试分组，把“测试”按钮请求按树顺序分发到对应的独立实现函数。
        /// </summary>
        /// <param name="sender">Designer 绑定的“测试”按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonSingleTest_Click(object sender, EventArgs e)
        {
            if (!TryGetTestTargetNodes(out IList<TreeNode> targetNodes))
            {
                return;
            }

            // 单项入口把叶子用例或父级展开后的叶子队列交给统一顺序调度器。
            _pendingTestCaseNodes = new Queue<TreeNode>(targetNodes);
            _stopTestCaseSequenceRequested = false;
            // 多叶子入口记录共同父级，子用例只保存 TXT，队列结束后再生成一张汇总截图。
            BeginBatchScreenshot(targetNodes);
            StartNextQueuedTestCase();
        }

        /// <summary>
        /// 清空当前测试大框右侧的测试信息显示，不修改测试树和配置选项。
        /// </summary>
        /// <param name="sender">Designer 绑定的“清空显示”按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonClearDisplay_Click(object sender, EventArgs e)
        {
            // 清空入口只作用于测试信息 RichTextBox，避免误删人工勾选状态或结果文件。
            richTextBoxTestInformation.Clear();
        }

        /// <summary>
        /// 在测试信息新增或原位更新后，把显示区域自动滚动到最后一行，确保当前进度始终可见。
        /// </summary>
        /// <param name="sender">Designer 绑定的测试信息 RichTextBox。</param>
        /// <param name="e">文本变化事件数据；当前实现不使用其附加信息。</param>
        private void richTextBoxTestInformation_TextChanged(object sender, EventArgs e)
        {
            richTextBoxTestInformation.SelectionStart = richTextBoxTestInformation.TextLength;
            richTextBoxTestInformation.SelectionLength = 0;
            // 文本变化入口把插入点定位到末尾后滚动，覆盖追加文本和进度原位替换两种更新方式。
            richTextBoxTestInformation.ScrollToCaret();
        }

        /// <summary>
        /// 响应“停止测试”按钮，通知当前 BST 兼容性后台流程立即停止后续通信和等待。
        /// </summary>
        /// <param name="sender">Designer 绑定的“停止测试”按钮。</param>
        /// <param name="e">按钮单击事件数据；当前实现不使用其附加信息。</param>
        private void buttonStopTest_Click(object sender, EventArgs e)
        {
            CancellationTokenSource cancellation = _currentTestCancellation;
            if (cancellation == null || cancellation.IsCancellationRequested)
            {
                richTextBoxTestInformation.AppendText("当前没有可停止的测试。\r\n");
                return;
            }

            // 大框统一停止入口发出取消信号，使当前用例停止等待并阻止下一次 DLL 调用。
            _stopTestCaseSequenceRequested = true;
            cancellation.Cancel();
            buttonStopTest.Enabled = false;
            richTextBoxTestInformation.AppendText("已请求停止测试，正在结束当前流程……\r\n");
        }

        /// <summary>
        /// 为当前测试大框启动一次统一的可取消测试流程，并同步更新操作按钮状态。
        /// </summary>
        /// <returns>当前用例必须传递给后台流程和协议服务的取消令牌。</returns>
        private CancellationToken BeginTestExecution()
        {
            if (_currentTestCancellation != null)
            {
                _currentTestCancellation.Dispose();
            }

            _currentTestCancellation = new CancellationTokenSource();
            buttonSingleTest.Enabled = false;
            buttonAllTests.Enabled = false;
            buttonStopTest.Enabled = true;
            UseWaitCursor = true;
            return _currentTestCancellation.Token;
        }

        /// <summary>
        /// 完成当前测试大框内的测试流程，释放统一取消资源并恢复操作按钮状态。
        /// </summary>
        private void CompleteTestExecution()
        {
            buttonSingleTest.Enabled = true;
            buttonAllTests.Enabled = true;
            buttonStopTest.Enabled = false;
            UseWaitCursor = false;
            if (_currentTestCancellation != null)
            {
                _currentTestCancellation.Dispose();
                _currentTestCancellation = null;
            }

            if (_pendingTestCaseNodes != null && _pendingTestCaseNodes.Count > 0 && !_stopTestCaseSequenceRequested)
            {
                // 当前用例完成后继续调度父级测试项剩余叶子，保持树中从上到下的顺序。
                StartNextQueuedTestCase();
                return;
            }

            // 队列正常结束或被停止后，以全部已执行子用例的汇总状态保存唯一最终截图。
            CompleteBatchScreenshot();
            _pendingTestCaseNodes = null;
            _stopTestCaseSequenceRequested = false;
        }

        /// <summary>
        /// 为包含多个叶子用例的父级测试启动汇总截图状态；单叶子测试保持逐用例截图。
        /// </summary>
        /// <param name="targetNodes">本次即将按顺序执行的全部叶子节点。</param>
        private void BeginBatchScreenshot(IList<TreeNode> targetNodes)
        {
            _isBatchScreenshotMode = targetNodes != null && targetNodes.Count > 1;
            _batchAllTestsSucceeded = true;
            _batchScreenshotTestName = null;
            if (!_isBatchScreenshotMode)
            {
                return;
            }

            TreeNode commonParent = FindCommonParent(targetNodes);
            _batchScreenshotTestName = commonParent == null ? "批量测试" : commonParent.Text;
        }

        /// <summary>查找一组叶子节点共同的最深父级，用作批量截图的用例名称。</summary>
        /// <param name="nodes">至少包含两个叶子节点的集合。</param>
        /// <returns>所有叶子的共同父节点；没有共同父节点时返回 null。</returns>
        private static TreeNode FindCommonParent(IList<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return null;
            List<TreeNode> firstPath = new List<TreeNode>();
            for (TreeNode node = nodes[0].Parent; node != null; node = node.Parent) firstPath.Add(node);
            foreach (TreeNode candidate in firstPath)
            {
                bool containsAll = true;
                for (int index = 1; index < nodes.Count && containsAll; index++)
                {
                    TreeNode current = nodes[index].Parent;
                    while (current != null && current != candidate) current = current.Parent;
                    containsAll = current == candidate;
                }
                if (containsAll) return candidate;
            }
            return null;
        }

        /// <summary>在父级批量执行结束时保存唯一汇总截图，并清理批量状态。</summary>
        private void CompleteBatchScreenshot()
        {
            if (!_isBatchScreenshotMode)
            {
                return;
            }

            try
            {
                if (checkBoxStoreTestResult.Checked)
                {
                    bool isSuccess = _batchAllTestsSucceeded && !_stopTestCaseSequenceRequested;
                    // 批量完成入口此时已显示所有子用例结果，截图代表父级测试的最终状态。
                    string screenshotPath = TestResultScreenshotWriter.CaptureTestForm(
                        string.IsNullOrWhiteSpace(_batchScreenshotTestName) ? "批量测试" : _batchScreenshotTestName,
                        this,
                        isSuccess,
                        DateTime.Now);
                    richTextBoxTestInformation.AppendText("父级测试最终截图：" + screenshotPath + "\r\n");
                }
            }
            catch (IOException exception)
            {
                richTextBoxTestInformation.AppendText("父级测试最终截图写入失败：" + exception.Message + "\r\n");
            }
            catch (UnauthorizedAccessException exception)
            {
                richTextBoxTestInformation.AppendText("父级测试最终截图写入失败：" + exception.Message + "\r\n");
            }
            finally
            {
                _isBatchScreenshotMode = false;
                _batchAllTestsSucceeded = true;
                _batchScreenshotTestName = null;
            }
        }

        /// <summary>
        /// 解析测试按钮对应的测试目标：多个已勾选叶子按树的显示顺序执行；没有勾选项时使用当前选中节点。
        /// </summary>
        /// <param name="targetNodes">输出将按顺序执行的叶子测试节点集合。</param>
        /// <returns>存在至少一个已勾选叶子，或存在可展开的有效选中节点时返回 true；否则返回 false。</returns>
        private bool TryGetTestTargetNodes(out IList<TreeNode> targetNodes)
        {
            targetNodes = new List<TreeNode>();
            foreach (TreeNode rootNode in treeViewTestItems.Nodes)
            {
                // 从根节点开始只收集已勾选叶子，避免父级与其子项被重复加入执行队列。
                CollectCheckedLeafNodes(rootNode, targetNodes);
            }

            if (targetNodes.Count > 0)
            {
                return true;
            }

            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            if (selectedNode == null)
            {
                richTextBoxTestInformation.AppendText("请先勾选或选中测试用例。\r\n");
                return false;
            }

            if (selectedNode.Nodes.Count == 0)
            {
                targetNodes.Add(selectedNode);
                return true;
            }

            // 父级测试项按当前树顺序递归展开，预读信息等多用例分组由此形成执行队列。
            CollectLeafTestNodes(selectedNode, targetNodes);
            if (targetNodes.Count == 0)
            {
                richTextBoxTestInformation.AppendText("当前测试分组没有可执行的叶子测试用例。\r\n");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 按 TreeView 显示顺序递归收集所有已勾选的叶子测试用例。
        /// </summary>
        /// <param name="node">当前递归检查的树节点。</param>
        /// <param name="checkedLeafNodes">用于接收已勾选叶子测试用例的列表；父级节点不会加入列表。</param>
        private static void CollectCheckedLeafNodes(TreeNode node, IList<TreeNode> checkedLeafNodes)
        {
            if (node.Nodes.Count == 0)
            {
                if (node.Checked)
                {
                    checkedLeafNodes.Add(node);
                }

                return;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                // 调用入口继续遍历子节点，维持界面中从上到下的执行顺序。
                CollectCheckedLeafNodes(childNode, checkedLeafNodes);
            }
        }

        /// <summary>
        /// 响应测试树复选状态变化；父级勾选或取消勾选时，将相同状态同步到其全部后代节点。
        /// </summary>
        /// <param name="sender">Designer 绑定的测试项 TreeView。</param>
        /// <param name="e">包含本次状态发生变化的节点及其当前勾选状态。</param>
        private void treeViewTestItems_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_isUpdatingTreeChecks || e.Node == null || e.Node.Nodes.Count == 0)
            {
                return;
            }

            try
            {
                _isUpdatingTreeChecks = true;
                // 父级复选入口把状态递归应用到全部子级，使整组测试可以一次选择或取消。
                SetDescendantCheckedState(e.Node, e.Node.Checked);
            }
            finally
            {
                _isUpdatingTreeChecks = false;
            }
        }

        /// <summary>
        /// 将指定父节点的勾选状态递归同步到全部子孙节点。
        /// </summary>
        /// <param name="parentNode">需要向下同步状态的父节点；调用前必须确认节点非空。</param>
        /// <param name="isChecked">要应用到所有后代节点的勾选状态；true 表示勾选，false 表示取消。</param>
        private static void SetDescendantCheckedState(TreeNode parentNode, bool isChecked)
        {
            foreach (TreeNode childNode in parentNode.Nodes)
            {
                childNode.Checked = isChecked;
                if (childNode.Nodes.Count > 0)
                {
                    // 调用入口继续向更深层级同步，保证多层测试分组中的叶子状态一致。
                    SetDescendantCheckedState(childNode, isChecked);
                }
            }
        }

        /// <summary>
        /// 按 TreeView 当前顺序递归收集指定测试分组下的所有叶子用例。
        /// </summary>
        /// <param name="node">待展开的测试分组节点。</param>
        /// <param name="leafNodes">用于接收叶子测试节点的列表。</param>
        private static void CollectLeafTestNodes(TreeNode node, IList<TreeNode> leafNodes)
        {
            if (node.Nodes.Count == 0)
            {
                leafNodes.Add(node);
                return;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                // 调用入口沿节点显示顺序展开，保证预读测试用例按 1、2、3 顺序执行。
                CollectLeafTestNodes(childNode, leafNodes);
            }
        }

        /// <summary>
        /// 执行队列中的下一个叶子测试用例；异步用例完成后由 CompleteTestExecution 继续调用。
        /// </summary>
        private void StartNextQueuedTestCase()
        {
            if (_pendingTestCaseNodes == null || _pendingTestCaseNodes.Count == 0 || _stopTestCaseSequenceRequested)
            {
                CompleteTestExecution();
                return;
            }

            TreeNode targetNode = _pendingTestCaseNodes.Dequeue();
            treeViewTestItems.SelectedNode = targetNode;
            string testCaseKey = BuildTestCaseKey(targetNode);
            if (!_testCaseHandlers.TryGetValue(testCaseKey, out Action testCaseHandler))
            {
                _batchAllTestsSucceeded = false;
                richTextBoxTestInformation.AppendText("未找到当前测试用例的独立实现入口：" + testCaseKey + "\r\n");
                // 缺少实现入口时跳过当前叶子，继续执行同一父级下的后续用例。
                StartNextQueuedTestCase();
                return;
            }

            try
            {
                // 调用入口把当前叶子节点交给其独立函数；异步流程会在完成时回到统一队列。
                testCaseHandler();
                if (_currentTestCancellation == null)
                {
                    // 占位或无设备流程同步返回，立即调度下一个叶子。
                    StartNextQueuedTestCase();
                }
            }
            catch (Exception exception)
            {
                _batchAllTestsSucceeded = false;
                richTextBoxTestInformation.AppendText("测试用例执行入口异常：" + exception.Message + "\r\n");
                // 当前入口异常不阻塞同一分组的后续用例执行。
                StartNextQueuedTestCase();
            }
        }

        /// <summary>
        /// 读取 OBU 版本号并保存完整 5.8G 请求/响应帧和测试结果。
        /// </summary>
        private async void RunObuVersionReadTest()
        {
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? ObuVersionTestCaseName : selectedNode.Text;
            if (_desktopCommService == null)
            {
                string unavailableMessage = "未连接主窗体台发服务，无法执行 OBU 版本号读取。";
                DateTime unavailableAt = DateTime.Now;
                richTextBoxTestInformation.AppendText(unavailableMessage + "\r\n");
                // 服务未连接时仍记录失败结论，便于复核当前测试大框状态。
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, unavailableAt, DateTime.Now);
                return;
            }

            DateTime startedAt = DateTime.Now;
            ObuVersionReadResult result = null;
            string failureMessage = string.Empty;
            // 版本号读取接入测试大框统一取消入口，停止按钮在后台协议流程运行时保持可用。
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                richTextBoxTestInformation.AppendText("开始读取 OBU 版本号……\r\n");
                // 将 DLL 通信放到后台，并把大框统一取消令牌传入每个版本读取协议步骤。
                result = await System.Threading.Tasks.Task.Run(
                    () => _desktopCommService.ReadObuVersion(VersionReadTimeoutMilliseconds, cancellationToken));
                richTextBoxTestInformation.AppendText(result.Message + "\r\n");
                if (result.IsSuccess)
                {
                    richTextBoxTestInformation.AppendText("OBU版本号为：" + result.Version + "\r\n");
                }
            }
            catch (OperationCanceledException)
            {
                failureMessage = "OBU 版本号读取已由用户停止。";
                richTextBoxTestInformation.AppendText(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                failureMessage = "版本号读取失败：" + exception.Message;
                richTextBoxTestInformation.AppendText(failureMessage + "\r\n");
            }
            catch (InvalidOperationException exception)
            {
                // 设备前置条件不足时只记录失败，不让测试窗口因异常退出。
                failureMessage = "版本号读取失败：" + exception.Message;
                richTextBoxTestInformation.AppendText(failureMessage + "\r\n");
            }
            finally
            {
                DateTime completedAt = DateTime.Now;
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                string version = result == null ? string.Empty : result.Version;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                // 用例函数完成后统一保存结果文件和测试大框截图。
                PersistTestArtifacts(testCaseName, isSuccess, message, version, exchanges, startedAt, completedAt);
                // 版本读取完成、失败或取消后统一释放取消资源并恢复操作按钮。
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 记录尚未实现协议流程的独立测试用例入口，并按当前选项保存占位结果。
        /// </summary>
        /// <param name="testCaseKey">包含父级测试项和叶子名称的完整树路径。</param>
        private void RunPlaceholderTestCase(string testCaseKey)
        {
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? testCaseKey : selectedNode.Text;
            string placeholderMessage = "当前测试用例仅完成名称迁移，具体逻辑将在后续添加。";
            DateTime startedAt = DateTime.Now;
            richTextBoxTestInformation.AppendText(placeholderMessage + "\r\n");
            // 占位用例也走统一结果入口，但不会启动设备通信。
            PersistTestArtifacts(testCaseName, false, placeholderMessage, string.Empty, null, startedAt, DateTime.Now);
        }

        /// <summary>
        /// 生成选中叶子节点的稳定路径，用于区分同名测试用例。
        /// </summary>
        /// <param name="selectedNode">当前选中的测试用例叶子节点。</param>
        /// <returns>从根节点到叶子节点的“&gt;”分隔路径。</returns>
        private static string BuildTestCaseKey(TreeNode selectedNode)
        {
            Stack<string> pathParts = new Stack<string>();
            for (TreeNode current = selectedNode; current != null; current = current.Parent)
            {
                pathParts.Push(current.Text);
            }

            return string.Join(" > ", pathParts);
        }

        /// <summary>
        /// 按“存储测试结果”选项为当前测试用例保存 TXT 和测试大框 PNG 截图。
        /// </summary>
        /// <param name="testCaseName">当前测试用例名称。</param>
        /// <param name="isSuccess">测试最终是否成功。</param>
        /// <param name="message">最终结果说明或失败原因。</param>
        /// <param name="version">读取到的 OBU 版本号；非版本读取用例可以为空。</param>
        /// <param name="exchanges">完整请求/响应帧；没有设备交互时为空。</param>
        /// <param name="startedAt">测试开始时间。</param>
        /// <param name="completedAt">测试完成时间。</param>
        private void PersistTestArtifacts(
            string testCaseName,
            bool isSuccess,
            string message,
            string version,
            IList<ObuProtocolExchange> exchanges,
            DateTime startedAt,
            DateTime completedAt)
        {
            string finalStatusText = isSuccess ? "测试成功" : "测试失败";
            if (_isBatchScreenshotMode)
            {
                _batchAllTestsSucceeded = _batchAllTestsSucceeded && isSuccess;
            }
            // 所有用例的统一完成入口先显示明确结论，使界面截图和人工查看都能直接识别测试状态。
            richTextBoxTestInformation.AppendText("最终测试结果：" + finalStatusText + "\r\n");

            if (!checkBoxStoreTestResult.Checked)
            {
                return;
            }

            try
            {
                // 存储选项入口先落盘 TXT，保证完整帧和最终结论先于截图被保存。
                string resultFilePath = TestResultFileWriter.WriteTestResult(
                    testCaseName,
                    isSuccess,
                    message,
                    version,
                    exchanges,
                    startedAt,
                    completedAt);
                richTextBoxTestInformation.AppendText("测试结果文件：" + resultFilePath + "\r\n");

                if (!_isBatchScreenshotMode)
                {
                    // 单叶子测试仍在当前用例完成时保存截图；父级批量测试由队列结束入口统一截图。
                    string screenshotPath = TestResultScreenshotWriter.CaptureTestForm(testCaseName, this, isSuccess, completedAt);
                    richTextBoxTestInformation.AppendText("测试截图文件：" + screenshotPath + "\r\n");
                }
            }
            catch (IOException exception)
            {
                richTextBoxTestInformation.AppendText("测试结果文件或截图写入失败：" + exception.Message + "\r\n");
            }
            catch (UnauthorizedAccessException exception)
            {
                richTextBoxTestInformation.AppendText("测试结果文件或截图写入失败：" + exception.Message + "\r\n");
            }
        }

        /// <summary>
        /// 双击测试项名称本身时弹出该测试项的流程说明；父级测试项不再只执行展开操作，点击勾选框不会触发。
        /// </summary>
        /// <param name="sender">Designer 绑定的测试用例树。</param>
        /// <param name="e">包含节点和鼠标位置的双击事件数据。</param>
        private void treeViewTestItems_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeViewHitTestInfo hitTest = treeViewTestItems.HitTest(e.Location);
            bool isNodeName = hitTest.Location == TreeViewHitTestLocations.Label;
            bool isParentExpandGlyph = hitTest.Location == TreeViewHitTestLocations.PlusMinus;
            if ((!isNodeName && !isParentExpandGlyph) || e.Node == null)
            {
                return;
            }

            if (e.Node.Nodes.Count > 0 && e.Node.IsExpanded)
            {
                // TreeView 默认双击父级节点会展开；流程说明优先时恢复为收起状态，避免双击行为被展开动作占用。
                e.Node.Collapse();
            }

            // 使用包含父级分组的完整路径，避免“参数00”“测试用例1”等重名叶子显示到错误的测试逻辑。
            string logicText = GetTestCaseLogic(GetTestCasePath(e.Node));
            // 双击入口只展示当前测试项说明，不启动设备通信，实际测试仍由“测试”按钮负责。
            using (TestCaseLogicForm logicForm = new TestCaseLogicForm(e.Node.Text, logicText))
            {
                logicForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 生成测试树节点从顶级分组到当前节点的完整中文路径。
        /// </summary>
        /// <param name="node">用户双击的测试树节点。</param>
        /// <returns>使用“ &gt; ”连接的完整路径；节点为空时返回空字符串。</returns>
        private static string GetTestCasePath(TreeNode node)
        {
            List<string> parts = new List<string>();
            for (TreeNode current = node; current != null; current = current.Parent)
            {
                parts.Insert(0, current.Text);
            }

            return string.Join(" > ", parts.ToArray());
        }

        /// <summary>
        /// 返回指定测试用例的逻辑说明；已接入 OBU BST、随机避让、防碰撞、预读和 255S 配置驱动流程。
        /// </summary>
        /// <param name="testCaseName">树中当前选中的叶子用例名称。</param>
        /// <returns>显示在逻辑弹窗中的中文说明。</returns>
        private static string GetTestCaseLogic(string testCaseName)
        {
            if (!string.IsNullOrEmpty(testCaseName)
                && testCaseName.EndsWith("1、BST兼容性测试", StringComparison.Ordinal))
            {
                return "测试目标：验证 OBU 对原万集上位机 BST 兼容性序列的响应能力，并记录完整 5.8G 空中交互帧。\r\n\r\n"
                    + "执行流程：\r\n"
                    + "1. 从 exe 同级 SetMe.ini 的 [SET_Bst] / number 读取测试总次数。\r\n"
                    + "2. 每次按原 OBU 模块规则随机组合 MAC 控制域、BST 标识、应用标识和可选参数，生成并发送一条 BST。\r\n"
                    + "3. BST 发送成功后接收并解析 VST；该用例不发送 EventReport。\r\n"
                    + "4. 每次 BST/VST 完成后等待 20 ms，再开始下一次测试。\r\n\r\n"
                    + "成功判定：配置次数内的每次 BST/VST 请求和响应均成功。\r\n"
                    + "结果文件：勾选“存储测试结果”后保存到 exe 同级 TestResults 文件夹，截图保存到 TestScreenshots 文件夹。";
            }

            if (!string.IsNullOrEmpty(testCaseName)
                && testCaseName.EndsWith(ObuVersionTestCaseName, StringComparison.Ordinal))
            {
                return "测试目标：读取 OBU 版本号，并记录完整 5.8G 空中交互帧。\r\n\r\n"
                    + "执行流程：\r\n"
                    + "1. 发送 BST，接收 VST，并使用 VST 返回的前 4 字节 MACID 建立会话。\r\n"
                    + "2. 通过 TransferChannel 发送 71 01 01 01，读取 8 字节随机数和 20 字节版本密文。\r\n"
                    + "3. 按原上位机规则对随机数和版本密文执行异或变换。\r\n"
                    + "4. 发送 71 01 15 02 加密数据完成 71 通道二次认证。\r\n"
                    + "5. 发送 70 01 01 72 读取版本号，提取响应第 27～46 字节的 20 字节 ASCII 内容。\r\n\r\n"
                    + "关键请求帧：\r\n"
                    + "BST：00 01 FF FF FF FF 50 03 91 C0 01 08 FE 01 53 0F 20 81 00 01 41 83 29 A0 1A 00 04 00 2B 00\r\n"
                    + "认证第一步：08 00 00 01 40 F7 91 05 F4 03 18 71 01 01 01\r\n"
                    + "认证第二步：08 00 00 01 40 77 91 05 F4 03 18 71 01 15 02 + 20 字节认证数据\r\n"
                    + "版本读取：08 00 00 01 40 F7 91 05 01 03 18 70 01 01 72\r\n\r\n"
                    + "成功判定：DLL 请求/响应返回码为 0，认证和版本读取响应末尾为 90 00 00，且成功提取版本文本。\r\n"
                    + "结果文件：测试完成后保存到 exe 同级 TestResults 文件夹。";
            }

            if (testCaseName.IndexOf("VST随机避让", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 OBU 在连续、无间隔 BST 下的 VST 随机避让行为。\r\n\r\n"
                    + "执行流程：\r\n"
                    + "1. 从 exe 同级 SetMe.ini 的 [SET_139_RandomDodge_ZeroSecond_num] / number 读取测试次数，当前配置为 200 次；配置缺失或非法时默认 200 次。\r\n"
                    + "2. 每轮生成带当前 Unix 时间的 30 字节 BST，连续发送并接收 VST。\r\n"
                    + "3. VST 后发送单向 EventReport 释放链路；EventReport 本身不调用接收函数。\r\n"
                    + "4. EventReport 完成后，再发送一条独立的 OtherFrames 时间窗采样请求，读取响应前 4 字节并按原公式计算时间窗。\r\n\r\n"
                    + "通过判定：只判断第一时间窗（0.01,2.85]、第二时间窗（3.01,5.85]、第三时间窗（6.01,8.85] 的计数，三窗计数都必须严格大于测试次数的 1/4；当前 200 次配置时每窗至少 51 次。BST、EventReport、采样通信状态和其他时间窗均不单独作为失败条件。\r\n"
                    + "说明：OtherFrames 是 EventReport 之后独立的时间窗采样请求/响应，不是 EventReport 的响应。";
            }

            if (testCaseName.IndexOf("防碰撞", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：按原 OBU 模块执行一轮完整软算交易，并测量五个关键阶段是否都在 8 ms 内完成。\r\n\r\n"
                    + "执行流程：\r\n"
                    + "1. 发送带当前 Unix 时间的防碰撞 BST，接收 VST，使用 VST 返回的 MACID 建立会话。\r\n"
                    + "2. 按界面选择的 3DES/SM4 软算方式执行 GetSecure。\r\n"
                    + "3. TransferChannel 一次读取余额、0015、0019并校验返回状态；原防碰撞函数不解码该回复覆盖数据，MAC1 软算继续使用 VST 预读的 ICC0015。随后将交易金额低字节设为 06，执行消费初始化并更新0019。\r\n"
                    + "4. 使用消费初始化回复中的 EP 和随机数在本地计算 MAC1，软算过程密钥和扣费帧统一使用终端交易序号 00 00 06 C6，获取交易认证后完成扣费。\r\n"
                    + "5. 发送 SetMMI，通过 OtherFrames 读取 20 字节的五段处理时间，最后发送单向 EventReport 断链。\r\n\r\n"
                    + "配置规则：SetMe.ini 的 [SET_139_AntiCol_num] / number 表示应返回的计时项数，必须为 5，不是测试循环次数。\r\n\r\n"
                    + "判定标准：五个大端 4 字节原始值分别除以 1000，再依次扣除 1.0455、1.1975、2.833、0.955、1.350 ms。五项均必须满足 0 < t < 8 ms；任一 DLL 调用失败、TransferChannel 任一 APDU 状态异常、VST 缺少完整 ICC0015，或计时数据不足 20 字节，均判定失败。\r\n"
                    + "EventReport 只执行台发侧单向发送，不接收、不等待 OBU 回复。";
            }

            if (testCaseName.IndexOf("预读信息", StringComparison.Ordinal) >= 0)
            {
                string target = testCaseName.IndexOf("用例1", StringComparison.Ordinal) >= 0 ? "依次读取 0019、0002"
                    : testCaseName.IndexOf("用例2", StringComparison.Ordinal) >= 0 ? "读取 0015" : "读取 0012";
                return "测试目标：验证当前 BST 预读声明及后续文件访问，当前子项为“" + target + "”。\r\n\r\n"
                    + "执行流程：对应预读 BST/VST → ESAM 选择 3F00 → 读取 ESAM 系统信息 → IC 选择 1001 → " + target + " → 单向 EventReport。\r\n\r\n"
                    + "数据校对：VST 偏移14开始的26字节为预读 ESAM 系统信息，偏移41开始为目标 IC 文件；用例1还从偏移84提取4字节余额。后续使用相同 APDU 实际读取并逐字节比较。\r\n\r\n"
                    + "通过标准：BST/VST 成功，每一条 TransferChannel 响应状态正常，且 ESAM 系统信息、目标 IC 文件以及用例1余额均与 VST 预读值完全一致。EventReport 发送成功即可，不接收回复。";
            }

            if (testCaseName.IndexOf("OBU信道选择", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 OBU 能在当前子项指定的 0/1 物理信道响应 BST。\r\n\r\n"
                    + "原上位机流程：先把台发初始化到目标信道；0 信道发送 21 字节 BST，1 信道发送 30 字节 BST；收到 VST 后发送单向 EventReport，随后恢复默认 0 信道。\r\n\r\n"
                    + "通过标准：目标信道初始化成功、BST 发送成功、VST 接收成功、EventReport 发送成功。两个子项合并执行时必须两项都通过。EventReport 不接收回复。";
            }

            if (testCaseName.IndexOf("6、Getsecure测试", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "测试目标：验证当前“访问许可 × 加密密钥标识”组合的 GetSecure 响应结构。\r\n\r\n"
                    + "执行流程：30 字节 BST/VST → 当前子项对应的 25/26/33/34 字节 GetSecure → SetMMI → 单向 EventReport。无访问许可使用 91 05，有访问许可使用 91 0D；14 80/14 00 区分是否携带密钥标识。\r\n\r\n"
                    + "通过标准：DLL 请求/响应成功；响应指定字段为 15 01；按算法布局从响应位置 100 或 108 取出的 8 字节 Authenticator 必须全为 00。任一字段不符即失败。EventReport 不接收回复。";
            }

            if (testCaseName.IndexOf("7、transferchannel测试", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "测试目标：验证当前子项要求的 APDU 数量、通道和组合能在一条 TransferChannel 请求中正确执行。\r\n\r\n"
                    + "执行流程：24 字节 BST/VST → TransferChannel → SetMMI → 单向 EventReport。用例1～7为 IC 通道重复1～7条读余额；用例8～10为 ESAM 通道重复1～3条取4字节随机数；用例11为 IC 通道余额/0015/0012三条不同 APDU；用例12为 ESAM 通道选择3F00/读EF01/取芯片序列号三条不同 APDU。\r\n\r\n"
                    + "通过标准：BST/VST 成功；TransferChannel 的 DataList 数量与 APDUList 一致，每条响应均以 90 00 结束且 ReturnStatus 为 00；SetMMI 与 EventReport 发送成功。第11/12项不得用重复 APDU 代替。";
            }

            if (testCaseName.IndexOf("8、SetMMI测试", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "测试目标：验证 SetMMI 参数 00～04 对 OBU 声光提示的控制。\r\n\r\n"
                    + "原上位机流程：BST/VST → GetSecure → 当前参数 SetMMI → 等待 3 秒供人工观察 → 单向 EventReport。参数00为鸣一声/绿灯；01～03为不同异常提示组合；04为连续声光提示，具体表现需由测试人员现场确认。\r\n\r\n"
                    + "自动通过标准：BST/VST、GetSecure、SetMMI 请求和响应均成功，EventReport 发送成功。声光表现属于人工判据，软件不能仅凭 DLL 返回码声称其已正确。";
            }

            if (testCaseName.IndexOf("9、拼帧指令测试", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 OBU 对 1～5 个连续协议帧拼接发送时的拆分、顺序执行和拼接响应能力。\r\n\r\n"
                    + "原上位机流程：每组先 BST/VST，再遍历 GetSecure、IC读0015、ESAM取芯片序列号、SetMMI、Special 五种帧；分别构造1帧、2帧、3帧、4帧、5帧拼接组合。发送缓冲区按组合连续拼接，期望响应也按相同顺序拼接；预计响应超过100字节的组合跳过。\r\n\r\n"
                    + "轮次规则：固定执行5轮，各轮组合总数依次为5、25、125、625、3125；预计响应超过100字节的组合不下发，因此实际下发数依次为5、25、125、612、2754。每轮开始建链一次，组合失败时断链并重新建链，轮末统一断链。\r\n\r\n"
                    + "通过标准：全部实际下发组合均完成对应 DLL 请求和响应调用。旧版 Link1 中响应长度和逐字节比较代码已被注释，因此移植逻辑不额外启用这两项判定；任一收发失败仍使整体测试失败。";
            }

            if (testCaseName.IndexOf("255S保持", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 OBU 完成交易后，在 255 秒保持规则下对后续 BST 的交易/不交易选择。\r\n\r\n"
                    + "配置来源：exe 同级 BST_Locked_Sutong.ini 的 [BST_Locked_Sutong]；奇数键为 BST，紧随的偶数键为判定字节。00 表示应交易，01 表示不应交易。\r\n\r\n"
                    + "前置配置：用例启动后不再弹窗确认，程序默认关闭BID和UnixTime自动更新并重新初始化台发；测试结束分别恢复原配置，确保配置文件中的历史BeaconID和固定时间差不被DLL改写。LLC保持用户当前初始化设置。\r\n\r\n"
                    + "原上位机流程：判定00时要求 BST/VST 成功并完成车辆信息、IC信息、消费初始化、扣费、SetMMI、单向 EventReport，然后等待10秒；判定01时发送配置 BST，若收到 VST即说明255S规则失效并判失败，未收到VST则等待2秒进入下一条。\r\n\r\n"
                    + "交易参数：[TRADE_SET]/19前四字节为金额、末字节为出入口状态；算法跟随界面3DES/SM4。消费初始化交易序号从00开始循环，模板时间字段保持旧值1A A1 DD 02。\r\n\r\n"
                    + "通过标准：配置合法，所有00项完整交易成功，所有01项均无VST；任一项不符即整体失败。旧程序忽略SetMMI失败，EventReport不接收OBU回复。";
            }

            if (testCaseName.IndexOf("备用测试项", StringComparison.Ordinal) >= 0
                && testCaseName.IndexOf("车道交易测试", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：按原上位机备用测试项的软交易流程完成车道交易；实际分发函数为 RunLaneTransactionTest。\r\n\r\n"
                    + "原上位机流程：\r\n"
                    + "1. 从 exe 同级 SetMe.ini 的 [TRADE_SET]/19 读取 5 字节交易配置：前 4 字节为扣费金额，最后 1 字节为写入 ICC 0019 的出入口状态；配置缺失或非法时停止测试。\r\n"
                    + "2. 发送车道交易 BST，接收 VST；可选校验 VST 状态。\r\n"
                    + "3. 根据界面算法选择，在进程内使用 3DES 或 SM4 软算完成密钥分散、车辆信息解密和 MAC 计算；不访问硬件 PSAM。\r\n"
                    + "4. 按交易轮次交替读取或写入 ESAM EF04；读取 ICC 0019、0002 等卡片文件。\r\n"
                    + "5. 使用配置的 4 字节金额执行消费初始化，并把配置的最后 1 字节作为出入口状态写入 ICC 0019；获取消费认证、完成扣费，再按轮次写回 ESAM；调用 SetMMI，最后只发送单向 EventReport 断链。\r\n\r\n"
                    + "算法选择：3SDE 对应原 algorithmType=0，消费初始化算法标识 0x01；SM4 对应原 algorithmType=1，消费初始化算法标识 0x41。\r\n"
                    + "原上位机通过标准：配置的每一轮均完成上述交易步骤，成功轮数等于总轮数；任一 BST/VST、车辆信息校验、卡片文件、消费初始化、软算 MAC、扣费或 SetMMI 失败则失败。\r\n\r\n"
                    + "当前迁移状态：已接入独立软算服务和通信序列；真实 RSU/OBU 交互仍需连接设备确认，EventReport 发送后不接收回复。";
            }
            if (testCaseName.IndexOf("备用测试项", StringComparison.Ordinal) >= 0
                && testCaseName.IndexOf("门架交易测试", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：按旧版 WJ_MastTrade 流程完成一轮或多轮门架交易，并保存每一步完整空口帧。\r\n\r\n"
                    + "执行流程：门架专用 BST/VST → GetSecure 读取车辆信息 → 读取 ICC 0019/0002 → 读取 ESAM EF04 → 门架专用 EF04 写入 → 消费初始化 → 门架 40 字节扣费 → 单向 EventReport。旧版 WJ_MastTrade 在扣费后不追加 SetMMI。\r\n\r\n"
                    + "配置来源：交易次数和间隔读取 SetMe.ini 的 [SET_Trade_GB]；扣费金额读取 [TRADE_SET]/19 的前 4 字节。原 WJ_MastTrade 的 ConsumeInitialize_MJ_ruanpsam 不使用配置末字节，门架交易状态固定为 0x03；[TRADE_SET]/19 的最后 1 字节仍仅供车道交易使用。\r\n\r\n"
                    + "算法规则：沿用旧版 WJ_MastTrade 的门架报文布局，但 algorithmType 按界面选择；3DES 使用 0x01，SM4 使用 0x41，MAC1 采用对应软算法。通过标准为配置的每一轮完整步骤成功；EventReport 为单向发送，不接收回复。真实 RSU/OBU 交互仍需实机确认。";
            }
            if (testCaseName.IndexOf("备用测试项", StringComparison.Ordinal) >= 0
                && testCaseName.IndexOf("典型交易测试", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：移植原 WJ_Trade_typicaltransfer / WJ_Trade_typicaltransfer_SM4 的六个典型交易分支。\r\n\r\n"
                    + "固定流程：每个分支均执行 26 字节 BST/VST、GetSecure、分支专用读卡/交易组合、SetMMI 和单向 EventReport。六个分支依次覆盖：六种单/组合读卡后扣费；更新0019与扣费合并；三项组合读取、扣费读余额及外部认证更新EF08；消费初始化后取交易认证；初始化更新0019并扣费读余额；消费初始化后取交易认证。\r\n\r\n"
                    + "参数来源：金额读取 exe 同级 SetMe.ini 的 [TRADE_SET]/19 前4字节；原典型交易函数把0019出入口状态固定为04（ETC出口），不读取配置第5字节。MACID、合同信息和ICC0015来自当次VST；EP和随机数来自当次消费初始化回复；交易时间取运行时UTC；终端机编号固定为六个37；SM4/3DES及算法标识41/01来自界面选择。\r\n\r\n"
                    + "通过标准：六个固定分支全部完成，任一 APDU、SetMMI 或断链发送失败则整体失败。";
            }
            if (testCaseName.IndexOf("11、交易过程中响应广播帧测试", StringComparison.Ordinal) >= 0
                && testCaseName.IndexOf("消费初始化", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 OBU 完成消费初始化后不再响应广播 BST。\r\n\r\n"
                    + "执行流程：BST/VST → GetSecure → 消费初始化 TransferChannel → 再次发送广播 BST。\r\n\r\n"
                    + "通过标准：前三步请求/响应成功，消费初始化响应末尾为 90 00 00；第二次 BST 发送成功且接收 VST 返回非零，即 OBU 没有响应。若收到 VST 则失败。";
            }

            if (testCaseName.IndexOf("11、交易过程中响应广播帧测试", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 OBU 读取车辆信息后不再响应广播 BST。\r\n\r\n"
                    + "执行流程：BST/VST → GetSecure 读取车辆信息 → 再次发送广播 BST。\r\n\r\n"
                    + "通过标准：首次 BST/VST 和 GetSecure 请求/响应成功；第二次 BST 发送成功且接收 VST 返回非零，即 OBU 没有响应。若收到 VST 则失败。";
            }

            if (testCaseName.IndexOf("12、交易过程中响应不同MAC测试", StringComparison.Ordinal) >= 0)
            {
                string macRule = testCaseName.IndexOf("全ff", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "TransferChannel 的 MAC 固定为 FF FF FF FF"
                    : testCaseName.IndexOf("用例3", StringComparison.Ordinal) >= 0
                        ? "TransferChannel 的 MAC 第三字节在当前 OBU MAC 基础上加一"
                        : "TransferChannel 的 MAC 末字节在当前 OBU MAC 基础上加一";
                return "测试目标：验证 OBU 不响应目标 MAC 与自身不一致的 TransferChannel。\r\n\r\n"
                    + "执行流程：BST/VST → GetSecure 读取车辆信息 → 发送错误 MAC 的读取 0015 文件 TransferChannel。\r\n\r\n"
                    + "错误 MAC 规则：" + macRule + "；发送时禁止通信服务自动改回当前 OBU MAC。\r\n\r\n"
                    + "通过标准：首次 BST/VST 和 GetSecure 成功；错误 MAC TransferChannel 发送成功且接收返回非零，即 OBU 未响应。若收到响应则失败。";
            }

            if (testCaseName.IndexOf("14、指令集测试", StringComparison.Ordinal) >= 0)
            {
                return "测试目标：验证 ESAM 指令在正常参数、边界参数和错误参数下的响应状态码。\r\n\r\n"
                    + "执行流程：BST/VST → 选择 ESAM 3F00 目录 → 发送当前用例 APDU → SetMMI → EventReport 单向断链。\r\n\r\n"
                    + "通过标准：正常随机数、芯片序列号、EF04/0015/001A 读取或选择用例返回 90 00 00；9 字节随机数、错误 P1、错误 CLA、错误 INS 和错误长度用例分别返回原上位机规定的 67 00 00、6A 86 00、6E 00 00、6D 00 00；状态码不匹配即失败。";
            }

            return "测试项：" + (string.IsNullOrWhiteSpace(testCaseName) ? "未识别" : testCaseName) + "\r\n\r\n"
                + "当前实现状态：尚未实现。\r\n\r\n"
                + "目前仅建立了独立测试入口，尚未配置该项的 5.8G 数据帧、交互步骤和成功判定，因此不会启动设备通信，也不会把占位入口报告为测试通过。";
        }

        /// <summary>
        /// 判断版本号读取过程中常见的原生 DLL 互操作异常。
        /// </summary>
        /// <param name="exception">待判断的异常实例。</param>
        /// <returns>属于 DLL 加载或 Win32 调用异常时返回 true。</returns>
        private static bool IsNativeInteropException(Exception exception)
        {
            return exception is DllNotFoundException
                || exception is BadImageFormatException
                || exception is EntryPointNotFoundException
                || exception is System.ComponentModel.Win32Exception;
        }
    }
}
