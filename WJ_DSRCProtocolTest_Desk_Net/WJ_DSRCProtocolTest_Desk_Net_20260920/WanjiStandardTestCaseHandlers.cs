using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WJ_DSRCProtocolTest_Desk_Net.Services;

namespace WJ_DSRCProtocolTest_Desk_Net
{
    partial class WanjiStandardTestForm
    {
        private const int BstCompatibilityTimeoutMilliseconds = 500;
        // 原 WJ_Trade_instruction 在每个子用例发送 EventReport 后固定 Sleep(1000)，
        // 该等待用于释放本次链路并保证下一子用例的 BST 不会紧接着发送。
        private const int InstructionCaseIntervalMilliseconds = 1000;
        private const int DefaultVstRandomDodgeCount = 200;
        private int _bstProgressTextStart = -1;
        private int _bstProgressTextLength;
        private int _laneProgressTextStart = -1;
        private int _laneProgressTextLength;
        private int _gantryProgressTextStart = -1;
        private int _gantryProgressTextLength;
        private int _typicalProgressTextStart = -1;
        private int _typicalProgressTextLength;

        /// <summary>
        /// 创建全部叶子测试用例的独立函数分发表。
        /// </summary>
        /// <returns>以完整测试树路径为键、以独立用例函数为值的分发表。</returns>
        private Dictionary<string, Action> CreateTestCaseHandlers()
        {
            return new Dictionary<string, Action>(StringComparer.Ordinal)
            {
                { "通用测试项 > 0、OBU版本号读取", RunObuVersionReadTest },
                { "通用测试项 > 1、BST兼容性测试", RunBstCompatibilityTest },
                { "通用测试项 > 2、VST随机避让测试 > 测试用例1：无间隔发送BST", RunTestCase002 },
                { "通用测试项 > 3、防碰撞测试", RunTestCase003 },
                { "通用测试项 > 4、预读信息测试 > 测试用例1：预读0019文件、0002文件", RunTestCase004 },
                { "通用测试项 > 4、预读信息测试 > 测试用例2：预读0015文件", RunTestCase005 },
                { "通用测试项 > 4、预读信息测试 > 测试用例3：预读0012文件", RunTestCase006 },
                { "通用测试项 > 5、OBU信道选择测试 > 测试用例1：0信道读取文件", RunTestCase007 },
                { "通用测试项 > 5、OBU信道选择测试 > 测试用例2：1信道读取文件", RunTestCase008 },
                { "通用测试项 > 6、Getsecure测试 > 测试用例1：无访问许可，有加密秘钥标识", RunTestCase009 },
                { "通用测试项 > 6、Getsecure测试 > 测试用例2：无访问许可，无加密秘钥标识", RunTestCase010 },
                { "通用测试项 > 6、Getsecure测试 > 测试用例3：有访问许可，有加密秘钥标识", RunTestCase011 },
                { "通用测试项 > 6、Getsecure测试 > 测试用例4：有访问许可，无加密秘钥标识", RunTestCase012 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例1：1条APDU指令读余额", RunTestCase013 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例2：2条相同APDU指令读余额", RunTestCase014 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例3：3条相同APDU指令读余额", RunTestCase015 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例4：4条相同APDU指令读余额", RunTestCase016 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例5：5条相同APDU指令读余额", RunTestCase017 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例6：6条相同APDU指令读余额", RunTestCase018 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例7：7条相同APDU指令读余额", RunTestCase019 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例8：1条APDU指令操作ESAM取4字节随机数", RunTestCase020 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例9：2条APDU指令操作ESAM取4字节随机数", RunTestCase021 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例10：3条APDU指令操作ESAM取4字节随机数", RunTestCase022 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例11：3条不相同的IC通道APDU指令操作读余额、读0015文件、读0012文件", RunTestCase023 },
                { "通用测试项 > 7、transferchannel测试 > 测试用例12：3条不相同的ESAM通道APDU指令操作进3F00目录、读EF01文件、取芯片序列号", RunTestCase024 },
                { "通用测试项 > 8、SetMMI测试 > 测试用例1：参数00", RunTestCase025 },
                { "通用测试项 > 8、SetMMI测试 > 测试用例2：参数01", RunTestCase026 },
                { "通用测试项 > 8、SetMMI测试 > 测试用例3：参数02", RunTestCase027 },
                { "通用测试项 > 8、SetMMI测试 > 测试用例4：参数03", RunTestCase028 },
                { "通用测试项 > 8、SetMMI测试 > 测试用例5：参数04", RunTestCase029 },
                { "通用测试项 > 9、拼帧指令测试", RunTestCase030 },
                { "通用测试项 > 10、255S保持测试", RunTestCase031 },
                { "通用测试项 > 11、交易过程中响应广播帧测试 > 用例1：获取车辆信息后发送BST", RunTestCase032 },
                { "通用测试项 > 11、交易过程中响应广播帧测试 > 用例2：消费初始化后发送BST", RunTestCase033 },
                { "通用测试项 > 12、交易过程中响应不同MAC测试 > 用例1：获取车辆信息后发送其他标签MAC的transferChannel", RunTestCase034 },
                { "通用测试项 > 12、交易过程中响应不同MAC测试 > 用例2：读取车辆信息之后发送全ff的transferChannel", RunTestCase035 },
                { "通用测试项 > 12、交易过程中响应不同MAC测试 > 用例3：读取车辆信息之后发送其他MAC的transferChannel", RunTestCase036 },
                { "通用测试项 > 13、地标交易测试", RunTestCase037 },
                { "通用测试项 > 14、指令集测试 > 测试用例1：取4字节随机数", RunTestCase038 },
                { "通用测试项 > 14、指令集测试 > 测试用例2：取8字节随机数", RunTestCase039 },
                { "通用测试项 > 14、指令集测试 > 测试用例3：取16字节随机数", RunTestCase040 },
                { "通用测试项 > 14、指令集测试 > 测试用例4：取9字节随机数", RunTestCase041 },
                { "通用测试项 > 14、指令集测试 > 测试用例5：发送随机数p1参数不正确", RunTestCase042 },
                { "通用测试项 > 14、指令集测试 > 测试用例6：发送随机数CLA参数不正确", RunTestCase043 },
                { "通用测试项 > 14、指令集测试 > 测试用例7：发送随机数INS参数不正确", RunTestCase044 },
                { "通用测试项 > 14、指令集测试 > 测试用例8：取4字节芯片序列号", RunTestCase045 },
                { "通用测试项 > 14、指令集测试 > 测试用例9：发送取芯片序列号指令p1参数不正确", RunTestCase046 },
                { "通用测试项 > 14、指令集测试 > 测试用例10：发送取芯片序列号指令长度不正确", RunTestCase047 },
                { "通用测试项 > 14、指令集测试 > 测试用例11：取芯片序列号INS参数不正确", RunTestCase048 },
                { "通用测试项 > 14、指令集测试 > 测试用例12：取芯片序列号CLA参数不正确", RunTestCase049 },
                { "通用测试项 > 14、指令集测试 > 测试用例13：选择EF04文件", RunTestCase050 },
                { "通用测试项 > 14、指令集测试 > 测试用例14：选择EF04文件参数不正确", RunTestCase051 },
                { "通用测试项 > 14、指令集测试 > 测试用例15：选择EF04文件CLA不正确", RunTestCase052 },
                { "通用测试项 > 14、指令集测试 > 测试用例16：读取0015文件测试", RunTestCase053 },
                { "通用测试项 > 14、指令集测试 > 测试用例17：读取001A文件", RunTestCase054 },
                { "通用测试项 > 14、指令集测试 > 测试用例19：取响应代码返回6982", RunTestCase055 },
                { "通用测试项 > 14、指令集测试 > 测试用例20：响应代码6981不支持安全报文", RunTestCase056 },
                { "通用测试项 > 14、指令集测试 > 测试用例21：响应代码6A83未找到记录", RunTestCase057 },
                { "通用测试项 > 14、指令集测试 > 测试用例22：响应代码6984引用数据无效", RunTestCase058 },
                { "通用测试项 > 14、指令集测试 > 测试用例23：响应代码6984未申请随机数", RunTestCase059 },
                { "通用测试项 > 14、指令集测试 > 测试用例24：响应代码6986不满足命令执行", RunTestCase060 },
                { "通用测试项 > 14、指令集测试 > 测试用例25：响应代码6988安全报文数据项不正确", RunTestCase061 },
                { "通用测试项 > 14、指令集测试 > 测试用例26：响应代码6B00参数不正确，偏移地址超出EF", RunTestCase062 },
                { "通用测试项 > 14、指令集测试 > 测试用例27：响应代码6F00判断不准确", RunTestCase063 },
                { "通用测试项 > 14、指令集测试 > 测试用例28：响应代码6985不满足引用条件", RunTestCase064 },
                { "通用测试项 > 15、保留文件读写测试", RunTestCase065 },
                { "通用测试项 > 16、单帧唤醒测试", RunTestCase066 },
                { "通用测试项 > 17、模拟真实交易流测试", RunTestCase067 },
                { "通用测试项 > 18、万集自有协议测试 > 测试用例1：5.8G修改MACID", RunTestCase068 },
                { "通用测试项 > 18、万集自有协议测试 > 测试用例2：5.8G修改SN号", RunTestCase069 },
                { "通用测试项 > 18、万集自有协议测试 > 测试用例3：5.8G读取蓝牙地址", RunTestCase070 },
                { "通用测试项 > 18、万集自有协议测试 > 测试用例4：5.8G读写地区", RunTestCase071 },
                { "通用测试项 > 18、万集自有协议测试 > 测试用例5：不同通道验证测试", RunTestCase072 },
                { "通用测试项 > 18、万集自有协议测试 > 测试用例6：射频发数测试（55AA）", RunTestCase073 },
                { "通用测试项 > 19、播报金额测试（语音款）", RunTestCase074 },
                { "通用测试项 > 20、拼帧交易测试（语音款）", RunTestCase075 },
                { "通用测试项 > 21、交易后pin认证异常播报测试（语音款）", RunTestCase076 },
                { "通用测试项 > 22、交易25次后防拆2S失效测试", RunTestCase077 },
                { "通用测试项 > 23、VST回复EquipmentStatus测试", RunTestCase078 },
                { "单片式专用测试项 > 1、双通道圈存测试", RunTestCase079 },
                { "单片式专用测试项 > 2、双通道切换测试 > 测试用例1：ESAM读取EF01文件，ICC读取0016文件", RunTestCase080 },
                { "单片式专用测试项 > 2、双通道切换测试 > 测试用例2：ICC读取0016文件，ESAM读取EF04文件", RunTestCase081 },
                { "单片式专用测试项 > 2、双通道切换测试 > 测试用例3：ESAM读取EF04文件，ICC读取0015文件", RunTestCase082 },
                { "单片式专用测试项 > 2、双通道切换测试 > 测试用例4：ICC读取0015文件，ESAM读取EF01文件", RunTestCase083 },
                { "单片式专用测试项 > 3、发行与激活测试 > 用例1：一次发行测试（单片）", RunTestCase084 },
                { "单片式专用测试项 > 3、发行与激活测试 > 用例2：二次发行测试（单片）", RunTestCase085 },
                { "单片式专用测试项 > 3、发行与激活测试 > 用例3：标签激活测试（单片）", RunTestCase086 },
                { "双片式专用测试项 > 1、发行与激活测试 > 用例1：一次发行测试（双片）", RunTestCase087 },
                { "双片式专用测试项 > 1、发行与激活测试 > 用例2：二次发行测试（双片）", RunTestCase088 },
                { "双片式专用测试项 > 1、发行与激活测试 > 用例3：标签激活测试（双片）", RunTestCase089 },
                { "双片式专用测试项 > 2、无卡机制测试 > 用例1：不插卡片通过门架", RunTestCase090 },
                { "双片式专用测试项 > 2、无卡机制测试 > 用例2：低电模式通过门架", RunTestCase091 },
                { "双片式专用测试项 > 2、无卡机制测试 > 用例3：天线给OBU设置不回IC卡状态通过门架", RunTestCase092 },
                { "地区专用测试项 > 1、ESAM指令检测（广东）", RunTestCase093 },
                { "备用测试项 > 1、车道交易测试", RunLaneTransactionTest },
                { "备用测试项 > 2、门架交易测试", RunGantryTransactionTest },
                { "备用测试项 > 3、典型交易测试", RunTypicalTransactionTest }
            };
        }

        /// <summary>
        /// 按原 OBU 模块的配置次数随机生成 BST，执行 BST/VST 交互并记录结果。
        /// </summary>
        private void RunBstCompatibilityTest()
        {
            const string defaultTestCaseName = "1、BST兼容性测试";
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? defaultTestCaseName : selectedNode.Text;
            DateTime startedAt = DateTime.Now;
            AppendBstTestInformation("开始执行 OBU BST 兼容性测试（按 SET_Bst 配置次数随机生成 BST）……\r\n");
            // BST 用例启动入口重置上一次原位进度区域，确保本次计数不会覆盖旧测试内容。
            ResetBstCompatibilityProgress();

            if (_desktopCommService == null)
            {
                // 没有共享台发服务时仍在 UI 线程完成失败记录，不启动后台任务。
                FinishBstCompatibilityTest(
                    testCaseName,
                    startedAt,
                    new BstCompatibilityExecutionResult(
                        false,
                        "未连接主窗体台发服务，无法执行 BST 兼容性测试。",
                        new List<ObuProtocolExchange>(),
                        DateTime.Now));
                return;
            }

            // BST 用例从测试大框统一入口取得取消令牌，使同一个停止按钮适用于所有实际用例。
            CancellationToken cancellationToken = BeginTestExecution();
            // 车道交易入口重置上次原位成功计数，确保本次进度显示在新的固定位置。
            ResetLaneTransactionProgress();
            // 通信和可取消的 20 ms 帧间等待在后台执行，保持 UI 和停止按钮可响应。
            Task.Run(() => ExecuteBstCompatibilityTest(cancellationToken)).ContinueWith(
                completedTask =>
                {
                    if (IsDisposed || !IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        BeginInvoke(new Action(() => FinishBstCompatibilityTask(testCaseName, startedAt, completedTask)));
                    }
                    catch (InvalidOperationException)
                    {
                        // 窗体关闭过程中不再回写 UI，后台通信结果由统一 Dispose 流程结束。
                    }
                },
                TaskScheduler.Default);
        }

        /// <summary>
        /// 在后台线程执行 BST 兼容性通信序列，并在每次通信及帧间等待前响应停止请求。
        /// </summary>
        /// <param name="cancellationToken">由停止按钮触发的取消令牌；取消后不再发起下一次 DLL 调用。</param>
        /// <returns>包含成功状态、最终说明、完整交互帧和完成时间的执行结果。</returns>
        private BstCompatibilityExecutionResult ExecuteBstCompatibilityTest(CancellationToken cancellationToken)
        {
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            bool isSuccess = false;
            string resultMessage = string.Empty;

            try
            {
                string configurationWarning;
                // BST 后台执行入口先读取 exe 同级 SetMe.ini，沿用原上位机的测试次数配置。
                int repeatCount = ReadBstCompatibilityRepeatCount(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                    out configurationWarning);
                if (!string.IsNullOrEmpty(configurationWarning))
                {
                    AppendBstTestInformation(configurationWarning + "\r\n");
                }

                // 配置读取完成后创建唯一的进度显示位置，后续每轮只替换该位置的计数。
                UpdateBstCompatibilityProgress(0, repeatCount);

                Random random = new Random();
                int successCount = 0;
                for (int cycleIndex = 0; cycleIndex < repeatCount; cycleIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        resultMessage = "测试结果：BST兼容性测试已由用户停止。";
                        break;
                    }

                    // 每次按原 OBU 模块规则随机生成一条 BST，不复用 CPC 模块的固定 23 帧序列。
                    byte[] bstFrame = CreateObuBstCompatibilityFrame(random);
                    byte[] bstCommand = BuildBstCommand(bstFrame);
                    TransparentCommandResult bstResult = _desktopCommService.ExecuteTransparent(
                        bstCommand, BstCompatibilityTimeoutMilliseconds);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
                    // 当前交互结束后在固定位置更新已测试次数，不为每一条 BST 新增日志行。
                    UpdateBstCompatibilityProgress(cycleIndex + 1, repeatCount);

                    if (!bstResult.IsSuccess)
                    {
                        resultMessage = string.Format(
                            "第 {0}/{1} 次 BST/VST 交互失败：{2}",
                            cycleIndex + 1,
                            repeatCount,
                            bstResult.ErrorMessage);
                        break;
                    }

                    successCount++;
                    if (cycleIndex < repeatCount - 1)
                    {
                        // 原 OBU 模块每次 BST/VST 后仅等待 20 ms，不发送 EventReport。
                        if (cancellationToken.WaitHandle.WaitOne(GetBstCompatibilityDelay()))
                        {
                            resultMessage = "测试结果：BST兼容性测试已由用户停止。";
                            break;
                        }
                    }
                }

                if (successCount == repeatCount)
                {
                    isSuccess = true;
                    resultMessage = string.Format("测试结果：BST兼容性测试成功（完成 {0} 次）！", repeatCount);
                }
                else if (string.IsNullOrEmpty(resultMessage))
                {
                    resultMessage = string.Format("测试结果：BST兼容性测试失败（成功 {0}/{1} 次）。", successCount, repeatCount);
                }
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                resultMessage = "BST兼容性测试失败：" + exception.Message;
            }
            catch (InvalidOperationException exception)
            {
                resultMessage = "BST兼容性测试失败：" + exception.Message;
            }
            catch (Exception exception)
            {
                resultMessage = "BST兼容性测试失败：" + exception.Message;
            }

            return new BstCompatibilityExecutionResult(
                isSuccess,
                string.IsNullOrEmpty(resultMessage) ? "BST兼容性测试未完成。" : resultMessage,
                exchanges,
                DateTime.Now);
        }

        /// <summary>
        /// 将后台 BST 执行任务的结果切回 UI 线程并完成结果文件和截图保存。
        /// </summary>
        /// <param name="testCaseName">当前测试用例名称。</param>
        /// <param name="startedAt">测试开始时间。</param>
        /// <param name="completedTask">后台 BST 执行任务。</param>
        private void FinishBstCompatibilityTask(
            string testCaseName,
            DateTime startedAt,
            Task<BstCompatibilityExecutionResult> completedTask)
        {
            BstCompatibilityExecutionResult result;
            if (completedTask.IsCanceled)
            {
                result = new BstCompatibilityExecutionResult(
                    false,
                    "BST兼容性测试已取消。",
                    new List<ObuProtocolExchange>(),
                    DateTime.Now);
            }
            else if (completedTask.IsFaulted)
            {
                result = new BstCompatibilityExecutionResult(
                    false,
                    "BST兼容性测试后台执行失败：" + completedTask.Exception.GetBaseException().Message,
                    new List<ObuProtocolExchange>(),
                    DateTime.Now);
            }
            else
            {
                result = completedTask.Result;
            }

            FinishBstCompatibilityTest(testCaseName, startedAt, result);
        }

        /// <summary>
        /// 在 UI 线程显示 BST 最终结果，并保存完整交互帧和测试大框截图。
        /// </summary>
        /// <param name="testCaseName">当前测试用例名称。</param>
        /// <param name="startedAt">测试开始时间。</param>
        /// <param name="result">后台或前置校验产生的 BST 执行结果。</param>
        private void FinishBstCompatibilityTest(
            string testCaseName,
            DateTime startedAt,
            BstCompatibilityExecutionResult result)
        {
            try
            {
                // 最终结果入口先结束原位进度行，再在下一行显示唯一的测试结论。
                CompleteBstCompatibilityProgress();
                AppendBstTestInformation(result.Message + "\r\n");
                PersistTestArtifacts(
                    testCaseName,
                    result.IsSuccess,
                    result.Message,
                    string.Empty,
                    result.Exchanges,
                    startedAt,
                    result.CompletedAt);
            }
            finally
            {
                // BST 完成、失败或取消后统一释放大框级取消资源并恢复操作按钮。
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 将 BST 后台线程的进度文本安全地追加到测试信息显示区。
        /// </summary>
        /// <param name="message">需要追加的进度文本，通常已包含换行符。</param>
        private void AppendBstTestInformation(string message)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<string>(AppendBstTestInformation), message);
                }
                catch (InvalidOperationException)
                {
                    // 窗体关闭过程中忽略尚未显示的后台进度文本。
                }

                return;
            }

            richTextBoxTestInformation.AppendText(message);
        }

        /// <summary>
        /// 重置 BST 兼容性测试的原位进度区域，使下一次更新创建新的固定显示位置。
        /// </summary>
        private void ResetBstCompatibilityProgress()
        {
            _bstProgressTextStart = -1;
            _bstProgressTextLength = 0;
        }

        /// <summary>
        /// 在测试信息框的同一位置更新 BST 兼容性测试计数，不为每次测试追加新行。
        /// </summary>
        /// <param name="completedCount">已经完成通信尝试的次数，允许为 0。</param>
        /// <param name="totalCount">从配置文件读取的本次测试总次数，必须大于 0。</param>
        private void UpdateBstCompatibilityProgress(int completedCount, int totalCount)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<int, int>(UpdateBstCompatibilityProgress), completedCount, totalCount);
                }
                catch (InvalidOperationException)
                {
                    // 窗体关闭过程中忽略尚未显示的后台进度更新。
                }

                return;
            }

            string progressText = string.Format("测试进度：{0}/{1}", completedCount, totalCount);
            if (_bstProgressTextStart < 0 || _bstProgressTextStart + _bstProgressTextLength > richTextBoxTestInformation.TextLength)
            {
                _bstProgressTextStart = richTextBoxTestInformation.TextLength;
                richTextBoxTestInformation.AppendText(progressText);
            }
            else
            {
                richTextBoxTestInformation.Select(_bstProgressTextStart, _bstProgressTextLength);
                richTextBoxTestInformation.SelectedText = progressText;
            }

            _bstProgressTextLength = progressText.Length;
            richTextBoxTestInformation.SelectionStart = richTextBoxTestInformation.TextLength;
            richTextBoxTestInformation.SelectionLength = 0;
        }

        /// <summary>
        /// 结束 BST 兼容性测试的原位进度行，并将后续最终结论放到新行显示。
        /// </summary>
        private void CompleteBstCompatibilityProgress()
        {
            if (_bstProgressTextStart >= 0)
            {
                richTextBoxTestInformation.AppendText("\r\n");
            }

            ResetBstCompatibilityProgress();
        }

        /// <summary>
        /// 按原 OBU 测试模块规则随机生成一条 BST 兼容性测试负载。
        /// </summary>
        /// <param name="random">当前后台测试流程独占的随机数生成器，不得为空。</param>
        /// <returns>不含本地两字节类型标识的单条 BST 负载。</returns>
        /// <exception cref="ArgumentNullException">随机数生成器为空时抛出。</exception>
        private static byte[] CreateObuBstCompatibilityFrame(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            List<byte> frame = new List<byte>(64)
            {
                0xFF, 0xFF, 0xFF, 0xFF,
                0x50, 0x03
            };
            byte[] macControls = { 0x91, 0x99, 0xA1, 0xC1 };
            byte[] bstIdentifiers = { 0xC0, 0xC1, 0xC2, 0xC4 };
            byte[] mandatoryApplications = { 0x01, 0x03, 0x7F };
            byte[] containerOptions = { 0x00, 0x01, 0x03, 0x07, 0x7F };
            byte[] didOptions = { 0x01, 0x02, 0xFF };

            frame.Add(macControls[random.Next(macControls.Length)]);
            frame.Add(bstIdentifiers[random.Next(bstIdentifiers.Length)]);
            frame.AddRange(new byte[] { 0x01, 0x01, 0x01, 0x01 });
            frame.AddRange(new byte[] { 0x57, 0x57, 0x57, 0x57 });
            frame.Add(0x00);
            frame.Add(mandatoryApplications[random.Next(mandatoryApplications.Length)]);

            byte applicationFlags = 0x01;
            if (random.Next(2) == 1)
            {
                applicationFlags |= 0x80;
            }
            if (random.Next(2) == 0)
            {
                applicationFlags |= 0x40;
            }
            frame.Add(applicationFlags);

            if ((applicationFlags & 0x80) != 0)
            {
                frame.Add(didOptions[random.Next(didOptions.Length)]);
            }

            if ((applicationFlags & 0x40) != 0)
            {
                byte containerFlags = random.Next(2) == 1 ? (byte)0x80 : (byte)0x00;
                frame.Add((byte)(containerFlags | containerOptions[random.Next(containerOptions.Length)]));
                if ((containerFlags & 0x80) != 0)
                {
                    frame.Add(0x29);
                    byte preReadFlags = 0;
                    if (random.Next(2) == 0) preReadFlags |= 0x80;
                    if (random.Next(2) == 0) preReadFlags |= 0x40;
                    if (random.Next(2) == 0) preReadFlags |= 0x20;
                    if (random.Next(2) == 0) preReadFlags |= 0x10;
                    frame.Add(preReadFlags);
                    if (preReadFlags == 0)
                    {
                        frame.Add(0x63);
                    }
                    else
                    {
                        for (int mask = 0x80; mask >= 0x10; mask >>= 1)
                        {
                            if ((preReadFlags & mask) != 0)
                            {
                                frame.Add(random.Next(2) == 0 ? (byte)0x00 : (byte)0x06);
                                frame.Add(0x04);
                            }
                        }
                    }
                }
            }

            frame.Add(0x00);
            return frame.ToArray();
        }

        /// <summary>
        /// 为 BST 负载补充本地透传类型标识 00 01。
        /// </summary>
        /// <param name="payload">不含本地类型标识的 BST 空口负载。</param>
        /// <returns>可交给台发服务执行的完整 BST 透传帧。</returns>
        private static byte[] BuildBstCommand(byte[] payload)
        {
            byte[] command = new byte[payload.Length + 2];
            command[0] = 0x00;
            command[1] = 0x01;
            Buffer.BlockCopy(payload, 0, command, 2, payload.Length);
            return command;
        }

        /// <summary>
        /// 返回原 OBU 模块每次 BST/VST 兼容性测试之间的等待时间。
        /// </summary>
        /// <returns>原 `WJ_ProtocolTest_Bst` 流程使用的 20 毫秒。</returns>
        private static int GetBstCompatibilityDelay()
        {
            return 20;
        }

        /// <summary>
        /// 从原上位机兼容的 SetMe.ini 中读取 BST 兼容性测试重复次数。
        /// </summary>
        /// <param name="configurationPath">SetMe.ini 的完整路径，运行时传入 exe 同级文件。</param>
        /// <param name="warning">配置缺失或格式无效时返回的提示；成功读取时为空。</param>
        /// <returns>大于零的测试重复次数；配置不可用时返回原默认值 1。</returns>
        private static int ReadBstCompatibilityRepeatCount(string configurationPath, out string warning)
        {
            const string sectionName = "SET_Bst";
            const int defaultRepeatCount = 1;
            warning = string.Empty;

            if (!File.Exists(configurationPath))
            {
                warning = "未找到 exe 同级 SetMe.ini，BST 兼容性测试次数使用默认值 1。";
                return defaultRepeatCount;
            }

            try
            {
                string currentSection = string.Empty;
                foreach (string rawLine in File.ReadAllLines(configurationPath, Encoding.Default))
                {
                    string line = rawLine.Trim();
                    if (line.StartsWith("[", StringComparison.Ordinal))
                    {
                        int closingBracketIndex = line.IndexOf(']');
                        currentSection = closingBracketIndex > 1
                            ? line.Substring(1, closingBracketIndex - 1).Trim()
                            : string.Empty;
                        continue;
                    }

                    if (!string.Equals(currentSection, sectionName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    int separatorIndex = line.IndexOf('=');
                    if (separatorIndex <= 0 ||
                        !string.Equals(line.Substring(0, separatorIndex).Trim(), "number", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    int repeatCount;
                    if (int.TryParse(line.Substring(separatorIndex + 1).Trim(), out repeatCount) && repeatCount > 0)
                    {
                        return repeatCount;
                    }

                    warning = "SetMe.ini 的 BST 兼容性测试次数无效，使用默认值 1。";
                    return defaultRepeatCount;
                }

                warning = "SetMe.ini 未配置 BST 兼容性测试次数，使用默认值 1。";
                return defaultRepeatCount;
            }
            catch (IOException exception)
            {
                warning = "读取 SetMe.ini 失败，BST 兼容性测试次数使用默认值 1：" + exception.Message;
                return defaultRepeatCount;
            }
            catch (UnauthorizedAccessException exception)
            {
                warning = "无权读取 SetMe.ini，BST 兼容性测试次数使用默认值 1：" + exception.Message;
                return defaultRepeatCount;
            }
        }

        /// <summary>
        /// 将协议字节数组格式化为大写十六进制文本，供测试信息区和结果文件回溯。
        /// </summary>
        /// <param name="data">待格式化的数据；空值或空数组返回“(空)”。</param>
        /// <returns>以空格分隔的大写十六进制字符串。</returns>
        private static string FormatBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "(空)";
            }

            StringBuilder builder = new StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(data[i].ToString("X2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 启动一个由独立测试用例函数提供帧序列的后台协议测试流程。
        /// </summary>
        /// <param name="testCaseKey">当前叶子用例的完整树路径，用于结果文件和日志标题。</param>
        /// <param name="steps">按原上位机顺序排列的透传步骤；每步包含本地类型、空口负载和是否等待间隔。</param>
        /// <param name="physicalChannelId">需要临时切换的物理信道；为空时保持当前信道。</param>
        /// <param name="disableAutomaticMacChange">是否按旧不同MAC用例进入固定台发初始化模式，并在结束后恢复完整原配置。</param>
        private void RunProtocolSequenceTest(string testCaseKey, IList<ProtocolSequenceStep> steps, int? physicalChannelId = null, bool disableAutomaticMacChange = false)
        {
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? testCaseKey : selectedNode.Text;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                string unavailableMessage = "未连接主窗体台发服务，无法执行当前 OBU 标准测试。";
                richTextBoxTestInformation.AppendText(unavailableMessage + "\r\n");
                // 无设备句柄时仍保存失败结果，便于确认用例入口已经正确接入。
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            // 将 DLL 通信和帧间等待移到后台，保证停止按钮和测试信息区保持响应。
            Task.Run(() => ExecuteProtocolSequence(testCaseKey, steps, cancellationToken, physicalChannelId, disableAutomaticMacChange)).ContinueWith(
                completedTask =>
                {
                    if (IsDisposed || !IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        BeginInvoke(new Action(() => FinishProtocolSequenceTask(testCaseName, startedAt, completedTask)));
                    }
                    catch (InvalidOperationException)
                    {
                        // 窗体关闭期间不再回写后台测试结果。
                    }
                },
                TaskScheduler.Default);
        }

        /// <summary>
        /// 按原 OBU 测试顺序逐步发送协议帧并保存每步请求/响应。
        /// </summary>
        /// <param name="testCaseKey">用例完整路径，用于信息区首行。</param>
        /// <param name="steps">待执行的透传步骤集合。</param>
        /// <param name="cancellationToken">停止按钮提供的取消令牌；每个 DLL 调用前检查。</param>
        /// <param name="physicalChannelId">需要临时切换的物理信道；为空时保持当前信道。</param>
        /// <param name="disableAutomaticMacChange">是否临时切换到旧不同MAC用例的固定台发初始化模式。</param>
        /// <returns>包含完整交互帧、成功状态和最终结果的后台执行结果。</returns>
        private ProtocolSequenceExecutionResult ExecuteProtocolSequence(
            string testCaseKey,
            IList<ProtocolSequenceStep> steps,
            CancellationToken cancellationToken,
            int? physicalChannelId = null,
            bool disableAutomaticMacChange = false)
        {
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int originalPhysicalChannel = _desktopCommService.CurrentPhysicalChannelId;
            DesktopInitializationOptions originalDifferentMacOptions = null;
            bool terminalEventReportSent = false;
            try
            {
                if (steps == null || steps.Count == 0)
                {
                    return new ProtocolSequenceExecutionResult(false, "当前用例未配置协议步骤。", exchanges, DateTime.Now);
                }

                AppendProtocolTestInformation("开始执行：" + testCaseKey + "\r\n");
                if (disableAutomaticMacChange)
                {
                    // 三个子项均按旧固定初始化参数进入测试，确保全FF地址不受当前会话开关影响。
                    DesktopInitializationResult initialization = _desktopCommService.BeginDifferentMacTestMode(out originalDifferentMacOptions);
                    if (!initialization.IsSuccess)
                    {
                        return new ProtocolSequenceExecutionResult(false,
                            string.Format("关闭 MAC 自动更新失败：请求={0}，响应={1}。",
                                initialization.RequestResult,
                                initialization.ResponseResult.HasValue ? initialization.ResponseResult.Value.ToString() : "未执行"),
                            exchanges, DateTime.Now);
                    }
                    AppendProtocolTestInformation("已切换旧不同MAC测试模式：BID沿用原状态，LLC=开，UnixTime=关，MAC=关，信道=0，超时=500ms。\r\n");
                }
                if (physicalChannelId.HasValue)
                {
                    // 旧 WJ_Trade_ChannelChange 在发对应 BST 前重新初始化台发到 0/1 物理信道。
                    DesktopInitializationResult initialization = _desktopCommService.ReinitializePhysicalChannel(physicalChannelId.Value);
                    if (!initialization.IsSuccess)
                    {
                        return new ProtocolSequenceExecutionResult(false,
                            string.Format("切换物理信道 {0} 失败：请求={1}，响应={2}。", physicalChannelId.Value,
                                initialization.RequestResult,
                                initialization.ResponseResult.HasValue ? initialization.ResponseResult.Value.ToString() : "未执行"),
                            exchanges, DateTime.Now);
                    }
                    AppendProtocolTestInformation("台发物理信道已切换为 " + physicalChannelId.Value + "。\r\n");
                }
                for (int index = 0; index < steps.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ProtocolSequenceStep step = steps[index];
                    // 界面入口仅显示协议步骤；完整发送帧由 exchanges 保留到测试结果文档。
                    AppendProtocolTestInformation(
                        string.Format("步骤 {0}/{1}：{2}\r\n", index + 1, steps.Count, step.Name));
                    // 每一步均通过统一台发服务执行，服务根据本地类型决定是否等待响应。
                    TransparentCommandResult result = _desktopCommService.ExecuteTransparent(
                        BuildTypedCommand(step.CommandType, step.Payload),
                        step.TimeoutMilliseconds,
                        step.ApplySessionMac,
                        step.SessionMacVariant);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                    // 标记已实际进入末尾断链步骤；失败退出时据此避免重复发送 EventReport。
                    if (step.CommandType == 0x05)
                    {
                        terminalEventReportSent = true;
                    }
                    if (disableAutomaticMacChange && step.CommandType == 0x01
                        && result.RequestData != null && result.RequestData.Length >= 12)
                    {
                        // 三个子项的BST使用同一全局BeaconID计数，输出最终值以便核对跨用例递增。
                        AppendProtocolTestInformation("实际下发 BST BeaconID：" + FormatBytes(new[]
                        {
                            result.RequestData[9], result.RequestData[10], result.RequestData[11]
                        }) + "\r\n");
                    }
                    if (step.SessionMacVariant != 0 && result.RequestData != null && result.RequestData.Length >= 4)
                    {
                        // 负向用例直接显示传入DLL的最终寻址地址，便于确认全FF和第三字节加一未被覆盖。
                        AppendProtocolTestInformation(
                            "实际下发 MACID：" + FormatBytes(new[]
                            {
                                result.RequestData[0], result.RequestData[1],
                                result.RequestData[2], result.RequestData[3]
                            }) + "\r\n");
                    }
                    if (step.CommandType == 0x05)
                    {
                        // EventReport 是单向断链帧；界面仅说明动作结果，底层返回码保留在结果文档。
                        AppendProtocolTestInformation(result.RequestResult == 0
                            ? "EventReport 已发送，不等待 OBU 回复。\r\n"
                            : "EventReport 发送失败。\r\n");
                    }
                    else if (step.ExpectNoResponse)
                    {
                        // 负向测试以请求成功且接收函数返回非零作为“OBU 未响应”的判据。
                        AppendProtocolTestInformation(
                            string.Format("{0}：{1}\r\n", step.Name,
                                result.RequestResult == 0 && result.ResponseResult.HasValue && result.ResponseResult.Value != 0
                                    ? "符合预期，OBU 未响应。"
                                    : "未满足预期的无响应条件。"));
                    }
                    else
                    {
                        AppendProtocolTestInformation(result.IsSuccess
                            ? "当前步骤通信完成。\r\n"
                            : "当前步骤通信失败。\r\n");
                    }
                    bool stepSucceeded = step.ExpectNoResponse
                        ? result.RequestResult == 0 && result.ResponseResult.HasValue && result.ResponseResult.Value != 0
                        : result.IsSuccess;
                    if (stepSucceeded && step.ResponseValidator != null)
                    {
                        // 调用当前步骤自带的数据判定，避免仅凭 DLL 返回码把协议内容错误判为通过。
                        stepSucceeded = step.ResponseValidator(result.ResponseData);
                    }
                    if (!stepSucceeded)
                    {
                        return new ProtocolSequenceExecutionResult(
                            false,
                            step.ExpectNoResponse
                                ? string.Format("步骤 {0}/{1}（{2}）失败：OBU 对预期无响应的帧返回了数据。", index + 1, steps.Count, step.Name)
                                : string.Format("步骤 {0}/{1}（{2}）失败：{3}", index + 1, steps.Count, step.Name,
                                    step.ResponseValidator != null && result.IsSuccess ? step.ValidationFailureMessage : result.ErrorMessage),
                            exchanges,
                            DateTime.Now);
                    }

                    if (step.DelayMilliseconds > 0 && cancellationToken.WaitHandle.WaitOne(step.DelayMilliseconds))
                    {
                        return new ProtocolSequenceExecutionResult(false, "测试已由用户停止。", exchanges, DateTime.Now);
                    }
                }

                return new ProtocolSequenceExecutionResult(true, "测试流程完成，所有已配置协议步骤均成功。", exchanges, DateTime.Now);
            }
            catch (OperationCanceledException)
            {
                return new ProtocolSequenceExecutionResult(false, "测试已由用户停止。", exchanges, DateTime.Now);
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                return new ProtocolSequenceExecutionResult(false, "DLL 通信失败：" + exception.Message, exchanges, DateTime.Now);
            }
            catch (InvalidOperationException exception)
            {
                return new ProtocolSequenceExecutionResult(false, "测试前置条件失败：" + exception.Message, exchanges, DateTime.Now);
            }
            catch (Exception exception)
            {
                return new ProtocolSequenceExecutionResult(false, "测试执行失败：" + exception.Message, exchanges, DateTime.Now);
            }
            finally
            {
                // 任一含末尾断链帧的标准测试提前结束时都必须释放 OBU 会话，避免影响队列中的下一条 BST。
                if (!terminalEventReportSent)
                {
                    TrySendMissingTerminalEventReport(steps, exchanges);
                }
                if (disableAutomaticMacChange && originalDifferentMacOptions != null && _desktopCommService.IsOpen)
                {
                    try
                    {
                        // 用例结束恢复用户测试前的所有台发参数，避免固定测试模式泄漏到后续交易。
                        _desktopCommService.RestoreInitializationOptions(originalDifferentMacOptions);
                    }
                    catch (Exception exception)
                    {
                        AppendProtocolTestInformation("恢复原台发初始化配置失败：" + exception.Message + "\r\n");
                    }
                }
                if (physicalChannelId.HasValue && _desktopCommService.IsOpen
                    && _desktopCommService.CurrentPhysicalChannelId != originalPhysicalChannel)
                {
                    // 单叶子用例结束也恢复用户执行前的信道，避免影响后续用例。
                    try
                    {
                        _desktopCommService.ReinitializePhysicalChannel(originalPhysicalChannel);
                    }
                    catch (Exception exception)
                    {
                        AppendProtocolTestInformation("恢复原物理信道失败：" + exception.Message + "\r\n");
                    }
                }
            }
        }

        /// <summary>
        /// 在协议序列提前返回且尚未执行配置末帧时，补发该序列的单向 EventReport 以断开 OBU 会话。
        /// </summary>
        /// <param name="steps">当前用例原始步骤，用于取得其配置的 EventReport 负载与超时。</param>
        /// <param name="exchanges">测试结果要保存的完整请求/响应记录；补发记录会追加到此集合。</param>
        private void TrySendMissingTerminalEventReport(
            IList<ProtocolSequenceStep> steps,
            ICollection<ObuProtocolExchange> exchanges)
        {
            if (steps == null || _desktopCommService == null || !_desktopCommService.IsOpen)
            {
                return;
            }

            ProtocolSequenceStep eventReportStep = null;
            for (int index = steps.Count - 1; index >= 0; index--)
            {
                if (steps[index].CommandType == 0x05)
                {
                    eventReportStep = steps[index];
                    break;
                }
            }
            if (eventReportStep == null)
            {
                return;
            }

            try
            {
                // 只发送台发断链通知，不把 EventReport 当成需要 OBU 回复的交互帧。
                AppendProtocolTestInformation("当前用例在末尾断链前结束，补发 EventReport 释放 OBU 会话。\r\n");
                TransparentCommandResult result = _desktopCommService.ExecuteTransparent(
                    BuildTypedCommand(eventReportStep.CommandType, eventReportStep.Payload),
                    eventReportStep.TimeoutMilliseconds,
                    eventReportStep.ApplySessionMac,
                    eventReportStep.SessionMacVariant);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                AppendProtocolTestInformation(result.RequestResult == 0
                    ? "补发 EventReport 已发送，不等待 OBU 回复。\r\n"
                    : "补发 EventReport 发送失败。\r\n");
            }
            catch (Exception exception)
            {
                // 清理动作不能覆盖原用例的失败结论；保留异常信息供结果文件和实机排查使用。
                AppendProtocolTestInformation("补发 EventReport 异常：" + exception.Message + "\r\n");
            }
        }

        /// <summary>
        /// 将后台协议序列任务切回 UI 线程，统一写入结果文件和测试大框截图。
        /// </summary>
        /// <param name="testCaseName">测试用例叶子名称。</param>
        /// <param name="startedAt">测试启动时间。</param>
        /// <param name="completedTask">后台执行任务。</param>
        private void FinishProtocolSequenceTask(
            string testCaseName,
            DateTime startedAt,
            Task<ProtocolSequenceExecutionResult> completedTask)
        {
            ProtocolSequenceExecutionResult result = completedTask.IsFaulted
                ? new ProtocolSequenceExecutionResult(false, "后台协议测试失败：" + completedTask.Exception.GetBaseException().Message, null, DateTime.Now)
                : completedTask.IsCanceled
                    ? new ProtocolSequenceExecutionResult(false, "测试已取消。", null, DateTime.Now)
                    : completedTask.Result;
            AppendProtocolTestInformation(result.Message + "\r\n");
            PersistTestArtifacts(testCaseName, result.IsSuccess, result.Message, string.Empty, result.Exchanges, startedAt, result.CompletedAt);
            // 协议序列完成、失败或取消后释放统一停止资源并恢复按钮。
            CompleteTestExecution();
        }

        /// <summary>
        /// 在测试信息区线程安全地追加协议测试进度。
        /// </summary>
        /// <param name="message">要追加的文本。</param>
        private void AppendProtocolTestInformation(string message)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<string>(AppendProtocolTestInformation), message);
                }
                catch (InvalidOperationException)
                {
                    // 窗体关闭期间忽略尚未显示的后台文本。
                }

                return;
            }

            richTextBoxTestInformation.AppendText(message);
        }

        /// <summary>
        /// 创建本地透传类型前缀，保持协议负载与原上位机空口帧分离。
        /// </summary>
        /// <param name="commandType">本地类型：1 BST、2 GetSecure、3 TransferChannel、4 SetMMI、5 EventReport。</param>
        /// <param name="payload">原上位机传给 DLL 的空口负载，不包含本地两字节类型。</param>
        /// <returns>可交给 DesktopCommService 的完整透传命令。</returns>
        private static byte[] BuildTypedCommand(byte commandType, byte[] payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            byte[] command = new byte[payload.Length + 2];
            command[0] = 0x00;
            command[1] = commandType;
            Buffer.BlockCopy(payload, 0, command, 2, payload.Length);
            return command;
        }

        /// <summary>
        /// 读取指定配置节的测试次数，供随机避让、防碰撞和预读测试沿用原配置。
        /// </summary>
        /// <param name="sectionName">SetMe.ini 节名。</param>
        /// <param name="defaultValue">配置缺失或非法时使用的默认次数。</param>
        /// <returns>大于零的测试次数。</returns>
        private static int ReadConfiguredTestCount(string sectionName, int defaultValue)
        {
            string path = ResolveRuntimeFilePath("SetMe.ini");
            if (!File.Exists(path))
            {
                return defaultValue;
            }

            string currentSection = string.Empty;
            foreach (string rawLine in File.ReadAllLines(path, Encoding.Default))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    int closeIndex = line.IndexOf(']');
                    currentSection = closeIndex > 1 ? line.Substring(1, closeIndex - 1).Trim() : string.Empty;
                    continue;
                }

                if (!string.Equals(currentSection, sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int separator = line.IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                int value;
                string valueText = line.Substring(separator + 1).Split(new[] { ';' }, 2)[0].Trim();
                if (int.TryParse(valueText, out value) && value > 0)
                {
                    return value;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 创建原 OBU 随机避让测试每轮使用的 30 字节 BST，并刷新其中的 Unix 时间字段。
        /// </summary>
        /// <returns>与原 WJ_Trade_RandomDodge_ZeroSecond 相同的 BST 空口负载。</returns>
        private static byte[] CreateVstRandomDodgeBstFrame()
        {
            byte[] frame = new byte[]
            {
                0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00,
                0x03, 0xA6, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x41, 0x87,
                0x29, 0x20, 0x27, 0x00, 0x2B, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            long unixSeconds = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            frame[12] = (byte)(unixSeconds >> 24);
            frame[13] = (byte)(unixSeconds >> 16);
            frame[14] = (byte)(unixSeconds >> 8);
            frame[15] = (byte)unixSeconds;
            return frame;
        }

        /// <summary>
        /// 从 OtherFrames 响应的前四个字节计算原上位机使用的随机避让时间窗值。
        /// </summary>
        /// <param name="responseData">OtherFrames 响应数据；前四字节为大端毫秒值。</param>
        /// <returns>按原公式计算的时间窗值，单位为毫秒。</returns>
        private static double CalculateVstRandomDodgeWindow(byte[] responseData)
        {
            if (responseData == null || responseData.Length < 4)
            {
                throw new ArgumentException("随机避让时间窗响应至少需要 4 个字节。", nameof(responseData));
            }

            uint rawMilliseconds = ((uint)responseData[0] << 24)
                | ((uint)responseData[1] << 16)
                | ((uint)responseData[2] << 8)
                | responseData[3];
            return rawMilliseconds / 1000.0 - 2.6044;
        }

        /// <summary>
        /// 判断 TransferChannel 响应是否以原上位机要求的 90 00 00 状态结束。
        /// </summary>
        /// <param name="responseData">DLL 返回的 TransferChannel 响应。</param>
        /// <returns>响应至少三字节且末三字节为 90 00 00 时返回 true。</returns>
        private static bool HasSuccessfulTransferStatus(byte[] responseData)
        {
            return responseData != null
                && responseData.Length >= 3
                && responseData[responseData.Length - 3] == 0x90
                && responseData[responseData.Length - 2] == 0x00
                && responseData[responseData.Length - 1] == 0x00;
        }

        /// <summary>按 TransferChannel.response 的 DataList 逐条校验 APDU 状态及 ReturnStatus。</summary>
        /// <param name="responseData">DLL 返回的完整空口响应。</param>
        /// <param name="expectedDataCount">请求中声明的 APDU 数量。</param>
        /// <returns>数量相同、每条数据以 90 00 结束且 ReturnStatus 为 00 时返回 true。</returns>
        private static bool HasSuccessfulTransferResponse(byte[] responseData, int expectedDataCount)
        {
            if (responseData == null || responseData.Length < 14 || responseData[12] != expectedDataCount)
            {
                return false;
            }

            int offset = 13;
            for (int index = 0; index < expectedDataCount; index++)
            {
                if (offset >= responseData.Length) return false;
                int dataLength = responseData[offset++];
                if (dataLength < 2 || offset + dataLength > responseData.Length) return false;
                if (responseData[offset + dataLength - 2] != 0x90 || responseData[offset + dataLength - 1] != 0x00) return false;
                offset += dataLength;
            }

            return offset < responseData.Length && responseData[offset] == 0x00;
        }

        /// <summary>
        /// 判断 TransferChannel 响应末尾是否为指定的三字节状态码。
        /// </summary>
        /// <param name="responseData">DLL 返回的 TransferChannel 响应。</param>
        /// <param name="status1">状态码第一个字节。</param>
        /// <param name="status2">状态码第二个字节。</param>
        /// <param name="status3">状态码第三个字节。</param>
        /// <returns>响应末尾严格匹配指定状态码时返回 true。</returns>
        private static bool HasTransferStatus(byte[] responseData, byte status1, byte status2, byte status3)
        {
            return responseData != null && responseData.Length >= 3
                && responseData[responseData.Length - 3] == status1
                && responseData[responseData.Length - 2] == status2
                && responseData[responseData.Length - 1] == status3;
        }

        /// <summary>
        /// 创建原上位机“指令集测试”第 1～17 项的协议步骤。
        /// </summary>
        /// <param name="caseNumber">指令集用例编号，支持 1～17。</param>
        /// <returns>BST、进入 ESAM 3F00、目标 APDU、SetMMI 和 EventReport 步骤。</returns>
        /// <exception cref="ArgumentOutOfRangeException">用例编号不在 1～17 范围内时抛出。</exception>
        private static IList<ProtocolSequenceStep> CreateInstructionSteps(int caseNumber)
        {
            if (caseNumber < 1 || caseNumber > 17)
            {
                throw new ArgumentOutOfRangeException(nameof(caseNumber), "当前批次只移植指令集第 1～17 项。 ");
            }

            byte[] apdu;
            byte status1 = 0x90;
            byte status2 = 0x00;
            byte status3 = 0x00;
            switch (caseNumber)
            {
                case 1: apdu = new byte[] { 0x05, 0x00, 0x84, 0x00, 0x00, 0x04 }; break;
                case 2: apdu = new byte[] { 0x05, 0x00, 0x84, 0x00, 0x00, 0x08 }; break;
                case 3: apdu = new byte[] { 0x05, 0x00, 0x84, 0x00, 0x00, 0x10 }; break;
                case 4: apdu = new byte[] { 0x05, 0x00, 0x84, 0x00, 0x00, 0x09 }; status1 = 0x67; break;
                case 5: apdu = new byte[] { 0x05, 0x00, 0x84, 0x01, 0x00, 0x04 }; status1 = 0x6A; status2 = 0x86; break;
                case 6: apdu = new byte[] { 0x05, 0x04, 0x84, 0x00, 0x00, 0x04 }; status1 = 0x6E; break;
                case 7: apdu = new byte[] { 0x05, 0x00, 0x83, 0x00, 0x00, 0x04 }; status1 = 0x6D; break;
                case 8: apdu = new byte[] { 0x05, 0x80, 0xF6, 0x00, 0x03, 0x04 }; break;
                case 9: apdu = new byte[] { 0x05, 0x80, 0xF6, 0x01, 0x03, 0x04 }; status1 = 0x6A; status2 = 0x86; break;
                // 旧 WJ_Trade_instruction 的“Le 长度不正确”分支固定判定 6C 04 00，
                // 其中 04 是 OBU 指示的正确 Le，而非通用长度错误 67 00。
                case 10: apdu = new byte[] { 0x05, 0x80, 0xF6, 0x00, 0x03, 0x05 }; status1 = 0x6C; status2 = 0x04; break;
                case 11: apdu = new byte[] { 0x05, 0x80, 0xF7, 0x00, 0x03, 0x04 }; status1 = 0x6D; break;
                case 12: apdu = new byte[] { 0x05, 0x00, 0xF6, 0x00, 0x03, 0x04 }; status1 = 0x6E; break;
                case 13: apdu = new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xEF, 0x04 }; break;
                case 14: apdu = new byte[] { 0x07, 0x00, 0xA4, 0x01, 0x00, 0x02, 0xEF, 0x04 }; status1 = 0x6A; status2 = 0x86; break;
                case 15: apdu = new byte[] { 0x07, 0x04, 0xA4, 0x00, 0x00, 0x02, 0xEF, 0x04 }; status1 = 0x6E; break;
                case 16: apdu = new byte[] { 0x05, 0x00, 0xB0, 0x95, 0x00, 0x2B }; break;
                default: apdu = new byte[] { 0x05, 0x00, 0xB0, 0x1A, 0x00, 0x04 }; break;
            }

            byte[] bst = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x00, 0x25, 0x00 };
            byte[] select3F00 = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x01, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00 };
            List<byte> transfer = new List<byte> { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x01 };
            transfer.AddRange(apdu);
            byte expected1 = status1;
            byte expected2 = status2;
            byte expected3 = status3;
            Func<byte[], bool> validator = response => HasTransferStatus(response, expected1, expected2, expected3);
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("指令集测试 BST", 0x01, bst, 0),
                new ProtocolSequenceStep("选择 ESAM 3F00 目录", 0x03, select3F00, 0, false, HasSuccessfulTransferStatus, "选择 3F00 目录未返回 90 00 00。"),
                new ProtocolSequenceStep(string.Format("执行指令集第 {0} 项 APDU", caseNumber), 0x03, transfer.ToArray(), 0, false, validator, string.Format("目标 APDU 未返回 {0:X2} {1:X2} {2:X2}。", expected1, expected2, expected3)),
                new ProtocolSequenceStep("SetMMI", 0x04, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0),
                // 指令集子用例结束后统一等待 1 秒，再由父级队列调度下一子项。
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, InstructionCaseIntervalMilliseconds)
            };
        }

        /// <summary>
        /// 创建原上位机“指令集测试”第 19～28 项响应码测试步骤。
        /// </summary>
        /// <param name="caseNumber">界面中的指令集用例编号，支持 19～28。</param>
        /// <returns>与原 WJ_Trade_instruction 分支一致的 BST、目录选择、目标 APDU 和断链步骤。</returns>
        /// <exception cref="ArgumentOutOfRangeException">用例编号不在 19～28 范围内时抛出。</exception>
        private static IList<ProtocolSequenceStep> CreateInstructionResponseCodeSteps(int caseNumber)
        {
            if (caseNumber < 19 || caseNumber > 28)
            {
                throw new ArgumentOutOfRangeException(nameof(caseNumber), "响应码测试只支持指令集第 19～28 项。");
            }

            byte[] bst = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x00, 0x25, 0x00 };
            List<ProtocolSequenceStep> steps = new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("指令集响应码测试 BST", 0x01, bst, 0)
            };
            byte llc = 0xF7;
            AddInstructionTransferStep(steps, "选择 ESAM 3F00 目录", ref llc, 0x02,
                new[] { new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00 } }, 0x90, 0x00, 0x00);

            switch (caseNumber)
            {
                case 19:
                    AddInstructionTransferStep(steps, "选择 ESAM DF01 目录", ref llc, 0x02,
                        new[] { new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xDF, 0x01 } }, 0x90, 0x00, 0x00);
                    AddInstructionTransferStep(steps, "读取 EF01 并检查 69 82", ref llc, 0x02,
                        new[] { new byte[] { 0x05, 0x00, 0xB0, 0x81, 0x00, 0x29 } }, 0x69, 0x82, 0x00);
                    break;
                case 20:
                    AddInstructionTransferStep(steps, "选择 ESAM DF01 目录", ref llc, 0x02,
                        new[] { new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xDF, 0x01 } }, 0x90, 0x00, 0x00);
                    AddInstructionTransferStep(steps, "读取 EF02 并检查 69 81", ref llc, 0x02,
                        new[] { new byte[] { 0x05, 0x00, 0xB0, 0x82, 0x00, 0x29 } }, 0x69, 0x81, 0x00);
                    break;
                case 21:
                    AddInstructionTransferStep(steps, "选择 ESAM DF01 目录", ref llc, 0x02,
                        new[] { new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xDF, 0x01 } }, 0x90, 0x00, 0x00);
                    AddInstructionTransferStep(steps, "读取不存在记录并检查 6A 83", ref llc, 0x01,
                        new[] { new byte[] { 0x05, 0x00, 0xB2, 0x3C, 0x14, 0x39 } }, 0x6A, 0x83, 0x00);
                    break;
                case 22:
                    AddInstructionTransferStep(steps, "外部认证引用数据无效并检查 69 84", ref llc, 0x02,
                        new[] { new byte[] { 0x07, 0x00, 0x82, 0x00, 0x00, 0x08, 0x3F, 0x00 } }, 0x69, 0x84, 0x00);
                    break;
                case 23:
                    AddInstructionTransferStep(steps, "未申请随机数更新系统信息并检查 69 84", ref llc, 0x02,
                        new[] { new byte[] { 0x0A, 0x04, 0xD6, 0x81, 0x00, 0x05, 0x11, 0x22, 0x33, 0x44, 0x44 } }, 0x69, 0x84, 0x00);
                    break;
                case 24:
                    AddInstructionTransferStep(steps, "选择 EF04 后更新记录并检查 69 86", ref llc, 0x02,
                        new[]
                        {
                            new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xEF, 0x04 },
                            new byte[] { 0x06, 0x00, 0xD6, 0x01, 0x3A, 0x01, 0xAA }
                        }, 0x69, 0x86, 0x00);
                    break;
                case 25:
                    AddInstructionTransferStep(steps, "申请 4 字节随机数", ref llc, 0x02,
                        new[] { new byte[] { 0x05, 0x00, 0x84, 0x00, 0x00, 0x04 } }, 0x90, 0x00, 0x00);
                    AddInstructionTransferStep(steps, "错误安全报文更新系统信息并检查 69 88", ref llc, 0x02,
                        new[] { new byte[] { 0x0A, 0x04, 0xD6, 0x81, 0x00, 0x05, 0x11, 0x22, 0x33, 0x44, 0x44 } }, 0x69, 0x88, 0x00);
                    break;
                case 26:
                    AddInstructionTransferStep(steps, "越界读取系统信息并检查 6B 00", ref llc, 0x02,
                        new[] { new byte[] { 0x05, 0x00, 0xB0, 0x81, 0x63, 0x29 } }, 0x6B, 0x00, 0x00);
                    break;
                case 27:
                    AddInstructionTransferStep(steps, "无前置命令取响应并检查 6F 00", ref llc, 0x02,
                        new[] { new byte[] { 0x05, 0x00, 0xC0, 0x00, 0x00, 0x24 } }, 0x6F, 0x00, 0x00);
                    break;
                default:
                    AddInstructionTransferStep(steps, "锁卡条件不满足并检查 69 85", ref llc, 0x02,
                        new[] { new byte[] { 0x05, 0x00, 0x59, 0x00, 0x00, 0x01 } }, 0x69, 0x85, 0x00);
                    break;
            }

            steps.Add(new ProtocolSequenceStep("SetMMI", 0x04,
                new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, llc, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0));
            // 与第 1～17 项一致：旧 WJ_Trade_instruction 在单向 EventReport 后仅等待 1 秒。
            steps.Add(new ProtocolSequenceStep("EventReport（单向断链）", 0x05,
                new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, InstructionCaseIntervalMilliseconds));
            return steps;
        }

        /// <summary>
        /// 向响应码测试序列加入一条 TransferChannel，并将 LLC 翻转到下一帧所需值。
        /// </summary>
        /// <param name="steps">待追加的协议步骤。</param>
        /// <param name="name">日志中显示的步骤名称。</param>
        /// <param name="llc">当前帧 LLC；追加后在 F7 和 77 之间翻转。</param>
        /// <param name="channelId">TransferChannel 的 ChannelID，01 为 ICC、02 为 ESAM。</param>
        /// <param name="apdus">按原上位机 DataList 顺序排列、且包含各自长度字节的 APDU。</param>
        /// <param name="status1">期望响应状态第一字节。</param>
        /// <param name="status2">期望响应状态第二字节。</param>
        /// <param name="status3">期望 ReturnStatus。</param>
        private static void AddInstructionTransferStep(
            ICollection<ProtocolSequenceStep> steps,
            string name,
            ref byte llc,
            byte channelId,
            IEnumerable<byte[]> apdus,
            byte status1,
            byte status2,
            byte status3)
        {
            List<byte[]> apduList = new List<byte[]>(apdus);
            List<byte> payload = new List<byte>
            {
                0x08, 0x00, 0x00, 0x01, 0x40, llc, 0x91, 0x05, 0x01, 0x03, 0x18, channelId, (byte)apduList.Count
            };
            foreach (byte[] apdu in apduList)
            {
                payload.AddRange(apdu);
            }

            byte expected1 = status1;
            byte expected2 = status2;
            byte expected3 = status3;
            steps.Add(new ProtocolSequenceStep(
                name,
                0x03,
                payload.ToArray(),
                0,
                false,
                response => HasTransferStatus(response, expected1, expected2, expected3),
                string.Format("响应末尾不是 {0:X2} {1:X2} {2:X2}。", expected1, expected2, expected3)));
            llc = llc == 0xF7 ? (byte)0x77 : (byte)0xF7;
        }

        /// <summary>
        /// 创建旧版 <c>WJ_Trade_DoubleChannelChange</c> 中一个双通道切换子项的完整空口序列。
        /// </summary>
        /// <param name="caseNumber">界面子项编号，取值为 1～4。</param>
        /// <returns>BST/VST、GetSecure、两条跨 ICC/ESAM 的 TransferChannel、SetMMI 及单向 EventReport。</returns>
        /// <exception cref="ArgumentOutOfRangeException">子项编号不在 1～4 范围内时抛出。</exception>
        /// <remarks>
        /// 旧程序每个子项都重新建链，并从当前 LLC 翻转到 F7（GetSecure）、77（首条 Transfer）、F7（第二条
        /// Transfer）及 77（SetMMI）。Transfer 响应必须以 <c>90 00 00</c> 结束；EventReport 不等待响应。
        /// </remarks>
        private static IList<ProtocolSequenceStep> CreateDoubleChannelSwitchingSteps(int caseNumber)
        {
            if (caseNumber < 1 || caseNumber > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(caseNumber), "双通道切换测试仅支持用例 1～4。");
            }

            byte[] firstTransfer;
            byte[] secondTransfer;
            string firstName;
            string secondName;
            switch (caseNumber)
            {
                case 1:
                    firstName = "TransferChannel：ESAM 3F00 读取 EF01";
                    firstTransfer = CreateDoubleChannelTransfer(0x77, 0x02, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00, 0x05, 0x00, 0xB0, 0x81, 0x00, 0x10 });
                    secondName = "TransferChannel：ICC 3F00 读取 0016";
                    secondTransfer = CreateDoubleChannelTransfer(0xF7, 0x01, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00, 0x05, 0x00, 0xB0, 0x96, 0x00, 0x10 });
                    break;
                case 2:
                    firstName = "TransferChannel：ICC 3F00 读取 0016";
                    firstTransfer = CreateDoubleChannelTransfer(0x77, 0x01, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00, 0x05, 0x00, 0xB0, 0x96, 0x00, 0x10 });
                    secondName = "TransferChannel：ESAM DF01 读取 EF04";
                    secondTransfer = CreateDoubleChannelTransfer(0xF7, 0x02, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xDF, 0x01, 0x05, 0x00, 0xB0, 0x84, 0x00, 0x10 });
                    break;
                case 3:
                    firstName = "TransferChannel：ESAM DF01 读取 EF04";
                    firstTransfer = CreateDoubleChannelTransfer(0x77, 0x02, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xDF, 0x01, 0x05, 0x00, 0xB0, 0x84, 0x00, 0x10 });
                    secondName = "TransferChannel：ICC 1001 读取 0015";
                    secondTransfer = CreateDoubleChannelTransfer(0xF7, 0x01, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x10, 0x01, 0x05, 0x00, 0xB0, 0x95, 0x00, 0x10 });
                    break;
                default:
                    firstName = "TransferChannel：ICC 1001 读取 0015";
                    firstTransfer = CreateDoubleChannelTransfer(0x77, 0x01, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x10, 0x01, 0x05, 0x00, 0xB0, 0x95, 0x00, 0x10 });
                    secondName = "TransferChannel：ESAM 3F00 读取 EF01";
                    secondTransfer = CreateDoubleChannelTransfer(0xF7, 0x02, new byte[] { 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00, 0x05, 0x00, 0xB0, 0x81, 0x00, 0x10 });
                    break;
            }

            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("双通道切换 BST", 0x01, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x00, 0x25, 0x00 }, 0),
                new ProtocolSequenceStep("GetSecure", 0x02, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x3B, 0xEF, 0x69, 0xD0, 0x9C, 0x27, 0x08, 0xFA, 0x30, 0x00, 0x00 }, 0),
                new ProtocolSequenceStep(firstName, 0x03, firstTransfer, 0, false, HasSuccessfulTransferStatus, firstName + " 未返回 90 00 00。"),
                new ProtocolSequenceStep(secondName, 0x03, secondTransfer, 0, false, HasSuccessfulTransferStatus, secondName + " 未返回 90 00 00。"),
                new ProtocolSequenceStep("SetMMI", 0x04, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>
        /// 依据旧双通道切换固定头组装仅包含一条 APDU 的 TransferChannel 负载。
        /// </summary>
        /// <param name="llc">旧流程翻转后的 LLC。</param>
        /// <param name="channelId">01 为 ICC，02 为 ESAM。</param>
        /// <param name="apdus">顺序拼接且各自含长度字节的 APDU。</param>
        /// <returns>可直接交给本地类型 0003 的 DSRC TransferChannel 负载。</returns>
        private static byte[] CreateDoubleChannelTransfer(byte llc, byte channelId, byte[] apdus)
        {
            List<byte> payload = new List<byte> { 0x08, 0x00, 0x00, 0x01, 0x40, llc, 0x91, 0x05, 0x01, 0x03, 0x18, channelId, 0x02 };
            payload.AddRange(apdus);
            return payload.ToArray();
        }

        /// <summary>
        /// 创建旧版 <c>WJ_SuTong_ESAMfile</c> 对应的 ICC 0009 保留文件读写步骤。
        /// </summary>
        /// <returns>BST/VST、GetSecure、ICC 1001 目录、读取及写入0009、SetMMI和单向断链步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateReservedFileReadWriteSteps()
        {
            // 原用例对可读写的 ICC 0009 保留文件写入该固定10字节，不复读也不恢复原内容。
            byte[] writeData = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99 };
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("保留文件读写 BST", 0x01,
                    new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00, 0x04, 0xEE, 0x62, 0xBC, 0x02, 0x96, 0x00, 0x01, 0x41, 0x87, 0x29, 0xB0, 0x1A, 0x00, 0x04, 0x00, 0x1C, 0x00, 0x2B, 0x00 }, 0),
                new ProtocolSequenceStep("GetSecure：读取车辆信息", 0x02,
                    new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x3B, 0xD1, 0x5B, 0xE7, 0xB0, 0x6C, 0xDF, 0x97, 0xBE, 0x00, 0x00 }, 0),
                new ProtocolSequenceStep("TransferChannel：选择 ICC 1001 目录", 0x03,
                    new byte[] { 0x6A, 0x20, 0xF1, 0x13, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x10, 0x01 }, 0,
                    false, HasSuccessfulTransferStatus, "选择 ICC 1001 目录未返回 90 00 00。"),
                new ProtocolSequenceStep("TransferChannel：读取 ICC 0009 保留文件", 0x03,
                    new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x05, 0x00, 0xB0, 0x89, 0x00, 0x0A }, 0,
                    false, HasSuccessfulTransferStatus, "读取 ICC 0009 保留文件未返回 90 00 00。"),
                new ProtocolSequenceStep("TransferChannel：写入 ICC 0009 保留文件（00~99）", 0x03,
                    new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x0F, 0x00, 0xD6, 0x89, 0x00, 0x0A,
                        writeData[0], writeData[1], writeData[2], writeData[3], writeData[4], writeData[5], writeData[6], writeData[7], writeData[8], writeData[9] }, 0,
                    false, HasSuccessfulTransferStatus, "写入 ICC 0009 保留文件未返回 90 00 00。"),
                new ProtocolSequenceStep("SetMMI", 0x04,
                    new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05,
                    new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>
        /// 按原 OBU GetSecure 测试检查响应中的 15 01 标识及八字节全零认证器。
        /// </summary>
        /// <param name="responseData">GetSecure 响应数据。</param>
        /// <returns>同时满足标识和认证器规则时返回 true。</returns>
        private static bool HasExpectedGetSecureResponse(byte[] responseData, bool useSm4)
        {
            int authenticatorOffset = useSm4 ? 108 : 100;
            if (responseData == null || responseData.Length < authenticatorOffset + 8
                || responseData[10] != 0x15 || responseData[11] != 0x01)
            {
                return false;
            }

            // 旧程序 SM4 从偏移 108、3DES 从偏移 100 读取认证器。
            return IsZeroRange(responseData, authenticatorOffset, 8);
        }

        /// <summary>
        /// 检查字节数组指定区域是否全部为零。
        /// </summary>
        /// <param name="data">待检查数组。</param>
        /// <param name="offset">起始位置。</param>
        /// <param name="count">检查字节数。</param>
        /// <returns>区域有效且全部为零时返回 true。</returns>
        private static bool IsZeroRange(byte[] data, int offset, int count)
        {
            if (data == null || offset < 0 || count < 0 || offset + count > data.Length)
            {
                return false;
            }

            for (int index = offset; index < offset + count; index++)
            {
                if (data[index] != 0x00)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 创建原 OBU 预读信息测试的 BST 和读取链路。
        /// </summary>
        /// <param name="variant">0 表示预读 0019/0002，1 表示预读 0015，2 表示预读 0012。</param>
        /// <returns>按原上位机顺序排列的预读及后续读取步骤。</returns>
        private static IList<ProtocolSequenceStep> CreatePreReadSteps(int variant)
        {
            byte[][] preReadBsts =
            {
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x03, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x90, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00 },
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x04, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x20, 0x1A, 0x00, 0x2B, 0x00 },
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x05, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x40, 0x1A, 0x00, 0x24, 0x00 }
            };
            int safeVariant = Math.Max(0, Math.Min(preReadBsts.Length - 1, variant));
            Func<byte[], bool> transferValidator = HasSuccessfulTransferStatus;
            byte[] preReadSystemInformation = null;
            byte[] preReadTargetInformation = null;
            Func<byte[], bool> vstValidator = responseData =>
            {
                int targetLength = safeVariant == 0 || safeVariant == 1 ? 43 : 36;
                int requiredLength = safeVariant == 0 ? 88 : 41 + targetLength;
                if (responseData == null || responseData.Length < requiredLength)
                {
                    return false;
                }

                // 原函数直接从 VST 偏移 14 提取 26 字节 ESAM 系统信息，从偏移 41 提取目标 IC 文件。
                preReadSystemInformation = CopyProtocolBytes(responseData, 14, 26);
                preReadTargetInformation = CopyProtocolBytes(responseData, 41, targetLength);
                if (safeVariant == 0)
                {
                    byte[] balance = CopyProtocolBytes(responseData, 84, 4);
                    byte[] combinedTarget = new byte[47];
                    Buffer.BlockCopy(preReadTargetInformation, 0, combinedTarget, 0, 43);
                    Buffer.BlockCopy(balance, 0, combinedTarget, 43, 4);
                    preReadTargetInformation = combinedTarget;
                }
                return true;
            };
            Func<byte[], bool> systemInformationValidator = responseData =>
                HasSuccessfulTransferStatus(responseData)
                && preReadSystemInformation != null
                && HasProtocolBytes(responseData, 14, preReadSystemInformation);
            List<ProtocolSequenceStep> steps = new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("预读 BST", 0x01, preReadBsts[safeVariant], 0, false, vstValidator, "VST 长度不足，无法提取原上位机规定的预读字段。"),
                new ProtocolSequenceStep("TransferChannel：进入 ESAM 3F00", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x01, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00 }, 0, false, transferValidator, "响应末尾不是 90 00 00。"),
                new ProtocolSequenceStep("TransferChannel：读取 ESAM 系统信息并与预读值比较", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x01, 0x05, 0x00, 0xB0, 0x81, 0x00, 0x1A }, 0, false, systemInformationValidator, "ESAM EF01 实读的26字节系统信息与 VST 预读值不一致，或响应状态异常。"),
                new ProtocolSequenceStep("TransferChannel：选择 IC 1001 目录", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x10, 0x01 }, 0, false, transferValidator, "响应末尾不是 90 00 00。")
            };

            // 原 OBU 上位机三个预读用例的目标文件不同，不应在每个用例里把 0019、0015、0012 全部读取。
            if (safeVariant == 0)
            {
                byte[] actual0019 = null;
                steps.Add(new ProtocolSequenceStep("TransferChannel：读取 0019", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x05, 0x00, 0xB2, 0x01, 0xCC, 0x2B }, 0, false,
                    responseData => HasSuccessfulTransferStatus(responseData) && TryCopyProtocolBytes(responseData, 14, 43, out actual0019), "0019 响应长度不足或状态异常。"));
                steps.Add(new ProtocolSequenceStep("TransferChannel：读取余额并与预读0019/0002比较", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x05, 0x80, 0x5C, 0x00, 0x02, 0x04 }, 0, false,
                    responseData => HasSuccessfulTransferStatus(responseData)
                        && actual0019 != null
                        && preReadTargetInformation != null
                        && HasProtocolBytes(preReadTargetInformation, 0, actual0019)
                        && HasProtocolBytes(responseData, 14, CopyProtocolBytes(preReadTargetInformation, 43, 4)),
                    "实读0019或4字节余额与 VST 预读值不一致，或响应状态异常。"));
            }
            else if (safeVariant == 1)
            {
                steps.Add(new ProtocolSequenceStep("TransferChannel：读取 0015 并与预读值比较", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x05, 0x00, 0xB0, 0x95, 0x00, 0x2B }, 0, false,
                    responseData => HasSuccessfulTransferStatus(responseData) && preReadTargetInformation != null && HasProtocolBytes(responseData, 14, preReadTargetInformation),
                    "实读0015的43字节内容与 VST 预读值不一致，或响应状态异常。"));
            }
            else
            {
                steps.Add(new ProtocolSequenceStep("TransferChannel：读取 0012 并与预读值比较", 0x03, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x05, 0x00, 0xB0, 0x92, 0x00, 0x24 }, 0, false,
                    responseData => HasSuccessfulTransferStatus(responseData) && preReadTargetInformation != null && HasProtocolBytes(responseData, 14, preReadTargetInformation),
                    "实读0012的36字节内容与 VST 预读值不一致，或响应状态异常。"));
            }

            steps.Add(new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 2000));
            return steps;
        }

        /// <summary>复制协议帧中由原上位机固定偏移定义的数据字段。</summary>
        /// <param name="source">完整 VST 或 TransferChannel 响应。</param><param name="offset">原上位机使用的零基偏移。</param><param name="length">字段字节数。</param>
        /// <returns>独立复制的数据字段。</returns>
        private static byte[] CopyProtocolBytes(byte[] source, int offset, int length)
        {
            byte[] value = new byte[length];
            Buffer.BlockCopy(source, offset, value, 0, length);
            return value;
        }

        /// <summary>在响应长度满足要求时复制指定协议字段。</summary>
        /// <param name="source">完整协议响应。</param><param name="offset">字段偏移。</param><param name="length">字段长度。</param><param name="value">成功时返回独立字段副本。</param>
        /// <returns>字段范围完整时返回 true。</returns>
        private static bool TryCopyProtocolBytes(byte[] source, int offset, int length, out byte[] value)
        {
            value = null;
            if (source == null || offset < 0 || length < 0 || offset + length > source.Length) return false;
            value = CopyProtocolBytes(source, offset, length);
            return true;
        }

        /// <summary>逐字节比较响应指定偏移处的数据与预读数据。</summary>
        /// <param name="source">完整响应或已提取的数据。</param><param name="offset">比较起始偏移。</param><param name="expected">VST 预读的期望字节。</param>
        /// <returns>范围完整且全部字节一致时返回 true。</returns>
        private static bool HasProtocolBytes(byte[] source, int offset, byte[] expected)
        {
            if (source == null || expected == null || offset < 0 || offset + expected.Length > source.Length) return false;
            for (int index = 0; index < expected.Length; index++) if (source[offset + index] != expected[index]) return false;
            return true;
        }

        /// <summary>
        /// 从原 OBU 模块使用的 BST_Locked_Sutong.ini 读取 255S 保持测试配置。
        /// </summary>
        /// <returns>每个配置 BST 对应一个协议步骤；判定字节为 01 的步骤要求 OBU 不回复 VST。</returns>
        private static IList<ProtocolSequenceStep> Create255KeepSteps()
        {
            string path = ResolveRuntimeFilePath("BST_Locked_Sutong.ini");
            List<ProtocolSequenceStep> steps = new List<ProtocolSequenceStep>();
            int count = ReadIniInteger(path, "BST_Locked_Sutong", "number", 0);
            for (int index = 0; index < count; index++)
            {
                string frameText = ReadIniValue(path, "BST_Locked_Sutong", (index * 2 + 1).ToString());
                string judgeText = ReadIniValue(path, "BST_Locked_Sutong", (index * 2 + 2).ToString());
                byte[] frame = ParseHexBytes(frameText);
                byte[] judge = ParseHexBytes(judgeText);
                if (frame.Length == 0 || judge.Length == 0 || (judge[0] != 0x00 && judge[0] != 0x01))
                {
                    continue;
                }

                // 原上位机判定字节 00 表示应进入交易并回复 VST，01 表示 BST 无效且不应回复。
                steps.Add(new ProtocolSequenceStep(
                    string.Format("255S 配置 BST {0}（期望判定 {1}）", index + 1, judge[0] == 0x00 ? "交易" : "不交易"),
                    0x01,
                    frame,
                    judge[0] == 0x01 ? 2000 : 0,
                    judge[0] == 0x01));
            }

            if (steps.Count == 0)
            {
                throw new InvalidOperationException("BST_Locked_Sutong.ini 未提供可执行的 255S 保持测试配置。");
            }

            return steps;
        }

        /// <summary>
        /// 定位 exe 同级运行配置；设计器/测试宿主基目录不同时回退到程序集所在目录。
        /// </summary>
        /// <param name="fileName">运行配置文件名。</param>
        /// <returns>优先返回 exe 同级路径；无法判断时返回当前应用基目录路径。</returns>
        private static string ResolveRuntimeFilePath(string fileName)
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(basePath))
            {
                return basePath;
            }

            string assemblyDirectory = Path.GetDirectoryName(typeof(WanjiStandardTestForm).Assembly.Location);
            if (!string.IsNullOrEmpty(assemblyDirectory))
            {
                string assemblyPath = Path.Combine(assemblyDirectory, fileName);
                if (File.Exists(assemblyPath))
                {
                    return assemblyPath;
                }
            }

            return basePath;
        }

        /// <summary>
        /// 创建 OBU 信道选择测试的 BST/VST 和释放链路步骤。
        /// </summary>
        /// <param name="channel">0 或 1，表示原上位机选择的物理信道。</param>
        /// <returns>对应信道的协议步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateChannelSteps(int channel)
        {
            byte[] bst = channel == 0
                ? new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00, 0x05, 0xD0, 0x62, 0xBB, 0xFA, 0x89, 0x00, 0x01, 0x41, 0x35, 0x00 }
                : new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00, 0x05, 0xCF, 0x62, 0xBB, 0xFA, 0x73, 0x01, 0x01, 0x41, 0x87, 0x29, 0xB0, 0x1A, 0x00, 0x04, 0x00, 0x1C, 0x00, 0x2B, 0x00 };
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep(string.Format("信道 {0} BST", channel), 0x01, bst, 0),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>
        /// 创建不同 GetSecure 参数组合的测试步骤。
        /// </summary>
        /// <param name="variant">1～4，对应原上位机四种许可/密钥标识组合。</param>
        /// <returns>BST、GetSecure、SetMMI 和 EventReport 步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateGetSecureSteps(int variant, bool useSm4)
        {
            byte[][] getSecureFrames =
            {
                new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x4F, 0x14, 0xAD, 0xFB, 0xE4, 0x8B, 0xF6, 0xBC, 0xC7, 0x40, 0x43 },
                new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x4F, 0x14, 0xAD, 0xFB, 0xE4, 0x8B, 0xF6, 0xBC, 0xC7, 0x40 },
                new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x0D, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x4F, 0x85, 0x71, 0x3C, 0x48, 0x23, 0x13, 0x1F, 0xC8, 0x40, 0x43 },
                new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x0D, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x4F, 0x14, 0xEF, 0x8A, 0xFD, 0x8C, 0xD2, 0xD5, 0x78, 0x40 }
            };
            int safeVariant = Math.Max(1, Math.Min(4, variant));
            byte[] getSecure = (byte[])getSecureFrames[safeVariant - 1].Clone();
            if (!useSm4)
            {
                // 旧 WJ_Trade_DiffGetsecure 在 3DES 模式把末尾 SM4 能力/算法标识清零。
                getSecure[getSecure.Length - 1] = 0x00;
                if (safeVariant == 1 || safeVariant == 3) getSecure[getSecure.Length - 2] = 0x00;
            }
            Func<byte[], bool> validator = response => HasExpectedGetSecureResponse(response, useSm4);
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("GetSecure 测试 BST", 0x01, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0xB0, 0x1A, 0x00, 0x04, 0x00, 0x1C, 0x00, 0x2B, 0x00 }, 0),
                new ProtocolSequenceStep("GetSecure", 0x02, getSecure, 0, false, validator, "响应的 15 01 标识或当前算法对应的八字节全零认证器不符合原测试标准。"),
                new ProtocolSequenceStep("SetMMI", 0x04, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>
        /// 创建 TransferChannel APDU 数量测试步骤。
        /// </summary>
        /// <param name="apduCount">连续相同 APDU 的数量。</param>
        /// <param name="esam">是否使用 ESAM 取随机数 APDU。</param>
        /// <returns>BST、TransferChannel、SetMMI 和 EventReport 步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateTransferSteps(int apduCount, bool esam)
        {
            apduCount = Math.Max(1, Math.Min(9, apduCount));
            byte[] apdu = esam ? new byte[] { 0x05, 0x00, 0x84, 0x00, 0x00, 0x04 } : new byte[] { 0x05, 0x80, 0x5C, 0x00, 0x02, 0x04 };
            List<byte> transfer = new List<byte>(16 + apduCount * apdu.Length)
            {
                0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18,
                (byte)(esam ? 0x02 : 0x01), (byte)apduCount
            };
            for (int index = 0; index < apduCount; index++)
            {
                transfer.AddRange(apdu);
            }
            int expectedCount = apduCount;
            Func<byte[], bool> validator = response => HasSuccessfulTransferResponse(response, expectedCount);

            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("TransferChannel 测试 BST", 0x01, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x00, 0x25, 0x00 }, 0),
                new ProtocolSequenceStep(string.Format("TransferChannel：{0} 条{1} APDU", apduCount, esam ? "ESAM" : "IC"), 0x03, transfer.ToArray(), 0, false, validator, "DataList 数量、某条 APDU 的 90 00 状态或 ReturnStatus 不符合要求。"),
                new ProtocolSequenceStep("SetMMI", 0x04, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>
        /// 创建原 OBU TransferChannel 第 11 或第 12 项的三条不同 APDU 测试步骤。
        /// </summary>
        /// <param name="esam">false 表示 IC 的余额/0015/0012，true 表示 ESAM 的 3F00/EF01/芯片序列号。</param>
        /// <returns>包含 BST、三条不同 APDU、SetMMI 和单向 EventReport 的步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateMixedTransferSteps(bool esam)
        {
            byte[] transfer = esam
                ? new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x03, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3F, 0x00, 0x05, 0x00, 0xB0, 0x81, 0x00, 0x29, 0x05, 0x80, 0xF6, 0x00, 0x03, 0x04 }
                : new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x03, 0x05, 0x80, 0x5C, 0x00, 0x02, 0x04, 0x05, 0x00, 0xB0, 0x95, 0x00, 0x2B, 0x05, 0x00, 0xB0, 0x92, 0x00, 0x10 };
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("TransferChannel 测试 BST", 0x01, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x00, 0x25, 0x00 }, 0),
                new ProtocolSequenceStep(esam ? "TransferChannel：三条不同 ESAM APDU" : "TransferChannel：三条不同 IC APDU", 0x03, transfer, 0, false, response => HasSuccessfulTransferResponse(response, 3), "DataList 数量、某条 APDU 的 90 00 状态或 ReturnStatus 不符合要求。"),
                new ProtocolSequenceStep("SetMMI", 0x04, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, 0),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>
        /// 创建 SetMMI 参数测试步骤。
        /// </summary>
        /// <param name="parameter">原上位机使用的 00～04 参数值。</param>
        /// <returns>BST、GetSecure、SetMMI 和 EventReport 步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateSetMmiSteps(int parameter)
        {
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("SetMMI 测试 BST", 0x01, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x01, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x00, 0x25, 0x00 }, 0),
                new ProtocolSequenceStep("GetSecure 前置交互", 0x02, new byte[] { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x3B, 0xDB, 0x27, 0x0D, 0xCD, 0xDB, 0xC6, 0x9D, 0xA7, 0x00, 0x00 }, 0),
                new ProtocolSequenceStep("SetMMI 参数 " + parameter.ToString("X2"), 0x04, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x04, 0x1A, (byte)parameter }, 3000),
                new ProtocolSequenceStep("EventReport（单向断链）", 0x05, new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 }, 1000)
            };
        }

        /// <summary>在后台执行旧 WJ_ProtocolTest_Link1 的 1～5 帧拼帧轮次。</summary>
        /// <param name="cancellationToken">停止按钮对应的取消令牌。</param>
        /// <returns>包含各轮组合结果及全部实际收发帧的执行结果。</returns>
        private ProtocolSequenceExecutionResult ExecuteLinkFrameRounds(CancellationToken cancellationToken)
        {
            byte[] bst = { 0xFF,0xFF,0xFF,0xFF,0x50,0x03,0x91,0xC0,0x08,0,0,1,0x54,0xC1,0x1C,2,0,1,0x41,0x87,0x29,0,0x25,0 };
            byte[] eventReport = { 0x08,0,0,1,0x40,0x03,0x91,0x60,0,0 };
            byte[][] heads =
            {
                new byte[] { 0x80,0,0,0,0x40,0x77,0x91,5,1,0,0x14,0x80,1,0,0,7,1,2,3,4,5,6,7,8,0,0 },
                new byte[] { 0x80,0,0,0,0x40,0x77,0x91,5,1,3,0x18,1,1,5,0,0xB0,0x95,0,4 },
                new byte[] { 0x80,0,0,0,0x40,0x77,0x91,5,1,3,0x18,2,1,5,0x80,0xF6,0,3,4 },
                new byte[] { 0x80,0,0,0,0x40,0x77,0x91,5,1,4,0x1A,0 },
                new byte[] { 0x80,0,0,0,0x40,0x77,0x91,4,1,2,8 }
            };
            byte[][] tails =
            {
                new byte[] { 0x91,5,1,0,0x14,0x80,1,0,0,7,1,2,3,4,5,6,7,8,0,0 },
                new byte[] { 0x91,5,1,3,0x18,1,1,5,0,0xB0,0x95,0,4 },
                new byte[] { 0x91,5,1,3,0x18,2,1,5,0x80,0xF6,0,3,4 },
                new byte[] { 0x91,5,1,4,0x1A,0 },
                new byte[] { 0x91,4,1,2,8 }
            };
            byte[] commandTypes = { 2,3,3,4,6 };
            int[] responseLengths = { 30,14,14,5,13 };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int sent = 0, skipped = 0, failed = 0;
            try
            {
                for (int frameCount = 1; frameCount <= 5; frameCount++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int total = IntegerPower(5, frameCount);
                    AppendProtocolTestInformation(string.Format("拼帧第 {0}/5 轮开始：遍历 {1} 种组合。\r\n", frameCount, total));
                    // 每轮开始只建链一次；正常组合沿用当前会话。
                    TransparentCommandResult bstResult = _desktopCommService.ExecuteTransparent(BuildTypedCommand(1, bst), BstCompatibilityTimeoutMilliseconds);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
                    if (!bstResult.IsSuccess) return new ProtocolSequenceExecutionResult(false, "第 " + frameCount + " 轮建链失败：" + bstResult.ErrorMessage, exchanges, DateTime.Now);

                    int roundSent = 0, roundSkipped = 0;
                    for (int number = 0; number < total; number++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int[] indexes = DecodeLinkCombination(number, frameCount);
                        int expectedLength = 0;
                        for (int index = 0; index < indexes.Length; index++) expectedLength += responseLengths[indexes[index]];
                        if (expectedLength > 100) { skipped++; roundSkipped++; continue; }

                        byte[] payload = JoinLinkFrames(indexes, heads, tails);
                        TransparentCommandResult result = _desktopCommService.ExecuteTransparent(BuildTypedCommand(commandTypes[indexes[0]], payload), BstCompatibilityTimeoutMilliseconds);
                        exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                        sent++; roundSent++;
                        if (!result.IsSuccess)
                        {
                            failed++;
                            AppendProtocolTestInformation(string.Format("第 {0} 轮组合 {1} 失败：{2}\r\n", frameCount, FormatLinkCombination(indexes), result.ErrorMessage));
                            // 旧流程只在组合失败时断链、等待 2 秒并重新建链后继续。
                            _desktopCommService.ExecuteTransparent(BuildTypedCommand(5, eventReport), BstCompatibilityTimeoutMilliseconds);
                            if (cancellationToken.WaitHandle.WaitOne(2000)) cancellationToken.ThrowIfCancellationRequested();
                            bstResult = _desktopCommService.ExecuteTransparent(BuildTypedCommand(1, bst), BstCompatibilityTimeoutMilliseconds);
                            exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
                            if (!bstResult.IsSuccess) return new ProtocolSequenceExecutionResult(false, "失败恢复建链失败：" + bstResult.ErrorMessage, exchanges, DateTime.Now);
                        }
                    }

                    _desktopCommService.ExecuteTransparent(BuildTypedCommand(5, eventReport), BstCompatibilityTimeoutMilliseconds);
                    AppendProtocolTestInformation(string.Format("拼帧第 {0}/5 轮完成：下发 {1}，超长跳过 {2}。\r\n", frameCount, roundSent, roundSkipped));
                    if (cancellationToken.WaitHandle.WaitOne(3000)) cancellationToken.ThrowIfCancellationRequested();
                }

                string message = string.Format("拼帧五轮完成：下发 {0} 个组合，超长跳过 {1} 个，失败 {2} 个。", sent, skipped, failed);
                return new ProtocolSequenceExecutionResult(failed == 0 && sent > 0, message, exchanges, DateTime.Now);
            }
            catch (OperationCanceledException) { return new ProtocolSequenceExecutionResult(false, "拼帧测试已由用户停止。", exchanges, DateTime.Now); }
            catch (Exception exception) { return new ProtocolSequenceExecutionResult(false, "拼帧测试执行失败：" + exception.Message, exchanges, DateTime.Now); }
        }

        /// <summary>计算拼帧轮次的五进制组合总数。</summary>
        /// <param name="value">底数。</param><param name="exponent">非负指数。</param>
        /// <returns>整数幂结果。</returns>
        private static int IntegerPower(int value, int exponent)
        {
            int result = 1;
            for (int index = 0; index < exponent; index++) result *= value;
            return result;
        }

        /// <summary>按旧 k0～k4 的顺序解码五进制组合编号。</summary>
        /// <param name="number">从零开始的组合编号。</param><param name="frameCount">当前轮拼接帧数。</param>
        /// <returns>从首帧到末帧的类型下标。</returns>
        private static int[] DecodeLinkCombination(int number, int frameCount)
        {
            int[] indexes = new int[frameCount];
            for (int index = frameCount - 1; index >= 0; index--) { indexes[index] = number % 5; number /= 5; }
            return indexes;
        }

        /// <summary>拼接一个完整首帧和后续无公共头帧片段。</summary>
        /// <param name="indexes">各帧类型下标。</param><param name="heads">五种完整首帧。</param><param name="tails">五种后续帧片段。</param>
        /// <returns>一次交给 DLL 下发的拼帧缓冲区。</returns>
        private static byte[] JoinLinkFrames(int[] indexes, byte[][] heads, byte[][] tails)
        {
            List<byte> bytes = new List<byte>(heads[indexes[0]]);
            for (int index = 1; index < indexes.Length; index++) bytes.AddRange(tails[indexes[index]]);
            return bytes.ToArray();
        }

        /// <summary>生成与旧日志一致的连续类型编号。</summary>
        /// <param name="indexes">各帧类型下标。</param>
        /// <returns>例如 0、12 或 30421。</returns>
        private static string FormatLinkCombination(int[] indexes)
        {
            StringBuilder builder = new StringBuilder(indexes.Length);
            for (int index = 0; index < indexes.Length; index++) builder.Append(indexes[index]);
            return builder.ToString();
        }

        /// <summary>
        /// 创建“获取车辆信息后发送 BST”用例的协议步骤。
        /// </summary>
        /// <returns>首次建链、GetSecure 及第二次广播 BST 无响应判定步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateRespondBroadcastAfterVehicleInfoSteps()
        {
            byte[] bst = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00, 0x05, 0x95, 0x62, 0xBB, 0xF7, 0xC7, 0x00, 0x01, 0x41, 0x87, 0x29, 0xB0, 0x1A, 0x00, 0x04, 0x00, 0x1C, 0x00, 0x2B, 0x00 };
            byte[] getSecure = { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x3B, 0xC9, 0x94, 0x87, 0x34, 0x3C, 0x08, 0xDE, 0xE9, 0x00, 0x00 };
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("建立会话 BST/VST", 0x01, bst, 0),
                new ProtocolSequenceStep("GetSecure：读取车辆信息", 0x02, getSecure, 0),
                new ProtocolSequenceStep("再次发送广播 BST，OBU 不应回复 VST", 0x01, bst, 3000, true)
            };
        }

        /// <summary>
        /// 创建“消费初始化后发送 BST”用例的协议步骤。
        /// </summary>
        /// <param name="useSm4">为 true 时使用 SM4 的 41 参数，否则使用 3DES 的 01 参数。</param>
        /// <returns>首次建链、GetSecure、消费初始化及第二次广播 BST 无响应判定步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateRespondBroadcastAfterPurchaseInitializationSteps(bool useSm4)
        {
            List<ProtocolSequenceStep> steps = new List<ProtocolSequenceStep>(CreateRespondBroadcastAfterVehicleInfoSteps());
            steps.RemoveAt(steps.Count - 1);
            byte[] purchaseInitialization = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x10, 0x80, 0x50, 0x03, 0x02, 0x0B, 0x01, 0x00, 0x00, 0x00, 0x01, 0x37, 0x37, 0x37, 0x37, 0x37, 0x37 };
            purchaseInitialization[19] = useSm4 ? (byte)0x41 : (byte)0x01;
            steps.Add(new ProtocolSequenceStep("TransferChannel：消费初始化", 0x03, purchaseInitialization, 0, false, HasSuccessfulTransferStatus, "消费初始化响应末尾不是 90 00 00。"));
            byte[] bst = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00, 0x05, 0x95, 0x62, 0xBB, 0xF7, 0xC7, 0x00, 0x01, 0x41, 0x87, 0x29, 0xB0, 0x1A, 0x00, 0x04, 0x00, 0x1C, 0x00, 0x2B, 0x00 };
            steps.Add(new ProtocolSequenceStep("消费初始化后发送广播 BST，OBU 不应回复 VST", 0x01, bst, 3000, true));
            return steps;
        }

        /// <summary>
        /// 创建读取车辆信息后发送错误 MAC TransferChannel 的负向测试步骤。
        /// </summary>
        /// <param name="macVariant">错误 MAC 变体：1 表示末字节加一，2 表示全 FF，3 表示第三字节加一。</param>
        /// <returns>BST/VST、GetSecure 以及应当无响应的错误 MAC TransferChannel 步骤。</returns>
        private static IList<ProtocolSequenceStep> CreateDifferentMacTransferSteps(int macVariant)
        {
            byte[] bst = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x0E, 0x00, 0x05, 0x9F, 0x62, 0xBB, 0xF8, 0x24, 0x00, 0x01, 0x41, 0x87, 0x29, 0xB0, 0x1A, 0x00, 0x04, 0x00, 0x1C, 0x00, 0x2B, 0x00 };
            byte[] getSecure = { 0x66, 0x90, 0x53, 0x9C, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x3B, 0xDB, 0x27, 0x0D, 0xCD, 0xDB, 0xC6, 0x9D, 0xA7, 0x00, 0x00 };
            byte[] transfer = { 0x66, 0x90, 0x53, 0x9D, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x05, 0x00, 0xB0, 0x95, 0x00, 0x2B };
            return new List<ProtocolSequenceStep>
            {
                new ProtocolSequenceStep("建立会话 BST/VST", 0x01, bst, 0),
                new ProtocolSequenceStep("GetSecure：读取车辆信息", 0x02, getSecure, 0),
                new ProtocolSequenceStep("发送错误 MAC 的 TransferChannel，OBU 不应回复", 0x03, transfer, 2000, true, null, "OBU 响应了错误 MAC 的 TransferChannel。", true, macVariant)
            };
        }

        /// <summary>
        /// 创建原 WJ_ProtocolTest_SingleFrame 单帧唤醒测试的全部 BST/VST 轮次。
        /// </summary>
        /// <returns>次数取自 exe 同级 SetMe.ini 的 SET_DANZHEN/number，每轮使用原 SendBST_GB 帧并间隔 2000 ms。</returns>
        private static IList<ProtocolSequenceStep> CreateSingleFrameWakeSteps()
        {
            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
            int configuredCount = ReadIniInteger(configurationPath, "SET_DANZHEN", "number", 30);
            byte[] bst =
            {
                0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02,
                0x00, 0x01, 0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00,
                0x04, 0x00, 0x2B, 0x00
            };
            List<ProtocolSequenceStep> steps = new List<ProtocolSequenceStep>(configuredCount);
            for (int index = 0; index < configuredCount; index++)
            {
                // 每轮均克隆原 SendBST_GB 模板；动态 BID/UnixTime 仍按当前台发初始化勾选项处理。
                steps.Add(new ProtocolSequenceStep(
                    string.Format("单帧唤醒第 {0}/{1} 轮 BST/VST", index + 1, configuredCount),
                    0x01,
                    (byte[])bst.Clone(),
                    2000));
            }
            return steps;
        }

        /// <summary>
        /// 创建原 WJ_RFSendDataTest 的 73 通道 55AA 射频收发测试步骤。
        /// </summary>
        /// <returns>按 SET_SEND_RF 次数生成会话；每个会话含50组01/02指令、SetMMI和配置间隔。</returns>
        private static IList<ProtocolSequenceStep> CreateRfSendDataSteps()
        {
            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
            int configuredCount = ReadIniInteger(configurationPath, "SET_SEND_RF", "number", 10);
            int cycleInterval = ReadIniInteger(configurationPath, "SET_Trade_GB", "interval", 0);
            byte[] bst =
            {
                0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02,
                0x00, 0x01, 0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00,
                0x04, 0x00, 0x2B, 0x00
            };
            List<ProtocolSequenceStep> steps = new List<ProtocolSequenceStep>();
            for (int cycle = 0; cycle < configuredCount; cycle++)
            {
                steps.Add(new ProtocolSequenceStep(
                    string.Format("55AA 第 {0}/{1} 轮 BST/VST", cycle + 1, configuredCount),
                    0x01, (byte[])bst.Clone(), 0));
                byte llc = 0xF7;
                for (int pair = 0; pair < 50; pair++)
                {
                    byte[] command01 = { 0x08, 0x00, 0x00, 0x01, 0x40, llc, 0x91, 0x05, 0x01, 0x03, 0x18, 0x73, 0x01, 0x01, 0x01 };
                    steps.Add(new ProtocolSequenceStep(
                        string.Format("第 {0} 轮第 {1}/50 组：73通道01返回55AA", cycle + 1, pair + 1),
                        0x03, command01, 0, false,
                        response => HasAlternatingRfPayload(response, 0x55, 0xAA),
                        "73通道01未返回108字节交替55AA及90 00 00。"));
                    llc = llc == 0xF7 ? (byte)0x77 : (byte)0xF7;

                    byte[] command02 = { 0x08, 0x00, 0x00, 0x01, 0x40, llc, 0x91, 0x05, 0x01, 0x03, 0x18, 0x73, 0x01, 0x01, 0x02 };
                    steps.Add(new ProtocolSequenceStep(
                        string.Format("第 {0} 轮第 {1}/50 组：73通道02返回AA55", cycle + 1, pair + 1),
                        0x03, command02, 0, false,
                        response => HasAlternatingRfPayload(response, 0xAA, 0x55),
                        "73通道02未返回108字节交替AA55及90 00 00。"));
                    llc = llc == 0xF7 ? (byte)0x77 : (byte)0xF7;
                }
                steps.Add(new ProtocolSequenceStep("SetMMI", 0x04,
                    new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, llc, 0x91, 0x05, 0x01, 0x04, 0x1A, 0x00 }, cycleInterval));
            }
            return steps;
        }

        /// <summary>
        /// 校验73通道响应从偏移14开始的108字节是否按指定两个字节交替，并检查90 00 00结尾。
        /// </summary>
        /// <param name="response">TransferChannel完整响应。</param>
        /// <param name="first">交替数据偶数位置期望值。</param>
        /// <param name="second">交替数据奇数位置期望值。</param>
        /// <returns>长度、108字节模式和三字节成功状态全部正确时返回true。</returns>
        private static bool HasAlternatingRfPayload(byte[] response, byte first, byte second)
        {
            if (response == null || response.Length < 125 || !HasTransferStatus(response, 0x90, 0x00, 0x00))
            {
                return false;
            }
            for (int index = 0; index < 108; index++)
            {
                byte expected = (index & 1) == 0 ? first : second;
                if (response[14 + index] != expected)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 读取简单 INI 节中的整数键值。
        /// </summary>
        /// <param name="path">INI 文件路径。</param>
        /// <param name="sectionName">节名。</param>
        /// <param name="key">键名。</param>
        /// <param name="defaultValue">读取失败时的默认值。</param>
        /// <returns>解析到的整数。</returns>
        private static int ReadIniInteger(string path, string sectionName, string key, int defaultValue)
        {
            int value;
            return int.TryParse(ReadIniValue(path, sectionName, key), out value) && value > 0 ? value : defaultValue;
        }

        /// <summary>
        /// 读取简单 INI 节中的字符串键值，并去除行尾注释。
        /// </summary>
        /// <param name="path">INI 文件路径。</param>
        /// <param name="sectionName">节名。</param>
        /// <param name="key">键名。</param>
        /// <returns>键值；文件、节或键不存在时返回空字符串。</returns>
        private static string ReadIniValue(string path, string sectionName, string key)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            string currentSection = string.Empty;
            foreach (string rawLine in File.ReadAllLines(path, Encoding.Default))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    int closeIndex = line.IndexOf(']');
                    currentSection = closeIndex > 1 ? line.Substring(1, closeIndex - 1).Trim() : string.Empty;
                    continue;
                }

                if (!string.Equals(currentSection, sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int separator = line.IndexOf('=');
                if (separator <= 0 || !string.Equals(line.Substring(0, separator).Trim(), key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return line.Substring(separator + 1).Split(new[] { '/', ';' }, 2)[0].Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// 将连续十六进制字符转换为协议字节数组。
        /// </summary>
        /// <param name="hexText">不含分隔符或含空白分隔符的十六进制文本。</param>
        /// <returns>解析结果；非法或奇数长度文本返回空数组。</returns>
        private static byte[] ParseHexBytes(string hexText)
        {
            if (string.IsNullOrWhiteSpace(hexText))
            {
                return new byte[0];
            }

            string compact = hexText.Replace(" ", string.Empty).Replace("\t", string.Empty).Replace("-", string.Empty);
            if ((compact.Length & 1) != 0)
            {
                return new byte[0];
            }

            byte[] result = new byte[compact.Length / 2];
            for (int index = 0; index < result.Length; index++)
            {
                byte value;
                if (!byte.TryParse(compact.Substring(index * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out value))
                {
                    return new byte[0];
                }

                result[index] = value;
            }

            return result;
        }

        /// <summary>
        /// 执行 OBU 模块的 VST 随机避让测试，不复用 BST 兼容性测试的随机 BST/VST 计数流程。
        /// </summary>
        /// <param name="count">从 SetMe.ini 读取的随机避让循环次数；必须为正数。</param>
        private void RunVstRandomDodgeTest(int count)
        {
            if (count <= 0)
            {
                count = 15;
            }

            const string testCaseKey = "通用测试项 > 2、VST随机避让测试 > 测试用例1：无间隔发送BST";
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? testCaseKey : selectedNode.Text;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                // 没有共享台发服务时直接保存失败结果，避免后台任务访问空服务。
                string unavailableMessage = "未连接主窗体台发服务，无法执行 VST 随机避让测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            // 独立入口启动时间窗统计流程，保持测试按钮和停止按钮在 DLL 等待期间可响应。
            Task.Run(() => ExecuteVstRandomDodgeTest(count, cancellationToken)).ContinueWith(
                completedTask =>
                {
                    if (IsDisposed || !IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        BeginInvoke(new Action(() => FinishVstRandomDodgeTask(testCaseName, startedAt, completedTask)));
                    }
                    catch (InvalidOperationException)
                    {
                        // 窗体关闭过程中不再回写 UI，后台通信结果由统一 Dispose 流程结束。
                    }
                },
                TaskScheduler.Default);
        }

        /// <summary>
        /// 执行 OBU 随机避让流程并仅按三个目标时间窗的累计数量形成最终判定。
        /// </summary>
        /// <param name="count">测试轮数；由 SetMe.ini 的 number 配置读取，必须为正数。</param>
        /// <param name="cancellationToken">测试大框停止按钮提供的取消令牌；每次 DLL 调用前检查。</param>
        /// <returns>包含三段时间窗计数、完整交互帧和最终通过状态的执行结果。</returns>
        private VstRandomDodgeExecutionResult ExecuteVstRandomDodgeTest(
            int count,
            CancellationToken cancellationToken)
        {
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int time3 = 0;
            int time6 = 0;
            int time9 = 0;
            int timeElse = 0;
            int completedCount = 0;
            try
            {
                AppendProtocolTestInformation("开始执行：VST随机避让时间窗测试（原 OBU 流程）\r\n");
                byte[] eventReport = new byte[] { 0x08, 0x00, 0x00, 0x01, 0x40, 0x03, 0x91, 0x60, 0x00, 0x00 };
                for (int cycleIndex = 0; cycleIndex < count; cycleIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    byte[] bstFrame = CreateVstRandomDodgeBstFrame();
                    // 界面入口仅显示轮次；完整 BST/VST 数据由 exchanges 写入勾选后的结果文档。
                    AppendProtocolTestInformation(
                        string.Format("第 {0}/{1} 轮：正在执行 BST/VST 交互。\r\n", cycleIndex + 1, count));

                    // 先完成本轮 BST/VST，后续时间窗采样必须使用本轮建立的 OBU 会话。
                    TransparentCommandResult bstResult = _desktopCommService.ExecuteTransparent(
                        BuildTypedCommand(0x01, bstFrame), BstCompatibilityTimeoutMilliseconds);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
                    if (bstResult.IsSuccess)
                    {
                        AppendProtocolTestInformation("BST/VST 交互完成。\r\n");
                    }
                    if (!bstResult.IsSuccess)
                    {
                        AppendProtocolTestInformation(
                            string.Format("第 {0}/{1} 轮 BST/VST 通信异常，继续统计时间窗。\r\n", cycleIndex + 1, count));
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    // EventReport 只执行台发发送函数，不调用接收函数；这是原规则中的单向断链动作。
                    TransparentCommandResult eventResult = _desktopCommService.ExecuteTransparent(
                        BuildTypedCommand(0x05, eventReport), BstCompatibilityTimeoutMilliseconds);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(eventResult));
                    AppendProtocolTestInformation(eventResult.RequestResult == 0
                        ? "EventReport 已发送，不等待 OBU 回复。\r\n"
                        : "EventReport 发送失败。\r\n");
                    cancellationToken.ThrowIfCancellationRequested();
                    // OtherFrames 是独立的随机避让时间窗采样，不是 EventReport 的响应；其响应前四字节用于统计窗口。
                    TransparentCommandResult timingResult = _desktopCommService.ExecuteTransparent(
                        BuildTypedCommand(0x06, eventReport), BstCompatibilityTimeoutMilliseconds);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(timingResult));
                    if (timingResult.IsSuccess)
                    {
                        AppendProtocolTestInformation("随机避让时间窗采样完成。\r\n");
                    }
                    if (!timingResult.IsSuccess || timingResult.ResponseData == null || timingResult.ResponseData.Length < 4)
                    {
                        timeElse++;
                        completedCount++;
                        AppendProtocolTestInformation(
                            string.Format("第 {0}/{1} 轮未取得有效时间窗，继续下一轮；累计 第一={2}、第二={3}、第三={4}。\r\n",
                                cycleIndex + 1, count, time3, time6, time9));
                        continue;
                    }

                    double window = CalculateVstRandomDodgeWindow(timingResult.ResponseData);
                    string windowName;
                    if (window <= 2.85 && window > 0.01)
                    {
                        time3++;
                        windowName = "第一时间窗（0.01, 2.85]";
                    }
                    else if (window <= 5.85 && window > 3.01)
                    {
                        time6++;
                        windowName = "第二时间窗（3.01, 5.85]";
                    }
                    else if (window <= 8.85 && window > 6.01)
                    {
                        time9++;
                        windowName = "第三时间窗（6.01, 8.85]";
                    }
                    else
                    {
                        timeElse++;
                        windowName = "其他时间窗";
                    }

                    completedCount++;
                    AppendProtocolTestInformation(
                        string.Format("时间窗值：{0:F4} ms，归类：{1}；累计 第一={2}、第二={3}、第三={4}、其他={5}\r\n",
                            window, windowName, time3, time6, time9, timeElse));
                }

                int minimumPerWindowExclusive = count / 4 + 1;
                bool passed = time3 >= minimumPerWindowExclusive
                    && time6 >= minimumPerWindowExclusive
                    && time9 >= minimumPerWindowExclusive;
                string message = string.Format(
                    "VST随机避让测试{0}：完成 {1}/{2} 轮；第一时间窗={3}、第二时间窗={4}、第三时间窗={5}、其他/无效={6}；最终结果只按三个目标时间窗判断，每窗必须严格大于测试次数的 1/4（本次至少 {7}）。",
                    passed ? "通过" : "未通过",
                    completedCount,
                    count,
                    time3,
                    time6,
                    time9,
                    timeElse,
                    minimumPerWindowExclusive);
                return new VstRandomDodgeExecutionResult(
                    passed,
                    message,
                    exchanges,
                    time3,
                    time6,
                    time9,
                    timeElse,
                    completedCount,
                    DateTime.Now);
            }
            catch (OperationCanceledException)
            {
                return new VstRandomDodgeExecutionResult(
                    false,
                    string.Format("VST随机避让测试已由用户停止：已完成 {0}/{1} 轮。", completedCount, count),
                    exchanges,
                    time3,
                    time6,
                    time9,
                    timeElse,
                    completedCount,
                    DateTime.Now);
            }
            catch (Exception exception) when (IsNativeInteropException(exception))
            {
                return new VstRandomDodgeExecutionResult(false, "VST随机避让 DLL 通信失败：" + exception.Message, exchanges, time3, time6, time9, timeElse, completedCount, DateTime.Now);
            }
            catch (Exception exception)
            {
                return new VstRandomDodgeExecutionResult(false, "VST随机避让测试失败：" + exception.Message, exchanges, time3, time6, time9, timeElse, completedCount, DateTime.Now);
            }
        }

        /// <summary>
        /// 将 VST 随机避让后台任务切回 UI 线程，显示统计结果并按开关保存交互帧和截图。
        /// </summary>
        /// <param name="testCaseName">测试用例叶子名称。</param>
        /// <param name="startedAt">测试开始时间。</param>
        /// <param name="completedTask">后台随机避让任务。</param>
        private void FinishVstRandomDodgeTask(
            string testCaseName,
            DateTime startedAt,
            Task<VstRandomDodgeExecutionResult> completedTask)
        {
            VstRandomDodgeExecutionResult result = completedTask.IsFaulted
                ? new VstRandomDodgeExecutionResult(false, "后台随机避让测试失败：" + completedTask.Exception.GetBaseException().Message, null, 0, 0, 0, 0, 0, DateTime.Now)
                : completedTask.IsCanceled
                    ? new VstRandomDodgeExecutionResult(false, "随机避让测试已取消。", null, 0, 0, 0, 0, 0, DateTime.Now)
                    : completedTask.Result;
            AppendProtocolTestInformation(result.Message + "\r\n");
            PersistTestArtifacts(testCaseName, result.IsSuccess, result.Message, string.Empty, result.Exchanges, startedAt, result.CompletedAt);
            // 随机避让流程结束后释放大框统一停止资源，恢复测试按钮状态。
            CompleteTestExecution();
        }

        /// <summary>
        /// 执行测试用例“测试用例1：无间隔发送BST”的独立入口。
        /// </summary>
        private void RunTestCase002()
        {
            int count = ReadConfiguredTestCount("SET_139_RandomDodge_ZeroSecond_num", DefaultVstRandomDodgeCount);
            // 随机避让入口读取 exe 同级 SetMe.ini 的次数，未配置或非法时使用默认 200 次。
            RunVstRandomDodgeTest(count);
        }

        /// <summary>
        /// 执行测试用例“3、防碰撞测试”的独立入口。
        /// </summary>
        private async void RunTestCase003()
        {
            const string testCaseName = "通用测试项 > 3、防碰撞测试";
            DateTime startedAt = DateTime.Now;
            if (_laneTransactionService == null || _desktopCommService == null)
            {
                const string unavailableMessage = "防碰撞测试未通过：台发通信或软算交易服务未初始化。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                // 服务不可用时仍记录明确的失败结果，便于勾选存储后追溯。
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            AntiCollisionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
                AppendProtocolTestInformation("开始执行防碰撞测试。\r\n");
                // 独立服务在后台线程执行动态 VST、软算交易和五段时间窗判定，避免阻塞 WinForms 界面。
                result = await Task.Run(
                    () => _laneTransactionService.ExecuteAntiCollision(
                        _desktopCommService,
                        configurationPath,
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n")),
                    cancellationToken);
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "防碰撞测试已停止，测试失败。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception)
            {
                failureMessage = "防碰撞测试异常，测试失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string finalMessage = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                DateTime completedAt = result == null ? DateTime.Now : result.CompletedAt;
                // 统一保存最终判定和完整空口帧，并在流程结束后恢复测试按钮状态。
                PersistTestArtifacts(testCaseName, isSuccess, finalMessage, string.Empty, exchanges, startedAt, completedAt);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“测试用例1：预读0019文件、0002文件”的独立入口。
        /// </summary>
        private void RunTestCase004()
        {
            // 预读 0019/0002 入口发送对应 BST，再按原顺序读取 ESAM 目录、系统信息和 IC 文件。
            RunProtocolSequenceTest(
                "通用测试项 > 4、预读信息测试 > 测试用例1：预读0019文件、0002文件",
                CreatePreReadSteps(0));
        }

        /// <summary>
        /// 执行测试用例“测试用例2：预读0015文件”的独立入口。
        /// </summary>
        private void RunTestCase005()
        {
            // 预读 0015 入口保持与原上位机相同的 BST 预读字段，并执行读取链路。
            RunProtocolSequenceTest(
                "通用测试项 > 4、预读信息测试 > 测试用例2：预读0015文件",
                CreatePreReadSteps(1));
        }

        /// <summary>
        /// 执行测试用例“测试用例3：预读0012文件”的独立入口。
        /// </summary>
        private void RunTestCase006()
        {
            // 预读 0012 入口保持与原上位机相同的 BST 预读字段，并执行读取链路。
            RunProtocolSequenceTest(
                "通用测试项 > 4、预读信息测试 > 测试用例3：预读0012文件",
                CreatePreReadSteps(2));
        }

        /// <summary>
        /// 执行测试用例“测试用例1：0信道读取文件”的独立入口。
        /// </summary>
        private void RunTestCase007()
        {
            // 信道 0 用例调用原 OBU 信道选择帧序列。
            RunProtocolSequenceTest("通用测试项 > 5、OBU信道选择测试 > 测试用例1：0信道读取文件", CreateChannelSteps(0), 0);
        }

        /// <summary>
        /// 执行测试用例“测试用例2：1信道读取文件”的独立入口。
        /// </summary>
        private void RunTestCase008()
        {
            // 信道 1 用例调用原 OBU 信道选择帧序列。
            RunProtocolSequenceTest("通用测试项 > 5、OBU信道选择测试 > 测试用例2：1信道读取文件", CreateChannelSteps(1), 1);
        }

        /// <summary>
        /// 执行测试用例“测试用例1：无访问许可，有加密秘钥标识”的独立入口。
        /// </summary>
        private void RunTestCase009()
        {
            RunProtocolSequenceTest("通用测试项 > 6、Getsecure测试 > 测试用例1：无访问许可，有加密秘钥标识", CreateGetSecureSteps(1, radioButtonSm4.Checked));
        }

        /// <summary>
        /// 执行测试用例“测试用例2：无访问许可，无加密秘钥标识”的独立入口。
        /// </summary>
        private void RunTestCase010()
        {
            RunProtocolSequenceTest("通用测试项 > 6、Getsecure测试 > 测试用例2：无访问许可，无加密秘钥标识", CreateGetSecureSteps(2, radioButtonSm4.Checked));
        }

        /// <summary>
        /// 执行测试用例“测试用例3：有访问许可，有加密秘钥标识”的独立入口。
        /// </summary>
        private void RunTestCase011()
        {
            RunProtocolSequenceTest("通用测试项 > 6、Getsecure测试 > 测试用例3：有访问许可，有加密秘钥标识", CreateGetSecureSteps(3, radioButtonSm4.Checked));
        }

        /// <summary>
        /// 执行测试用例“测试用例4：有访问许可，无加密秘钥标识”的独立入口。
        /// </summary>
        private void RunTestCase012()
        {
            RunProtocolSequenceTest("通用测试项 > 6、Getsecure测试 > 测试用例4：有访问许可，无加密秘钥标识", CreateGetSecureSteps(4, radioButtonSm4.Checked));
        }

        /// <summary>
        /// 执行测试用例“测试用例1：1条APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase013()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例1：1条APDU指令读余额", CreateTransferSteps(1, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例2：2条相同APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase014()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例2：2条相同APDU指令读余额", CreateTransferSteps(2, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例3：3条相同APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase015()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例3：3条相同APDU指令读余额", CreateTransferSteps(3, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例4：4条相同APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase016()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例4：4条相同APDU指令读余额", CreateTransferSteps(4, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例5：5条相同APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase017()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例5：5条相同APDU指令读余额", CreateTransferSteps(5, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例6：6条相同APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase018()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例6：6条相同APDU指令读余额", CreateTransferSteps(6, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例7：7条相同APDU指令读余额”的独立入口。
        /// </summary>
        private void RunTestCase019()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例7：7条相同APDU指令读余额", CreateTransferSteps(7, false));
        }

        /// <summary>
        /// 执行测试用例“测试用例8：1条APDU指令操作ESAM取4字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase020()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例8：1条APDU指令操作ESAM取4字节随机数", CreateTransferSteps(1, true));
        }

        /// <summary>
        /// 执行测试用例“测试用例9：2条APDU指令操作ESAM取4字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase021()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例9：2条APDU指令操作ESAM取4字节随机数", CreateTransferSteps(2, true));
        }

        /// <summary>
        /// 执行测试用例“测试用例10：3条APDU指令操作ESAM取4字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase022()
        {
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例10：3条APDU指令操作ESAM取4字节随机数", CreateTransferSteps(3, true));
        }

        /// <summary>
        /// 执行测试用例“测试用例11：3条不相同的IC通道APDU指令操作读余额、读0015文件、读0012文件”的独立入口。
        /// </summary>
        private void RunTestCase023()
        {
            // 第 11 项必须发送三条不同 IC APDU，不能复用“三条相同 APDU”的构造函数。
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例11：3条不相同的IC通道APDU指令操作读余额、读0015文件、读0012文件", CreateMixedTransferSteps(false));
        }

        /// <summary>
        /// 执行测试用例“测试用例12：3条不相同的ESAM通道APDU指令操作进3F00目录、读EF01文件、取芯片序列号”的独立入口。
        /// </summary>
        private void RunTestCase024()
        {
            // 第 12 项必须发送三条不同 ESAM APDU，不能复用“三条相同 APDU”的构造函数。
            RunProtocolSequenceTest("通用测试项 > 7、transferchannel测试 > 测试用例12：3条不相同的ESAM通道APDU指令操作进3F00目录、读EF01文件、取芯片序列号", CreateMixedTransferSteps(true));
        }

        /// <summary>
        /// 执行测试用例“测试用例1：参数00”的独立入口。
        /// </summary>
        private void RunTestCase025()
        {
            RunProtocolSequenceTest("通用测试项 > 8、SetMMI测试 > 测试用例1：参数00", CreateSetMmiSteps(0));
        }

        /// <summary>
        /// 执行测试用例“测试用例2：参数01”的独立入口。
        /// </summary>
        private void RunTestCase026()
        {
            RunProtocolSequenceTest("通用测试项 > 8、SetMMI测试 > 测试用例2：参数01", CreateSetMmiSteps(1));
        }

        /// <summary>
        /// 执行测试用例“测试用例3：参数02”的独立入口。
        /// </summary>
        private void RunTestCase027()
        {
            RunProtocolSequenceTest("通用测试项 > 8、SetMMI测试 > 测试用例3：参数02", CreateSetMmiSteps(2));
        }

        /// <summary>
        /// 执行测试用例“测试用例4：参数03”的独立入口。
        /// </summary>
        private void RunTestCase028()
        {
            RunProtocolSequenceTest("通用测试项 > 8、SetMMI测试 > 测试用例4：参数03", CreateSetMmiSteps(3));
        }

        /// <summary>
        /// 执行测试用例“测试用例5：参数04”的独立入口。
        /// </summary>
        private void RunTestCase029()
        {
            RunProtocolSequenceTest("通用测试项 > 8、SetMMI测试 > 测试用例5：参数04", CreateSetMmiSteps(4));
        }

        /// <summary>
        /// 执行测试用例“9、拼帧指令测试”的独立入口。
        /// </summary>
        private void RunTestCase030()
        {
            const string testCaseKey = "通用测试项 > 9、拼帧指令测试";
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? "9、拼帧指令测试" : selectedNode.Text;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                string message = "未连接主窗体台发服务，无法执行拼帧测试。";
                richTextBoxTestInformation.AppendText(message + "\r\n");
                PersistTestArtifacts(testCaseName, false, message, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            AppendProtocolTestInformation("开始执行：" + testCaseKey + "\r\n");
            // 专用后台入口负责完整五轮组合遍历，不能退化为普通逐帧序列。
            Task.Run(() => ExecuteLinkFrameRounds(cancellationToken)).ContinueWith(completedTask =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() => FinishProtocolSequenceTask(testCaseName, startedAt, completedTask)));
                }
                catch (InvalidOperationException)
                {
                    // 窗体关闭期间不再回写测试结果。
                }
            }, TaskScheduler.Default);
        }

        /// <summary>
        /// 执行测试用例“10、255S保持测试”的独立入口。
        /// </summary>
        private async void RunTestCase031()
        {
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? "10、255S保持测试" : selectedNode.Text;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailable = "未连接主窗体台发服务，无法执行255S保持测试。";
                AppendProtocolTestInformation(unavailable + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailable, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            IList<ProtocolSequenceStep> configuredSteps = Create255KeepSteps();
            List<byte[]> bstFrames = new List<byte[]>();
            List<bool> expectTrades = new List<bool>();
            for (int index = 0; index < configuredSteps.Count; index++)
            {
                bstFrames.Add(configuredSteps[index].Payload);
                expectTrades.Add(!configuredSteps[index].ExpectNoResponse);
            }

            bool originalBidChange = _desktopCommService.CurrentBidChange;
            bool originalUnixTimeChange = _desktopCommService.CurrentUnixTimeChange;
            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                // 255S 默认测试配置不启用 BID/UnixTime 更新，直接重初始化，避免弹窗和 DLL 改写配置帧。
                DesktopInitializationResult initialization = await Task.Run(() => _desktopCommService.ReinitializeBidAndUnixTimeChange(false, false));
                if (!initialization.IsSuccess)
                    throw new InvalidOperationException(string.Format("关闭BID和UnixTime更新后重新初始化失败：请求={0}，响应={1}。", initialization.RequestResult,
                        initialization.ResponseResult.HasValue ? initialization.ResponseResult.Value.ToString() : "未执行"));

                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行255S保持测试：已关闭BID和UnixTime自动更新并重新初始化台发，配置时间按固定秒差原样发送，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。\r\n");
                // 专用服务按配置逐项执行无响应判定或完整软算交易。
                result = await Task.Run(() => _laneTransactionService.Execute255Keep(
                    _desktopCommService,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                    bstFrames,
                    expectTrades,
                    algorithm,
                    cancellationToken,
                    message => AppendProtocolTestInformation(message + "\r\n"),
                    (success, total) => UpdateLaneTransactionProgress(success, total)), cancellationToken);
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "255S保持测试已由用户停止。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException || exception is ArgumentException)
            {
                failureMessage = "255S保持测试失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                CompleteLaneTransactionProgress();
                if (_desktopCommService.IsOpen
                    && (_desktopCommService.CurrentBidChange != originalBidChange
                        || _desktopCommService.CurrentUnixTimeChange != originalUnixTimeChange))
                {
                    try
                    {
                        // 测试结束分别恢复 BID 与 UnixTime 的原初始化状态，避免影响其他测试。
                        await Task.Run(() => _desktopCommService.ReinitializeBidAndUnixTimeChange(originalBidChange, originalUnixTimeChange));
                    }
                    catch (Exception restoreException)
                    {
                        AppendProtocolTestInformation("恢复原BID/UnixTime更新配置失败：" + restoreException.Message + "\r\n");
                    }
                }

                string message = result == null ? failureMessage : result.Message;
                PersistTestArtifacts(testCaseName, result != null && result.IsSuccess, message, string.Empty,
                    result == null ? null : result.Exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“用例1：获取车辆信息后发送BST”的独立入口。
        /// </summary>
        private void RunTestCase032()
        {
            // 用例入口执行原 OBU 模块第一项流程，并把第二次广播 BST 无 VST 作为通过条件。
            RunProtocolSequenceTest(
                "通用测试项 > 11、交易过程中响应广播帧测试 > 用例1：获取车辆信息后发送BST",
                CreateRespondBroadcastAfterVehicleInfoSteps());
        }

        /// <summary>
        /// 执行测试用例“用例2：消费初始化后发送BST”的独立入口。
        /// </summary>
        private void RunTestCase033()
        {
            // 用例入口依据当前算法选项构造消费初始化参数，再检查第二次广播 BST 无 VST。
            RunProtocolSequenceTest(
                "通用测试项 > 11、交易过程中响应广播帧测试 > 用例2：消费初始化后发送BST",
                CreateRespondBroadcastAfterPurchaseInitializationSteps(radioButtonSm4.Checked));
        }

        /// <summary>
        /// 执行测试用例“用例1：获取车辆信息后发送其他标签MAC的transferChannel”的独立入口。
        /// </summary>
        private void RunTestCase034()
        {
            // 用例入口保留末字节加一的错误 MAC，并以 OBU 不响应 TransferChannel 为通过条件。
            RunProtocolSequenceTest(
                "通用测试项 > 12、交易过程中响应不同MAC测试 > 用例1：获取车辆信息后发送其他标签MAC的transferChannel",
                CreateDifferentMacTransferSteps(1), null, true);
        }

        /// <summary>
        /// 执行测试用例“用例2：读取车辆信息之后发送全ff的transferChannel”的独立入口。
        /// </summary>
        private void RunTestCase035()
        {
            // 用例入口保留全 FF MAC，并以 OBU 不响应 TransferChannel 为通过条件。
            RunProtocolSequenceTest(
                "通用测试项 > 12、交易过程中响应不同MAC测试 > 用例2：读取车辆信息之后发送全ff的transferChannel",
                CreateDifferentMacTransferSteps(2), null, true);
        }

        /// <summary>
        /// 执行测试用例“用例3：读取车辆信息之后发送其他MAC的transferChannel”的独立入口。
        /// </summary>
        private void RunTestCase036()
        {
            // 用例入口保留第三字节加一的错误 MAC，并以 OBU 不响应 TransferChannel 为通过条件。
            RunProtocolSequenceTest(
                "通用测试项 > 12、交易过程中响应不同MAC测试 > 用例3：读取车辆信息之后发送其他MAC的transferChannel",
                CreateDifferentMacTransferSteps(3), null, true);
        }

        /// <summary>
        /// 执行测试用例“13、地标交易测试”的独立入口。
        /// </summary>
        private void RunTestCase037()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("通用测试项 > 13、地标交易测试");
        }

        /// <summary>
        /// 执行备用测试项“1、车道交易测试”，并按原软交易流程调用进程内算法服务。
        /// </summary>
        private async void RunLaneTransactionTest()
        {
            TreeNode selectedNode = treeViewTestItems.SelectedNode;
            string testCaseName = selectedNode == null ? "1、车道交易测试" : selectedNode.Text;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                string unavailableMessage = "未连接主窗体台发服务，无法执行车道交易测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行备用测试项：车道交易测试（原软算流程，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "）……\r\n");
                // 调用入口把阻塞式台发通信放到后台线程，停止按钮通过统一令牌终止后续步骤。
                result = await Task.Run(
                    () => _laneTransactionService.Execute(
                        _desktopCommService,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (successCount, totalCount) => UpdateLaneTransactionProgress(successCount, totalCount)),
                    cancellationToken);
                // 车道交易结束后关闭原位计数行，再显示最终结论或失败详情。
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "车道交易测试已由用户停止。";
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException)
            {
                failureMessage = "车道交易测试失败：" + exception.Message;
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                DateTime completedAt = DateTime.Now;
                string message = result == null ? failureMessage : result.Message;
                bool isSuccess = result != null && result.IsSuccess;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                // 结果入口统一保存每一轮已发生的完整空中交互帧和截图。
                PersistTestArtifacts(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, completedAt);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行备用测试项“2、门架交易测试”，并按旧版 WJ_MastTrade 流程调用门架交易服务。
        /// </summary>
        private async void RunGantryTransactionTest()
        {
            const string testCaseName = "备用测试项 > 2、门架交易测试";
            DateTime startedAt = DateTime.Now;
            // 门架入口使用独立的原位计数行，避免覆盖上一次门架测试的进度文本。
            ResetGantryTransactionProgress();
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行门架交易测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                // 门架交易沿用旧 WJ_MastTrade 的报文布局，但软算法必须使用界面当前选择。
                // SM4 的消费初始化算法标识为 0x41，3DES 的算法标识为 0x01。
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行备用测试项：门架交易测试（旧 WJ_MastTrade 流程，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "）……\r\n");
                // 门架通信包含多个阻塞式 TransferChannel 步骤，放入后台任务以保持 WinForms 停止按钮响应。
                result = await Task.Run(
                    () => _laneTransactionService.ExecuteGantry(
                        _desktopCommService,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (successCount, totalCount) => UpdateGantryTransactionProgress(successCount, totalCount)),
                    cancellationToken);
                CompleteGantryTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "门架交易测试已由用户停止。";
                CompleteGantryTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException)
            {
                failureMessage = "门架交易测试失败：" + exception.Message;
                CompleteGantryTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                DateTime completedAt = DateTime.Now;
                string message = result == null ? failureMessage : result.Message;
                bool isSuccess = result != null && result.IsSuccess;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                // 门架测试与车道测试共用结果文件和截图入口，保持现有交付格式一致。
                PersistTestArtifacts(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, completedAt);
                CompleteTestExecution();
            }
        }

        /// <summary>执行备用测试项“3、典型交易测试”的六个原上位机固定分支。</summary>
        private async void RunTypicalTransactionTest()
        {
            const string testCaseName = "备用测试项 > 3、典型交易测试";
            DateTime startedAt = DateTime.Now;
            ResetTypicalTransactionProgress();
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行典型交易测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行备用测试项：典型交易测试（原上位机六分支流程，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "）……\r\n");
                // 原 C++ 用例包含阻塞式 BST、GetSecure 和多条 TransferChannel，统一放入后台线程执行。
                result = await Task.Run(
                    () => _laneTransactionService.ExecuteTypical(
                        _desktopCommService,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (successCount, totalCount) => UpdateTypicalTransactionProgress(successCount, totalCount)),
                    cancellationToken);
                CompleteTypicalTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "典型交易测试已由用户停止。";
                CompleteTypicalTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException || exception is ArgumentException)
            {
                failureMessage = "典型交易测试失败：" + exception.Message;
                CompleteTypicalTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                DateTime completedAt = DateTime.Now;
                string message = result == null ? failureMessage : result.Message;
                PersistTestArtifacts(testCaseName, result != null && result.IsSuccess, message, string.Empty,
                    result == null ? null : result.Exchanges, startedAt, completedAt);
                CompleteTestExecution();
            }
        }

        /// <summary>重置典型交易固定进度区域。</summary>
        private void ResetTypicalTransactionProgress() { _typicalProgressTextStart = -1; _typicalProgressTextLength = 0; }

        /// <summary>原位更新六个典型交易分支的成功数。</summary>
        private void UpdateTypicalTransactionProgress(int successCount, int totalCount)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<int, int>(UpdateTypicalTransactionProgress), successCount, totalCount); }
                catch (InvalidOperationException) { }
                return;
            }

            string text = string.Format("典型交易成功分支：{0}/{1}", successCount, totalCount);
            if (_typicalProgressTextStart < 0 || _typicalProgressTextStart + _typicalProgressTextLength > richTextBoxTestInformation.TextLength)
            {
                _typicalProgressTextStart = richTextBoxTestInformation.TextLength;
                richTextBoxTestInformation.AppendText(text);
            }
            else
            {
                richTextBoxTestInformation.Select(_typicalProgressTextStart, _typicalProgressTextLength);
                richTextBoxTestInformation.SelectedText = text;
            }
            _typicalProgressTextLength = text.Length;
            richTextBoxTestInformation.SelectionStart = richTextBoxTestInformation.TextLength;
            richTextBoxTestInformation.SelectionLength = 0;
        }

        /// <summary>结束典型交易进度行。</summary>
        private void CompleteTypicalTransactionProgress()
        {
            if (_typicalProgressTextStart >= 0) richTextBoxTestInformation.AppendText("\r\n");
            ResetTypicalTransactionProgress();
        }

        /// <summary>
        /// 重置车道交易测试的原位成功进度区域。
        /// </summary>
        private void ResetLaneTransactionProgress()
        {
            _laneProgressTextStart = -1;
            _laneProgressTextLength = 0;
        }

        /// <summary>
        /// 在测试信息框的同一位置更新车道交易成功次数，不追加逐轮成功日志。
        /// </summary>
        /// <param name="successCount">已经完整通过车道交易流程的轮数，允许为 0。</param>
        /// <param name="totalCount">配置文件指定的本次总轮数，必须大于 0。</param>
        private void UpdateLaneTransactionProgress(int successCount, int totalCount)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<int, int>(UpdateLaneTransactionProgress), successCount, totalCount);
                }
                catch (InvalidOperationException)
                {
                    // 窗体关闭过程中忽略尚未显示的后台进度更新。
                }

                return;
            }

            string progressText = string.Format("车道交易成功次数：{0}/{1}", successCount, totalCount);
            if (_laneProgressTextStart < 0 || _laneProgressTextStart + _laneProgressTextLength > richTextBoxTestInformation.TextLength)
            {
                _laneProgressTextStart = richTextBoxTestInformation.TextLength;
                richTextBoxTestInformation.AppendText(progressText);
            }
            else
            {
                richTextBoxTestInformation.Select(_laneProgressTextStart, _laneProgressTextLength);
                richTextBoxTestInformation.SelectedText = progressText;
            }

            _laneProgressTextLength = progressText.Length;
            richTextBoxTestInformation.SelectionStart = richTextBoxTestInformation.TextLength;
            richTextBoxTestInformation.SelectionLength = 0;
        }

        /// <summary>
        /// 结束车道交易原位进度行，使失败详情或最终结论从下一行开始显示。
        /// </summary>
        private void CompleteLaneTransactionProgress()
        {
            if (_laneProgressTextStart >= 0)
            {
                richTextBoxTestInformation.AppendText("\r\n");
            }

            ResetLaneTransactionProgress();
        }

        /// <summary>重置门架交易测试的原位成功进度区域。</summary>
        private void ResetGantryTransactionProgress()
        {
            _gantryProgressTextStart = -1;
            _gantryProgressTextLength = 0;
        }

        /// <summary>在测试信息框同一位置更新门架交易成功次数。</summary>
        /// <param name="successCount">已完整通过的门架交易轮数。</param><param name="totalCount">配置的总轮数。</param>
        private void UpdateGantryTransactionProgress(int successCount, int totalCount)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<int, int>(UpdateGantryTransactionProgress), successCount, totalCount); }
                catch (InvalidOperationException) { }
                return;
            }

            string progressText = string.Format("门架交易成功次数：{0}/{1}", successCount, totalCount);
            if (_gantryProgressTextStart < 0 || _gantryProgressTextStart + _gantryProgressTextLength > richTextBoxTestInformation.TextLength)
            {
                _gantryProgressTextStart = richTextBoxTestInformation.TextLength;
                richTextBoxTestInformation.AppendText(progressText);
            }
            else
            {
                richTextBoxTestInformation.Select(_gantryProgressTextStart, _gantryProgressTextLength);
                richTextBoxTestInformation.SelectedText = progressText;
            }

            _gantryProgressTextLength = progressText.Length;
            richTextBoxTestInformation.SelectionStart = richTextBoxTestInformation.TextLength;
            richTextBoxTestInformation.SelectionLength = 0;
        }

        /// <summary>结束门架交易原位进度行，使最终结论从下一行开始显示。</summary>
        private void CompleteGantryTransactionProgress()
        {
            if (_gantryProgressTextStart >= 0) richTextBoxTestInformation.AppendText("\r\n");
            ResetGantryTransactionProgress();
        }

        /// <summary>
        /// 执行测试用例“测试用例1：取4字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase038()
        {
            // 指令集入口执行 4 字节随机数 APDU，并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例1：取4字节随机数", CreateInstructionSteps(1));
        }

        /// <summary>
        /// 执行测试用例“测试用例2：取8字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase039()
        {
            // 指令集入口执行 8 字节随机数 APDU，并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例2：取8字节随机数", CreateInstructionSteps(2));
        }

        /// <summary>
        /// 执行测试用例“测试用例3：取16字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase040()
        {
            // 指令集入口执行 16 字节随机数 APDU，并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例3：取16字节随机数", CreateInstructionSteps(3));
        }

        /// <summary>
        /// 执行测试用例“测试用例4：取9字节随机数”的独立入口。
        /// </summary>
        private void RunTestCase041()
        {
            // 指令集入口执行 9 字节边界 APDU，并检查 67 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例4：取9字节随机数", CreateInstructionSteps(4));
        }

        /// <summary>
        /// 执行测试用例“测试用例5：发送随机数p1参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase042()
        {
            // 指令集入口验证随机数 P1 错误应返回 6A 86 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例5：发送随机数p1参数不正确", CreateInstructionSteps(5));
        }

        /// <summary>
        /// 执行测试用例“测试用例6：发送随机数CLA参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase043()
        {
            // 指令集入口验证随机数 CLA 错误应返回 6E 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例6：发送随机数CLA参数不正确", CreateInstructionSteps(6));
        }

        /// <summary>
        /// 执行测试用例“测试用例7：发送随机数INS参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase044()
        {
            // 指令集入口验证随机数 INS 错误应返回 6D 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例7：发送随机数INS参数不正确", CreateInstructionSteps(7));
        }

        /// <summary>
        /// 执行测试用例“测试用例8：取4字节芯片序列号”的独立入口。
        /// </summary>
        private void RunTestCase045()
        {
            // 指令集入口执行 4 字节芯片序列号 APDU，并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例8：取4字节芯片序列号", CreateInstructionSteps(8));
        }

        /// <summary>
        /// 执行测试用例“测试用例9：发送取芯片序列号指令p1参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase046()
        {
            // 指令集入口验证芯片序列号 P1 错误应返回 6A 86 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例9：发送取芯片序列号指令p1参数不正确", CreateInstructionSteps(9));
        }

        /// <summary>
        /// 执行测试用例“测试用例10：发送取芯片序列号指令长度不正确”的独立入口。
        /// </summary>
        private void RunTestCase047()
        {
            // 旧指令集用例10以 6C 04 00 表示 Le 应为4字节，不能按通用67 00处理。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例10：发送取芯片序列号指令长度不正确", CreateInstructionSteps(10));
        }

        /// <summary>
        /// 执行测试用例“测试用例11：取芯片序列号INS参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase048()
        {
            // 指令集入口验证芯片序列号 INS 错误应返回 6D 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例11：取芯片序列号INS参数不正确", CreateInstructionSteps(11));
        }

        /// <summary>
        /// 执行测试用例“测试用例12：取芯片序列号CLA参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase049()
        {
            // 指令集入口验证芯片序列号 CLA 错误应返回 6E 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例12：取芯片序列号CLA参数不正确", CreateInstructionSteps(12));
        }

        /// <summary>
        /// 执行测试用例“测试用例13：选择EF04文件”的独立入口。
        /// </summary>
        private void RunTestCase050()
        {
            // 指令集入口执行 EF04 文件选择并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例13：选择EF04文件", CreateInstructionSteps(13));
        }

        /// <summary>
        /// 执行测试用例“测试用例14：选择EF04文件参数不正确”的独立入口。
        /// </summary>
        private void RunTestCase051()
        {
            // 指令集入口验证 EF04 选择参数错误应返回 6A 86 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例14：选择EF04文件参数不正确", CreateInstructionSteps(14));
        }

        /// <summary>
        /// 执行测试用例“测试用例15：选择EF04文件CLA不正确”的独立入口。
        /// </summary>
        private void RunTestCase052()
        {
            // 指令集入口验证 EF04 选择 CLA 错误应返回 6E 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例15：选择EF04文件CLA不正确", CreateInstructionSteps(15));
        }

        /// <summary>
        /// 执行测试用例“测试用例16：读取0015文件测试”的独立入口。
        /// </summary>
        private void RunTestCase053()
        {
            // 指令集入口读取 0015 文件并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例16：读取0015文件测试", CreateInstructionSteps(16));
        }

        /// <summary>
        /// 执行测试用例“测试用例17：读取001A文件”的独立入口。
        /// </summary>
        private void RunTestCase054()
        {
            // 指令集入口读取 001A 文件并检查 90 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例17：读取001A文件", CreateInstructionSteps(17));
        }

        /// <summary>
        /// 执行测试用例“测试用例19：取响应代码返回6982”的独立入口。
        /// </summary>
        private void RunTestCase055()
        {
            // 原分支依次选择 3F00、DF01，再读取 EF01 并检查 69 82 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例19：取响应代码返回6982", CreateInstructionResponseCodeSteps(19));
        }

        /// <summary>
        /// 执行测试用例“测试用例20：响应代码6981不支持安全报文”的独立入口。
        /// </summary>
        private void RunTestCase056()
        {
            // 原分支依次选择 3F00、DF01，再读取 EF02 并检查 69 81 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例20：响应代码6981不支持安全报文", CreateInstructionResponseCodeSteps(20));
        }

        /// <summary>
        /// 执行测试用例“测试用例21：响应代码6A83未找到记录”的独立入口。
        /// </summary>
        private void RunTestCase057()
        {
            // 原分支选择目录后在 ICC 信道读取不存在记录，并检查 6A 83 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例21：响应代码6A83未找到记录", CreateInstructionResponseCodeSteps(21));
        }

        /// <summary>
        /// 执行测试用例“测试用例22：响应代码6984引用数据无效”的独立入口。
        /// </summary>
        private void RunTestCase058()
        {
            // 直接发送带无效引用数据的外部认证命令，检查 69 84 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例22：响应代码6984引用数据无效", CreateInstructionResponseCodeSteps(22));
        }

        /// <summary>
        /// 执行测试用例“测试用例23：响应代码6984未申请随机数”的独立入口。
        /// </summary>
        private void RunTestCase059()
        {
            // 不执行 GET CHALLENGE，直接更新系统信息并检查 69 84 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例23：响应代码6984未申请随机数", CreateInstructionResponseCodeSteps(23));
        }

        /// <summary>
        /// 执行测试用例“测试用例24：响应代码6986不满足命令执行”的独立入口。
        /// </summary>
        private void RunTestCase060()
        {
            // 同一 TransferChannel 内发送选择 EF04 和更新记录两条 APDU，检查 69 86 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例24：响应代码6986不满足命令执行", CreateInstructionResponseCodeSteps(24));
        }

        /// <summary>
        /// 执行测试用例“测试用例25：响应代码6988安全报文数据项不正确”的独立入口。
        /// </summary>
        private void RunTestCase061()
        {
            // 先申请随机数，再发送原上位机的错误安全报文并检查 69 88 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例25：响应代码6988安全报文数据项不正确", CreateInstructionResponseCodeSteps(25));
        }

        /// <summary>
        /// 执行测试用例“测试用例26：响应代码6B00参数不正确，偏移地址超出EF”的独立入口。
        /// </summary>
        private void RunTestCase062()
        {
            // 使用偏移 0x8163 读取系统信息文件，检查 6B 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例26：响应代码6B00参数不正确，偏移地址超出EF", CreateInstructionResponseCodeSteps(26));
        }

        /// <summary>
        /// 执行测试用例“测试用例27：响应代码6F00判断不准确”的独立入口。
        /// </summary>
        private void RunTestCase063()
        {
            // 无前置可取响应命令时发送 GET RESPONSE，检查 6F 00 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例27：响应代码6F00判断不准确", CreateInstructionResponseCodeSteps(27));
        }

        /// <summary>
        /// 执行测试用例“测试用例28：响应代码6985不满足引用条件”的独立入口。
        /// </summary>
        private void RunTestCase064()
        {
            // 在不满足锁卡条件时发送锁卡命令，检查 69 85 00。
            RunProtocolSequenceTest("通用测试项 > 14、指令集测试 > 测试用例28：响应代码6985不满足引用条件", CreateInstructionResponseCodeSteps(28));
        }

        /// <summary>
        /// 执行测试用例“15、保留文件读写测试”的独立入口。
        /// </summary>
        private void RunTestCase065()
        {
            // 旧 WJ_SuTong_ESAMfile 对 ICC 0009 保留文件执行读、固定数据写入及结果校验。
            RunProtocolSequenceTest("通用测试项 > 15、保留文件读写测试", CreateReservedFileReadWriteSteps());
        }

        /// <summary>
        /// 执行测试用例“16、单帧唤醒测试”的独立入口。
        /// </summary>
        private void RunTestCase066()
        {
            RunSingleFrameWakeTest();
        }

        /// <summary>
        /// 按旧版 WJ_ProtocolTest_SingleFrame 的专用初始化、逐轮 BST/VST 和最终汇总判据执行单帧唤醒测试。
        /// </summary>
        private async void RunSingleFrameWakeTest()
        {
            const string testCaseName = "通用测试项 > 16、单帧唤醒测试";
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行单帧唤醒测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
            int count = ReadIniInteger(configurationPath, "SET_DANZHEN", "number", 30);
            DesktopInitializationOptions originalOptions = null;
            ObuVersionReadResult result = null;
            string failureMessage = string.Empty;
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                // 旧用例固定台发参数但保留用户当前四项动态更新开关，结束时恢复完整原配置。
                DesktopInitializationResult initialization = await Task.Run(() => _desktopCommService.BeginSingleFrameWakeTestMode(out originalOptions));
                if (!initialization.IsSuccess)
                {
                    throw new InvalidOperationException(string.Format(
                        "单帧唤醒专用台发初始化失败：请求={0}，响应={1}。",
                        initialization.RequestResult,
                        initialization.ResponseResult.HasValue ? initialization.ResponseResult.Value.ToString() : "未执行"));
                }

                AppendProtocolTestInformation(string.Format(
                    "开始执行单帧唤醒：次数={0}，台发参数 BST=90、重发间隔=20、重发次数=32、功率=10、信道=0、超时=80ms。\r\n",
                    count));
                // 专用服务保留“BST失败停止、VST失败继续、全部VST成功才通过”的旧用例判定。
                result = await Task.Run(() => _desktopCommService.ExecuteSingleFrameWakeTest(80, count, cancellationToken));
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "单帧唤醒测试已停止。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception)
            {
                failureMessage = "单帧唤醒测试失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                if (originalOptions != null && _desktopCommService.IsOpen)
                {
                    try
                    {
                        // 恢复用户开始测试前的全部台发初始化配置，避免专用固定参数影响后续用例。
                        await Task.Run(() => _desktopCommService.RestoreInitializationOptions(originalOptions));
                    }
                    catch (Exception restoreException)
                    {
                        AppendProtocolTestInformation("恢复原台发配置失败：" + restoreException.Message + "\r\n");
                    }
                }

                string message = result == null ? failureMessage : result.Message;
                PersistTestArtifacts(testCaseName, result != null && result.IsSuccess, message,
                    result == null ? string.Empty : result.Version,
                    result == null ? null : result.Exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“17、模拟真实交易流测试”的独立入口。
        /// </summary>
        private void RunTestCase067()
        {
            RunSimulatedRealTransactionFlowTest();
        }

        /// <summary>在后台执行原 WJ_TransactionFlow_ruansuan 的入口、门架、出口连续交易流并恢复原台发配置。</summary>
        private async void RunSimulatedRealTransactionFlowTest()
        {
            const string testCaseName = "通用测试项 > 17、模拟真实交易流测试";
            DateTime startedAt = DateTime.Now;
            ResetLaneTransactionProgress();
            if (_desktopCommService == null)
            {
                const string message = "未连接主窗体台发服务，无法执行模拟真实交易流测试。";
                AppendProtocolTestInformation(message + "\r\n");
                PersistTestArtifacts(testCaseName, false, message, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            DesktopInitializationOptions originalOptions = null;
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                DesktopInitializationResult initialization = await Task.Run(() => _desktopCommService.BeginSimulatedRealTradeTestMode(out originalOptions));
                if (!initialization.IsSuccess) throw new InvalidOperationException("模拟真实交易流专用台发初始化失败：请求=" + initialization.RequestResult + "，响应=" + (initialization.ResponseResult.HasValue ? initialization.ResponseResult.Value.ToString() : "未执行") + "。");
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行模拟真实交易流：入口→门架→出口，算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。\r\n");
                result = await Task.Run(() => _laneTransactionService.ExecuteSimulatedRealTransactionFlow(_desktopCommService,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"), algorithm, cancellationToken,
                    message => AppendProtocolTestInformation(message + "\r\n"), (success, total) => UpdateLaneTransactionProgress(success, total)), cancellationToken);
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException) { failureMessage = "模拟真实交易流测试已停止。"; AppendProtocolTestInformation(failureMessage + "\r\n"); }
            catch (Exception exception) { failureMessage = "模拟真实交易流测试失败：" + exception.Message; AppendProtocolTestInformation(failureMessage + "\r\n"); }
            finally
            {
                CompleteLaneTransactionProgress();
                if (originalOptions != null && _desktopCommService.IsOpen)
                {
                    try { await Task.Run(() => _desktopCommService.RestoreInitializationOptions(originalOptions)); }
                    catch (Exception restoreException) { AppendProtocolTestInformation("恢复原台发配置失败：" + restoreException.Message + "\r\n"); }
                }
                PersistTestArtifacts(testCaseName, result != null && result.IsSuccess, result == null ? failureMessage : result.Message, string.Empty,
                    result == null ? null : result.Exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“测试用例1：5.8G修改MACID”的独立入口。
        /// </summary>
        private void RunTestCase068()
        {
            // 次数取自原SET_AmendMACIDSN配置，每轮生成4字节0x00～0x59随机MACID。
            RunAmendMacOrSnTest(false);
        }

        /// <summary>
        /// 执行测试用例“测试用例2：5.8G修改SN号”的独立入口。
        /// </summary>
        private void RunTestCase069()
        {
            // 次数取自原SET_AmendMACIDSN配置，每轮写入8字节随机SN并通过D9命令复读核对。
            RunAmendMacOrSnTest(true);
        }

        /// <summary>
        /// 在后台执行万集自有协议MACID或SN修改测试，并保存最后写入值及全部轮次交互记录。
        /// </summary>
        /// <param name="modifySerialNumber">为true执行SN写入和复读，为false执行MACID写入。</param>
        private async void RunAmendMacOrSnTest(bool modifySerialNumber)
        {
            string leafName = modifySerialNumber ? "测试用例2：5.8G修改SN号" : "测试用例1：5.8G修改MACID";
            string testCaseName = "通用测试项 > 18、万集自有协议测试 > " + leafName;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行MACID/SN修改测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
            int count = ReadIniInteger(configurationPath, "SET_AmendMACIDSN", "number", 10);
            ObuVersionReadResult result = null;
            string failureMessage = string.Empty;
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                AppendProtocolTestInformation(string.Format("开始执行{0}，配置次数={1}。\r\n", leafName, count));
                // 多轮认证和写入包含阻塞式DLL调用，转入后台线程并保留统一停止能力。
                result = await Task.Run(() => _desktopCommService.AmendMacIdOrSerialNumber(
                    BstCompatibilityTimeoutMilliseconds, count, modifySerialNumber, cancellationToken));
                AppendProtocolTestInformation(result.Message + "\r\n");
                if (!string.IsNullOrEmpty(result.Version))
                {
                    AppendProtocolTestInformation("最后写入值：" + result.Version + "\r\n");
                }
            }
            catch (OperationCanceledException)
            {
                failureMessage = leafName + "已停止。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception)
            {
                failureMessage = leafName + "失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, result == null ? string.Empty : result.Version, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“测试用例3：5.8G读取蓝牙地址”的独立入口。
        /// </summary>
        private void RunTestCase070()
        {
            // 原用例在71通道认证后使用70通道75命令读取6字节蓝牙地址。
            RunBluetoothMacTest();
        }

        /// <summary>
        /// 在后台执行万集自有协议蓝牙地址读取，并保存6字节地址及完整5.8G交互记录。
        /// </summary>
        private async void RunBluetoothMacTest()
        {
            const string testCaseName = "通用测试项 > 18、万集自有协议测试 > 测试用例3：5.8G读取蓝牙地址";
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法读取蓝牙地址。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            ObuVersionReadResult result = null;
            string failureMessage = string.Empty;
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                AppendProtocolTestInformation("开始执行5.8G蓝牙地址读取。\r\n");
                // 认证和读取均为阻塞式DLL调用，放入后台任务以保持停止按钮有效。
                result = await Task.Run(() => _desktopCommService.ReadBluetoothMac(BstCompatibilityTimeoutMilliseconds, cancellationToken));
                AppendProtocolTestInformation(result.Message + "\r\n");
                if (!string.IsNullOrEmpty(result.Version))
                {
                    AppendProtocolTestInformation("蓝牙地址：" + result.Version + "\r\n");
                }
            }
            catch (OperationCanceledException)
            {
                failureMessage = "蓝牙地址读取已停止。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception)
            {
                failureMessage = "蓝牙地址读取失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, result == null ? string.Empty : result.Version, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“测试用例4：5.8G读写地区”的独立入口。
        /// </summary>
        private void RunTestCase071()
        {
            // 原流程用首次DD实读地区原值执行DC回写，再次DD复读核对。
            RunWanjiAreaOrChannelTest(false);
        }

        /// <summary>
        /// 执行测试用例“测试用例5：不同通道验证测试”的独立入口。
        /// </summary>
        private void RunTestCase072()
        {
            // 通道指令字节来自SET_Channel/Channel，当前配置最终形成70 01 01 72。
            RunWanjiAreaOrChannelTest(true);
        }

        /// <summary>
        /// 在后台执行万集自有协议地区原值读写或配置通道指令验证，并保存完整交互记录。
        /// </summary>
        /// <param name="validateConfiguredChannel">为true执行SET_Channel通道指令；为false执行地区读写复核。</param>
        private async void RunWanjiAreaOrChannelTest(bool validateConfiguredChannel)
        {
            string leafName = validateConfiguredChannel ? "测试用例5：不同通道验证测试" : "测试用例4：5.8G读写地区";
            string testCaseName = "通用测试项 > 18、万集自有协议测试 > " + leafName;
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行地区/通道测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            string configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini");
            int configuredChannel = ReadIniInteger(configurationPath, "SET_Channel", "Channel", 0x72);
            ObuVersionReadResult result = null;
            string failureMessage = string.Empty;
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                AppendProtocolTestInformation(validateConfiguredChannel
                    ? string.Format("开始不同通道验证，配置指令=0x{0:X2}。\r\n", configuredChannel & 0xFF)
                    : "开始地区原值读取、回写和复读校验。\r\n");
                // 71认证及后续私有指令均为阻塞式DLL调用，放到后台执行并保留停止能力。
                result = await Task.Run(() => _desktopCommService.ExecuteWanjiAreaOrChannelTest(
                    BstCompatibilityTimeoutMilliseconds, validateConfiguredChannel, (byte)(configuredChannel & 0xFF), cancellationToken));
                AppendProtocolTestInformation(result.Message + "\r\n");
                if (!string.IsNullOrEmpty(result.Version))
                {
                    AppendProtocolTestInformation((validateConfiguredChannel ? "通道响应数据：" : "地区字节：0x") + result.Version + "\r\n");
                }
            }
            catch (OperationCanceledException)
            {
                failureMessage = leafName + "已停止。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception)
            {
                failureMessage = leafName + "失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, result == null ? string.Empty : result.Version, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“测试用例6：射频发数测试（55AA）”的独立入口。
        /// </summary>
        private void RunTestCase073()
        {
            // 原用例每轮执行50对73通道01/02指令，分别严格检查55AA与AA55交替数据。
            RunProtocolSequenceTest("通用测试项 > 18、万集自有协议测试 > 测试用例6：射频发数测试（55AA）", CreateRfSendDataSteps());
        }

        /// <summary>
        /// 执行测试用例“19、播报金额测试（语音款）”的独立入口。
        /// </summary>
        private async void RunTestCase074()
        {
            const string testCaseName = "通用测试项 > 19、播报金额测试（语音款）";
            DateTime startedAt = DateTime.Now;
            ResetLaneTransactionProgress();
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行播报金额测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行播报金额测试：九档固定金额，轮间 8000 ms，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。\r\n");
                // 九档交易包含阻塞式设备调用，放到后台线程并复用统一停止令牌和进度行。
                result = await Task.Run(
                    () => _laneTransactionService.ExecuteAmountAnnouncement(
                        _desktopCommService,
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (success, total) => UpdateLaneTransactionProgress(success, total)),
                    cancellationToken);
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "播报金额测试已由用户停止。";
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException)
            {
                failureMessage = "播报金额测试失败：" + exception.Message;
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“20、拼帧交易测试（语音款）”的独立入口。
        /// </summary>
        private async void RunTestCase075()
        {
            const string testCaseName = "通用测试项 > 20、拼帧交易测试（语音款）";
            DateTime startedAt = DateTime.Now;
            ResetLaneTransactionProgress();
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行拼帧交易测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行拼帧交易测试：每轮在同一链路连续完成两笔扣费，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。\r\n");
                // 拼帧交易包含同一会话的连续阻塞式通信，放到后台线程并保留统一停止能力。
                result = await Task.Run(
                    () => _laneTransactionService.ExecuteFrameConcatenation(
                        _desktopCommService,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (success, total) => UpdateLaneTransactionProgress(success, total)),
                    cancellationToken);
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "拼帧交易测试已由用户停止。";
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException)
            {
                failureMessage = "拼帧交易测试失败：" + exception.Message;
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“21、交易后pin认证异常播报测试（语音款）”的独立入口。
        /// </summary>
        private async void RunTestCase076()
        {
            const string testCaseName = "通用测试项 > 21、交易后pin认证异常播报测试（语音款）";
            DateTime startedAt = DateTime.Now;
            ResetLaneTransactionProgress();
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行交易后 PIN 认证异常播报测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行交易后 PIN 认证异常播报测试：同链路双扣费后校验 69 82 00，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。\r\n");
                // 该固定一轮用例包含连续阻塞式交易和负向 PIN 判定，放到后台线程并接入停止令牌。
                result = await Task.Run(
                    () => _laneTransactionService.ExecutePinAuthenticationAnomaly(
                        _desktopCommService,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (success, total) => UpdateLaneTransactionProgress(success, total)),
                    cancellationToken);
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "交易后 PIN 认证异常播报测试已由用户停止。";
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException)
            {
                failureMessage = "交易后 PIN 认证异常播报测试失败：" + exception.Message;
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“22、交易25次后防拆2S失效测试”的独立入口。
        /// </summary>
        private async void RunTestCase077()
        {
            const string testCaseName = "通用测试项 > 22、交易25次后防拆2S失效测试";
            DateTime startedAt = DateTime.Now;
            ResetLaneTransactionProgress();
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法执行交易25次后防拆2S失效测试。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken cancellationToken = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failureMessage = string.Empty;
            try
            {
                SoftTradeAlgorithm algorithm = radioButtonSm4.Checked ? SoftTradeAlgorithm.Sm4 : SoftTradeAlgorithm.TripleDes;
                AppendProtocolTestInformation("开始执行交易25次后防拆2S失效测试：固定27轮完整交易，算法="
                    + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。\r\n");
                // 27轮连续硬件交互耗时较长，放入后台线程并允许停止按钮中断轮间等待。
                result = await Task.Run(
                    () => _laneTransactionService.ExecuteTrade25TamperTimeout(
                        _desktopCommService,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"),
                        algorithm,
                        cancellationToken,
                        message => AppendProtocolTestInformation(message + "\r\n"),
                        (success, total) => UpdateLaneTransactionProgress(success, total)),
                    cancellationToken);
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failureMessage = "交易25次后防拆2S失效测试已由用户停止。";
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception) when (IsNativeInteropException(exception) || exception is InvalidOperationException || exception is IOException)
            {
                failureMessage = "交易25次后防拆2S失效测试失败：" + exception.Message;
                CompleteLaneTransactionProgress();
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, string.Empty, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“23、VST回复EquipmentStatus测试”的独立入口。
        /// </summary>
        private void RunTestCase078()
        {
            // 该入口必须完成71通道动态认证后再发送DD读取命令，不能由静态协议步骤替代。
            RunEquipmentStatusTest();
        }

        /// <summary>
        /// 在后台执行原 VST EquipmentStatus 流程，并将状态值、完整交互帧和最终判定写入统一测试结果。
        /// </summary>
        private async void RunEquipmentStatusTest()
        {
            const string testCaseName = "通用测试项 > 23、VST回复EquipmentStatus测试";
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailableMessage = "未连接主窗体台发服务，无法读取 EquipmentStatus。";
                AppendProtocolTestInformation(unavailableMessage + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailableMessage, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            ObuVersionReadResult result = null;
            string failureMessage = string.Empty;
            CancellationToken cancellationToken = BeginTestExecution();
            try
            {
                AppendProtocolTestInformation("开始执行 VST EquipmentStatus 测试。\r\n");
                // 动态认证及DD读取含阻塞式 DLL 调用，转到后台线程保持测试窗口可响应。
                result = await Task.Run(() => _desktopCommService.ReadEquipmentStatus(BstCompatibilityTimeoutMilliseconds, cancellationToken));
                AppendProtocolTestInformation(result.Message + "\r\n");
                if (!string.IsNullOrEmpty(result.Version))
                {
                    AppendProtocolTestInformation("EquipmentStatus：0x" + result.Version + "\r\n");
                }
            }
            catch (OperationCanceledException)
            {
                failureMessage = "EquipmentStatus 测试已停止。";
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            catch (Exception exception)
            {
                failureMessage = "EquipmentStatus 测试失败：" + exception.Message;
                AppendProtocolTestInformation(failureMessage + "\r\n");
            }
            finally
            {
                bool isSuccess = result != null && result.IsSuccess;
                string message = result == null ? failureMessage : result.Message;
                IList<ObuProtocolExchange> exchanges = result == null ? null : result.Exchanges;
                PersistTestArtifacts(testCaseName, isSuccess, message, result == null ? string.Empty : result.Version, exchanges, startedAt, DateTime.Now);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“1、双通道圈存测试”的独立入口。
        /// </summary>
        private void RunTestCase079()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("单片式专用测试项 > 1、双通道圈存测试");
        }

        /// <summary>
        /// 执行测试用例“测试用例1：ESAM读取EF01文件，ICC读取0016文件”的独立入口。
        /// </summary>
        private void RunTestCase080()
        {
            RunProtocolSequenceTest(
                "单片式专用测试项 > 2、双通道切换测试 > 测试用例1：ESAM读取EF01文件，ICC读取0016文件",
                CreateDoubleChannelSwitchingSteps(1));
        }

        /// <summary>
        /// 执行测试用例“测试用例2：ICC读取0016文件，ESAM读取EF04文件”的独立入口。
        /// </summary>
        private void RunTestCase081()
        {
            RunProtocolSequenceTest(
                "单片式专用测试项 > 2、双通道切换测试 > 测试用例2：ICC读取0016文件，ESAM读取EF04文件",
                CreateDoubleChannelSwitchingSteps(2));
        }

        /// <summary>
        /// 执行测试用例“测试用例3：ESAM读取EF04文件，ICC读取0015文件”的独立入口。
        /// </summary>
        private void RunTestCase082()
        {
            RunProtocolSequenceTest(
                "单片式专用测试项 > 2、双通道切换测试 > 测试用例3：ESAM读取EF04文件，ICC读取0015文件",
                CreateDoubleChannelSwitchingSteps(3));
        }

        /// <summary>
        /// 执行测试用例“测试用例4：ICC读取0015文件，ESAM读取EF01文件”的独立入口。
        /// </summary>
        private void RunTestCase083()
        {
            RunProtocolSequenceTest(
                "单片式专用测试项 > 2、双通道切换测试 > 测试用例4：ICC读取0015文件，ESAM读取EF01文件",
                CreateDoubleChannelSwitchingSteps(4));
        }

        /// <summary>
        /// 执行测试用例“用例1：一次发行测试（单片）”的独立入口。
        /// </summary>
        private void RunTestCase084()
        {
            RunOneChipIssueActivationTest(
                "单片式专用测试项 > 3、发行与激活测试 > 用例1：一次发行测试（单片）",
                (token, log) => _oneChipIssueActivationService.ExecuteFirstIssue(_desktopCommService, log, token));
        }

        /// <summary>
        /// 执行测试用例“用例2：二次发行测试（单片）”的独立入口。
        /// </summary>
        private void RunTestCase085()
        {
            RunOneChipIssueActivationTest(
                "单片式专用测试项 > 3、发行与激活测试 > 用例2：二次发行测试（单片）",
                (token, log) => _oneChipIssueActivationService.ExecuteSecondIssue(_desktopCommService, log, token));
        }

        /// <summary>
        /// 执行测试用例“用例3：标签激活测试（单片）”的独立入口。
        /// </summary>
        private void RunTestCase086()
        {
            RunOneChipIssueActivationTest(
                "单片式专用测试项 > 3、发行与激活测试 > 用例3：标签激活测试（单片）",
                (token, log) => _oneChipIssueActivationService.ExecuteActivation(_desktopCommService, log, token));
        }

        /// <summary>在后台执行会永久改写ESAM/ICC数据的单片式发行或激活流程，并保存完整交互帧。</summary>
        /// <param name="testCaseName">测试树中的完整用例路径。</param>
        /// <param name="execute">服务层执行委托。</param>
        private async void RunOneChipIssueActivationTest(
            string testCaseName,
            Func<CancellationToken, Action<string>, LaneTransactionExecutionResult> execute)
        {
            DateTime startedAt = DateTime.Now;
            if (_desktopCommService == null)
            {
                const string unavailable = "未连接主窗体台发服务，无法执行单片式发行或激活测试。";
                AppendProtocolTestInformation(unavailable + "\r\n");
                PersistTestArtifacts(testCaseName, false, unavailable, string.Empty, null, startedAt, DateTime.Now);
                return;
            }

            CancellationToken token = BeginTestExecution();
            LaneTransactionExecutionResult result = null;
            string failure = string.Empty;
            try
            {
                AppendProtocolTestInformation("开始执行：" + testCaseName + "。该流程会按旧版参数永久修改标签内数据。\r\n");
                result = await Task.Run(() => execute(token, AppendProtocolTestInformation));
                AppendProtocolTestInformation(result.Message + "\r\n");
            }
            catch (OperationCanceledException)
            {
                failure = "发行或激活测试已由用户停止。";
                AppendProtocolTestInformation(failure + "\r\n");
            }
            catch (Exception exception)
            {
                failure = "发行或激活测试失败：" + exception.Message;
                AppendProtocolTestInformation(failure + "\r\n");
            }
            finally
            {
                bool success = result != null && result.IsSuccess;
                PersistTestArtifacts(testCaseName, success, result == null ? failure : result.Message, string.Empty,
                    result == null ? null : result.Exchanges, startedAt, result == null ? DateTime.Now : result.CompletedAt);
                CompleteTestExecution();
            }
        }

        /// <summary>
        /// 执行测试用例“用例1：一次发行测试（双片）”的独立入口。
        /// </summary>
        private void RunTestCase087()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("双片式专用测试项 > 1、发行与激活测试 > 用例1：一次发行测试（双片）");
        }

        /// <summary>
        /// 执行测试用例“用例2：二次发行测试（双片）”的独立入口。
        /// </summary>
        private void RunTestCase088()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("双片式专用测试项 > 1、发行与激活测试 > 用例2：二次发行测试（双片）");
        }

        /// <summary>
        /// 执行测试用例“用例3：标签激活测试（双片）”的独立入口。
        /// </summary>
        private void RunTestCase089()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("双片式专用测试项 > 1、发行与激活测试 > 用例3：标签激活测试（双片）");
        }

        /// <summary>
        /// 执行测试用例“用例1：不插卡片通过门架”的独立入口。
        /// </summary>
        private void RunTestCase090()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("双片式专用测试项 > 2、无卡机制测试 > 用例1：不插卡片通过门架");
        }

        /// <summary>
        /// 执行测试用例“用例2：低电模式通过门架”的独立入口。
        /// </summary>
        private void RunTestCase091()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("双片式专用测试项 > 2、无卡机制测试 > 用例2：低电模式通过门架");
        }

        /// <summary>
        /// 执行测试用例“用例3：天线给OBU设置不回IC卡状态通过门架”的独立入口。
        /// </summary>
        private void RunTestCase092()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("双片式专用测试项 > 2、无卡机制测试 > 用例3：天线给OBU设置不回IC卡状态通过门架");
        }

        /// <summary>
        /// 执行测试用例“1、ESAM指令检测（广东）”的独立入口。
        /// </summary>
        private void RunTestCase093()
        {
            // 独立函数先保留该用例的完整路径，后续在此处补充对应 5.8G 交互逻辑。
            RunPlaceholderTestCase("地区专用测试项 > 1、ESAM指令检测（广东）");
        }

        /// <summary>
        /// 保存一个协议序列步骤的本地类型、空口负载和时序要求。
        /// </summary>
        private sealed class ProtocolSequenceStep
        {
            /// <summary>
            /// 创建协议序列步骤。
            /// </summary>
            /// <param name="name">步骤中文名称。</param>
            /// <param name="commandType">本地透传类型。</param>
            /// <param name="payload">不含本地两字节类型的空口负载。</param>
            /// <param name="delayMilliseconds">步骤完成后等待的毫秒数。</param>
            /// <param name="expectNoResponse">是否按请求成功但接收返回非零判定成功，适用于广播 BST 和 255S 无交易配置。</param>
            /// <param name="responseValidator">可选的响应内容判定函数；为空时仅检查 DLL 请求/响应返回码。</param>
            /// <param name="validationFailureMessage">响应内容不符合判定时显示的中文原因。</param>
            /// <param name="applySessionMac">是否由通信服务显式写入当前 OBU MAC；该用例行为独立于台发 MAC 自动更新配置。</param>
            /// <param name="sessionMacVariant">基于当前 OBU MAC 生成错误地址的变体编号；0 表示不变。</param>
            internal ProtocolSequenceStep(
                string name,
                byte commandType,
                byte[] payload,
                int delayMilliseconds,
                bool expectNoResponse = false,
                Func<byte[], bool> responseValidator = null,
                string validationFailureMessage = "响应内容不符合原上位机判定标准。",
                bool applySessionMac = true,
                int sessionMacVariant = 0)
            {
                Name = name ?? string.Empty;
                CommandType = commandType;
                Payload = payload == null ? new byte[0] : (byte[])payload.Clone();
                DelayMilliseconds = Math.Max(0, delayMilliseconds);
                TimeoutMilliseconds = BstCompatibilityTimeoutMilliseconds;
                ExpectNoResponse = expectNoResponse;
                ResponseValidator = responseValidator;
                ValidationFailureMessage = string.IsNullOrWhiteSpace(validationFailureMessage)
                    ? "响应内容不符合当前步骤判定标准。"
                    : validationFailureMessage;
                ApplySessionMac = applySessionMac;
                SessionMacVariant = sessionMacVariant;
            }

            internal string Name { get; }
            internal byte CommandType { get; }
            internal byte[] Payload { get; }
            internal int DelayMilliseconds { get; }
            internal int TimeoutMilliseconds { get; }
            internal bool ExpectNoResponse { get; }
            internal Func<byte[], bool> ResponseValidator { get; }
            internal string ValidationFailureMessage { get; }
            internal bool ApplySessionMac { get; }
            internal int SessionMacVariant { get; }
        }

        /// <summary>
        /// 保存一个协议序列测试的完整交互和最终状态。
        /// </summary>
        private sealed class ProtocolSequenceExecutionResult
        {
            /// <summary>
            /// 创建协议序列执行结果。
            /// </summary>
            /// <param name="isSuccess">所有步骤是否按各自响应规则完成。</param>
            /// <param name="message">最终结果或失败原因。</param>
            /// <param name="exchanges">已执行步骤的完整请求/响应记录。</param>
            /// <param name="completedAt">执行完成时间。</param>
            internal ProtocolSequenceExecutionResult(
                bool isSuccess,
                string message,
                IList<ObuProtocolExchange> exchanges,
                DateTime completedAt)
            {
                IsSuccess = isSuccess;
                Message = message ?? string.Empty;
                Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly();
                CompletedAt = completedAt;
            }

            internal bool IsSuccess { get; }
            internal string Message { get; }
            internal IList<ObuProtocolExchange> Exchanges { get; }
            internal DateTime CompletedAt { get; }
        }

        /// <summary>
        /// 保存后台 BST 兼容性测试的通信结果，供 UI 线程完成显示和结果持久化。
        /// </summary>
        private sealed class BstCompatibilityExecutionResult
        {
            /// <summary>
            /// 创建后台 BST 兼容性测试结果。
            /// </summary>
            /// <param name="isSuccess">配置次数内的 BST/VST 交互是否全部成功。</param>
            /// <param name="message">最终测试结果或失败原因。</param>
            /// <param name="exchanges">已完成的完整请求/响应交互帧。</param>
            /// <param name="completedAt">后台流程完成时间。</param>
            internal BstCompatibilityExecutionResult(
                bool isSuccess,
                string message,
                IList<ObuProtocolExchange> exchanges,
                DateTime completedAt)
            {
                IsSuccess = isSuccess;
                Message = message ?? string.Empty;
                Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly();
                CompletedAt = completedAt;
            }

            internal bool IsSuccess { get; }
            internal string Message { get; }
            internal IList<ObuProtocolExchange> Exchanges { get; }
            internal DateTime CompletedAt { get; }
        }

        /// <summary>
        /// 保存 VST 随机避让测试的时间窗统计、完整交互帧和最终状态。
        /// </summary>
        private sealed class VstRandomDodgeExecutionResult
        {
            /// <summary>
            /// 创建随机避让时间窗执行结果。
            /// </summary>
            /// <param name="isSuccess">是否满足完成轮数和三段时间窗最低计数。</param>
            /// <param name="message">最终结果说明或失败原因。</param>
            /// <param name="exchanges">已执行步骤的完整请求/响应记录；EventReport 不包含响应占位记录。</param>
            /// <param name="time3Count">第一时间窗计数。</param>
            /// <param name="time6Count">第二时间窗计数。</param>
            /// <param name="time9Count">第三时间窗计数。</param>
            /// <param name="otherCount">不落入前三段时间窗的计数。</param>
            /// <param name="completedCount">已完成时间窗采样的轮数。</param>
            /// <param name="completedAt">后台流程完成时间。</param>
            internal VstRandomDodgeExecutionResult(
                bool isSuccess,
                string message,
                IList<ObuProtocolExchange> exchanges,
                int time3Count,
                int time6Count,
                int time9Count,
                int otherCount,
                int completedCount,
                DateTime completedAt)
            {
                IsSuccess = isSuccess;
                Message = message ?? string.Empty;
                Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly();
                Time3Count = time3Count;
                Time6Count = time6Count;
                Time9Count = time9Count;
                OtherCount = otherCount;
                CompletedCount = completedCount;
                CompletedAt = completedAt;
            }

            internal bool IsSuccess { get; }
            internal string Message { get; }
            internal IList<ObuProtocolExchange> Exchanges { get; }
            internal int Time3Count { get; }
            internal int Time6Count { get; }
            internal int Time9Count { get; }
            internal int OtherCount { get; }
            internal int CompletedCount { get; }
            internal DateTime CompletedAt { get; }
        }

    }
}
