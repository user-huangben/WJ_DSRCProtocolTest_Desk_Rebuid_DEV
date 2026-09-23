using System.Drawing;
using System.Windows.Forms;

namespace WJ_ComprehensiveTest_Desk
{
    partial class WanjiCpcStandardTestForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel tableLayoutRoot;
        private Label labelTitle;
        private TableLayoutPanel tableLayoutContent;
        private GroupBox groupBoxTestItems;
        private TreeView treeViewTestItems;
        private GroupBox groupBoxOperations;
        private GroupBox groupBoxInformation;
        private RichTextBox richTextBoxInformation;
        private Button buttonSingleTest;
        private Button buttonAllTests;
        private Button buttonStopTest;
        private Button buttonClearDisplay;

        /// <summary>
        /// 释放万集 CPC 卡标准测试窗口使用的 Designer 组件资源。
        /// </summary>
        /// <param name="disposing">为 <see langword="true"/> 时释放托管组件资源。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化万集 CPC 卡标准测试独立大框的标题、测试树、操作区和测试信息区。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tableLayoutRoot = new TableLayoutPanel();
            labelTitle = new Label();
            tableLayoutContent = new TableLayoutPanel();
            groupBoxTestItems = new GroupBox();
            treeViewTestItems = new TreeView();
            groupBoxOperations = new GroupBox();
            groupBoxInformation = new GroupBox();
            richTextBoxInformation = new RichTextBox();
            buttonSingleTest = new Button();
            buttonAllTests = new Button();
            buttonStopTest = new Button();
            buttonClearDisplay = new Button();
            tableLayoutRoot.SuspendLayout();
            tableLayoutContent.SuspendLayout();
            groupBoxTestItems.SuspendLayout();
            groupBoxOperations.SuspendLayout();
            groupBoxInformation.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutRoot
            // 
            tableLayoutRoot.ColumnCount = 1;
            tableLayoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutRoot.Controls.Add(labelTitle, 0, 0);
            tableLayoutRoot.Controls.Add(tableLayoutContent, 0, 1);
            tableLayoutRoot.Dock = DockStyle.Fill;
            tableLayoutRoot.Padding = new Padding(20);
            tableLayoutRoot.RowCount = 2;
            tableLayoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            tableLayoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // 
            // labelTitle
            // 
            labelTitle.BackColor = Color.FromArgb(28, 49, 78);
            labelTitle.Dock = DockStyle.Fill;
            labelTitle.Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold);
            labelTitle.ForeColor = Color.White;
            labelTitle.Padding = new Padding(18, 0, 0, 0);
            labelTitle.Text = "万集 CPC 卡标准测试";
            labelTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tableLayoutContent
            // 
            tableLayoutContent.ColumnCount = 3;
            tableLayoutContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            tableLayoutContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            tableLayoutContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            tableLayoutContent.Controls.Add(groupBoxTestItems, 0, 0);
            tableLayoutContent.Controls.Add(groupBoxOperations, 1, 0);
            tableLayoutContent.Controls.Add(groupBoxInformation, 2, 0);
            tableLayoutContent.Dock = DockStyle.Fill;
            tableLayoutContent.Margin = new Padding(0, 14, 0, 0);
            tableLayoutContent.RowCount = 1;
            tableLayoutContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // 
            // groupBoxTestItems
            // 
            groupBoxTestItems.Controls.Add(treeViewTestItems);
            groupBoxTestItems.Dock = DockStyle.Fill;
            groupBoxTestItems.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            groupBoxTestItems.Margin = new Padding(0, 0, 8, 0);
            groupBoxTestItems.Padding = new Padding(12);
            groupBoxTestItems.Text = "测试项选择";
            // 
            // treeViewTestItems
            // 
            treeViewTestItems.CheckBoxes = true;
            treeViewTestItems.Dock = DockStyle.Fill;
            treeViewTestItems.Font = new Font("Microsoft YaHei UI", 10F);
            treeViewTestItems.HideSelection = false;
            treeViewTestItems.Nodes.AddRange(new TreeNode[]
            {
                new TreeNode("卡片基础信息", new TreeNode[]
                {
                    new TreeNode("卡片复位"),
                    new TreeNode("卡片身份读取"),
                    new TreeNode("卡片版本读取")
                }),
                new TreeNode("文件与应用", new TreeNode[]
                {
                    new TreeNode("应用选择"),
                    new TreeNode("文件读取")
                }),
                new TreeNode("交易数据", new TreeNode[]
                {
                    new TreeNode("余额读取"),
                    new TreeNode("交易记录读取")
                }),
                new TreeNode("异常测试", new TreeNode[]
                {
                    new TreeNode("卡片未插入"),
                    new TreeNode("权限错误")
                })
            });
            treeViewTestItems.SelectedNode = treeViewTestItems.Nodes[0];
            treeViewTestItems.AfterCheck += new TreeViewEventHandler(treeViewTestItems_AfterCheck);
            treeViewTestItems.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeViewTestItems_NodeMouseDoubleClick);
            // 
            // groupBoxOperations
            // 
            groupBoxOperations.Controls.Add(buttonClearDisplay);
            groupBoxOperations.Controls.Add(buttonStopTest);
            groupBoxOperations.Controls.Add(buttonAllTests);
            groupBoxOperations.Controls.Add(buttonSingleTest);
            groupBoxOperations.Dock = DockStyle.Fill;
            groupBoxOperations.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            groupBoxOperations.Margin = new Padding(0, 0, 8, 0);
            groupBoxOperations.Padding = new Padding(8);
            groupBoxOperations.Text = "操作选项";
            // 
            // buttonSingleTest
            // 
            buttonSingleTest.Dock = DockStyle.Top;
            buttonSingleTest.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonSingleTest.Height = 38;
            buttonSingleTest.Text = "测试";
            buttonSingleTest.UseVisualStyleBackColor = true;
            buttonSingleTest.Click += new System.EventHandler(buttonSingleTest_Click);
            // 
            // buttonAllTests
            // 
            buttonAllTests.Dock = DockStyle.Top;
            buttonAllTests.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonAllTests.Height = 38;
            buttonAllTests.Text = "全部测试";
            buttonAllTests.UseVisualStyleBackColor = true;
            buttonAllTests.Click += new System.EventHandler(buttonAllTests_Click);
            // 
            // buttonStopTest
            // 
            buttonStopTest.Dock = DockStyle.Top;
            buttonStopTest.Enabled = false;
            buttonStopTest.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonStopTest.Height = 38;
            buttonStopTest.Text = "停止测试";
            buttonStopTest.UseVisualStyleBackColor = true;
            buttonStopTest.Click += new System.EventHandler(buttonStopTest_Click);
            // 
            // buttonClearDisplay
            // 
            buttonClearDisplay.Dock = DockStyle.Top;
            buttonClearDisplay.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonClearDisplay.Height = 38;
            buttonClearDisplay.Text = "清空显示";
            buttonClearDisplay.UseVisualStyleBackColor = true;
            buttonClearDisplay.Click += new System.EventHandler(buttonClearDisplay_Click);
            // 
            // groupBoxInformation
            // 
            groupBoxInformation.Controls.Add(richTextBoxInformation);
            groupBoxInformation.Dock = DockStyle.Fill;
            groupBoxInformation.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            groupBoxInformation.Padding = new Padding(12);
            groupBoxInformation.Text = "测试信息显示";
            // 
            // richTextBoxInformation
            // 
            richTextBoxInformation.BackColor = Color.White;
            richTextBoxInformation.BorderStyle = BorderStyle.FixedSingle;
            richTextBoxInformation.Dock = DockStyle.Fill;
            richTextBoxInformation.Font = new Font("Consolas", 10F);
            richTextBoxInformation.ForeColor = Color.FromArgb(55, 65, 81);
            richTextBoxInformation.ReadOnly = true;
            richTextBoxInformation.Text = "万集 CPC 卡标准测试框架已加载。请选择叶子用例后执行；具体卡片协议处理器待按原上位机逐项移植。";
            richTextBoxInformation.TextChanged += new System.EventHandler(richTextBoxInformation_TextChanged);
            // 
            // WanjiCpcStandardTestForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(249, 250, 252);
            ClientSize = new Size(1200, 760);
            Controls.Add(tableLayoutRoot);
            Font = new Font("Microsoft YaHei UI", 9F);
            MinimumSize = new Size(980, 620);
            Name = "WanjiCpcStandardTestForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "万集 CPC 卡标准测试";
            groupBoxInformation.ResumeLayout(false);
            groupBoxOperations.ResumeLayout(false);
            groupBoxTestItems.ResumeLayout(false);
            tableLayoutContent.ResumeLayout(false);
            tableLayoutRoot.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
