namespace WJ_ComprehensiveTest_Desk
{
    partial class WanjiStandardTestForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRoot;
        private System.Windows.Forms.Label labelCategoryTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutContent;
        private System.Windows.Forms.GroupBox groupBoxTestSelection;
        private System.Windows.Forms.TreeView treeViewTestItems;
        private System.Windows.Forms.GroupBox groupBoxOperations;
        private System.Windows.Forms.CheckBox checkBoxStoreTestResult;
        private System.Windows.Forms.CheckBox checkBoxTradeCheckEsam;
        private System.Windows.Forms.GroupBox groupBoxAlgorithm;
        private System.Windows.Forms.RadioButton radioButton3Sde;
        private System.Windows.Forms.RadioButton radioButtonSm4;
        private System.Windows.Forms.Button buttonSingleTest;
        private System.Windows.Forms.Button buttonAllTests;
        private System.Windows.Forms.Button buttonStopTest;
        private System.Windows.Forms.Button buttonClearDisplay;
        private System.Windows.Forms.GroupBox groupBoxTestInformation;
        private System.Windows.Forms.RichTextBox richTextBoxTestInformation;

        /// <summary>
        /// 释放测试项窗体持有的 Designer 组件资源。
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
        /// 初始化万集 OBU 标准测试窗口的标题、测试项选择区、操作区和测试信息区。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("0、OBU版本号读取");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("1、BST兼容性测试");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("测试用例1：无间隔发送BST");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("2、VST随机避让测试", new System.Windows.Forms.TreeNode[] {
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("3、防碰撞测试");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("测试用例1：预读0019文件、0002文件");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("测试用例2：预读0015文件");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("测试用例3：预读0012文件");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("4、预读信息测试", new System.Windows.Forms.TreeNode[] {
            treeNode6,
            treeNode7,
            treeNode8});
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("测试用例1：0信道读取文件");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("测试用例2：1信道读取文件");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("5、OBU信道选择测试", new System.Windows.Forms.TreeNode[] {
            treeNode10,
            treeNode11});
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("测试用例1：无访问许可，有加密秘钥标识");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("测试用例2：无访问许可，无加密秘钥标识");
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("测试用例3：有访问许可，有加密秘钥标识");
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("测试用例4：有访问许可，无加密秘钥标识");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("6、Getsecure测试", new System.Windows.Forms.TreeNode[] {
            treeNode13,
            treeNode14,
            treeNode15,
            treeNode16});
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("测试用例1：1条APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode19 = new System.Windows.Forms.TreeNode("测试用例2：2条相同APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("测试用例3：3条相同APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("测试用例4：4条相同APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("测试用例5：5条相同APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("测试用例6：6条相同APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("测试用例7：7条相同APDU指令读余额");
            System.Windows.Forms.TreeNode treeNode25 = new System.Windows.Forms.TreeNode("测试用例8：1条APDU指令操作ESAM取4字节随机数");
            System.Windows.Forms.TreeNode treeNode26 = new System.Windows.Forms.TreeNode("测试用例9：2条APDU指令操作ESAM取4字节随机数");
            System.Windows.Forms.TreeNode treeNode27 = new System.Windows.Forms.TreeNode("测试用例10：3条APDU指令操作ESAM取4字节随机数");
            System.Windows.Forms.TreeNode treeNode28 = new System.Windows.Forms.TreeNode("测试用例11：3条不相同的IC通道APDU指令操作读余额、读0015文件、读0012文件");
            System.Windows.Forms.TreeNode treeNode29 = new System.Windows.Forms.TreeNode("测试用例12：3条不相同的ESAM通道APDU指令操作进3F00目录、读EF01文件、取芯片序列号");
            System.Windows.Forms.TreeNode treeNode30 = new System.Windows.Forms.TreeNode("7、transferchannel测试", new System.Windows.Forms.TreeNode[] {
            treeNode18,
            treeNode19,
            treeNode20,
            treeNode21,
            treeNode22,
            treeNode23,
            treeNode24,
            treeNode25,
            treeNode26,
            treeNode27,
            treeNode28,
            treeNode29});
            System.Windows.Forms.TreeNode treeNode31 = new System.Windows.Forms.TreeNode("测试用例1：参数00");
            System.Windows.Forms.TreeNode treeNode32 = new System.Windows.Forms.TreeNode("测试用例2：参数01");
            System.Windows.Forms.TreeNode treeNode33 = new System.Windows.Forms.TreeNode("测试用例3：参数02");
            System.Windows.Forms.TreeNode treeNode34 = new System.Windows.Forms.TreeNode("测试用例4：参数03");
            System.Windows.Forms.TreeNode treeNode35 = new System.Windows.Forms.TreeNode("测试用例5：参数04");
            System.Windows.Forms.TreeNode treeNode36 = new System.Windows.Forms.TreeNode("8、SetMMI测试", new System.Windows.Forms.TreeNode[] {
            treeNode31,
            treeNode32,
            treeNode33,
            treeNode34,
            treeNode35});
            System.Windows.Forms.TreeNode treeNode37 = new System.Windows.Forms.TreeNode("9、拼帧指令测试");
            System.Windows.Forms.TreeNode treeNode38 = new System.Windows.Forms.TreeNode("10、255S保持测试");
            System.Windows.Forms.TreeNode treeNode39 = new System.Windows.Forms.TreeNode("用例1：获取车辆信息后发送BST");
            System.Windows.Forms.TreeNode treeNode40 = new System.Windows.Forms.TreeNode("用例2：消费初始化后发送BST");
            System.Windows.Forms.TreeNode treeNode41 = new System.Windows.Forms.TreeNode("11、交易过程中响应广播帧测试", new System.Windows.Forms.TreeNode[] {
            treeNode39,
            treeNode40});
            System.Windows.Forms.TreeNode treeNode42 = new System.Windows.Forms.TreeNode("用例1：获取车辆信息后发送其他标签MAC的transferChannel");
            System.Windows.Forms.TreeNode treeNode43 = new System.Windows.Forms.TreeNode("用例2：读取车辆信息之后发送全ff的transferChannel");
            System.Windows.Forms.TreeNode treeNode44 = new System.Windows.Forms.TreeNode("用例3：读取车辆信息之后发送其他MAC的transferChannel");
            System.Windows.Forms.TreeNode treeNode45 = new System.Windows.Forms.TreeNode("12、交易过程中响应不同MAC测试", new System.Windows.Forms.TreeNode[] {
            treeNode42,
            treeNode43,
            treeNode44});
            System.Windows.Forms.TreeNode treeNode46 = new System.Windows.Forms.TreeNode("13、地标交易测试");
            System.Windows.Forms.TreeNode treeNode47 = new System.Windows.Forms.TreeNode("测试用例1：取4字节随机数");
            System.Windows.Forms.TreeNode treeNode48 = new System.Windows.Forms.TreeNode("测试用例2：取8字节随机数");
            System.Windows.Forms.TreeNode treeNode49 = new System.Windows.Forms.TreeNode("测试用例3：取16字节随机数");
            System.Windows.Forms.TreeNode treeNode50 = new System.Windows.Forms.TreeNode("测试用例4：取9字节随机数");
            System.Windows.Forms.TreeNode treeNode51 = new System.Windows.Forms.TreeNode("测试用例5：发送随机数p1参数不正确");
            System.Windows.Forms.TreeNode treeNode52 = new System.Windows.Forms.TreeNode("测试用例6：发送随机数CLA参数不正确");
            System.Windows.Forms.TreeNode treeNode53 = new System.Windows.Forms.TreeNode("测试用例7：发送随机数INS参数不正确");
            System.Windows.Forms.TreeNode treeNode54 = new System.Windows.Forms.TreeNode("测试用例8：取4字节芯片序列号");
            System.Windows.Forms.TreeNode treeNode55 = new System.Windows.Forms.TreeNode("测试用例9：发送取芯片序列号指令p1参数不正确");
            System.Windows.Forms.TreeNode treeNode56 = new System.Windows.Forms.TreeNode("测试用例10：发送取芯片序列号指令长度不正确");
            System.Windows.Forms.TreeNode treeNode57 = new System.Windows.Forms.TreeNode("测试用例11：取芯片序列号INS参数不正确");
            System.Windows.Forms.TreeNode treeNode58 = new System.Windows.Forms.TreeNode("测试用例12：取芯片序列号CLA参数不正确");
            System.Windows.Forms.TreeNode treeNode59 = new System.Windows.Forms.TreeNode("测试用例13：选择EF04文件");
            System.Windows.Forms.TreeNode treeNode60 = new System.Windows.Forms.TreeNode("测试用例14：选择EF04文件参数不正确");
            System.Windows.Forms.TreeNode treeNode61 = new System.Windows.Forms.TreeNode("测试用例15：选择EF04文件CLA不正确");
            System.Windows.Forms.TreeNode treeNode62 = new System.Windows.Forms.TreeNode("测试用例16：读取0015文件测试");
            System.Windows.Forms.TreeNode treeNode63 = new System.Windows.Forms.TreeNode("测试用例17：读取001A文件");
            System.Windows.Forms.TreeNode treeNode64 = new System.Windows.Forms.TreeNode("测试用例19：取响应代码返回6982");
            System.Windows.Forms.TreeNode treeNode65 = new System.Windows.Forms.TreeNode("测试用例20：响应代码6981不支持安全报文");
            System.Windows.Forms.TreeNode treeNode66 = new System.Windows.Forms.TreeNode("测试用例21：响应代码6A83未找到记录");
            System.Windows.Forms.TreeNode treeNode67 = new System.Windows.Forms.TreeNode("测试用例22：响应代码6984引用数据无效");
            System.Windows.Forms.TreeNode treeNode68 = new System.Windows.Forms.TreeNode("测试用例23：响应代码6984未申请随机数");
            System.Windows.Forms.TreeNode treeNode69 = new System.Windows.Forms.TreeNode("测试用例24：响应代码6986不满足命令执行");
            System.Windows.Forms.TreeNode treeNode70 = new System.Windows.Forms.TreeNode("测试用例25：响应代码6988安全报文数据项不正确");
            System.Windows.Forms.TreeNode treeNode71 = new System.Windows.Forms.TreeNode("测试用例26：响应代码6B00参数不正确，偏移地址超出EF");
            System.Windows.Forms.TreeNode treeNode72 = new System.Windows.Forms.TreeNode("测试用例27：响应代码6F00判断不准确");
            System.Windows.Forms.TreeNode treeNode73 = new System.Windows.Forms.TreeNode("测试用例28：响应代码6985不满足引用条件");
            System.Windows.Forms.TreeNode treeNode74 = new System.Windows.Forms.TreeNode("14、指令集测试", new System.Windows.Forms.TreeNode[] {
            treeNode47,
            treeNode48,
            treeNode49,
            treeNode50,
            treeNode51,
            treeNode52,
            treeNode53,
            treeNode54,
            treeNode55,
            treeNode56,
            treeNode57,
            treeNode58,
            treeNode59,
            treeNode60,
            treeNode61,
            treeNode62,
            treeNode63,
            treeNode64,
            treeNode65,
            treeNode66,
            treeNode67,
            treeNode68,
            treeNode69,
            treeNode70,
            treeNode71,
            treeNode72,
            treeNode73});
            System.Windows.Forms.TreeNode treeNode75 = new System.Windows.Forms.TreeNode("15、保留文件读写测试");
            System.Windows.Forms.TreeNode treeNode76 = new System.Windows.Forms.TreeNode("16、单帧唤醒测试");
            System.Windows.Forms.TreeNode treeNode77 = new System.Windows.Forms.TreeNode("17、模拟真实交易流测试");
            System.Windows.Forms.TreeNode treeNode78 = new System.Windows.Forms.TreeNode("测试用例1：5.8G修改MACID");
            System.Windows.Forms.TreeNode treeNode79 = new System.Windows.Forms.TreeNode("测试用例2：5.8G修改SN号");
            System.Windows.Forms.TreeNode treeNode80 = new System.Windows.Forms.TreeNode("测试用例3：5.8G读取蓝牙地址");
            System.Windows.Forms.TreeNode treeNode81 = new System.Windows.Forms.TreeNode("测试用例4：5.8G读写地区");
            System.Windows.Forms.TreeNode treeNode82 = new System.Windows.Forms.TreeNode("测试用例5：不同通道验证测试");
            System.Windows.Forms.TreeNode treeNode83 = new System.Windows.Forms.TreeNode("测试用例6：射频发数测试（55AA）");
            System.Windows.Forms.TreeNode treeNode84 = new System.Windows.Forms.TreeNode("18、万集自有协议测试", new System.Windows.Forms.TreeNode[] {
            treeNode78,
            treeNode79,
            treeNode80,
            treeNode81,
            treeNode82,
            treeNode83});
            System.Windows.Forms.TreeNode treeNode85 = new System.Windows.Forms.TreeNode("19、播报金额测试（语音款）");
            System.Windows.Forms.TreeNode treeNode86 = new System.Windows.Forms.TreeNode("20、拼帧交易测试（语音款）");
            System.Windows.Forms.TreeNode treeNode87 = new System.Windows.Forms.TreeNode("21、交易后pin认证异常播报测试（语音款）");
            System.Windows.Forms.TreeNode treeNode88 = new System.Windows.Forms.TreeNode("22、交易25次后防拆2S失效测试");
            System.Windows.Forms.TreeNode treeNode89 = new System.Windows.Forms.TreeNode("23、VST回复EquipmentStatus测试");
            System.Windows.Forms.TreeNode treeNode90 = new System.Windows.Forms.TreeNode("通用测试项", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode4,
            treeNode5,
            treeNode9,
            treeNode12,
            treeNode17,
            treeNode30,
            treeNode36,
            treeNode37,
            treeNode38,
            treeNode41,
            treeNode45,
            treeNode46,
            treeNode74,
            treeNode75,
            treeNode76,
            treeNode77,
            treeNode84,
            treeNode85,
            treeNode86,
            treeNode87,
            treeNode88,
            treeNode89});
            System.Windows.Forms.TreeNode treeNode91 = new System.Windows.Forms.TreeNode("1、双通道圈存测试");
            System.Windows.Forms.TreeNode treeNode92 = new System.Windows.Forms.TreeNode("测试用例1：ESAM读取EF01文件，ICC读取0016文件");
            System.Windows.Forms.TreeNode treeNode93 = new System.Windows.Forms.TreeNode("测试用例2：ICC读取0016文件，ESAM读取EF04文件");
            System.Windows.Forms.TreeNode treeNode94 = new System.Windows.Forms.TreeNode("测试用例3：ESAM读取EF04文件，ICC读取0015文件");
            System.Windows.Forms.TreeNode treeNode95 = new System.Windows.Forms.TreeNode("测试用例4：ICC读取0015文件，ESAM读取EF01文件");
            System.Windows.Forms.TreeNode treeNode96 = new System.Windows.Forms.TreeNode("2、双通道切换测试", new System.Windows.Forms.TreeNode[] {
            treeNode92,
            treeNode93,
            treeNode94,
            treeNode95});
            System.Windows.Forms.TreeNode treeNode97 = new System.Windows.Forms.TreeNode("用例1：一次发行测试（单片）");
            System.Windows.Forms.TreeNode treeNode98 = new System.Windows.Forms.TreeNode("用例2：二次发行测试（单片）");
            System.Windows.Forms.TreeNode treeNode99 = new System.Windows.Forms.TreeNode("用例3：标签激活测试（单片）");
            System.Windows.Forms.TreeNode treeNode100 = new System.Windows.Forms.TreeNode("3、发行与激活测试", new System.Windows.Forms.TreeNode[] {
            treeNode97,
            treeNode98,
            treeNode99});
            System.Windows.Forms.TreeNode treeNode101 = new System.Windows.Forms.TreeNode("单片式专用测试项", new System.Windows.Forms.TreeNode[] {
            treeNode91,
            treeNode96,
            treeNode100});
            System.Windows.Forms.TreeNode treeNode102 = new System.Windows.Forms.TreeNode("用例1：一次发行测试（双片）");
            System.Windows.Forms.TreeNode treeNode103 = new System.Windows.Forms.TreeNode("用例2：二次发行测试（双片）");
            System.Windows.Forms.TreeNode treeNode104 = new System.Windows.Forms.TreeNode("用例3：标签激活测试（双片）");
            System.Windows.Forms.TreeNode treeNode105 = new System.Windows.Forms.TreeNode("1、发行与激活测试", new System.Windows.Forms.TreeNode[] {
            treeNode102,
            treeNode103,
            treeNode104});
            System.Windows.Forms.TreeNode treeNode106 = new System.Windows.Forms.TreeNode("用例1：不插卡片通过门架");
            System.Windows.Forms.TreeNode treeNode107 = new System.Windows.Forms.TreeNode("用例2：低电模式通过门架");
            System.Windows.Forms.TreeNode treeNode108 = new System.Windows.Forms.TreeNode("用例3：天线给OBU设置不回IC卡状态通过门架");
            System.Windows.Forms.TreeNode treeNode109 = new System.Windows.Forms.TreeNode("2、无卡机制测试", new System.Windows.Forms.TreeNode[] {
            treeNode106,
            treeNode107,
            treeNode108});
            System.Windows.Forms.TreeNode treeNode110 = new System.Windows.Forms.TreeNode("双片式专用测试项", new System.Windows.Forms.TreeNode[] {
            treeNode105,
            treeNode109});
            System.Windows.Forms.TreeNode treeNode111 = new System.Windows.Forms.TreeNode("1、ESAM指令检测（广东）");
            System.Windows.Forms.TreeNode treeNode112 = new System.Windows.Forms.TreeNode("地区专用测试项", new System.Windows.Forms.TreeNode[] {
            treeNode111});
            System.Windows.Forms.TreeNode treeNode113 = new System.Windows.Forms.TreeNode("1、车道交易测试");
            System.Windows.Forms.TreeNode treeNode114 = new System.Windows.Forms.TreeNode("2、门架交易测试");
            System.Windows.Forms.TreeNode treeNode115 = new System.Windows.Forms.TreeNode("3、典型交易测试");
            System.Windows.Forms.TreeNode treeNode116 = new System.Windows.Forms.TreeNode("备用测试项", new System.Windows.Forms.TreeNode[] {
            treeNode113,
            treeNode114,
            treeNode115});
            this.tableLayoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.labelCategoryTitle = new System.Windows.Forms.Label();
            this.tableLayoutContent = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxTestSelection = new System.Windows.Forms.GroupBox();
            this.treeViewTestItems = new System.Windows.Forms.TreeView();
            this.groupBoxOperations = new System.Windows.Forms.GroupBox();
            this.buttonClearDisplay = new System.Windows.Forms.Button();
            this.buttonStopTest = new System.Windows.Forms.Button();
            this.buttonAllTests = new System.Windows.Forms.Button();
            this.buttonSingleTest = new System.Windows.Forms.Button();
            this.groupBoxAlgorithm = new System.Windows.Forms.GroupBox();
            this.radioButton3Sde = new System.Windows.Forms.RadioButton();
            this.radioButtonSm4 = new System.Windows.Forms.RadioButton();
            this.checkBoxTradeCheckEsam = new System.Windows.Forms.CheckBox();
            this.checkBoxStoreTestResult = new System.Windows.Forms.CheckBox();
            this.groupBoxTestInformation = new System.Windows.Forms.GroupBox();
            this.richTextBoxTestInformation = new System.Windows.Forms.RichTextBox();
            this.tableLayoutRoot.SuspendLayout();
            this.tableLayoutContent.SuspendLayout();
            this.groupBoxTestSelection.SuspendLayout();
            this.groupBoxOperations.SuspendLayout();
            this.groupBoxAlgorithm.SuspendLayout();
            this.groupBoxTestInformation.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutRoot
            // 
            this.tableLayoutRoot.ColumnCount = 1;
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Controls.Add(this.labelCategoryTitle, 0, 0);
            this.tableLayoutRoot.Controls.Add(this.tableLayoutContent, 0, 1);
            this.tableLayoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRoot.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutRoot.Name = "tableLayoutRoot";
            this.tableLayoutRoot.Padding = new System.Windows.Forms.Padding(20);
            this.tableLayoutRoot.RowCount = 2;
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Size = new System.Drawing.Size(1200, 760);
            this.tableLayoutRoot.TabIndex = 0;
            // 
            // labelCategoryTitle
            // 
            this.labelCategoryTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(49)))), ((int)(((byte)(78)))));
            this.labelCategoryTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCategoryTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.labelCategoryTitle.ForeColor = System.Drawing.Color.White;
            this.labelCategoryTitle.Location = new System.Drawing.Point(23, 20);
            this.labelCategoryTitle.Name = "labelCategoryTitle";
            this.labelCategoryTitle.Padding = new System.Windows.Forms.Padding(18, 0, 0, 0);
            this.labelCategoryTitle.Size = new System.Drawing.Size(1154, 68);
            this.labelCategoryTitle.TabIndex = 0;
            this.labelCategoryTitle.Text = "万集 OBU 标准测试";
            this.labelCategoryTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutContent
            // 
            this.tableLayoutContent.ColumnCount = 3;
            this.tableLayoutContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tableLayoutContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46F));
            this.tableLayoutContent.Controls.Add(this.groupBoxTestSelection, 0, 0);
            this.tableLayoutContent.Controls.Add(this.groupBoxOperations, 1, 0);
            this.tableLayoutContent.Controls.Add(this.groupBoxTestInformation, 2, 0);
            this.tableLayoutContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutContent.Location = new System.Drawing.Point(20, 102);
            this.tableLayoutContent.Margin = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.tableLayoutContent.Name = "tableLayoutContent";
            this.tableLayoutContent.RowCount = 1;
            this.tableLayoutContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutContent.Size = new System.Drawing.Size(1160, 638);
            this.tableLayoutContent.TabIndex = 1;
            // 
            // groupBoxTestSelection
            // 
            this.groupBoxTestSelection.Controls.Add(this.treeViewTestItems);
            this.groupBoxTestSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTestSelection.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxTestSelection.Location = new System.Drawing.Point(0, 0);
            this.groupBoxTestSelection.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.groupBoxTestSelection.Name = "groupBoxTestSelection";
            this.groupBoxTestSelection.Padding = new System.Windows.Forms.Padding(12);
            this.groupBoxTestSelection.Size = new System.Drawing.Size(479, 638);
            this.groupBoxTestSelection.TabIndex = 0;
            this.groupBoxTestSelection.TabStop = false;
            this.groupBoxTestSelection.Text = "测试项选择";
            // 
            // treeViewTestItems
            // 
            this.treeViewTestItems.CheckBoxes = true;
            this.treeViewTestItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewTestItems.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.treeViewTestItems.HideSelection = false;
            this.treeViewTestItems.Location = new System.Drawing.Point(12, 38);
            this.treeViewTestItems.Name = "treeViewTestItems";
            treeNode1.Name = "";
            treeNode1.Text = "0、OBU版本号读取";
            treeNode2.Name = "";
            treeNode2.Text = "1、BST兼容性测试";
            treeNode3.Name = "";
            treeNode3.Text = "测试用例1：无间隔发送BST";
            treeNode4.Name = "";
            treeNode4.Text = "2、VST随机避让测试";
            treeNode5.Name = "";
            treeNode5.Text = "3、防碰撞测试";
            treeNode6.Name = "";
            treeNode6.Text = "测试用例1：预读0019文件、0002文件";
            treeNode7.Name = "";
            treeNode7.Text = "测试用例2：预读0015文件";
            treeNode8.Name = "";
            treeNode8.Text = "测试用例3：预读0012文件";
            treeNode9.Name = "";
            treeNode9.Text = "4、预读信息测试";
            treeNode10.Name = "";
            treeNode10.Text = "测试用例1：0信道读取文件";
            treeNode11.Name = "";
            treeNode11.Text = "测试用例2：1信道读取文件";
            treeNode12.Name = "";
            treeNode12.Text = "5、OBU信道选择测试";
            treeNode13.Name = "";
            treeNode13.Text = "测试用例1：无访问许可，有加密秘钥标识";
            treeNode14.Name = "";
            treeNode14.Text = "测试用例2：无访问许可，无加密秘钥标识";
            treeNode15.Name = "";
            treeNode15.Text = "测试用例3：有访问许可，有加密秘钥标识";
            treeNode16.Name = "";
            treeNode16.Text = "测试用例4：有访问许可，无加密秘钥标识";
            treeNode17.Name = "";
            treeNode17.Text = "6、Getsecure测试";
            treeNode18.Name = "";
            treeNode18.Text = "测试用例1：1条APDU指令读余额";
            treeNode19.Name = "";
            treeNode19.Text = "测试用例2：2条相同APDU指令读余额";
            treeNode20.Name = "";
            treeNode20.Text = "测试用例3：3条相同APDU指令读余额";
            treeNode21.Name = "";
            treeNode21.Text = "测试用例4：4条相同APDU指令读余额";
            treeNode22.Name = "";
            treeNode22.Text = "测试用例5：5条相同APDU指令读余额";
            treeNode23.Name = "";
            treeNode23.Text = "测试用例6：6条相同APDU指令读余额";
            treeNode24.Name = "";
            treeNode24.Text = "测试用例7：7条相同APDU指令读余额";
            treeNode25.Name = "";
            treeNode25.Text = "测试用例8：1条APDU指令操作ESAM取4字节随机数";
            treeNode26.Name = "";
            treeNode26.Text = "测试用例9：2条APDU指令操作ESAM取4字节随机数";
            treeNode27.Name = "";
            treeNode27.Text = "测试用例10：3条APDU指令操作ESAM取4字节随机数";
            treeNode28.Name = "";
            treeNode28.Text = "测试用例11：3条不相同的IC通道APDU指令操作读余额、读0015文件、读0012文件";
            treeNode29.Name = "";
            treeNode29.Text = "测试用例12：3条不相同的ESAM通道APDU指令操作进3F00目录、读EF01文件、取芯片序列号";
            treeNode30.Name = "";
            treeNode30.Text = "7、transferchannel测试";
            treeNode31.Name = "";
            treeNode31.Text = "测试用例1：参数00";
            treeNode32.Name = "";
            treeNode32.Text = "测试用例2：参数01";
            treeNode33.Name = "";
            treeNode33.Text = "测试用例3：参数02";
            treeNode34.Name = "";
            treeNode34.Text = "测试用例4：参数03";
            treeNode35.Name = "";
            treeNode35.Text = "测试用例5：参数04";
            treeNode36.Name = "";
            treeNode36.Text = "8、SetMMI测试";
            treeNode37.Name = "";
            treeNode37.Text = "9、拼帧指令测试";
            treeNode38.Name = "";
            treeNode38.Text = "10、255S保持测试";
            treeNode39.Name = "";
            treeNode39.Text = "用例1：获取车辆信息后发送BST";
            treeNode40.Name = "";
            treeNode40.Text = "用例2：消费初始化后发送BST";
            treeNode41.Name = "";
            treeNode41.Text = "11、交易过程中响应广播帧测试";
            treeNode42.Name = "";
            treeNode42.Text = "用例1：获取车辆信息后发送其他标签MAC的transferChannel";
            treeNode43.Name = "";
            treeNode43.Text = "用例2：读取车辆信息之后发送全ff的transferChannel";
            treeNode44.Name = "";
            treeNode44.Text = "用例3：读取车辆信息之后发送其他MAC的transferChannel";
            treeNode45.Name = "";
            treeNode45.Text = "12、交易过程中响应不同MAC测试";
            treeNode46.Name = "";
            treeNode46.Text = "13、地标交易测试";
            treeNode47.Name = "";
            treeNode47.Text = "测试用例1：取4字节随机数";
            treeNode48.Name = "";
            treeNode48.Text = "测试用例2：取8字节随机数";
            treeNode49.Name = "";
            treeNode49.Text = "测试用例3：取16字节随机数";
            treeNode50.Name = "";
            treeNode50.Text = "测试用例4：取9字节随机数";
            treeNode51.Name = "";
            treeNode51.Text = "测试用例5：发送随机数p1参数不正确";
            treeNode52.Name = "";
            treeNode52.Text = "测试用例6：发送随机数CLA参数不正确";
            treeNode53.Name = "";
            treeNode53.Text = "测试用例7：发送随机数INS参数不正确";
            treeNode54.Name = "";
            treeNode54.Text = "测试用例8：取4字节芯片序列号";
            treeNode55.Name = "";
            treeNode55.Text = "测试用例9：发送取芯片序列号指令p1参数不正确";
            treeNode56.Name = "";
            treeNode56.Text = "测试用例10：发送取芯片序列号指令长度不正确";
            treeNode57.Name = "";
            treeNode57.Text = "测试用例11：取芯片序列号INS参数不正确";
            treeNode58.Name = "";
            treeNode58.Text = "测试用例12：取芯片序列号CLA参数不正确";
            treeNode59.Name = "";
            treeNode59.Text = "测试用例13：选择EF04文件";
            treeNode60.Name = "";
            treeNode60.Text = "测试用例14：选择EF04文件参数不正确";
            treeNode61.Name = "";
            treeNode61.Text = "测试用例15：选择EF04文件CLA不正确";
            treeNode62.Name = "";
            treeNode62.Text = "测试用例16：读取0015文件测试";
            treeNode63.Name = "";
            treeNode63.Text = "测试用例17：读取001A文件";
            treeNode64.Name = "";
            treeNode64.Text = "测试用例19：取响应代码返回6982";
            treeNode65.Name = "";
            treeNode65.Text = "测试用例20：响应代码6981不支持安全报文";
            treeNode66.Name = "";
            treeNode66.Text = "测试用例21：响应代码6A83未找到记录";
            treeNode67.Name = "";
            treeNode67.Text = "测试用例22：响应代码6984引用数据无效";
            treeNode68.Name = "";
            treeNode68.Text = "测试用例23：响应代码6984未申请随机数";
            treeNode69.Name = "";
            treeNode69.Text = "测试用例24：响应代码6986不满足命令执行";
            treeNode70.Name = "";
            treeNode70.Text = "测试用例25：响应代码6988安全报文数据项不正确";
            treeNode71.Name = "";
            treeNode71.Text = "测试用例26：响应代码6B00参数不正确，偏移地址超出EF";
            treeNode72.Name = "";
            treeNode72.Text = "测试用例27：响应代码6F00判断不准确";
            treeNode73.Name = "";
            treeNode73.Text = "测试用例28：响应代码6985不满足引用条件";
            treeNode74.Name = "";
            treeNode74.Text = "14、指令集测试";
            treeNode75.Name = "";
            treeNode75.Text = "15、保留文件读写测试";
            treeNode76.Name = "";
            treeNode76.Text = "16、单帧唤醒测试";
            treeNode77.Name = "";
            treeNode77.Text = "17、模拟真实交易流测试";
            treeNode78.Name = "";
            treeNode78.Text = "测试用例1：5.8G修改MACID";
            treeNode79.Name = "";
            treeNode79.Text = "测试用例2：5.8G修改SN号";
            treeNode80.Name = "";
            treeNode80.Text = "测试用例3：5.8G读取蓝牙地址";
            treeNode81.Name = "";
            treeNode81.Text = "测试用例4：5.8G读写地区";
            treeNode82.Name = "";
            treeNode82.Text = "测试用例5：不同通道验证测试";
            treeNode83.Name = "";
            treeNode83.Text = "测试用例6：射频发数测试（55AA）";
            treeNode84.Name = "";
            treeNode84.Text = "18、万集自有协议测试";
            treeNode85.Name = "";
            treeNode85.Text = "19、播报金额测试（语音款）";
            treeNode86.Name = "";
            treeNode86.Text = "20、拼帧交易测试（语音款）";
            treeNode87.Name = "";
            treeNode87.Text = "21、交易后pin认证异常播报测试（语音款）";
            treeNode88.Name = "";
            treeNode88.Text = "22、交易25次后防拆2S失效测试";
            treeNode89.Name = "";
            treeNode89.Text = "23、VST回复EquipmentStatus测试";
            treeNode90.Name = "";
            treeNode90.Text = "通用测试项";
            treeNode91.Name = "";
            treeNode91.Text = "1、双通道圈存测试";
            treeNode92.Name = "";
            treeNode92.Text = "测试用例1：ESAM读取EF01文件，ICC读取0016文件";
            treeNode93.Name = "";
            treeNode93.Text = "测试用例2：ICC读取0016文件，ESAM读取EF04文件";
            treeNode94.Name = "";
            treeNode94.Text = "测试用例3：ESAM读取EF04文件，ICC读取0015文件";
            treeNode95.Name = "";
            treeNode95.Text = "测试用例4：ICC读取0015文件，ESAM读取EF01文件";
            treeNode96.Name = "";
            treeNode96.Text = "2、双通道切换测试";
            treeNode97.Name = "";
            treeNode97.Text = "用例1：一次发行测试（单片）";
            treeNode98.Name = "";
            treeNode98.Text = "用例2：二次发行测试（单片）";
            treeNode99.Name = "";
            treeNode99.Text = "用例3：标签激活测试（单片）";
            treeNode100.Name = "";
            treeNode100.Text = "3、发行与激活测试";
            treeNode101.Name = "";
            treeNode101.Text = "单片式专用测试项";
            treeNode102.Name = "";
            treeNode102.Text = "用例1：一次发行测试（双片）";
            treeNode103.Name = "";
            treeNode103.Text = "用例2：二次发行测试（双片）";
            treeNode104.Name = "";
            treeNode104.Text = "用例3：标签激活测试（双片）";
            treeNode105.Name = "";
            treeNode105.Text = "1、发行与激活测试";
            treeNode106.Name = "";
            treeNode106.Text = "用例1：不插卡片通过门架";
            treeNode107.Name = "";
            treeNode107.Text = "用例2：低电模式通过门架";
            treeNode108.Name = "";
            treeNode108.Text = "用例3：天线给OBU设置不回IC卡状态通过门架";
            treeNode109.Name = "";
            treeNode109.Text = "2、无卡机制测试";
            treeNode110.Name = "";
            treeNode110.Text = "双片式专用测试项";
            treeNode111.Name = "";
            treeNode111.Text = "1、ESAM指令检测（广东）";
            treeNode112.Name = "";
            treeNode112.Text = "地区专用测试项";
            treeNode113.Name = "";
            treeNode113.Text = "1、车道交易测试";
            treeNode114.Name = "";
            treeNode114.Text = "2、门架交易测试";
            treeNode115.Name = "";
            treeNode115.Text = "3、典型交易测试";
            treeNode116.Name = "";
            treeNode116.Text = "备用测试项";
            this.treeViewTestItems.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode90,
            treeNode101,
            treeNode110,
            treeNode112,
            treeNode116});
            this.treeViewTestItems.Size = new System.Drawing.Size(455, 588);
            this.treeViewTestItems.TabIndex = 0;
            this.treeViewTestItems.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewTestItems_AfterCheck);
            this.treeViewTestItems.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewTestItems_NodeMouseDoubleClick);
            // 
            // groupBoxOperations
            // 
            this.groupBoxOperations.Controls.Add(this.buttonClearDisplay);
            this.groupBoxOperations.Controls.Add(this.buttonStopTest);
            this.groupBoxOperations.Controls.Add(this.buttonAllTests);
            this.groupBoxOperations.Controls.Add(this.buttonSingleTest);
            this.groupBoxOperations.Controls.Add(this.groupBoxAlgorithm);
            this.groupBoxOperations.Controls.Add(this.checkBoxTradeCheckEsam);
            this.groupBoxOperations.Controls.Add(this.checkBoxStoreTestResult);
            this.groupBoxOperations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOperations.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxOperations.Location = new System.Drawing.Point(487, 0);
            this.groupBoxOperations.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.groupBoxOperations.Name = "groupBoxOperations";
            this.groupBoxOperations.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxOperations.Size = new System.Drawing.Size(131, 638);
            this.groupBoxOperations.TabIndex = 1;
            this.groupBoxOperations.TabStop = false;
            this.groupBoxOperations.Text = "操作选项";
            // 
            // buttonClearDisplay
            // 
            this.buttonClearDisplay.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonClearDisplay.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonClearDisplay.Location = new System.Drawing.Point(8, 287);
            this.buttonClearDisplay.Name = "buttonClearDisplay";
            this.buttonClearDisplay.Size = new System.Drawing.Size(115, 38);
            this.buttonClearDisplay.TabIndex = 5;
            this.buttonClearDisplay.Text = "清空显示";
            this.buttonClearDisplay.UseVisualStyleBackColor = true;
            this.buttonClearDisplay.Click += new System.EventHandler(this.buttonClearDisplay_Click);
            // 
            // buttonStopTest
            // 
            this.buttonStopTest.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonStopTest.Enabled = false;
            this.buttonStopTest.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonStopTest.Location = new System.Drawing.Point(8, 249);
            this.buttonStopTest.Name = "buttonStopTest";
            this.buttonStopTest.Size = new System.Drawing.Size(115, 38);
            this.buttonStopTest.TabIndex = 4;
            this.buttonStopTest.Text = "停止测试";
            this.buttonStopTest.UseVisualStyleBackColor = true;
            this.buttonStopTest.Click += new System.EventHandler(this.buttonStopTest_Click);
            // 
            // buttonAllTests
            // 
            this.buttonAllTests.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonAllTests.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonAllTests.Location = new System.Drawing.Point(8, 211);
            this.buttonAllTests.Name = "buttonAllTests";
            this.buttonAllTests.Size = new System.Drawing.Size(115, 38);
            this.buttonAllTests.TabIndex = 3;
            this.buttonAllTests.Text = "全部测试";
            this.buttonAllTests.UseVisualStyleBackColor = true;
            // 
            // buttonSingleTest
            // 
            this.buttonSingleTest.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonSingleTest.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSingleTest.Location = new System.Drawing.Point(8, 173);
            this.buttonSingleTest.Name = "buttonSingleTest";
            this.buttonSingleTest.Size = new System.Drawing.Size(115, 38);
            this.buttonSingleTest.TabIndex = 2;
            this.buttonSingleTest.Text = "测试";
            this.buttonSingleTest.UseVisualStyleBackColor = true;
            this.buttonSingleTest.Click += new System.EventHandler(this.buttonSingleTest_Click);
            // 
            // groupBoxAlgorithm
            // 
            this.groupBoxAlgorithm.Controls.Add(this.radioButton3Sde);
            this.groupBoxAlgorithm.Controls.Add(this.radioButtonSm4);
            this.groupBoxAlgorithm.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxAlgorithm.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.groupBoxAlgorithm.Location = new System.Drawing.Point(8, 87);
            this.groupBoxAlgorithm.Name = "groupBoxAlgorithm";
            this.groupBoxAlgorithm.Padding = new System.Windows.Forms.Padding(8, 5, 5, 5);
            this.groupBoxAlgorithm.Size = new System.Drawing.Size(115, 86);
            this.groupBoxAlgorithm.TabIndex = 1;
            this.groupBoxAlgorithm.TabStop = false;
            this.groupBoxAlgorithm.Text = "算法选择";
            // 
            // radioButton3Sde
            // 
            this.radioButton3Sde.AutoSize = true;
            this.radioButton3Sde.Location = new System.Drawing.Point(10, 25);
            this.radioButton3Sde.Name = "radioButton3Sde";
            this.radioButton3Sde.Size = new System.Drawing.Size(78, 28);
            this.radioButton3Sde.TabIndex = 0;
            this.radioButton3Sde.TabStop = false;
            this.radioButton3Sde.Text = "3SDE";
            this.radioButton3Sde.UseVisualStyleBackColor = true;
            // 
            // radioButtonSm4
            // 
            this.radioButtonSm4.AutoSize = true;
            this.radioButtonSm4.Checked = true;
            this.radioButtonSm4.Location = new System.Drawing.Point(10, 52);
            this.radioButtonSm4.Name = "radioButtonSm4";
            this.radioButtonSm4.Size = new System.Drawing.Size(72, 28);
            this.radioButtonSm4.TabIndex = 1;
            this.radioButtonSm4.TabStop = true;
            this.radioButtonSm4.Text = "SM4";
            this.radioButtonSm4.UseVisualStyleBackColor = true;
            // 
            // checkBoxTradeCheckEsam
            // 
            this.checkBoxTradeCheckEsam.AutoSize = true;
            this.checkBoxTradeCheckEsam.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxTradeCheckEsam.Font = new System.Drawing.Font("Microsoft YaHei UI", 7F);
            this.checkBoxTradeCheckEsam.Location = new System.Drawing.Point(8, 59);
            this.checkBoxTradeCheckEsam.Name = "checkBoxTradeCheckEsam";
            this.checkBoxTradeCheckEsam.Padding = new System.Windows.Forms.Padding(2, 4, 0, 0);
            this.checkBoxTradeCheckEsam.Size = new System.Drawing.Size(115, 28);
            this.checkBoxTradeCheckEsam.TabIndex = 0;
            this.checkBoxTradeCheckEsam.Text = "交易_校验ESAM";
            this.checkBoxTradeCheckEsam.UseVisualStyleBackColor = true;
            // 
            // checkBoxStoreTestResult
            // 
            this.checkBoxStoreTestResult.AutoSize = true;
            this.checkBoxStoreTestResult.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxStoreTestResult.Font = new System.Drawing.Font("Microsoft YaHei UI", 7F);
            this.checkBoxStoreTestResult.Location = new System.Drawing.Point(8, 31);
            this.checkBoxStoreTestResult.Name = "checkBoxStoreTestResult";
            this.checkBoxStoreTestResult.Padding = new System.Windows.Forms.Padding(2, 4, 0, 0);
            this.checkBoxStoreTestResult.Size = new System.Drawing.Size(115, 28);
            this.checkBoxStoreTestResult.TabIndex = 0;
            this.checkBoxStoreTestResult.Text = "存储测试结果";
            this.checkBoxStoreTestResult.UseVisualStyleBackColor = true;
            // 
            // groupBoxTestInformation
            // 
            this.groupBoxTestInformation.Controls.Add(this.richTextBoxTestInformation);
            this.groupBoxTestInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTestInformation.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxTestInformation.Location = new System.Drawing.Point(629, 3);
            this.groupBoxTestInformation.Name = "groupBoxTestInformation";
            this.groupBoxTestInformation.Padding = new System.Windows.Forms.Padding(12);
            this.groupBoxTestInformation.Size = new System.Drawing.Size(528, 632);
            this.groupBoxTestInformation.TabIndex = 2;
            this.groupBoxTestInformation.TabStop = false;
            this.groupBoxTestInformation.Text = "测试信息显示";
            // 
            // richTextBoxTestInformation
            // 
            this.richTextBoxTestInformation.BackColor = System.Drawing.Color.White;
            this.richTextBoxTestInformation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxTestInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxTestInformation.Font = new System.Drawing.Font("Consolas", 10F);
            this.richTextBoxTestInformation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.richTextBoxTestInformation.Location = new System.Drawing.Point(12, 38);
            this.richTextBoxTestInformation.Name = "richTextBoxTestInformation";
            this.richTextBoxTestInformation.ReadOnly = true;
            this.richTextBoxTestInformation.Size = new System.Drawing.Size(504, 582);
            this.richTextBoxTestInformation.TabIndex = 0;
            this.richTextBoxTestInformation.Text = "测试信息将在执行测试后显示。";
            this.richTextBoxTestInformation.TextChanged += new System.EventHandler(this.richTextBoxTestInformation_TextChanged);
            // 
            // WanjiStandardTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1200, 760);
            this.Controls.Add(this.tableLayoutRoot);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.MinimumSize = new System.Drawing.Size(980, 620);
            this.Name = "WanjiStandardTestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "标准测试";
            this.tableLayoutRoot.ResumeLayout(false);
            this.tableLayoutContent.ResumeLayout(false);
            this.groupBoxTestSelection.ResumeLayout(false);
            this.groupBoxOperations.ResumeLayout(false);
            this.groupBoxOperations.PerformLayout();
            this.groupBoxAlgorithm.ResumeLayout(false);
            this.groupBoxAlgorithm.PerformLayout();
            this.groupBoxTestInformation.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
