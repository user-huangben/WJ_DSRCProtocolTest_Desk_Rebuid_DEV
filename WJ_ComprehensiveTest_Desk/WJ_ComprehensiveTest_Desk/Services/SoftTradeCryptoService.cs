using System;
using System.Globalization;
using System.Security.Cryptography;

namespace WJ_DSRCProtocolTest_Desk_Net.Services
{
    /// <summary>
    /// 原上位机“软交易”算法选择。
    /// </summary>
    internal enum SoftTradeAlgorithm
    {
        /// <summary>使用原 ruanPsamCheckMAC1/2 的双倍 3DES 规则。</summary>
        TripleDes = 0,

        /// <summary>使用原 ruanSM4CheckMAC1/2 的 SM4 规则。</summary>
        Sm4 = 1
    }

    /// <summary>
    /// 在进程内实现原上位机的软交易密钥分散、MAC1 和时间提取。
    /// 该服务不打开读卡器、不调用 PSAM，也不依赖用户提供的密码 DLL。
    /// </summary>
    internal sealed class SoftTradeCryptoService
    {
        private static readonly byte[] TripleDesRootKey =
        {
            0xF3, 0xD8, 0x68, 0xB2, 0x16, 0x67, 0x9B, 0xE6,
            0x2C, 0x8F, 0x82, 0xEF, 0xE6, 0xCA, 0xC7, 0x4F
        };

        private static readonly byte[] Sm4RootKey =
        {
            0xB6, 0xC4, 0x30, 0x72, 0xD0, 0x62, 0x6F, 0xC5,
            0x02, 0xC8, 0x49, 0x1F, 0x1F, 0x01, 0x5B, 0x87
        };

        private static readonly byte[] Sm4Sbox =
        {
            0xD6,0x90,0xE9,0xFE,0xCC,0xE1,0x3D,0xB7,0x16,0xB6,0x14,0xC2,0x28,0xFB,0x2C,0x05,
            0x2B,0x67,0x9A,0x76,0x2A,0xBE,0x04,0xC3,0xAA,0x44,0x13,0x26,0x49,0x86,0x06,0x99,
            0x9C,0x42,0x50,0xF4,0x91,0xEF,0x98,0x7A,0x33,0x54,0x0B,0x43,0xED,0xCF,0xAC,0x62,
            0xE4,0xB3,0x1C,0xA9,0xC9,0x08,0xE8,0x95,0x80,0xDF,0x94,0xFA,0x75,0x8F,0x3F,0xA6,
            0x47,0x07,0xA7,0xFC,0xF3,0x73,0x17,0xBA,0x83,0x59,0x3C,0x19,0xE6,0x85,0x4F,0xA8,
            0x68,0x6B,0x81,0xB2,0x71,0x64,0xDA,0x8B,0xF8,0xEB,0x0F,0x4B,0x70,0x56,0x9D,0x35,
            0x1E,0x24,0x0E,0x5E,0x63,0x58,0xD1,0xA2,0x25,0x22,0x7C,0x3B,0x01,0x21,0x78,0x87,
            0xD4,0x00,0x46,0x57,0x9F,0xD3,0x27,0x52,0x4C,0x36,0x02,0xE7,0xA0,0xC4,0xC8,0x9E,
            0xEA,0xBF,0x8A,0xD2,0x40,0xC7,0x38,0xB5,0xA3,0xF7,0xF2,0xCE,0xF9,0x61,0x15,0xA1,
            0xE0,0xAE,0x5D,0xA4,0x9B,0x34,0x1A,0x55,0xAD,0x93,0x32,0x30,0xF5,0x8C,0xB1,0xE3,
            0x1D,0xF6,0xE2,0x2E,0x82,0x66,0xCA,0x60,0xC0,0x29,0x23,0xAB,0x0D,0x53,0x4E,0x6F,
            0xD5,0xDB,0x37,0x45,0xDE,0xFD,0x8E,0x2F,0x03,0xFF,0x6A,0x72,0x6D,0x6C,0x5B,0x51,
            0x8D,0x1B,0xAF,0x92,0xBB,0xDD,0xBC,0x7F,0x11,0xD9,0x5C,0x41,0x1F,0x10,0x5A,0xD8,
            0x0A,0xC1,0x31,0x88,0xA5,0xCD,0x7B,0xBD,0x2D,0x74,0xD0,0x12,0xB8,0xE5,0xB4,0xB0,
            0x89,0x69,0x97,0x4A,0x0C,0x96,0x77,0x7E,0x65,0xB9,0xF1,0x09,0xC5,0x6E,0xC6,0x84,
            0x18,0xF0,0x7D,0xEC,0x3A,0xDC,0x4D,0x20,0x79,0xEE,0x5F,0x3E,0xD7,0xCB,0x39,0x48
        };

        private static readonly uint[] Sm4Fk = { 0xA3B1BAC6, 0x56AA3350, 0x677D9197, 0xB27022DC };

        private static readonly uint[] Sm4Ck =
        {
            0x00070E15,0x1C232A31,0x383F464D,0x545B6269,0x70777E85,0x8C939AA1,0xA8AFB6BD,0xC4CBD2D9,
            0xE0E7EEF5,0xFC030A11,0x181F262D,0x343B4249,0x50575E65,0x6C737A81,0x888F969D,0xA4ABB2B9,
            0xC0C7CED5,0xDCE3EAF1,0xF8FF060D,0x141B2229,0x30373E45,0x4C535A61,0x686F767D,0x848B9299,
            0xA0A7AEB5,0xBCC3CAD1,0xD8DFE6ED,0xF4FB0209,0x10171E25,0x2C333A41,0x484F565D,0x646B7279
        };

        /// <summary>
        /// 按原软交易入口生成 MAC1，并使用计算瞬间的 UTC 时间参与 MAC1。
        /// </summary>
        /// <param name="algorithm">界面选择的 3DES 或 SM4 算法。</param>
        /// <param name="icc0015">VST 或预读得到的 ICC0015，至少包含合同序列号和芯片序列号。</param>
        /// <param name="consumeInitializeResponse">消费初始化完整响应，原偏移 18/25 分别为 EP 和随机数。</param>
        /// <param name="tradeSetting">4 字节金额加 1 字节通行状态；第 5 字节只写入 0019，不参与 MAC1 交易类型选择。</param>
        /// <param name="tradeTime">输出当前 UTC 时间对应的 7 字节 BCD 时间。</param>
        /// <param name="mac1">输出 4 字节 MAC1。</param>
        /// <param name="errorMessage">失败时输出原因。</param>
        /// <returns>字段完整且软算法计算成功时返回 true。</returns>
        internal bool TryCreateMac1(
            SoftTradeAlgorithm algorithm,
            byte[] icc0015,
            byte[] consumeInitializeResponse,
            byte[] tradeSetting,
            out byte[] tradeTime,
            out byte[] mac1,
            out string errorMessage)
        {
            tradeTime = null;
            mac1 = null;
            errorMessage = string.Empty;
            if (icc0015 == null || icc0015.Length < 20)
            {
                errorMessage = "ICC0015 不足 20 字节，无法分散合同序列号和芯片序列号。";
                return false;
            }

            if (consumeInitializeResponse == null || consumeInitializeResponse.Length < 29)
            {
                errorMessage = "消费初始化响应不足 29 字节，无法提取 EP 和随机数。";
                return false;
            }

            if (tradeSetting == null || tradeSetting.Length < 5)
            {
                errorMessage = "交易配置不足 5 字节。";
                return false;
            }

            try
            {
                // 原 ETC.cpp 的 Decode_Apdu 在收到消费初始化响应后直接取当前 UTC 时间，
                // 并不是从响应尾部读取时间字段；这里保持同一软算语义。
                tradeTime = CreateTradeTime();
                byte[] contractSerial = CopyRepeatedContractSerial(icc0015);
                byte[] chipSerial = CopyBytes(icc0015, 12, 8);
                byte[] randomAndEp = new byte[8];
                Buffer.BlockCopy(consumeInitializeResponse, 25, randomAndEp, 0, 4);
                Buffer.BlockCopy(consumeInitializeResponse, 18, randomAndEp, 4, 2);
                randomAndEp[6] = 0x06;
                randomAndEp[7] = 0xC6;

                // 原上位机 Decode_Apdu 的车道软算入口固定令 IccTransType=0x09；
                // SetMe.ini 第 5 字节是写入 0019 的通行状态，不能误作 MAC1 交易类型。
                byte transactionType = 0x09;
                byte[] macInput = new byte[18];
                Buffer.BlockCopy(tradeSetting, 0, macInput, 0, 4);
                macInput[4] = transactionType;
                for (int index = 5; index < 11; index++) macInput[index] = 0x37;
                Buffer.BlockCopy(tradeTime, 0, macInput, 11, 7);

                if (algorithm == SoftTradeAlgorithm.Sm4)
                {
                    byte[] secondKey = Sm4DeriveKey(Sm4RootKey, contractSerial);
                    byte[] thirdKey = Sm4DeriveKey(secondKey, chipSerial);
                    byte[] macKey = Sm4DeriveKey(thirdKey, randomAndEp);
                    mac1 = TakeFirstFour(Sm4CalculateMac(macKey, new byte[8], macInput));
                }
                else
                {
                    byte[] secondKey = TripleDesDeriveKey(TripleDesRootKey, contractSerial);
                    byte[] thirdKey = TripleDesDeriveKey(secondKey, chipSerial);
                    byte[] macKey = TripleDesEncrypt(thirdKey, randomAndEp);
                    mac1 = TakeFirstFour(SingleDesCalculateMac(macKey, new byte[8], macInput));
                }

                return true;
            }
            catch (CryptographicException exception)
            {
                errorMessage = "软算法计算失败：" + exception.Message;
                return false;
            }
            catch (ArgumentException exception)
            {
                errorMessage = "软算法输入无效：" + exception.Message;
                return false;
            }
        }

        /// <summary>按原双片式 SM4 典型交易规则，用全零外部认证密钥计算 8 字节认证数据。</summary>
        /// <param name="random">ICC 返回的 16 字节随机数。</param>
        /// <returns>SM4 密文前后 8 字节异或得到的认证数据。</returns>
        internal byte[] CreateTypicalSm4ExternalAuthentication(byte[] random)
        {
            if (random == null || random.Length != 16) throw new ArgumentException("外部认证随机数必须为 16 字节。", nameof(random));
            byte[] encrypted = Sm4EncryptBlock(new byte[16], random);
            byte[] authentication = new byte[8];
            for (int index = 0; index < authentication.Length; index++) authentication[index] = (byte)(encrypted[index] ^ encrypted[index + 8]);
            return authentication;
        }

        /// <summary>按原单片/双片分支计算 SM4 外部认证数据。</summary>
        /// <param name="random">ICC 的 16 字节随机数。</param><param name="esamVersion">0015 中的 ESAM 版本；52 为单片。</param>
        /// <param name="firstFactor">0015 提供的一次分散因子。</param><param name="secondFactor">0019 提供的二次分散因子。</param>
        /// <returns>8 字节外部认证数据。</returns>
        internal byte[] CreateTypicalSm4ExternalAuthentication(byte[] random, byte esamVersion, byte[] firstFactor, byte[] secondFactor)
        {
            if (esamVersion != 0x52) return CreateTypicalSm4ExternalAuthentication(random);
            byte[] root = { 0x5C,0x2B,0x20,0xB0,0x65,0x72,0xC6,0x88,0x38,0xED,0x39,0x00,0x00,0xDC,0xBF,0x16 };
            byte[] key = Sm4DeriveKey(Sm4DeriveKey(root, firstFactor), secondFactor);
            byte[] encrypted = Sm4EncryptBlock(key, random);
            byte[] authentication = new byte[8];
            for (int index = 0; index < 8; index++) authentication[index] = (byte)(encrypted[index] ^ encrypted[index + 8]);
            return authentication;
        }

        /// <summary>按旧发行/激活流程连续使用多个8字节因子分散指定的16字节SM4根密钥。</summary>
        /// <param name="rootKey">旧上位机发行流程使用的16字节根密钥。</param>
        /// <param name="diversificationFactors">按一次、二次、三次顺序排列的8字节分散因子。</param>
        /// <returns>最后一级16字节派生密钥。</returns>
        internal byte[] DeriveSm4Key(byte[] rootKey, params byte[][] diversificationFactors)
        {
            if (rootKey == null || rootKey.Length != 16) throw new ArgumentException("SM4根密钥必须为16字节。", nameof(rootKey));
            byte[] key = (byte[])rootKey.Clone();
            foreach (byte[] factor in diversificationFactors ?? new byte[0][])
            {
                if (factor == null || factor.Length != 8) throw new ArgumentException("SM4分散因子必须为8字节。", nameof(diversificationFactors));
                key = Sm4DeriveKey(key, factor);
            }
            return key;
        }

        /// <summary>按旧 <c>SM4CalcuMac</c> 规则计算发行/激活安全报文的前4字节MAC。</summary>
        /// <param name="key">16字节SM4密钥。</param><param name="random">4或8字节随机数初值。</param><param name="message">参与MAC计算的完整APDU原文。</param>
        /// <returns>4字节MAC。</returns>
        internal byte[] CalculateSm4Mac(byte[] key, byte[] random, byte[] message)
        {
            if (random == null || (random.Length != 4 && random.Length != 8)) throw new ArgumentException("随机数必须为4或8字节。", nameof(random));
            byte[] initialization = new byte[8];
            Buffer.BlockCopy(random, 0, initialization, 0, random.Length);
            return Sm4CalculateMac(key, initialization, message ?? throw new ArgumentNullException(nameof(message)));
        }

        /// <summary>使用旧发行流程相同的无填充SM4 ECB方式解密固定块数据。</summary>
        /// <param name="key">16字节SM4密钥。</param><param name="input">16字节倍数的密文。</param><returns>与输入等长的明文。</returns>
        internal byte[] DecryptSm4(byte[] key, byte[] input)
        {
            if (key == null || key.Length != 16) throw new ArgumentException("SM4密钥必须为16字节。", nameof(key));
            if (input == null || input.Length == 0 || input.Length % 16 != 0) throw new ArgumentException("SM4密文必须为非空的16字节倍数。", nameof(input));
            byte[] output = new byte[input.Length];
            for (int offset = 0; offset < input.Length; offset += 16)
            {
                byte[] block = new byte[16];
                Buffer.BlockCopy(input, offset, block, 0, 16);
                Buffer.BlockCopy(Sm4DecryptBlock(key, block), 0, output, offset, 16);
            }
            return output;
        }

        /// <summary>使用旧发行流程相同的无填充SM4 ECB方式加密固定块数据。</summary>
        internal byte[] EncryptSm4(byte[] key, byte[] input)
        {
            if (key == null || key.Length != 16) throw new ArgumentException("SM4密钥必须为16字节。", nameof(key));
            if (input == null || input.Length == 0 || input.Length % 16 != 0) throw new ArgumentException("SM4明文必须为非空的16字节倍数。", nameof(input));
            byte[] output = new byte[input.Length];
            for (int offset = 0; offset < input.Length; offset += 16)
            {
                byte[] block = new byte[16];
                Buffer.BlockCopy(input, offset, block, 0, 16);
                Buffer.BlockCopy(Sm4EncryptBlock(key, block), 0, output, offset, 16);
            }
            return output;
        }

        /// <summary>按原普通典型交易的两级分散规则计算 3DES 外部认证数据。</summary>
        /// <param name="random">ICC 的 16 字节随机数。</param><param name="firstFactor">一次分散因子。</param><param name="secondFactor">二次分散因子。</param>
        /// <returns>3DES 密文前后 8 字节异或得到的认证数据。</returns>
        internal byte[] CreateTypicalTripleDesExternalAuthentication(byte[] random, byte[] firstFactor, byte[] secondFactor)
        {
            if (random == null || random.Length != 16) throw new ArgumentException("外部认证随机数必须为 16 字节。", nameof(random));
            byte[] root = { 0x41,0x6C,0xD0,0x6C,0x98,0xAE,0xB7,0x91,0xED,0x6F,0xB0,0x9F,0xC9,0xDF,0xA5,0x3A };
            byte[] key = TripleDesDeriveKey(TripleDesDeriveKey(root, firstFactor), secondFactor);
            byte[] encrypted = TripleDesEncrypt(key, random);
            byte[] authentication = new byte[8];
            for (int index = 0; index < 8; index++) authentication[index] = (byte)(encrypted[index] ^ encrypted[index + 8]);
            return authentication;
        }

        /// <summary>按旧北京典型交易的应用密钥分散规则解密并校验 GetSecure 车辆信息。</summary>
        /// <param name="algorithm">由当次 VST 合同版本决定的算法。</param>
        /// <param name="contractProvider">VST 中的 8 字节合同发行方。</param>
        /// <param name="contractSerial">VST 中的 8 字节合同序列号。</param>
        /// <param name="getSecureResponse">GetSecure 完整回复帧。</param>
        /// <param name="errorMessage">失败时的明确原因。</param>
        /// <returns>解密后的固定 32 字节车辆信息与旧用例期望一致时为 true。</returns>
        internal bool TryValidateBeijingVehicleInformation(
            SoftTradeAlgorithm algorithm,
            byte[] contractProvider,
            byte[] contractSerial,
            byte[] getSecureResponse,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (contractProvider == null || contractProvider.Length != 8 || contractSerial == null || contractSerial.Length != 8)
            {
                errorMessage = "VST 合同发行方或合同序列号长度不是 8 字节。";
                return false;
            }

            int encryptedLength = algorithm == SoftTradeAlgorithm.Sm4 ? 64 : 48;
            int plainOffset = algorithm == SoftTradeAlgorithm.Sm4 ? 17 : 9;
            if (getSecureResponse == null || getSecureResponse.Length < 12 + encryptedLength)
            {
                errorMessage = "GetSecure 回复长度不足，无法取得完整车辆信息密文。";
                return false;
            }

            try
            {
                byte[] key = OneChipIssueActivationService.GetRootKeyCopy();
                byte[] factor = OneChipIssueActivationService.GetApplicationEncryptionFactorCopy();
                if (algorithm == SoftTradeAlgorithm.Sm4)
                {
                    key = Sm4DeriveKey(Sm4DeriveKey(Sm4DeriveKey(key, factor), contractProvider), contractSerial);
                }
                else
                {
                    key = TripleDesDeriveKey(TripleDesDeriveKey(TripleDesDeriveKey(key, factor), contractProvider), contractSerial);
                }

                byte[] encrypted = CopyBytes(getSecureResponse, 12, encryptedLength);
                byte[] plain = algorithm == SoftTradeAlgorithm.Sm4 ? DecryptSm4(key, encrypted) : TripleDesDecrypt(key, encrypted);
                byte[] expected = OneChipIssueActivationService.GetVehicleInformationCopy();
                if (plainOffset + expected.Length > plain.Length)
                {
                    errorMessage = "车辆信息明文长度不足。";
                    return false;
                }

                for (int index = 0; index < expected.Length; index++)
                {
                    if (plain[plainOffset + index] != expected[index])
                    {
                        errorMessage = "解密后的车辆信息与旧北京用例固定数据不一致。";
                        return false;
                    }
                }

                return true;
            }
            catch (CryptographicException exception)
            {
                errorMessage = "车辆信息解密失败：" + exception.Message;
                return false;
            }
            catch (ArgumentException exception)
            {
                errorMessage = "车辆信息解密输入无效：" + exception.Message;
                return false;
            }
        }

        /// <summary>生成原上位机使用的当前 UTC 7 字节 BCD 交易时间。</summary>
        /// <returns>年、月、日、时、分、秒及保留字节组成的 7 字节 BCD 时间。</returns>
        private static byte[] CreateTradeTime()
        {
            DateTime utc = DateTime.UtcNow;
            return new[]
            {
                (byte)((utc.Year / 1000 << 4) | ((utc.Year / 100) % 10)),
                (byte)(((utc.Year / 10) % 10 << 4) | (utc.Year % 10)),
                ToBcd(utc.Month), ToBcd(utc.Day), ToBcd(utc.Hour), ToBcd(utc.Minute), ToBcd(utc.Second)
            };
        }

        /// <summary>复制 ICC0015 前 4 字节两次作为合同序列号分散因子。</summary>
        /// <param name="icc0015">至少 4 字节的 ICC0015。</param>
        /// <returns>8 字节分散因子。</returns>
        private static byte[] CopyRepeatedContractSerial(byte[] icc0015)
        {
            byte[] value = new byte[8];
            Buffer.BlockCopy(icc0015, 0, value, 0, 4);
            Buffer.BlockCopy(icc0015, 0, value, 4, 4);
            return value;
        }

        /// <summary>从数组指定位置复制固定长度字节。</summary>
        /// <param name="source">源数组。</param><param name="offset">起始偏移。</param><param name="length">复制长度。</param>
        /// <returns>复制结果。</returns>
        private static byte[] CopyBytes(byte[] source, int offset, int length)
        {
            if (offset < 0 || length < 0 || offset + length > source.Length) throw new ArgumentException("字段超出响应范围。", nameof(offset));
            byte[] result = new byte[length];
            Buffer.BlockCopy(source, offset, result, 0, length);
            return result;
        }

        /// <summary>将十进制数编码为 BCD。</summary>
        /// <param name="value">0～99 的十进制数。</param><returns>BCD 字节。</returns>
        private static byte ToBcd(int value) { return (byte)(((value / 10) << 4) | (value % 10)); }

        /// <summary>按原 DesDerivedKey 规则生成 16 字节双倍 3DES 分散密钥。</summary>
        /// <param name="key">16 字节主密钥。</param><param name="diversification">8 字节分散因子。</param><returns>16 字节派生密钥。</returns>
        private static byte[] TripleDesDeriveKey(byte[] key, byte[] diversification)
        {
            byte[] input = new byte[8];
            Buffer.BlockCopy(diversification, 0, input, 0, 8);
            byte[] result = new byte[16];
            Buffer.BlockCopy(TripleDesEncrypt(key, input), 0, result, 0, 8);
            for (int index = 0; index < input.Length; index++) input[index] = (byte)~diversification[index];
            Buffer.BlockCopy(TripleDesEncrypt(key, input), 0, result, 8, 8);
            return result;
        }

        /// <summary>使用 16 字节密钥执行无填充 ECB 3DES 加密。</summary>
        /// <param name="key">16 字节 3DES 密钥。</param><param name="input">8 字节块或其倍数。</param><returns>与输入等长的密文。</returns>
        private static byte[] TripleDesEncrypt(byte[] key, byte[] input)
        {
            using (TripleDES algorithm = TripleDES.Create())
            {
                algorithm.Key = key;
                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.None;
                using (ICryptoTransform transform = algorithm.CreateEncryptor()) return transform.TransformFinalBlock(input, 0, input.Length);
            }
        }

        /// <summary>使用 16 字节密钥执行无填充 ECB 3DES 解密。</summary>
        /// <param name="key">16 字节 3DES 密钥。</param><param name="input">8 字节块或其倍数。</param>
        /// <returns>与输入等长的明文。</returns>
        private static byte[] TripleDesDecrypt(byte[] key, byte[] input)
        {
            using (TripleDES algorithm = TripleDES.Create())
            {
                algorithm.Key = key;
                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.None;
                using (ICryptoTransform transform = algorithm.CreateDecryptor()) return transform.TransformFinalBlock(input, 0, input.Length);
            }
        }

        /// <summary>按原 SingleMAC 的 XOR、0x80 填充和单 DES 加密规则生成 8 字节 MAC。</summary>
        /// <param name="key">8 字节单 DES 密钥。</param><param name="initialization">8 字节初始值。</param><param name="input">待计算的原文。</param><returns>8 字节 MAC。</returns>
        private static byte[] SingleDesCalculateMac(byte[] key, byte[] initialization, byte[] input)
        {
            byte[] state = (byte[])initialization.Clone();
            int blockCount = (input.Length / 8) + 1;
            byte[] padded = new byte[blockCount * 8];
            Buffer.BlockCopy(input, 0, padded, 0, input.Length);
            padded[input.Length] = 0x80;
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = key;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.None;
                using (ICryptoTransform transform = des.CreateEncryptor())
                {
                    byte[] block = new byte[8];
                    for (int offset = 0; offset < padded.Length; offset += 8)
                    {
                        for (int index = 0; index < 8; index++) block[index] = (byte)(state[index] ^ padded[offset + index]);
                        state = transform.TransformBlock(block, 0, 8, block, 0) == 8 ? (byte[])block.Clone() : state;
                    }
                }
            }
            return state;
        }

        /// <summary>按原 SM4DerivedKey 规则使用分散因子和按位取反分散。</summary>
        /// <param name="key">16 字节 SM4 主密钥。</param><param name="diversification">8 字节分散因子。</param><returns>16 字节派生密钥。</returns>
        private static byte[] Sm4DeriveKey(byte[] key, byte[] diversification)
        {
            byte[] input = new byte[16];
            Buffer.BlockCopy(diversification, 0, input, 0, 8);
            for (int index = 0; index < 8; index++) input[index + 8] = (byte)~diversification[index];
            return Sm4EncryptBlock(key, input);
        }

        /// <summary>按原 SM4CalcuMac 规则执行 CBC-MAC。</summary>
        /// <param name="key">16 字节 SM4 密钥。</param><param name="initialization">8 字节随机数初值。</param><param name="input">待计算原文。</param><returns>前 4 字节 MAC。</returns>
        private static byte[] Sm4CalculateMac(byte[] key, byte[] initialization, byte[] input)
        {
            byte[] iv = new byte[16];
            Buffer.BlockCopy(initialization, 0, iv, 0, 8);
            byte[] padded = new byte[((input.Length + 1 + 15) / 16) * 16];
            Buffer.BlockCopy(input, 0, padded, 0, input.Length);
            padded[input.Length] = 0x80;
            for (int offset = 0; offset < padded.Length; offset += 16)
            {
                byte[] block = new byte[16];
                for (int index = 0; index < 16; index++) block[index] = (byte)(iv[index] ^ padded[offset + index]);
                iv = Sm4EncryptBlock(key, block);
            }
            return TakeFirstFour(iv);
        }

        /// <summary>实现标准 SM4 单块加密。</summary>
        /// <param name="key">16 字节密钥。</param><param name="input">16 字节明文块。</param><returns>16 字节密文块。</returns>
        private static byte[] Sm4EncryptBlock(byte[] key, byte[] input)
        {
            if (key == null || key.Length != 16) throw new ArgumentException("SM4 密钥必须为 16 字节。", nameof(key));
            if (input == null || input.Length != 16) throw new ArgumentException("SM4 输入块必须为 16 字节。", nameof(input));
            uint[] roundKeys = new uint[32];
            uint[] keyWords = new uint[36];
            for (int index = 0; index < 4; index++) keyWords[index] = ReadUInt32(key, index * 4) ^ Sm4Fk[index];
            for (int index = 0; index < 32; index++)
            {
                uint keyInput = keyWords[index + 1] ^ keyWords[index + 2] ^ keyWords[index + 3] ^ Sm4Ck[index];
                roundKeys[index] = keyWords[index] ^ Sm4KeyTransform(keyInput);
                keyWords[index + 4] = roundKeys[index];
            }

            uint[] dataWords = new uint[36];
            for (int index = 0; index < 4; index++) dataWords[index] = ReadUInt32(input, index * 4);
            for (int index = 0; index < 32; index++)
            {
                uint dataInput = dataWords[index + 1] ^ dataWords[index + 2] ^ dataWords[index + 3] ^ roundKeys[index];
                dataWords[index + 4] = dataWords[index] ^ Sm4DataTransform(dataInput);
            }
            byte[] output = new byte[16];
            for (int index = 0; index < 4; index++) WriteUInt32(output, index * 4, dataWords[35 - index]);
            return output;
        }

        /// <summary>使用反向轮密钥执行标准SM4单块解密。</summary>
        /// <param name="key">16字节密钥。</param><param name="input">16字节密文块。</param><returns>16字节明文块。</returns>
        private static byte[] Sm4DecryptBlock(byte[] key, byte[] input)
        {
            if (key == null || key.Length != 16) throw new ArgumentException("SM4 密钥必须为 16 字节。", nameof(key));
            if (input == null || input.Length != 16) throw new ArgumentException("SM4 输入块必须为 16 字节。", nameof(input));
            uint[] roundKeys = new uint[32];
            uint[] keyWords = new uint[36];
            for (int index = 0; index < 4; index++) keyWords[index] = ReadUInt32(key, index * 4) ^ Sm4Fk[index];
            for (int index = 0; index < 32; index++)
            {
                uint keyInput = keyWords[index + 1] ^ keyWords[index + 2] ^ keyWords[index + 3] ^ Sm4Ck[index];
                roundKeys[index] = keyWords[index] ^ Sm4KeyTransform(keyInput);
                keyWords[index + 4] = roundKeys[index];
            }
            uint[] dataWords = new uint[36];
            for (int index = 0; index < 4; index++) dataWords[index] = ReadUInt32(input, index * 4);
            for (int index = 0; index < 32; index++)
            {
                uint dataInput = dataWords[index + 1] ^ dataWords[index + 2] ^ dataWords[index + 3] ^ roundKeys[31 - index];
                dataWords[index + 4] = dataWords[index] ^ Sm4DataTransform(dataInput);
            }
            byte[] output = new byte[16];
            WriteUInt32(output, 0, dataWords[35]);
            WriteUInt32(output, 4, dataWords[34]);
            WriteUInt32(output, 8, dataWords[33]);
            WriteUInt32(output, 12, dataWords[32]);
            return output;
        }

        /// <summary>执行 SM4 数据变换。</summary>
        /// <param name="value">32 位中间值。</param><returns>线性变换后的值。</returns>
        private static uint Sm4DataTransform(uint value)
        {
            uint substituted = Sm4Substitute(value);
            return substituted ^ RotateLeft(substituted, 2) ^ RotateLeft(substituted, 10) ^ RotateLeft(substituted, 18) ^ RotateLeft(substituted, 24);
        }

        /// <summary>执行 SM4 密钥扩展变换。</summary>
        /// <param name="value">32 位中间值。</param><returns>密钥扩展结果。</returns>
        private static uint Sm4KeyTransform(uint value)
        {
            uint substituted = Sm4Substitute(value);
            return substituted ^ RotateLeft(substituted, 13) ^ RotateLeft(substituted, 23);
        }

        /// <summary>对 32 位值的四个字节执行 SM4 S 盒替换。</summary>
        /// <param name="value">待替换值。</param><returns>替换后的值。</returns>
        private static uint Sm4Substitute(uint value)
        {
            int first = (int)((value >> 24) & 0xFFU);
            int second = (int)((value >> 16) & 0xFFU);
            int third = (int)((value >> 8) & 0xFFU);
            int fourth = (int)(value & 0xFFU);
            return ((uint)Sm4Sbox[first] << 24)
                | ((uint)Sm4Sbox[second] << 16)
                | ((uint)Sm4Sbox[third] << 8)
                | Sm4Sbox[fourth];
        }

        /// <summary>循环左移 32 位无符号整数。</summary>
        /// <param name="value">待移位值。</param><param name="count">移位位数。</param><returns>移位结果。</returns>
        private static uint RotateLeft(uint value, int count) { return (value << count) | (value >> (32 - count)); }

        /// <summary>以大端序读取 32 位整数。</summary>
        /// <param name="data">源数组。</param><param name="offset">起始偏移。</param><returns>整数值。</returns>
        private static uint ReadUInt32(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24)
                | ((uint)data[offset + 1] << 16)
                | ((uint)data[offset + 2] << 8)
                | data[offset + 3];
        }

        /// <summary>以大端序写入 32 位整数。</summary>
        /// <param name="data">目标数组。</param><param name="offset">起始偏移。</param><param name="value">待写入值。</param>
        private static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24); data[offset + 1] = (byte)(value >> 16); data[offset + 2] = (byte)(value >> 8); data[offset + 3] = (byte)value;
        }

        /// <summary>复制 MAC 的前四字节。</summary>
        /// <param name="data">完整 MAC。</param><returns>4 字节 MAC。</returns>
        private static byte[] TakeFirstFour(byte[] data) { byte[] result = new byte[4]; Buffer.BlockCopy(data, 0, result, 0, 4); return result; }
    }
}
