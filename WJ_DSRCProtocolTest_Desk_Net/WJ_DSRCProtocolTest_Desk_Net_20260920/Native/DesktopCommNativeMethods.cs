using System.Runtime.InteropServices;

namespace WJ_DSRCProtocolTest_Desk_Net.Native
{
    internal static class DesktopCommNativeMethods
    {
        private const string LibraryName = "DeskTopComm.dll";

        [DllImport(LibraryName, EntryPoint = "fDeskTop_Open", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        internal static extern int Open(int mode, string device, int port);

        [DllImport(LibraryName, EntryPoint = "fDeskTop_Close", CallingConvention = CallingConvention.StdCall)]
        internal static extern int Close(int handle);

        [DllImport(LibraryName, EntryPoint = "fDeskTop_INIT_rq", CallingConvention = CallingConvention.StdCall)]
        internal static extern int InitializeRequest(
            int handle,
            [In] byte[] unixTime,
            int bstInterval,
            int retryInterval,
            int retryTimes,
            int txPower,
            int physicalChannelId,
            int bidChange,
            int llcChange,
            int unixTimeChange,
            int macChange,
            int sendDataMode,
            int bstMode14K,
            int timeout);

        [DllImport(LibraryName, EntryPoint = "fDeskTop_INIT_rs", CallingConvention = CallingConvention.StdCall)]
        internal static extern int InitializeResponse(
            int handle,
            ref int desktopStatus,
            [Out] byte[] desktopInfo,
            int timeout);

        [DllImport(LibraryName, EntryPoint = "fOpenDLLLog", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SetLogEnabled(int mode);

        /// <summary>
        /// 通过 stdcall 调用 x86 DeskTopComm.dll 的 fBST，在已初始化台发句柄上发送 BST 数据。
        /// </summary>
        /// <param name="handle">由 fDeskTop_Open 返回且仍有效的台发句柄，所有权仍归服务层。</param>
        /// <param name="frameData">由调用方持有的 BST 数据缓冲区，DLL 在调用期间读取。</param>
        /// <param name="length">缓冲区中参与发送的有效字节数。</param>
        /// <param name="timeout">DLL 调用超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fBST", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendBst(int handle, [In, Out] byte[] frameData, int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 x86 DeskTopComm.dll 的 fVST，在发送 BST 后接收 VST 数据。
        /// </summary>
        /// <param name="handle">已打开并初始化的台发句柄。</param>
        /// <param name="frameData">调用方分配的接收缓冲区，DLL 写入响应，所有权不转移。</param>
        /// <param name="length">传入缓冲区容量并由 DLL 写回有效响应长度。</param>
        /// <param name="timeout">接收超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fVST", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReceiveVst(int handle, [Out] byte[] frameData, ref int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fGetSecure_rq 发送 GetSecure 请求。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">请求数据缓冲区，所有权归调用方。</param>
        /// <param name="length">请求有效字节数。</param>
        /// <param name="timeout">发送超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fGetSecure_rq", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendGetSecure(int handle, [In, Out] byte[] frameData, int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fGetSecure_rs 接收 GetSecure 响应。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">调用方分配的接收缓冲区。</param>
        /// <param name="length">传入容量并写回有效字节数。</param>
        /// <param name="timeout">接收超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fGetSecure_rs", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReceiveGetSecure(int handle, [Out] byte[] frameData, ref int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fTransferChannel_rq 发送 TransferChannel 请求。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">请求数据缓冲区。</param>
        /// <param name="length">请求有效字节数。</param>
        /// <param name="timeout">发送超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fTransferChannel_rq", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendTransferChannel(int handle, [In, Out] byte[] frameData, int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fTransferChannel_rs 接收 TransferChannel 响应。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">调用方分配的接收缓冲区。</param>
        /// <param name="length">传入容量并写回有效字节数。</param>
        /// <param name="timeout">接收超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fTransferChannel_rs", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReceiveTransferChannel(int handle, [Out] byte[] frameData, ref int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fSetMMI_rq 发送 SetMMI 请求。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">请求数据缓冲区。</param>
        /// <param name="length">请求有效字节数。</param>
        /// <param name="timeout">发送超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fSetMMI_rq", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendSetMmi(int handle, [In, Out] byte[] frameData, int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fSetMMI_rs 接收 SetMMI 响应。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">调用方分配的接收缓冲区。</param>
        /// <param name="length">传入容量并写回有效字节数。</param>
        /// <param name="timeout">接收超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fSetMMI_rs", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReceiveSetMmi(int handle, [Out] byte[] frameData, ref int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fEventReport 发送单向 EventReport 帧，不等待响应。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">事件报告数据缓冲区。</param>
        /// <param name="length">有效字节数。</param>
        /// <param name="timeout">发送超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fEventReport", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendEventReport(int handle, [In, Out] byte[] frameData, int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fOtherFrames_rq 发送其他类型请求帧。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">请求数据缓冲区。</param>
        /// <param name="length">请求有效字节数。</param>
        /// <param name="timeout">发送超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fOtherFrames_rq", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendOtherFrame(int handle, [In, Out] byte[] frameData, int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fOtherFrames_rs 接收其他类型响应帧。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="frameData">调用方分配的接收缓冲区。</param>
        /// <param name="length">传入容量并写回有效字节数。</param>
        /// <param name="timeout">接收超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fOtherFrames_rs", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReceiveOtherFrame(int handle, [Out] byte[] frameData, ref int length, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fICC_CHANNEL_rq 向指定 IC/CPC 卡槽发送 APDU 数据。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="slot">卡槽号；透传页沿用原上位机固定值 1。</param>
        /// <param name="frameData">首字节为后续 APDU 长度的请求缓冲区。</param>
        /// <param name="timeout">发送超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fICC_CHANNEL_rq", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendIccChannel(int handle, int slot, [In, Out] byte[] frameData, int timeout);

        /// <summary>
        /// 通过 stdcall 调用 fICC_CHANNEL_rs 接收 IC/CPC 卡槽响应。
        /// </summary>
        /// <param name="handle">已初始化台发句柄。</param>
        /// <param name="apduList">APDU 序号，透传页初始传入 0 并允许 DLL 更新。</param>
        /// <param name="frameData">调用方分配的响应缓冲区，首字节包含原 DLL 定义的长度信息。</param>
        /// <param name="timeout">接收超时时间，单位为毫秒。</param>
        /// <returns>返回 0 表示成功，其他值为 DLL 错误码。</returns>
        [DllImport(LibraryName, EntryPoint = "fICC_CHANNEL_rs", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReceiveIccChannel(int handle, ref int apduList, [Out] byte[] frameData, int timeout);
    }
}
