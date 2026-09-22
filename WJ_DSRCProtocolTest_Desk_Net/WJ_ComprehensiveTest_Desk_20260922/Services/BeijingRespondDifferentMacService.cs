using System;
using System.Collections.Generic;
using System.Threading;

namespace WJ_DSRCProtocolTest_Desk_Net.Services
{
    /// <summary>执行旧北京地标 WJ_OneChip_RespondDiffMac 的单个错误 MAC 子测试。</summary>
    internal sealed class BeijingRespondDifferentMacService
    {
        internal BeijingRespondDifferentMacResult Execute(DesktopCommService comm, int macVariant, CancellationToken cancellationToken)
        {
            if (comm == null) throw new ArgumentNullException(nameof(comm));
            if (macVariant < 1 || macVariant > 3) throw new ArgumentOutOfRangeException(nameof(macVariant));
            var exchanges = new List<ObuProtocolExchange>();
            DesktopInitializationOptions originalOptions;
            DesktopInitializationResult initialization = comm.BeginDifferentMacTestMode(out originalOptions);
            if (!initialization.IsSuccess) return new BeijingRespondDifferentMacResult(false, "不同 MAC 测试模式初始化失败。", exchanges);
            try
            {
                byte[] bst = { 0xFF,0xFF,0xFF,0xFF,0x50,0x03,0x91,0xC0,0x0E,0x00,0x05,0x9F,0x62,0xBB,0xF8,0x24,0x00,0x01,0x41,0x87,0x29,0xB0,0x1A,0x00,0x04,0x00,0x1C,0x00,0x2B,0x00 };
                byte[] secure = { 0x66,0x90,0x53,0x9C,0x40,0x77,0x91,0x05,0x01,0x00,0x14,0x80,0x01,0x00,0x00,0x3B,0xDB,0x27,0x0D,0xCD,0xDB,0xC6,0x9D,0xA7,0x00,0x00 };
                byte[] transfer = { 0x66,0x90,0x53,0x9D,0x40,0xF7,0x91,0x05,0x01,0x03,0x18,0x01,0x01,0x05,0x00,0xB0,0x95,0x00,0x2B };
                TransparentCommandResult result = comm.ExecuteTransparent(Build(1, bst), 500);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (!result.IsSuccess) return Fail("BST/VST 建链失败：" + result.ErrorMessage, exchanges);
                cancellationToken.ThrowIfCancellationRequested();
                result = comm.ExecuteTransparent(Build(2, secure), 500);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (!result.IsSuccess) return Fail("GetSecure 失败：" + result.ErrorMessage, exchanges);
                result = comm.ExecuteTransparent(Build(3, transfer), 500, true, macVariant);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (result.ResponseResult == 0) return Fail("错误 MAC 的 TransferChannel 收到响应。", exchanges);
                if (result.RequestResult != 0) return Fail("错误 MAC 的 TransferChannel 发送失败：" + result.ErrorMessage, exchanges);
                return new BeijingRespondDifferentMacResult(true, "错误 MAC 的 TransferChannel 未收到 OBU 响应，测试通过。", exchanges);
            }
            finally
            {
                comm.RestoreInitializationOptions(originalOptions);
            }
        }

        private static BeijingRespondDifferentMacResult Fail(string message, IList<ObuProtocolExchange> exchanges) => new BeijingRespondDifferentMacResult(false, message, exchanges);
        private static byte[] Build(byte type, byte[] payload)
        {
            byte[] command = new byte[payload.Length + 2]; command[0] = 0x00; command[1] = type; Buffer.BlockCopy(payload, 0, command, 2, payload.Length); return command;
        }
    }

    internal sealed class BeijingRespondDifferentMacResult
    {
        internal BeijingRespondDifferentMacResult(bool success, string message, IList<ObuProtocolExchange> exchanges) { IsSuccess = success; Message = message; Exchanges = exchanges; }
        internal bool IsSuccess { get; }
        internal string Message { get; }
        internal IList<ObuProtocolExchange> Exchanges { get; }
    }
}
