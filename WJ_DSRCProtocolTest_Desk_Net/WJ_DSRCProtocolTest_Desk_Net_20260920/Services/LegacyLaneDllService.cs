using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using WJ_DSRCProtocolTest_Desk_Net.Native;

namespace WJ_DSRCProtocolTest_Desk_Net.Services
{
    /// <summary>
    /// 对车道交易使用的三个用户 DLL 做依赖验证，并封装原上位机使用的 3DES/MAC 算法。
    /// </summary>
    internal sealed class LegacyLaneDllService
    {
        /// <summary>
        /// 验证三个 DLL 能在当前 x86 进程中加载，并读取 gmssl 版本字符串。
        /// </summary>
        /// <returns>验证结果和可诊断的失败原因。</returns>
        internal LegacyDllAvailability VerifyAvailability()
        {
            IntPtr cardReader = IntPtr.Zero;
            IntPtr readerDriver = IntPtr.Zero;
            IntPtr gmssl = IntPtr.Zero;
            try
            {
                // 调用入口只做 LoadLibrary，不打开读卡器、不访问 USB、不改变设备状态。
                cardReader = LegacyLaneNativeMethods.LoadLibrary(GetDllPath("mwCardReader.dll"));
                readerDriver = LegacyLaneNativeMethods.LoadLibrary(GetDllPath("wdcrwv.dll"));
                gmssl = LegacyLaneNativeMethods.LoadLibrary(GetDllPath("gmssl.dll"));
                if (cardReader == IntPtr.Zero || readerDriver == IntPtr.Zero || gmssl == IntPtr.Zero)
                {
                    return new LegacyDllAvailability(false, "mwCardReader.dll=" + FormatModule(cardReader)
                        + ", wdcrwv.dll=" + FormatModule(readerDriver)
                        + ", gmssl.dll=" + FormatModule(gmssl)
                        + "；请确认三个 DLL 与 exe 同级且为 x86 版本。", string.Empty);
                }

                string version = string.Empty;
                try
                {
                    IntPtr versionPointer = LegacyLaneNativeMethods.GmsslVersionString();
                    if (versionPointer != IntPtr.Zero)
                    {
                        version = Marshal.PtrToStringAnsi(versionPointer) ?? string.Empty;
                    }
                }
                catch (EntryPointNotFoundException exception)
                {
                    return new LegacyDllAvailability(false, "gmssl.dll 已加载，但 gmssl_version_str 导出不可用：" + exception.Message, string.Empty);
                }

                return new LegacyDllAvailability(true, "三个车道交易 DLL 已加载。", version);
            }
            catch (Win32Exception exception)
            {
                return new LegacyDllAvailability(false, "加载车道交易 DLL 失败：" + exception.Message, string.Empty);
            }
            catch (DllNotFoundException exception)
            {
                return new LegacyDllAvailability(false, "车道交易 DLL 不可用：" + exception.Message, string.Empty);
            }
            finally
            {
                if (gmssl != IntPtr.Zero) LegacyLaneNativeMethods.FreeLibrary(gmssl);
                if (readerDriver != IntPtr.Zero) LegacyLaneNativeMethods.FreeLibrary(readerDriver);
                if (cardReader != IntPtr.Zero) LegacyLaneNativeMethods.FreeLibrary(cardReader);
            }
        }

        /// <summary>
        /// 使用原 wdcrwv.dll 计算 16 字节双倍 3DES 派生密钥。
        /// </summary>
        /// <param name="rootKey">16 字节根密钥。</param>
        /// <param name="diversification">8 字节分散因子。</param>
        /// <returns>16 字节派生密钥。</returns>
        /// <exception cref="InvalidOperationException">DLL 返回非零错误码时抛出。</exception>
        internal byte[] DeriveTripleDesKey(byte[] rootKey, byte[] diversification)
        {
            if (rootKey == null || rootKey.Length != 16) throw new ArgumentException("根密钥必须为 16 字节。", nameof(rootKey));
            if (diversification == null || diversification.Length != 8) throw new ArgumentException("分散因子必须为 8 字节。", nameof(diversification));
            byte[] result = new byte[16];
            byte[] block = new byte[8];
            byte[] complement = (byte[])diversification.Clone();
            int first = LegacyLaneNativeMethods.TripleDes(1, 8, diversification, rootKey, block);
            if (first != 0) throw new InvalidOperationException("TripleDES 第一次派生失败，返回码 " + first + "。");
            Buffer.BlockCopy(block, 0, result, 0, 8);
            for (int index = 0; index < complement.Length; index++) complement[index] = (byte)~complement[index];
            int second = LegacyLaneNativeMethods.TripleDes(1, 8, complement, rootKey, block);
            if (second != 0) throw new InvalidOperationException("TripleDES 第二次派生失败，返回码 " + second + "。");
            Buffer.BlockCopy(block, 0, result, 8, 8);
            return result;
        }

        /// <summary>使用原 wdcrwv.dll 对 8 字节块执行 3DES。</summary>
        /// <param name="mode">1 为加密，2 为解密。</param>
        /// <param name="input">长度为 8 的倍数的数据。</param>
        /// <param name="key">16 字节密钥。</param>
        /// <returns>与输入等长的结果数据。</returns>
        internal byte[] TripleDes(uint mode, byte[] input, byte[] key)
        {
            if (input == null || input.Length == 0 || input.Length % 8 != 0) throw new ArgumentException("3DES 输入长度必须为正的 8 字节倍数。", nameof(input));
            if (key == null || key.Length != 16) throw new ArgumentException("3DES 密钥必须为 16 字节。", nameof(key));
            byte[] output = new byte[input.Length];
            int result = LegacyLaneNativeMethods.TripleDes(mode, (uint)input.Length, input, key, output);
            if (result != 0) throw new InvalidOperationException("TripleDES 执行失败，返回码 " + result + "。");
            return output;
        }

        /// <summary>使用原 wdcrwv.dll 计算单 DES MAC。</summary>
        /// <param name="key">8 字节 MAC 密钥。</param>
        /// <param name="initialization">8 字节初始向量。</param>
        /// <param name="input">待计算数据。</param>
        /// <returns>至少 4 字节的 MAC 结果。</returns>
        internal byte[] SingleMac(byte[] key, byte[] initialization, byte[] input)
        {
            if (key == null || key.Length != 8) throw new ArgumentException("MAC 密钥必须为 8 字节。", nameof(key));
            if (initialization == null || initialization.Length != 8) throw new ArgumentException("MAC 初始向量必须为 8 字节。", nameof(initialization));
            if (input == null || input.Length == 0) throw new ArgumentException("MAC 输入不能为空。", nameof(input));
            byte[] mac = new byte[8];
            int result = LegacyLaneNativeMethods.SingleMac(key, initialization, (uint)input.Length, input, mac);
            if (result != 0) throw new InvalidOperationException("SingleMAC 执行失败，返回码 " + result + "。");
            return mac;
        }

        /// <summary>格式化 LoadLibrary 结果。</summary>
        /// <param name="module">模块句柄。</param>
        /// <returns>成功或失败文本。</returns>
        private static string FormatModule(IntPtr module)
        {
            return module == IntPtr.Zero ? "失败" : "成功";
        }

        /// <summary>按程序目录生成 DLL 的绝对路径，避免工作目录变化导致误判 DLL 缺失。</summary>
        /// <param name="fileName">DLL 文件名。</param>
        /// <returns>程序目录下的绝对路径。</returns>
        private static string GetDllPath(string fileName)
        {
            string assemblyDirectory = System.IO.Path.GetDirectoryName(typeof(LegacyLaneDllService).Assembly.Location);
            if (string.IsNullOrWhiteSpace(assemblyDirectory))
            {
                assemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            return System.IO.Path.Combine(assemblyDirectory, fileName);
        }
    }

    /// <summary>三个用户 DLL 的加载验证结果。</summary>
    internal sealed class LegacyDllAvailability
    {
        /// <summary>创建 DLL 验证结果。</summary>
        /// <param name="isAvailable">三个依赖是否均可用。</param>
        /// <param name="message">诊断文本。</param>
        /// <param name="gmsslVersion">gmssl 版本文本。</param>
        internal LegacyDllAvailability(bool isAvailable, string message, string gmsslVersion)
        {
            IsAvailable = isAvailable;
            Message = message ?? string.Empty;
            GmsslVersion = gmsslVersion ?? string.Empty;
        }

        internal bool IsAvailable { get; }
        internal string Message { get; }
        internal string GmsslVersion { get; }
    }
}
