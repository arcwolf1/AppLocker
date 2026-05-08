namespace AppLocker
{
    partial class ManagerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox listBoxPrograms;
        private System.Windows.Forms.Button btnReplaceExe;
        private System.Windows.Forms.Button btnRestoreExe;
        private System.Windows.Forms.Button btnChangePassword;
        private System.Windows.Forms.Button btnCreateShortcut;
        private System.Windows.Forms.Label labelTitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManagerForm));
            this.listBoxPrograms = new System.Windows.Forms.ListBox();
            this.btnReplaceExe = new System.Windows.Forms.Button();
            this.btnRestoreExe = new System.Windows.Forms.Button();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.btnCreateShortcut = new System.Windows.Forms.Button();
            this.labelTitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listBoxPrograms
            // 
            this.listBoxPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPrograms.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.listBoxPrograms.FormattingEnabled = true;
            this.listBoxPrograms.ItemHeight = 21;
            this.listBoxPrograms.Location = new System.Drawing.Point(12, 38);
            this.listBoxPrograms.Name = "listBoxPrograms";
            this.listBoxPrograms.Size = new System.Drawing.Size(760, 340);
            this.listBoxPrograms.TabIndex = 0;
            // 
            // btnReplaceExe
            // 
            this.btnReplaceExe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReplaceExe.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnReplaceExe.Location = new System.Drawing.Point(12, 397);
            this.btnReplaceExe.Name = "btnReplaceExe";
            this.btnReplaceExe.Size = new System.Drawing.Size(100, 40);
            this.btnReplaceExe.TabIndex = 5;
            this.btnReplaceExe.Text = "加密程序";
            this.btnReplaceExe.UseVisualStyleBackColor = true;
            this.btnReplaceExe.Click += new System.EventHandler(this.btnReplaceExe_Click);
            // 
            // btnRestoreExe
            // 
            this.btnRestoreExe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRestoreExe.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnRestoreExe.Location = new System.Drawing.Point(122, 397);
            this.btnRestoreExe.Name = "btnRestoreExe";
            this.btnRestoreExe.Size = new System.Drawing.Size(100, 40);
            this.btnRestoreExe.TabIndex = 6;
            this.btnRestoreExe.Text = "还原程序";
            this.btnRestoreExe.UseVisualStyleBackColor = true;
            this.btnRestoreExe.Click += new System.EventHandler(this.btnRestoreExe_Click);
            // 
            // btnChangePassword
            // 
            this.btnChangePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnChangePassword.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnChangePassword.Location = new System.Drawing.Point(232, 397);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(100, 40);
            this.btnChangePassword.TabIndex = 7;
            this.btnChangePassword.Text = "修改密码";
            this.btnChangePassword.UseVisualStyleBackColor = true;
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);
            // 
            // btnCreateShortcut
            // 
            this.btnCreateShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreateShortcut.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnCreateShortcut.Location = new System.Drawing.Point(342, 397);
            this.btnCreateShortcut.Name = "btnCreateShortcut";
            this.btnCreateShortcut.Size = new System.Drawing.Size(100, 40);
            this.btnCreateShortcut.TabIndex = 3;
            this.btnCreateShortcut.Text = "快捷方式";
            this.btnCreateShortcut.UseVisualStyleBackColor = true;
            this.btnCreateShortcut.Click += new System.EventHandler(this.btnCreateShortcut_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(12, 9);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(132, 26);
            this.labelTitle.TabIndex = 4;
            this.labelTitle.Text = "已加密程序：";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(256, 441);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(272, 31);
            this.label1.TabIndex = 8;
            this.label1.Text = "Copyright Arcwolf - 2026";
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 481);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.btnChangePassword);
            this.Controls.Add(this.btnRestoreExe);
            this.Controls.Add(this.btnReplaceExe);
            this.Controls.Add(this.btnCreateShortcut);
            this.Controls.Add(this.listBoxPrograms);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "应用锁 By Arcwolf";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label label1;
    }
}