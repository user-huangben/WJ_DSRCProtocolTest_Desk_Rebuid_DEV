using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using WJ_ComprehensiveTest_Desk.Native;

namespace WJ_ComprehensiveTest_Desk.Services
{
    internal sealed class DesktopCommService : IDisposable
    {
        private const int Success = 0;
        private const int TransparentBufferSize = 1024;
        private int _handle;
        private int _beaconId;
        private byte[] _obuMac;
        private DesktopInitializationOptions _initializationOptions;

        internal bool IsOpen => _handle > 0;
        internal bool IsInitialized { get; private set; }

        /// <summary>
        /// 通过 x86 stdcall DLL 打开指定串口对应的台发通信句柄，并清除旧初始化状态。
        /// </summary>
        /// <param name="portName">系统已识别的串口名称，例如 COM1；不能为空。</param>
        /// <returns>大于 0 表示有效 DLL 句柄，小于或等于 0 表示 DLL 打开失败返回码。</returns>
        /// <exception cref="InvalidOperationException">已有有效通信句柄时抛出。</exception>
        internal int Open(string portName)
        {
            if (IsOpen)
            {
                throw new InvalidOperationException("通信端口已经打开。");
            }

            // 打开入口使用原上位机串口模式 0，由服务层接管成功返回的句柄生命周期。
            int result = DesktopCommNativeMethods.Open(0, portName, 0);
            if (result > Success)
            {
                _handle = result;
                IsInitialized = false;
            }

            return result;
        }

        /// <summary>保留当前台发初始化参数，仅修改物理信道并重新初始化设备。</summary>
        /// <param name="physicalChannelId">目标物理信道；原 OBU 信道选择用例使用 0 或 1。</param>
        /// <returns>台发重新初始化结果。</returns>
        /// <exception cref="InvalidOperationException">台发尚未初始化时抛出。</exception>
        internal DesktopInitializationResult ReinitializePhysicalChannel(int physicalChannelId)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法切换物理信道。");
            }

            // 信道测试只覆盖物理信道字段，其余参数完整复制当前会话。
            DesktopInitializationOptions options = new DesktopInitializationOptions
            {
                BstInterval = _initializationOptions.BstInterval,
                RetryInterval = _initializationOptions.RetryInterval,
                RetryTimes = _initializationOptions.RetryTimes,
                Timeout = _initializationOptions.Timeout,
                TxPower = _initializationOptions.TxPower,
                PhysicalChannelId = physicalChannelId,
                BidChange = _initializationOptions.BidChange,
                UnixTimeChange = _initializationOptions.UnixTimeChange,
                LlcChange = _initializationOptions.LlcChange,
                MacChange = _initializationOptions.MacChange
            };
            return Initialize(options);
        }

        /// <summary>获取当前成功初始化时使用的物理信道。</summary>
        internal int CurrentPhysicalChannelId => _initializationOptions == null ? 0 : _initializationOptions.PhysicalChannelId;

        /// <summary>保留当前台发其他初始化参数，修改 BID 和 UnixTime 自动更新开关并重新初始化。</summary>
        /// <param name="bidChangeEnabled">true 表示由台发自动更新 BID；255S 保持测试必须传 false。</param>
        /// <param name="unixTimeChangeEnabled">true 表示由台发自动更新 UnixTime；255S 保持测试必须传 false，以原样发送配置中的固定时间序列。</param>
        /// <returns>台发重新初始化的请求、响应和设备状态。</returns>
        /// <exception cref="InvalidOperationException">台发尚未初始化，无法取得现有参数时抛出。</exception>
        internal DesktopInitializationResult ReinitializeBidAndUnixTimeChange(bool bidChangeEnabled, bool unixTimeChangeEnabled)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法修改 BID 更新配置。");
            }

            // 255S 专用入口只覆盖 BID 与 UnixTime 更新开关，避免改变功率、信道、LLC 和 MAC 设置。
            DesktopInitializationOptions options = new DesktopInitializationOptions
            {
                BstInterval = _initializationOptions.BstInterval,
                RetryInterval = _initializationOptions.RetryInterval,
                RetryTimes = _initializationOptions.RetryTimes,
                Timeout = _initializationOptions.Timeout,
                TxPower = _initializationOptions.TxPower,
                PhysicalChannelId = _initializationOptions.PhysicalChannelId,
                BidChange = bidChangeEnabled,
                UnixTimeChange = unixTimeChangeEnabled,
                LlcChange = _initializationOptions.LlcChange,
                MacChange = _initializationOptions.MacChange
            };
            return Initialize(options);
        }

        /// <summary>获取当前成功初始化配置中的 BID 自动更新状态。</summary>
        internal bool CurrentBidChange => _initializationOptions != null && _initializationOptions.BidChange;

        /// <summary>获取当前成功初始化配置中的 MAC 自动更新状态。</summary>
        internal bool CurrentMacChange => _initializationOptions != null && _initializationOptions.MacChange;

        /// <summary>获取当前成功初始化配置中的 LLC 自动更新状态。</summary>
        internal bool CurrentLlcChange => _initializationOptions != null && _initializationOptions.LlcChange;

        /// <summary>
        /// 保留当前台发其他初始化参数，仅修改 LLC 自动更新开关并重新初始化。
        /// </summary>
        /// <param name="llcChangeEnabled">true 表示 DLL 自动更新 LLC；透明测试显式组帧期间传 false。</param>
        /// <returns>台发重新初始化的请求、响应和设备状态。</returns>
        /// <exception cref="InvalidOperationException">台发尚未初始化，无法取得现有参数时抛出。</exception>
        internal DesktopInitializationResult ReinitializeLlcChange(bool llcChangeEnabled)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法修改 LLC 更新配置。");
            }

            DesktopInitializationOptions options = new DesktopInitializationOptions
            {
                BstInterval = _initializationOptions.BstInterval,
                RetryInterval = _initializationOptions.RetryInterval,
                RetryTimes = _initializationOptions.RetryTimes,
                Timeout = _initializationOptions.Timeout,
                TxPower = _initializationOptions.TxPower,
                PhysicalChannelId = _initializationOptions.PhysicalChannelId,
                BidChange = _initializationOptions.BidChange,
                UnixTimeChange = _initializationOptions.UnixTimeChange,
                LlcChange = llcChangeEnabled,
                MacChange = _initializationOptions.MacChange
            };
            // 透明测试显式生成每帧 LLC 后关闭 DLL 二次修改，保证列表、结果日志和实际调用缓冲区一致。
            return Initialize(options);
        }

        /// <summary>
        /// 保留当前台发其他初始化参数，仅修改 MAC 自动更新开关并重新初始化。
        /// </summary>
        /// <param name="macChangeEnabled">true 表示 DLL 根据 VST 自动更新后续帧 MAC；错误 MAC 负向测试必须传 false。</param>
        /// <returns>台发重新初始化的请求、响应和设备状态。</returns>
        /// <exception cref="InvalidOperationException">台发尚未初始化，无法取得现有参数时抛出。</exception>
        internal DesktopInitializationResult ReinitializeMacChange(bool macChangeEnabled)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法修改 MAC 更新配置。");
            }

            DesktopInitializationOptions options = new DesktopInitializationOptions
            {
                BstInterval = _initializationOptions.BstInterval,
                RetryInterval = _initializationOptions.RetryInterval,
                RetryTimes = _initializationOptions.RetryTimes,
                Timeout = _initializationOptions.Timeout,
                TxPower = _initializationOptions.TxPower,
                PhysicalChannelId = _initializationOptions.PhysicalChannelId,
                BidChange = _initializationOptions.BidChange,
                UnixTimeChange = _initializationOptions.UnixTimeChange,
                LlcChange = _initializationOptions.LlcChange,
                MacChange = macChangeEnabled
            };
            // 错误 MAC 用例必须让 DLL 保留调用方传入的地址，不能在发射前再次替换成 VST MAC。
            return Initialize(options);
        }

        /// <summary>
        /// 按旧版 WJ_Trade_RespondDiffMac 的固定参数重置台发，并返回调用前的完整配置以供流程结束后恢复。
        /// </summary>
        /// <param name="originalOptions">输出调用前已成功初始化的完整配置；调用方必须在测试结束后传给 <see cref="RestoreInitializationOptions"/>。</param>
        /// <returns>固定为 BST=30、重发间隔/次数=10、功率=10、信道=0、LLC=开、UnixTime=关、MAC=关、超时=500ms，且保留原BID更新状态的初始化结果。</returns>
        /// <exception cref="InvalidOperationException">台发尚未初始化，无法保存原配置或进入不同MAC测试模式时抛出。</exception>
        /// <remarks>通过 x86 stdcall 的 fDeskTop_INIT_rq/fDeskTop_INIT_rs 完成；DLL 句柄仍由服务持有，返回0表示调用成功。</remarks>
        internal DesktopInitializationResult BeginDifferentMacTestMode(out DesktopInitializationOptions originalOptions)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法进入不同MAC测试模式。");
            }

            originalOptions = CloneInitializationOptions(_initializationOptions);
            // 旧 WJ_Trade_RespondDiffMac 固定数组：{30,10,10,10,0,0,1,0,0,0,0,500}。
            DesktopInitializationOptions legacyOptions = new DesktopInitializationOptions
            {
                BstInterval = 30,
                RetryInterval = 10,
                RetryTimes = 10,
                Timeout = 500,
                TxPower = 10,
                PhysicalChannelId = 0,
                // 旧函数由全局 gBIDChange 决定是否递增 gBeaconID，专用初始化不能覆盖该全局开关。
                BidChange = originalOptions.BidChange,
                LlcChange = true,
                UnixTimeChange = false,
                MacChange = false
            };
            return Initialize(legacyOptions);
        }

        /// <summary>
        /// 按旧版 <c>WJ_ProtocolTest_SingleFrame</c> 的固定台发参数进入单帧唤醒测试模式，并保存调用前配置。
        /// </summary>
        /// <param name="originalOptions">输出调用前已成功初始化的完整配置；测试结束后必须传给 <see cref="RestoreInitializationOptions"/>。</param>
        /// <returns>固定为 BST=90、重发间隔=20、重发次数=32、功率=10、物理信道=0、超时=80ms 的初始化结果。</returns>
        /// <exception cref="InvalidOperationException">台发尚未完成初始化，无法保存原配置或进入单帧唤醒测试模式时抛出。</exception>
        /// <remarks>旧用例保留 BID、LLC、UnixTime 和 MAC 自动更新开关；仅覆盖其明确写死的台发参数。DLL 使用 x86 stdcall，返回0表示成功。</remarks>
        internal DesktopInitializationResult BeginSingleFrameWakeTestMode(out DesktopInitializationOptions originalOptions)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法进入单帧唤醒测试模式。");
            }

            originalOptions = CloneInitializationOptions(_initializationOptions);
            DesktopInitializationOptions legacyOptions = new DesktopInitializationOptions
            {
                BstInterval = 90,
                RetryInterval = 20,
                RetryTimes = 32,
                Timeout = 80,
                TxPower = 10,
                PhysicalChannelId = 0,
                BidChange = originalOptions.BidChange,
                LlcChange = originalOptions.LlcChange,
                UnixTimeChange = originalOptions.UnixTimeChange,
                MacChange = originalOptions.MacChange
            };
            return Initialize(legacyOptions);
        }

        /// <summary>
        /// 按旧版 <c>WJ_TransactionFlow_ruansuan</c> 的参数进入模拟真实交易流专用台发模式。
        /// </summary>
        /// <param name="originalOptions">输出测试前完整配置；调用方结束后必须恢复。</param>
        /// <returns>BST=30、功率=10、物理信道=0、BID更新开启、UnixTime更新关闭、MAC更新开启的初始化结果。</returns>
        /// <exception cref="InvalidOperationException">台发未初始化时抛出。</exception>
        /// <remarks>旧函数沿用当前重发间隔、重发次数、超时和 LLC 开关；这些字段不应被本专用模式重构。</remarks>
        internal DesktopInitializationResult BeginSimulatedRealTradeTestMode(out DesktopInitializationOptions originalOptions)
        {
            if (!IsInitialized || _initializationOptions == null)
            {
                throw new InvalidOperationException("台发尚未完成初始化，无法进入模拟真实交易流测试模式。");
            }

            originalOptions = CloneInitializationOptions(_initializationOptions);
            DesktopInitializationOptions legacyOptions = new DesktopInitializationOptions
            {
                BstInterval = 30,
                RetryInterval = originalOptions.RetryInterval,
                RetryTimes = originalOptions.RetryTimes,
                Timeout = originalOptions.Timeout,
                TxPower = 10,
                PhysicalChannelId = 0,
                BidChange = true,
                LlcChange = originalOptions.LlcChange,
                UnixTimeChange = false,
                MacChange = true
            };
            return Initialize(legacyOptions);
        }

        /// <summary>
        /// 执行旧版单帧唤醒的逐轮 BST/VST 流程，并以所有配置轮次均收到有效 VST 作为最终通过条件。
        /// </summary>
        /// <param name="timeout">每轮 BST 请求和 VST 响应的 DLL 超时时间，单位为毫秒，必须大于0。</param>
        /// <param name="count">从 SetMe.ini 的 SET_DANZHEN/number 读取的测试轮数，必须大于0。</param>
        /// <param name="cancellationToken">测试窗口统一取消令牌；每轮及轮间等待前检查。</param>
        /// <returns>Version 字段为“成功轮数/总轮数”；仅成功轮数等于总轮数时 IsSuccess 为 true。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">超时或轮数不合法时抛出。</exception>
        /// <exception cref="OperationCanceledException">测试在轮次之间被停止时抛出。</exception>
        /// <remarks>BST 请求失败按旧逻辑立即停止；VST 接收失败会保留原始 DLL 返回码并继续后续轮次。每轮间隔固定2000ms。</remarks>
        internal ObuVersionReadResult ExecuteSingleFrameWakeTest(int timeout, int count, CancellationToken cancellationToken)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。");
            }
            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "单帧唤醒超时必须大于0毫秒。");
            }
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "单帧唤醒测试次数必须大于0。");
            }

            byte[] bstCommand =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02, 0x00, 0x01,
                0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            int successCount = 0;

            for (int cycle = 0; cycle < count; cycle++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // ExecuteTransparent 保持旧 SendBST_GB/RecvVst_GB 的请求、响应分离返回码，且会按本用例初始化开关更新动态 BST 字段。
                TransparentCommandResult result = ExecuteTransparent((byte[])bstCommand.Clone(), timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(result));
                if (result.RequestResult != Success)
                {
                    return new ObuVersionReadResult(
                        false,
                        string.Format("{0}/{1}", successCount, count),
                        string.Format("第{0}/{1}轮 BST 发送失败：{2}。", cycle + 1, count, result.ErrorMessage),
                        exchanges);
                }

                if (result.ResponseResult == Success)
                {
                    successCount++;
                }

                if (cancellationToken.WaitHandle.WaitOne(2000))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            bool success = successCount == count;
            return new ObuVersionReadResult(
                success,
                string.Format("{0}/{1}", successCount, count),
                success
                    ? string.Format("单帧唤醒测试通过：有效 VST {0}/{1}。", successCount, count)
                    : string.Format("单帧唤醒测试失败：有效 VST {0}/{1}。", successCount, count),
                exchanges);
        }

        /// <summary>
        /// 恢复调用方在专用测试前保存的完整台发初始化配置。
        /// </summary>
        /// <param name="options">由 <see cref="BeginDifferentMacTestMode"/> 输出且未被调用方修改的原始配置。</param>
        /// <returns>恢复配置后的 DLL 初始化结果。</returns>
        /// <exception cref="ArgumentNullException">配置为空时抛出。</exception>
        /// <remarks>通过 x86 stdcall 的初始化请求/响应恢复状态；仅应在同一有效台发句柄上调用。</remarks>
        internal DesktopInitializationResult RestoreInitializationOptions(DesktopInitializationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return Initialize(CloneInitializationOptions(options));
        }

        /// <summary>获取当前成功初始化配置中的 UnixTime 自动更新状态。</summary>
        internal bool CurrentUnixTimeChange => _initializationOptions != null && _initializationOptions.UnixTimeChange;

        /// <summary>
        /// 关闭当前台发 DLL 句柄，并在成功时清除初始化状态和动态 BST 配置。
        /// </summary>
        /// <returns>无句柄或 DLL 成功关闭时返回 0，其他值为 DLL 错误码。</returns>
        internal int Close()
        {
            if (!IsOpen)
            {
                return Success;
            }

            // 关闭入口把当前有效句柄交给 DLL，只有 DLL 确认成功后才清除本地状态。
            int result = DesktopCommNativeMethods.Close(_handle);
            if (result == Success)
            {
                _handle = 0;
                IsInitialized = false;
                _initializationOptions = null;
            }

            return result;
        }

        internal int SetLogEnabled(bool enabled)
        {
            return DesktopCommNativeMethods.SetLogEnabled(enabled ? 1 : 0);
        }

        internal static bool IsLogSettingSuccessful(int result)
        {
            // DeskTopComm.dll uses a BOOL-style return value for fOpenDLLLog.
            return result == 1;
        }

        /// <summary>
        /// 使用界面参数发送台发初始化请求并等待响应，成功后允许透传测试使用当前句柄。
        /// </summary>
        /// <param name="options">已完成非负数校验的台发初始化参数和动态字段开关。</param>
        /// <returns>包含请求返回码、响应返回码、设备状态和原始设备信息的初始化结果。</returns>
        /// <exception cref="InvalidOperationException">通信端口尚未打开时抛出。</exception>
        internal DesktopInitializationResult Initialize(DesktopInitializationOptions options)
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("请先打开通信端口。");
            }

            // 初始化请求入口需要当前网络序 UnixTime，并把全部台发参数一次性交给 DLL。
            byte[] unixTime = GetUnixTimeBytes();
            int requestResult = DesktopCommNativeMethods.InitializeRequest(
                _handle,
                unixTime,
                options.BstInterval,
                options.RetryInterval,
                options.RetryTimes,
                options.TxPower,
                options.PhysicalChannelId,
                options.BidChange ? 1 : 0,
                options.LlcChange ? 1 : 0,
                options.UnixTimeChange ? 1 : 0,
                options.MacChange ? 1 : 0,
                0,
                0,
                options.Timeout);

            if (requestResult != Success)
            {
                return new DesktopInitializationResult(requestResult, null, 0, null);
            }

            int desktopStatus = 0;
            byte[] desktopInfo = new byte[100];
            // 仅在请求成功后进入响应等待，防止无效请求继续占用超时时间。
            int responseResult = DesktopCommNativeMethods.InitializeResponse(
                _handle,
                ref desktopStatus,
                desktopInfo,
                options.Timeout);

            DesktopInitializationResult result = new DesktopInitializationResult(requestResult, responseResult, desktopStatus, desktopInfo);
            if (result.IsSuccess)
            {
                IsInitialized = true;
                _initializationOptions = options;
            }

            return result;
        }

        /// <summary>
        /// 按原上位机两字节本地类型标识执行一条透传命令，并在需要时接收对应响应。
        /// </summary>
        /// <param name="commandFrame">完整命令；前两字节必须为 00 和 01～07，后续字节才传给 DLL。</param>
        /// <param name="timeout">每次 DLL 请求或响应调用的超时时间，单位为毫秒，必须大于 0。</param>
        /// <param name="applySessionMac">是否由当前用例把 0002～0005 帧的前四字节显式替换为当前会话 OBU MAC；不受台发 MAC 自动更新开关影响。</param>
        /// <param name="sessionMacVariant">会话 MAC 变体：0 不变，1 末字节加一，2 全 FF，3 第三字节加一；仅在 applySessionMac 为 true 时生效。</param>
        /// <returns>包含命令名称、请求/响应返回码、响应数据和错误说明的执行结果。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="ArgumentException">命令为空、过长或本地类型标识无效时抛出。</exception>
        /// <remarks>DLL 句柄和所有缓冲区均由本服务持有；调用约定为 x86 stdcall，返回 0 表示成功。</remarks>
        internal TransparentCommandResult ExecuteTransparent(byte[] commandFrame, int timeout, bool applySessionMac = true, int sessionMacVariant = 0, bool applyBstDynamicFields = true)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。 ");
            }

            if (commandFrame == null || commandFrame.Length < 3 || commandFrame.Length > TransparentBufferSize)
            {
                throw new ArgumentException("透传帧长度必须为 3～1024 字节。", nameof(commandFrame));
            }

            if (commandFrame[0] != 0x00 || commandFrame[1] < 0x01 || commandFrame[1] > 0x07)
            {
                throw new ArgumentException("透传帧类型必须为 0001～0007。", nameof(commandFrame));
            }

            byte commandType = commandFrame[1];
            byte[] payload = new byte[commandFrame.Length - 2];
            Buffer.BlockCopy(commandFrame, 2, payload, 0, payload.Length);
            if (commandType == 0x07 && payload.Length > byte.MaxValue)
            {
                throw new ArgumentException("ICC/CPC 透传数据不能超过 255 字节。", nameof(commandFrame));
            }
            if (commandType == 0x01)
            {
                _obuMac = null;
                // 模拟真实交易流的断链复发 BST 必须保持刚才的 BID；其他调用仍按初始化选项更新动态字段。
                if (applyBstDynamicFields)
                {
                    ApplyBstDynamicFields(payload);
                }
            }
            else if (applySessionMac && commandType >= 0x02 && commandType <= 0x05 && payload.Length >= 4
                && _obuMac != null)
            {
                // 用例显式要求会话寻址时始终写入VST返回的MAC；台发MAC自动更新开关不应屏蔽测试用例的手工变体。
                Buffer.BlockCopy(_obuMac, 0, payload, 0, 4);
                if (sessionMacVariant == 1)
                {
                    payload[3]++;
                }
                else if (sessionMacVariant == 2)
                {
                    payload[0] = payload[1] = payload[2] = payload[3] = 0xFF;
                }
                else if (sessionMacVariant == 3)
                {
                    payload[2]++;
                }
            }

            byte[] responseBuffer = new byte[TransparentBufferSize];
            int responseLength = responseBuffer.Length;
            int requestResult;
            int? responseResult = null;

            switch (commandType)
            {
                case 0x01:
                    // 0001 入口先发 BST，成功后等待 VST。
                    requestResult = DesktopCommNativeMethods.SendBst(_handle, payload, payload.Length, timeout);
                    if (requestResult == Success)
                    {
                        responseResult = DesktopCommNativeMethods.ReceiveVst(_handle, responseBuffer, ref responseLength, timeout);
                    }
                    break;
                case 0x02:
                    // 0002 入口执行 GetSecure 请求/响应对。
                    requestResult = DesktopCommNativeMethods.SendGetSecure(_handle, payload, payload.Length, timeout);
                    if (requestResult == Success)
                    {
                        responseResult = DesktopCommNativeMethods.ReceiveGetSecure(_handle, responseBuffer, ref responseLength, timeout);
                    }
                    break;
                case 0x03:
                    // 0003 入口执行 TransferChannel 请求/响应对。
                    requestResult = DesktopCommNativeMethods.SendTransferChannel(_handle, payload, payload.Length, timeout);
                    if (requestResult == Success)
                    {
                        responseResult = DesktopCommNativeMethods.ReceiveTransferChannel(_handle, responseBuffer, ref responseLength, timeout);
                    }
                    break;
                case 0x04:
                    // 0004 入口执行 SetMMI 请求/响应对。
                    requestResult = DesktopCommNativeMethods.SendSetMmi(_handle, payload, payload.Length, timeout);
                    if (requestResult == Success)
                    {
                        responseResult = DesktopCommNativeMethods.ReceiveSetMmi(_handle, responseBuffer, ref responseLength, timeout);
                    }
                    break;
                case 0x05:
                    // 0005 是单向 EventReport，原流程不等待响应。
                    requestResult = DesktopCommNativeMethods.SendEventReport(_handle, payload, payload.Length, timeout);
                    responseLength = 0;
                    break;
                case 0x06:
                    // 0006 入口执行其他帧请求/响应对。
                    requestResult = DesktopCommNativeMethods.SendOtherFrame(_handle, payload, payload.Length, timeout);
                    if (requestResult == Success)
                    {
                        responseResult = DesktopCommNativeMethods.ReceiveOtherFrame(_handle, responseBuffer, ref responseLength, timeout);
                    }
                    break;
                default:
                    // 0007 入口沿用原透传页的卡槽 1 和长度前缀格式执行 IC/CPC APDU。
                    byte[] iccRequest = new byte[payload.Length + 1];
                    iccRequest[0] = checked((byte)payload.Length);
                    Buffer.BlockCopy(payload, 0, iccRequest, 1, payload.Length);
                    requestResult = DesktopCommNativeMethods.SendIccChannel(_handle, 1, iccRequest, timeout);
                    if (requestResult == Success)
                    {
                        int apduList = 0;
                        responseResult = DesktopCommNativeMethods.ReceiveIccChannel(_handle, ref apduList, responseBuffer, timeout);
                        responseLength = Math.Min(responseBuffer.Length, responseBuffer[0] + 3);
                    }
                    break;
            }

            byte[] responseData = responseResult == Success
                ? CopyResponse(responseBuffer, responseLength)
                : new byte[0];
            if (commandType == 0x01 && responseData.Length >= 4)
            {
                _obuMac = new byte[4];
                // 保存成功 VST 的地址，后续请求沿用该 OBU 会话。
                Buffer.BlockCopy(responseData, 0, _obuMac, 0, 4);
            }
            return new TransparentCommandResult(
                GetTransparentCommandName(commandType),
                requestResult,
                responseResult,
                responseData) { RequestData = (byte[])payload.Clone() };
        }

        /// <summary>
        /// 按原万集 OBU 标准测试流程读取 OBU 版本号：先完成 BST/VST，再执行 71 通道认证，最后读取版本号。
        /// </summary>
        /// <param name="timeout">每个 DLL 请求或响应调用的超时时间，单位为毫秒，必须大于 0。</param>
        /// <param name="cancellationToken">测试大框的统一取消令牌；在每次非托管 DLL 调用前检查。</param>
        /// <returns>包含成功状态、版本号或失败步骤说明的读取结果；不会掩盖 DLL 原始返回码。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="OperationCanceledException">停止按钮在协议步骤之间请求取消时抛出。</exception>
        internal ObuVersionReadResult ReadObuVersion(int timeout, CancellationToken cancellationToken)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。");
            }

            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "版本号读取超时必须大于 0 毫秒。");
            }

            byte[] bstCommand =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x01, 0x08, 0xFE, 0x01, 0x53, 0x0F, 0x20, 0x81, 0x00, 0x01,
                0x41, 0x83, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();

            cancellationToken.ThrowIfCancellationRequested();
            // 版本号读取必须先建立 BST/VST 会话，后续请求需要使用 VST 返回的 OBU MAC。
            TransparentCommandResult bstResult = ExecuteTransparent(bstCommand, timeout);
            // 保存 BST 上行和 VST 下行的完整数据，供测试结果文件回溯空中交互。
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
            if (!bstResult.IsSuccess || bstResult.ResponseData.Length < 4)
            {
                return new ObuVersionReadResult(false, string.Empty, "BST/VST 交互失败：" + bstResult.ErrorMessage, exchanges);
            }

            byte[] obuMac = new byte[4];
            Buffer.BlockCopy(bstResult.ResponseData, 0, obuMac, 0, obuMac.Length);

            // 认证第一步读取随机数和版本密文，第二步回传按原上位机规则变换后的密文。
            cancellationToken.ThrowIfCancellationRequested();
            ObuVersionReadResult verifyResult = VerifyVersionChannel(obuMac, timeout, exchanges, cancellationToken);
            if (!verifyResult.IsSuccess)
            {
                return verifyResult;
            }

            byte[] versionCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, new byte[] { 0x70, 0x01, 0x01, 0x72 });
            cancellationToken.ThrowIfCancellationRequested();
            // 认证成功后发送 70 01 01 72，响应的第 27～46 字节为 20 字节版本内容。
            TransparentCommandResult versionResult = ExecuteTransparent(versionCommand, timeout);
            // 保存版本读取请求和响应，结果文件需要包含最后一组完整交互帧。
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(versionResult));
            if (!versionResult.IsSuccess || !HasSuccessfulStatus(versionResult.ResponseData))
            {
                return new ObuVersionReadResult(false, string.Empty, "读取版本号响应失败：" + versionResult.ErrorMessage, exchanges);
            }

            if (versionResult.ResponseData.Length < 47)
            {
                return new ObuVersionReadResult(false, string.Empty, "读取版本号响应长度不足，无法提取 20 字节版本内容。", exchanges);
            }

            string version = Encoding.ASCII.GetString(versionResult.ResponseData, 27, 20).Trim('\0', ' ');
            return new ObuVersionReadResult(true, version, "OBU 版本号读取成功。", exchanges);
        }

        /// <summary>
        /// 按原 WJ_VstEquipmentclass_VersionTest 流程读取并显示 VST EquipmentStatus：建立会话、完成71通道认证、发送70通道DD读取命令，最后单向断链。
        /// </summary>
        /// <param name="timeout">每个 DLL 请求或响应调用的超时时间，单位为毫秒，必须大于0。</param>
        /// <param name="cancellationToken">测试窗口统一取消令牌；每次非托管调用前检查。</param>
        /// <returns>Version 字段承载十六进制 EquipmentStatus；成功要求 BST/VST、认证、DD读取和 EventReport 发送均成功。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">超时时间不大于0时抛出。</exception>
        /// <exception cref="OperationCanceledException">测试在协议步骤间被停止时抛出。</exception>
        /// <remarks>DLL 句柄和缓冲区由服务持有；x86 stdcall 返回0表示调用成功，EventReport只发送、不等待回复。</remarks>
        internal ObuVersionReadResult ReadEquipmentStatus(int timeout, CancellationToken cancellationToken)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。");
            }
            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "EquipmentStatus 读取超时必须大于0毫秒。");
            }

            byte[] bstCommand =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02, 0x00, 0x01,
                0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();

            cancellationToken.ThrowIfCancellationRequested();
            // 原用例先用 SendBST_GB 建立会话，EquipmentInfo 位于已解码 VST 的末尾状态区。
            TransparentCommandResult bstResult = ExecuteTransparent(bstCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
            if (!bstResult.IsSuccess || bstResult.ResponseData.Length < 7)
            {
                return new ObuVersionReadResult(false, string.Empty, "BST/VST 交互失败：" + bstResult.ErrorMessage, exchanges);
            }

            byte[] obuMac = new byte[4];
            Buffer.BlockCopy(bstResult.ResponseData, 0, obuMac, 0, obuMac.Length);
            byte equipmentInfo = bstResult.ResponseData[bstResult.ResponseData.Length - 3];

            cancellationToken.ThrowIfCancellationRequested();
            // DD读取之前必须复用原上位机 HandsetVerifyToken 对应的71通道两步认证。
            ObuVersionReadResult verifyResult = VerifyVersionChannel(obuMac, timeout, exchanges, cancellationToken);
            if (!verifyResult.IsSuccess)
            {
                return new ObuVersionReadResult(false, equipmentInfo.ToString("X2"), verifyResult.Message, exchanges);
            }

            byte[] readStatusCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, new byte[] { 0x70, 0x01, 0x01, 0xDD });
            cancellationToken.ThrowIfCancellationRequested();
            // 认证后发送旧程序固定的70 01 01 DD，保留完整响应供日志核对。
            TransparentCommandResult statusResult = ExecuteTransparent(readStatusCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(statusResult));
            if (!statusResult.IsSuccess || !HasSuccessfulStatus(statusResult.ResponseData))
            {
                return new ObuVersionReadResult(false, equipmentInfo.ToString("X2"), "70通道DD状态读取失败：" + statusResult.ErrorMessage, exchanges);
            }

            byte[] eventReport = { 0x00, 0x05, obuMac[0], obuMac[1], obuMac[2], obuMac[3], 0x03, 0x99, 0x60, 0x01, 0x00 };
            cancellationToken.ThrowIfCancellationRequested();
            // 状态读取完成后按原流程发送单向 EventReport 释放链路。
            TransparentCommandResult eventResult = ExecuteTransparent(eventReport, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(eventResult));
            return eventResult.IsSuccess
                ? new ObuVersionReadResult(true, equipmentInfo.ToString("X2"), "VST EquipmentStatus读取完成。", exchanges)
                : new ObuVersionReadResult(false, equipmentInfo.ToString("X2"), "EventReport发送失败：" + eventResult.ErrorMessage, exchanges);
        }

        /// <summary>
        /// 按原 WJ_MPoint_BLEMAC 流程读取 OBU 蓝牙 MAC：建立会话、完成71通道认证、发送70通道75命令、SetMMI并单向断链。
        /// </summary>
        /// <param name="timeout">每个 DLL 请求或响应调用的超时时间，单位为毫秒，必须大于0。</param>
        /// <param name="cancellationToken">测试窗口统一取消令牌；每次非托管调用前检查。</param>
        /// <returns>Version 字段承载6字节十六进制蓝牙地址；所有协议步骤成功时 IsSuccess 为 true。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">超时时间不大于0时抛出。</exception>
        /// <exception cref="OperationCanceledException">测试在协议步骤间被停止时抛出。</exception>
        /// <remarks>DLL 句柄和缓冲区由服务持有；x86 stdcall 返回0表示调用成功，EventReport只发送、不等待回复。</remarks>
        internal ObuVersionReadResult ReadBluetoothMac(int timeout, CancellationToken cancellationToken)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。");
            }
            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "蓝牙地址读取超时必须大于0毫秒。");
            }

            byte[] bstCommand =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02, 0x00, 0x01,
                0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            cancellationToken.ThrowIfCancellationRequested();
            // 原蓝牙地址用例使用 SendBST_GB 建链，并以本轮 VST MAC 作为后续会话地址。
            TransparentCommandResult bstResult = ExecuteTransparent(bstCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
            if (!bstResult.IsSuccess || bstResult.ResponseData.Length < 4)
            {
                return new ObuVersionReadResult(false, string.Empty, "BST/VST交互失败：" + bstResult.ErrorMessage, exchanges);
            }

            byte[] obuMac = new byte[4];
            Buffer.BlockCopy(bstResult.ResponseData, 0, obuMac, 0, obuMac.Length);
            cancellationToken.ThrowIfCancellationRequested();
            // 70通道私有读取前必须完成 HandsetVerifyToken 对应的71通道两步认证。
            ObuVersionReadResult verifyResult = VerifyVersionChannel(obuMac, timeout, exchanges, cancellationToken);
            if (!verifyResult.IsSuccess)
            {
                return new ObuVersionReadResult(false, string.Empty, verifyResult.Message, exchanges);
            }

            byte[] readCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, new byte[] { 0x70, 0x01, 0x01, 0x75 });
            cancellationToken.ThrowIfCancellationRequested();
            // 发送原固定75命令，旧程序从完整响应偏移14提取6字节蓝牙MAC。
            TransparentCommandResult readResult = ExecuteTransparent(readCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(readResult));
            if (!readResult.IsSuccess || !HasSuccessfulStatus(readResult.ResponseData) || readResult.ResponseData.Length < 20)
            {
                return new ObuVersionReadResult(false, string.Empty, "蓝牙地址读取响应失败：" + readResult.ErrorMessage, exchanges);
            }
            string bluetoothMac = BitConverter.ToString(readResult.ResponseData, 14, 6).Replace("-", " ");

            byte[] setMmiCommand = { 0x00, 0x04, obuMac[0], obuMac[1], obuMac[2], obuMac[3], 0x40, 0x77, 0x99, 0x05, 0x01, 0x04, 0x1A, 0x00 };
            cancellationToken.ThrowIfCancellationRequested();
            // 读取成功后按原 LLC 轮换发送 SetMMI。
            TransparentCommandResult setMmiResult = ExecuteTransparent(setMmiCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(setMmiResult));
            if (!setMmiResult.IsSuccess)
            {
                return new ObuVersionReadResult(false, bluetoothMac, "SetMMI发送失败：" + setMmiResult.ErrorMessage, exchanges);
            }

            byte[] eventReport = { 0x00, 0x05, obuMac[0], obuMac[1], obuMac[2], obuMac[3], 0x03, 0x99, 0x60, 0x01, 0x00 };
            cancellationToken.ThrowIfCancellationRequested();
            // 最后发送单向 EventReport 释放链路，不调用响应接收函数。
            TransparentCommandResult eventResult = ExecuteTransparent(eventReport, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(eventResult));
            return eventResult.IsSuccess
                ? new ObuVersionReadResult(true, bluetoothMac, "蓝牙地址读取成功。", exchanges)
                : new ObuVersionReadResult(false, bluetoothMac, "EventReport发送失败：" + eventResult.ErrorMessage, exchanges);
        }

        /// <summary>
        /// 按原 WJ_70_AmendMACID/WJ_70_AmendSN 流程重复修改 OBU MACID 或 SN，并在修改SN时执行D9复读校验。
        /// </summary>
        /// <param name="timeout">每个 DLL 请求或响应调用的超时时间，单位为毫秒，必须大于0。</param>
        /// <param name="count">从 SetMe.ini SET_AmendMACIDSN/number 读取的测试轮数，必须大于0。</param>
        /// <param name="modifySerialNumber">为true时执行8字节SN写入和复读；为false时执行4字节MACID写入。</param>
        /// <param name="cancellationToken">测试窗口统一取消令牌；每次非托管调用及轮次等待前检查。</param>
        /// <returns>Version 字段承载最后一次写入值；所有配置轮次均通过时 IsSuccess 为true。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">超时或测试轮数不合法时抛出。</exception>
        /// <exception cref="OperationCanceledException">测试在协议步骤间被停止时抛出。</exception>
        /// <remarks>DLL 句柄和缓冲区由服务持有；x86 stdcall 返回0表示调用成功。随机字节严格保持原程序0x00～0x59范围。</remarks>
        internal ObuVersionReadResult AmendMacIdOrSerialNumber(
            int timeout,
            int count,
            bool modifySerialNumber,
            CancellationToken cancellationToken)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。");
            }
            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "测试超时必须大于0毫秒。");
            }
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "修改MACID/SN测试次数必须大于0。");
            }

            byte[] bstCommand =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02, 0x00, 0x01,
                0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            Random random = new Random(unchecked(Environment.TickCount * 397 ^ DateTime.Now.Millisecond));
            string lastValue = string.Empty;

            for (int cycle = 0; cycle < count; cycle++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // 每轮修改前重新广播建链，MACID修改后的下一轮必须从新VST获取新会话地址。
                TransparentCommandResult bstResult = ExecuteTransparent((byte[])bstCommand.Clone(), timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
                if (!bstResult.IsSuccess || bstResult.ResponseData.Length < 4)
                {
                    return new ObuVersionReadResult(false, lastValue, string.Format("第{0}/{1}轮BST/VST失败：{2}", cycle + 1, count, bstResult.ErrorMessage), exchanges);
                }
                byte[] obuMac = new byte[4];
                Buffer.BlockCopy(bstResult.ResponseData, 0, obuMac, 0, obuMac.Length);

                cancellationToken.ThrowIfCancellationRequested();
                // 70通道写操作前逐轮完成旧 HandsetVerifyToken 对应的71通道认证。
                ObuVersionReadResult verifyResult = VerifyVersionChannel(obuMac, timeout, exchanges, cancellationToken);
                if (!verifyResult.IsSuccess)
                {
                    return new ObuVersionReadResult(false, lastValue, string.Format("第{0}/{1}轮认证失败：{2}", cycle + 1, count, verifyResult.Message), exchanges);
                }

                int valueLength = modifySerialNumber ? 8 : 4;
                byte[] randomValue = new byte[valueLength];
                for (int index = 0; index < randomValue.Length; index++)
                {
                    randomValue[index] = (byte)random.Next(0, 90);
                }
                lastValue = BitConverter.ToString(randomValue).Replace("-", " ");
                byte[] writeInstruction = new byte[4 + randomValue.Length];
                writeInstruction[0] = 0x70;
                writeInstruction[1] = 0x01;
                writeInstruction[2] = modifySerialNumber ? (byte)0x09 : (byte)0x05;
                writeInstruction[3] = modifySerialNumber ? (byte)0xD4 : (byte)0x71;
                Buffer.BlockCopy(randomValue, 0, writeInstruction, 4, randomValue.Length);
                byte[] writeCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, writeInstruction);
                // 使用本轮VST MAC发送随机写入值，成功状态必须为完整90 00 00。
                TransparentCommandResult writeResult = ExecuteTransparent(writeCommand, timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(writeResult));
                if (!writeResult.IsSuccess || !HasSuccessfulStatus(writeResult.ResponseData))
                {
                    return new ObuVersionReadResult(false, lastValue, string.Format("第{0}/{1}轮写入失败：{2}", cycle + 1, count, writeResult.ErrorMessage), exchanges);
                }

                byte nextLlc = 0x77;
                if (modifySerialNumber)
                {
                    byte[] readCommand = CreateTransferCommand(obuMac, nextLlc, 0x01, new byte[] { 0x70, 0x01, 0x01, 0xD9 });
                    // SN写入后按原流程立即D9复读，响应末尾状态前三的8字节必须等于写入值。
                    TransparentCommandResult readResult = ExecuteTransparent(readCommand, timeout);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(readResult));
                    if (!readResult.IsSuccess || !HasSuccessfulStatus(readResult.ResponseData) || readResult.ResponseData.Length < 11)
                    {
                        return new ObuVersionReadResult(false, lastValue, string.Format("第{0}/{1}轮SN复读失败：{2}", cycle + 1, count, readResult.ErrorMessage), exchanges);
                    }
                    int valueOffset = readResult.ResponseData.Length - 11;
                    for (int index = 0; index < randomValue.Length; index++)
                    {
                        if (readResult.ResponseData[valueOffset + index] != randomValue[index])
                        {
                            return new ObuVersionReadResult(false, lastValue, string.Format("第{0}/{1}轮SN复读值与写入值不一致。", cycle + 1, count), exchanges);
                        }
                    }
                    nextLlc = 0xF7;
                }

                if (modifySerialNumber)
                {
                    byte[] setMmiCommand = { 0x00, 0x04, obuMac[0], obuMac[1], obuMac[2], obuMac[3], 0x40, nextLlc, 0x99, 0x05, 0x01, 0x04, 0x1A, 0x00 };
                    // 原SN流程复读后继续轮换LLC并发送SetMMI；MACID流程没有该步骤。
                    TransparentCommandResult setMmiResult = ExecuteTransparent(setMmiCommand, timeout);
                    exchanges.Add(ObuProtocolExchange.FromTransparentResult(setMmiResult));
                    if (!setMmiResult.IsSuccess)
                    {
                        return new ObuVersionReadResult(false, lastValue, string.Format("第{0}/{1}轮SetMMI失败：{2}", cycle + 1, count, setMmiResult.ErrorMessage), exchanges);
                    }
                }
                if (cancellationToken.WaitHandle.WaitOne(2000))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            string targetName = modifySerialNumber ? "SN号" : "MACID";
            return new ObuVersionReadResult(true, lastValue, string.Format("{0}修改测试完成，成功{1}/{1}轮。", targetName, count), exchanges);
        }

        /// <summary>
        /// 执行原万集自有协议“地区原值读写”或“配置通道指令验证”流程，共用BST/VST、71认证、SetMMI和EventReport步骤。
        /// </summary>
        /// <param name="timeout">每个 DLL 请求或响应调用的超时时间，单位为毫秒，必须大于0。</param>
        /// <param name="validateConfiguredChannel">为true时验证 configuredChannel 指令；为false时执行DD读取、DC原值回写及DD复读。</param>
        /// <param name="configuredChannel">从SetMe.ini SET_Channel/Channel读取的单字节通道指令；仅验证通道时使用。</param>
        /// <param name="cancellationToken">测试窗口统一取消令牌；每次非托管调用前检查。</param>
        /// <returns>Version字段承载地区字节或通道响应数据；全部协议步骤成功时IsSuccess为true。</returns>
        /// <exception cref="InvalidOperationException">通信端口未打开或台发未初始化时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">超时时间不大于0时抛出。</exception>
        /// <exception cref="OperationCanceledException">测试在协议步骤间被停止时抛出。</exception>
        /// <remarks>DLL句柄和缓冲区由服务持有；x86 stdcall返回0表示调用成功，EventReport为单向发送。</remarks>
        internal ObuVersionReadResult ExecuteWanjiAreaOrChannelTest(
            int timeout,
            bool validateConfiguredChannel,
            byte configuredChannel,
            CancellationToken cancellationToken)
        {
            if (!IsOpen || !IsInitialized)
            {
                throw new InvalidOperationException("通信端口必须已打开且台发必须已初始化。");
            }
            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "测试超时必须大于0毫秒。");
            }

            byte[] bstCommand =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x50, 0x03, 0x91, 0xC0,
                0x08, 0x00, 0x00, 0x00, 0x66, 0xC1, 0x52, 0x02, 0x00, 0x01,
                0x41, 0x87, 0x29, 0xA0, 0x1A, 0x00, 0x04, 0x00, 0x2B, 0x00
            };
            List<ObuProtocolExchange> exchanges = new List<ObuProtocolExchange>();
            cancellationToken.ThrowIfCancellationRequested();
            // 两个原用例都从SendBST_GB建链开始，并使用本轮VST返回的MAC。
            TransparentCommandResult bstResult = ExecuteTransparent(bstCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(bstResult));
            if (!bstResult.IsSuccess || bstResult.ResponseData.Length < 4)
            {
                return new ObuVersionReadResult(false, string.Empty, "BST/VST交互失败：" + bstResult.ErrorMessage, exchanges);
            }
            byte[] obuMac = new byte[4];
            Buffer.BlockCopy(bstResult.ResponseData, 0, obuMac, 0, obuMac.Length);

            cancellationToken.ThrowIfCancellationRequested();
            // 私有70通道指令之前执行旧HandsetVerifyToken对应的71通道认证。
            ObuVersionReadResult verifyResult = VerifyVersionChannel(obuMac, timeout, exchanges, cancellationToken);
            if (!verifyResult.IsSuccess)
            {
                return new ObuVersionReadResult(false, string.Empty, verifyResult.Message, exchanges);
            }

            string displayedValue;
            byte nextLlc;
            if (validateConfiguredChannel)
            {
                byte[] channelCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, new byte[] { 0x70, 0x01, 0x01, configuredChannel });
                // 原函数把SET_Channel单字节写入指令码位置，最终帧为70 01 01 xx。
                TransparentCommandResult channelResult = ExecuteTransparent(channelCommand, timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(channelResult));
                if (!channelResult.IsSuccess || !HasSuccessfulStatus(channelResult.ResponseData))
                {
                    return new ObuVersionReadResult(false, configuredChannel.ToString("X2"), "通道指令验证失败：" + channelResult.ErrorMessage, exchanges);
                }
                displayedValue = channelResult.ResponseData.Length > 17
                    ? BitConverter.ToString(channelResult.ResponseData, 14, channelResult.ResponseData.Length - 17).Replace("-", " ")
                    : string.Empty;
                nextLlc = 0x77;
            }
            else
            {
                byte[] readAreaCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, new byte[] { 0x70, 0x01, 0x01, 0xDD });
                // 首次DD读取当前地区；旧代码随后以该实读值覆盖INI值并原值回写。
                TransparentCommandResult firstRead = ExecuteTransparent(readAreaCommand, timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(firstRead));
                if (!firstRead.IsSuccess || !HasSuccessfulStatus(firstRead.ResponseData) || firstRead.ResponseData.Length < 18)
                {
                    return new ObuVersionReadResult(false, string.Empty, "首次读取地区失败：" + firstRead.ErrorMessage, exchanges);
                }
                byte area = firstRead.ResponseData[14];
                displayedValue = area.ToString("X2");

                byte[] writeAreaCommand = CreateTransferCommand(obuMac, 0x77, 0x01, new byte[] { 0x70, 0x01, 0x02, 0xDC, area });
                // DC写入严格使用首次DD实读的同一地区字节，而不是SET_AreaCode配置值。
                TransparentCommandResult writeArea = ExecuteTransparent(writeAreaCommand, timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(writeArea));
                if (!writeArea.IsSuccess || !HasSuccessfulStatus(writeArea.ResponseData))
                {
                    return new ObuVersionReadResult(false, displayedValue, "地区原值回写失败：" + writeArea.ErrorMessage, exchanges);
                }

                byte[] verifyAreaCommand = CreateTransferCommand(obuMac, 0xF7, 0x01, new byte[] { 0x70, 0x01, 0x01, 0xDD });
                // 回写后再次DD复读，偏移14的地区字节必须与首次读取一致。
                TransparentCommandResult secondRead = ExecuteTransparent(verifyAreaCommand, timeout);
                exchanges.Add(ObuProtocolExchange.FromTransparentResult(secondRead));
                if (!secondRead.IsSuccess || !HasSuccessfulStatus(secondRead.ResponseData) || secondRead.ResponseData.Length < 18 || secondRead.ResponseData[14] != area)
                {
                    return new ObuVersionReadResult(false, displayedValue, "地区回写后的复读值不一致。", exchanges);
                }
                nextLlc = 0x77;
            }

            byte[] setMmiCommand = { 0x00, 0x04, obuMac[0], obuMac[1], obuMac[2], obuMac[3], 0x40, nextLlc, 0x99, 0x05, 0x01, 0x04, 0x1A, 0x00 };
            // 成功完成私有指令后按原流程发送SetMMI。
            TransparentCommandResult setMmiResult = ExecuteTransparent(setMmiCommand, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(setMmiResult));
            if (!setMmiResult.IsSuccess)
            {
                return new ObuVersionReadResult(false, displayedValue, "SetMMI失败：" + setMmiResult.ErrorMessage, exchanges);
            }

            byte[] eventReport = { 0x00, 0x05, obuMac[0], obuMac[1], obuMac[2], obuMac[3], 0x03, 0x99, 0x60, 0x01, 0x00 };
            cancellationToken.ThrowIfCancellationRequested();
            // 最后单向发送EventReport释放会话，不等待OBU响应。
            TransparentCommandResult eventResult = ExecuteTransparent(eventReport, timeout);
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(eventResult));
            string successMessage = validateConfiguredChannel ? "不同通道指令验证成功。" : "地区原值读写和复读校验成功。";
            return eventResult.IsSuccess
                ? new ObuVersionReadResult(true, displayedValue, successMessage, exchanges)
                : new ObuVersionReadResult(false, displayedValue, "EventReport发送失败：" + eventResult.ErrorMessage, exchanges);
        }

        /// <summary>
        /// 执行原上位机 71 通道两步认证，并把认证失败定位到具体阶段。
        /// </summary>
        /// <param name="obuMac">BST/VST 返回的 4 字节 OBU MAC 地址。</param>
        /// <param name="timeout">每个认证请求或响应调用的超时时间，单位为毫秒。</param>
        /// <param name="exchanges">当前版本号读取流程的交互帧收集器；认证请求和响应会追加到其中。</param>
        /// <param name="cancellationToken">测试大框的统一取消令牌；在每次认证 DLL 调用前检查。</param>
        /// <returns>认证成功或失败原因。</returns>
        /// <exception cref="OperationCanceledException">停止按钮在两步认证之间请求取消时抛出。</exception>
        private ObuVersionReadResult VerifyVersionChannel(
            byte[] obuMac,
            int timeout,
            IList<ObuProtocolExchange> exchanges,
            CancellationToken cancellationToken)
        {
            byte[] firstVerifyCommand = CreateTransferCommand(obuMac, 0xF7, 0xF4, new byte[] { 0x71, 0x01, 0x01, 0x01 });
            cancellationToken.ThrowIfCancellationRequested();
            // 认证第一步获取随机数和 20 字节版本密文，响应数据用于生成第二步认证材料。
            TransparentCommandResult firstResult = ExecuteTransparent(firstVerifyCommand, timeout);
            // 保存 71 认证第一步的完整请求和响应，便于定位随机数或密文异常。
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(firstResult));
            if (!firstResult.IsSuccess || !HasSuccessfulStatus(firstResult.ResponseData))
            {
                return new ObuVersionReadResult(false, string.Empty, "71 通道认证第一步失败：" + firstResult.ErrorMessage, exchanges);
            }

            if (firstResult.ResponseData.Length < 43)
            {
                return new ObuVersionReadResult(false, string.Empty, "71 通道认证第一步响应长度不足。", exchanges);
            }

            byte[] encryptedVersion = TransformVersionChallenge(firstResult.ResponseData, 15);
            byte[] secondInstruction = new byte[24];
            secondInstruction[0] = 0x71;
            secondInstruction[1] = 0x01;
            secondInstruction[2] = 0x15;
            secondInstruction[3] = 0x02;
            Buffer.BlockCopy(encryptedVersion, 0, secondInstruction, 4, encryptedVersion.Length);
            byte[] secondVerifyCommand = CreateTransferCommand(obuMac, 0x77, 0xF4, secondInstruction);
            cancellationToken.ThrowIfCancellationRequested();
            // 认证第二步回传变换后的版本密文，只有 900000 响应才允许继续读取版本号。
            TransparentCommandResult secondResult = ExecuteTransparent(secondVerifyCommand, timeout);
            // 保存 71 认证第二步的完整请求和响应，结果文件按实际调用顺序输出。
            exchanges.Add(ObuProtocolExchange.FromTransparentResult(secondResult));
            return secondResult.IsSuccess && HasSuccessfulStatus(secondResult.ResponseData)
                ? new ObuVersionReadResult(true, string.Empty, "71 通道认证成功。", exchanges)
                : new ObuVersionReadResult(false, string.Empty, "71 通道认证第二步失败：" + secondResult.ErrorMessage, exchanges);
        }

        /// <summary>
        /// 生成带本地透传类型标识的 TransferChannel 命令。
        /// </summary>
        /// <param name="obuMac">4 字节 OBU MAC 地址。</param>
        /// <param name="llc">LLC 控制域，当前版本认证流程使用 0xF7 或 0x77。</param>
        /// <param name="serviceByte">TransferChannel 服务字节；认证使用 0xF4，版本读取使用 0x01。</param>
        /// <param name="instruction">应用层指令，从 0x71 或 0x70 开始。</param>
        /// <returns>可交给 ExecuteTransparent 的完整透传命令。</returns>
        private static byte[] CreateTransferCommand(byte[] obuMac, byte llc, byte serviceByte, byte[] instruction)
        {
            byte[] command = new byte[2 + 11 + instruction.Length];
            command[0] = 0x00;
            command[1] = 0x03;
            command[2] = 0x08;
            command[3] = 0x00;
            command[4] = 0x00;
            command[5] = 0x01;
            command[6] = 0x40;
            command[7] = llc;
            command[8] = 0x91;
            command[9] = 0x05;
            command[10] = serviceByte;
            command[11] = 0x03;
            command[12] = 0x18;
            Buffer.BlockCopy(obuMac, 0, command, 2, 4);
            Buffer.BlockCopy(instruction, 0, command, 13, instruction.Length);
            return command;
        }

        /// <summary>
        /// 根据原上位机规则对认证响应中的 8 字节随机数和 20 字节版本密文进行两次异或变换。
        /// </summary>
        /// <param name="responseData">认证第一步完整响应数据。</param>
        /// <param name="dataOffset">随机数和密文起始位置；原流程为 15。</param>
        /// <returns>第二步认证需要回传的 20 字节密文。</returns>
        private static byte[] TransformVersionChallenge(byte[] responseData, int dataOffset)
        {
            byte[] random = new byte[8];
            byte[] versionCipher = new byte[20];
            Buffer.BlockCopy(responseData, dataOffset, random, 0, random.Length);
            Buffer.BlockCopy(responseData, dataOffset + random.Length, versionCipher, 0, versionCipher.Length);

            byte[] version = new byte[20];
            for (int index = 0; index < version.Length; index++)
            {
                version[index] = (byte)(versionCipher[index] ^ (byte)~random[(index % 4) * 2]);
            }

            byte[] result = new byte[20];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = (byte)(version[index] ^ (byte)~random[(index % 4) * 2 + 1]);
            }

            return result;
        }

        /// <summary>
        /// 检查 DLL 返回帧末尾是否为原上位机使用的 90 00 00 成功状态。
        /// </summary>
        /// <param name="responseData">DLL 返回的完整响应帧。</param>
        /// <returns>包含 90 00 00 且长度足够时返回 true。</returns>
        private static bool HasSuccessfulStatus(byte[] responseData)
        {
            return responseData != null
                && responseData.Length >= 3
                && responseData[responseData.Length - 3] == 0x90
                && responseData[responseData.Length - 2] == 0x00
                && responseData[responseData.Length - 1] == 0x00;
        }

        /// <summary>
        /// 根据透传命令类型字节返回界面和日志使用的命令名称。
        /// </summary>
        /// <param name="commandType">本地类型标识第二字节，支持 0x01～0x07。</param>
        /// <returns>已知类型的协议名称；未知类型返回“未知类型”。</returns>
        internal static string GetTransparentCommandName(byte commandType)
        {
            switch (commandType)
            {
                case 0x01: return "BST / VST";
                case 0x02: return "GetSecure";
                case 0x03: return "TransferChannel";
                case 0x04: return "SetMMI";
                case 0x05: return "EventReport";
                case 0x06: return "OtherFrames";
                case 0x07: return "ICC / CPC";
                default: return "未知类型";
            }
        }

        /// <summary>
        /// 按台发初始化选项更新 BST 的 BeaconID 和 UnixTime 字段。
        /// </summary>
        /// <param name="payload">不含两字节本地类型标识的 BST 缓冲区；字段在原数组内更新。</param>
        private void ApplyBstDynamicFields(byte[] payload)
        {
            if (_initializationOptions == null)
            {
                return;
            }

            if (_initializationOptions.BidChange && payload.Length >= 12)
            {
                // 对齐旧程序的全局 gBeaconID：跨用例连续递增，不能每次从新BST模板的固定字节重新计算。
                _beaconId = (_beaconId + 1) & 0x00FFFFFF;
                payload[9] = (byte)(_beaconId >> 16);
                payload[10] = (byte)(_beaconId >> 8);
                payload[11] = (byte)_beaconId;
            }

            if (_initializationOptions.UnixTimeChange && payload.Length >= 16)
            {
                // BST 动态时间入口复用初始化阶段的网络序 UnixTime 生成逻辑。
                byte[] unixTime = GetUnixTimeBytes();
                Buffer.BlockCopy(unixTime, 0, payload, 12, unixTime.Length);
            }
        }

        /// <summary>
        /// 按 DLL 返回的有效长度复制响应，避免把未写入的缓冲区尾部暴露给界面。
        /// </summary>
        /// <param name="buffer">DLL 写入的固定长度响应缓冲区。</param>
        /// <param name="length">DLL 报告的有效长度，超界值会限制在缓冲区范围。</param>
        /// <returns>独立且长度准确的响应字节数组。</returns>
        private static byte[] CopyResponse(byte[] buffer, int length)
        {
            int safeLength = Math.Max(0, Math.Min(buffer.Length, length));
            byte[] result = new byte[safeLength];
            Buffer.BlockCopy(buffer, 0, result, 0, safeLength);
            return result;
        }

        public void Dispose()
        {
            Close();
        }

        private static byte[] GetUnixTimeBytes()
        {
            long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            uint value = unchecked((uint)seconds);
            return new[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        /// <summary>
        /// 克隆台发初始化参数，避免调用方保存的恢复配置被后续重初始化操作修改。
        /// </summary>
        /// <param name="options">要复制的初始化配置。</param>
        /// <returns>字段值完全独立的新配置对象。</returns>
        private static DesktopInitializationOptions CloneInitializationOptions(DesktopInitializationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new DesktopInitializationOptions
            {
                BstInterval = options.BstInterval,
                RetryInterval = options.RetryInterval,
                RetryTimes = options.RetryTimes,
                Timeout = options.Timeout,
                TxPower = options.TxPower,
                PhysicalChannelId = options.PhysicalChannelId,
                BidChange = options.BidChange,
                UnixTimeChange = options.UnixTimeChange,
                LlcChange = options.LlcChange,
                MacChange = options.MacChange
            };
        }
    }

    internal sealed class DesktopInitializationOptions
    {
        internal int BstInterval { get; set; }
        internal int RetryInterval { get; set; }
        internal int RetryTimes { get; set; }
        internal int Timeout { get; set; }
        internal int TxPower { get; set; }
        internal int PhysicalChannelId { get; set; }
        internal bool BidChange { get; set; }
        internal bool UnixTimeChange { get; set; }
        internal bool LlcChange { get; set; }
        internal bool MacChange { get; set; }
    }

    internal sealed class DesktopInitializationResult
    {
        internal DesktopInitializationResult(int requestResult, int? responseResult, int desktopStatus, byte[] desktopInfo)
        {
            RequestResult = requestResult;
            ResponseResult = responseResult;
            DesktopStatus = desktopStatus;
            DesktopInfo = desktopInfo;
        }

        internal int RequestResult { get; }
        internal int? ResponseResult { get; }
        internal int DesktopStatus { get; }
        internal byte[] DesktopInfo { get; }

        internal bool IsSuccess => RequestResult == 0 && ResponseResult == 0;
    }

    /// <summary>
    /// 表示一次 OBU 版本号读取流程的结果。
    /// </summary>
    internal sealed class ObuVersionReadResult
    {
        /// <summary>
        /// 创建 OBU 版本号读取结果。
        /// </summary>
        /// <param name="isSuccess">流程是否完成且版本号响应通过校验。</param>
        /// <param name="version">读取到的 ASCII 版本文本；失败时为空字符串。</param>
        /// <param name="message">面向测试信息显示区的阶段结果或失败原因。</param>
        /// <param name="exchanges">按实际调用顺序保存的完整请求/响应帧；失败时也包含已经完成的交互。</param>
        internal ObuVersionReadResult(bool isSuccess, string version, string message, IList<ObuProtocolExchange> exchanges)
        {
            IsSuccess = isSuccess;
            Version = version ?? string.Empty;
            Message = message ?? string.Empty;
            Exchanges = new List<ObuProtocolExchange>(exchanges ?? new List<ObuProtocolExchange>()).AsReadOnly();
        }

        internal bool IsSuccess { get; }
        internal string Version { get; }
        internal string Message { get; }
        internal IList<ObuProtocolExchange> Exchanges { get; }
    }

    /// <summary>
    /// 表示一次 OBU 版本号读取流程中的完整透传请求和响应。
    /// </summary>
    internal sealed class ObuProtocolExchange
    {
        /// <summary>
        /// 根据一次 DLL 透传结果复制请求、响应和 DLL 返回码，避免结果文件依赖可复用缓冲区。
        /// </summary>
        /// <param name="result">已完成或失败的透传结果；请求和响应数组由本方法复制。</param>
        /// <returns>可供结果文件格式化的独立交互记录。</returns>
        internal static ObuProtocolExchange FromTransparentResult(TransparentCommandResult result)
        {
            return new ObuProtocolExchange(
                result.CommandName,
                result.RequestData,
                result.ResponseData,
                result.RequestResult,
                result.ResponseResult);
        }

        /// <summary>
        /// 创建一条完整透传交互记录。
        /// </summary>
        /// <param name="commandName">协议命令名称。</param>
        /// <param name="requestData">实际交给 DLL 的上行空口数据，不包含本地两字节类型标识。</param>
        /// <param name="responseData">DLL 接收的下行空口数据；无响应时为空数组。</param>
        /// <param name="requestResult">DLL 请求返回码。</param>
        /// <param name="responseResult">DLL 响应返回码；单向请求或未进入响应阶段时为空。</param>
        private ObuProtocolExchange(string commandName, byte[] requestData, byte[] responseData, int requestResult, int? responseResult)
        {
            CommandName = commandName ?? string.Empty;
            RequestData = requestData == null ? new byte[0] : (byte[])requestData.Clone();
            ResponseData = responseData == null ? new byte[0] : (byte[])responseData.Clone();
            RequestResult = requestResult;
            ResponseResult = responseResult;
        }

        internal string CommandName { get; }
        internal byte[] RequestData { get; }
        internal byte[] ResponseData { get; }
        internal int RequestResult { get; }
        internal int? ResponseResult { get; }
    }

    /// <summary>
    /// 表示一条透传命令的 DLL 请求、可选响应和接收数据。
    /// </summary>
    internal sealed class TransparentCommandResult
    {
        /// <summary>
        /// 创建透传命令执行结果。
        /// </summary>
        /// <param name="commandName">命令类型名称。</param>
        /// <param name="requestResult">DLL 请求返回码，0 表示成功。</param>
        /// <param name="responseResult">需要响应时的 DLL 返回码；单向命令为空。</param>
        /// <param name="responseData">成功接收到的有效响应数据，不含缓冲区尾部。</param>
        internal TransparentCommandResult(string commandName, int requestResult, int? responseResult, byte[] responseData)
        {
            CommandName = commandName;
            RequestResult = requestResult;
            ResponseResult = responseResult;
            ResponseData = responseData;
        }

        internal string CommandName { get; }
        internal int RequestResult { get; }
        internal int? ResponseResult { get; }
        internal byte[] ResponseData { get; }
        /// <summary>服务层动态处理并交给 DLL 的请求缓冲区快照；不代表独立空口抓包。</summary>
        internal byte[] RequestData { get; set; }
        internal bool IsSuccess => RequestResult == 0 && (!ResponseResult.HasValue || ResponseResult.Value == 0);
        internal string ErrorMessage => RequestResult != 0
            ? $"请求返回码 {RequestResult}"
            : ResponseResult.HasValue && ResponseResult.Value != 0
                ? $"响应返回码 {ResponseResult.Value}"
                : string.Empty;
    }
}
