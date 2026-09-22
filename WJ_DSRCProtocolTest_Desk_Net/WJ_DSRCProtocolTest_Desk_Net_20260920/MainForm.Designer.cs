namespace WJ_DSRCProtocolTest_Desk_Net
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRoot;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Panel panelStationChannel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutStationChannel;
        private System.Windows.Forms.Label labelStationChannelTitle;
        private System.Windows.Forms.GroupBox groupBoxCommunication;
        private System.Windows.Forms.TableLayoutPanel tableLayoutCommunication;
        private System.Windows.Forms.ComboBox comboBoxPort;
        private System.Windows.Forms.Button buttonOpenClose;
        private System.Windows.Forms.Label labelPortMonitorStatus;
        private System.Windows.Forms.Timer timerPortMonitor;
        private System.Windows.Forms.GroupBox groupBoxStationParameters;
        private System.Windows.Forms.TableLayoutPanel tableLayoutParameters;
        private System.Windows.Forms.Label labelBstInterval;
        private System.Windows.Forms.TextBox textBoxBstInterval;
        private System.Windows.Forms.Label labelBstIntervalUnit;
        private System.Windows.Forms.Label labelRetryInterval;
        private System.Windows.Forms.TextBox textBoxRetryInterval;
        private System.Windows.Forms.Label labelRetryIntervalUnit;
        private System.Windows.Forms.Label labelRetryTimes;
        private System.Windows.Forms.TextBox textBoxRetryTimes;
        private System.Windows.Forms.Label labelRetryTimesUnit;
        private System.Windows.Forms.Label labelTimeout;
        private System.Windows.Forms.TextBox textBoxTimeout;
        private System.Windows.Forms.Label labelTimeoutUnit;
        private System.Windows.Forms.Label labelTxPower;
        private System.Windows.Forms.TextBox textBoxTxPower;
        private System.Windows.Forms.Label labelPhysicalChannel;
        private System.Windows.Forms.TextBox textBoxPhysicalChannel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutOptions;
        private System.Windows.Forms.CheckBox checkBoxBidChange;
        private System.Windows.Forms.CheckBox checkBoxUnixTimeChange;
        private System.Windows.Forms.CheckBox checkBoxLlcChange;
        private System.Windows.Forms.CheckBox checkBoxMacChange;
        private System.Windows.Forms.CheckBox checkBoxEnableLog;
        private System.Windows.Forms.Button buttonInitialize;
        private System.Windows.Forms.GroupBox groupBoxDeviceStatus;
        private System.Windows.Forms.Label labelDeviceStatus;
        private System.Windows.Forms.Panel panelWorkspace;
        private System.Windows.Forms.TabControl tabControlModules;
        private System.Windows.Forms.TabPage tabPageTransparentMode;
        private System.Windows.Forms.TabPage tabPageWanjiStandard;
        private System.Windows.Forms.TabPage tabPageDailyUse;
        private global::WJ_DSRCProtocolTest_Desk_Net.Controls.TransparentTestControl transparentTestControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutWanjiCategories;
        private System.Windows.Forms.Button buttonWanjiObuStandard;
        private System.Windows.Forms.Button buttonBeijingLocalStandard;
        private System.Windows.Forms.Button buttonWanjiCpcStandard;
        private System.Windows.Forms.Button buttonWatchmanBroadcast;
        private System.Windows.Forms.Label labelDailyPlaceholder;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.TabControl tabControlProducts;
        private System.Windows.Forms.TabPage tabPageDsrcProduct;
        private System.Windows.Forms.TabPage tabPageObuSerialProduct;
        private System.Windows.Forms.TabPage tabPageCreateCardProduct;
        private System.Windows.Forms.Label labelObuSerialPlaceholder;
        private System.Windows.Forms.Label labelCreateCardPlaceholder;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化主窗体全部静态控件、布局、页签以及设计器事件绑定。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControlProducts = new System.Windows.Forms.TabControl();
            this.tabPageDsrcProduct = new System.Windows.Forms.TabPage();
            this.tabPageObuSerialProduct = new System.Windows.Forms.TabPage();
            this.labelObuSerialPlaceholder = new System.Windows.Forms.Label();
            this.tabPageCreateCardProduct = new System.Windows.Forms.TabPage();
            this.labelCreateCardPlaceholder = new System.Windows.Forms.Label();
            this.tableLayoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panelStationChannel = new System.Windows.Forms.Panel();
            this.tableLayoutStationChannel = new System.Windows.Forms.TableLayoutPanel();
            this.labelStationChannelTitle = new System.Windows.Forms.Label();
            this.groupBoxCommunication = new System.Windows.Forms.GroupBox();
            this.tableLayoutCommunication = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxPort = new System.Windows.Forms.ComboBox();
            this.buttonOpenClose = new System.Windows.Forms.Button();
            this.labelPortMonitorStatus = new System.Windows.Forms.Label();
            this.groupBoxStationParameters = new System.Windows.Forms.GroupBox();
            this.tableLayoutParameters = new System.Windows.Forms.TableLayoutPanel();
            this.labelBstInterval = new System.Windows.Forms.Label();
            this.textBoxBstInterval = new System.Windows.Forms.TextBox();
            this.labelBstIntervalUnit = new System.Windows.Forms.Label();
            this.labelRetryInterval = new System.Windows.Forms.Label();
            this.textBoxRetryInterval = new System.Windows.Forms.TextBox();
            this.labelRetryIntervalUnit = new System.Windows.Forms.Label();
            this.labelRetryTimes = new System.Windows.Forms.Label();
            this.textBoxRetryTimes = new System.Windows.Forms.TextBox();
            this.labelRetryTimesUnit = new System.Windows.Forms.Label();
            this.labelTimeout = new System.Windows.Forms.Label();
            this.textBoxTimeout = new System.Windows.Forms.TextBox();
            this.labelTimeoutUnit = new System.Windows.Forms.Label();
            this.labelTxPower = new System.Windows.Forms.Label();
            this.textBoxTxPower = new System.Windows.Forms.TextBox();
            this.labelPhysicalChannel = new System.Windows.Forms.Label();
            this.textBoxPhysicalChannel = new System.Windows.Forms.TextBox();
            this.flowLayoutOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBoxBidChange = new System.Windows.Forms.CheckBox();
            this.checkBoxUnixTimeChange = new System.Windows.Forms.CheckBox();
            this.checkBoxLlcChange = new System.Windows.Forms.CheckBox();
            this.checkBoxMacChange = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableLog = new System.Windows.Forms.CheckBox();
            this.buttonInitialize = new System.Windows.Forms.Button();
            this.groupBoxDeviceStatus = new System.Windows.Forms.GroupBox();
            this.labelDeviceStatus = new System.Windows.Forms.Label();
            this.panelWorkspace = new System.Windows.Forms.Panel();
            this.tabControlModules = new System.Windows.Forms.TabControl();
            this.tabPageTransparentMode = new System.Windows.Forms.TabPage();
            this.transparentTestControl = new WJ_DSRCProtocolTest_Desk_Net.Controls.TransparentTestControl();
            this.tabPageWanjiStandard = new System.Windows.Forms.TabPage();
            this.tableLayoutWanjiCategories = new System.Windows.Forms.TableLayoutPanel();
            this.buttonWanjiObuStandard = new System.Windows.Forms.Button();
            this.buttonBeijingLocalStandard = new System.Windows.Forms.Button();
            this.buttonWanjiCpcStandard = new System.Windows.Forms.Button();
            this.buttonWatchmanBroadcast = new System.Windows.Forms.Button();
            this.tabPageDailyUse = new System.Windows.Forms.TabPage();
            this.labelDailyPlaceholder = new System.Windows.Forms.Label();
            this.timerPortMonitor = new System.Windows.Forms.Timer(this.components);
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControlProducts.SuspendLayout();
            this.tabPageDsrcProduct.SuspendLayout();
            this.tabPageObuSerialProduct.SuspendLayout();
            this.tabPageCreateCardProduct.SuspendLayout();
            this.tableLayoutRoot.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelStationChannel.SuspendLayout();
            this.tableLayoutStationChannel.SuspendLayout();
            this.groupBoxCommunication.SuspendLayout();
            this.tableLayoutCommunication.SuspendLayout();
            this.groupBoxStationParameters.SuspendLayout();
            this.tableLayoutParameters.SuspendLayout();
            this.flowLayoutOptions.SuspendLayout();
            this.groupBoxDeviceStatus.SuspendLayout();
            this.panelWorkspace.SuspendLayout();
            this.tabControlModules.SuspendLayout();
            this.tabPageTransparentMode.SuspendLayout();
            this.tabPageWanjiStandard.SuspendLayout();
            this.tableLayoutWanjiCategories.SuspendLayout();
            this.tabPageDailyUse.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlProducts
            // 
            this.tabControlProducts.Controls.Add(this.tabPageDsrcProduct);
            this.tabControlProducts.Controls.Add(this.tabPageObuSerialProduct);
            this.tabControlProducts.Controls.Add(this.tabPageCreateCardProduct);
            this.tabControlProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProducts.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControlProducts.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.tabControlProducts.ItemSize = new System.Drawing.Size(145, 28);
            this.tabControlProducts.Location = new System.Drawing.Point(0, 0);
            this.tabControlProducts.Name = "tabControlProducts";
            this.tabControlProducts.SelectedIndex = 0;
            this.tabControlProducts.Size = new System.Drawing.Size(1280, 777);
            this.tabControlProducts.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlProducts.TabIndex = 0;
            this.tabControlProducts.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControlProducts_DrawItem);
            // 
            // tabPageDsrcProduct
            // 
            this.tabPageDsrcProduct.Controls.Add(this.tableLayoutRoot);
            this.tabPageDsrcProduct.Location = new System.Drawing.Point(4, 46);
            this.tabPageDsrcProduct.Name = "tabPageDsrcProduct";
            this.tabPageDsrcProduct.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDsrcProduct.Size = new System.Drawing.Size(1272, 727);
            this.tabPageDsrcProduct.TabIndex = 0;
            this.tabPageDsrcProduct.Text = "WJ_DSRC_Protocol_test";
            this.tabPageDsrcProduct.UseVisualStyleBackColor = true;
            // 
            // tabPageObuSerialProduct
            // 
            this.tabPageObuSerialProduct.Controls.Add(this.labelObuSerialPlaceholder);
            this.tabPageObuSerialProduct.Location = new System.Drawing.Point(4, 46);
            this.tabPageObuSerialProduct.Name = "tabPageObuSerialProduct";
            this.tabPageObuSerialProduct.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageObuSerialProduct.Size = new System.Drawing.Size(1272, 727);
            this.tabPageObuSerialProduct.TabIndex = 1;
            this.tabPageObuSerialProduct.Text = "WJ_OBUSerial_Desk";
            this.tabPageObuSerialProduct.UseVisualStyleBackColor = true;
            // 
            // labelObuSerialPlaceholder
            // 
            this.labelObuSerialPlaceholder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelObuSerialPlaceholder.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelObuSerialPlaceholder.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.labelObuSerialPlaceholder.Name = "labelObuSerialPlaceholder";
            this.labelObuSerialPlaceholder.Text = "WJ_OBUSerial_Desk\r\n\r\n测试功能待后续添加";
            this.labelObuSerialPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPageCreateCardProduct
            // 
            this.tabPageCreateCardProduct.Controls.Add(this.labelCreateCardPlaceholder);
            this.tabPageCreateCardProduct.Location = new System.Drawing.Point(4, 46);
            this.tabPageCreateCardProduct.Name = "tabPageCreateCardProduct";
            this.tabPageCreateCardProduct.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCreateCardProduct.Size = new System.Drawing.Size(1272, 727);
            this.tabPageCreateCardProduct.TabIndex = 2;
            this.tabPageCreateCardProduct.Text = "WJ_CreatCard_Desk";
            this.tabPageCreateCardProduct.UseVisualStyleBackColor = true;
            // 
            // labelCreateCardPlaceholder
            // 
            this.labelCreateCardPlaceholder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCreateCardPlaceholder.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelCreateCardPlaceholder.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.labelCreateCardPlaceholder.Name = "labelCreateCardPlaceholder";
            this.labelCreateCardPlaceholder.Text = "WJ_CreatCard_Desk\r\n\r\n测试功能待后续添加";
            this.labelCreateCardPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutRoot
            // 
            this.tableLayoutRoot.ColumnCount = 2;
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280F));
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Controls.Add(this.panelStationChannel, 0, 0);
            this.tableLayoutRoot.Controls.Add(this.panelWorkspace, 1, 0);
            this.tableLayoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRoot.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutRoot.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutRoot.Name = "tableLayoutRoot";
            this.tableLayoutRoot.RowCount = 1;
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Size = new System.Drawing.Size(1266, 721);
            this.tableLayoutRoot.TabIndex = 0;
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(49)))), ((int)(((byte)(78)))));
            this.tableLayoutRoot.SetColumnSpan(this.panelHeader, 2);
            this.panelHeader.Controls.Add(this.labelSubtitle);
            this.panelHeader.Controls.Add(this.labelTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1280, 82);
            this.panelHeader.TabIndex = 0;
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.AutoSize = true;
            this.labelSubtitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.labelSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(214)))), ((int)(((byte)(232)))));
            this.labelSubtitle.Location = new System.Drawing.Point(29, 51);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(188, 17);
            this.labelSubtitle.TabIndex = 1;
            this.labelSubtitle.Text = "DSRC 协议测试上位机（重构版）";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(26, 14);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(358, 31);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "WJ_DSRC_Protocol_test";
            // 
            // panelStationChannel
            // 
            this.panelStationChannel.AutoScroll = true;
            this.panelStationChannel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelStationChannel.Controls.Add(this.tableLayoutStationChannel);
            this.panelStationChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStationChannel.Location = new System.Drawing.Point(0, 82);
            this.panelStationChannel.Margin = new System.Windows.Forms.Padding(0);
            this.panelStationChannel.Name = "panelStationChannel";
            this.panelStationChannel.Padding = new System.Windows.Forms.Padding(14);
            this.panelStationChannel.Size = new System.Drawing.Size(280, 695);
            this.panelStationChannel.TabIndex = 1;
            // 
            // tableLayoutStationChannel
            // 
            this.tableLayoutStationChannel.ColumnCount = 1;
            this.tableLayoutStationChannel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutStationChannel.Controls.Add(this.labelStationChannelTitle, 0, 0);
            this.tableLayoutStationChannel.Controls.Add(this.groupBoxCommunication, 0, 1);
            this.tableLayoutStationChannel.Controls.Add(this.groupBoxStationParameters, 0, 2);
            this.tableLayoutStationChannel.Controls.Add(this.groupBoxDeviceStatus, 0, 3);
            this.tableLayoutStationChannel.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutStationChannel.Location = new System.Drawing.Point(14, 14);
            this.tableLayoutStationChannel.Name = "tableLayoutStationChannel";
            this.tableLayoutStationChannel.RowCount = 4;
            this.tableLayoutStationChannel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutStationChannel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutStationChannel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 470F));
            this.tableLayoutStationChannel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutStationChannel.Size = new System.Drawing.Size(235, 714);
            this.tableLayoutStationChannel.TabIndex = 0;
            // 
            // labelStationChannelTitle
            // 
            this.labelStationChannelTitle.AutoSize = true;
            this.labelStationChannelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStationChannelTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelStationChannelTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(63)))), ((int)(((byte)(82)))));
            this.labelStationChannelTitle.Location = new System.Drawing.Point(3, 0);
            this.labelStationChannelTitle.Name = "labelStationChannelTitle";
            this.labelStationChannelTitle.Size = new System.Drawing.Size(229, 34);
            this.labelStationChannelTitle.TabIndex = 0;
            this.labelStationChannelTitle.Text = "台发设置通道";
            this.labelStationChannelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxCommunication
            // 
            this.groupBoxCommunication.Controls.Add(this.tableLayoutCommunication);
            this.groupBoxCommunication.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCommunication.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxCommunication.Location = new System.Drawing.Point(3, 37);
            this.groupBoxCommunication.Name = "groupBoxCommunication";
            this.groupBoxCommunication.Padding = new System.Windows.Forms.Padding(10, 12, 10, 10);
            this.groupBoxCommunication.Size = new System.Drawing.Size(229, 124);
            this.groupBoxCommunication.TabIndex = 1;
            this.groupBoxCommunication.TabStop = false;
            this.groupBoxCommunication.Text = "通信端口";
            // 
            // tableLayoutCommunication
            // 
            this.tableLayoutCommunication.ColumnCount = 2;
            this.tableLayoutCommunication.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutCommunication.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutCommunication.Controls.Add(this.comboBoxPort, 0, 0);
            this.tableLayoutCommunication.Controls.Add(this.buttonOpenClose, 1, 0);
            this.tableLayoutCommunication.Controls.Add(this.labelPortMonitorStatus, 0, 1);
            this.tableLayoutCommunication.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutCommunication.Location = new System.Drawing.Point(10, 28);
            this.tableLayoutCommunication.Name = "tableLayoutCommunication";
            this.tableLayoutCommunication.RowCount = 2;
            this.tableLayoutCommunication.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutCommunication.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutCommunication.Size = new System.Drawing.Size(209, 86);
            this.tableLayoutCommunication.TabIndex = 0;
            // 
            // comboBoxPort
            // 
            this.comboBoxPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPort.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.comboBoxPort.FormattingEnabled = true;
            this.comboBoxPort.Location = new System.Drawing.Point(3, 11);
            this.comboBoxPort.Name = "comboBoxPort";
            this.comboBoxPort.Size = new System.Drawing.Size(98, 25);
            this.comboBoxPort.TabIndex = 0;
            // 
            // buttonOpenClose
            // 
            this.buttonOpenClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenClose.BackColor = System.Drawing.Color.White;
            this.buttonOpenClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(197)))), ((int)(((byte)(211)))));
            this.buttonOpenClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOpenClose.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.buttonOpenClose.Location = new System.Drawing.Point(107, 7);
            this.buttonOpenClose.Name = "buttonOpenClose";
            this.buttonOpenClose.Size = new System.Drawing.Size(99, 34);
            this.buttonOpenClose.TabIndex = 1;
            this.buttonOpenClose.Text = "打开端口";
            this.buttonOpenClose.UseVisualStyleBackColor = false;
            this.buttonOpenClose.Click += new System.EventHandler(this.buttonOpenClose_Click);
            // 
            // labelPortMonitorStatus
            // 
            this.tableLayoutCommunication.SetColumnSpan(this.labelPortMonitorStatus, 2);
            this.labelPortMonitorStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPortMonitorStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.labelPortMonitorStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(101)))), ((int)(((byte)(119)))));
            this.labelPortMonitorStatus.Location = new System.Drawing.Point(3, 48);
            this.labelPortMonitorStatus.Name = "labelPortMonitorStatus";
            this.labelPortMonitorStatus.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.labelPortMonitorStatus.Size = new System.Drawing.Size(203, 38);
            this.labelPortMonitorStatus.TabIndex = 2;
            this.labelPortMonitorStatus.Text = "正在识别串口…";
            this.labelPortMonitorStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxStationParameters
            // 
            this.groupBoxStationParameters.Controls.Add(this.tableLayoutParameters);
            this.groupBoxStationParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxStationParameters.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxStationParameters.Location = new System.Drawing.Point(3, 167);
            this.groupBoxStationParameters.Name = "groupBoxStationParameters";
            this.groupBoxStationParameters.Padding = new System.Windows.Forms.Padding(10, 12, 10, 10);
            this.groupBoxStationParameters.Size = new System.Drawing.Size(229, 464);
            this.groupBoxStationParameters.TabIndex = 2;
            this.groupBoxStationParameters.TabStop = false;
            this.groupBoxStationParameters.Text = "台发参数";
            // 
            // tableLayoutParameters
            // 
            this.tableLayoutParameters.ColumnCount = 3;
            this.tableLayoutParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tableLayoutParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutParameters.Controls.Add(this.labelBstInterval, 0, 0);
            this.tableLayoutParameters.Controls.Add(this.textBoxBstInterval, 1, 0);
            this.tableLayoutParameters.Controls.Add(this.labelBstIntervalUnit, 2, 0);
            this.tableLayoutParameters.Controls.Add(this.labelRetryInterval, 0, 1);
            this.tableLayoutParameters.Controls.Add(this.textBoxRetryInterval, 1, 1);
            this.tableLayoutParameters.Controls.Add(this.labelRetryIntervalUnit, 2, 1);
            this.tableLayoutParameters.Controls.Add(this.labelRetryTimes, 0, 2);
            this.tableLayoutParameters.Controls.Add(this.textBoxRetryTimes, 1, 2);
            this.tableLayoutParameters.Controls.Add(this.labelRetryTimesUnit, 2, 2);
            this.tableLayoutParameters.Controls.Add(this.labelTimeout, 0, 3);
            this.tableLayoutParameters.Controls.Add(this.textBoxTimeout, 1, 3);
            this.tableLayoutParameters.Controls.Add(this.labelTimeoutUnit, 2, 3);
            this.tableLayoutParameters.Controls.Add(this.labelTxPower, 0, 4);
            this.tableLayoutParameters.Controls.Add(this.textBoxTxPower, 1, 4);
            this.tableLayoutParameters.Controls.Add(this.labelPhysicalChannel, 0, 5);
            this.tableLayoutParameters.Controls.Add(this.textBoxPhysicalChannel, 1, 5);
            this.tableLayoutParameters.Controls.Add(this.flowLayoutOptions, 0, 6);
            this.tableLayoutParameters.Controls.Add(this.buttonInitialize, 0, 7);
            this.tableLayoutParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutParameters.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.tableLayoutParameters.Location = new System.Drawing.Point(10, 28);
            this.tableLayoutParameters.Name = "tableLayoutParameters";
            this.tableLayoutParameters.RowCount = 8;
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 145F));
            this.tableLayoutParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutParameters.Size = new System.Drawing.Size(209, 426);
            this.tableLayoutParameters.TabIndex = 0;
            // 
            // labelBstInterval
            // 
            this.labelBstInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelBstInterval.Location = new System.Drawing.Point(3, 0);
            this.labelBstInterval.Name = "labelBstInterval";
            this.labelBstInterval.Size = new System.Drawing.Size(98, 38);
            this.labelBstInterval.TabIndex = 0;
            this.labelBstInterval.Text = "BST 间隔";
            this.labelBstInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxBstInterval
            // 
            this.textBoxBstInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBstInterval.Location = new System.Drawing.Point(107, 7);
            this.textBoxBstInterval.Name = "textBoxBstInterval";
            this.textBoxBstInterval.Size = new System.Drawing.Size(55, 23);
            this.textBoxBstInterval.TabIndex = 0;
            this.textBoxBstInterval.Text = "30";
            // 
            // labelBstIntervalUnit
            // 
            this.labelBstIntervalUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelBstIntervalUnit.Location = new System.Drawing.Point(168, 0);
            this.labelBstIntervalUnit.Name = "labelBstIntervalUnit";
            this.labelBstIntervalUnit.Size = new System.Drawing.Size(38, 38);
            this.labelBstIntervalUnit.TabIndex = 1;
            this.labelBstIntervalUnit.Text = "ms";
            this.labelBstIntervalUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelRetryInterval
            // 
            this.labelRetryInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRetryInterval.Location = new System.Drawing.Point(3, 38);
            this.labelRetryInterval.Name = "labelRetryInterval";
            this.labelRetryInterval.Size = new System.Drawing.Size(98, 38);
            this.labelRetryInterval.TabIndex = 2;
            this.labelRetryInterval.Text = "非 BST 帧间隔";
            this.labelRetryInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxRetryInterval
            // 
            this.textBoxRetryInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRetryInterval.Location = new System.Drawing.Point(107, 45);
            this.textBoxRetryInterval.Name = "textBoxRetryInterval";
            this.textBoxRetryInterval.Size = new System.Drawing.Size(55, 23);
            this.textBoxRetryInterval.TabIndex = 1;
            this.textBoxRetryInterval.Text = "20";
            // 
            // labelRetryIntervalUnit
            // 
            this.labelRetryIntervalUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRetryIntervalUnit.Location = new System.Drawing.Point(168, 38);
            this.labelRetryIntervalUnit.Name = "labelRetryIntervalUnit";
            this.labelRetryIntervalUnit.Size = new System.Drawing.Size(38, 38);
            this.labelRetryIntervalUnit.TabIndex = 3;
            this.labelRetryIntervalUnit.Text = "ms";
            this.labelRetryIntervalUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelRetryTimes
            // 
            this.labelRetryTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRetryTimes.Location = new System.Drawing.Point(3, 76);
            this.labelRetryTimes.Name = "labelRetryTimes";
            this.labelRetryTimes.Size = new System.Drawing.Size(98, 38);
            this.labelRetryTimes.TabIndex = 4;
            this.labelRetryTimes.Text = "帧发送次数";
            this.labelRetryTimes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxRetryTimes
            // 
            this.textBoxRetryTimes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRetryTimes.Location = new System.Drawing.Point(107, 83);
            this.textBoxRetryTimes.Name = "textBoxRetryTimes";
            this.textBoxRetryTimes.Size = new System.Drawing.Size(55, 23);
            this.textBoxRetryTimes.TabIndex = 2;
            this.textBoxRetryTimes.Text = "10";
            // 
            // labelRetryTimesUnit
            // 
            this.labelRetryTimesUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRetryTimesUnit.Location = new System.Drawing.Point(168, 76);
            this.labelRetryTimesUnit.Name = "labelRetryTimesUnit";
            this.labelRetryTimesUnit.Size = new System.Drawing.Size(38, 38);
            this.labelRetryTimesUnit.TabIndex = 5;
            this.labelRetryTimesUnit.Text = "次";
            this.labelRetryTimesUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTimeout
            // 
            this.labelTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTimeout.Location = new System.Drawing.Point(3, 114);
            this.labelTimeout.Name = "labelTimeout";
            this.labelTimeout.Size = new System.Drawing.Size(98, 38);
            this.labelTimeout.TabIndex = 6;
            this.labelTimeout.Text = "交易超时";
            this.labelTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTimeout
            // 
            this.textBoxTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTimeout.Location = new System.Drawing.Point(107, 121);
            this.textBoxTimeout.Name = "textBoxTimeout";
            this.textBoxTimeout.Size = new System.Drawing.Size(55, 23);
            this.textBoxTimeout.TabIndex = 3;
            this.textBoxTimeout.Text = "500";
            // 
            // labelTimeoutUnit
            // 
            this.labelTimeoutUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTimeoutUnit.Location = new System.Drawing.Point(168, 114);
            this.labelTimeoutUnit.Name = "labelTimeoutUnit";
            this.labelTimeoutUnit.Size = new System.Drawing.Size(38, 38);
            this.labelTimeoutUnit.TabIndex = 7;
            this.labelTimeoutUnit.Text = "ms";
            this.labelTimeoutUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTxPower
            // 
            this.labelTxPower.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTxPower.Location = new System.Drawing.Point(3, 152);
            this.labelTxPower.Name = "labelTxPower";
            this.labelTxPower.Size = new System.Drawing.Size(98, 38);
            this.labelTxPower.TabIndex = 8;
            this.labelTxPower.Text = "功率等级";
            this.labelTxPower.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTxPower
            // 
            this.textBoxTxPower.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutParameters.SetColumnSpan(this.textBoxTxPower, 2);
            this.textBoxTxPower.Location = new System.Drawing.Point(107, 159);
            this.textBoxTxPower.Name = "textBoxTxPower";
            this.textBoxTxPower.Size = new System.Drawing.Size(99, 23);
            this.textBoxTxPower.TabIndex = 4;
            this.textBoxTxPower.Text = "10";
            // 
            // labelPhysicalChannel
            // 
            this.labelPhysicalChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPhysicalChannel.Location = new System.Drawing.Point(3, 190);
            this.labelPhysicalChannel.Name = "labelPhysicalChannel";
            this.labelPhysicalChannel.Size = new System.Drawing.Size(98, 38);
            this.labelPhysicalChannel.TabIndex = 9;
            this.labelPhysicalChannel.Text = "物理信道";
            this.labelPhysicalChannel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxPhysicalChannel
            // 
            this.textBoxPhysicalChannel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutParameters.SetColumnSpan(this.textBoxPhysicalChannel, 2);
            this.textBoxPhysicalChannel.Location = new System.Drawing.Point(107, 197);
            this.textBoxPhysicalChannel.Name = "textBoxPhysicalChannel";
            this.textBoxPhysicalChannel.Size = new System.Drawing.Size(99, 23);
            this.textBoxPhysicalChannel.TabIndex = 5;
            this.textBoxPhysicalChannel.Text = "0";
            // 
            // flowLayoutOptions
            // 
            this.tableLayoutParameters.SetColumnSpan(this.flowLayoutOptions, 3);
            this.flowLayoutOptions.Controls.Add(this.checkBoxBidChange);
            this.flowLayoutOptions.Controls.Add(this.checkBoxUnixTimeChange);
            this.flowLayoutOptions.Controls.Add(this.checkBoxLlcChange);
            this.flowLayoutOptions.Controls.Add(this.checkBoxMacChange);
            this.flowLayoutOptions.Controls.Add(this.checkBoxEnableLog);
            this.flowLayoutOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutOptions.Location = new System.Drawing.Point(3, 231);
            this.flowLayoutOptions.Name = "flowLayoutOptions";
            this.flowLayoutOptions.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flowLayoutOptions.Size = new System.Drawing.Size(203, 139);
            this.flowLayoutOptions.TabIndex = 6;
            // 
            // checkBoxBidChange
            // 
            this.checkBoxBidChange.AutoSize = true;
            this.checkBoxBidChange.Checked = true;
            this.checkBoxBidChange.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBidChange.Location = new System.Drawing.Point(3, 11);
            this.checkBoxBidChange.Name = "checkBoxBidChange";
            this.checkBoxBidChange.Size = new System.Drawing.Size(76, 21);
            this.checkBoxBidChange.TabIndex = 0;
            this.checkBoxBidChange.Text = "BID 更新";
            // 
            // checkBoxUnixTimeChange
            // 
            this.checkBoxUnixTimeChange.AutoSize = true;
            this.checkBoxUnixTimeChange.Checked = true;
            this.checkBoxUnixTimeChange.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUnixTimeChange.Location = new System.Drawing.Point(85, 11);
            this.checkBoxUnixTimeChange.Name = "checkBoxUnixTimeChange";
            this.checkBoxUnixTimeChange.Size = new System.Drawing.Size(108, 21);
            this.checkBoxUnixTimeChange.TabIndex = 1;
            this.checkBoxUnixTimeChange.Text = "UnixTime 更新";
            // 
            // checkBoxLlcChange
            // 
            this.checkBoxLlcChange.AutoSize = true;
            this.checkBoxLlcChange.Checked = true;
            this.checkBoxLlcChange.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLlcChange.Location = new System.Drawing.Point(3, 38);
            this.checkBoxLlcChange.Name = "checkBoxLlcChange";
            this.checkBoxLlcChange.Size = new System.Drawing.Size(75, 21);
            this.checkBoxLlcChange.TabIndex = 2;
            this.checkBoxLlcChange.Text = "LLC 更新";
            // 
            // checkBoxMacChange
            // 
            this.checkBoxMacChange.AutoSize = true;
            this.checkBoxMacChange.Checked = true;
            this.checkBoxMacChange.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMacChange.Location = new System.Drawing.Point(84, 38);
            this.checkBoxMacChange.Name = "checkBoxMacChange";
            this.checkBoxMacChange.Size = new System.Drawing.Size(83, 21);
            this.checkBoxMacChange.TabIndex = 3;
            this.checkBoxMacChange.Text = "MAC 更新";
            // 
            // checkBoxEnableLog
            // 
            this.checkBoxEnableLog.AutoSize = true;
            this.checkBoxEnableLog.Checked = true;
            this.checkBoxEnableLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableLog.Location = new System.Drawing.Point(3, 65);
            this.checkBoxEnableLog.Name = "checkBoxEnableLog";
            this.checkBoxEnableLog.Size = new System.Drawing.Size(75, 21);
            this.checkBoxEnableLog.TabIndex = 4;
            this.checkBoxEnableLog.Text = "启用日志";
            this.checkBoxEnableLog.CheckedChanged += new System.EventHandler(this.checkBoxEnableLog_CheckedChanged);
            // 
            // buttonInitialize
            // 
            this.buttonInitialize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(101)))), ((int)(((byte)(162)))));
            this.tableLayoutParameters.SetColumnSpan(this.buttonInitialize, 3);
            this.buttonInitialize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonInitialize.Enabled = false;
            this.buttonInitialize.FlatAppearance.BorderSize = 0;
            this.buttonInitialize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInitialize.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonInitialize.ForeColor = System.Drawing.Color.White;
            this.buttonInitialize.Location = new System.Drawing.Point(3, 376);
            this.buttonInitialize.Name = "buttonInitialize";
            this.buttonInitialize.Size = new System.Drawing.Size(203, 47);
            this.buttonInitialize.TabIndex = 7;
            this.buttonInitialize.Text = "初始化台发";
            this.buttonInitialize.UseVisualStyleBackColor = false;
            this.buttonInitialize.Click += new System.EventHandler(this.buttonInitialize_Click);
            // 
            // groupBoxDeviceStatus
            // 
            this.groupBoxDeviceStatus.Controls.Add(this.labelDeviceStatus);
            this.groupBoxDeviceStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDeviceStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxDeviceStatus.Location = new System.Drawing.Point(3, 637);
            this.groupBoxDeviceStatus.Name = "groupBoxDeviceStatus";
            this.groupBoxDeviceStatus.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxDeviceStatus.Size = new System.Drawing.Size(229, 74);
            this.groupBoxDeviceStatus.TabIndex = 3;
            this.groupBoxDeviceStatus.TabStop = false;
            this.groupBoxDeviceStatus.Text = "设备状态";
            // 
            // labelDeviceStatus
            // 
            this.labelDeviceStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDeviceStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.labelDeviceStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(101)))), ((int)(((byte)(119)))));
            this.labelDeviceStatus.Location = new System.Drawing.Point(10, 26);
            this.labelDeviceStatus.Name = "labelDeviceStatus";
            this.labelDeviceStatus.Size = new System.Drawing.Size(209, 38);
            this.labelDeviceStatus.TabIndex = 0;
            this.labelDeviceStatus.Text = "等待打开通信端口";
            // 
            // panelWorkspace
            // 
            this.panelWorkspace.BackColor = System.Drawing.Color.White;
            this.panelWorkspace.Controls.Add(this.tabControlModules);
            this.panelWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWorkspace.Location = new System.Drawing.Point(280, 82);
            this.panelWorkspace.Margin = new System.Windows.Forms.Padding(0);
            this.panelWorkspace.Name = "panelWorkspace";
            this.panelWorkspace.Padding = new System.Windows.Forms.Padding(18);
            this.panelWorkspace.Size = new System.Drawing.Size(1000, 695);
            this.panelWorkspace.TabIndex = 2;
            // 
            // tabControlModules
            // 
            this.tabControlModules.Controls.Add(this.tabPageTransparentMode);
            this.tabControlModules.Controls.Add(this.tabPageWanjiStandard);
            this.tabControlModules.Controls.Add(this.tabPageDailyUse);
            this.tabControlModules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlModules.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.tabControlModules.Location = new System.Drawing.Point(18, 18);
            this.tabControlModules.Name = "tabControlModules";
            this.tabControlModules.SelectedIndex = 0;
            this.tabControlModules.Size = new System.Drawing.Size(964, 659);
            this.tabControlModules.TabIndex = 0;
            // 
            // tabPageTransparentMode
            // 
            this.tabPageTransparentMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.tabPageTransparentMode.Controls.Add(this.transparentTestControl);
            this.tabPageTransparentMode.Location = new System.Drawing.Point(4, 26);
            this.tabPageTransparentMode.Name = "tabPageTransparentMode";
            this.tabPageTransparentMode.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTransparentMode.Size = new System.Drawing.Size(956, 629);
            this.tabPageTransparentMode.TabIndex = 0;
            this.tabPageTransparentMode.Text = "透传模式";
            // 
            // transparentTestControl
            // 
            this.transparentTestControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.transparentTestControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transparentTestControl.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.transparentTestControl.Location = new System.Drawing.Point(3, 3);
            this.transparentTestControl.Name = "transparentTestControl";
            this.transparentTestControl.Size = new System.Drawing.Size(950, 623);
            this.transparentTestControl.TabIndex = 0;
            // 
            // tabPageWanjiStandard
            // 
            this.tabPageWanjiStandard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.tabPageWanjiStandard.Controls.Add(this.tableLayoutWanjiCategories);
            this.tabPageWanjiStandard.Location = new System.Drawing.Point(4, 26);
            this.tabPageWanjiStandard.Name = "tabPageWanjiStandard";
            this.tabPageWanjiStandard.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWanjiStandard.Size = new System.Drawing.Size(956, 629);
            this.tabPageWanjiStandard.TabIndex = 1;
            this.tabPageWanjiStandard.Text = "WJ_DSRC_Protocol_test";
            // 
            // tableLayoutWanjiCategories
            // 
            this.tableLayoutWanjiCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.tableLayoutWanjiCategories.ColumnCount = 4;
            this.tableLayoutWanjiCategories.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutWanjiCategories.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutWanjiCategories.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutWanjiCategories.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutWanjiCategories.Controls.Add(this.buttonWanjiObuStandard, 0, 0);
            this.tableLayoutWanjiCategories.Controls.Add(this.buttonBeijingLocalStandard, 1, 0);
            this.tableLayoutWanjiCategories.Controls.Add(this.buttonWanjiCpcStandard, 2, 0);
            this.tableLayoutWanjiCategories.Controls.Add(this.buttonWatchmanBroadcast, 3, 0);
            this.tableLayoutWanjiCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutWanjiCategories.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutWanjiCategories.Name = "tableLayoutWanjiCategories";
            this.tableLayoutWanjiCategories.Padding = new System.Windows.Forms.Padding(18);
            this.tableLayoutWanjiCategories.RowCount = 1;
            this.tableLayoutWanjiCategories.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutWanjiCategories.Size = new System.Drawing.Size(950, 623);
            this.tableLayoutWanjiCategories.TabIndex = 0;
            // 
            // buttonWanjiObuStandard
            // 
            this.buttonWanjiObuStandard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(101)))), ((int)(((byte)(162)))));
            this.buttonWanjiObuStandard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonWanjiObuStandard.FlatAppearance.BorderSize = 0;
            this.buttonWanjiObuStandard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonWanjiObuStandard.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold);
            this.buttonWanjiObuStandard.ForeColor = System.Drawing.Color.White;
            this.buttonWanjiObuStandard.Location = new System.Drawing.Point(28, 28);
            this.buttonWanjiObuStandard.Margin = new System.Windows.Forms.Padding(10);
            this.buttonWanjiObuStandard.Name = "buttonWanjiObuStandard";
            this.buttonWanjiObuStandard.Size = new System.Drawing.Size(208, 567);
            this.buttonWanjiObuStandard.TabIndex = 0;
            this.buttonWanjiObuStandard.Tag = "万集 OBU 标准测试";
            this.buttonWanjiObuStandard.Text = "万集 OBU\r\n标准测试";
            this.buttonWanjiObuStandard.UseVisualStyleBackColor = false;
            this.buttonWanjiObuStandard.Click += new System.EventHandler(this.buttonWanjiObuStandard_Click);
            // 
            // buttonBeijingLocalStandard
            // 
            this.buttonBeijingLocalStandard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(119)))), ((int)(((byte)(177)))));
            this.buttonBeijingLocalStandard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonBeijingLocalStandard.FlatAppearance.BorderSize = 0;
            this.buttonBeijingLocalStandard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBeijingLocalStandard.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold);
            this.buttonBeijingLocalStandard.ForeColor = System.Drawing.Color.White;
            this.buttonBeijingLocalStandard.Location = new System.Drawing.Point(256, 28);
            this.buttonBeijingLocalStandard.Margin = new System.Windows.Forms.Padding(10);
            this.buttonBeijingLocalStandard.Name = "buttonBeijingLocalStandard";
            this.buttonBeijingLocalStandard.Size = new System.Drawing.Size(208, 567);
            this.buttonBeijingLocalStandard.TabIndex = 1;
            this.buttonBeijingLocalStandard.Tag = "北京地标协议测试";
            this.buttonBeijingLocalStandard.Text = "北京地标\r\n协议测试";
            this.buttonBeijingLocalStandard.UseVisualStyleBackColor = false;
            this.buttonBeijingLocalStandard.Click += new System.EventHandler(this.buttonBeijingLocalStandard_Click);
            // 
            // buttonWanjiCpcStandard
            // 
            this.buttonWanjiCpcStandard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(137)))), ((int)(((byte)(191)))));
            this.buttonWanjiCpcStandard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonWanjiCpcStandard.FlatAppearance.BorderSize = 0;
            this.buttonWanjiCpcStandard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonWanjiCpcStandard.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold);
            this.buttonWanjiCpcStandard.ForeColor = System.Drawing.Color.White;
            this.buttonWanjiCpcStandard.Location = new System.Drawing.Point(484, 28);
            this.buttonWanjiCpcStandard.Margin = new System.Windows.Forms.Padding(10);
            this.buttonWanjiCpcStandard.Name = "buttonWanjiCpcStandard";
            this.buttonWanjiCpcStandard.Size = new System.Drawing.Size(208, 567);
            this.buttonWanjiCpcStandard.TabIndex = 2;
            this.buttonWanjiCpcStandard.Tag = "万集 CPC 卡标准测试";
            this.buttonWanjiCpcStandard.Text = "万集 CPC 卡\r\n标准测试";
            this.buttonWanjiCpcStandard.UseVisualStyleBackColor = false;
            this.buttonWanjiCpcStandard.Click += new System.EventHandler(this.buttonWanjiCpcStandard_Click);
            // 
            // buttonWatchmanBroadcast
            // 
            this.buttonWatchmanBroadcast.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(151)))), ((int)(((byte)(199)))));
            this.buttonWatchmanBroadcast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonWatchmanBroadcast.FlatAppearance.BorderSize = 0;
            this.buttonWatchmanBroadcast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonWatchmanBroadcast.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold);
            this.buttonWatchmanBroadcast.ForeColor = System.Drawing.Color.White;
            this.buttonWatchmanBroadcast.Location = new System.Drawing.Point(712, 28);
            this.buttonWatchmanBroadcast.Margin = new System.Windows.Forms.Padding(10);
            this.buttonWatchmanBroadcast.Name = "buttonWatchmanBroadcast";
            this.buttonWatchmanBroadcast.Size = new System.Drawing.Size(210, 567);
            this.buttonWatchmanBroadcast.TabIndex = 3;
            this.buttonWatchmanBroadcast.Tag = "守望者播报测试";
            this.buttonWatchmanBroadcast.Text = "守望者\r\n播报测试";
            this.buttonWatchmanBroadcast.UseVisualStyleBackColor = false;
            this.buttonWatchmanBroadcast.Click += new System.EventHandler(this.buttonWatchmanBroadcast_Click);
            // 
            // tabPageDailyUse
            // 
            this.tabPageDailyUse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.tabPageDailyUse.Controls.Add(this.labelDailyPlaceholder);
            this.tabPageDailyUse.Location = new System.Drawing.Point(4, 26);
            this.tabPageDailyUse.Name = "tabPageDailyUse";
            this.tabPageDailyUse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDailyUse.Size = new System.Drawing.Size(956, 629);
            this.tabPageDailyUse.TabIndex = 2;
            this.tabPageDailyUse.Text = "日常使用";
            // 
            // labelDailyPlaceholder
            // 
            this.labelDailyPlaceholder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDailyPlaceholder.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Bold);
            this.labelDailyPlaceholder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(99)))), ((int)(((byte)(117)))));
            this.labelDailyPlaceholder.Location = new System.Drawing.Point(3, 3);
            this.labelDailyPlaceholder.Name = "labelDailyPlaceholder";
            this.labelDailyPlaceholder.Size = new System.Drawing.Size(950, 623);
            this.labelDailyPlaceholder.TabIndex = 0;
            this.labelDailyPlaceholder.Text = "日常使用功能区待后续实现";
            this.labelDailyPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timerPortMonitor
            // 
            this.timerPortMonitor.Enabled = true;
            this.timerPortMonitor.Interval = 1000;
            this.timerPortMonitor.Tick += new System.EventHandler(this.timerPortMonitor_Tick);
            // 
            // statusStripMain
            // 
            this.statusStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStripMain.Location = new System.Drawing.Point(0, 777);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(1280, 22);
            this.statusStripMain.TabIndex = 1;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(68, 17);
            this.toolStripStatusLabel.Text = "状态：就绪";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1280, 799);
            this.Controls.Add(this.tabControlProducts);
            this.Controls.Add(this.statusStripMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1080, 720);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WJ_DSRCProtocolTest_Desk_Net_20260920";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tableLayoutRoot.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.tabPageCreateCardProduct.ResumeLayout(false);
            this.tabPageObuSerialProduct.ResumeLayout(false);
            this.tabPageDsrcProduct.ResumeLayout(false);
            this.tabControlProducts.ResumeLayout(false);
            this.panelStationChannel.ResumeLayout(false);
            this.tableLayoutStationChannel.ResumeLayout(false);
            this.tableLayoutStationChannel.PerformLayout();
            this.groupBoxCommunication.ResumeLayout(false);
            this.tableLayoutCommunication.ResumeLayout(false);
            this.groupBoxStationParameters.ResumeLayout(false);
            this.tableLayoutParameters.ResumeLayout(false);
            this.tableLayoutParameters.PerformLayout();
            this.flowLayoutOptions.ResumeLayout(false);
            this.flowLayoutOptions.PerformLayout();
            this.groupBoxDeviceStatus.ResumeLayout(false);
            this.panelWorkspace.ResumeLayout(false);
            this.tabControlModules.ResumeLayout(false);
            this.tabPageTransparentMode.ResumeLayout(false);
            this.tabPageWanjiStandard.ResumeLayout(false);
            this.tableLayoutWanjiCategories.ResumeLayout(false);
            this.tabPageDailyUse.ResumeLayout(false);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
