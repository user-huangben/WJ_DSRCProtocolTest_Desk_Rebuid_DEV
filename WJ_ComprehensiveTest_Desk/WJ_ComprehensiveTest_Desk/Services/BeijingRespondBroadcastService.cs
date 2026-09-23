using System;
using System.Collections.Generic;
using System.Threading;

namespace WJ_ComprehensiveTest_Desk.Services
{
    /// <summary>执行旧北京地标 WJ_OneChip_RespondBroadcast 的两轮广播响应测试。</summary>
    internal sealed class BeijingRespondBroadcastService
    {
        private const int TimeoutMilliseconds = 500;

        /// <summary>执行单个拆分后的北京地标广播响应子测试。</summary>
        /// <param name="afterPurchaseInitialization">为 true 时在第二次 BST 前执行消费初始化。</param>
        internal BeijingRespondBroadcastResult Execute(DesktopCommService comm, bool afterPurchaseInitialization, bool useSm4, CancellationToken cancellationToken)
        {
            if (comm == null) throw new ArgumentNullException(nameof(comm));
            var exchanges = new List<ObuProtocolExchange>();
            byte[] bst = { 0xFF,0xFF,0xFF,0xFF,0x50,0x03,0x91,0xC0,0x0E,0x00,0x05,0x95,0x62,0xBB,0xF7,0xC7,0x00,0x01,0x41,0x87,0x29,0xB0,0x1A,0x00,0x04,0x00,0x1C,0x00,0x2B,0x00 };
            byte[] secure = { 0x66,0x90,0x53,0x9C,0x40,0x77,0x91,0x05,0x01,0x00,0x14,0x80,0x01,0x00,0x00,0x3B,0xC9,0x94,0x87,0x34,0x3C,0x08,0xDE,0xE9,0x00,0x00 };
            byte[] initialize = { 0x08,0x00,0x00,0x01,0x40,0xF7,0x91,0x05,0x01,0x03,0x18,0x01,0x01,0x10,0x80,0x50,0x03,0x02,0x0B,0x01,0x00,0x00,0x00,0x01,0x37,0x37,0x37,0x37,0x37,0x37 };
            initialize[19] = useSm4 ? (byte)0x41 : (byte)0x01;
            int round = afterPurchaseInitialization ? 2 : 1;
            {
                cancellationToken.ThrowIfCancellationRequested();
                TransparentCommandResult result = comm.ExecuteTransparent(Build(1, bst), TimeoutMilliseconds);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (result.RequestResult != 0 || result.ResponseResult != 0) return Fail("第" + (round + 1) + "轮 BST/VST 建链失败：" + result.ErrorMessage, exchanges);
                result = comm.ExecuteTransparent(Build(2, secure), TimeoutMilliseconds);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (!result.IsSuccess) return Fail("第" + (round + 1) + "轮 GetSecure 失败：" + result.ErrorMessage, exchanges);
                if (afterPurchaseInitialization)
                {
                    result = comm.ExecuteTransparent(Build(3, initialize), TimeoutMilliseconds);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                    if (!result.IsSuccess || result.ResponseData.Length < 3 || result.ResponseData[result.ResponseData.Length - 3] != 0x90 || result.ResponseData[result.ResponseData.Length - 2] != 0x00 || result.ResponseData[result.ResponseData.Length - 1] != 0x00)
                        return Fail("消费初始化未返回 90 00 00。", exchanges);
                }
                result = comm.ExecuteTransparent(Build(1, bst), TimeoutMilliseconds);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (result.RequestResult != 0) return Fail("第" + (round + 1) + "轮响应广播 BST 发送失败。", exchanges);
                if (result.ResponseResult == 0) return Fail("第" + (round + 1) + "轮广播 BST 收到 VST，未满足无响应判定。", exchanges);
                if (cancellationToken.WaitHandle.WaitOne(3000)) cancellationToken.ThrowIfCancellationRequested();
            }
            return new BeijingRespondBroadcastResult(true,
                afterPurchaseInitialization
                    ? "消费初始化后发送广播 BST 测试通过：未收到 VST。"
                    : "获取车辆信息后发送广播 BST 测试通过：未收到 VST。", exchanges);
        }

        private static BeijingRespondBroadcastResult Fail(string message, IList<ObuProtocolExchange> exchanges) => new BeijingRespondBroadcastResult(false, message, exchanges);
        private static byte[] Build(byte type, byte[] payload)
        {
            byte[] command = new byte[payload.Length + 2];
            // DesktopCommService 使用两字节本地类型头：0001 BST、0002 GetSecure、0003 TransferChannel。
            command[0] = 0x00;
            command[1] = type;
            Buffer.BlockCopy(payload, 0, command, 2, payload.Length);
            return command;
        }
    }

    internal sealed class BeijingRespondBroadcastResult
    {
        internal BeijingRespondBroadcastResult(bool success, string message, IList<ObuProtocolExchange> exchanges) { IsSuccess = success; Message = message; Exchanges = exchanges; }
        internal bool IsSuccess { get; }
        internal string Message { get; }
        internal IList<ObuProtocolExchange> Exchanges { get; }
    }
}
