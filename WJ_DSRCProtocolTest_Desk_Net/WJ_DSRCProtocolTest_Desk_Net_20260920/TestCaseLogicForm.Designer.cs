namespace WJ_DSRCProtocolTest_Desk_Net
{
    partial class TestCaseLogicForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRoot;
        private System.Windows.Forms.Label labelTestCaseName;
        private System.Windows.Forms.RichTextBox richTextBoxLogic;
        private System.Windows.Forms.Button buttonClose;

        /// <summary>
        /// 释放测试逻辑说明弹窗持有的 Designer 组件资源。
        /// </summary>
        /// <param name="disposing">为 <see langword="true"/> 时释放托管组件容器。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化测试逻辑说明弹窗的标题、说明文本和关闭按钮。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.labelTestCaseName = new System.Windows.Forms.Label();
            this.richTextBoxLogic = new System.Windows.Forms.RichTextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.tableLayoutRoot.SuspendLayout();
            this.SuspendLayout();
            //
            // tableLayoutRoot
            //
            this.tableLayoutRoot.ColumnCount = 1;
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Controls.Add(this.labelTestCaseName, 0, 0);
            this.tableLayoutRoot.Controls.Add(this.richTextBoxLogic, 0, 1);
            this.tableLayoutRoot.Controls.Add(this.buttonClose, 0, 2);
            this.tableLayoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRoot.Padding = new System.Windows.Forms.Padding(16);
            this.tableLayoutRoot.RowCount = 3;
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutRoot.TabIndex = 0;
            //
            // labelTestCaseName
            //
            this.labelTestCaseName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTestCaseName.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.labelTestCaseName.ForeColor = System.Drawing.Color.FromArgb(28, 49, 78);
            this.labelTestCaseName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // richTextBoxLogic
            //
            this.richTextBoxLogic.BackColor = System.Drawing.Color.White;
            this.richTextBoxLogic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxLogic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxLogic.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.richTextBoxLogic.ReadOnly = true;
            this.richTextBoxLogic.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxLogic.TabIndex = 1;
            //
            // buttonClose
            //
            this.buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonClose.Size = new System.Drawing.Size(96, 32);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "关闭";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            //
            // TestCaseLogicForm
            //
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.tableLayoutRoot);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.Name = "TestCaseLogicForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "测试逻辑";
            this.tableLayoutRoot.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
