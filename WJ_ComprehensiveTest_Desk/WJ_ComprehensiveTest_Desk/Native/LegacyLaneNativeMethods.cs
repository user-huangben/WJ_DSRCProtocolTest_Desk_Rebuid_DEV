using System;
using System.Runtime.InteropServices;

namespace WJ_ComprehensiveTest_Desk.Native
{
    /// <summary>
    /// 原车道交易依赖 DLL 的最小 ABI 声明。所有入口集中在此文件，避免业务层散落 P/Invoke。
    /// </summary>
    internal static class LegacyLaneNativeMethods
    {
        private const string CardReaderLibrary = "mwCardReader.dll";
        private const string ReaderDriverLibrary = "wdcrwv.dll";
        private const string GmsslLibrary = "gmssl.dll";
        private const string Kernel32Library = "kernel32.dll";

        /// <summary>调用 wdcrwv.dll 的 3DES 单块/多块入口。</summary>
        /// <param name="mode">原 DLL 模式值；1 为加密/派生，2 为解密。</param>
        /// <param name="inputLength">输入长度，必须是 8 的倍数。</param>
        /// <param name="inputData">输入数据。</param>
        /// <param name="inputKey">16 字节 3DES 密钥。</param>
        /// <param name="outputData">输出缓冲区。</param>
        /// <returns>原 DLL 返回码，0 通常表示成功。</returns>
        [DllImport(ReaderDriverLibrary, EntryPoint = "TripleDES", CallingConvention = CallingConvention.StdCall)]
        internal static extern int TripleDes(uint mode, uint inputLength, byte[] inputData, byte[] inputKey, byte[] outputData);

        /// <summary>调用 wdcrwv.dll 的单 DES MAC 入口。</summary>
        /// <param name="singleMacKey">8 字节 MAC 密钥。</param>
        /// <param name="initData">8 字节初始向量。</param>
        /// <param name="sourceDataLength">参与计算的输入长度。</param>
        /// <param name="sourceData">待计算数据。</param>
        /// <param name="macData">至少 4 字节输出缓冲区。</param>
        /// <returns>原 DLL 返回码。</returns>
        [DllImport(ReaderDriverLibrary, EntryPoint = "SingleMAC", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SingleMac(byte[] singleMacKey, byte[] initData, uint sourceDataLength, byte[] sourceData, byte[] macData);

        /// <summary>调用 mwCardReader.dll 打开读卡器；当前车道交易仅声明该入口，实际卡机操作由后续卡槽 UI 驱动。</summary>
        /// <param name="readerName">读卡器名称的 ANSI 缓冲区。</param>
        /// <param name="readerHandle">输出读卡器句柄。</param>
        /// <param name="openMode">原 DLL 打开模式。</param>
        /// <param name="reserved">原 DLL 保留参数。</param>
        /// <returns>原 DLL 返回码。</returns>
        [DllImport(CardReaderLibrary, EntryPoint = "mw_dev_openReader", CallingConvention = CallingConvention.StdCall)]
        internal static extern int OpenReader(byte[] readerName, out IntPtr readerHandle, uint openMode, uint reserved);

        /// <summary>调用 mwCardReader.dll 关闭读卡器。</summary>
        /// <param name="readerHandle">由打开入口返回的句柄。</param>
        /// <returns>原 DLL 返回码。</returns>
        [DllImport(CardReaderLibrary, EntryPoint = "mw_dev_closeReader", CallingConvention = CallingConvention.StdCall)]
        internal static extern int CloseReader(IntPtr readerHandle);

        /// <summary>调用 mwCardReader.dll 执行 CPU 卡 APDU。</summary>
        /// <param name="readerHandle">读卡器句柄。</param>
        /// <param name="slot">卡槽号。</param>
        /// <param name="command">APDU 请求缓冲区。</param>
        /// <param name="commandLength">请求长度。</param>
        /// <param name="response">响应缓冲区。</param>
        /// <param name="responseLength">输入容量并由 DLL 写回有效长度。</param>
        /// <returns>原 DLL 返回码。</returns>
        [DllImport(CardReaderLibrary, EntryPoint = "mw_cpu_apdu", CallingConvention = CallingConvention.StdCall)]
        internal static extern int CpuApdu(IntPtr readerHandle, uint slot, byte[] command, uint commandLength, byte[] response, ref int responseLength);

        /// <summary>调用 gmssl.dll 的版本字符串入口，用于启动时验证 DLL 确实可加载。</summary>
        /// <returns>ANSI 版本字符串指针；调用方不得释放。</returns>
        [DllImport(GmsslLibrary, EntryPoint = "gmssl_version_str", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GmsslVersionString();

        /// <summary>加载指定原生 DLL，供适配层做不修改设备状态的依赖验证。</summary>
        /// <param name="libraryName">DLL 文件名或绝对路径。</param>
        /// <returns>非零模块句柄表示加载成功。</returns>
        [DllImport(Kernel32Library, EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string libraryName);

        /// <summary>释放由 LoadLibrary 返回且不再使用的模块句柄。</summary>
        /// <param name="module">模块句柄。</param>
        /// <returns>非零表示释放成功。</returns>
        [DllImport(Kernel32Library, EntryPoint = "FreeLibrary", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr module);
    }
}
