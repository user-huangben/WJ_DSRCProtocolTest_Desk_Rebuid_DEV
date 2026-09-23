using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WJ_ComprehensiveTest_Desk.Services
{
    /// <summary>
    /// 将测试大框当前界面保存为 PNG 截图。
    /// </summary>
    internal static class TestResultScreenshotWriter
    {
        /// <summary>
        /// 截取指定测试窗体的完整客户区显示内容，并保存到 exe 同级 TestScreenshots 目录。
        /// </summary>
        /// <param name="testCaseName">测试用例显示名称；用于文件名。</param>
        /// <param name="testForm">当前测试大框窗体；调用时必须已经完成布局并处于可绘制状态。</param>
        /// <param name="isSuccess">测试最终是否成功，用于文件名。</param>
        /// <param name="testTime">测试完成时间，用于文件名。</param>
        /// <returns>实际生成的 PNG 文件绝对路径。</returns>
        /// <exception cref="ArgumentException">测试用例名称为空或测试窗体为空时抛出。</exception>
        /// <exception cref="IOException">截图目录或文件无法写入时抛出。</exception>
        internal static string CaptureTestForm(string testCaseName, Form testForm, bool isSuccess, DateTime testTime)
        {
            if (string.IsNullOrWhiteSpace(testCaseName))
            {
                throw new ArgumentException("测试用例名称不能为空。", nameof(testCaseName));
            }

            if (testForm == null)
            {
                throw new ArgumentException("测试窗体不能为空。", nameof(testForm));
            }

            string screenshotDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestScreenshots");
            // 截图入口在测试完成后创建目录，未勾选存储时不产生运行目录。
            Directory.CreateDirectory(screenshotDirectory);

            string statusName = isSuccess ? "成功" : "失败";
            string fileName = SanitizeFileName(RemoveTestCaseNumber(testCaseName))
                + "_"
                + statusName
                + "_"
                + testTime.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)
                + "_万集"
                + ".png";
            string filePath = Path.Combine(screenshotDirectory, fileName);

            using (Bitmap bitmap = new Bitmap(testForm.Width, testForm.Height))
            {
                // DrawToBitmap 捕获当前测试大框的完整界面，包含测试项、操作选项和测试信息区域。
                testForm.DrawToBitmap(bitmap, new Rectangle(Point.Empty, testForm.Size));
                bitmap.Save(filePath, ImageFormat.Png);
            }

            return filePath;
        }

        /// <summary>
        /// 替换 Windows 文件名不允许的字符，保留中文测试用例名称的可读性。
        /// </summary>
        /// <param name="fileName">原始测试用例名称。</param>
        /// <returns>可用作截图文件名的字符串。</returns>
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

        /// <summary>
        /// 去掉用例或测试分组名称开头的编号，例如“3、”“用例2：”和“测试用例12：”。
        /// </summary>
        /// <param name="testCaseName">界面显示的原始用例名称。</param>
        /// <returns>不含开头编号的名称；无法识别编号时返回去除首尾空白后的原名称。</returns>
        private static string RemoveTestCaseNumber(string testCaseName)
        {
            string name = testCaseName.Trim();
            string withoutNumber = Regex.Replace(
                name,
                @"^\s*(?:(?:测试)?用例\s*)?\d+\s*[、：:._\-]\s*",
                string.Empty,
                RegexOptions.CultureInvariant);
            return string.IsNullOrWhiteSpace(withoutNumber) ? name : withoutNumber.Trim();
        }
    }
}
