using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace WJ_ComprehensiveTest_Desk.Services
{
    /// <summary>原上位机 WJ_Trade_typicaltransfer / WJ_Trade_typicaltransfer_SM4 的典型交易迁移。</summary>
    internal sealed partial class LaneTransactionService
    {
        private const byte TypicalTradeState = 0x04;

        /// <summary>按原上位机固定的六个典型交易分支执行测试。</summary>
        /// <param name="desktopCommService">已经打开并初始化的台发服务。</param>
        /// <param name="configurationPath">exe 同级 SetMe.ini 路径。</param>
        /// <param name="algorithm">界面选择的 3DES 或 SM4。</param>
        /// <param name="cancellationToken">停止令牌。</param>
        /// <param name="log">测试信息日志。</param>
        /// <param name="progress">成功分支数进度。</param>
        /// <returns>六个分支的执行结果和完整交互帧。</returns>
        internal LaneTransactionExecutionResult ExecuteTypical(
            DesktopCommService desktopCommService,
            string configurationPath,
            SoftTradeAlgorithm algorithm,
            CancellationToken cancellationToken,
            Action<string> log,
            Action<int, int> progress)
        {
            if (desktopCommService == null) throw new ArgumentNullException(nameof(desktopCommService));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            const int total = 6;
            byte[] configured = ReadTradeSetting(configurationPath);
            byte[] tradeSetting = { configured[0], configured[1], configured[2], configured[3], TypicalTradeState };
            log("典型交易参数来源：金额=" + ToHex(new[] { tradeSetting[0], tradeSetting[1], tradeSetting[2], tradeSetting[3] })
                + "（SetMe.ini [TRADE_SET]/19 前4字节）；出入口状态=04（原典型交易函数固定 ETC 出口，不读取配置末字节 "
                + configured[4].ToString("X2", CultureInfo.InvariantCulture) + "）；算法标识="
                + (algorithm == SoftTradeAlgorithm.Sm4 ? "41（SM4）" : "01（3DES）") + "。完成分支数固定为 6。 ");
            progress(0, total);
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int success = 0;
            string failure = string.Empty;
            for (int testCase = 1; testCase <= total; testCase++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                LaneCycleResult cycle = ExecuteTypicalCase(desktopCommService, testCase, tradeSetting, algorithm, exchanges, cancellationToken, log);
                if (!cycle.IsSuccess) { failure = cycle.Message; break; }
                success++;
                progress(success, total);
            }

            bool passed = success == total;
            string message = passed
                ? "典型交易测试通过：6/6 个原上位机分支全部成功。"
                : "典型交易测试未通过：成功 " + success + "/6。原因：" + failure;
            return new LaneTransactionExecutionResult(passed, message, success, total, exchanges, DateTime.Now);
        }

        /// <summary>执行一个原典型交易 switch 分支。</summary>
        private LaneCycleResult ExecuteTypicalCase(DesktopCommService service, int testCase, byte[] tradeSetting,
            SoftTradeAlgorithm algorithm, IList<ObuProtocolExchange> exchanges, CancellationToken token, Action<string> log)
        {
            byte[] sessionMac = null;
            bool eventSent = false;
            byte[] firstFactor = null;
            byte[] secondFactor = null;
            byte esamVersion = 0;
            try
            {
                log("典型交易分支 " + testCase + "/6 开始。");
                TransparentCommandResult result = ExecuteStep(service, new byte[] { 0x00, 0x01 }, CreateTypicalBstFrame(), exchanges, token, "典型交易 BST/VST", log, false);
                if (!result.IsSuccess || result.ResponseData == null || result.ResponseData.Length == 0) return FailStep("分支" + testCase + " BST/VST", "未收到有效 VST。", result, false, log);
                VstSnapshot vst = ParseVst(result.ResponseData);
                if (vst == null || vst.Icc0015 == null || vst.Icc0015.Length < 20) return FailStep("分支" + testCase + " VST", "缺少完整 ICC0015。", result, false, log);
                sessionMac = vst.MacId;
                byte llc = 0x77;
                if (testCase == 1 && algorithm == SoftTradeAlgorithm.Sm4)
                {
                    // 原 SM4 入口仅在首分支先读取 ESAM 芯片序列号，后续分支复用同一派生上下文。
                    result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalEsamChipSerialFrame(vst.MacId, llc), exchanges, token, "读取ESAM芯片序列号", log, true);
                    if (!IsResponseSuccessful(result, true)) return FailStep("分支1 ESAM芯片序列号", "回复未通过校验。", result, true, log);
                }
                result = ExecuteStep(service, new byte[] { 0x00, 0x02 }, CreateGetSecureFrame(vst.MacId, llc, algorithm), exchanges, token, "读取车辆信息", log, true);
                if (!IsResponseSuccessful(result, false)) return FailStep("分支" + testCase + " GetSecure", "车辆信息回复无效。", result, false, log);

                if (testCase == 1)
                {
                    foreach (byte[] readFrame in CreateTypicalCaseOneReads(vst.MacId))
                    {
                        llc = ToggleLlc(llc); readFrame[5] = llc;
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, readFrame, exchanges, token, "典型组合读卡", log, true);
                        if (!IsResponseSuccessful(result, true)) return FailStep("分支1 组合读卡", "读卡回复未通过校验。", result, true, log);
                    }
                }
                else if (testCase == 3)
                {
                    llc = ToggleLlc(llc);
                    result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalReadFrame(vst.MacId, llc, 4), exchanges, token, "读取0015/0019/余额", log, true);
                    if (!IsResponseSuccessful(result, true)) return FailStep("分支3 组合读取", "回复未通过校验。", result, true, log);
                    if (result.ResponseData.Length < 34) return Fail("分支3组合读取回复不足34字节，无法提取外部认证分散因子。");
                    firstFactor = CopyRange(result.ResponseData, 14, 8); secondFactor = CopyRange(result.ResponseData, 26, 8); esamVersion = result.ResponseData[23];
                    llc = ToggleLlc(llc);
                    result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalReadFrame(vst.MacId, llc, 5), exchanges, token, "读取0019/EF08", log, true);
                    if (!IsResponseSuccessful(result, true)) return FailStep("分支3 读取0019/EF08", "回复未通过校验。", result, true, log);
                }
                else
                {
                    llc = ToggleLlc(llc);
                    result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalReadFrame(vst.MacId, llc, 3), exchanges, token, "读取0019/余额", log, true);
                    if (!IsResponseSuccessful(result, true)) return FailStep("分支" + testCase + " 读取0019/余额", "回复未通过校验。", result, true, log);
                }

                llc = ToggleLlc(llc);
                bool initializeWithUpdate = testCase == 3 || testCase == 5;
                byte[] initialize = CreateTypicalInitializeFrame(vst.MacId, llc, tradeSetting, algorithm, initializeWithUpdate);
                result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, initialize, exchanges, token, initializeWithUpdate ? "消费初始化+更新0019" : "消费初始化", log, true);
                if (!IsResponseSuccessful(result, true)) return FailStep("分支" + testCase + " 消费初始化", "回复未通过校验。", result, true, log);
                byte[] initializeResponse = result.ResponseData;

                if (testCase == 4 || testCase == 6)
                {
                    llc = ToggleLlc(llc);
                    result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTransactionAuthenticationFrame(vst.MacId, llc, initializeResponse), exchanges, token, "获取交易认证", log, true);
                    if (!IsResponseSuccessful(result, true)) return FailStep("分支" + testCase + " 交易认证", "回复未通过校验。", result, true, log);
                }
                else
                {
                    byte[] tradeTime; byte[] mac1; string cryptoError;
                    if (!_softTradeCryptoService.TryCreateMac1(algorithm, vst.Icc0015, initializeResponse, tradeSetting, out tradeTime, out mac1, out cryptoError)) return Fail(cryptoError);
                    llc = ToggleLlc(llc);
                    if (testCase == 1)
                    {
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalUpdate0019Frame(vst.MacId, llc), exchanges, token, "更新0019", log, true);
                        if (!IsResponseSuccessful(result, true)) return FailStep("分支1 更新0019", "回复未通过校验。", result, true, log);
                        llc = ToggleLlc(llc);
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, token, "扣费", log, true);
                    }
                    else if (testCase == 2)
                    {
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalUpdateAndConsumeFrame(vst.MacId, llc, tradeTime, mac1), exchanges, token, "更新0019+扣费", log, true);
                    }
                    else
                    {
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalConsumeAndBalanceFrame(vst.MacId, llc, tradeTime, mac1), exchanges, token, "扣费+读余额", log, true);
                    }
                    if (!IsResponseSuccessful(result, true)) return FailStep("分支" + testCase + " 扣费阶段", "回复未通过校验。", result, true, log);

                    if (testCase == 3)
                    {
                        llc = ToggleLlc(llc);
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalIccDirectoryFrame(vst.MacId, llc), exchanges, token, "选择ICC 1001目录", log, true);
                        if (!IsResponseSuccessful(result, true)) return FailStep("分支3 选择ICC 1001目录", "回复未通过校验。", result, true, log);
                        llc = ToggleLlc(llc);
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalRandomFrame(vst.MacId, llc), exchanges, token, "ICC取16字节随机数", log, true);
                        if (!IsResponseSuccessful(result, true)) return FailStep("分支3 随机数", "回复未通过校验。", result, true, log);
                        byte[] random; string randomError;
                        if (!TryGetTransferResponseData(result.ResponseData, 0, 16, out random, out randomError)) return Fail(randomError);
                        byte[] authentication = algorithm == SoftTradeAlgorithm.Sm4
                            ? _softTradeCryptoService.CreateTypicalSm4ExternalAuthentication(random, esamVersion, firstFactor, secondFactor)
                            : _softTradeCryptoService.CreateTypicalTripleDesExternalAuthentication(random, firstFactor, secondFactor);
                        llc = ToggleLlc(llc);
                        result = ExecuteStep(service, new byte[] { 0x00, 0x03 }, CreateTypicalExternalAuthFrame(vst.MacId, llc, authentication, algorithm), exchanges, token, "外部认证+更新EF08", log, true);
                        if (!IsResponseSuccessful(result, true)) return FailStep("分支3 外部认证+更新EF08", "回复未通过校验。", result, true, log);
                    }
                }

                llc = ToggleLlc(llc);
                result = ExecuteStep(service, new byte[] { 0x00, 0x04 }, CreateSetMmiFrame(vst.MacId, llc), exchanges, token, "SetMMI", log, true);
                if (!IsResponseSuccessful(result, false)) return FailStep("分支" + testCase + " SetMMI", "回复无效。", result, false, log);
                eventSent = true;
                result = ExecuteStep(service, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(vst.MacId), exchanges, token, "EventReport（单向）", log, false);
                return result.IsSuccess ? new LaneCycleResult(true, string.Empty) : FailStep("分支" + testCase + " EventReport", "发送失败。", result, false, log);
            }
            finally
            {
                if (sessionMac != null && !eventSent && !token.IsCancellationRequested)
                {
                    try { ExecuteStep(service, new byte[] { 0x00, 0x05 }, CreateEventReportFrame(sessionMac), exchanges, token, "失败后EventReport清理", log, false); }
                    catch (Exception exception) { log("典型交易失败后断链清理异常：" + exception.Message); }
                }
            }
        }

        /// <summary>创建旧 SendBST_GB_139 的 26 字节 BST，时间字段取当前 Unix 时间。</summary>
        internal static byte[] CreateTypicalBstFrame()
        {
            byte[] frame = { 0xFF,0xFF,0xFF,0xFF,0x50,0x03,0x91,0xC0,0x08,0x00,0x00,0x00,0x54,0xC1,0x1C,0x02,0x00,0x01,0x41,0x87,0x29,0x20,0x1A,0x00,0x2B,0x00 };
            long value = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); frame[12]=(byte)(value>>24); frame[13]=(byte)(value>>16); frame[14]=(byte)(value>>8); frame[15]=(byte)value; return frame;
        }

        /// <summary>创建原 EsamGetChipNumber8 的“选择3F00+取芯片序列号”帧。</summary>
        private static byte[] CreateTypicalEsamChipSerialFrame(byte[] macId, byte llc)
        {
            byte[] frame={0x08,0,0,1,0x40,0x77,0x91,0x05,0x01,0x03,0x18,0x02,0x02,0x07,0x00,0xA4,0,0,0x02,0x3F,0,0x05,0x80,0xF6,0,0x03,0x04};
            CopyMacAndLlc(frame,macId,llc); return frame;
        }

        /// <summary>创建分支1的六种单/组合读卡帧。</summary>
        internal static IEnumerable<byte[]> CreateTypicalCaseOneReads(byte[] macId)
        {
            foreach (int kind in new[] { 0, 1, 2, 3, 6, 4 }) yield return CreateTypicalReadFrame(macId, 0x77, kind);
        }

        /// <summary>按原 Framesend[1..8] 创建典型交易读卡帧。</summary>
        internal static byte[] CreateTypicalReadFrame(byte[] macId, byte llc, int kind)
        {
            byte[][] apdus = {
                new byte[]{0x00,0xB0,0x95,0x00,0x2B}, new byte[]{0x00,0xB2,0x01,0xCC,0x2B}, new byte[]{0x80,0x5C,0x00,0x02,0x04}, new byte[]{0x00,0xB0,0x88,0x00,0x1E}
            };
            int[][] map = { new[]{0}, new[]{1}, new[]{2}, new[]{1,2}, new[]{0,1,2}, new[]{1,3}, new[]{0,2} };
            int[] selected = map[kind]; byte[] frame = new byte[13 + selected.Length * 6];
            byte[] header = {0x08,0,0,1,0x40,llc,0x91,0x05,0x01,0x03,0x18,0x01,(byte)selected.Length}; Buffer.BlockCopy(header,0,frame,0,13); CopyMacAndLlc(frame,macId,llc);
            int offset=13; foreach(int index in selected){frame[offset++]=5; Buffer.BlockCopy(apdus[index],0,frame,offset,5); offset+=5;} return frame;
        }

        /// <summary>创建 30 或 79 字节消费初始化帧；金额取配置，状态固定 04。</summary>
        internal static byte[] CreateTypicalInitializeFrame(byte[] macId, byte llc, byte[] tradeSetting, SoftTradeAlgorithm algorithm, bool includeUpdate)
        {
            byte[] full = {0x08,0,0,1,0x40,0x77,0x91,0x05,0x01,0x03,0x18,0x01,0x02,0x10,0x80,0x50,0x03,0x02,0x0B,0x01,0,0,0,1,0x37,0x37,0x37,0x37,0x37,0x37,0x30,0x80,0xDC,0xAA,0xC8,0x2B,0xAA,0x29,0,0x34,0x01,0x08,0xFE,0x01,0x1A,0xA1,0xDD,0x02,0x01,0x04,0,0x27,0x10,0x02,0xC0,0xB6,0xCB,0xD5,0x41,0x4E,0x35,0x39,0x35,0x5A,0,0,0,0,0x34,0x01,0,0,1,0x1A,0xA1,0xDD,0x02,0,0};
            CopyMacAndLlc(full,macId,llc); full[19]=algorithm==SoftTradeAlgorithm.Sm4?(byte)0x41:(byte)0x01; Buffer.BlockCopy(tradeSetting,0,full,20,4); full[49]=TypicalTradeState;
            if(includeUpdate) return full; full[12]=1; byte[] shortFrame=new byte[30]; Buffer.BlockCopy(full,0,shortFrame,0,30); return shortFrame;
        }

        /// <summary>创建固定 04 状态的 0019 更新 APDU。</summary>
        internal static byte[] CreateTypicalUpdate0019Frame(byte[] macId, byte llc)
        {
            byte[] source=CreateTypicalInitializeFrame(macId,llc,new byte[]{0,0,0,1,4},SoftTradeAlgorithm.Sm4,true); byte[] frame=new byte[62]; Buffer.BlockCopy(source,0,frame,0,13); frame[12]=1; Buffer.BlockCopy(source,30,frame,13,49); CopyMacAndLlc(frame,macId,llc); return frame;
        }

        /// <summary>创建更新0019与扣费合并帧。</summary>
        internal static byte[] CreateTypicalUpdateAndConsumeFrame(byte[] macId, byte llc, byte[] time, byte[] mac1)
        {
            byte[] update=CreateTypicalUpdate0019Frame(macId,llc); byte[] frame=new byte[83]; Buffer.BlockCopy(update,0,frame,0,62); frame[12]=2; byte[] consume={0x14,0x80,0x54,0x01,0,0x0F,0,0,0x06,0xC6,0,0,0,0,0,0,0,0,0,0,0}; Buffer.BlockCopy(consume,0,frame,62,21); Buffer.BlockCopy(time,0,frame,72,7); Buffer.BlockCopy(mac1,0,frame,79,4); return frame;
        }

        /// <summary>创建扣费并读取余额的双 APDU 帧。</summary>
        internal static byte[] CreateTypicalConsumeAndBalanceFrame(byte[] macId, byte llc, byte[] time, byte[] mac1)
        {
            byte[] frame=CreateGantryConsumeFrame(macId,llc,time,mac1); frame[12]=2; return frame;
        }

        /// <summary>创建 ICC 取 16 字节随机数帧。</summary>
        internal static byte[] CreateTypicalRandomFrame(byte[] macId, byte llc)
        {
            byte[] frame={0x08,0,0,1,0x40,0x77,0x91,0x05,0x01,0x03,0x18,0x01,0x01,0x05,0x00,0x84,0,0,0x10}; CopyMacAndLlc(frame,macId,llc); return frame;
        }

        /// <summary>创建原 EnterDirectory_ICC(01,10,01) 的选择 1001 目录帧。</summary>
        internal static byte[] CreateTypicalIccDirectoryFrame(byte[] macId, byte llc)
        {
            byte[] frame={0x08,0,0,1,0x40,0x77,0x91,0x05,0x01,0x03,0x18,0x01,0x01,0x07,0x00,0xA4,0,0,0x02,0x10,0x01}; CopyMacAndLlc(frame,macId,llc); return frame;
        }

        /// <summary>复制协议回复中的固定字段。</summary>
        private static byte[] CopyRange(byte[] source, int offset, int length)
        {
            byte[] value=new byte[length]; Buffer.BlockCopy(source,offset,value,0,length); return value;
        }

        /// <summary>创建外部认证与更新 EF08 的合并帧。</summary>
        internal static byte[] CreateTypicalExternalAuthFrame(byte[] macId, byte llc, byte[] authentication, SoftTradeAlgorithm algorithm)
        {
            byte[] frame={0x08,0,0,1,0x40,0x77,0x91,0x05,0x01,0x03,0x18,0x01,0x02,0x0D,0x00,0x82,0,0x01,0x08,0,0,0,0,0,0,0,0,0x23,0x00,0xD6,0x88,0,0x1E,0xAA,0xBB,0xCC,0xDD,0xEE,0xFF,0,0x11,0x22,0x33,0x44,0x55,0x66,0x77,0x88,0x99,0,0x11,0x22,0x33,0x44,0x55,0x66,0x77,0x88,0x99,0,0x11,0x22,0x33}; CopyMacAndLlc(frame,macId,llc); frame[17]=algorithm==SoftTradeAlgorithm.Sm4?(byte)0x41:(byte)0x01; Buffer.BlockCopy(authentication,0,frame,19,8); return frame;
        }
    }
}
