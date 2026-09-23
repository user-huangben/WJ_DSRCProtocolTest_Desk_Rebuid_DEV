using System;
using System.Windows.Forms;

namespace WJ_ComprehensiveTest_Desk
{
    /// <summary>
    /// 显示单个测试用例的测试逻辑说明；当前窗体只负责展示，不执行设备通信。
    /// </summary>
    public partial class TestCaseLogicForm : Form
    {
        /// <summary>
        /// 创建测试逻辑说明弹窗。
        /// </summary>
        /// <param name="testCaseName">测试用例名称；不能为空或空白。</param>
        /// <param name="logicText">要展示的测试逻辑、关键帧和成功判定说明。</param>
        /// <exception cref="ArgumentException">测试用例名称为空或逻辑文本为空时抛出。</exception>
        public TestCaseLogicForm(string testCaseName, string logicText)
        {
            if (string.IsNullOrWhiteSpace(testCaseName))
            {
                throw new ArgumentException("测试用例名称不能为空。", nameof(testCaseName));
            }

            if (string.IsNullOrWhiteSpace(logicText))
            {
                throw new ArgumentException("测试逻辑说明不能为空。", nameof(logicText));
            }

            // 构造入口先加载 Designer 控件，再把当前用例名称和逻辑文本填入显示区。
            InitializeComponent();
            Text = "测试逻辑 - " + testCaseName;
            labelTestCaseName.Text = testCaseName;
            richTextBoxLogic.Text = logicText;
        }

        /// <summary>
        /// 关闭当前测试逻辑说明弹窗。
        /// </summary>
        /// <param name="sender">Designer 绑定的关闭按钮。</param>
        /// <param name="e">按钮单击事件数据。</param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            // 关闭入口只结束说明弹窗，不影响外层测试窗体和台发通信状态。
            Close();
        }
    }
}
