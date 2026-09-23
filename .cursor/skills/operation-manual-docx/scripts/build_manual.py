from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from xml.sax.saxutils import escape

WORKSPACE = Path(__file__).resolve().parents[4]
ROOT = WORKSPACE / 'WJ_ComprehensiveTest_Desk'
OUT = ROOT / '操作说明.docx'
ART = ROOT / 'artifacts' / 'manual'
HAS_MAIN_SHOT = (ART / 'mainform.png').exists()


def p(text, style='Normal', bold=False, center=False, italic=False):
    jc = '<w:jc w:val="center"/>' if center else ''
    rpr = ''
    if bold:
        rpr += '<w:b/>'
    if italic:
        rpr += '<w:i/>'
    rpr += '<w:rFonts w:ascii="Microsoft YaHei" w:eastAsia="Microsoft YaHei" w:hAnsi="Microsoft YaHei"/>'
    return (
        f'<w:p><w:pPr><w:pStyle w:val="{style}"/>{jc}</w:pPr>'
        f'<w:r><w:rPr>{rpr}</w:rPr><w:t xml:space="preserve">{escape(text)}</w:t></w:r></w:p>'
    )


def heading(text, level):
    return p(text, style=f'Heading{level}')


def pending(note):
    return p('待截图/待确认：' + note, italic=True)


def table(headers, rows):
    def cell(text, header=False, alt=False):
        fill = '1F4E78' if header else ('EAF2F8' if alt else 'FFFFFF')
        color = '<w:color w:val="FFFFFF"/>' if header else ''
        bold = '<w:b/>' if header else ''
        return (
            '<w:tc><w:tcPr><w:shd w:fill="' + fill + '" w:val="clear"/>'
            '<w:tcW w:w="2000" w:type="dxa"/></w:tcPr>'
            '<w:p><w:r><w:rPr>' + bold + color
            + '<w:rFonts w:ascii="Microsoft YaHei" w:eastAsia="Microsoft YaHei" w:hAnsi="Microsoft YaHei"/>'
            + '<w:sz w:val="18"/></w:rPr>'
            + '<w:t xml:space="preserve">' + escape(text) + '</w:t></w:r></w:p></w:tc>'
        )

    xml = '<w:tbl><w:tblPr><w:tblW w:w="5000" w:type="pct"/><w:tblBorders>'
    for edge in ('top', 'left', 'bottom', 'right', 'insideH', 'insideV'):
        xml += f'<w:{edge} w:val="single" w:sz="4" w:space="0" w:color="8FA4B8"/>'
    xml += '</w:tblBorders></w:tblPr><w:tr>'
    xml += ''.join(cell(h, True) for h in headers)
    xml += '</w:tr>'
    for i, row in enumerate(rows):
        xml += '<w:tr>' + ''.join(cell(c, False, i % 2 == 1) for c in row) + '</w:tr>'
    xml += '</w:tbl>' + p('')
    return xml


shot_note = (
    '工作区 artifacts/manual/mainform.png 存在，可另存标注图。'
    if HAS_MAIN_SHOT
    else '工作区缺少 artifacts/manual/mainform.png，主窗体与测试大框配图均未插入。'
)

body = ''.join([
    p('WJ_ComprehensiveTest_Desk 操作说明', 'Title', center=True),
    p('适用版本 WJ_ComprehensiveTest_Desk_20260922', bold=True, center=True),
    p('文档依据现行 Designer、后置事件和服务入口编写。界面截图缺失处已标“待截图/待确认”。不写入密钥、密文或完整卡数据。', center=True),
    p(''),
    heading('1 使用范围、启动与退出', 1),
    p('本说明面向使用 WJ_ComprehensiveTest_Desk_20260922.exe 做 DSRC 协议测试的人员。程序提供三个一级产品 Tab。只有 WJ_DSRC_Protocol_test 已实现台发、透传和标准测试入口；WJ_OBUSerial_Desk 与 WJ_CreatCard_Desk 仅显示“测试功能待后续添加”。'),
    p('启动：运行 exe 后进入主窗体。默认停在一级产品 Tab「WJ_DSRC_Protocol_test」，左侧为「台发设置通道」，右侧为二级功能 Tab。'),
    p('退出：关闭主窗体。若通信端口仍打开，程序会尝试关闭 DeskTopComm 句柄。标准测试大框为模态对话框，须先关闭测试窗再操作主窗体。'),
    p('前置条件（所有会发空口的操作共用）：台发已用串口接到本机，待测 OBU 处于可响应状态，exe 同级存在 DeskTopComm.dll 以及 SetMe.ini、BST_Locked.ini、BST_Locked_Sutong.ini。未打开端口且未初始化台发成功前，不要点透传「测试」或标准测试「测试」。'),
    heading('2 每次测试都必须先完成的主页准备', 1),
    p('标准测试窗体不自己打开串口。万集 OBU、北京地标等大框复用主窗体已经打开并初始化成功的同一台发句柄。因此无论做透传还是车道交易，都必须先在主页完成下列步骤。'),
    pending(shot_note + ' 需要主窗体整窗：一级 Tab、左侧台发、右侧功能区。'),
    table(['步骤', '控件所在区域', '操作', '成功表现', '失败表现'], [
        ('1', '通信端口', '在下拉框选择已识别的 COM 口。底部监测文字会随插拔更新。', '下拉框列出目标端口。', '显示“未识别到串口，正在实时监测”。无端口时「打开端口」不可用。'),
        ('2', '通信端口', '点击「打开端口」。', '按钮变为「关闭端口」，端口下拉禁用，状态为“COMx 已打开”，「初始化台发」可用。', '状态栏报打开失败和返回码。检查占用或线缆后重试。'),
        ('3', '台发参数', '核对 BST 间隔、非 BST 帧间隔、帧发送次数、交易超时、功率等级、物理信道。均为非负整数。界面默认分别为 30 ms、20 ms、10 次、500 ms、10、0。', '输入合法，焦点不跳回错误框。', '非法时提示“某字段必须是非负整数”。'),
        ('4', '台发参数', '按用例要求勾选 BID 更新、UnixTime 更新、LLC 更新、MAC 更新、启用日志。不要在未确认用例要求时改这些开关。', '勾选状态与计划一致。', '部分用例（如 255S 保持）会在运行中自行改 BID/UnixTime 并在结束时恢复。'),
        ('5', '台发参数', '点击「初始化台发」。未开端口时提示“请先打开通信端口。”', '状态显示“台发初始化成功，设备状态：…”。请求码和响应码均为 0。', '发送或接收初始化失败时显示返回码。此时不要进入测试。'),
    ]),
    p('「启用日志」在端口已打开时可即时下发 DLL 日志开关。初始化过程也会按当前勾选再设一次。DLL 日志文件在 exe 同级，与界面「存储测试结果」不是同一件事。'),
    p('「关闭端口」会关掉句柄，「初始化台发」随之禁用。关闭后再测须重新打开并初始化。'),
    heading('3 一级产品 Tab', 1),
    heading('3.1 WJ_DSRC_Protocol_test', 2),
    p('一级产品 Tab，进入后才看得到左侧台发通道和右侧二级功能 Tab。只切换本 Tab 不会打开串口，也不会启动任何测试。'),
    heading('3.2 WJ_OBUSerial_Desk', 2),
    p('预留页，文案为“测试功能待后续添加”。当前版本不要在此做 OBU 串口测试。占位，未实现。'),
    heading('3.3 WJ_CreatCard_Desk', 2),
    p('预留页，文案为“测试功能待后续添加”。当前版本不要在此做发卡。占位，未实现。'),
    heading('4 推荐操作：万集 OBU 备用车道交易测试', 1),
    p('下面按现行界面把“车道交易测试”从主页写到出结果。其他已实现叶子的进门方式相同，只是树路径和判定不同。'),
    heading('4.1 从主页走到测试大框', 2),
    table(['步骤', '界面位置', '操作', '说明'], [
        ('A', '主窗体左侧', '按第 2 章打开串口并初始化台发，直到状态为初始化成功。', '大框打开后不能再点主窗体。句柄必须在打开大框前就绪。'),
        ('B', '一级产品 Tab', '确认当前是「WJ_DSRC_Protocol_test」。', '若在 OBU 串口或发卡页，先切回本 Tab。'),
        ('C', '右侧二级功能 Tab', '点击标题为「WJ_DSRC_Protocol_test」的二级 Tab。它和一级 Tab 同名，里面是四个标准测试大按钮，不是透传页。', '左侧三个二级 Tab 从左到右为：透传模式、WJ_DSRC_Protocol_test、日常使用。'),
        ('D', '二级 Tab 内四个按钮', '点击「万集 OBU 标准测试」。', '以模态对话框打开「万集 OBU 标准测试」大框，并传入主窗体台发服务。'),
    ]),
    pending('二级 Tab「WJ_DSRC_Protocol_test」四按钮页、万集 OBU 测试大框整窗。'),
    heading('4.2 在大框内选中并执行车道交易', 2),
    table(['步骤', '控件', '操作', '效果'], [
        ('E', '测试项选择树', '展开「备用测试项」。', '看到三个叶子：1、车道交易测试；2、门架交易测试；3、典型交易测试。'),
        ('F', '测试项选择树', '只勾选「1、车道交易测试」。不要同时勾选门架或典型交易，也不要勾选整个「备用测试项」父节点（父节点会把三个叶子都勾上并按顺序连跑）。', '「测试」只执行车道交易这一条。'),
        ('G', '操作区算法', '选择 3SDE 或 SM4。默认 SM4。车道交易用该选择决定消费初始化算法标识（3DES 为 0x01，SM4 为 0x41）和进程内 MAC1。', '不访问读卡器或硬件 PSAM。'),
        ('H', '存储测试结果', '需要 TXT 和整窗 PNG 时勾选。默认不勾选。未勾选不创建 TestResults、TestScreenshots。', '只影响是否落盘，不影响是否发空口。'),
        ('I', '交易_校验ESAM', '按用例要求决定是否勾选。车道交易软算入口以服务层已确认流程为准。', '双击叶子可看流程说明，不会发帧。'),
        ('J', '「测试」', '点击。', '禁用「测试」「全部测试」，启用「停止测试」。右侧测试信息出现“开始执行备用测试项：车道交易测试…”。'),
    ]),
    p('也可先单击选中叶子但不勾选，再点「测试」：没有勾选项时，程序用当前选中节点。推荐勾选，避免树焦点停在别的叶子上误跑。'),
    p('「全部测试」会按树顺序跑当前可见的全部叶子，其中包含尚未实现的占位项；占位项不会发空口，也不会记为通过。车道交易专项不要用「全部测试」。'),
    heading('4.3 车道交易发什么、怎么算通过', 2),
    p('配置：exe 同级 SetMe.ini。[SET_Trade_GB] 的 number 为轮数（缺省 1），interval 为轮间隔毫秒（缺省 8000）。[TRADE_SET]/19 必须是 10 个十六进制字符：前 8 个为 4 字节扣费金额，后 2 个为写入 ICC 0019 的出入口状态。缺节、长度不对或非法字符会立刻失败并写明原因，不会静默用全零。'),
    p('空口顺序（每一轮）：车道 BST → VST → GetSecure 等车辆/卡片步骤 → 按轮次读写 ESAM EF04 与 ICC 0019/0002 → 用配置金额做消费初始化并把末字节写入 0019 → 软算 MAC1 → 扣费 → SetMMI → 单向 EventReport（只发不收）。'),
    p('通过：配置的每一轮上述步骤都成功，成功轮数等于总轮数。BST/VST、车辆信息、卡片文件、消费初始化、MAC、扣费或 SetMMI 任一步失败则整例失败。DLL 返回成功不等于测试通过。常规有应答 APDU 以 90 00 为成功；出现 93 02 等时，测试信息会写期望、实际和完整异常帧。'),
    p('界面只显示步骤、进度、失败原因和结论。完整下行/回复和 DLL 返回码仅在勾选存储后写入 exe 同级 TestResults。截图写入 TestScreenshots。'),
    p('停止：测试中点「停止测试」。已发出的帧仍应留在结果里。结束（成功、失败或取消）后恢复按钮。'),
    p('本说明生成时未连真实 RSU/OBU。扣费、写卡、蜂鸣和最终通过仍须实机确认。'),
    heading('5 台发设置通道逐项说明', 1),
    pending('台发区特写截图。'),
    table(['控件', '所在区域', '用途', '输入规则', '操作后效果', '是否发帧/写结果'], [
        ('通信端口', '通信端口', '选择 DeskTopComm 使用的 COM。', '必须是监测到的端口名。', '仅改选择，未点打开前不通信。', '否'),
        ('打开端口/关闭端口', '通信端口', '打开或关闭 DLL 句柄。', '打开前须有端口。', '见第 2 章。', '打开/关闭设备，不发业务帧'),
        ('串口监测文字', '通信端口', '显示识别结果或插拔变化。', '只读。', '不改变通信。', '否'),
        ('BST 间隔', '台发参数', '初始化写入 BST 间隔。', '非负整数，单位 ms。', '仅在初始化时下发。', '初始化请求'),
        ('非 BST 帧间隔', '台发参数', '非 BST 等待。', '非负整数，ms。', '随初始化下发。', '初始化请求'),
        ('帧发送次数', '台发参数', '台发重发次数。', '非负整数。', '过大则测试变长。', '初始化请求'),
        ('交易超时', '台发参数', '交易等待上限。', '非负整数，ms。', '随初始化下发。', '初始化请求'),
        ('功率等级', '台发参数', '发射功率。', '非负整数，范围以设备为准。', '随初始化下发。', '初始化请求'),
        ('物理信道', '台发参数', '物理信道编号。', '非负整数。部分用例运行中会改信道并在结束恢复。', '随初始化下发。', '初始化请求'),
        ('BID/UnixTime/LLC/MAC 更新', '台发参数', '初始化时对应字段是否自动更新。', '勾选或不选。', '未确认用例要求不要改。', '初始化请求'),
        ('启用日志', '台发参数', 'DLL 过程日志。', '勾选。', '端口已开时可立即生效。', '不写 TestResults'),
        ('初始化台发', '台发参数', '把参数发到台发。', '端口已开且参数合法。', '成功后才能测。', '发初始化，不写用例结果文件'),
        ('设备状态', '设备状态', '打开、初始化、错误摘要。', '只读。', '红色为错误，绿色为正常。', '否'),
    ]),
    heading('6 二级功能页：透传模式', 1),
    p('在一级 Tab「WJ_DSRC_Protocol_test」内点二级 Tab「透传模式」。透传用于手工组待发列表并发送，不等于某个标准测试叶子。执行前仍须第 2 章的开端口和初始化。未初始化时状态文字为“请先在左侧打开端口并初始化台发”。'),
    table(['控件', '所在区域', '用途', '操作与效果', '改界面/列表/发设备/只显示'], [
        ('帧解析区', '左侧帧解析', '显示选中实时帧的字段。', '点击右上方某条实时上下行帧后解析。短帧提示截断。', '只显示'),
        ('上下行帧（实时）列表', '实时区', 'DLL 产生的上下行记录。', '点选后加载到解析和 BUF。', '只显示'),
        ('打开', '实时区', '从文件加载实时帧显示。', '只改显示，不自动测试。', '改界面'),
        ('保存', '实时区', '保存当前实时帧显示。', '写文件，不发设备。', '改界面/文件'),
        ('清空（实时区）', '实时区', '清实时列表。', '不影响人工待发列表。', '改界面'),
        ('帧操作列表', '帧操作', '人工待发序列。', '选中后上移、下移、删除。', '维护待发列表'),
        ('上移/下移/删除/清空', '帧操作', '调整或移除待发项。', '清空只清人工列表。', '维护待发列表'),
        ('交易帧 + 添加', '帧操作', '加入协议模板。', '先选类型再添加。', '维护待发列表'),
        ('IC 卡 + 添加', '帧操作', '加入 IC 模板。', '同上。', '维护待发列表'),
        ('ESAM + 添加', '帧操作', '加入 ESAM 模板。', '同上。', '维护待发列表'),
        ('BUF / 修改 / 清空 BUF', '帧操作', '编辑当前帧缓冲。', '修改写入 BUF；清空 BUF 不影响列表顺序。', '改界面，影响后续发送内容'),
        ('测试', '帧操作', '按列表向已初始化台发发送。', '内部超时 500 ms。结果在解析/状态区。', '发设备帧'),
    ]),
    p('打开、保存、清空（实时区）只操作实时帧，不操作人工待发列表。'),
    heading('7 二级功能页：标准测试入口', 1),
    p('二级 Tab 标题为「WJ_DSRC_Protocol_test」（与一级产品 Tab 同名）。内有四个大按钮。点击后打开独立模态大框，不在主窗体内嵌测试树。'),
    table(['按钮', '打开窗体', '当前实现', '前置'], [
        ('万集 OBU 标准测试', 'WanjiStandardTestForm', '已接大多数叶子协议。见第 8、10 章。', '主页已开端口并初始化。'),
        ('北京地标协议测试', 'BeijingLocalStandardTestForm', '已接广播 BST、错误 MAC、典型交易共 7 个叶子。其余为目录或原版未实现。', '同上。'),
        ('万集 CPC 卡标准测试', 'WanjiCpcStandardTestForm', '仅有树和按钮。执行写“待移植”，不调用设备。占位。', '即使开了端口也不会发空口。'),
        ('守望者播报测试', 'WatchmanBroadcastTestForm', '同上，占位。', '同上。'),
    ]),
    heading('7.1 日常使用', 2),
    p('二级 Tab「日常使用」显示“日常使用功能区待后续实现”。占位，未实现。'),
    heading('8 万集 OBU 标准测试大框控件', 1),
    table(['控件', '所在区域', '用途', '操作与效果', '帧/日志/结果'], [
        ('测试项树', '测试项选择', '分组、子项、叶子。父勾选同步子孙。', '勾选叶子后点「测试」。双击只弹流程说明，不通信。', '勾选本身不发帧'),
        ('存储测试结果', '操作区顶部', '是否写 TXT/PNG。', '默认不勾。', '不勾选不建结果目录'),
        ('交易_校验ESAM', '操作区', 'ESAM 校验选项。', '按该叶子说明使用。', '可能影响判定，不单独存文件'),
        ('3SDE / SM4', '算法', '软算算法。默认 SM4。', '车道/门架/典型等交易读取此项。', '影响组帧与 MAC1'),
        ('测试', '操作区', '跑勾选叶子，无勾选则跑选中节点（含子叶子）。', '开始后禁用测试、启用停止。', '已实现叶子发空口'),
        ('全部测试', '操作区', '按树顺序跑全部叶子。', '含占位项，占位不发空口、不记通过。', '已实现项发空口'),
        ('停止测试', '操作区', '取消当前后台流程。', '无运行中测试时提示没有可停止的测试。', '已发帧仍可写入结果'),
        ('清空显示', '操作区', '清空右侧测试信息。', '不改勾选、不删已写文件。', '只改界面'),
        ('测试信息', '右侧', '步骤、进度、失败原因、结论、结果路径。', '自动滚到末行。', '完整帧不堆在此处'),
    ]),
    heading('9 北京地标、CPC、守望者大框', 1),
    p('北京地标：树来自原 ONECHIP 目录。已登记处理器的路径为「OBU基本测试 > 1、交易过程中响应广播帧测试」两条、「OBU基本测试 > 2、交易过程中响应不同MAC测试」三条、「OBU基本测试 > 3、典型交易测试」。典型交易有“校验车辆信息”勾选。未登记路径点运行不会按已迁移协议执行。原版标注未实现的项不要当成已移植。'),
    p('北京典型交易与万集备用「3、典型交易测试」不是同一套 GetSecure/间隔/分支，不要混用操作经验当判定。'),
    p('CPC 树：卡片复位、身份/版本读取、应用选择、文件读取、余额/交易记录、卡片未插入、权限错误。全部占位。'),
    p('守望者树：设备状态/交易完成播报、小额/整额/边界金额、余额不足/交易失败、交易后播报。全部占位。'),
    heading('10 万集 OBU 叶子实现状态与判定要点', 1),
    p('执行任意叶子前都走第 2 章和第 4.1 节进门。双击查看该叶子说明。下列占位项只迁了名称，点「测试」不会发空口，也不会报通过：通用 13、地标交易测试；单片 1、双通道圈存测试；双片发行与激活 3 项；双片无卡机制 3 项；地区 1、ESAM 指令检测（广东）。'),
    table(['树路径（摘要）', '状态', '关键判断'], [
        ('通用 > 0 版本号读取', '已实现', 'BST/VST、71 认证、读版本成功。'),
        ('通用 > 1 BST 兼容性', '已实现', '读 [SET_Bst]/number，随机 BST/VST，无 EventReport。'),
        ('通用 > 2 VST 随机避让', '已实现', '读避让次数；三窗计数均严格大于次数的 1/4。'),
        ('通用 > 3 防碰撞', '已实现', '五段计时 0<t<8 ms；[SET_139_AntiCol_num]/number 必须为 5。'),
        ('通用 > 4～8 预读/信道/GetSecure/TransferChannel/SetMMI', '已实现', '按各叶子说明；常规 APDU 要 90 00；EventReport 只发。'),
        ('通用 > 9 拼帧', '已实现', '按组合下发；过长组合跳过。'),
        ('通用 > 10 255S 保持', '已实现', '读 BST_Locked_Sutong.ini；00 须交易，01 须无 VST。'),
        ('通用 > 11 广播 BST', '已实现', '二次 BST 成功且无 VST。'),
        ('通用 > 12 不同 MAC', '已实现', '错误 MAC 的 TransferChannel 无响应为通过。'),
        ('通用 > 14 指令集', '已实现', '正常项 90 00 00；故意错误项核对声明的状态字。'),
        ('通用 > 15～23 及万集自有/语音等', '已实现入口', '按双击说明；语音和写卡效果须实机。'),
        ('单片发行与激活 3 项、双通道切换 4 项', '已实现入口', '按双击说明。'),
        ('备用 > 1 车道交易', '已实现', '见第 4 章。'),
        ('备用 > 2 门架交易', '已实现', '扣费后无 SetMMI；0019 状态固定 0x03。'),
        ('备用 > 3 典型交易', '已实现', '六个分支全成功；0019 状态固定 04。'),
        ('第 4.2 节所列 9 个占位', '仅名称', '不发空口，不记通过。'),
    ]),
    heading('11 日志与结果文件', 1),
    p('界面测试信息：摘要、步骤、进度、失败原因、结论、结果路径。'),
    p('DLL「启用日志」：exe 同级 DLL 过程日志，与是否勾选存储无关。'),
    p('存储测试结果：勾选后，已执行用例（含失败）写 TestResults 下“时间_用例名_测试结果.txt”，内容含结论、DLL 码、完整请求/响应。EventReport 只记发送返回码。同时截测试大框到 TestScreenshots。批量多叶子时子项写 TXT，队列结束后一张汇总 PNG。'),
    p('未勾选存储：不创建上述目录，不能把界面滚过的文字当成已存档。'),
    heading('12 异常处理', 1),
    table(['现象', '处理'], [
        ('找不到 DeskTopComm.dll 或位数不匹配', '确认 exe 同级为 x86 DLL。不要改程序为 x64。'),
        ('打不开串口', '查占用、线缆、监测列表后重开。'),
        ('初始化失败', '查端口、参数、设备。成功前不要点测试。'),
        ('请先打开通信端口', '回到主页完成第 2 章。大框打开期间主页点不到，须先关大框。'),
        ('未连接主窗体台发服务', '必须从主页按钮打开 OBU 大框，不要用无服务的构造方式。'),
        ('SetMe.ini [TRADE_SET]/19 非法', '按 10 位十六进制修正后再测，不要期望程序填默认金额。'),
        ('超时或无响应', '保存日志和帧；核对该叶子是否本来就要求无 VST。'),
        ('93 02 等非 90 00', '看测试信息中的步骤、期望、实际和完整帧。'),
        ('误跑多个叶子', '取消父节点勾选，只留目标叶子。'),
        ('OBU 串口/发卡/日常使用/CPC/守望者无协议', '当前为占位，不要当已实现。'),
    ]),
    heading('13 当前限制', 1),
    p('生成本说明时工作区没有可用的主窗体/测试大框 PNG，第 4 章及台发区配图均为待截图/待确认。'),
    p('未连接真实 RSU、OBU 或读卡器做本说明过程中的实机回归。空口响应、卡片数据、语音、永久写卡仍须在受控设备上确认。'),
    p('占位用例未写成已实现。'),
])

styles_xml = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:style w:type="paragraph" w:default="1" w:styleId="Normal">
    <w:name w:val="Normal"/>
    <w:rPr><w:rFonts w:ascii="Microsoft YaHei" w:eastAsia="Microsoft YaHei" w:hAnsi="Microsoft YaHei"/><w:sz w:val="20"/><w:color w:val="000000"/></w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Title">
    <w:name w:val="Title"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="240" w:after="240"/></w:pPr>
    <w:rPr><w:b/><w:sz w:val="48"/><w:color w:val="000000"/></w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Heading1">
    <w:name w:val="heading 1"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:outlineLvl w:val="0"/><w:spacing w:before="280" w:after="120"/></w:pPr>
    <w:rPr><w:b/><w:sz w:val="34"/><w:color w:val="000000"/></w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Heading2">
    <w:name w:val="heading 2"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:outlineLvl w:val="1"/><w:spacing w:before="200" w:after="80"/></w:pPr>
    <w:rPr><w:b/><w:sz w:val="28"/><w:color w:val="000000"/></w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Heading3">
    <w:name w:val="heading 3"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:outlineLvl w:val="2"/><w:spacing w:before="160" w:after="60"/></w:pPr>
    <w:rPr><w:b/><w:sz w:val="22"/><w:color w:val="000000"/></w:rPr>
  </w:style>
</w:styles>'''

document_xml = (
    '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
    '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
    '<w:body>' + body
    + '<w:sectPr><w:pgMar w:top="936" w:right="1008" w:bottom="936" w:left="1008"/></w:sectPr>'
    + '</w:body></w:document>'
)

content_types = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
  <Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
</Types>'''

rels = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
</Relationships>'''

doc_rels = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>'''

OUT.parent.mkdir(parents=True, exist_ok=True)
with ZipFile(OUT, 'w', ZIP_DEFLATED) as z:
    z.writestr('[Content_Types].xml', content_types)
    z.writestr('_rels/.rels', rels)
    z.writestr('word/document.xml', document_xml)
    z.writestr('word/_rels/document.xml.rels', doc_rels)
    z.writestr('word/styles.xml', styles_xml)

print(OUT)
