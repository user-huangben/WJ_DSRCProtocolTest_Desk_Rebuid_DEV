using System; using System.Collections.Generic; using System.Threading; using System.Threading.Tasks; using System.Windows.Forms;

namespace WJ_DSRCProtocolTest_Desk_Net
{
    /// <summary>
    /// 承载守望者播报测试的独立测试大框；具体测试项保留给后续需求实现。
    /// </summary>
    public partial class WatchmanBroadcastTestForm : Form
    {
        private CancellationTokenSource testCancellation;
        /// <summary>
        /// 初始化守望者播报测试独立窗口及其 Designer 管理的静态控件。
        /// </summary>
        public WatchmanBroadcastTestForm()
        {
            // 独立入口创建自己的窗体和控件，不能复用其他标准测试窗口实例。
            InitializeComponent();
        }
        /// <summary>执行当前选中的守望者播报框架占位流程。</summary>
        private async void buttonSingleTest_Click(object sender, EventArgs e) { await RunSelectedAsync(false); }
        /// <summary>按树顺序执行所有已勾选的守望者播报测试叶子。</summary>
        private async void buttonAllTests_Click(object sender, EventArgs e) { await RunSelectedAsync(true); }
        /// <summary>请求停止当前守望者播报测试流程。</summary>
        private void buttonStopTest_Click(object sender, EventArgs e) { if (testCancellation != null) testCancellation.Cancel(); }
        /// <summary>清空守望者播报测试日志显示。</summary>
        private void buttonClearDisplay_Click(object sender, EventArgs e) { richTextBoxInformation.Clear(); }

        /// <summary>
        /// 在守望者播报测试信息变化后将光标定位到末尾，确保最新状态始终可见。
        /// </summary>
        /// <param name="sender">Designer 绑定的测试信息 RichTextBox。</param>
        /// <param name="e">文本变化事件数据；当前实现不使用其附加信息。</param>
        private void richTextBoxInformation_TextChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.SelectionStart = richTextBoxInformation.TextLength;
            richTextBoxInformation.SelectionLength = 0;
            // 测试信息变化入口把显示位置滚动到末尾，覆盖追加日志和初始文本替换。
            richTextBoxInformation.ScrollToCaret();
        }

        /// <summary>同步父子节点勾选状态。</summary>
        private void treeViewTestItems_AfterCheck(object sender, TreeViewEventArgs e) { if (e.Action == TreeViewAction.Unknown) return; foreach (TreeNode n in e.Node.Nodes) n.Checked = e.Node.Checked; }
        /// <summary>显示选中播报用例的移植说明入口。</summary>
        private void treeViewTestItems_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) { richTextBoxInformation.AppendText("\r\n逻辑说明：" + e.Node.FullPath + "；当前为框架占位，待原上位机流程审计后接入。\r\n"); }
        private async Task RunSelectedAsync(bool allChecked) { var nodes = new List<TreeNode>(); Collect(treeViewTestItems.Nodes, nodes, allChecked); if (nodes.Count == 0 && treeViewTestItems.SelectedNode != null && treeViewTestItems.SelectedNode.Nodes.Count == 0) nodes.Add(treeViewTestItems.SelectedNode); if (nodes.Count == 0) { richTextBoxInformation.AppendText("\r\n请先选择守望者播报测试叶子。\r\n"); return; } testCancellation = new CancellationTokenSource(); buttonSingleTest.Enabled = buttonAllTests.Enabled = false; buttonStopTest.Enabled = true; try { foreach (TreeNode n in nodes) { testCancellation.Token.ThrowIfCancellationRequested(); richTextBoxInformation.AppendText("\r\n开始：" + n.FullPath + "（框架占位，未调用设备）"); await Task.Delay(50, testCancellation.Token); richTextBoxInformation.AppendText(" -> 待移植"); } } catch (OperationCanceledException) { richTextBoxInformation.AppendText("\r\n已停止。\r\n"); } finally { testCancellation.Dispose(); testCancellation = null; buttonSingleTest.Enabled = buttonAllTests.Enabled = true; buttonStopTest.Enabled = false; } }
        private static void Collect(TreeNodeCollection source, List<TreeNode> result, bool checkedOnly) { foreach (TreeNode n in source) { if (n.Nodes.Count == 0) { if (!checkedOnly || n.Checked) result.Add(n); } else Collect(n.Nodes, result, checkedOnly); } }
    }
}
