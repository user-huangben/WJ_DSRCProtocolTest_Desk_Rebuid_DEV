using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace WJ_ComprehensiveTest_Desk.Services
{
    /// <summary>按旧 <c>WJ_OneChip_typicaltransfer</c> 执行北京地标六分支典型交易。</summary>
    internal sealed class BeijingTypicalTransactionService
    {
        private const int DefaultIntervalMilliseconds = 8000;
        private const byte ExitTradeState = 0x04;
        private readonly SoftTradeCryptoService _crypto = new SoftTradeCryptoService();

        /// <summary>执行固定六分支北京地标典型交易。</summary>
        /// <param name="comm">已打开并初始化的台发服务。</param>
        /// <param name="configurationPath">当前程序使用的 SetMe.ini 完整路径。</param>
        /// <param name="validateVehicleInformation">是否解密并比对旧用例固定车辆信息。</param>
        /// <param name="token">统一停止令牌。</param>
        /// <param name="log">测试信息回调。</param>
        /// <param name="progress">已完成分支数回调。</param>
        /// <returns>六分支综合结果和完整帧记录。</returns>
        internal BeijingTypicalTransactionResult Execute(
            DesktopCommService comm,
            string configurationPath,
            bool validateVehicleInformation,
            CancellationToken token,
            Action<string> log,
            Action<int, int> progress)
        {
            if (comm == null) throw new ArgumentNullException(nameof(comm));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            byte[] configured = LaneTransactionService.ReadTradeSetting(configurationPath);
            byte[] tradeSetting = { configured[0], configured[1], configured[2], configured[3], ExitTradeState };
            int interval = LaneTransactionService.ReadIniInt(
                configurationPath, "SET_Trade_GB", "interval", DefaultIntervalMilliseconds);
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int successCount = 0;
            string failure = string.Empty;
            progress(0, 6);
            log("北京典型交易参数：金额=" + ToHex(tradeSetting, 0, 4)
                + "，状态=04，分支=固定6，分支间隔=" + interval.ToString(CultureInfo.InvariantCulture)
                + " ms，车辆信息校验=" + (validateVehicleInformation ? "开启" : "关闭") + "。");

            for (int branch = 1; branch <= 6; branch++)
            {
                token.ThrowIfCancellationRequested();
                BranchResult branchResult = ExecuteBranch(
                    comm, branch, tradeSetting, validateVehicleInformation, exchanges, token, log);
                if (!branchResult.IsSuccess)
                {
                    failure = branchResult.Message;
                    break;
                }

                successCount++;
                progress(successCount, 6);
                if (branch < 6 && token.WaitHandle.WaitOne(interval)) token.ThrowIfCancellationRequested();
            }

            bool passed = successCount == 6;
            string message = passed
                ? "北京地标典型交易测试通过：6/6 个分支全部成功。"
                : "北京地标典型交易测试未通过：成功 " + successCount + "/6。原因：" + failure;
            return new BeijingTypicalTransactionResult(passed, message, successCount, 6, exchanges);
        }

        /// <summary>执行一个独立分支；算法和分散字段仅来自当次 VST。</summary>
        private BranchResult ExecuteBranch(
            DesktopCommService comm,
            int branch,
            byte[] tradeSetting,
            bool validateVehicleInformation,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken token,
            Action<string> log)
        {
            byte[] sessionMac = null;
            bool eventReportAttempted = false;
            try
            {
                log("北京典型交易分支 " + branch + "/6 开始。");
                TransparentCommandResult result = Send(
                    comm, 0x01, LaneTransactionService.CreateTypicalBstFrame(), exchanges, token, "BST/VST", log, false);
                if (!IsSuccessful(result, false)) return Fail("分支" + branch + " BST/VST 未收到有效 VST。", result, log);

                LaneTransactionService.VstSnapshot vst = LaneTransactionService.ParseVst(result.ResponseData);
                if (vst == null || vst.MacId == null || vst.ContractProvider == null || vst.ContractSerial == null)
                    return Fail("分支" + branch + " VST 结构不完整。", result, log);
                sessionMac = (byte[])vst.MacId.Clone();
                SoftTradeAlgorithm algorithm = vst.ContractVersion == 0x51 || vst.ContractVersion == 0x52
                    ? SoftTradeAlgorithm.Sm4
                    : SoftTradeAlgorithm.TripleDes;
                if (branch != 4 && branch != 6 && (vst.Icc0015 == null || vst.Icc0015.Length < 20))
                    return Fail("分支" + branch + " VST 缺少软算所需的 ICC0015。", result, log);
                log("分支" + branch + " 合同版本=" + vst.ContractVersion.ToString("X2", CultureInfo.InvariantCulture)
                    + "，算法=" + (algorithm == SoftTradeAlgorithm.Sm4 ? "SM4" : "3DES") + "。");

                byte llc = 0x77;
                result = Send(
                    comm, 0x02, CreateBeijingGetSecureFrame(vst.MacId, llc, algorithm),
                    exchanges, token, "GetSecure读取车辆信息", log, true);
                if (!IsSuccessful(result, false)) return Fail("分支" + branch + " GetSecure 失败。", result, log);
                if (validateVehicleInformation)
                {
                    string vehicleError;
                    if (!_crypto.TryValidateBeijingVehicleInformation(
                        algorithm, vst.ContractProvider, vst.ContractSerial, result.ResponseData, out vehicleError))
                        return Fail("分支" + branch + " 车辆信息校验失败：" + vehicleError, result, log);
                }

                byte[] firstFactor = null;
                byte[] secondFactor = null;
                if (branch == 1)
                {
                    foreach (byte[] frame in LaneTransactionService.CreateTypicalCaseOneReads(vst.MacId))
                    {
                        llc = LaneTransactionService.ToggleLlc(llc);
                        frame[5] = llc;
                        result = Send(comm, 0x03, frame, exchanges, token, "典型组合读卡", log, true);
                        if (!IsSuccessful(result, true)) return Fail("分支1组合读卡失败。", result, log);
                    }
                }
                else if (branch == 3)
                {
                    llc = LaneTransactionService.ToggleLlc(llc);
                    result = Send(
                        comm, 0x03, LaneTransactionService.CreateTypicalReadFrame(vst.MacId, llc, 4),
                        exchanges, token, "读取0015/0019/余额", log, true);
                    if (!IsSuccessful(result, true) || result.ResponseData.Length < 34)
                        return Fail("分支3组合读取失败或回复不足34字节。", result, log);
                    firstFactor = Slice(result.ResponseData, 14, 8);
                    secondFactor = Slice(result.ResponseData, 26, 8);

                    llc = LaneTransactionService.ToggleLlc(llc);
                    result = Send(
                        comm, 0x03, LaneTransactionService.CreateTypicalReadFrame(vst.MacId, llc, 5),
                        exchanges, token, "读取0019/EF08", log, true);
                    if (!IsSuccessful(result, true)) return Fail("分支3读取0019/EF08失败。", result, log);
                }
                else if (branch != 2)
                {
                    llc = LaneTransactionService.ToggleLlc(llc);
                    result = Send(
                        comm, 0x03, LaneTransactionService.CreateTypicalReadFrame(vst.MacId, llc, 3),
                        exchanges, token, "读取0019/余额", log, true);
                    if (!IsSuccessful(result, true)) return Fail("分支" + branch + "读取0019/余额失败。", result, log);
                }

                llc = LaneTransactionService.ToggleLlc(llc);
                bool initializeWithUpdate = branch == 3 || branch == 5;
                result = Send(
                    comm, 0x03,
                    LaneTransactionService.CreateTypicalInitializeFrame(vst.MacId, llc, tradeSetting, algorithm, initializeWithUpdate),
                    exchanges, token, initializeWithUpdate ? "消费初始化+更新0019" : "消费初始化", log, true);
                if (!IsSuccessful(result, true)) return Fail("分支" + branch + "消费初始化失败。", result, log);
                byte[] initializeResponse = result.ResponseData;

                if (branch == 4 || branch == 6)
                {
                    llc = LaneTransactionService.ToggleLlc(llc);
                    result = Send(
                        comm, 0x03,
                        LaneTransactionService.CreateTransactionAuthenticationFrame(vst.MacId, llc, initializeResponse),
                        exchanges, token, "获取交易认证", log, true);
                    if (!IsSuccessful(result, true)) return Fail("分支" + branch + "获取交易认证失败。", result, log);
                }
                else
                {
                    byte[] tradeTime;
                    byte[] mac1;
                    string cryptoError;
                    if (!_crypto.TryCreateMac1(
                        algorithm, vst.Icc0015, initializeResponse, tradeSetting, out tradeTime, out mac1, out cryptoError))
                        return Fail("分支" + branch + " MAC1计算失败：" + cryptoError, result, log);

                    llc = LaneTransactionService.ToggleLlc(llc);
                    if (branch == 1)
                    {
                        result = Send(
                            comm, 0x03, LaneTransactionService.CreateTypicalUpdate0019Frame(vst.MacId, llc),
                            exchanges, token, "更新0019", log, true);
                        if (!IsSuccessful(result, true)) return Fail("分支1更新0019失败。", result, log);
                        llc = LaneTransactionService.ToggleLlc(llc);
                        result = Send(
                            comm, 0x03, LaneTransactionService.CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1),
                            exchanges, token, "扣费", log, true);
                    }
                    else if (branch == 2)
                    {
                        result = Send(
                            comm, 0x03,
                            LaneTransactionService.CreateTypicalUpdateAndConsumeFrame(vst.MacId, llc, tradeTime, mac1),
                            exchanges, token, "更新0019+扣费", log, true);
                    }
                    else
                    {
                        result = Send(
                            comm, 0x03,
                            LaneTransactionService.CreateTypicalConsumeAndBalanceFrame(vst.MacId, llc, tradeTime, mac1),
                            exchanges, token, "扣费+读余额", log, true);
                    }
                    if (!IsSuccessful(result, true)) return Fail("分支" + branch + "扣费阶段失败。", result, log);

                    if (branch == 3)
                    {
                        llc = LaneTransactionService.ToggleLlc(llc);
                        result = Send(
                            comm, 0x03, LaneTransactionService.CreateTypicalIccDirectoryFrame(vst.MacId, llc),
                            exchanges, token, "选择ICC 1001目录", log, true);
                        if (!IsSuccessful(result, true)) return Fail("分支3选择ICC目录失败。", result, log);

                        llc = LaneTransactionService.ToggleLlc(llc);
                        byte[] randomFrame = LaneTransactionService.CreateTypicalRandomFrame(vst.MacId, llc);
                        int randomLength = vst.ContractVersion == 0x40 ? 8 : 16;
                        randomFrame[randomFrame.Length - 1] = (byte)randomLength;
                        result = Send(comm, 0x03, randomFrame, exchanges, token, "ICC取随机数", log, true);
                        if (!IsSuccessful(result, true) || result.ResponseData.Length < 14 + randomLength)
                            return Fail("分支3随机数回复无效。", result, log);
                        byte[] random = new byte[16];
                        Buffer.BlockCopy(result.ResponseData, 14, random, 0, randomLength);
                        byte[] authentication = algorithm == SoftTradeAlgorithm.Sm4
                            ? _crypto.CreateTypicalSm4ExternalAuthentication(random, vst.ContractVersion, firstFactor, secondFactor)
                            : _crypto.CreateTypicalTripleDesExternalAuthentication(random, firstFactor, secondFactor);

                        llc = LaneTransactionService.ToggleLlc(llc);
                        result = Send(
                            comm, 0x03,
                            LaneTransactionService.CreateTypicalExternalAuthFrame(vst.MacId, llc, authentication, algorithm),
                            exchanges, token, "外部认证+更新EF08", log, true);
                        if (!IsSuccessful(result, true)) return Fail("分支3外部认证+更新EF08失败。", result, log);
                    }
                }

                llc = LaneTransactionService.ToggleLlc(llc);
                result = Send(
                    comm, 0x04, LaneTransactionService.CreateSetMmiFrame(vst.MacId, llc),
                    exchanges, token, "SetMMI（北京非强制）", log, true);
                if (!IsSuccessful(result, false)) log("分支" + branch + " SetMMI 未成功，按旧北京语义仅记录，不否决分支。");

                eventReportAttempted = true;
                result = Send(
                    comm, 0x05, LaneTransactionService.CreateEventReportFrame(vst.MacId),
                    exchanges, token, "EventReport（单向）", log, false);
                if (result.RequestResult != 0) return Fail("分支" + branch + " EventReport发送失败。", result, log);
                log("北京典型交易分支 " + branch + "/6 成功。");
                return new BranchResult(true, string.Empty);
            }
            finally
            {
                if (sessionMac != null && !eventReportAttempted && !token.IsCancellationRequested)
                {
                    try
                    {
                        TransparentCommandResult cleanup = Send(
                            comm, 0x05, LaneTransactionService.CreateEventReportFrame(sessionMac),
                            exchanges, CancellationToken.None, "失败后EventReport（单向清理）", log, false);
                        if (cleanup.RequestResult != 0) log("失败后EventReport清理发送失败：" + cleanup.ErrorMessage);
                    }
                    catch (Exception exception)
                    {
                        log("失败后EventReport清理异常：" + exception.Message);
                    }
                }
            }
        }

        /// <summary>发送一条帧并保存 DLL 请求、响应及返回码。</summary>
        private static TransparentCommandResult Send(
            DesktopCommService comm,
            byte type,
            byte[] frame,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken token,
            string stepName,
            Action<string> log,
            bool applyMac)
        {
            // 北京典型交易协议步骤通过共享台发服务执行，完整交互由结果列表统一持久化。
            TransparentCommandResult result = LaneTransactionService.ExecuteStep(
                comm, new[] { (byte)0x00, type }, frame, exchanges, token, stepName, log, applyMac);
            log(stepName + "：" + (result.IsSuccess ? "DLL调用成功" : "失败 - " + result.ErrorMessage));
            return result;
        }

        /// <summary>判断通信及可选 TransferChannel 多 APDU 状态是否成功。</summary>
        private static bool IsSuccessful(TransparentCommandResult result, bool transferStatus)
        {
            return LaneTransactionService.IsResponseSuccessful(result, transferStatus);
        }

        /// <summary>创建旧北京典型交易使用的 26 字节 GetSecure 帧。</summary>
        /// <param name="macId">当次 VST 的 4 字节 MACID。</param>
        /// <param name="llc">当前 LLC 控制域。</param>
        /// <param name="algorithm">合同版本对应的 3DES 或 SM4 算法。</param>
        /// <returns>车辆信息读取长度为 0x20、鉴别数据固定为 0102030401020304 的请求帧。</returns>
        private static byte[] CreateBeijingGetSecureFrame(byte[] macId, byte llc, SoftTradeAlgorithm algorithm)
        {
            if (macId == null || macId.Length < 4) throw new ArgumentException("MACID 必须为 4 字节。", nameof(macId));
            byte[] frame =
            {
                0x08,0x0E,0x8D,0x5C,0x40,0x77,0x91,0x05,0x01,0x00,0x14,0x80,0x01,0x00,0x00,0x20,
                0x01,0x02,0x03,0x04,0x01,0x02,0x03,0x04,0x00,0x00
            };
            Buffer.BlockCopy(macId, 0, frame, 0, 4);
            frame[5] = llc;
            if (algorithm == SoftTradeAlgorithm.Sm4)
            {
                frame[24] = 0x40;
                frame[25] = 0x43;
            }

            return frame;
        }

        /// <summary>记录失败步骤、完整实际帧和 DLL 返回码。</summary>
        private static BranchResult Fail(string message, TransparentCommandResult result, Action<string> log)
        {
            if (result != null)
            {
                log("失败下行帧：" + ToHex(result.RequestData, 0, result.RequestData.Length));
                log("失败回复帧：" + ToHex(result.ResponseData, 0, result.ResponseData.Length));
                log("DLL返回码：请求=" + result.RequestResult.ToString(CultureInfo.InvariantCulture)
                    + "，响应=" + (result.ResponseResult.HasValue
                        ? result.ResponseResult.Value.ToString(CultureInfo.InvariantCulture)
                        : "无"));
            }
            log("失败原因：" + message);
            return new BranchResult(false, message);
        }

        /// <summary>复制响应中的固定长度字段。</summary>
        private static byte[] Slice(byte[] source, int offset, int length)
        {
            if (source == null || offset < 0 || length < 0 || offset + length > source.Length)
                throw new ArgumentException("字段超出响应范围。", nameof(source));
            byte[] result = new byte[length];
            Buffer.BlockCopy(source, offset, result, 0, length);
            return result;
        }

        /// <summary>格式化数组指定范围。</summary>
        private static string ToHex(byte[] data, int offset, int length)
        {
            if (data == null || length <= 0) return string.Empty;
            System.Text.StringBuilder builder = new System.Text.StringBuilder(length * 2);
            for (int index = 0; index < length; index++)
                builder.Append(data[offset + index].ToString("X2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private sealed class BranchResult
        {
            internal BranchResult(bool isSuccess, string message) { IsSuccess = isSuccess; Message = message ?? string.Empty; }
            internal bool IsSuccess { get; }
            internal string Message { get; }
        }
    }

    /// <summary>北京地标典型交易六分支综合结果。</summary>
    internal sealed class BeijingTypicalTransactionResult
    {
        internal BeijingTypicalTransactionResult(
            bool isSuccess,
            string message,
            int successCount,
            int totalCount,
            IList<ObuProtocolExchange> exchanges)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            SuccessCount = successCount;
            TotalCount = totalCount;
            Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly();
        }

        internal bool IsSuccess { get; }
        internal string Message { get; }
        internal int SuccessCount { get; }
        internal int TotalCount { get; }
        internal IList<ObuProtocolExchange> Exchanges { get; }
    }
}
