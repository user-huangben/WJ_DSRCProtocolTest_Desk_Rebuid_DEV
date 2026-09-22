using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace WJ_DSRCProtocolTest_Desk_Net.Services
{
    /// <summary>
    /// 按旧单片式上位机的固定SM4根密钥、SetMe.ini数据源和5.8G TransferChannel顺序执行二次发行与标签激活。
    /// </summary>
    internal sealed class OneChipIssueActivationService
    {
        private const int TimeoutMilliseconds = 500;
        private static readonly byte[] RootKey = { 0x46,0xA1,0xA0,0x3B,0xA7,0x2A,0x63,0xF6,0xF4,0xB8,0xC0,0x6D,0x0F,0x0B,0x6D,0x6C };
        private static readonly byte[] SystemMaintenanceFactor = { 0xBF,0xA8,0xC6,0xAC,0xCE,0xAC,0xBB,0xA4 };
        private static readonly byte[] ApplicationMaintenanceFactor = { 0xD3,0xA6,0xD3,0xC3,0xCE,0xAC,0xBB,0xA4 };
        private static readonly byte[] ApplicationEncryptionFactor = { 0xBC,0xD3,0xC3,0xDC,0x00,0x00,0x00,0x03 };
        private static readonly byte[] SystemMasterFactor = { 0xBF,0xA8,0xC6,0xAC,0xD6,0xF7,0xBF,0xD8 };
        private static readonly byte[] ApplicationMasterFactor = { 0xD3,0xA6,0xD3,0xC3,0xD6,0xF7,0xBF,0xD8 };
        private static readonly byte[] Bst = { 0xFF,0xFF,0xFF,0xFF,0x50,0x03,0x91,0xC0,0x08,0x00,0x00,0x01,0x54,0xC1,0x1C,0x02,0x00,0x01,0x41,0x87,0x29,0x00,0x25,0x00 };
        private static readonly byte[] SetMmi = { 0x08,0x00,0x00,0x01,0x40,0x77,0x91,0x05,0x01,0x04,0x1A,0x00 };
        private static readonly byte[] EventReport = { 0x08,0x00,0x00,0x01,0x40,0x03,0x91,0x60,0x00,0x00 };
        private static readonly byte[] VehicleInformation = {
            0xB2,0xE2,0x41,0x31,0x32,0x33,0x34,0x35,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,
            0x0B,0xB8,0x05,0xDC,0x05,0xDC,0x00,0x27,0x10,0x00,0x4E,0x20,0x00,0x75,0x30,0x0A };

        private readonly SoftTradeCryptoService _crypto;

        /// <summary>创建单片式发行激活服务。</summary>
        /// <param name="crypto">复用当前项目SM4实现的软算法服务。</param>
        internal OneChipIssueActivationService(SoftTradeCryptoService crypto)
        {
            _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
        }

        /// <summary>复制旧单片式流程使用的根密钥，供同源北京交易的车辆信息解密复用。</summary>
        /// <returns>独立的 16 字节密钥副本。</returns>
        internal static byte[] GetRootKeyCopy() { return (byte[])RootKey.Clone(); }

        /// <summary>复制应用加密密钥业务因子。</summary>
        /// <returns>独立的 8 字节因子副本。</returns>
        internal static byte[] GetApplicationEncryptionFactorCopy() { return (byte[])ApplicationEncryptionFactor.Clone(); }

        /// <summary>复制旧北京流程用于校验的固定车辆信息。</summary>
        /// <returns>独立的 32 字节车辆信息副本。</returns>
        internal static byte[] GetVehicleInformationCopy() { return (byte[])VehicleInformation.Clone(); }

        /// <summary>执行旧 <c>WJ_FirstIssue_DoubleChannel</c> 的纯SM4一次发行流程。</summary>
        internal LaneTransactionExecutionResult ExecuteFirstIssue(DesktopCommService service, Action<string> log, CancellationToken token)
        {
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            try
            {
                byte[] file0015 = ReadIniHex("SET_Frame", "0015", 20);
                byte[] fileEf01 = ReadIniHex("SET_Frame", "EF01", 27);
                file0015[9] = 0x52; fileEf01[9] = 0x52;
                TransparentCommandResult vst = null;
                for (int retry = 0; retry < 3; retry++)
                {
                    vst = Send(service, 0x01, Bst, exchanges, token, "BST/VST", log, false);
                    if (vst.IsSuccess && vst.ResponseData.Length >= 32) break;
                }
                if (vst == null || !vst.IsSuccess || vst.ResponseData.Length < 32) return Finish(false, "三次BST后仍未取得有效VST。", 0, 1, exchanges);
                byte[] contractSerial = Slice(vst.ResponseData, 24, 8);

                if (!Transfer(service, CreateDirectoryFrame(0x02, 0x3F00), exchanges, token, "ESAM进入3F00", log)) return Finish(false, "ESAM进入3F00失败。", 0, 1, exchanges);
                TransparentCommandResult esamInfoResult = Send(service, 0x03, CreateReadBinaryFrame(0x02, 0x81, 0x1B), exchanges, token, "读取ESAM EF01", log, true);
                byte[] currentEsamIssuer = ReadTransferBytes(esamInfoResult, 27, "ESAM EF01");
                int esamType = ClassifyIssuer(currentEsamIssuer);
                if (esamType == 2) return Finish(false, "ESAM为全国正式密钥，旧用例禁止一次发行。", 0, 1, exchanges);

                if (!Transfer(service, CreateDirectoryFrame(0x01, 0x1001), exchanges, token, "ICC进入1001", log)) return Finish(false, "ICC进入1001失败。", 0, 1, exchanges);
                TransparentCommandResult iccInfoResult = Send(service, 0x03, CreateReadBinaryFrame(0x01, 0x95, 0x2B), exchanges, token, "读取ICC 0015", log, true);
                byte[] currentIccInfo = ReadTransferBytes(iccInfoResult, 20, "ICC 0015");
                int iccType = ClassifyIssuer(currentIccInfo);
                if (iccType == 2) return Finish(false, "ICC为全国正式密钥，旧用例禁止一次发行。", 0, 1, exchanges);

                TransparentCommandResult chipResult = Send(service, 0x03, CreateChipSerialFrame(), exchanges, token, "读取ESAM芯片序列号", log, true);
                byte[] chip4 = ReadTransferData(chipResult, 1, 4, "ESAM芯片序列号");
                byte[] chipSerial = Join(chip4, chip4);
                byte[] targetEsamIssuer = Slice(fileEf01, 0, 8);
                byte[][] esamKeys = DeriveEsamKeys(targetEsamIssuer, chipSerial, contractSerial);
                byte[] currentEsamSystemMaster = esamType == 0 ? new byte[16] : _crypto.DeriveSm4Key(RootKey, SystemMasterFactor, RepeatFirstFour(currentEsamIssuer), chipSerial);
                byte[] currentEsamApplicationMaster = esamType == 0 ? new byte[16] : _crypto.DeriveSm4Key(RootKey, ApplicationMasterFactor, RepeatFirstFour(currentEsamIssuer), chipSerial);

                if (!Transfer(service, CreateDirectoryFrame(0x02, 0x3F00), exchanges, token, "ESAM进入3F00发行目录", log)) return Finish(false, "ESAM发行前进入3F00失败。", 0, 1, exchanges);
                if (!ReplaceMasterKeyWithLegacyRetry(service, 0x02, EsamKeyRecord(0, esamKeys[0]), currentEsamSystemMaster, esamType == 0 ? esamKeys[0] : new byte[16], exchanges, token, log)) return Finish(false, "替换ESAM系统主控密钥失败。", 0, 1, exchanges);
                if (!ReplaceKey(service, 0x02, 1, EsamKeyRecord(1, esamKeys[1]), esamKeys[0], exchanges, token, log)) return Finish(false, "替换ESAM系统维护密钥失败。", 0, 1, exchanges);
                byte[] random = GetIssuanceRandom(service, 0x02, exchanges, token, log);
                if (random == null || !Transfer(service, CreateFirstIssueEf01Frame(fileEf01, contractSerial, esamKeys[1], random), exchanges, token, "更新ESAM EF01", log)) return Finish(false, "更新ESAM EF01失败。", 0, 1, exchanges);

                if (!Transfer(service, CreateDirectoryFrame(0x02, 0xDF01), exchanges, token, "ESAM进入DF01发行目录", log)) return Finish(false, "ESAM进入DF01失败。", 0, 1, exchanges);
                int[] esamIndexes = { 2, 3, 8, 9, 10, 11, 12 };
                for (int position = 0; position < esamIndexes.Length; position++)
                {
                    int index = esamIndexes[position];
                    byte[] current = index == 2 ? currentEsamApplicationMaster : esamKeys[2];
                    if (!ReplaceKey(service, 0x02, index, EsamKeyRecord(index, esamKeys[index]), current, exchanges, token, log)) return Finish(false, "替换ESAM DF01第" + index + "条密钥失败。", 0, 1, exchanges);
                }

                byte[] targetIccIssuer = Slice(file0015, 0, 8);
                byte[] targetIccSerial = Slice(file0015, 12, 8);
                byte[][] iccKeys = DeriveIccKeys(targetIccIssuer, chipSerial, targetIccSerial);
                byte[] currentIccIssuer = RepeatFirstFour(currentIccInfo);
                byte[] currentIccSystemMaster = iccType == 0 ? new byte[16] : _crypto.DeriveSm4Key(RootKey, SystemMasterFactor, currentIccIssuer, chipSerial);
                byte[] currentIccApplicationMaster = iccType == 0 ? new byte[16] : _crypto.DeriveSm4Key(RootKey, ApplicationMasterFactor, currentIccIssuer, chipSerial);
                if (!Transfer(service, CreateDirectoryFrame(0x01, 0x3F00), exchanges, token, "ICC进入3F00发行目录", log)) return Finish(false, "ICC进入3F00失败。", 0, 1, exchanges);
                if (!ReplaceMasterKeyWithLegacyRetry(service, 0x01, IccKeyRecord(0, iccKeys[0]), currentIccSystemMaster, iccType == 0 ? iccKeys[0] : new byte[16], exchanges, token, log)) return Finish(false, "替换ICC系统主控密钥失败。", 0, 1, exchanges);
                if (!ReplaceKey(service, 0x01, 1, IccKeyRecord(1, iccKeys[1]), iccKeys[0], exchanges, token, log)) return Finish(false, "替换ICC系统维护密钥失败。", 0, 1, exchanges);
                if (!Transfer(service, CreateDirectoryFrame(0x01, 0x1001), exchanges, token, "ICC进入1001发行目录", log)) return Finish(false, "ICC进入1001失败。", 0, 1, exchanges);
                int[] iccIndexes = { 2, 3, 13, 14, 15 };
                foreach (int index in iccIndexes)
                {
                    byte[] current = index == 2 ? currentIccApplicationMaster : iccKeys[2];
                    if (!ReplaceKey(service, 0x01, index, IccKeyRecord(index, iccKeys[index]), current, exchanges, token, log)) return Finish(false, "替换ICC 1001第" + index + "条密钥失败。", 0, 1, exchanges);
                }
                random = GetIssuanceRandom(service, 0x01, exchanges, token, log);
                if (random == null || !Transfer(service, CreateFirstIssue0015Frame(file0015, iccKeys[3], random), exchanges, token, "更新ICC 0015", log)) return Finish(false, "更新ICC 0015失败。", 0, 1, exchanges);
                Send(service, 0x04, SetMmi, exchanges, token, "SetMMI（旧用例仅记录回复）", log, false);
                Send(service, 0x05, EventReport, exchanges, token, "EventReport", log, false);
                return Finish(true, "单片式一次发行流程通过。", 1, 1, exchanges);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                TryEventReport(service, exchanges);
                return Finish(false, "单片式一次发行异常：" + exception.Message, 0, 1, exchanges);
            }
        }

        /// <summary>
        /// 执行旧 <c>WJ_SecondIssue_DoubleChannel</c> 的SM4二次发行流程。
        /// </summary>
        /// <param name="service">已打开并初始化的台发服务。</param><param name="log">线程安全日志回调。</param><param name="token">停止令牌。</param>
        /// <returns>包含全部原始帧和最终判定的结果。</returns>
        internal LaneTransactionExecutionResult ExecuteSecondIssue(DesktopCommService service, Action<string> log, CancellationToken token)
        {
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            try
            {
                byte[] original0015 = ReadIniHex("SET_Frame", "0015", 43);
                byte[] second0015 = ReadIniHex("SET_Frame_second", "seond_0015", 8);
                byte[] secondEf01 = ReadIniHex("SET_Frame_second", "second_EF01", 8);
                int count = ReadIniInt("SET_139_Dodge_num", "number", 1);
                for (int round = 0; round < count; round++)
                {
                    token.ThrowIfCancellationRequested();
                    TransparentCommandResult result = Send(service, 0x01, Bst, exchanges, token, "BST/VST", log, false);
                    if (!result.IsSuccess || result.ResponseData.Length < 32) return Finish(false, "VST不足32字节，无法取得发行方和芯片序列号分散因子。", round, count, exchanges);
                    byte[] issuer = Slice(result.ResponseData, 14, 8);
                    byte[] chipSerial = Slice(result.ResponseData, 24, 8);

                    if (!Transfer(service, CreateDirectoryFrame(0x02, 0x3F00), exchanges, token, "ESAM进入3F00", log)) return Finish(false, "ESAM进入3F00失败。", round, count, exchanges);
                    byte[] random4 = GetRandom(service, 0x02, 4, exchanges, token, log);
                    if (random4 == null) return Finish(false, "ESAM取得4字节随机数失败。", round, count, exchanges);
                    byte[] systemKey = _crypto.DeriveSm4Key(RootKey, SystemMaintenanceFactor, issuer, chipSerial);
                    if (!Transfer(service, CreateSecureUpdateFrame(0x02, 0x81, 0x12, secondEf01, systemKey, random4), exchanges, token, "更新ESAM合同日期", log)) return Finish(false, "更新ESAM合同日期失败。", round, count, exchanges);

                    if (!Transfer(service, CreateDirectoryFrame(0x02, 0xDF01), exchanges, token, "ESAM进入DF01", log)) return Finish(false, "ESAM进入DF01失败。", round, count, exchanges);
                    random4 = GetRandom(service, 0x02, 4, exchanges, token, log);
                    if (random4 == null) return Finish(false, "ESAM更新车辆信息前取随机数失败。", round, count, exchanges);
                    byte[] applicationKey = _crypto.DeriveSm4Key(RootKey, ApplicationMaintenanceFactor, issuer, chipSerial);
                    if (!Transfer(service, CreateVehicleUpdateFrame(applicationKey, random4), exchanges, token, "更新ESAM车辆信息", log)) return Finish(false, "更新ESAM车辆信息失败。", round, count, exchanges);

                    byte[] random8 = GetRandom(service, 0x02, 8, exchanges, token, log);
                    if (random8 == null) return Finish(false, "读取加密车辆信息前取8字节随机数失败。", round, count, exchanges);
                    result = Send(service, 0x03, CreateEncryptedVehicleReadFrame(random8), exchanges, token, "读取加密车辆信息", log, true);
                    if (!IsSuccessfulTransfer(result) || result.ResponseData.Length < 78) return Finish(false, "加密车辆信息读取失败或长度不足。", round, count, exchanges);
                    byte[] encryptionKey = _crypto.DeriveSm4Key(RootKey, ApplicationEncryptionFactor, issuer, chipSerial);
                    byte[] plain = _crypto.DecryptSm4(encryptionKey, Slice(result.ResponseData, 14, 64));
                    if (!EqualsAt(plain, 17, VehicleInformation)) return Finish(false, "解密后的车辆信息与旧用例固定数据不一致。", round, count, exchanges);

                    if (!Transfer(service, CreateDirectoryFrame(0x01, 0x1001), exchanges, token, "ICC进入1001", log)) return Finish(false, "ICC进入1001失败。", round, count, exchanges);
                    random4 = GetRandom(service, 0x01, 4, exchanges, token, log);
                    if (random4 == null) return Finish(false, "ICC更新0015前取随机数失败。", round, count, exchanges);
                    byte[] iccIssuer = new byte[8];
                    Buffer.BlockCopy(original0015, 0, iccIssuer, 0, 4);
                    Buffer.BlockCopy(original0015, 0, iccIssuer, 4, 4);
                    byte[] iccSerial = Slice(original0015, 12, 8);
                    byte[] iccKey = _crypto.DeriveSm4Key(RootKey, ApplicationMaintenanceFactor, iccIssuer, iccSerial);
                    if (!Transfer(service, CreateIcc0015UpdateFrame(second0015, iccKey, random4), exchanges, token, "更新ICC 0015", log)) return Finish(false, "更新ICC 0015失败。", round, count, exchanges);
                    if (!Transfer(service, CreateReadBinaryFrame(0x01, 0x95, 0x2B), exchanges, token, "复读ICC 0015", log)) return Finish(false, "复读ICC 0015失败。", round, count, exchanges);
                    Send(service, 0x04, SetMmi, exchanges, token, "SetMMI（旧用例仅记录回复）", log, false);
                    Send(service, 0x05, EventReport, exchanges, token, "EventReport", log, false);
                    if (token.WaitHandle.WaitOne(1000)) token.ThrowIfCancellationRequested();
                }
                return Finish(true, "单片式二次发行流程通过。", count, count, exchanges);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                TryEventReport(service, exchanges);
                return Finish(false, "单片式二次发行异常：" + exception.Message, 0, 1, exchanges);
            }
        }

        /// <summary>执行旧 <c>WJ_OneChip_TamperFlagResetOne</c> 的SM4标签激活流程。</summary>
        /// <param name="service">已打开并初始化的台发服务。</param><param name="log">线程安全日志回调。</param><param name="token">停止令牌。</param>
        /// <returns>包含全部原始帧和最终判定的结果。</returns>
        internal LaneTransactionExecutionResult ExecuteActivation(DesktopCommService service, Action<string> log, CancellationToken token)
        {
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int count = ReadIniInt("SET_139_Dodge_num", "number", 1);
            int success = 0;
            try
            {
                for (int round = 0; round < count; round++)
                {
                    token.ThrowIfCancellationRequested();
                    TransparentCommandResult result = Send(service, 0x01, Bst, exchanges, token, "BST/VST", log, false);
                    if (!result.IsSuccess || result.ResponseData.Length < 32) return Finish(false, "VST不足32字节，无法取得激活分散因子。", success, count, exchanges);
                    byte[] issuer = Slice(result.ResponseData, 14, 8);
                    byte[] chipSerial = Slice(result.ResponseData, 24, 8);
                    if (!Transfer(service, CreateDirectoryFrame(0x02, 0x3F00), exchanges, token, "ESAM进入3F00", log)) return Finish(false, "ESAM进入3F00失败。", success, count, exchanges);
                    if (!Transfer(service, CreateReadBinaryFrame(0x02, 0x81, 0x1B), exchanges, token, "读取ESAM EF01", log)) return Finish(false, "读取ESAM EF01失败。", success, count, exchanges);
                    byte[] random = GetRandom(service, 0x02, 4, exchanges, token, log);
                    if (random == null) return Finish(false, "激活前取得4字节随机数失败。", success, count, exchanges);
                    byte[] key = _crypto.DeriveSm4Key(RootKey, SystemMaintenanceFactor, issuer, chipSerial);
                    byte[] message = { 0x04,0xD6,0x81,0x1A,0x05,0x01 };
                    byte[] mac = _crypto.CalculateSm4Mac(key, random, message);
                    byte[] update = CreateTransfer(0x02, AddApduLength(Join(message, mac)));
                    if (!Transfer(service, update, exchanges, token, "更新ESAM防拆字节为01", log)) return Finish(false, "更新防拆字节失败。", success, count, exchanges);
                    success++;
                    Send(service, 0x04, SetMmi, exchanges, token, "SetMMI（旧用例仅记录回复）", log, false);
                    Send(service, 0x05, EventReport, exchanges, token, "EventReport", log, false);
                    if (token.WaitHandle.WaitOne(1000)) token.ThrowIfCancellationRequested();
                }
                return Finish(true, "单片式标签激活流程通过。", success, count, exchanges);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                TryEventReport(service, exchanges);
                return Finish(false, "单片式标签激活异常：" + exception.Message, success, count, exchanges);
            }
        }

        private static bool Transfer(DesktopCommService service, byte[] payload, ICollection<ObuProtocolExchange> exchanges, CancellationToken token, string name, Action<string> log)
        {
            TransparentCommandResult result = Send(service, 0x03, payload, exchanges, token, name, log, true);
            return IsSuccessfulTransfer(result);
        }

        private static TransparentCommandResult Send(DesktopCommService service, byte type, byte[] payload, ICollection<ObuProtocolExchange> exchanges, CancellationToken token, string name, Action<string> log, bool validateTransfer)
        {
            token.ThrowIfCancellationRequested();
            log(name + "。\r\n");
            byte[] command = new byte[payload.Length + 2]; command[1] = type; Buffer.BlockCopy(payload, 0, command, 2, payload.Length);
            TransparentCommandResult result = service.ExecuteTransparent(command, TimeoutMilliseconds);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
            if (!result.IsSuccess || (validateTransfer && !IsSuccessfulTransfer(result))) log(name + "失败：" + result.ErrorMessage + "\r\n");
            return result;
        }

        private static bool IsSuccessfulTransfer(TransparentCommandResult result)
        {
            byte[] data = result == null ? null : result.ResponseData;
            return result != null && result.IsSuccess && data != null && data.Length >= 3 && data[data.Length - 3] == 0x90 && data[data.Length - 2] == 0x00 && data[data.Length - 1] == 0x00;
        }

        private static byte[] GetRandom(DesktopCommService service, byte channel, byte length, ICollection<ObuProtocolExchange> exchanges, CancellationToken token, Action<string> log)
        {
            TransparentCommandResult result = Send(service, 0x03, CreateTransfer(channel, new byte[] { 0x05,0x00,0x84,0x00,0x00,length }), exchanges, token, "通道" + channel + "取" + length + "字节随机数", log, true);
            return IsSuccessfulTransfer(result) && result.ResponseData.Length >= 14 + length ? Slice(result.ResponseData, 14, length) : null;
        }

        private static byte[] CreateDirectoryFrame(byte channel, ushort fid) { return CreateTransfer(channel, new byte[] { 0x07,0x00,0xA4,0x00,0x00,0x02,(byte)(fid >> 8),(byte)fid }); }
        private static byte[] CreateReadBinaryFrame(byte channel, byte p1, byte length) { return CreateTransfer(channel, new byte[] { 0x05,0x00,0xB0,p1,0x00,length }); }
        private static byte[] CreateTransfer(byte channel, byte[] apdu)
        {
            byte[] frame = new byte[13 + apdu.Length]; byte[] header = { 0x08,0x00,0x00,0x01,0x40,0x77,0x91,0x05,0x01,0x03,0x18,channel,0x01 };
            Buffer.BlockCopy(header, 0, frame, 0, header.Length); Buffer.BlockCopy(apdu, 0, frame, header.Length, apdu.Length); return frame;
        }

        private byte[] CreateSecureUpdateFrame(byte channel, byte fileId, byte offset, byte[] data, byte[] key, byte[] random)
        {
            byte[] message = new byte[5 + data.Length]; message[0] = 0x04; message[1] = 0xD6; message[2] = fileId; message[3] = offset; message[4] = (byte)(data.Length + 4); Buffer.BlockCopy(data, 0, message, 5, data.Length);
            return CreateTransfer(channel, AddApduLength(Join(message, _crypto.CalculateSm4Mac(key, random, message))));
        }

        private byte[] CreateVehicleUpdateFrame(byte[] key, byte[] random)
        {
            byte[] message = new byte[37]; message[0] = 0x04; message[1] = 0xD6; message[2] = 0x81; message[3] = 0x00; message[4] = 0x24; Buffer.BlockCopy(VehicleInformation, 0, message, 5, VehicleInformation.Length);
            return CreateTransfer(0x02, AddApduLength(Join(message, _crypto.CalculateSm4Mac(key, random, message))));
        }

        private static byte[] CreateEncryptedVehicleReadFrame(byte[] random)
        {
            // 原 WjReadVehicleInfoAndDecrypt：0F为后续APDU正文长度，随机数后依次是密钥索引20和SM4密钥版本40。
            byte[] apdu = new byte[16];
            apdu[0] = 0x0F;
            apdu[1] = 0x00; apdu[2] = 0xB4; apdu[3] = 0x00; apdu[4] = 0x00; apdu[5] = 0x0A;
            Buffer.BlockCopy(random, 0, apdu, 6, 8);
            apdu[14] = 0x20;
            apdu[15] = 0x40;
            return CreateTransfer(0x02, apdu);
        }

        private byte[] CreateIcc0015UpdateFrame(byte[] dates, byte[] key, byte[] random)
        {
            byte[] message = { 0x04,0xD6,0x95,0x14,0x18,0,0,0,0,0,0,0,0,0xB2,0xE2,0x41,0x31,0x32,0x33,0x34,0x35,0,0,0,0 };
            Buffer.BlockCopy(dates, 0, message, 5, 8); return CreateTransfer(0x01, AddApduLength(Join(message, _crypto.CalculateSm4Mac(key, random, message))));
        }

        /// <summary>在动态构造的APDU前写入TransferChannel APDU项长度；长度字节本身不参与安全报文MAC。</summary>
        private static byte[] AddApduLength(byte[] apduBody)
        {
            if (apduBody == null || apduBody.Length == 0 || apduBody.Length > byte.MaxValue) throw new ArgumentException("APDU正文长度必须为1～255字节。", nameof(apduBody));
            byte[] encoded = new byte[apduBody.Length + 1]; encoded[0] = (byte)apduBody.Length; Buffer.BlockCopy(apduBody, 0, encoded, 1, apduBody.Length); return encoded;
        }

        private bool ReplaceKey(DesktopCommService service, byte channel, int index, byte[] record, byte[] currentKey, ICollection<ObuProtocolExchange> exchanges, CancellationToken token, Action<string> log)
        {
            byte[] random = GetIssuanceRandom(service, channel, exchanges, token, log); if (random == null) return false;
            byte[] encrypted = _crypto.EncryptSm4(currentKey, record); byte p1 = index == 0 || index == 2 ? (byte)0x00 : (byte)0xFF;
            byte[] message = new byte[37]; message[0]=0x84; message[1]=0xD4; message[2]=0x01; message[3]=p1; message[4]=0x24; Buffer.BlockCopy(encrypted,0,message,5,32);
            byte[] apdu = Join(new byte[]{0x29}, Join(message, _crypto.CalculateSm4Mac(currentKey, random, message)));
            return Transfer(service, CreateTransfer(channel, apdu), exchanges, token, "替换通道"+channel+"第"+index+"条密钥", log);
        }

        /// <summary>按旧一次发行逻辑，首条主控密钥失败时重新取4字节随机数并切换空白/自建当前密钥重试。</summary>
        private bool ReplaceMasterKeyWithLegacyRetry(DesktopCommService service, byte channel, byte[] record, byte[] primaryKey, byte[] alternateKey, ICollection<ObuProtocolExchange> exchanges, CancellationToken token, Action<string> log)
        {
            if (ReplaceKey(service, channel, 0, record, primaryKey, exchanges, token, log)) return true;
            log("首次替换系统主控密钥返回失败，按旧用例切换空白/自建当前密钥后重试。\r\n");
            return ReplaceKey(service, channel, 0, record, alternateKey, exchanges, token, log);
        }

        /// <summary>原一次发行只向卡片申请4字节随机数，参与SM4 MAC前在尾部补4字节00。</summary>
        private static byte[] GetIssuanceRandom(DesktopCommService service, byte channel, ICollection<ObuProtocolExchange> exchanges, CancellationToken token, Action<string> log)
        {
            byte[] random4 = GetRandom(service, channel, 4, exchanges, token, log);
            if (random4 == null) return null;
            byte[] random8 = new byte[8]; Buffer.BlockCopy(random4, 0, random8, 0, 4); return random8;
        }

        private byte[][] DeriveEsamKeys(byte[] issuer, byte[] chipSerial, byte[] contractSerial)
        {
            byte[][] keys=new byte[13][];
            keys[0]=_crypto.DeriveSm4Key(RootKey,SystemMasterFactor,issuer,chipSerial);
            keys[1]=_crypto.DeriveSm4Key(RootKey,SystemMaintenanceFactor,issuer,contractSerial);
            keys[2]=_crypto.DeriveSm4Key(RootKey,ApplicationMasterFactor,issuer,chipSerial);
            keys[3]=_crypto.DeriveSm4Key(RootKey,ApplicationMaintenanceFactor,issuer,contractSerial);
            keys[8]=_crypto.DeriveSm4Key(RootKey,new byte[]{0xCD,0xE2,0xB2,0xBF,0xC8,0xCF,0xD6,0xA4},issuer,contractSerial);
            keys[9]=_crypto.DeriveSm4Key(RootKey,new byte[]{0xBC,0xD3,0xC3,0xDC,0x00,0x00,0x00,0x03},issuer,contractSerial);
            keys[10]=_crypto.DeriveSm4Key(RootKey,new byte[]{0xBC,0xD3,0xC3,0xDC,0x00,0x00,0x00,0x02},issuer,contractSerial);
            keys[11]=_crypto.DeriveSm4Key(RootKey,new byte[]{0xBC,0xD3,0xC3,0xDC,0x00,0x00,0x00,0x01},issuer,contractSerial);
            keys[12]=_crypto.DeriveSm4Key(RootKey,new byte[]{0xD3,0xA6,0xD3,0xC3,0xC8,0xCF,0xD6,0xA4},issuer,contractSerial);
            return keys;
        }

        private byte[][] DeriveIccKeys(byte[] issuer, byte[] chipSerial, byte[] cardSerial)
        {
            byte[][] keys=new byte[16][]; keys[0]=_crypto.DeriveSm4Key(RootKey,SystemMasterFactor,issuer,chipSerial); keys[1]=_crypto.DeriveSm4Key(RootKey,SystemMaintenanceFactor,issuer,cardSerial); keys[2]=_crypto.DeriveSm4Key(RootKey,ApplicationMasterFactor,issuer,chipSerial); keys[3]=_crypto.DeriveSm4Key(RootKey,ApplicationMaintenanceFactor,issuer,cardSerial);
            keys[13]=_crypto.DeriveSm4Key(new byte[]{0x5C,0x2B,0x20,0xB0,0x65,0x72,0xC6,0x88,0x38,0xED,0x39,0x00,0x00,0xDC,0xBF,0x16},issuer,cardSerial); keys[14]=_crypto.DeriveSm4Key(Repeat(0xFF,16),issuer,cardSerial); keys[15]=_crypto.DeriveSm4Key(new byte[]{0xB6,0xC4,0x30,0x72,0xD0,0x62,0x6F,0xC5,0x02,0xC8,0x49,0x1F,0x1F,0x01,0x5B,0x87},issuer,cardSerial); return keys;
        }

        private static byte[] EsamKeyRecord(int index, byte[] key)
        {
            byte[][] prefixes={new byte[]{0x13,0x00,0x40,0x00},new byte[]{0x13,0x01,0x41,0x00},new byte[]{0x13,0x00,0x40,0x00},new byte[]{0x13,0x01,0x41,0x00},null,null,null,null,new byte[]{0x13,0x00,0x41,0x00},new byte[]{0x13,0x01,0x43,0x40},new byte[]{0x13,0x01,0x43,0x41},new byte[]{0x13,0x01,0x43,0x42},new byte[]{0x13,0x01,0x42,0x00}}; return CreateKeyRecord(prefixes[index],key);
        }
        private static byte[] IccKeyRecord(int index, byte[] key)
        {
            byte[] prefix; switch(index){case 0:prefix=new byte[]{0x13,0x00,0x40,0x00};break;case 1:prefix=new byte[]{0x13,0x01,0x41,0x00};break;case 2:prefix=new byte[]{0x13,0x00,0x40,0x00};break;case 3:prefix=new byte[]{0x13,0x01,0x41,0x00};break;case 13:prefix=new byte[]{0x13,0x00,0x41,0x00};break;case 14:prefix=new byte[]{0x13,0x08,0x40,0x00};break;default:prefix=new byte[]{0x13,0x17,0x41,0x41};break;} return CreateKeyRecord(prefix,key);
        }
        private static byte[] CreateKeyRecord(byte[] prefix,byte[] key){byte[] record=new byte[32];Buffer.BlockCopy(prefix,0,record,0,4);Buffer.BlockCopy(key,0,record,4,16);record[20]=0x80;return record;}
        private static byte[] CreateChipSerialFrame()
        {
            byte[] frame=CreateTransfer(0x02,new byte[]{0x07,0x00,0xA4,0x00,0x00,0x02,0x3F,0x00,0x05,0x80,0xF6,0x00,0x03,0x04});
            frame[12]=0x02; return frame;
        }
        private byte[] CreateFirstIssueEf01Frame(byte[] configured,byte[] contractSerial,byte[] key,byte[] random){byte[] data=Slice(configured,0,27);Buffer.BlockCopy(contractSerial,0,data,10,8);return CreateSecureUpdateFrame(0x02,0x81,0x00,data,key,random);}
        private byte[] CreateFirstIssue0015Frame(byte[] configured,byte[] key,byte[] random){return CreateSecureUpdateFrame(0x01,0x95,0x00,Slice(configured,0,20),key,random);}
        private static byte[] ReadTransferBytes(TransparentCommandResult result,int count,string name){if(!IsSuccessfulTransfer(result)||result.ResponseData.Length<14+count)throw new InvalidDataException(name+"回复失败或数据长度不足。");return Slice(result.ResponseData,14,count);}
        /// <summary>按TransferChannel DataList结构提取指定APDU的有效数据，不把前一条响应的长度和状态误作数据。</summary>
        private static byte[] ReadTransferData(TransparentCommandResult result, int dataIndex, int minimumLength, string name)
        {
            if (result == null || !result.IsSuccess || result.ResponseData == null || result.ResponseData.Length < 14) throw new InvalidDataException(name + "回复失败或结构不完整。");
            byte[] response = result.ResponseData; int dataCount = response[12];
            if (dataIndex < 0 || dataIndex >= dataCount) throw new InvalidDataException(name + "缺少第" + (dataIndex + 1) + "条Data。");
            int offset = 13;
            for (int index = 0; index < dataCount; index++)
            {
                if (offset >= response.Length) throw new InvalidDataException(name + "回复缺少长度字段。");
                int length = response[offset++];
                if (length < 2 || offset + length > response.Length) throw new InvalidDataException(name + "回复Data长度无效。");
                if (index == dataIndex)
                {
                    int payloadLength = length - 2;
                    if (response[offset + payloadLength] != 0x90 || response[offset + payloadLength + 1] != 0x00) throw new InvalidDataException(name + "APDU状态不是9000。");
                    if (payloadLength < minimumLength) throw new InvalidDataException(name + "有效数据长度不足" + minimumLength + "字节。");
                    return Slice(response, offset, payloadLength);
                }
                offset += length;
            }
            throw new InvalidDataException(name + "未找到目标Data。");
        }
        private static int ClassifyIssuer(byte[] data){bool zero=true,ff=true;for(int i=0;i<4;i++){zero&=data[i]==0;ff&=data[i]==0xFF;}if(zero||ff)return 0;return data[0]==0x45&&data[1]==0x54&&data[2]==0x43&&data[3]==0x01?2:1;}
        private static byte[] RepeatFirstFour(byte[] data){byte[] result=new byte[8];Buffer.BlockCopy(data,0,result,0,4);Buffer.BlockCopy(data,0,result,4,4);return result;}
        private static byte[] Repeat(byte value,int count){byte[] result=new byte[count];for(int i=0;i<count;i++)result[i]=value;return result;}

        private static byte[] ReadIniHex(string section, string key, int minimumLength)
        {
            string value = ReadIniValue(section, key); StringBuilder hex = new StringBuilder(); foreach (char c in value) if (Uri.IsHexDigit(c)) hex.Append(c);
            if (hex.Length == 0 || hex.Length % 2 != 0) throw new InvalidDataException("SetMe.ini [" + section + "] " + key + " 不是有效偶数字节十六进制。");
            byte[] data = new byte[hex.Length / 2]; for (int i = 0; i < data.Length; i++) data[i] = byte.Parse(hex.ToString(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (data.Length < minimumLength) throw new InvalidDataException("SetMe.ini [" + section + "] " + key + " 长度不足" + minimumLength + "字节。"); return data;
        }

        private static int ReadIniInt(string section, string key, int defaultValue) { return int.TryParse(ReadIniValue(section, key), out int value) && value > 0 ? value : defaultValue; }
        private static string ReadIniValue(string section, string key)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetMe.ini"); if (!File.Exists(path)) throw new FileNotFoundException("未找到发行配置SetMe.ini。", path);
            string current = string.Empty; foreach (string raw in File.ReadAllLines(path, Encoding.Default)) { string line = raw.Trim(); if (line.StartsWith("[") && line.EndsWith("]")) current = line.Substring(1, line.Length - 2); else if (string.Equals(current, section, StringComparison.OrdinalIgnoreCase)) { int index = line.IndexOf('='); if (index > 0 && string.Equals(line.Substring(0, index).Trim(), key, StringComparison.OrdinalIgnoreCase)) return line.Substring(index + 1).Trim(); } }
            throw new InvalidDataException("SetMe.ini缺少[" + section + "] " + key + "。");
        }

        private static byte[] Slice(byte[] data, int offset, int count) { byte[] result = new byte[count]; Buffer.BlockCopy(data, offset, result, 0, count); return result; }
        private static byte[] Join(byte[] first, byte[] second) { byte[] result = new byte[first.Length + second.Length]; Buffer.BlockCopy(first, 0, result, 0, first.Length); Buffer.BlockCopy(second, 0, result, first.Length, second.Length); return result; }
        private static bool EqualsAt(byte[] source, int offset, byte[] expected) { if (source == null || offset < 0 || source.Length < offset + expected.Length) return false; for (int i = 0; i < expected.Length; i++) if (source[offset + i] != expected[i]) return false; return true; }
        private static LaneTransactionExecutionResult Finish(bool success, string message, int completed, int total, IList<ObuProtocolExchange> exchanges) { return new LaneTransactionExecutionResult(success, message, completed, total, exchanges, DateTime.Now); }
        private static void TryEventReport(DesktopCommService service, ICollection<ObuProtocolExchange> exchanges) { try { if (service != null && service.IsOpen) { byte[] command = new byte[EventReport.Length + 2]; command[1] = 0x05; Buffer.BlockCopy(EventReport, 0, command, 2, EventReport.Length); exchanges.Add(ObuProtocolExchange.FromTransparentResult(service.ExecuteTransparent(command, TimeoutMilliseconds))); } } catch { } }
    }
}
