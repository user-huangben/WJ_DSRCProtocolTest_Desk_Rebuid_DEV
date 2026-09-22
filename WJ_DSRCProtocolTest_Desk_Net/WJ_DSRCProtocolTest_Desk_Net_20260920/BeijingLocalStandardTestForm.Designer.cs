using System.Drawing;
using System.Windows.Forms;

namespace WJ_DSRCProtocolTest_Desk_Net
{
    partial class BeijingLocalStandardTestForm
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
        private Button buttonRun;
        private Button buttonStop;
        private Button buttonSelectAll;
        private Button buttonClear;
        private ProgressBar progressBarExecution;
        private Label labelExecutionStatus;
        private CheckBox checkBoxStoreTestResult;
        private CheckBox checkBoxValidateVehicleInformation;

        /// <summary>释放北京地标协议测试窗口使用的 Designer 组件资源。</summary>
        /// <param name="disposing">为 true 时释放托管组件。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>初始化北京地标测试的标题、测试树、操作区和测试信息区。</summary>
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
            buttonRun = new Button();
            buttonStop = new Button();
            buttonSelectAll = new Button();
            buttonClear = new Button();
            progressBarExecution = new ProgressBar();
            labelExecutionStatus = new Label();
            checkBoxStoreTestResult = new CheckBox();
            checkBoxValidateVehicleInformation = new CheckBox();
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
            labelTitle.Text = "北京地标协议测试";
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
            treeViewTestItems.AfterCheck += new TreeViewEventHandler(treeViewTestItems_AfterCheck);
            treeViewTestItems.AfterSelect += new TreeViewEventHandler(treeViewTestItems_AfterSelect);
            // 
            // groupBoxOperations
            // 
            groupBoxOperations.Controls.Add(progressBarExecution);
            groupBoxOperations.Controls.Add(labelExecutionStatus);
            groupBoxOperations.Controls.Add(buttonClear);
            groupBoxOperations.Controls.Add(buttonSelectAll);
            groupBoxOperations.Controls.Add(buttonStop);
            groupBoxOperations.Controls.Add(buttonRun);
            groupBoxOperations.Controls.Add(checkBoxStoreTestResult);
            groupBoxOperations.Controls.Add(checkBoxValidateVehicleInformation);
            groupBoxOperations.Dock = DockStyle.Fill;
            groupBoxOperations.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            groupBoxOperations.Margin = new Padding(0, 0, 8, 0);
            groupBoxOperations.Padding = new Padding(8);
            groupBoxOperations.Text = "操作选项";
            // 
            // buttonRun
            // 
            buttonRun.Dock = DockStyle.Top;
            buttonRun.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonRun.Height = 38;
            buttonRun.Text = "执行所选";
            buttonRun.UseVisualStyleBackColor = true;
            buttonRun.Click += new System.EventHandler(buttonRun_Click);
            // 
            // checkBoxStoreTestResult
            // 
            checkBoxStoreTestResult.AutoSize = true;
            checkBoxStoreTestResult.Dock = DockStyle.Top;
            checkBoxStoreTestResult.Font = new Font("Microsoft YaHei UI", 7F);
            checkBoxStoreTestResult.Padding = new Padding(2, 4, 0, 0);
            checkBoxStoreTestResult.Size = new Size(115, 28);
            checkBoxStoreTestResult.TabIndex = 6;
            checkBoxStoreTestResult.Text = "存储测试结果";
            checkBoxStoreTestResult.UseVisualStyleBackColor = true;
            // 
            // checkBoxValidateVehicleInformation
            // 
            checkBoxValidateVehicleInformation.AutoSize = true;
            checkBoxValidateVehicleInformation.Checked = true;
            checkBoxValidateVehicleInformation.CheckState = CheckState.Checked;
            checkBoxValidateVehicleInformation.Dock = DockStyle.Top;
            checkBoxValidateVehicleInformation.Font = new Font("Microsoft YaHei UI", 7F);
            checkBoxValidateVehicleInformation.Padding = new Padding(2, 4, 0, 0);
            checkBoxValidateVehicleInformation.Size = new Size(115, 28);
            checkBoxValidateVehicleInformation.TabIndex = 7;
            checkBoxValidateVehicleInformation.Text = "校验车辆信息";
            checkBoxValidateVehicleInformation.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            buttonStop.Dock = DockStyle.Top;
            buttonStop.Enabled = false;
            buttonStop.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonStop.Height = 38;
            buttonStop.Text = "停止测试";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += new System.EventHandler(buttonStop_Click);
            // 
            // buttonSelectAll
            // 
            buttonSelectAll.Dock = DockStyle.Top;
            buttonSelectAll.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonSelectAll.Height = 38;
            buttonSelectAll.Text = "全选/取消";
            buttonSelectAll.UseVisualStyleBackColor = true;
            buttonSelectAll.Click += new System.EventHandler(buttonSelectAll_Click);
            // 
            // buttonClear
            // 
            buttonClear.Dock = DockStyle.Top;
            buttonClear.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            buttonClear.Height = 38;
            buttonClear.Text = "清空显示";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += new System.EventHandler(buttonClear_Click);
            // 
            // labelExecutionStatus
            // 
            labelExecutionStatus.AutoEllipsis = true;
            labelExecutionStatus.Dock = DockStyle.Top;
            labelExecutionStatus.Font = new Font("Microsoft YaHei UI", 7.5F);
            labelExecutionStatus.Height = 42;
            labelExecutionStatus.Text = "已选择 0 个叶子用例。";
            labelExecutionStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBarExecution
            // 
            progressBarExecution.Dock = DockStyle.Top;
            progressBarExecution.Height = 20;
            progressBarExecution.Margin = new Padding(4, 6, 4, 5);
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
            richTextBoxInformation.Text = "测试信息将在执行测试后显示。";
            richTextBoxInformation.TextChanged += new System.EventHandler(richTextBoxInformation_TextChanged);
            // 
            // BeijingLocalStandardTestForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(249, 250, 252);
            ClientSize = new Size(1200, 760);
            Controls.Add(tableLayoutRoot);
            Font = new Font("Microsoft YaHei UI", 9F);
            MinimumSize = new Size(980, 620);
            Name = "BeijingLocalStandardTestForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "北京地标协议测试";
            FormClosing += new FormClosingEventHandler(BeijingLocalStandardTestForm_FormClosing);
            groupBoxInformation.ResumeLayout(false);
            groupBoxOperations.ResumeLayout(false);
            groupBoxTestItems.ResumeLayout(false);
            tableLayoutContent.ResumeLayout(false);
            tableLayoutRoot.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
