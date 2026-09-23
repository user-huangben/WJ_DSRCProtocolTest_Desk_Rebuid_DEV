using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace WJ_ComprehensiveTest_Desk.Services
{
    /// <summary>
    /// 按原 OBU 模块软交易顺序执行备用测试项“车道交易测试”。
    /// 交易密钥和 MAC 由 SoftTradeCryptoService 在进程内计算，不访问硬件 PSAM。
    /// </summary>
    internal sealed partial class LaneTransactionService
    {
        private const int DefaultCount = 1;
        private const int DefaultIntervalMilliseconds = 8000;
        private const int ProtocolTimeoutMilliseconds = 1000;
        private const byte GantryTradeState = 0x03;
        private readonly SoftTradeCryptoService _softTradeCryptoService;

        /// <summary>创建车道交易服务。</summary>
        /// <param name="softTradeCryptoService">原软交易密钥分散和 MAC 计算服务。</param>
        internal LaneTransactionService(SoftTradeCryptoService softTradeCryptoService)
        {
            _softTradeCryptoService = softTradeCryptoService ?? throw new ArgumentNullException(nameof(softTradeCryptoService));
        }

        /// <summary>
        /// 执行一轮或多轮原车道交易交互，所有帧都保存在返回结果中。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 的完整路径。</param>
        /// <param name="algorithm">算法选择；0 为原 3DES 软算，1 为原 SM4 软算。</param>
        /// <param name="cancellationToken">停止按钮使用的取消令牌。</param>
        /// <param name="log">将配置、失败步骤和 OBU 回复分析写入测试信息显示区的回调。</param>
        /// <param name="progress">在固定界面位置更新已成功轮数和总轮数的回调。</param>
        /// <returns>包含成功次数、失败原因和完整协议帧的结果。</returns>
        internal LaneTransactionExecutionResult Execute(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int count = ReadIniInt(configurationPath, "SET_Trade_GB", "number", DefaultCount);
            int interval = ReadIniInt(configurationPath, "SET_Trade_GB", "interval", DefaultIntervalMilliseconds);
            byte[] tradeSetting = ReadTradeSetting(configurationPath);
            log("车道交易配置：次数=" + count + "，间隔=" + interval + " ms，交易金额="
                + ToHex(new byte[] { tradeSetting[0], tradeSetting[1], tradeSetting[2], tradeSetting[3] })
                + "，出入口状态=" + tradeSetting[4].ToString("X2", CultureInfo.InvariantCulture)
                + "，算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。");
            // 配置读取完成后创建唯一的成功进度位置，后续每轮仅更新计数。
            progress(0, count);

            int successCount = 0;
            string failureMessage = string.Empty;
            for (int index = 0; index < count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int sequenceNumber = index + 1;
                LaneCycleResult cycle = ExecuteCycle(
                    desktopCommService,
                    sequenceNumber,
                    tradeSetting,
                    algorithm,
                    exchanges,
                    cancellationToken,
                    log);
                if (!cycle.IsSuccess)
                {
                    failureMessage = cycle.Message;
                    break;
                }

                successCount++;
                // 一轮完整交易通过后才递增成功次数，失败轮次不计入成功值。
                progress(successCount, count);
                if (sequenceNumber < count && interval > 0)
                {
                    // 原上位机在每轮结束后等待 SET_Trade_GB/interval；等待期间可立即响应停止按钮。
                    if (cancellationToken.WaitHandle.WaitOne(interval))
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                }
            }

            bool isSuccess = successCount == count;
            string message = isSuccess
                ? "车道交易测试通过：" + successCount + "/" + count + " 轮完整流程成功。"
                : "车道交易测试未通过：成功 " + successCount + "/" + count + "。" + (string.IsNullOrEmpty(failureMessage) ? string.Empty : " 原因：" + failureMessage);
            return new LaneTransactionExecutionResult(isSuccess, message, successCount, count, exchanges, DateTime.Now);
        }

        /// <summary>
        /// 按旧版 WJ_TransactionFlow_ruansuan 执行入口、门架、出口连续交易流。
        /// </summary>
        /// <param name="desktopCommService">已按模拟真实交易流专用参数初始化的台发服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini，用于读取 SET_Trade_GB 与 TRADE_SET。</param>
        /// <param name="algorithm">界面选择的3DES或SM4软算法。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="log">步骤日志回调。</param>
        /// <param name="progress">完成交易流轮数回调。</param>
        /// <returns>每轮完整入口-门架-出口流的执行结果和所有实际交互帧。</returns>
        internal LaneTransactionExecutionResult ExecuteSimulatedRealTransactionFlow(
            DesktopCommService desktopCommService, string configurationPath, SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken, Action<string> log, Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            int count = ReadIniInt(configurationPath, "SET_Trade_GB", "number", DefaultCount);
            int interval = ReadIniInt(configurationPath, "SET_Trade_GB", "interval", DefaultIntervalMilliseconds);
            byte[] exitTradeSetting = ReadTradeSetting(configurationPath);
            byte[] entryTradeSetting = { 0x00, 0x00, 0x00, 0x00, 0x03 };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int successCount = 0;
            string failure = string.Empty;
            progress(0, count);
            log("模拟真实交易流参数：轮次=" + count + "，轮间隔=" + interval + " ms；入口金额=00000000、状态=03；门架金额="
                + ToHex(new[] { exitTradeSetting[0], exitTradeSetting[1], exitTradeSetting[2], exitTradeSetting[3] })
                + "、状态=03；出口金额与状态取 [TRADE_SET]/19=" + ToHex(exitTradeSetting) + "。");

            for (int cycle = 0; cycle < count; cycle++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                log("模拟真实交易流第 " + (cycle + 1) + "/" + count + " 轮：入口交易开始。");
                int entryExchangeIndex = exchanges.Count;
                LaneCycleResult entry = ExecuteCycle(desktopCommService, 1, entryTradeSetting, algorithm, exchanges, cancellationToken, log, 0);
                if (!entry.IsSuccess) { failure = "入口交易失败：" + entry.Message; break; }
                byte[] entryBst = GetSentBst(exchanges, entryExchangeIndex, "入口交易");

                if (cancellationToken.WaitHandle.WaitOne(500)) throw new OperationCanceledException(cancellationToken);
                if (!ExpectNoVst(desktopCommService, entryBst, exchanges, cancellationToken, log, "入口断链后同BID BST"))
                {
                    failure = "入口断链后 BST 仍收到 VST。";
                    break;
                }

                log("模拟真实交易流第 " + (cycle + 1) + "/" + count + " 轮：门架交易开始。");
                GantryCycleResult gantry = ExecuteGantryCycle(desktopCommService, cycle + 1, exitTradeSetting, algorithm, exchanges, cancellationToken, log);
                if (!gantry.IsSuccess) { failure = "门架交易失败：" + gantry.Message; break; }
                for (int index = 0; index < 30; index++)
                {
                    TransparentCommandResult mastBst = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x01 }, CreateGantryBstFrame(), exchanges, cancellationToken, "门架断链后 BST/VST " + (index + 1) + "/30", log, false);
                    if (!mastBst.IsSuccess || mastBst.ResponseData == null || mastBst.ResponseData.Length == 0)
                    {
                        failure = "门架断链后第 " + (index + 1) + "/30 次 BST/VST失败：" + mastBst.ErrorMessage;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(failure)) break;

                log("模拟真实交易流第 " + (cycle + 1) + "/" + count + " 轮：出口交易开始。");
                int exitExchangeIndex = exchanges.Count;
                LaneCycleResult exit = ExecuteCycle(desktopCommService, 2, exitTradeSetting, algorithm, exchanges, cancellationToken, log, cycle + 1);
                if (!exit.IsSuccess) { failure = "出口交易失败：" + exit.Message; break; }
                byte[] exitBst = GetSentBst(exchanges, exitExchangeIndex, "出口交易");
                if (cancellationToken.WaitHandle.WaitOne(500)) throw new OperationCanceledException(cancellationToken);
                if (!ExpectNoVst(desktopCommService, exitBst, exchanges, cancellationToken, log, "出口断链后同BID BST"))
                {
                    failure = "出口断链后 BST 仍收到 VST。";
                    break;
                }

                successCount++;
                progress(successCount, count);
                if (cycle + 1 < count && interval > 0 && cancellationToken.WaitHandle.WaitOne(interval))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            bool passed = successCount == count;
            return new LaneTransactionExecutionResult(passed,
                passed ? "模拟真实交易流测试通过：" + successCount + "/" + count + "轮入口-门架-出口流程成功。"
                    : "模拟真实交易流测试未通过：成功 " + successCount + "/" + count + "。原因：" + failure,
                successCount, count, exchanges, DateTime.Now);
        }

        /// <summary>发送断链后的 BST 并按旧用例要求确认请求成功但 VST 接收失败。</summary>
        private static bool ExpectNoVst(DesktopCommService service, byte[] bst, IList<ObuProtocolExchange> exchanges,
            CancellationToken token, Action<string> log, string stepName)
        {
            TransparentCommandResult result = ExecuteStep(service, new byte[] { 0x00, 0x01 }, bst, exchanges, token, stepName, log, false, false);
            return result.RequestResult == 0 && result.ResponseResult.HasValue && result.ResponseResult.Value != 0;
        }

        /// <summary>取得当前入口或出口阶段实际交给 DLL 的首个 BST，以便断链复发时保持其 BID 和 UnixTime。</summary>
        private static byte[] GetSentBst(IList<ObuProtocolExchange> exchanges, int startIndex, string stageName)
        {
            for (int index = startIndex; index < exchanges.Count; index++)
            {
                ObuProtocolExchange exchange = exchanges[index];
                if (exchange != null && exchange.CommandName == "BST / VST" && exchange.RequestData != null && exchange.RequestData.Length > 0)
                {
                    return (byte[])exchange.RequestData.Clone();
                }
            }
            throw new InvalidOperationException(stageName + "未找到实际下发的 BST，无法保持 BID 复发。");
        }

        /// <summary>
        /// 执行原上位机“播报金额测试（语音款）”的九档固定金额交易。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 软算法。</param>
        /// <param name="cancellationToken">停止按钮使用的取消令牌。</param>
        /// <param name="log">输出金额、语音预期和失败步骤的回调。</param>
        /// <param name="progress">更新已成功档数和总档数的回调。</param>
        /// <returns>九档金额的执行结果和完整交互帧。</returns>
        internal LaneTransactionExecutionResult ExecuteAmountAnnouncement(
            DesktopCommService desktopCommService,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            int[] amounts = { 1, 16, 562, 2000, 2232, 10000, 100162, 400000, 400001 };
            // 保持旧 WJ_AmountAnnouncementTest 的九档界面原文；这些是人工核验语音的期望内容，不按金额重新推导中文读法。
            string[] amountDisplays = { "0.01", "0.16", "5.62", "20", "22.32", "100", "1001.62", "4000", "4000.01" };
            string[] announcementDisplays =
            {
                "标签应播报：ETC消费零点零一元",
                "标签应播报：ETC消费零点一六元",
                "标签应播报：ETC消费五点六二元",
                "标签应播报：ETC消费二十元",
                "标签应播报：ETC消费二十二点三二元",
                "标签应播报：ETC消费一百元",
                "标签应播报：ETC消费一千零一点六二元",
                "标签应播报：ETC消费四千元",
                "标签应不播报，仅蜂鸣"
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            progress(0, amounts.Length);

            int successCount = 0;
            string failureMessage = string.Empty;
            for (int index = 0; index < amounts.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int amount = amounts[index];
                byte[] tradeSetting =
                {
                    (byte)(amount >> 24), (byte)(amount >> 16), (byte)(amount >> 8), (byte)amount, 0x04
                };
                log("交易金额：" + amountDisplays[index] + "元");
                log(announcementDisplays[index]);

                // 金额播报用例没有普通车道交易的 EF04 奇偶轮分支，调用其专用单轮流程。
                LaneCycleResult cycle = ExecuteAmountAnnouncementCycle(
                    desktopCommService, tradeSetting, algorithm, exchanges, cancellationToken, log);
                if (!cycle.IsSuccess)
                {
                    failureMessage = cycle.Message;
                    break;
                }

                successCount++;
                progress(successCount, amounts.Length);
                if (index + 1 < amounts.Length && cancellationToken.WaitHandle.WaitOne(DefaultIntervalMilliseconds))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            bool isSuccess = successCount == amounts.Length;
            string message = isSuccess
                ? "播报金额测试通过：9/9 档协议交易成功，请人工确认各档语音内容。"
                : "播报金额测试未通过：成功 " + successCount + "/9。原因：" + failureMessage;
            return new LaneTransactionExecutionResult(isSuccess, message, successCount, amounts.Length, exchanges, DateTime.Now);
        }

        /// <summary>
        /// 执行原上位机“拼帧交易测试（语音款）”，在同一链路内连续完成两笔配置金额交易。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 完整路径。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 软算法。</param>
        /// <param name="cancellationToken">停止按钮使用的取消令牌。</param>
        /// <param name="log">输出配置、轮次和失败步骤的回调。</param>
        /// <param name="progress">更新外层成功轮数和总轮数的回调。</param>
        /// <returns>所有配置轮次的执行结果和完整交互帧。</returns>
        internal LaneTransactionExecutionResult ExecuteFrameConcatenation(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            int count = ReadIniInt(configurationPath, "SET_Trade_GB", "number", DefaultCount);
            int interval = ReadIniInt(configurationPath, "SET_Trade_GB", "interval", DefaultIntervalMilliseconds);
            int firstAmount = ReadRequiredNonNegativeIniInt(configurationPath, "ConcateFrameTrade", "tradeCount1");
            int secondAmount = ReadRequiredNonNegativeIniInt(configurationPath, "ConcateFrameTrade", "tradeCount2");
            int[] amounts = { firstAmount, secondAmount };
            decimal announcedAmount = (decimal)firstAmount + secondAmount;
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            log(string.Format(CultureInfo.InvariantCulture,
                "拼帧交易配置：轮次={0}，间隔={1} ms，同链路金额1={2:F2}元，金额2={3:F2}元，最终播报金额={4:F2}元，状态=04，算法={5}。",
                count, interval, firstAmount / 100m, secondAmount / 100m, announcedAmount / 100m,
                algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES"));
            progress(0, count);

            int successCount = 0;
            string failureMessage = string.Empty;
            for (int index = 0; index < count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // 每轮只建立一次链路，并在一次 ICC 信息读取后连续执行两笔扣费。
                LaneCycleResult cycle = ExecuteFrameConcatenationCycle(
                    desktopCommService, amounts, algorithm, false, exchanges, cancellationToken, log);
                if (!cycle.IsSuccess)
                {
                    failureMessage = cycle.Message;
                    break;
                }

                successCount++;
                progress(successCount, count);
                if (index + 1 < count && interval > 0 && cancellationToken.WaitHandle.WaitOne(interval))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            bool isSuccess = successCount == count;
            string message = isSuccess
                ? "拼帧交易测试通过：" + successCount + "/" + count + " 轮均在同一链路完成两笔交易。"
                : "拼帧交易测试未通过：成功 " + successCount + "/" + count + "。原因：" + failureMessage;
            return new LaneTransactionExecutionResult(isSuccess, message, successCount, count, exchanges, DateTime.Now);
        }

        /// <summary>
        /// 执行交易后 PIN 认证异常播报测试：同链路完成两笔交易后，以错误 PIN 读取 0018 并期待 6982。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 完整路径。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 软算法。</param>
        /// <param name="cancellationToken">停止按钮使用的取消令牌。</param>
        /// <param name="log">输出配置及失败步骤的回调。</param>
        /// <param name="progress">更新固定一轮测试进度的回调。</param>
        /// <returns>错误 PIN 后是否收到 69 82 00 以及完整交互帧。</returns>
        internal LaneTransactionExecutionResult ExecutePinAuthenticationAnomaly(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            int[] amounts =
            {
                ReadRequiredNonNegativeIniInt(configurationPath, "ConcateFrameTrade", "tradeCount1"),
                ReadRequiredNonNegativeIniInt(configurationPath, "ConcateFrameTrade", "tradeCount2")
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            progress(0, 1);
            log(string.Format(CultureInfo.InvariantCulture,
                "PIN异常播报配置：同链路金额={0:F2}/{1:F2}元，错误PIN=88 88 88 00 00 00，预期状态=69 82 00，算法={2}。",
                amounts[0] / 100m, amounts[1] / 100m, algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES"));

            // 原用例固定一轮；复用同链路双交易核心，并在 SetMMI 前插入错误 PIN/0018 组合帧。
            LaneCycleResult cycle = ExecuteFrameConcatenationCycle(
                desktopCommService, amounts, algorithm, true, exchanges, cancellationToken, log);
            int successCount = cycle.IsSuccess ? 1 : 0;
            progress(successCount, 1);
            string message = cycle.IsSuccess
                ? "交易后 PIN 认证异常播报测试通过：错误 PIN 后读取 0018 返回 69 82 00，请人工确认语音。"
                : "交易后 PIN 认证异常播报测试未通过：" + cycle.Message;
            return new LaneTransactionExecutionResult(cycle.IsSuccess, message, successCount, 1, exchanges, DateTime.Now);
        }

        /// <summary>
        /// 执行旧版“交易25次后防拆2S失效测试”的27轮软算车道交易，并保留人工时长确认要求。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 完整路径，用于读取金额、状态及轮间等待。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 软算法。</param>
        /// <param name="cancellationToken">停止按钮使用的取消令牌。</param>
        /// <param name="log">输出配置、关键轮次及失败步骤的回调。</param>
        /// <param name="progress">更新27轮成功进度的回调。</param>
        /// <returns>27轮协议交易是否全部成功及完整交互帧；2秒现象仍需人工确认。</returns>
        internal LaneTransactionExecutionResult ExecuteTrade25TamperTimeout(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            const int totalCount = 27;
            int interval = ReadIniInt(configurationPath, "SET_Trade_GB", "interval", DefaultIntervalMilliseconds);
            byte[] tradeSetting = ReadTradeSetting(configurationPath);
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            log("防拆2S测试配置：固定27轮（前25轮累计，后2轮观察），间隔=" + interval
                + " ms，金额=" + ToHex(new byte[] { tradeSetting[0], tradeSetting[1], tradeSetting[2], tradeSetting[3] })
                + "，状态=" + tradeSetting[4].ToString("X2", CultureInfo.InvariantCulture)
                + "，交易后连续BST=关闭（旧默认），算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。");
            progress(0, totalCount);

            int successCount = 0;
            string failureMessage = string.Empty;
            for (int index = 0; index < totalCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (index == 24) log("开始第25轮：本轮完成后应触发防拆失效时间切换。");
                if (index == 25) log("开始第26轮：请观察首次失效时间是否为2秒。");
                if (index == 26) log("开始第27轮：请观察后续失效时间是否恢复为200毫秒。");

                // 旧用例的27轮交易主体与软算车道交易一致，并按轮次奇偶切换EF04读写方向。
                LaneCycleResult cycle = ExecuteCycle(
                    desktopCommService, index + 1, tradeSetting, algorithm, exchanges, cancellationToken, log, index);
                if (!cycle.IsSuccess)
                {
                    failureMessage = cycle.Message;
                    break;
                }

                successCount++;
                progress(successCount, totalCount);
                // 旧函数在每轮（包括最后一轮）结束后都等待 TradeGB_Interval。
                if (interval > 0 && cancellationToken.WaitHandle.WaitOne(interval))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            bool isSuccess = successCount == totalCount;
            string message = isSuccess
                ? "交易25次后防拆2S失效测试协议流程通过：27/27轮成功；请人工确认第25次后首次失效为2秒、后续恢复为200毫秒。"
                : "交易25次后防拆2S失效测试未通过：成功 " + successCount + "/27。原因：" + failureMessage;
            return new LaneTransactionExecutionResult(isSuccess, message, successCount, totalCount, exchanges, DateTime.Now);
        }

        /// <summary>
        /// 执行旧版 WJ_MastTrade 对应的门架交易流程。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 的完整路径。</param>
        /// <param name="algorithm">门架交易使用的软算法；3DES 或 SM4。</param>
        /// <param name="cancellationToken">停止按钮使用的取消令牌。</param>
        /// <param name="log">将配置、步骤和失败分析写入测试信息显示区的回调。</param>
        /// <param name="progress">在固定界面位置更新成功轮数和总轮数的回调。</param>
        /// <returns>包含成功次数、失败原因和完整协议帧的门架交易结果。</returns>
        internal LaneTransactionExecutionResult ExecuteGantry(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int count = ReadIniInt(configurationPath, "SET_Trade_GB", "number", DefaultCount);
            int interval = ReadIniInt(configurationPath, "SET_Trade_GB", "interval", DefaultIntervalMilliseconds);
            // 原 SMARTETC_TRADE 先把 [TRADE_SET]/19 解析到 gINI.File_19；门架专用消费初始化只取前四字节金额，状态固定为 0x03。
            byte[] gateTradeSetting = ReadTradeSetting(configurationPath);
            log("门架交易配置：次数=" + count + "，间隔=" + interval + " ms，交易金额="
                + ToHex(new byte[] { gateTradeSetting[0], gateTradeSetting[1], gateTradeSetting[2], gateTradeSetting[3] })
                + "（来源=[TRADE_SET]/19 前4字节），门架交易状态=" + GantryTradeState.ToString("X2", CultureInfo.InvariantCulture)
                + "（来源=原 ConsumeInitialize_MJ_ruanpsam 固定值；配置末字节 "
                + gateTradeSetting[4].ToString("X2", CultureInfo.InvariantCulture) + " 仅供车道交易使用）"
                + "，算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。");
            progress(0, count);

            int successCount = 0;
            string failureMessage = string.Empty;
            for (int index = 0; index < count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GantryCycleResult cycle = ExecuteGantryCycle(
                    desktopCommService, index + 1, gateTradeSetting, algorithm, exchanges, cancellationToken, log);
                if (!cycle.IsSuccess)
                {
                    failureMessage = cycle.Message;
                    break;
                }

                successCount++;
                progress(successCount, count);
                if (index + 1 < count && interval > 0 && cancellationToken.WaitHandle.WaitOne(interval))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            bool isSuccess = successCount == count;
            string message = isSuccess
                ? "门架交易测试通过：" + successCount + "/" + count + " 轮完整流程成功。"
                : "门架交易测试未通过：成功 " + successCount + "/" + count + "。"
                    + (string.IsNullOrEmpty(failureMessage) ? string.Empty : " 原因：" + failureMessage);
            return new LaneTransactionExecutionResult(isSuccess, message, successCount, count, exchanges, DateTime.Now);
        }

        /// <summary>按旧 WJ_SutongWanjiTest_255_RuanSuan 数据流执行配置驱动的 255S 保持测试。</summary>
        /// <param name="desktopCommService">已经打开、初始化且关闭 BID 自动更新的台发服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini，用于读取五字节交易金额和出入口状态。</param>
        /// <param name="bstFrames">按 BST_Locked_Sutong.ini 顺序解析的 BST 帧。</param>
        /// <param name="expectTrades">与 BST 一一对应；true 执行完整交易，false 要求无 VST。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 软算法。</param>
        /// <param name="cancellationToken">停止按钮取消令牌。</param>
        /// <param name="log">测试信息显示回调。</param>
        /// <param name="progress">完成一条配置后更新进度的回调。</param>
        /// <returns>全部配置判定、实际收发帧以及成功数量。</returns>
        internal LaneTransactionExecutionResult Execute255Keep(
            DesktopCommService desktopCommService,
            string configurationPath,
            IList<byte[]> bstFrames,
            IList<bool> expectTrades,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (bstFrames == null || expectTrades == null || bstFrames.Count == 0 || bstFrames.Count != expectTrades.Count)
                throw new ArgumentException("255S BST 与判定配置必须非空且数量一致。");
            if (desktopCommService.CurrentBidChange || desktopCommService.CurrentUnixTimeChange)
                throw new InvalidOperationException("255S 测试前必须关闭台发 BID 和 UnixTime 自动更新。");

            byte[] tradeSetting = ReadTradeSetting(configurationPath);
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int successCount = 0;
            int tradeSequence = 0;
            progress(0, bstFrames.Count);
            log("255S 参数：配置数=" + bstFrames.Count + "，金额=" + ToHex(new byte[] { tradeSetting[0], tradeSetting[1], tradeSetting[2], tradeSetting[3] })
                + "，出入口状态=" + tradeSetting[4].ToString("X2", CultureInfo.InvariantCulture)
                + "，算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "，BID自动更新=关闭，UnixTime自动更新=关闭。");

            for (int index = 0; index < bstFrames.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool expectTrade = expectTrades[index];
                log(string.Format("255S 配置 {0}/{1}：期望{2}。", index + 1, bstFrames.Count, expectTrade ? "完整交易" : "不响应 VST"));
                // 配置中的 BST 原样下发：第 1～5 轮保持 BID，第 5 轮 UnixTime 相对首轮固定增加 556 秒，第 6 轮仅改变 BID。
                TransparentCommandResult bstResult = ExecuteStep(desktopCommService, new byte[] { 0, 1 }, bstFrames[index], exchanges, cancellationToken, "255S BST/VST", log, false);

                if (!expectTrade)
                {
                    bool noResponse = bstResult.RequestResult == 0 && bstResult.ResponseResult.HasValue && bstResult.ResponseResult.Value != 0;
                    if (!noResponse)
                    {
                        return new LaneTransactionExecutionResult(false, "第 " + (index + 1) + " 条应不交易，但 OBU 返回了 VST或BST发送失败。", successCount, bstFrames.Count, exchanges, DateTime.Now);
                    }
                    successCount++;
                    progress(successCount, bstFrames.Count);
                    // 旧无交易分支在 VST 超时后额外等待 2 秒再进入下一配置。
                    if (cancellationToken.WaitHandle.WaitOne(2000)) throw new OperationCanceledException(cancellationToken);
                    continue;
                }

                if (!bstResult.IsSuccess || bstResult.ResponseData == null || bstResult.ResponseData.Length == 0)
                    return new LaneTransactionExecutionResult(false, "第 " + (index + 1) + " 条应交易，但未收到有效 VST。", successCount, bstFrames.Count, exchanges, DateTime.Now);
                int currentTradeSequence = tradeSequence;
                tradeSequence = (tradeSequence + 1) % 0xFF;
                LaneCycleResult cycle = Execute255TradeAfterVst(desktopCommService, bstResult.ResponseData, tradeSetting, currentTradeSequence, algorithm, exchanges, cancellationToken, log);
                if (!cycle.IsSuccess)
                    return new LaneTransactionExecutionResult(false, "第 " + (index + 1) + " 条完整交易失败：" + cycle.Message, successCount, bstFrames.Count, exchanges, DateTime.Now);

                successCount++;
                progress(successCount, bstFrames.Count);
                // 旧交易分支发送 EventReport 后等待 10 秒，保证后续时间戳 BST 按 255S 规则判定。
                if (cancellationToken.WaitHandle.WaitOne(10000)) throw new OperationCanceledException(cancellationToken);
            }

            return new LaneTransactionExecutionResult(true, "255S 保持测试通过：" + successCount + "/" + bstFrames.Count + " 条配置符合预期。", successCount, bstFrames.Count, exchanges, DateTime.Now);
        }

        /// <summary>在已经收到配置 BST 对应 VST 后，执行旧 255S 软算完整交易剩余步骤。</summary>
        /// <param name="desktopCommService">已初始化且关闭 BID 自动更新的台发服务。</param><param name="vstResponse">当前 BST 返回的完整 VST。</param>
        /// <param name="tradeSetting">五字节金额和出入口状态。</param><param name="sequenceNumber">本次测试内完整交易序号。</param><param name="algorithm">3DES 或 SM4。</param>
        /// <param name="exchanges">追加实际交互帧的集合。</param><param name="cancellationToken">停止令牌。</param><param name="log">阶段日志回调。</param>
        /// <returns>完整交易剩余步骤是否全部成功；SetMMI 按旧程序不作为失败条件。</returns>
        private LaneCycleResult Execute255TradeAfterVst(
            DesktopCommService desktopCommService, byte[] vstResponse, byte[] tradeSetting, int sequenceNumber,
            SoftTradeAlgorithm algorithm, IList<ObuProtocolExchange> exchanges, CancellationToken cancellationToken, Action<string> log)
        {
            VstSnapshot vst = ParseVst(vstResponse);
            if (vst == null || vst.Icc0015 == null || vst.Icc0015.Length < 20) return Fail("VST 缺少软算所需的 ICC0015。");
            byte llc = 0x77;
            TransparentCommandResult result = ExecuteStep(desktopCommService, new byte[] { 0, 2 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, cancellationToken, "255S 读取车辆信息", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("读取车辆信息", result.ErrorMessage, result, false, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0, 3 }, CreateGetIccFrame(vst.MacId, llc), exchanges, cancellationToken, "255S 读取 ICC 信息", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("读取 ICC 信息", result.ErrorMessage, result, true, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0, 3 }, Create255ConsumeInitializeFrame(vst.MacId, llc, tradeSetting, sequenceNumber, algorithm), exchanges, cancellationToken, "255S 消费初始化", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("消费初始化", result.ErrorMessage, result, true, log);
            byte[] tradeTime, mac1;
            string cryptoError;
            if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, result.ResponseData, tradeSetting, out tradeTime, out mac1, out cryptoError)) return Fail("MAC1计算失败：" + cryptoError);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0, 3 }, CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, cancellationToken, "255S 扣费", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("扣费", result.ErrorMessage, result, true, log);

            llc = ToggleLlc(llc);
            // 旧 255S 代码明确忽略 SetMMI 失败，只记录实际请求/响应后继续断链。
            ExecuteStep(desktopCommService, new byte[] { 0, 4 }, CreateSetMmiFrame(vst.MacId, llc), exchanges, cancellationToken, "255S SetMMI（非强制）", log, true);
            result = ExecuteStep(desktopCommService, new byte[] { 0, 5 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "255S EventReport（单向）", log, false);
            if (!result.IsSuccess) return FailStep("EventReport", result.ErrorMessage, result, false, log);
            return new LaneCycleResult(true, string.Empty);
        }

        /// <summary>创建旧 255S 软算函数使用的固定时间字段消费初始化帧。</summary>
        /// <param name="macId">当次 VST 返回的四字节 MACID。</param><param name="llc">当前由测试流程交替维护的 LLC。</param>
        /// <param name="tradeSetting">[TRADE_SET]/19 的前四字节金额及末字节出入口状态。</param><param name="sequenceNumber">从 0 开始并按旧全局变量规则在 0～254 循环的交易序号。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4。</param>
        /// <returns>79 字节 TransferChannel 请求；时间字段保持旧模板 `1A A1 DD 02`。</returns>
        private static byte[] Create255ConsumeInitializeFrame(byte[] macId, byte llc, byte[] tradeSetting, int sequenceNumber, SoftTradeAlgorithm algorithm)
        {
            byte[] frame = CreateConsumeInitializeFrame(macId, llc, tradeSetting, sequenceNumber, algorithm);
            // ConsumeInitialize_GB_ruanpsam 没有动态更新时间字段，必须撤销普通车道构造器的 UTC 覆盖。
            frame[44] = 0x1A;
            frame[45] = 0xA1;
            frame[46] = 0xDD;
            frame[47] = 0x02;
            return frame;
        }

        /// <summary>
        /// 执行旧版 WJ_MastTrade 的一轮门架交易：门架 BST/VST、车辆信息、ICC、EF04、消费初始化、扣费和断链通知。
        /// </summary>
        /// <param name="desktopCommService">台发通信服务。</param>
        /// <param name="sequenceNumber">当前轮次，从 1 开始。</param>
        /// <param name="tradeSetting">[TRADE_SET]/19 的 5 字节配置；门架仅使用前 4 字节金额，状态按原流程固定为 0x03。</param>
        /// <param name="algorithm">3DES 或 SM4。</param>
        /// <param name="exchanges">完整交互帧输出列表。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="log">步骤日志回调。</param>
        /// <returns>本轮成功或失败。</returns>
        private GantryCycleResult ExecuteGantryCycle(
            DesktopCommService desktopCommService,
            int sequenceNumber,
            byte[] tradeSetting,
            SoftTradeAlgorithm algorithm,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken cancellationToken,
            Action<string> log)
        {
            byte[] sessionMacId = null;
            bool eventReportAttempted = false;
            try
            {
                TransparentCommandResult result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x01 }, CreateGantryBstFrame(), exchanges, cancellationToken, "门架 BST / VST", log, false);
                if (!result.IsSuccess || result.ResponseData == null || result.ResponseData.Length == 0) return FailGantryStep("门架 BST/VST", "未收到有效 VST。" + result.ErrorMessage, result, false, log);

                VstSnapshot vst = ParseVst(result.ResponseData);
                if (vst != null && vst.MacId != null && vst.MacId.Length >= 4) sessionMacId = (byte[])vst.MacId.Clone();
                if (vst == null || vst.Icc0015 == null || vst.Icc0015.Length < 20)
                {
                    return new GantryCycleResult(false, "VST 未包含可用于门架交易 MAC1 的完整 ICC0015。");
                }

                byte llc = 0x77;
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x02 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, cancellationToken, "门架读取车辆信息 GetSecure", log, true);
                if (!IsResponseSuccessful(result, false)) return FailGantryStep("门架 GetSecure", "未收到有效回复。" + result.ErrorMessage, result, false, log);

                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateGetIccFrame(vst.MacId, llc), exchanges, cancellationToken, "门架读取 ICC 0019/0002", log, true);
                if (!IsResponseSuccessful(result, true)) return FailGantryStep("门架读取 ICC", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

                llc = ToggleLlc(llc);
                // 门架旧流程的 EF04 读取 APDU 与现有车道流程相同，仅由门架专用步骤顺序调用。
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateReadEsamFrame(vst.MacId, llc), exchanges, cancellationToken, "门架读取 ESAM EF04", log, true);
                if (!IsResponseSuccessful(result, true)) return FailGantryStep("门架读取 ESAM EF04", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateGantryWriteEsamFrame(vst.MacId, llc), exchanges, cancellationToken, "门架写入 ESAM EF04", log, true);
                if (!IsResponseSuccessful(result, true)) return FailGantryStep("门架写入 ESAM EF04", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

                llc = ToggleLlc(llc);
                // 门架消费初始化必须先写入与 MAC1 一致的终端交易序号，避免 OBU 使用报文中的序号验算时返回 93 02。
                byte[] consumeInitializeFrame = CreateGantryConsumeInitializeFrame(vst.MacId, llc, tradeSetting, algorithm);
                log("门架消费初始化关键字段：算法标识=" + consumeInitializeFrame[19].ToString("X2", CultureInfo.InvariantCulture)
                    + "，金额=" + ToHex(new byte[] { consumeInitializeFrame[20], consumeInitializeFrame[21], consumeInitializeFrame[22], consumeInitializeFrame[23] })
                    + "，终端交易序号=" + ToHex(new byte[] { consumeInitializeFrame[24], consumeInitializeFrame[25], consumeInitializeFrame[26], consumeInitializeFrame[27], consumeInitializeFrame[28], consumeInitializeFrame[29] })
                    + "，出入口状态=" + consumeInitializeFrame[49].ToString("X2", CultureInfo.InvariantCulture) + "。");
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, consumeInitializeFrame, exchanges, cancellationToken, "门架消费初始化", log, true);
                if (!IsResponseSuccessful(result, true)) return FailGantryStep("门架消费初始化", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

                byte[] tradeTime;
                byte[] mac1;
                string cryptoError;
                // 消费初始化返回 EP 和随机数后，沿用现有进程内软算服务生成门架扣费 MAC1。
                if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, result.ResponseData, tradeSetting, out tradeTime, out mac1, out cryptoError))
                {
                    return new GantryCycleResult(false, "门架软算法计算 MAC1 失败：" + cryptoError);
                }
                log("门架 MAC1 计算结果：算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES")
                    + "，交易时间=" + ToHex(tradeTime) + "，MAC1=" + ToHex(mac1) + "。");

                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateGantryConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, cancellationToken, "门架交易扣费", log, true);
                if (!IsResponseSuccessful(result, true)) return FailGantryStep("门架交易扣费", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

                // 旧版 WJ_MastTrade 在 Consume_MJ 后直接发送 EventReport，不再追加 SetMMI。
                llc = ToggleLlc(llc);
                eventReportAttempted = true;
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "门架发送 EventReport（单向）", log, false);
                if (!result.IsSuccess) return FailGantryStep("门架 EventReport（单向）", "帧发送失败。" + result.ErrorMessage, result, false, log);
                return new GantryCycleResult(true, string.Empty);
            }
            finally
            {
                // 旧版 WJ_MastTrade 的 _err 出口会无条件发送 EventReport；失败后也必须释放 OBU 会话，
                // 否则下一轮可能沿用未完成的消费状态并重复返回 93 02。
                if (sessionMacId != null && !eventReportAttempted && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        eventReportAttempted = true;
                        TransparentCommandResult cleanup = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(sessionMacId), exchanges, cancellationToken, "门架失败后 EventReport（单向清理）", log, false);
                        if (!cleanup.IsSuccess) log("门架失败后 EventReport 清理发送失败：" + cleanup.ErrorMessage);
                    }
                    catch (Exception cleanupException)
                    {
                        log("门架失败后 EventReport 清理异常：" + cleanupException.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 按原 OBU 防碰撞用例执行一轮软算交易，并读取台发返回的五段处理时间。
        /// </summary>
        /// <param name="desktopCommService">已经打开并初始化的台发通信服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 完整路径，用于读取交易金额和防碰撞目标计数。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 软算法。</param>
        /// <param name="cancellationToken">停止按钮提供的取消令牌；每次设备调用前检查。</param>
        /// <param name="log">向测试信息显示框追加阶段结果的回调。</param>
        /// <returns>包含五段校正时间、最终判定及完整空中交互帧的结果。</returns>
        internal AntiCollisionExecutionResult ExecuteAntiCollision(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (string.IsNullOrWhiteSpace(configurationPath)) throw new ArgumentException("配置文件路径不能为空。", nameof(configurationPath));
            if (log == null) throw new ArgumentNullException(nameof(log));

            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int expectedTimingCount = ReadIniInt(configurationPath, "SET_139_AntiCol_num", "number", 5);
            if (expectedTimingCount != 5)
            {
                return new AntiCollisionExecutionResult(false, "防碰撞配置错误：[SET_139_AntiCol_num] number 必须为 5。", null, exchanges, DateTime.Now);
            }

            byte[] tradeSetting = ReadTradeSetting(configurationPath);
            // 原 OBU 防碰撞流程在消费初始化前强制 File_19[3]=0x06，台发以此标记统计五段处理时间。
            // 该副本同时传入消费初始化和软算 MAC1，确保扣费帧与 OBU 会话中的金额完全一致。
            tradeSetting[3] = 0x06;
            byte llc = 0x77;
            log("开始执行防碰撞测试：一轮完整软算交易，算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。");

            TransparentCommandResult result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x01 }, CreateAntiCollisionBstFrame(), exchanges, cancellationToken, "BST/VST", log, false);
            if (!result.IsSuccess || result.ResponseData.Length == 0) return FailAntiCollision("BST/VST", result, false, exchanges, log);
            VstSnapshot vst = ParseVst(result.ResponseData);
            if (vst == null)
            {
                return new AntiCollisionExecutionResult(false, "VST 结构不完整，无法取得本次会话 MACID。", null, exchanges, DateTime.Now);
            }

            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x02 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, cancellationToken, "GetSecure", log, true);
            if (!IsResponseSuccessful(result, false)) return FailAntiCollision("GetSecure", result, false, exchanges, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateAntiCollisionCardReadFrame(vst.MacId, llc), exchanges, cancellationToken, "读取余额、0015、0019", log, true);
            if (!IsResponseSuccessful(result, true)) return FailAntiCollision("读取余额、0015、0019", result, true, exchanges, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateConsumeInitializeFrame(vst.MacId, llc, tradeSetting, 1, algorithm), exchanges, cancellationToken, "消费初始化并更新0019", log, true);
            if (!IsResponseSuccessful(result, true)) return FailAntiCollision("消费初始化并更新0019", result, true, exchanges, log);
            byte[] consumeInitializeResponse = result.ResponseData;

            byte[] tradeTime;
            byte[] mac1;
            string cryptoError;
            // 消费初始化返回 EP 和随机数后，按界面算法在进程内生成扣费所需 MAC1。
            // 原防碰撞函数只校验读卡 TransferChannel 的返回状态，并未解码该回复覆盖 gVST.ICC0015；
            // 软算继续使用当前 VST 预读的 ICC0015，保持与普通车道交易相同的密钥分散因子。
            if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, consumeInitializeResponse, tradeSetting, out tradeTime, out mac1, out cryptoError))
            {
                return new AntiCollisionExecutionResult(false, "软算法计算 MAC1 失败：" + cryptoError, null, exchanges, DateTime.Now);
            }

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateTransactionAuthenticationFrame(vst.MacId, llc, consumeInitializeResponse), exchanges, cancellationToken, "获取交易认证", log, true);
            if (!IsResponseSuccessful(result, true)) return FailAntiCollision("获取交易认证", result, true, exchanges, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, cancellationToken, "交易扣费", log, true);
            if (!IsResponseSuccessful(result, true)) return FailAntiCollision("交易扣费", result, true, exchanges, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x04 }, CreateSetMmiFrame(vst.MacId, llc), exchanges, cancellationToken, "SetMMI", log, true);
            if (!IsResponseSuccessful(result, false)) return FailAntiCollision("SetMMI", result, false, exchanges, log);

            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x06 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "读取五段处理时间", log, true);
            if (!result.IsSuccess || result.ResponseData == null || result.ResponseData.Length < 20) return FailAntiCollision("读取五段处理时间", result, false, exchanges, log);

            double[] timings = ParseAntiCollisionTimings(result.ResponseData);
            string[] names = { "GetSecure", "读取卡片信息", "消费初始化并更新0019", "获取交易认证", "交易扣费" };
            int successCount = 0;
            for (int index = 0; index < timings.Length; index++)
            {
                bool timingPassed = timings[index] > 0 && timings[index] < 8;
                if (timingPassed) successCount++;
                log(string.Format(CultureInfo.InvariantCulture, "{0}时间：{1:F4} ms，{2}。", names[index], timings[index], timingPassed ? "合格" : "不合格"));
            }

            cancellationToken.ThrowIfCancellationRequested();
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "EventReport 单向断链", log, false);
            if (!result.IsSuccess) return FailAntiCollision("EventReport 单向断链", result, false, exchanges, log);

            bool passed = successCount == expectedTimingCount;
            string message = string.Format("防碰撞测试{0}：五段 8 ms 时间窗合格 {1}/5。", passed ? "通过" : "未通过", successCount);
            return new AntiCollisionExecutionResult(passed, message, timings, exchanges, DateTime.Now);
        }

        /// <summary>
        /// 生成防碰撞步骤失败结果，并复用逐 APDU 的 OBU 回复分析。
        /// </summary>
        /// <param name="stepName">失败的交易阶段名称。</param>
        /// <param name="result">当前设备调用结果。</param>
        /// <param name="expectSuccessfulStatus">是否按 TransferChannel 结构逐条检查 APDU 状态码。</param>
        /// <param name="exchanges">截至失败时已经记录的完整空中帧。</param>
        /// <param name="log">测试信息显示回调。</param>
        /// <returns>失败的防碰撞执行结果。</returns>
        private static AntiCollisionExecutionResult FailAntiCollision(string stepName, TransparentCommandResult result, bool expectSuccessfulStatus, IList<ObuProtocolExchange> exchanges, Action<string> log)
        {
            string analysis = AnalyzeObuResponse(result, expectSuccessfulStatus);
            log("失败步骤：" + stepName + "；原因：" + analysis);
            return new AntiCollisionExecutionResult(false, stepName + "失败：" + analysis, null, exchanges, DateTime.Now);
        }

        /// <summary>执行播报金额测试的一档交易，不插入普通车道用例的 EF04 读写步骤。</summary>
        /// <param name="desktopCommService">台发通信服务。</param>
        /// <param name="tradeSetting">四字节固定金额和固定入口状态 03。</param>
        /// <param name="algorithm">界面选择的软算算法。</param>
        /// <param name="exchanges">完整交互帧输出列表。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="log">阶段日志回调。</param>
        /// <returns>当前金额档位是否完成协议交易。</returns>
        private LaneCycleResult ExecuteAmountAnnouncementCycle(
            DesktopCommService desktopCommService,
            byte[] tradeSetting,
            SoftTradeAlgorithm algorithm,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken cancellationToken,
            Action<string> log)
        {
            TransparentCommandResult result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x01 }, CreateLaneBstFrame(), exchanges, cancellationToken, "发送 BST / 接收 VST", log, false);
            if (!result.IsSuccess || result.ResponseData.Length == 0) return FailStep("BST/VST", "未收到有效 VST。" + result.ErrorMessage, result, false, log);

            VstSnapshot vst = ParseVst(result.ResponseData);
            if (vst == null || vst.Icc0015 == null || vst.Icc0015.Length < 20)
            {
                return FailStep("VST 数据解析", "VST 未包含消费软算所需的 ICC0015。", result, false, log);
            }

            byte llc = 0x77;
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x02 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, cancellationToken, "读取车辆信息 GetSecure（软算法）", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("读取车辆信息 GetSecure", "未收到有效 GetSecure 回复。" + result.ErrorMessage, result, false, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateGetIccFrame(vst.MacId, llc), exchanges, cancellationToken, "读取 ICC 0019/0002", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("读取 ICC 0019/0002", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateWjConsumeInitializeFrame(vst.MacId, llc, tradeSetting, algorithm), exchanges, cancellationToken, "消费初始化并更新 0019", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("消费初始化并更新 0019", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            byte[] tradeTime;
            byte[] mac1;
            string cryptoError;
            if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, result.ResponseData, tradeSetting, out tradeTime, out mac1, out cryptoError))
            {
                return Fail("软算法计算 MAC1 失败：" + cryptoError);
            }

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, cancellationToken, "扣费 Consume", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("扣费 Consume", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x04 }, CreateSetMmiFrame(vst.MacId, llc), exchanges, cancellationToken, "发送 SetMMI", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("SetMMI", "未收到有效 SetMMI 回复。" + result.ErrorMessage, result, false, log);

            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "发送 EventReport（单向）", log, false);
            if (!result.IsSuccess) return FailStep("EventReport（单向）", "帧发送失败。" + result.ErrorMessage, result, false, log);
            return new LaneCycleResult(true, string.Empty);
        }

        /// <summary>执行一轮拼帧交易，在一个 MAC 会话中按配置顺序完成两次消费。</summary>
        /// <param name="desktopCommService">台发通信服务。</param>
        /// <param name="amounts">两笔以分为单位的非负交易金额。</param>
        /// <param name="algorithm">界面选择的软算算法。</param>
        /// <param name="verifyPinAnomaly">是否在两笔交易后插入错误 PIN 与 0018 读取组合帧，并要求帧尾 69 82 00。</param>
        /// <param name="exchanges">完整交互帧输出列表。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="log">阶段日志回调。</param>
        /// <returns>两笔交易以及链路结束步骤是否全部成功。</returns>
        private LaneCycleResult ExecuteFrameConcatenationCycle(
            DesktopCommService desktopCommService,
            int[] amounts,
            SoftTradeAlgorithm algorithm,
            bool verifyPinAnomaly,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken cancellationToken,
            Action<string> log)
        {
            TransparentCommandResult result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x01 }, CreateLaneBstFrame(), exchanges, cancellationToken, "发送 BST / 接收 VST", log, false);
            if (!result.IsSuccess || result.ResponseData.Length == 0) return FailStep("BST/VST", "未收到有效 VST。" + result.ErrorMessage, result, false, log);
            VstSnapshot vst = ParseVst(result.ResponseData);
            if (vst == null || vst.Icc0015 == null || vst.Icc0015.Length < 20)
            {
                return FailStep("VST 数据解析", "VST 未包含消费软算所需的 ICC0015。", result, false, log);
            }

            byte llc = 0x77;
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x02 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, cancellationToken, "读取车辆信息 GetSecure（软算法）", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("读取车辆信息 GetSecure", "未收到有效 GetSecure 回复。" + result.ErrorMessage, result, false, log);
            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateGetIccFrame(vst.MacId, llc), exchanges, cancellationToken, "读取 ICC 0019/0002", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("读取 ICC 0019/0002", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            for (int transactionIndex = 0; transactionIndex < amounts.Length; transactionIndex++)
            {
                int amount = amounts[transactionIndex];
                byte[] tradeSetting =
                {
                    (byte)(amount >> 24), (byte)(amount >> 16), (byte)(amount >> 8), (byte)amount, 0x04
                };
                log(string.Format(CultureInfo.InvariantCulture, "同链路第 {0}/2 笔：{1:F2} 元。", transactionIndex + 1, amount / 100m));
                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateWjConsumeInitializeFrame(vst.MacId, llc, tradeSetting, algorithm), exchanges, cancellationToken, "消费初始化并更新 0019", log, true);
                if (!IsResponseSuccessful(result, true)) return FailStep("第 " + (transactionIndex + 1) + " 笔消费初始化", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

                byte[] tradeTime;
                byte[] mac1;
                string cryptoError;
                if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, result.ResponseData, tradeSetting, out tradeTime, out mac1, out cryptoError))
                {
                    return Fail("第 " + (transactionIndex + 1) + " 笔软算法计算 MAC1 失败：" + cryptoError);
                }

                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, cancellationToken, "扣费 Consume", log, true);
                if (!IsResponseSuccessful(result, true)) return FailStep("第 " + (transactionIndex + 1) + " 笔扣费 Consume", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);
            }

            if (verifyPinAnomaly)
            {
                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreatePinAnomalyRead0018Frame(vst.MacId, llc), exchanges, cancellationToken, "错误 PIN 认证并读取 0018 第1～4条", log, true);
                if (!result.IsSuccess || !EndsWith(result.ResponseData, new byte[] { 0x69, 0x82, 0x00 }))
                {
                    return FailStep("错误 PIN 认证并读取 0018", "回复帧尾不是预期的 69 82 00。" + result.ErrorMessage, result, false, log);
                }
            }

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x04 }, CreateSetMmiFrame(vst.MacId, llc), exchanges, cancellationToken, "发送 SetMMI", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("SetMMI", "未收到有效 SetMMI 回复。" + result.ErrorMessage, result, false, log);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "发送 EventReport（单向）", log, false);
            if (!result.IsSuccess) return FailStep("EventReport（单向）", "帧发送失败。" + result.ErrorMessage, result, false, log);
            return new LaneCycleResult(true, string.Empty);
        }

        /// <summary>执行原车道交易的一轮协议步骤。</summary>
        /// <param name="desktopCommService">台发通信服务。</param>
        /// <param name="sequenceNumber">当前轮次，从 1 开始。</param>
        /// <param name="tradeSetting">配置文件 [TRADE_SET]/19 解析出的 4 字节扣费金额和 1 字节出入口状态。</param>
        /// <param name="algorithm">界面选择的原上位机软算算法。</param>
        /// <param name="exchanges">完整交互帧输出列表。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="log">阶段日志回调。</param>
        /// <param name="transactionSerialOverride">消费初始化交易序号覆盖值；负数表示沿用从1开始的轮次号。</param>
        /// <returns>本轮成功或失败。</returns>
        private LaneCycleResult ExecuteCycle(
            DesktopCommService desktopCommService,
            int sequenceNumber,
            byte[] tradeSetting,
            SoftTradeAlgorithm algorithm,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken cancellationToken,
            Action<string> log,
            int transactionSerialOverride = -1)
        {
            byte[] bst = CreateLaneBstFrame();
            TransparentCommandResult result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x01 }, bst, exchanges, cancellationToken, "发送 BST / 接收 VST", log, false);
            if (!result.IsSuccess || result.ResponseData.Length == 0) return FailStep("BST/VST", "未收到有效 VST。" + result.ErrorMessage, result, false, log);

            VstSnapshot vst = ParseVst(result.ResponseData);
            if (vst == null || vst.Icc0015 == null || vst.Icc0015.Length < 20)
            {
                return FailStep("VST 数据解析", "VST 未包含可用于车道交易 MAC1 的完整 ICC0015（需要 0015 预读数据）。", result, false, log);
            }

            byte llc = 0x77;
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x02 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, cancellationToken, "读取车辆信息 GetSecure（软算法）", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("读取车辆信息 GetSecure", "未收到有效 GetSecure 回复。" + result.ErrorMessage, result, false, log);

            // 原 WJ_Trade_3DES 只在奇数轮读出 EF04，偶数轮保留上一轮状态。
            if ((sequenceNumber - 1) % 2 != 0)
            {
                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateReadEsamFrame(vst.MacId, llc), exchanges, cancellationToken, "读取 ESAM EF04", log, true);
                if (!IsResponseSuccessful(result, true)) return FailStep("读取 ESAM EF04", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);
            }

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateGetIccFrame(vst.MacId, llc), exchanges, cancellationToken, "读取 ICC 0019/0002", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("读取 ICC 0019/0002", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            // 原流程第 1 轮及后续偶数轮写入 EF04，奇数轮在扣费后写回 EF04。
            if (sequenceNumber == 1 || (sequenceNumber - 1) % 2 == 0)
            {
                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateWriteEsamInFrame(vst.MacId, llc), exchanges, cancellationToken, "写入 ESAM EF04", log, true);
                if (!IsResponseSuccessful(result, true)) return FailStep("写入 ESAM EF04", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);
            }

            llc = ToggleLlc(llc);
            // 防拆25次用例沿用旧全局 trade_num_25times 的00起始序号；其他车道交易保持原有轮次序号。
            int transactionSerial = transactionSerialOverride >= 0 ? transactionSerialOverride : sequenceNumber;
            byte[] consumeInitialize = CreateConsumeInitializeFrame(vst.MacId, llc, tradeSetting, transactionSerial, algorithm);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, consumeInitialize, exchanges, cancellationToken, "消费初始化（软算法）", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("消费初始化", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            byte[] tradeTime;
            byte[] mac1;
            string cryptoError;
            // 消费初始化返回 EP、随机数和交易时间后，按界面选中的软算法计算 MAC1。
            if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, result.ResponseData, tradeSetting, out tradeTime, out mac1, out cryptoError))
            {
                log("\r\n失败步骤：软算法计算 MAC1\r\n失败原因：" + cryptoError + " 未发送扣费帧。");
                return Fail(cryptoError + " 未发送扣费帧。");
            }

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, cancellationToken, "扣费 Consume", log, true);
            if (!IsResponseSuccessful(result, true)) return FailStep("扣费 Consume", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);

            if ((sequenceNumber - 1) % 2 != 0)
            {
                llc = ToggleLlc(llc);
                result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x03 }, CreateWriteEsamOutFrame(vst.MacId, llc), exchanges, cancellationToken, "写回 ESAM EF04", log, true);
                if (!IsResponseSuccessful(result, true)) return FailStep("写回 ESAM EF04", "回复未通过成功状态校验。" + result.ErrorMessage, result, true, log);
            }

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x04 }, CreateSetMmiFrame(vst.MacId, llc), exchanges, cancellationToken, "发送 SetMMI", log, true);
            if (!IsResponseSuccessful(result, false)) return FailStep("SetMMI", "未收到有效 SetMMI 回复。" + result.ErrorMessage, result, false, log);

            llc = ToggleLlc(llc);
            result = ExecuteStep(desktopCommService, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(vst.MacId), exchanges, cancellationToken, "发送 EventReport（单向）", log, false);
            if (!result.IsSuccess) return FailStep("EventReport（单向）", "帧发送失败。" + result.ErrorMessage, result, false, log);
            return new LaneCycleResult(true, string.Empty);
        }

        /// <summary>调用一条透传命令并保存请求/响应帧。</summary>
        /// <param name="desktopCommService">台发通信服务。</param>
        /// <param name="commandPrefix">本地透传类型前缀，0001～0005。</param>
        /// <param name="payload">空口帧，不含本地类型前缀。</param>
        /// <param name="exchanges">完整交互帧输出列表。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="stepName">显示用步骤名称。</param>
        /// <param name="log">步骤日志回调。</param>
        /// <param name="applyMac">是否由服务层按当前会话 MAC 更新请求。</param>
        /// <returns>DLL 执行结果。</returns>
        internal static TransparentCommandResult ExecuteStep(
            DesktopCommService desktopCommService,
            byte[] commandPrefix,
            byte[] payload,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken cancellationToken,
            string stepName,
            Action<string> log,
            bool applyMac,
            bool applyBstDynamicFields = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] command = new byte[commandPrefix.Length + payload.Length];
            Buffer.BlockCopy(commandPrefix, 0, command, 0, commandPrefix.Length);
            Buffer.BlockCopy(payload, 0, command, commandPrefix.Length, payload.Length);
            TransparentCommandResult result = desktopCommService.ExecuteTransparent(command, ProtocolTimeoutMilliseconds, applyMac, 0, applyBstDynamicFields);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
            return result;
        }

        /// <summary>
        /// 输出车道交易失败步骤、该步骤的请求/回复帧及基于 OBU 回复的失败分析。
        /// </summary>
        /// <param name="stepName">失败的业务协议步骤名称。</param>
        /// <param name="reason">业务层已确认的失败原因。</param>
        /// <param name="result">失败步骤的通信结果，包含实际空中请求和 OBU 回复。</param>
        /// <param name="expectSuccessfulStatus">该步骤是否要求回复末尾严格为 90 00 00。</param>
        /// <param name="log">测试信息显示回调，不得为空。</param>
        /// <returns>包含同一失败原因的单轮失败结果。</returns>
        private static LaneCycleResult FailStep(
            string stepName,
            string reason,
            TransparentCommandResult result,
            bool expectSuccessfulStatus,
            Action<string> log)
        {
            // 原位成功计数没有换行，失败详情入口先换行以保持信息分区清晰。
            log("\r\n失败步骤：" + stepName);
            if (result != null)
            {
                log("下行请求 Len=" + result.RequestData.Length + "：" + FormatAirFrame(result.RequestData));
                log(result.ResponseData.Length > 0
                    ? "OBU 上行回复 Len=" + result.ResponseData.Length + "：" + FormatAirFrame(result.ResponseData)
                    : "OBU 上行回复：未收到有效数据。");
                // 失败入口结合通信状态和回复末尾状态码，给出可直接定位的协议判断。
                log("OBU 回复分析：" + AnalyzeObuResponse(result, expectSuccessfulStatus));
            }

            log("失败原因：" + reason);
            return Fail(stepName + "失败：" + reason);
        }

        /// <summary>输出门架交易失败步骤并生成本轮失败结果。</summary>
        /// <param name="stepName">失败的门架协议步骤。</param><param name="reason">业务层确认的失败原因。</param><param name="result">失败步骤的通信结果。</param><param name="expectSuccessfulStatus">是否检查 TransferChannel 的成功状态。</param><param name="log">测试信息显示回调。</param>
        /// <returns>包含步骤和原因的门架交易失败结果。</returns>
        private static GantryCycleResult FailGantryStep(string stepName, string reason, TransparentCommandResult result, bool expectSuccessfulStatus, Action<string> log)
        {
            log("\r\n失败步骤：" + stepName);
            if (result != null)
            {
                log("下行请求 Len=" + result.RequestData.Length + "：" + FormatAirFrame(result.RequestData));
                log(result.ResponseData != null && result.ResponseData.Length > 0
                    ? "OBU 上行回复 Len=" + result.ResponseData.Length + "：" + FormatAirFrame(result.ResponseData)
                    : "OBU 上行回复：未收到有效数据。");
                log("OBU 回复分析：" + AnalyzeObuResponse(result, expectSuccessfulStatus));
            }

            log("失败原因：" + reason);
            return new GantryCycleResult(false, stepName + "失败：" + reason);
        }

        /// <summary>
        /// 根据请求状态、接收状态和每条 APDU 独立响应码分析 OBU 回复为何未通过。
        /// </summary>
        /// <param name="result">当前失败步骤的通信结果；为空表示没有可分析的交互。</param>
        /// <param name="expectSuccessfulStatus">是否要求按 TransferChannel 的 APDUList/DataList 结构逐条校验。</param>
        /// <returns>适合显示给测试人员的中文失败分析，不包含 DLL 实现细节。</returns>
        private static string AnalyzeObuResponse(TransparentCommandResult result, bool expectSuccessfulStatus)
        {
            if (result == null)
            {
                return "没有可分析的交互结果。";
            }

            if (result.RequestResult != 0)
            {
                return "下行请求未成功发送，因此未进入 OBU 回复判定。";
            }

            if (!result.ResponseResult.HasValue)
            {
                return "该步骤没有响应接收结果。";
            }

            if (result.ResponseResult.Value != 0 || result.ResponseData == null || result.ResponseData.Length == 0)
            {
                return "在规定时间内未收到有效 OBU 回复。";
            }

            if (!expectSuccessfulStatus)
            {
                return "已收到 OBU 回复，但回复内容未满足当前步骤的数据结构要求。";
            }

            string transferAnalysis;
            // TransferChannel 失败入口必须逐项解析，避免最后一条 90 00 掩盖前面 APDU 的错误状态。
            ValidateTransferChannelResponse(result.RequestData, result.ResponseData, out transferAnalysis);
            return transferAnalysis;
        }

        /// <summary>
        /// 将一条空中协议帧格式化为便于人工核对的空格分隔十六进制文本。
        /// </summary>
        /// <param name="data">不含本地命令类型前缀的完整空中帧；为空时返回“（空）”。</param>
        /// <returns>每字节两位大写十六进制并以空格分隔的完整帧文本。</returns>
        private static string FormatAirFrame(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "（空）";
            }

            StringBuilder builder = new StringBuilder(data.Length * 3 - 1);
            for (int index = 0; index < data.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(data[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        /// <summary>按命令类型判断通信回复是否成功，TransferChannel 逐条校验所有 APDU 响应。</summary>
        /// <param name="result">透传执行结果。</param>
        /// <param name="requireTransferStatus">为 true 时要求 APDUList 与 DataList 数量一致、每条响应末尾为 90 00 且 ReturnStatus 为 00。</param>
        /// <returns>通信成功、回复非空且满足当前命令状态规则时返回 true。</returns>
        internal static bool IsResponseSuccessful(TransparentCommandResult result, bool requireTransferStatus)
        {
            if (result == null || !result.IsSuccess || result.ResponseData == null || result.ResponseData.Length == 0) return false;
            if (!requireTransferStatus) return true;
            string analysis;
            // TransferChannel 判定入口解析每一条 APDU 回复，任意一条失败即判定当前交易步骤失败。
            return ValidateTransferChannelResponse(result.RequestData, result.ResponseData, out analysis);
        }

        /// <summary>
        /// 按原上位机 TransferChannel 请求和响应结构逐条核对 APDU 返回状态。
        /// </summary>
        /// <param name="requestData">完整 TransferChannel.request 空中帧；第 13 字节为 APDUList。</param>
        /// <param name="responseData">完整 TransferChannel.response 空中帧；第 13 字节为 DataList。</param>
        /// <param name="analysis">返回全部可解析异常的中文汇总；成功时说明全部指令及 ReturnStatus 均通过。</param>
        /// <returns>请求/响应结构完整、数量一致、每条 Data 末尾均为 90 00 且 ReturnStatus 为 00 时返回 true。</returns>
        private static bool ValidateTransferChannelResponse(byte[] requestData, byte[] responseData, out string analysis)
        {
            List<byte[]> requestApdus;
            string parseError;
            if (!TryParseTransferRequestApdus(requestData, out requestApdus, out parseError))
            {
                analysis = "TransferChannel 请求结构无效：" + parseError;
                return false;
            }

            if (responseData == null || responseData.Length < 14)
            {
                analysis = "TransferChannel 回复长度不足，无法读取 DataList 和 ReturnStatus。";
                return false;
            }

            List<string> failures = new List<string>();
            int dataCount = responseData[12];
            if (dataCount != requestApdus.Count)
            {
                failures.Add(string.Format("指令数量不一致：下发 {0} 条 APDU，OBU 回复 {1} 条 Data。", requestApdus.Count, dataCount));
            }

            int offset = 13;
            for (int index = 0; index < dataCount; index++)
            {
                if (offset >= responseData.Length)
                {
                    failures.Add(string.Format("OBU 第 {0}/{1} 条回复缺少长度字段，后续回复无法继续定位。", index + 1, dataCount));
                    analysis = string.Join("\r\n", failures);
                    return false;
                }

                int dataLength = responseData[offset++];
                if (dataLength < 2 || offset + dataLength > responseData.Length)
                {
                    failures.Add(string.Format("OBU 第 {0}/{1} 条回复长度无效：声明 {2} 字节，后续回复无法继续定位。", index + 1, dataCount, dataLength));
                    analysis = string.Join("\r\n", failures);
                    return false;
                }

                byte status1 = responseData[offset + dataLength - 2];
                byte status2 = responseData[offset + dataLength - 1];
                if (status1 != 0x90 || status2 != 0x00)
                {
                    // 异常显示入口统一查询 ESAM 手册状态码，避免各测试步骤重复维护错误说明。
                    string statusDescription = DescribeEsamStatusCode(status1, status2);
                    string requestDescription = index < requestApdus.Count
                        ? FormatAirFrame(requestApdus[index])
                        : "无对应下发指令";
                    failures.Add(string.Format(
                        "第 {0}/{1} 条 APDU 执行失败；下发指令：{2}；OBU 响应码：{3:X2} {4:X2}；错误原因：{5}。",
                        index + 1,
                        dataCount,
                        requestDescription,
                        status1,
                        status2,
                        statusDescription));
                }

                offset += dataLength;
            }

            if (offset >= responseData.Length)
            {
                failures.Add("OBU 回复缺少 TransferChannel ReturnStatus。");
                analysis = string.Join("\r\n", failures);
                return false;
            }

            byte returnStatus = responseData[offset];
            if (returnStatus != 0x00)
            {
                failures.Add(string.Format("OBU TransferChannel ReturnStatus 为 {0:X2}，期望为 00。", returnStatus));
            }

            if (failures.Count > 0)
            {
                analysis = string.Format("共发现 {0} 个异常：\r\n{1}", failures.Count, string.Join("\r\n", failures));
                return false;
            }

            analysis = string.Format("全部 {0} 条 APDU 响应码均为 90 00，OBU ReturnStatus 为 00。", dataCount);
            return true;
        }

        /// <summary>
        /// 按 TransferChannel.response 的 DataList 结构提取指定 APDU 的有效数据，不包含末尾 SW1/SW2。
        /// </summary>
        /// <param name="responseData">完整 TransferChannel.response 空口帧，第 13 字节为 DataList 数量。</param>
        /// <param name="dataIndex">要提取的 Data 序号，从 0 开始。</param>
        /// <param name="minimumPayloadLength">不包含 SW1/SW2 的最小有效数据长度，单位为字节。</param>
        /// <param name="payload">成功时返回独立复制的 APDU 有效数据；失败时为 null。</param>
        /// <param name="errorMessage">失败时返回数量、长度或状态码错误；成功时为空。</param>
        /// <returns>指定 Data 存在、长度足够且状态码为 90 00 时返回 true。</returns>
        private static bool TryGetTransferResponseData(byte[] responseData, int dataIndex, int minimumPayloadLength, out byte[] payload, out string errorMessage)
        {
            payload = null;
            errorMessage = string.Empty;
            if (responseData == null || responseData.Length < 14)
            {
                errorMessage = "TransferChannel 回复长度不足。";
                return false;
            }

            int dataCount = responseData[12];
            if (dataIndex < 0 || dataIndex >= dataCount)
            {
                errorMessage = string.Format("需要第 {0} 条 Data，实际仅返回 {1} 条。", dataIndex + 1, dataCount);
                return false;
            }

            int offset = 13;
            for (int index = 0; index < dataCount; index++)
            {
                if (offset >= responseData.Length)
                {
                    errorMessage = string.Format("第 {0} 条 Data 缺少长度字段。", index + 1);
                    return false;
                }

                int dataLength = responseData[offset++];
                if (dataLength < 2 || offset + dataLength > responseData.Length)
                {
                    errorMessage = string.Format("第 {0} 条 Data 长度无效。", index + 1);
                    return false;
                }

                if (index == dataIndex)
                {
                    int payloadLength = dataLength - 2;
                    byte status1 = responseData[offset + payloadLength];
                    byte status2 = responseData[offset + payloadLength + 1];
                    if (status1 != 0x90 || status2 != 0x00)
                    {
                        // 目标 Data 异常时复用手册状态码解释，便于实机定位。
                        errorMessage = string.Format("响应码为 {0:X2} {1:X2}：{2}。", status1, status2, DescribeEsamStatusCode(status1, status2));
                        return false;
                    }

                    if (payloadLength < minimumPayloadLength)
                    {
                        errorMessage = string.Format("有效数据仅 {0} 字节，需要至少 {1} 字节。", payloadLength, minimumPayloadLength);
                        return false;
                    }

                    payload = new byte[payloadLength];
                    Buffer.BlockCopy(responseData, offset, payload, 0, payloadLength);
                    return true;
                }

                offset += dataLength;
            }

            errorMessage = "未找到指定 Data。";
            return false;
        }

        /// <summary>
        /// 根据《ESAM技术手册（多逻辑通道）V1.1》解释 APDU 的 SW1、SW2 状态码。
        /// </summary>
        /// <param name="status1">APDU 响应状态字 SW1。</param>
        /// <param name="status2">APDU 响应状态字 SW2；对 61xx、63Cx、6Cxx 包含长度或剩余次数。</param>
        /// <returns>对应的中文状态原因；手册未定义时返回明确的未知状态提示。</returns>
        private static string DescribeEsamStatusCode(byte status1, byte status2)
        {
            if (status1 == 0x61)
            {
                return status2 == 0x00
                    ? "指令已执行，仍有不少于 256 字节响应数据需要获取"
                    : string.Format("指令已执行，仍有 {0} 字节响应数据需要获取", status2);
            }

            if (status1 == 0x63 && (status2 & 0xF0) == 0xC0)
            {
                return string.Format("认证失败，剩余尝试次数为 {0}", status2 & 0x0F);
            }

            if (status1 == 0x6C)
            {
                return status2 == 0x00
                    ? "Le 长度错误，正确长度或剩余长度不少于 256 字节"
                    : string.Format("Le 长度错误，正确长度或最大可读长度为 {0} 字节", status2);
            }

            switch ((status1 << 8) | status2)
            {
                case 0x9000: return "指令正确执行";
                case 0x6581: return "写 EEPROM 失败";
                case 0x6600: return "不支持当前算法";
                case 0x6700: return "报文、Lc 或 Le 长度错误";
                case 0x6901: return "当前状态错误，命令不被接受";
                case 0x6981: return "文件类型错误或文件结构不匹配";
                case 0x6982: return "安全条件不满足";
                case 0x6983: return "认证方法或相关密钥已锁定";
                case 0x6984: return "尚未获取随机数";
                case 0x6985: return "使用条件不满足；拆卸计数指令中可能表示拆卸次数已经为 0";
                case 0x6988: return "安全报文错误或 MAC 校验错误";
                case 0x6A80: return "命令数据错误";
                case 0x6A82: return "文件未找到";
                case 0x6A83: return "记录未找到";
                case 0x6A84: return "存储空间不足";
                case 0x6A86: return "参数 P1/P2 错误";
                case 0x6A88: return "密钥未找到";
                case 0x6B00: return "偏移地址错误或偏移超出 EF 文件范围";
                case 0x6D00: return "当前状态不支持该指令";
                case 0x6E00: return "CLA 错误";
                case 0x6F00: return "数据无效";
                case 0x9302: return "MAC 错误";
                case 0x9303: return "当前应用已永久锁定";
                case 0x9403: return "密钥未找到";
                case 0x9406: return "交易序号错误";
                case 0x9407: return "复合应用被禁止";
                default:
                    return string.Format("ESAM 技术手册未定义状态码 {0:X2} {1:X2}", status1, status2);
            }
        }

        /// <summary>
        /// 从 TransferChannel.request 中按 APDUList 和每条长度字段提取下发指令。
        /// </summary>
        /// <param name="requestData">不含本地类型前缀的完整 TransferChannel.request 空中帧。</param>
        /// <param name="apdus">成功时返回按下发顺序复制的全部 APDU 指令。</param>
        /// <param name="errorMessage">失败时返回具体结构错误；成功时为空。</param>
        /// <returns>请求至少包含固定头、APDUList，且每条 APDU 长度均未越界时返回 true。</returns>
        private static bool TryParseTransferRequestApdus(byte[] requestData, out List<byte[]> apdus, out string errorMessage)
        {
            apdus = new List<byte[]>();
            errorMessage = string.Empty;
            if (requestData == null || requestData.Length < 13)
            {
                errorMessage = "帧长度不足 13 字节。";
                return false;
            }

            int apduCount = requestData[12];
            int offset = 13;
            for (int index = 0; index < apduCount; index++)
            {
                if (offset >= requestData.Length)
                {
                    errorMessage = string.Format("第 {0}/{1} 条 APDU 缺少长度字段。", index + 1, apduCount);
                    return false;
                }

                int apduLength = requestData[offset++];
                if (apduLength <= 0 || offset + apduLength > requestData.Length)
                {
                    errorMessage = string.Format("第 {0}/{1} 条 APDU 长度无效：声明 {2} 字节。", index + 1, apduCount, apduLength);
                    return false;
                }

                byte[] apdu = new byte[apduLength];
                Buffer.BlockCopy(requestData, offset, apdu, 0, apduLength);
                apdus.Add(apdu);
                offset += apduLength;
            }

            return true;
        }

        /// <summary>创建原 WJ_Trade_3DES 使用的 28 字节车道 BST。</summary>
        /// <returns>不含本地透传类型前缀的 BST 帧。</returns>
        private static byte[] CreateLaneBstFrame()
        {
            return new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00 };
        }

        /// <summary>创建旧版 WJ_MastTrade 使用的 28 字节门架 BST。</summary>
        /// <returns>不含本地透传类型前缀的门架 BST 帧。</returns>
        private static byte[] CreateGantryBstFrame()
        {
            byte[] frame = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0xA2, 0x00, 0x00, 0x00, 0x54, 0xC1, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0x30, 0x1A, 0x00, 0x2B, 0x00, 0x04, 0x00 };
            long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            frame[12] = (byte)(unixTime >> 24);
            frame[13] = (byte)(unixTime >> 16);
            frame[14] = (byte)(unixTime >> 8);
            frame[15] = (byte)unixTime;
            return frame;
        }

        /// <summary>
        /// 创建原 OBU 防碰撞测试使用的 28 字节 BST，并写入当前 Unix 时间。
        /// </summary>
        /// <returns>不含本地透传命令前缀的防碰撞 BST 帧。</returns>
        private static byte[] CreateAntiCollisionBstFrame()
        {
            byte[] frame = { 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x02, 0x00, 0x01, 0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00 };
            long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            frame[12] = (byte)(unixTime >> 24);
            frame[13] = (byte)(unixTime >> 16);
            frame[14] = (byte)(unixTime >> 8);
            frame[15] = (byte)unixTime;
            return frame;
        }

        /// <summary>
        /// 创建防碰撞测试中一次读取余额、ICC 0015 和 ICC 0019 的 TransferChannel 帧。
        /// </summary>
        /// <param name="macId">VST 返回的 4 字节 MACID。</param>
        /// <param name="llc">当前 LLC 控制域，只允许上层流程传入已切换的会话值。</param>
        /// <returns>31 字节 TransferChannel 空口帧。</returns>
        private static byte[] CreateAntiCollisionCardReadFrame(byte[] macId, byte llc)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x03, 0x05, 0x80, 0x5C, 0x00, 0x02, 0x04, 0x05, 0x00, 0xB0, 0x95, 0x01, 0x2B, 0x05, 0x00, 0xB2, 0x01, 0xCC, 0x2B };
            // 在发送前用本次 VST 会话的 MACID 和 LLC 替换模板字段。
            CopyMacAndLlc(frame, macId, llc);
            return frame;
        }

        /// <summary>
        /// 创建防碰撞测试的交易认证 TransferChannel 帧，认证序列取自消费初始化回复。
        /// </summary>
        /// <param name="macId">VST 返回的 4 字节 MACID。</param>
        /// <param name="llc">当前 LLC 控制域。</param>
        /// <param name="consumeInitializeResponse">消费初始化完整回复，至少 20 字节；第 18、19 字节为原上位机使用的交易认证序列。</param>
        /// <returns>22 字节交易认证帧。</returns>
        /// <exception cref="ArgumentException">消费初始化回复长度不足 20 字节时抛出。</exception>
        internal static byte[] CreateTransactionAuthenticationFrame(byte[] macId, byte llc, byte[] consumeInitializeResponse)
        {
            if (consumeInitializeResponse == null || consumeInitializeResponse.Length < 20)
            {
                throw new ArgumentException("消费初始化回复长度不足，无法取得交易认证序列。", nameof(consumeInitializeResponse));
            }

            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x08, 0x80, 0x5A, 0x00, 0x09, 0x02, 0x04, 0x3A, 0x06 };
            // 交易认证必须继承当前会话 MACID、LLC 及消费初始化返回的 EP 序列。
            CopyMacAndLlc(frame, macId, llc);
            frame[19] = consumeInitializeResponse[18];
            frame[20] = consumeInitializeResponse[19];
            return frame;
        }

        /// <summary>
        /// 将 OtherFrames 返回的五个大端 32 位计时值换算为原 OBU 防碰撞测试的校正毫秒值。
        /// </summary>
        /// <param name="responseData">OtherFrames 回复，前 20 字节必须包含五个大端计时值。</param>
        /// <returns>按 GetSecure、读卡、消费初始化、交易认证、扣费顺序排列的五个校正毫秒值。</returns>
        /// <exception cref="ArgumentException">回复长度少于 20 字节时抛出。</exception>
        private static double[] ParseAntiCollisionTimings(byte[] responseData)
        {
            if (responseData == null || responseData.Length < 20)
            {
                throw new ArgumentException("防碰撞计时数据必须至少为 20 字节。", nameof(responseData));
            }

            double[] corrections = { 1.0455, 1.1975, 2.833, 0.955, 1.350 };
            double[] timings = new double[corrections.Length];
            for (int index = 0; index < corrections.Length; index++)
            {
                int offset = index * 4;
                uint raw = ((uint)responseData[offset] << 24)
                    | ((uint)responseData[offset + 1] << 16)
                    | ((uint)responseData[offset + 2] << 8)
                    | responseData[offset + 3];
                timings[index] = raw / 1000.0 - corrections[index];
            }

            return timings;
        }

        /// <summary>创建原车道交易 GetSecure 请求。</summary>
        /// <param name="macId">VST 返回的 4 字节 MACID。</param>
        /// <param name="llc">当前 LLC 控制域。</param>
        /// <returns>26 字节 GetSecure 帧。</returns>
        internal static byte[] CreateGetSecureFrame(byte[] macId, byte llc, SoftTradeAlgorithm algorithm)
        {
            byte[] frame = { 0x08, 0x41, 0x2A, 0xBA, 0x40, 0x77, 0x91, 0x05, 0x01, 0x00, 0x14, 0x80, 0x01, 0x00, 0x00, 0x4F, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x00, 0x00 };
            if (algorithm == SoftTradeAlgorithm.Sm4)
            {
                frame[24] = 0x40;
                frame[25] = 0x43;
            }
            CopyMacAndLlc(frame, macId, llc);
            return frame;
        }

        /// <summary>创建读取 ESAM EF04 的 27 字节 TransferChannel 帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param>
        /// <returns>27 字节帧。</returns>
        private static byte[] CreateReadEsamFrame(byte[] macId, byte llc)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x02, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xEF, 0x04, 0x05, 0x00, 0xB0, 0x01, 0x3A, 0x5B };
            CopyMacAndLlc(frame, macId, llc); return frame;
        }

        /// <summary>创建旧版门架交易使用的 120 字节 ESAM EF04 写入帧。</summary>
        /// <param name="macId">VST 返回的 4 字节 MACID。</param><param name="llc">当前 LLC 控制域。</param>
        /// <returns>不含本地透传类型前缀的门架 EF04 写入帧。</returns>
        private static byte[] CreateGantryWriteEsamFrame(byte[] macId, byte llc)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x02, 0x60, 0x00, 0xD6, 0x01, 0x3A, 0x5B, 0xAA, 0x29, 0x00, 0x11, 0x01, 0x0D, 0x01, 0x41, 0x5F, 0x05, 0x6C, 0xF7, 0x01, 0x03, 0x05, 0x34, 0x0C, 0x5F, 0x05, 0x6E, 0x0D, 0xBE, 0xA9, 0x50, 0x37, 0x43, 0x52, 0x32, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xFF, 0xB1, 0xB1, 0xBE, 0xA9, 0x11, 0x01, 0x00, 0x01, 0x16, 0x11, 0x11, 0x01, 0x12, 0x01, 0x22, 0x00, 0x00, 0x76, 0x57, 0x30, 0x01, 0x01, 0x00, 0x01, 0x44, 0x00, 0x01, 0x1D, 0x00, 0x02, 0x00, 0x19, 0x44, 0x00, 0x01, 0x0D, 0x01, 0x00, 0x01, 0x44, 0x02, 0x00, 0x01, 0x1D, 0x2B, 0xB3, 0x24, 0x83, 0x63, 0x6C, 0xF4, 0x03, 0x09, 0x00, 0xD6, 0x01, 0x95, 0x04, 0x11, 0x00, 0x01, 0x1D };
            CopyMacAndLlc(frame, macId, llc); return frame;
        }

        /// <summary>创建读取 ICC 0019/0002 的 25 字节 TransferChannel 帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param>
        /// <returns>25 字节帧。</returns>
        private static byte[] CreateGetIccFrame(byte[] macId, byte llc)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x02, 0x05, 0x00, 0xB2, 0x01, 0xCC, 0x2B, 0x05, 0x80, 0x5C, 0x00, 0x02, 0x04 };
            CopyMacAndLlc(frame, macId, llc); return frame;
        }

        /// <summary>创建原上位机 122 字节 ESAM EF04 写入帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param>
        /// <returns>122 字节帧。</returns>
        private static byte[] CreateWriteEsamInFrame(byte[] macId, byte llc)
        {
            byte[] frame = new byte[122];
            byte[] prefix = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x02, 0x07, 0x00, 0xA4, 0x00, 0x00, 0x02, 0xEF, 0x04, 0x64, 0x00, 0xD6, 0x01, 0x3A, 0x5F, 0xAA, 0x29, 0x00, 0x11, 0x01, 0x0D, 0x0D, 0x01, 0x5F, 0x05, 0x2F, 0xEA, 0x01, 0x03, 0x00, 0x00, 0x00, 0x5F, 0x05, 0x2F, 0xEA, 0xBE, 0xA9, 0x50, 0x37, 0x43, 0x52, 0x32, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xFF, 0xB1, 0xB1, 0xBE, 0xA9, 0x11, 0x01, 0x00, 0x01, 0x16, 0x11, 0x11, 0x01, 0x12, 0x01, 0x22, 0x00, 0x00, 0x76, 0x57, 0x30, 0x01, 0x01, 0x00, 0x00, 0x00 };
            Buffer.BlockCopy(prefix, 0, frame, 0, prefix.Length);
            frame[118] = 0x11;
            CopyMacAndLlc(frame, macId, llc); return frame;
        }

        /// <summary>创建原软交易消费初始化帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param><param name="tradeSetting">[TRADE_SET]/19 的 4 字节扣费金额和 1 字节出入口状态。</param><param name="sequenceNumber">当前轮次。</param>
        /// <returns>79 字节帧。</returns>
        private static byte[] CreateConsumeInitializeFrame(byte[] macId, byte llc, byte[] tradeSetting, int sequenceNumber, SoftTradeAlgorithm algorithm)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x02, 0x10, 0x80, 0x50, 0x03, 0x02, 0x0B, 0x01, 0x00, 0x00, 0x00, 0x01, 0x37, 0x37, 0x37, 0x37, 0x37, 0x37, 0x30, 0x80, 0xDC, 0xAA, 0xC8, 0x2B, 0xAA, 0x29, 0x00, 0x34, 0x01, 0x08, 0xFE, 0x01, 0x1A, 0xA1, 0xDD, 0x02, 0x01, 0x03, 0x00, 0x27, 0x10, 0x02, 0xC0, 0xB6, 0xCB, 0xD5, 0x41, 0x4E, 0x35, 0x39, 0x35, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x34, 0x01, 0x00, 0x00, 0x01, 0x1A, 0xA1, 0xDD, 0x02, 0x00, 0x00 };
            CopyMacAndLlc(frame, macId, llc);
            Buffer.BlockCopy(tradeSetting, 0, frame, 20, 4);
            frame[19] = algorithm == SoftTradeAlgorithm.Sm4 ? (byte)0x41 : (byte)0x01;
            frame[40] = (byte)(sequenceNumber & 0xFF);
            frame[49] = tradeSetting[4];
            frame[44] = (byte)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() >> 24);
            frame[45] = (byte)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() >> 16);
            frame[46] = (byte)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() >> 8);
            frame[47] = (byte)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return frame;
        }

        /// <summary>创建旧 WjConsumeInitialize 使用的固定终端序号、固定时间和 ETC 出口状态消费初始化帧。</summary>
        /// <param name="macId">VST 返回的四字节 MACID。</param>
        /// <param name="llc">当前 LLC 控制域。</param>
        /// <param name="tradeSetting">前四字节为用例指定金额；第五字节由本函数固定为 04。</param>
        /// <param name="algorithm">3DES 或 SM4，用于设置算法标识。</param>
        /// <returns>与旧 WjConsumeInitialize 一致的 79 字节 TransferChannel 帧。</returns>
        private static byte[] CreateWjConsumeInitializeFrame(byte[] macId, byte llc, byte[] tradeSetting, SoftTradeAlgorithm algorithm)
        {
            byte[] frame = CreateConsumeInitializeFrame(macId, llc, tradeSetting, 1, algorithm);
            for (int index = 24; index < 30; index++) frame[index] = 0x37;
            frame[44] = 0x1A;
            frame[45] = 0xA1;
            frame[46] = 0xDD;
            frame[47] = 0x02;
            frame[49] = 0x04;
            return frame;
        }

        /// <summary>创建旧 PIN 异常用例的错误 PIN 认证及四条 0018 记录读取组合帧。</summary>
        /// <param name="macId">VST 返回的四字节 MACID。</param>
        /// <param name="llc">紧接第二笔扣费后的 LLC 控制域。</param>
        /// <returns>包含错误 PIN 及 B2 01～04 四条 APDU 的 49 字节 TransferChannel 帧。</returns>
        private static byte[] CreatePinAnomalyRead0018Frame(byte[] macId, byte llc)
        {
            byte[] frame =
            {
                0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x05,
                0x0B, 0x00, 0x20, 0x00, 0x00, 0x06, 0x88, 0x88, 0x88, 0x00, 0x00, 0x00,
                0x05, 0x00, 0xB2, 0x01, 0xC4, 0x17,
                0x05, 0x00, 0xB2, 0x02, 0xC4, 0x17,
                0x05, 0x00, 0xB2, 0x03, 0xC4, 0x17,
                0x05, 0x00, 0xB2, 0x04, 0xC4, 0x17
            };
            CopyMacAndLlc(frame, macId, llc);
            return frame;
        }

        /// <summary>判断实际回复是否以指定字节序列结束。</summary>
        /// <param name="data">设备返回的完整空中回复，可为空。</param>
        /// <param name="suffix">必须匹配的非空帧尾。</param>
        /// <returns>回复长度足够且帧尾逐字节相同时返回 true。</returns>
        private static bool EndsWith(byte[] data, byte[] suffix)
        {
            if (data == null || suffix == null || suffix.Length == 0 || data.Length < suffix.Length) return false;
            int offset = data.Length - suffix.Length;
            for (int index = 0; index < suffix.Length; index++)
            {
                if (data[offset + index] != suffix[index]) return false;
            }

            return true;
        }

        /// <summary>创建旧版门架交易专用的消费初始化帧。</summary>
        /// <param name="macId">VST 返回的 4 字节 MACID。</param><param name="llc">当前 LLC 控制域。</param><param name="tradeSetting">[TRADE_SET]/19 的 5 字节配置；此门架帧只取前 4 字节作为金额。</param><param name="algorithm">3DES 或 SM4。</param>
        /// <returns>79 字节门架消费初始化 TransferChannel 帧。</returns>
        private static byte[] CreateGantryConsumeInitializeFrame(byte[] macId, byte llc, byte[] tradeSetting, SoftTradeAlgorithm algorithm)
        {
            // 旧版 C++ 数组按 79 字节发送，初始化列表实际依赖尾部零填充；在 C# 中显式补齐该字节。
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0x77, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x02, 0x10, 0x80, 0x50, 0x03, 0x02, 0x0B, 0x01, 0x00, 0x00, 0x00, 0x00, 0x21, 0x11, 0x00, 0x00, 0x89, 0x81, 0x30, 0x80, 0xDC, 0xAA, 0xC8, 0x2B, 0xAA, 0x29, 0x00, 0x11, 0x01, 0x0D, 0x01, 0x41, 0x5F, 0x05, 0x6C, 0xF7, 0x01, 0x03, 0x34, 0x0C, 0x5F, 0x05, 0x6E, 0x0D, 0xBE, 0xA9, 0x50, 0x37, 0x43, 0x52, 0x32, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x01, 0x1D, 0x00 };
            CopyMacAndLlc(frame, macId, llc);
            Buffer.BlockCopy(tradeSetting, 0, frame, 20, 4);
            frame[19] = algorithm == SoftTradeAlgorithm.Sm4 ? (byte)0x41 : (byte)0x01;
            // 原 ConsumeInitialize_MJ_ruanpsam 会在发送前将模板中的终端交易序号覆盖为六个 0x37；
            // MAC1 原文也使用相同的六字节值，二者必须严格一致，否则 OBU 在 80 54 返回 93 02。
            for (int index = 24; index < 30; index++) frame[index] = 0x37;
            // 这里对应旧版 ConsumeInitialize_MJ_ruanpsam；旧版没有改写模板中的 5F 05 6C F7，
            // 只有普通车道 ConsumeInitialize 才会按当前时间更新该字段。门架兼容流程必须保持原始 79 字节布局。
            // 原门架专用函数没有读取 gINI.File_19[4]，而是在组帧末尾明确固定交易状态为 0x03。
            frame[49] = GantryTradeState;
            return frame;
        }

        /// <summary>创建扣费帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param><param name="tradeTime">7 字节 UTC BCD 交易时间。</param><param name="mac1">4 字节 MAC1。</param>
        /// <returns>34 字节消费帧。</returns>
        internal static byte[] CreateConsumeFrame(byte[] macId, byte llc, byte[] tradeTime, byte[] mac1)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x01, 0x14, 0x80, 0x54, 0x01, 0x00, 0x0F, 0x00, 0x00, 0x06, 0xC6, 0x20, 0x14, 0x02, 0x27, 0x19, 0x24, 0x50, 0xE8, 0x48, 0x2E, 0x6E };
            CopyMacAndLlc(frame, macId, llc); Buffer.BlockCopy(tradeTime, 0, frame, 23, 7); Buffer.BlockCopy(mac1, 0, frame, 30, 4); return frame;
        }

        /// <summary>创建旧版门架交易使用的 40 字节扣费 TransferChannel 帧。</summary>
        /// <param name="macId">VST 返回的 4 字节 MACID。</param><param name="llc">当前 LLC 控制域。</param><param name="tradeTime">7 字节交易时间。</param><param name="mac1">4 字节 MAC1。</param>
        /// <returns>40 字节门架扣费帧。</returns>
        private static byte[] CreateGantryConsumeFrame(byte[] macId, byte llc, byte[] tradeTime, byte[] mac1)
        {
            byte[] frame = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x01, 0x02, 0x14, 0x80, 0x54, 0x01, 0x00, 0x0F, 0x00, 0x00, 0x06, 0xC6, 0x20, 0x20, 0x07, 0x08, 0x13, 0x57, 0x43, 0x78, 0x30, 0xA7, 0xE7, 0x05, 0x80, 0x5C, 0x00, 0x02, 0x04 };
            CopyMacAndLlc(frame, macId, llc); Buffer.BlockCopy(tradeTime, 0, frame, 23, 7); Buffer.BlockCopy(mac1, 0, frame, 30, 4); return frame;
        }

        /// <summary>创建原 108 字节 ESAM 写回帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param>
        /// <returns>108 字节帧。</returns>
        private static byte[] CreateWriteEsamOutFrame(byte[] macId, byte llc)
        {
            byte[] frame = new byte[108];
            byte[] prefix = { 0x08, 0x00, 0x00, 0x01, 0x40, 0xF7, 0x91, 0x05, 0x01, 0x03, 0x18, 0x02, 0x02, 0x08, 0x00, 0xD6, 0x01, 0x3A, 0x03, 0x00, 0x00, 0x00, 0x55, 0x00, 0xD6, 0x01, 0x95, 0x50 };
            Buffer.BlockCopy(prefix, 0, frame, 0, prefix.Length); CopyMacAndLlc(frame, macId, llc); return frame;
        }

        /// <summary>创建 SetMMI 帧。</summary>
        /// <param name="macId">VST MACID。</param><param name="llc">LLC 控制域。</param>
        /// <returns>12 字节帧。</returns>
        internal static byte[] CreateSetMmiFrame(byte[] macId, byte llc)
        {
            byte[] frame = { 0x08, 0x0E, 0x8D, 0x5C, 0x40, 0xF7, 0x99, 0x05, 0x01, 0x04, 0x1A, 0x00 }; CopyMacAndLlc(frame, macId, llc); return frame;
        }

        /// <summary>创建单向 EventReport 帧。</summary>
        /// <param name="macId">VST MACID。</param>
        /// <returns>10 字节单向帧。</returns>
        internal static byte[] CreateEventReportFrame(byte[] macId)
        {
            byte[] frame = { 0x08, 0x0E, 0x8D, 0x5C, 0x40, 0x03, 0x91, 0x60, 0x01, 0x00 }; Buffer.BlockCopy(macId, 0, frame, 0, 4); return frame;
        }

        /// <summary>替换帧 MACID 和 LLC 控制域。</summary>
        /// <param name="frame">至少 6 字节空口帧。</param><param name="macId">4 字节 MACID。</param><param name="llc">LLC 控制域。</param>
        private static void CopyMacAndLlc(byte[] frame, byte[] macId, byte llc)
        {
            if (macId == null || macId.Length < 4) throw new ArgumentException("MACID 必须为 4 字节。", nameof(macId));
            Buffer.BlockCopy(macId, 0, frame, 0, 4); frame[5] = llc;
        }

        /// <summary>按原上位机规则切换 LLC 0x77/0xF7。</summary>
        /// <param name="llc">当前 LLC。</param><returns>切换后的 LLC。</returns>
        internal static byte ToggleLlc(byte llc) { return llc == 0x77 ? (byte)0xF7 : (byte)0x77; }

        /// <summary>解析 VST 中的 MACID 和预读 ICC0015。</summary>
        /// <param name="response">fVST 返回的完整帧。</param>
        /// <returns>解析结果；结构不完整时返回 null。</returns>
        internal static VstSnapshot ParseVst(byte[] response)
        {
            if (response == null || response.Length < 15) return null;
            int offset = 0;
            byte[] mac = new byte[4]; Buffer.BlockCopy(response, offset, mac, 0, 4); offset += 4;
            offset++; byte llc = response[offset++]; if (llc == 0xE0) offset++;
            if (response.Length < offset + 5) return null;
            offset += 4; // head, VST sign, SupportConfig, ApplicationList
            byte privateInfo = response[offset++];
            if ((privateInfo & 0x80) != 0) { if (offset >= response.Length) return null; offset++; }
            if ((privateInfo & 0x40) == 0 || offset >= response.Length) return null;
            byte rndOption = response[offset++];
            if (offset >= response.Length) return null;
            byte sysInfoFile = response[offset++];
            if (sysInfoFile != 0x20 && sysInfoFile != 0x27 && sysInfoFile != 0x1A) return null;
            // 原 CODE_ENCODE.cpp 先解析 BST 的 sysInfoFileMode（本车道 BST 为 0x1A），
            // VST 中的 Container Type 0x20/0x27 只是类型标识，不能直接当作长度 32/39。
            // 当前车道交易请求的系统信息长度固定为 BST 的 0x1A，即 26 字节。
            const int requestedSystemLength = 0x1A;
            int systemLength = requestedSystemLength;
            if (response.Length < offset + systemLength) return null;
            int systemOffset = offset;
            byte[] contractProvider = new byte[8];
            byte[] contractSerial = new byte[8];
            Buffer.BlockCopy(response, systemOffset, contractProvider, 0, contractProvider.Length);
            byte contractVersion = response[systemOffset + 9];
            Buffer.BlockCopy(response, systemOffset + 10, contractSerial, 0, contractSerial.Length);
            offset += systemLength;
            if ((rndOption & 0x80) != 0) { if (response.Length < offset + 9) return null; offset += 9; }
            if ((rndOption & 0x20) == 0 || response.Length <= offset) return new VstSnapshot(mac, null, contractProvider, contractVersion, contractSerial);
            byte gbIccInfo = response[offset++];
            if (gbIccInfo != 0x28 || response.Length < offset + 43) return new VstSnapshot(mac, null, contractProvider, contractVersion, contractSerial);
            byte[] icc0015 = new byte[43]; Buffer.BlockCopy(response, offset, icc0015, 0, icc0015.Length);
            return new VstSnapshot(mac, icc0015, contractProvider, contractVersion, contractSerial);
        }

        /// <summary>读取 SetMe.ini 中的整数。</summary>
        /// <param name="path">配置路径。</param><param name="section">节名。</param><param name="key">键名。</param><param name="defaultValue">缺失或非法时的默认值。</param>
        /// <returns>配置值。</returns>
        internal static int ReadIniInt(string path, string section, string key, int defaultValue)
        {
            string value = ReadIniValue(path, section, key); int parsed; return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && parsed > 0 ? parsed : defaultValue;
        }

        /// <summary>读取拼帧交易必需的非负十进制金额配置。</summary>
        /// <param name="path">SetMe.ini 完整路径。</param>
        /// <param name="section">配置节名称。</param>
        /// <param name="key">配置键名称。</param>
        /// <returns>以分为单位的非负金额。</returns>
        /// <exception cref="InvalidDataException">配置缺失、不是整数或超出非负 Int32 范围时抛出。</exception>
        private static int ReadRequiredNonNegativeIniInt(string path, string section, string key)
        {
            string value = ReadIniValue(path, section, key);
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) || parsed < 0)
            {
                throw new InvalidDataException("[" + section + "]/" + key + " 必须是以分为单位的非负整数。");
            }

            return parsed;
        }

        /// <summary>
        /// 从 SetMe.ini 的 [TRADE_SET]/19 读取交易配置。
        /// </summary>
        /// <param name="path">SetMe.ini 的完整路径；文件必须存在，并包含恰好 5 字节的十六进制配置。</param>
        /// <returns>5 字节配置；前 4 字节是扣费金额，最后 1 字节是写入 ICC 0019 的出入口状态。</returns>
        /// <exception cref="InvalidDataException">配置项缺失、长度不是 5 字节或包含非法十六进制字符时抛出。</exception>
        internal static byte[] ReadTradeSetting(string path)
        {
            // 交易入口必须从用户指定的 [TRADE_SET]/19 读取金额和状态，禁止回退为静默的全零交易参数。
            string value = ReadIniValue(path, "TRADE_SET", "19");
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException("SetMe.ini 缺少 [TRADE_SET]/19，无法取得车道交易金额和出入口状态。");
            }

            string normalizedValue = value.Replace(" ", string.Empty).Replace("\t", string.Empty);
            if (normalizedValue.Length != 10)
            {
                throw new InvalidDataException("SetMe.ini 的 [TRADE_SET]/19 必须为 10 个十六进制字符：前 8 个字符表示 4 字节扣费金额，最后 2 个字符表示出入口状态。");
            }

            byte[] result = new byte[5];
            for (int index = 0; index < result.Length; index++)
            {
                byte parsed;
                if (!byte.TryParse(normalizedValue.Substring(index * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsed))
                {
                    throw new InvalidDataException("SetMe.ini 的 [TRADE_SET]/19 包含非法十六进制字符。");
                }

                result[index] = parsed;
            }

            return result;
        }

        /// <summary>
        /// 读取简单 INI 键值，并兼容节标题右侧的双斜线或分号说明文字。
        /// </summary>
        /// <param name="path">INI 文件完整路径；文件按系统默认编码读取。</param>
        /// <param name="section">目标节名，不包含方括号，比较时不区分大小写。</param>
        /// <param name="key">目标键名，比较时不区分大小写。</param>
        /// <returns>找到时返回等号右侧去除首尾空白的值；文件、节或键不存在时返回 null。</returns>
        private static string ReadIniValue(string path, string section, string key)
        {
            if (!File.Exists(path)) return null;

            string active = string.Empty;
            foreach (string rawLine in File.ReadAllLines(path, Encoding.Default))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith(";", StringComparison.Ordinal)) continue;

                int sectionEnd = line.IndexOf(']');
                if (line.StartsWith("[", StringComparison.Ordinal) && sectionEnd > 1)
                {
                    // SetMe.ini 的节标题允许在右方括号后直接追加 // 中文说明。
                    active = line.Substring(1, sectionEnd - 1).Trim();
                    continue;
                }

                if (!string.Equals(active, section, StringComparison.OrdinalIgnoreCase)) continue;
                int separator = line.IndexOf('=');
                if (separator <= 0) continue;
                if (string.Equals(line.Substring(0, separator).Trim(), key, StringComparison.OrdinalIgnoreCase)) return line.Substring(separator + 1).Trim();
            }

            return null;
        }

        /// <summary>格式化二进制数据。</summary><param name="data">数据。</param><returns>无分隔大写十六进制。</returns>
        private static string ToHex(byte[] data) { if (data == null) return string.Empty; StringBuilder builder = new StringBuilder(data.Length * 2); foreach (byte value in data) builder.Append(value.ToString("X2", CultureInfo.InvariantCulture)); return builder.ToString(); }

        /// <summary>生成失败结果。</summary><param name="message">失败原因。</param><returns>失败轮次结果。</returns>
        private static LaneCycleResult Fail(string message) { return new LaneCycleResult(false, message); }

        private sealed class LaneCycleResult
        {
            /// <summary>创建单轮车道交易结果。</summary>
            /// <param name="isSuccess">本轮是否成功。</param>
            /// <param name="message">失败原因；成功时可为空。</param>
            internal LaneCycleResult(bool isSuccess, string message) { IsSuccess = isSuccess; Message = message ?? string.Empty; }
            internal bool IsSuccess { get; }
            internal string Message { get; }
        }

        /// <summary>保存单轮门架交易的执行结果。</summary>
        /// <param name="isSuccess">本轮是否成功。</param><param name="message">失败原因；成功时为空。</param>
        private sealed class GantryCycleResult
        {
            /// <summary>创建门架交易单轮结果。</summary>
            /// <param name="isSuccess">本轮是否成功。</param><param name="message">失败原因；成功时为空。</param>
            internal GantryCycleResult(bool isSuccess, string message) { IsSuccess = isSuccess; Message = message ?? string.Empty; }
            internal bool IsSuccess { get; }
            internal string Message { get; }
        }

        internal sealed class VstSnapshot
        {
            /// <summary>创建 VST 快照。</summary>
            /// <param name="macId">VST 返回的 MACID。</param><param name="icc0015">VST 中的预读 ICC0015；无法解析时为空。</param>
            /// <param name="contractProvider">合同发行方标识。</param><param name="contractVersion">合同版本。</param><param name="contractSerial">合同序列号。</param>
            internal VstSnapshot(byte[] macId, byte[] icc0015, byte[] contractProvider, byte contractVersion, byte[] contractSerial)
            { MacId = macId; Icc0015 = icc0015; ContractProvider = contractProvider; ContractVersion = contractVersion; ContractSerial = contractSerial; }
            internal byte[] MacId { get; }
            internal byte[] Icc0015 { get; }
            internal byte[] ContractProvider { get; }
            internal byte ContractVersion { get; }
            internal byte[] ContractSerial { get; }
        }
    }

    /// <summary>车道交易执行结果。</summary>
    internal sealed class LaneTransactionExecutionResult
    {
        /// <summary>创建车道交易结果。</summary>
        /// <param name="isSuccess">所有配置轮次是否成功。</param><param name="message">最终说明。</param><param name="successCount">成功轮数。</param><param name="totalCount">总轮数。</param><param name="exchanges">完整交互帧。</param><param name="completedAt">完成时间。</param>
        internal LaneTransactionExecutionResult(bool isSuccess, string message, int successCount, int totalCount, IList<ObuProtocolExchange> exchanges, DateTime completedAt)
        { IsSuccess = isSuccess; Message = message ?? string.Empty; SuccessCount = successCount; TotalCount = totalCount; Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly(); CompletedAt = completedAt; }
        internal bool IsSuccess { get; }
        internal string Message { get; }
        internal int SuccessCount { get; }
        internal int TotalCount { get; }
        internal IList<ObuProtocolExchange> Exchanges { get; }
        internal DateTime CompletedAt { get; }
    }

    /// <summary>防碰撞交易和五段处理时间的综合执行结果。</summary>
    internal sealed class AntiCollisionExecutionResult
    {
        /// <summary>
        /// 创建防碰撞测试结果。
        /// </summary>
        /// <param name="isSuccess">完整交易成功且五段校正时间均处于 (0,8) ms 时为 true。</param>
        /// <param name="message">最终中文判定或失败原因。</param>
        /// <param name="timings">五段校正毫秒值；未进入计时判定时可为 null。</param>
        /// <param name="exchanges">用于结果文件的完整空中交互帧。</param>
        /// <param name="completedAt">测试完成时间。</param>
        internal AntiCollisionExecutionResult(bool isSuccess, string message, double[] timings, IList<ObuProtocolExchange> exchanges, DateTime completedAt)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            Timings = timings == null ? null : (double[])timings.Clone();
            Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly();
            CompletedAt = completedAt;
        }

        /// <summary>获取测试是否通过。</summary>
        internal bool IsSuccess { get; }

        /// <summary>获取最终判定或失败原因。</summary>
        internal string Message { get; }

        /// <summary>获取五段校正毫秒值；未读取时为 null。</summary>
        internal double[] Timings { get; }

        /// <summary>获取完整空中交互帧。</summary>
        internal IList<ObuProtocolExchange> Exchanges { get; }

        /// <summary>获取测试完成时间。</summary>
        internal DateTime CompletedAt { get; }
    }
}
