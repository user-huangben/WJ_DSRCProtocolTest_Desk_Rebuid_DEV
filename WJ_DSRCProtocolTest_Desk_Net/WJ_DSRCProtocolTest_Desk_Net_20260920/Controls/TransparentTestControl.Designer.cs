namespace WJ_DSRCProtocolTest_Desk_Net.Controls
{
    partial class TransparentTestControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRoot;
        private System.Windows.Forms.GroupBox groupBoxFrameAnalysis;
        private System.Windows.Forms.RichTextBox richTextBoxFrameAnalysis;
        private System.Windows.Forms.Button buttonClearLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRight;
        private System.Windows.Forms.GroupBox groupBoxFrameSequence;
        private System.Windows.Forms.TableLayoutPanel tableLayoutSequence;
        private System.Windows.Forms.ListBox listBoxLiveFrames;
        private System.Windows.Forms.ListBox listBoxFrameSequence;
        private System.Windows.Forms.Button buttonSetFrame;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveDown;
        private System.Windows.Forms.Button buttonDeleteFrame;
        private System.Windows.Forms.Button buttonClearSequence;
        private System.Windows.Forms.Button buttonOpenSequence;
        private System.Windows.Forms.Button buttonSaveSequence;
        private System.Windows.Forms.GroupBox groupBoxFrameEditor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutEditor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutBufferActions;
        private System.Windows.Forms.Label labelProtocolTemplate;
        private System.Windows.Forms.ComboBox comboBoxProtocolTemplate;
        private System.Windows.Forms.Button buttonAddProtocolTemplate;
        private System.Windows.Forms.Label labelIcTemplate;
        private System.Windows.Forms.ComboBox comboBoxIcTemplate;
        private System.Windows.Forms.Button buttonAddIcTemplate;
        private System.Windows.Forms.Label labelEsamTemplate;
        private System.Windows.Forms.ComboBox comboBoxEsamTemplate;
        private System.Windows.Forms.Button buttonAddEsamTemplate;
        private System.Windows.Forms.Label labelFrameBuffer;
        private System.Windows.Forms.TextBox textBoxFrameBuffer;
        private System.Windows.Forms.Button buttonClearBuffer;
        private System.Windows.Forms.Button buttonRunTest;
        private System.Windows.Forms.Label labelExecutionStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialogSequence;
        private System.Windows.Forms.SaveFileDialog saveFileDialogSequence;

        /// <summary>
        /// 释放透传测试控件持有的 Designer 组件资源。
        /// </summary>
        /// <param name="disposing">为 <see langword="true"/> 时释放托管组件容器。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 创建并配置透传测试页全部静态控件、布局关系、Tab 顺序和事件绑定。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxFrameAnalysis = new System.Windows.Forms.GroupBox();
            this.richTextBoxFrameAnalysis = new System.Windows.Forms.RichTextBox();
            this.tableLayoutRight = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxFrameSequence = new System.Windows.Forms.GroupBox();
            this.tableLayoutSequence = new System.Windows.Forms.TableLayoutPanel();
            this.listBoxLiveFrames = new System.Windows.Forms.ListBox();
            this.buttonOpenSequence = new System.Windows.Forms.Button();
            this.buttonSaveSequence = new System.Windows.Forms.Button();
            this.buttonClearLog = new System.Windows.Forms.Button();
            this.groupBoxFrameEditor = new System.Windows.Forms.GroupBox();
            this.tableLayoutEditor = new System.Windows.Forms.TableLayoutPanel();
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.listBoxFrameSequence = new System.Windows.Forms.ListBox();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.buttonDeleteFrame = new System.Windows.Forms.Button();
            this.buttonClearSequence = new System.Windows.Forms.Button();
            this.labelProtocolTemplate = new System.Windows.Forms.Label();
            this.comboBoxProtocolTemplate = new System.Windows.Forms.ComboBox();
            this.buttonAddProtocolTemplate = new System.Windows.Forms.Button();
            this.labelIcTemplate = new System.Windows.Forms.Label();
            this.comboBoxIcTemplate = new System.Windows.Forms.ComboBox();
            this.buttonAddIcTemplate = new System.Windows.Forms.Button();
            this.labelEsamTemplate = new System.Windows.Forms.Label();
            this.comboBoxEsamTemplate = new System.Windows.Forms.ComboBox();
            this.buttonAddEsamTemplate = new System.Windows.Forms.Button();
            this.labelFrameBuffer = new System.Windows.Forms.Label();
            this.textBoxFrameBuffer = new System.Windows.Forms.TextBox();
            this.tableLayoutBufferActions = new System.Windows.Forms.TableLayoutPanel();
            this.buttonSetFrame = new System.Windows.Forms.Button();
            this.buttonClearBuffer = new System.Windows.Forms.Button();
            this.labelExecutionStatus = new System.Windows.Forms.Label();
            this.buttonRunTest = new System.Windows.Forms.Button();
            this.openFileDialogSequence = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogSequence = new System.Windows.Forms.SaveFileDialog();
            this.tableLayoutRoot.SuspendLayout();
            this.groupBoxFrameAnalysis.SuspendLayout();
            this.tableLayoutRight.SuspendLayout();
            this.groupBoxFrameSequence.SuspendLayout();
            this.tableLayoutSequence.SuspendLayout();
            this.groupBoxFrameEditor.SuspendLayout();
            this.tableLayoutEditor.SuspendLayout();
            this.tableLayoutBufferActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutRoot
            // 
            this.tableLayoutRoot.ColumnCount = 2;
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.tableLayoutRoot.Controls.Add(this.groupBoxFrameAnalysis, 0, 0);
            this.tableLayoutRoot.Controls.Add(this.tableLayoutRight, 1, 0);
            this.tableLayoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRoot.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutRoot.Name = "tableLayoutRoot";
            this.tableLayoutRoot.Padding = new System.Windows.Forms.Padding(8);
            this.tableLayoutRoot.RowCount = 1;
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Size = new System.Drawing.Size(920, 620);
            this.tableLayoutRoot.TabIndex = 0;
            // 
            // groupBoxFrameAnalysis
            // 
            this.groupBoxFrameAnalysis.Controls.Add(this.richTextBoxFrameAnalysis);
            this.groupBoxFrameAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFrameAnalysis.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxFrameAnalysis.Location = new System.Drawing.Point(11, 11);
            this.groupBoxFrameAnalysis.Name = "groupBoxFrameAnalysis";
            this.groupBoxFrameAnalysis.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxFrameAnalysis.Size = new System.Drawing.Size(319, 598);
            this.groupBoxFrameAnalysis.TabIndex = 0;
            this.groupBoxFrameAnalysis.TabStop = false;
            this.groupBoxFrameAnalysis.Text = "帧解析";
            // 
            // richTextBoxFrameAnalysis
            // 
            this.richTextBoxFrameAnalysis.BackColor = System.Drawing.Color.White;
            this.richTextBoxFrameAnalysis.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxFrameAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxFrameAnalysis.Font = new System.Drawing.Font("Consolas", 9F);
            this.richTextBoxFrameAnalysis.Location = new System.Drawing.Point(10, 26);
            this.richTextBoxFrameAnalysis.Name = "richTextBoxFrameAnalysis";
            this.richTextBoxFrameAnalysis.ReadOnly = true;
            this.richTextBoxFrameAnalysis.Size = new System.Drawing.Size(299, 562);
            this.richTextBoxFrameAnalysis.TabIndex = 0;
            this.richTextBoxFrameAnalysis.Text = "点击右上方实时上下行帧进行解析。";
            this.richTextBoxFrameAnalysis.WordWrap = false;
            // 
            // tableLayoutRight
            // 
            this.tableLayoutRight.ColumnCount = 1;
            this.tableLayoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRight.Controls.Add(this.groupBoxFrameSequence, 0, 0);
            this.tableLayoutRight.Controls.Add(this.groupBoxFrameEditor, 0, 1);
            this.tableLayoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRight.Location = new System.Drawing.Point(336, 8);
            this.tableLayoutRight.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.tableLayoutRight.Name = "tableLayoutRight";
            this.tableLayoutRight.RowCount = 2;
            this.tableLayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutRight.Size = new System.Drawing.Size(576, 604);
            this.tableLayoutRight.TabIndex = 1;
            // 
            // groupBoxFrameSequence
            // 
            this.groupBoxFrameSequence.Controls.Add(this.tableLayoutSequence);
            this.groupBoxFrameSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFrameSequence.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxFrameSequence.Location = new System.Drawing.Point(3, 3);
            this.groupBoxFrameSequence.Name = "groupBoxFrameSequence";
            this.groupBoxFrameSequence.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxFrameSequence.Size = new System.Drawing.Size(570, 175);
            this.groupBoxFrameSequence.TabIndex = 0;
            this.groupBoxFrameSequence.TabStop = false;
            this.groupBoxFrameSequence.Text = "上下行帧（实时）";
            // 
            // tableLayoutSequence
            // 
            this.tableLayoutSequence.ColumnCount = 2;
            this.tableLayoutSequence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutSequence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutSequence.Controls.Add(this.listBoxLiveFrames, 0, 0);
            this.tableLayoutSequence.Controls.Add(this.buttonOpenSequence, 1, 0);
            this.tableLayoutSequence.Controls.Add(this.buttonSaveSequence, 1, 1);
            this.tableLayoutSequence.Controls.Add(this.buttonClearLog, 1, 2);
            this.tableLayoutSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutSequence.Location = new System.Drawing.Point(8, 24);
            this.tableLayoutSequence.Name = "tableLayoutSequence";
            this.tableLayoutSequence.RowCount = 3;
            this.tableLayoutSequence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutSequence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutSequence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutSequence.Size = new System.Drawing.Size(554, 143);
            this.tableLayoutSequence.TabIndex = 0;
            // 
            // listBoxLiveFrames
            // 
            this.listBoxLiveFrames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLiveFrames.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBoxLiveFrames.FormattingEnabled = true;
            this.listBoxLiveFrames.HorizontalScrollbar = true;
            this.listBoxLiveFrames.ItemHeight = 14;
            this.listBoxLiveFrames.Location = new System.Drawing.Point(3, 3);
            this.listBoxLiveFrames.Name = "listBoxLiveFrames";
            this.tableLayoutSequence.SetRowSpan(this.listBoxLiveFrames, 3);
            this.listBoxLiveFrames.Size = new System.Drawing.Size(472, 137);
            this.listBoxLiveFrames.TabIndex = 0;
            this.listBoxLiveFrames.SelectedIndexChanged += new System.EventHandler(this.listBoxLiveFrames_SelectedIndexChanged);
            // 
            // buttonOpenSequence
            // 
            this.buttonOpenSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOpenSequence.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOpenSequence.Location = new System.Drawing.Point(482, 4);
            this.buttonOpenSequence.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOpenSequence.Name = "buttonOpenSequence";
            this.buttonOpenSequence.Size = new System.Drawing.Size(68, 39);
            this.buttonOpenSequence.TabIndex = 1;
            this.buttonOpenSequence.Text = "打开";
            this.buttonOpenSequence.UseVisualStyleBackColor = true;
            this.buttonOpenSequence.Click += new System.EventHandler(this.buttonOpenSequence_Click);
            // 
            // buttonSaveSequence
            // 
            this.buttonSaveSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSaveSequence.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSaveSequence.Location = new System.Drawing.Point(482, 51);
            this.buttonSaveSequence.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSaveSequence.Name = "buttonSaveSequence";
            this.buttonSaveSequence.Size = new System.Drawing.Size(68, 39);
            this.buttonSaveSequence.TabIndex = 2;
            this.buttonSaveSequence.Text = "保存";
            this.buttonSaveSequence.UseVisualStyleBackColor = true;
            this.buttonSaveSequence.Click += new System.EventHandler(this.buttonSaveSequence_Click);
            // 
            // buttonClearLog
            // 
            this.buttonClearLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearLog.Location = new System.Drawing.Point(481, 97);
            this.buttonClearLog.Name = "buttonClearLog";
            this.buttonClearLog.Size = new System.Drawing.Size(70, 43);
            this.buttonClearLog.TabIndex = 1;
            this.buttonClearLog.Text = "清空";
            this.buttonClearLog.UseVisualStyleBackColor = true;
            this.buttonClearLog.Click += new System.EventHandler(this.buttonClearLog_Click);
            // 
            // groupBoxFrameEditor
            // 
            this.groupBoxFrameEditor.Controls.Add(this.tableLayoutEditor);
            this.groupBoxFrameEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFrameEditor.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxFrameEditor.Location = new System.Drawing.Point(3, 184);
            this.groupBoxFrameEditor.Name = "groupBoxFrameEditor";
            this.groupBoxFrameEditor.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxFrameEditor.Size = new System.Drawing.Size(570, 417);
            this.groupBoxFrameEditor.TabIndex = 1;
            this.groupBoxFrameEditor.TabStop = false;
            this.groupBoxFrameEditor.Text = "帧操作";
            // 
            // tableLayoutEditor
            // 
            this.tableLayoutEditor.ColumnCount = 4;
            this.tableLayoutEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableLayoutEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableLayoutEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableLayoutEditor.Controls.Add(this.buttonMoveUp, 3, 0);
            this.tableLayoutEditor.Controls.Add(this.listBoxFrameSequence, 0, 0);
            this.tableLayoutEditor.Controls.Add(this.buttonMoveDown, 3, 1);
            this.tableLayoutEditor.Controls.Add(this.buttonDeleteFrame, 3, 2);
            this.tableLayoutEditor.Controls.Add(this.buttonClearSequence, 3, 3);
            this.tableLayoutEditor.Controls.Add(this.labelProtocolTemplate, 0, 4);
            this.tableLayoutEditor.Controls.Add(this.comboBoxProtocolTemplate, 1, 4);
            this.tableLayoutEditor.Controls.Add(this.buttonAddProtocolTemplate, 2, 4);
            this.tableLayoutEditor.Controls.Add(this.labelIcTemplate, 0, 5);
            this.tableLayoutEditor.Controls.Add(this.comboBoxIcTemplate, 1, 5);
            this.tableLayoutEditor.Controls.Add(this.buttonAddIcTemplate, 2, 5);
            this.tableLayoutEditor.Controls.Add(this.labelEsamTemplate, 0, 6);
            this.tableLayoutEditor.Controls.Add(this.comboBoxEsamTemplate, 1, 6);
            this.tableLayoutEditor.Controls.Add(this.buttonAddEsamTemplate, 2, 6);
            this.tableLayoutEditor.Controls.Add(this.labelFrameBuffer, 0, 7);
            this.tableLayoutEditor.Controls.Add(this.textBoxFrameBuffer, 1, 7);
            this.tableLayoutEditor.Controls.Add(this.tableLayoutBufferActions, 3, 7);
            this.tableLayoutEditor.Controls.Add(this.labelExecutionStatus, 0, 8);
            this.tableLayoutEditor.Controls.Add(this.buttonRunTest, 3, 4);
            this.tableLayoutEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutEditor.Location = new System.Drawing.Point(8, 24);
            this.tableLayoutEditor.Name = "tableLayoutEditor";
            this.tableLayoutEditor.RowCount = 9;
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutEditor.Size = new System.Drawing.Size(554, 385);
            this.tableLayoutEditor.TabIndex = 0;
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMoveUp.Location = new System.Drawing.Point(462, 4);
            this.buttonMoveUp.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(88, 32);
            this.buttonMoveUp.TabIndex = 2;
            this.buttonMoveUp.Text = "上移";
            this.buttonMoveUp.UseVisualStyleBackColor = true;
            this.buttonMoveUp.Click += new System.EventHandler(this.buttonMoveUp_Click);
            // 
            // listBoxFrameSequence
            // 
            this.tableLayoutEditor.SetColumnSpan(this.listBoxFrameSequence, 3);
            this.listBoxFrameSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxFrameSequence.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBoxFrameSequence.FormattingEnabled = true;
            this.listBoxFrameSequence.HorizontalScrollbar = true;
            this.listBoxFrameSequence.IntegralHeight = false;
            this.listBoxFrameSequence.ItemHeight = 14;
            this.listBoxFrameSequence.Location = new System.Drawing.Point(3, 3);
            this.listBoxFrameSequence.Name = "listBoxFrameSequence";
            this.tableLayoutEditor.SetRowSpan(this.listBoxFrameSequence, 4);
            this.listBoxFrameSequence.Size = new System.Drawing.Size(452, 154);
            this.listBoxFrameSequence.TabIndex = 0;
            this.listBoxFrameSequence.SelectedIndexChanged += new System.EventHandler(this.listBoxFrameSequence_SelectedIndexChanged);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMoveDown.Location = new System.Drawing.Point(462, 44);
            this.buttonMoveDown.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(88, 32);
            this.buttonMoveDown.TabIndex = 3;
            this.buttonMoveDown.Text = "下移";
            this.buttonMoveDown.UseVisualStyleBackColor = true;
            this.buttonMoveDown.Click += new System.EventHandler(this.buttonMoveDown_Click);
            // 
            // buttonDeleteFrame
            // 
            this.buttonDeleteFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonDeleteFrame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDeleteFrame.Location = new System.Drawing.Point(462, 84);
            this.buttonDeleteFrame.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDeleteFrame.Name = "buttonDeleteFrame";
            this.buttonDeleteFrame.Size = new System.Drawing.Size(88, 32);
            this.buttonDeleteFrame.TabIndex = 4;
            this.buttonDeleteFrame.Text = "删除";
            this.buttonDeleteFrame.UseVisualStyleBackColor = true;
            this.buttonDeleteFrame.Click += new System.EventHandler(this.buttonDeleteFrame_Click);
            // 
            // buttonClearSequence
            // 
            this.buttonClearSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonClearSequence.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearSequence.Location = new System.Drawing.Point(462, 124);
            this.buttonClearSequence.Margin = new System.Windows.Forms.Padding(4);
            this.buttonClearSequence.Name = "buttonClearSequence";
            this.buttonClearSequence.Size = new System.Drawing.Size(88, 32);
            this.buttonClearSequence.TabIndex = 5;
            this.buttonClearSequence.Text = "清空";
            this.buttonClearSequence.UseVisualStyleBackColor = true;
            this.buttonClearSequence.Click += new System.EventHandler(this.buttonClearSequence_Click);
            // 
            // labelProtocolTemplate
            // 
            this.labelProtocolTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelProtocolTemplate.Location = new System.Drawing.Point(3, 160);
            this.labelProtocolTemplate.Name = "labelProtocolTemplate";
            this.labelProtocolTemplate.Size = new System.Drawing.Size(66, 40);
            this.labelProtocolTemplate.TabIndex = 6;
            this.labelProtocolTemplate.Text = "交易帧";
            this.labelProtocolTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxProtocolTemplate
            // 
            this.comboBoxProtocolTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxProtocolTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProtocolTemplate.FormattingEnabled = true;
            this.comboBoxProtocolTemplate.Location = new System.Drawing.Point(75, 170);
            this.comboBoxProtocolTemplate.Name = "comboBoxProtocolTemplate";
            this.comboBoxProtocolTemplate.Size = new System.Drawing.Size(284, 25);
            this.comboBoxProtocolTemplate.TabIndex = 7;
            // 
            // buttonAddProtocolTemplate
            // 
            this.buttonAddProtocolTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAddProtocolTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddProtocolTemplate.Location = new System.Drawing.Point(365, 163);
            this.buttonAddProtocolTemplate.Name = "buttonAddProtocolTemplate";
            this.buttonAddProtocolTemplate.Size = new System.Drawing.Size(90, 34);
            this.buttonAddProtocolTemplate.TabIndex = 8;
            this.buttonAddProtocolTemplate.Text = "添加";
            this.buttonAddProtocolTemplate.UseVisualStyleBackColor = true;
            this.buttonAddProtocolTemplate.Click += new System.EventHandler(this.buttonAddProtocolTemplate_Click);
            // 
            // labelIcTemplate
            // 
            this.labelIcTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelIcTemplate.Location = new System.Drawing.Point(3, 200);
            this.labelIcTemplate.Name = "labelIcTemplate";
            this.labelIcTemplate.Size = new System.Drawing.Size(66, 40);
            this.labelIcTemplate.TabIndex = 9;
            this.labelIcTemplate.Text = "IC 卡";
            this.labelIcTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxIcTemplate
            // 
            this.comboBoxIcTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxIcTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIcTemplate.FormattingEnabled = true;
            this.comboBoxIcTemplate.Location = new System.Drawing.Point(75, 210);
            this.comboBoxIcTemplate.Name = "comboBoxIcTemplate";
            this.comboBoxIcTemplate.Size = new System.Drawing.Size(284, 25);
            this.comboBoxIcTemplate.TabIndex = 10;
            // 
            // buttonAddIcTemplate
            // 
            this.buttonAddIcTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAddIcTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddIcTemplate.Location = new System.Drawing.Point(365, 203);
            this.buttonAddIcTemplate.Name = "buttonAddIcTemplate";
            this.buttonAddIcTemplate.Size = new System.Drawing.Size(90, 34);
            this.buttonAddIcTemplate.TabIndex = 11;
            this.buttonAddIcTemplate.Text = "添加";
            this.buttonAddIcTemplate.UseVisualStyleBackColor = true;
            this.buttonAddIcTemplate.Click += new System.EventHandler(this.buttonAddIcTemplate_Click);
            // 
            // labelEsamTemplate
            // 
            this.labelEsamTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelEsamTemplate.Location = new System.Drawing.Point(3, 240);
            this.labelEsamTemplate.Name = "labelEsamTemplate";
            this.labelEsamTemplate.Size = new System.Drawing.Size(66, 40);
            this.labelEsamTemplate.TabIndex = 12;
            this.labelEsamTemplate.Text = "ESAM";
            this.labelEsamTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxEsamTemplate
            // 
            this.comboBoxEsamTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxEsamTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEsamTemplate.FormattingEnabled = true;
            this.comboBoxEsamTemplate.Location = new System.Drawing.Point(75, 250);
            this.comboBoxEsamTemplate.Name = "comboBoxEsamTemplate";
            this.comboBoxEsamTemplate.Size = new System.Drawing.Size(284, 25);
            this.comboBoxEsamTemplate.TabIndex = 13;
            // 
            // buttonAddEsamTemplate
            // 
            this.buttonAddEsamTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAddEsamTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddEsamTemplate.Location = new System.Drawing.Point(365, 243);
            this.buttonAddEsamTemplate.Name = "buttonAddEsamTemplate";
            this.buttonAddEsamTemplate.Size = new System.Drawing.Size(90, 34);
            this.buttonAddEsamTemplate.TabIndex = 14;
            this.buttonAddEsamTemplate.Text = "添加";
            this.buttonAddEsamTemplate.UseVisualStyleBackColor = true;
            this.buttonAddEsamTemplate.Click += new System.EventHandler(this.buttonAddEsamTemplate_Click);
            // 
            // labelFrameBuffer
            // 
            this.labelFrameBuffer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelFrameBuffer.Location = new System.Drawing.Point(3, 280);
            this.labelFrameBuffer.Name = "labelFrameBuffer";
            this.labelFrameBuffer.Size = new System.Drawing.Size(66, 70);
            this.labelFrameBuffer.TabIndex = 15;
            this.labelFrameBuffer.Text = "BUF";
            this.labelFrameBuffer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxFrameBuffer
            // 
            this.textBoxFrameBuffer.AcceptsReturn = true;
            this.textBoxFrameBuffer.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.tableLayoutEditor.SetColumnSpan(this.textBoxFrameBuffer, 2);
            this.textBoxFrameBuffer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxFrameBuffer.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxFrameBuffer.Location = new System.Drawing.Point(75, 283);
            this.textBoxFrameBuffer.Multiline = true;
            this.textBoxFrameBuffer.Name = "textBoxFrameBuffer";
            this.textBoxFrameBuffer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxFrameBuffer.Size = new System.Drawing.Size(380, 64);
            this.textBoxFrameBuffer.TabIndex = 6;
            // 
            // tableLayoutBufferActions
            // 
            this.tableLayoutBufferActions.ColumnCount = 1;
            this.tableLayoutBufferActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutBufferActions.Controls.Add(this.buttonSetFrame, 0, 0);
            this.tableLayoutBufferActions.Controls.Add(this.buttonClearBuffer, 0, 1);
            this.tableLayoutBufferActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutBufferActions.Location = new System.Drawing.Point(458, 280);
            this.tableLayoutBufferActions.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutBufferActions.Name = "tableLayoutBufferActions";
            this.tableLayoutBufferActions.RowCount = 2;
            this.tableLayoutBufferActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutBufferActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutBufferActions.Size = new System.Drawing.Size(96, 70);
            this.tableLayoutBufferActions.TabIndex = 18;
            // 
            // buttonSetFrame
            // 
            this.buttonSetFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSetFrame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSetFrame.Location = new System.Drawing.Point(3, 3);
            this.buttonSetFrame.Name = "buttonSetFrame";
            this.buttonSetFrame.Size = new System.Drawing.Size(90, 29);
            this.buttonSetFrame.TabIndex = 1;
            this.buttonSetFrame.Text = "修改";
            this.buttonSetFrame.UseVisualStyleBackColor = true;
            this.buttonSetFrame.Click += new System.EventHandler(this.buttonSetFrame_Click);
            // 
            // buttonClearBuffer
            // 
            this.buttonClearBuffer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonClearBuffer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearBuffer.Location = new System.Drawing.Point(3, 38);
            this.buttonClearBuffer.Name = "buttonClearBuffer";
            this.buttonClearBuffer.Size = new System.Drawing.Size(90, 29);
            this.buttonClearBuffer.TabIndex = 16;
            this.buttonClearBuffer.Text = "清空 BUF";
            this.buttonClearBuffer.UseVisualStyleBackColor = true;
            this.buttonClearBuffer.Click += new System.EventHandler(this.buttonClearBuffer_Click);
            // 
            // labelExecutionStatus
            // 
            this.tableLayoutEditor.SetColumnSpan(this.labelExecutionStatus, 4);
            this.labelExecutionStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelExecutionStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.labelExecutionStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(101)))), ((int)(((byte)(119)))));
            this.labelExecutionStatus.Location = new System.Drawing.Point(3, 350);
            this.labelExecutionStatus.Name = "labelExecutionStatus";
            this.labelExecutionStatus.Size = new System.Drawing.Size(548, 35);
            this.labelExecutionStatus.TabIndex = 17;
            this.labelExecutionStatus.Text = "请先在左侧打开端口并初始化台发";
            this.labelExecutionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonRunTest
            // 
            this.buttonRunTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(101)))), ((int)(((byte)(162)))));
            this.buttonRunTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRunTest.FlatAppearance.BorderSize = 0;
            this.buttonRunTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRunTest.ForeColor = System.Drawing.Color.White;
            this.buttonRunTest.Location = new System.Drawing.Point(461, 163);
            this.buttonRunTest.Name = "buttonRunTest";
            this.buttonRunTest.Size = new System.Drawing.Size(90, 34);
            this.buttonRunTest.TabIndex = 9;
            this.buttonRunTest.Text = "测试";
            this.buttonRunTest.UseVisualStyleBackColor = false;
            this.buttonRunTest.Click += new System.EventHandler(this.buttonRunTest_Click);
            // 
            // openFileDialogSequence
            // 
            this.openFileDialogSequence.DefaultExt = "txt";
            this.openFileDialogSequence.Filter = "透传帧文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
            // 
            // saveFileDialogSequence
            // 
            this.saveFileDialogSequence.DefaultExt = "txt";
            this.saveFileDialogSequence.Filter = "透传帧文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
            // 
            // TransparentTestControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.Controls.Add(this.tableLayoutRoot);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.Name = "TransparentTestControl";
            this.Size = new System.Drawing.Size(920, 620);
            this.tableLayoutRoot.ResumeLayout(false);
            this.groupBoxFrameAnalysis.ResumeLayout(false);
            this.tableLayoutRight.ResumeLayout(false);
            this.groupBoxFrameSequence.ResumeLayout(false);
            this.tableLayoutSequence.ResumeLayout(false);
            this.groupBoxFrameEditor.ResumeLayout(false);
            this.tableLayoutEditor.ResumeLayout(false);
            this.tableLayoutEditor.PerformLayout();
            this.tableLayoutBufferActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
