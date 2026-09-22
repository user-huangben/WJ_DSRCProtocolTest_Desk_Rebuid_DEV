using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WJ_DSRCProtocolTest_Desk_Net.Services
{
    /// <summary>
    /// 将测试用例的最终结果和完整空中交互帧保存到 exe 同级的 TestResults 目录。
    /// </summary>
    internal static class TestResultFileWriter
    {
        /// <summary>
        /// 写入一次测试用例的结构化测试结果文件。
        /// </summary>
        /// <param name="testCaseName">测试用例显示名称；用于文件名和文件内容。</param>
        /// <param name="isSuccess">最终测试是否成功。</param>
        /// <param name="message">最终结果说明或失败原因。</param>
        /// <param name="version">读取到的 OBU 版本号；失败时可以为空。</param>
        /// <param name="exchanges">按实际调用顺序排列的完整请求/响应帧。</param>
        /// <param name="startedAt">测试开始时间。</param>
        /// <param name="completedAt">测试完成时间。</param>
        /// <returns>实际生成的结果文件绝对路径。</returns>
        /// <exception cref="IOException">结果目录或文件无法写入时抛出。</exception>
        /// <exception cref="UnauthorizedAccessException">当前进程没有结果目录写入权限时抛出。</exception>
        internal static string WriteTestResult(
            string testCaseName,
            bool isSuccess,
            string message,
            string version,
            IList<ObuProtocolExchange> exchanges,
            DateTime startedAt,
            DateTime completedAt)
        {
            if (string.IsNullOrWhiteSpace(testCaseName))
            {
                throw new ArgumentException("测试用例名称不能为空。", nameof(testCaseName));
            }

            string resultDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults");
            // 结果文件入口在测试完成后创建目录，避免未执行测试时生成空目录。
            Directory.CreateDirectory(resultDirectory);

            string resultText = BuildResultText(testCaseName, isSuccess, message, version, exchanges, startedAt, completedAt);
            string statusName = isSuccess ? "测试成功" : "测试失败";
            string fileName = startedAt.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture)
                + "_" + SanitizeFileName(testCaseName)
                + "_" + statusName
                + ".txt";
            string filePath = Path.Combine(resultDirectory, fileName);
            File.WriteAllText(filePath, resultText, new UTF8Encoding(false));
            return filePath;
        }

        /// <summary>
        /// 生成结果文件正文，保留每一组请求/响应的长度、十六进制内容和 DLL 返回码。
        /// </summary>
        /// <param name="testCaseName">测试用例显示名称。</param>
        /// <param name="isSuccess">最终测试是否成功。</param>
        /// <param name="message">最终结果说明或失败原因。</param>
        /// <param name="version">读取到的 OBU 版本号。</param>
        /// <param name="exchanges">完整空中交互记录。</param>
        /// <param name="startedAt">测试开始时间。</param>
        /// <param name="completedAt">测试完成时间。</param>
        /// <returns>UTF-8 文本内容。</returns>
        private static string BuildResultText(
            string testCaseName,
            bool isSuccess,
            string message,
            string version,
            IList<ObuProtocolExchange> exchanges,
            DateTime startedAt,
            DateTime completedAt)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DSRC 上位机测试结果");
            builder.AppendLine("测试用例：" + testCaseName);
            builder.AppendLine("开始时间：" + startedAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.AppendLine("结束时间：" + completedAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.AppendLine("最终测试结果：" + (isSuccess ? "测试成功" : "测试失败"));
            builder.AppendLine("结果说明：" + (message ?? string.Empty));
            builder.AppendLine("OBU版本号：" + (string.IsNullOrEmpty(version) ? "未读取" : version));
            builder.AppendLine();
            builder.AppendLine("完整空中交互帧：");

            if (exchanges == null || exchanges.Count == 0)
            {
                builder.AppendLine("（未获得有效交互帧）");
            }
            else
            {
                for (int index = 0; index < exchanges.Count; index++)
                {
                    // 结果文件按 DLL 实际调用顺序展开每一组上行和下行帧。
                    AppendExchange(builder, index + 1, exchanges[index]);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// 将一组透传交互追加到结果文本。
        /// </summary>
        /// <param name="builder">目标文本构造器。</param>
        /// <param name="sequence">交互序号，从 1 开始。</param>
        /// <param name="exchange">待写入的请求/响应记录。</param>
        private static void AppendExchange(StringBuilder builder, int sequence, ObuProtocolExchange exchange)
        {
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", sequence, exchange.CommandName));
            if (!exchange.ResponseResult.HasValue)
            {
                // EventReport 等单向帧只记录台发下行发送，不生成 OBU 响应或零长度响应占位行。
                builder.AppendLine("  下行发送 Len=" + exchange.RequestData.Length + "：" + FormatBytes(exchange.RequestData));
                builder.AppendLine("  DLL发送返回码：" + exchange.RequestResult);
                return;
            }

            builder.AppendLine("  上行请求 Len=" + exchange.RequestData.Length + "：" + FormatBytes(exchange.RequestData));
            builder.AppendLine("  下行响应 Len=" + exchange.ResponseData.Length + "：" + FormatBytes(exchange.ResponseData));
            builder.AppendLine("  DLL请求返回码：" + exchange.RequestResult);
            builder.AppendLine("  DLL响应返回码：" + exchange.ResponseResult.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 将帧字节按大写十六进制和空格格式化。
        /// </summary>
        /// <param name="data">待格式化的帧字节。</param>
        /// <returns>适合人工查看和复制的十六进制字符串。</returns>
        private static string FormatBytes(byte[] data)
        {
            return data == null || data.Length == 0
                ? "（空）"
                : BitConverter.ToString(data).Replace("-", " ");
        }

        /// <summary>
        /// 替换 Windows 文件名不允许的字符，保留中文用例名称可读性。
        /// </summary>
        /// <param name="fileName">原始用例名称。</param>
        /// <returns>可用作文件名片段的字符串。</returns>
        private static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder(fileName.Length);
            foreach (char character in fileName)
            {
                builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
            }

            return builder.ToString();
        }
    }
}
