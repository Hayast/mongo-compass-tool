namespace mongo
{
    partial class ConnListForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.labelHost = new System.Windows.Forms.Label();
            this.textBoxHost = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelDatabase = new System.Windows.Forms.Label();
            this.textBoxDatabase = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.listBoxConnections = new System.Windows.Forms.ListBox();
            this.buttonAddConn = new System.Windows.Forms.Button();
            this.buttonCopyConn = new System.Windows.Forms.Button();
            this.buttonEditConn = new System.Windows.Forms.Button();
            this.buttonDeleteConn = new System.Windows.Forms.Button();
            this.comboBoxAuthMechanism = new System.Windows.Forms.ComboBox();
            this.checkBoxAuthSource = new System.Windows.Forms.CheckBox();
            this.textBoxAuthSource = new System.Windows.Forms.TextBox();
            this.buttonOpenCompass = new System.Windows.Forms.Button();
            this.buttonBackup = new System.Windows.Forms.Button();
            this.buttonRestore = new System.Windows.Forms.Button();
            this.buttonCreateDatabase = new System.Windows.Forms.Button();

            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelHost
            // 
            this.labelHost.AutoSize = true;
            this.labelHost.Location = new System.Drawing.Point(40, 38);
            this.labelHost.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelHost.Name = "labelHost";
            this.labelHost.Size = new System.Drawing.Size(52, 15);
            this.labelHost.TabIndex = 0;
            this.labelHost.Text = "主机：";
            // 
            // textBoxHost
            // 
            this.textBoxHost.Location = new System.Drawing.Point(120, 34);
            this.textBoxHost.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxHost.Name = "textBoxHost";
            this.textBoxHost.Size = new System.Drawing.Size(199, 25);
            this.textBoxHost.TabIndex = 1;
            this.textBoxHost.Text = "localhost";
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(40, 81);
            this.labelPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(52, 15);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "端口：";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(120, 78);
            this.textBoxPort.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(199, 25);
            this.textBoxPort.TabIndex = 3;
            this.textBoxPort.Text = "27017";
            // 
            // labelDatabase
            // 
            this.labelDatabase.AutoSize = true;
            this.labelDatabase.Location = new System.Drawing.Point(40, 125);
            this.labelDatabase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDatabase.Name = "labelDatabase";
            this.labelDatabase.Size = new System.Drawing.Size(67, 15);
            this.labelDatabase.TabIndex = 4;
            this.labelDatabase.Text = "数据库：";
            // 
            // textBoxDatabase
            // 
            this.textBoxDatabase.Location = new System.Drawing.Point(120, 121);
            this.textBoxDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDatabase.Name = "textBoxDatabase";
            this.textBoxDatabase.Size = new System.Drawing.Size(199, 25);
            this.textBoxDatabase.TabIndex = 5;
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(40, 169);
            this.labelUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(67, 15);
            this.labelUsername.TabIndex = 6;
            this.labelUsername.Text = "用户名：";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(120, 165);
            this.textBoxUsername.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(199, 25);
            this.textBoxUsername.TabIndex = 7;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(40, 212);
            this.labelPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(52, 15);
            this.labelPassword.TabIndex = 8;
            this.labelPassword.Text = "密码：";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(120, 209);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(199, 25);
            this.textBoxPassword.TabIndex = 9;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(417, 347);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(4);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(200, 29);
            this.buttonConnect.TabIndex = 10;
            this.buttonConnect.Text = "连接";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Visible = false;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.ForeColor = System.Drawing.Color.Blue;
            this.labelStatus.Location = new System.Drawing.Point(40, 312);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 15);
            this.labelStatus.TabIndex = 11;
            // 
            // listBoxConnections
            // 
            this.listBoxConnections.ItemHeight = 15;
            this.listBoxConnections.Location = new System.Drawing.Point(360, 34);
            this.listBoxConnections.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxConnections.Name = "listBoxConnections";
            this.listBoxConnections.Size = new System.Drawing.Size(521, 229);
            this.listBoxConnections.TabIndex = 20;
            this.listBoxConnections.SelectedIndexChanged += new System.EventHandler(this.listBoxConnections_SelectedIndexChanged);
            // 
            // buttonAddConn
            // 
            this.buttonAddConn.Location = new System.Drawing.Point(359, 278);
            this.buttonAddConn.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddConn.Name = "buttonAddConn";
            this.buttonAddConn.Size = new System.Drawing.Size(73, 29);
            this.buttonAddConn.TabIndex = 21;
            this.buttonAddConn.Text = "添加";
            this.buttonAddConn.UseVisualStyleBackColor = true;
            this.buttonAddConn.Click += new System.EventHandler(this.buttonAddConn_Click);
            // 
            // buttonCopyConn
            // 
            this.buttonCopyConn.Location = new System.Drawing.Point(446, 278);
            this.buttonCopyConn.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCopyConn.Name = "buttonCopyConn";
            this.buttonCopyConn.Size = new System.Drawing.Size(73, 29);
            this.buttonCopyConn.TabIndex = 22;
            this.buttonCopyConn.Text = "复制";
            this.buttonCopyConn.UseVisualStyleBackColor = true;
            this.buttonCopyConn.Click += new System.EventHandler(this.buttonCopyConn_Click);
            // 
            // buttonEditConn
            // 
            this.buttonEditConn.Location = new System.Drawing.Point(532, 278);
            this.buttonEditConn.Margin = new System.Windows.Forms.Padding(4);
            this.buttonEditConn.Name = "buttonEditConn";
            this.buttonEditConn.Size = new System.Drawing.Size(73, 29);
            this.buttonEditConn.TabIndex = 23;
            this.buttonEditConn.Text = "修改";
            this.buttonEditConn.UseVisualStyleBackColor = true;
            this.buttonEditConn.Click += new System.EventHandler(this.buttonEditConn_Click);
            // 
            // buttonDeleteConn
            // 
            this.buttonDeleteConn.Location = new System.Drawing.Point(619, 278);
            this.buttonDeleteConn.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDeleteConn.Name = "buttonDeleteConn";
            this.buttonDeleteConn.Size = new System.Drawing.Size(73, 29);
            this.buttonDeleteConn.TabIndex = 24;
            this.buttonDeleteConn.Text = "删除";
            this.buttonDeleteConn.UseVisualStyleBackColor = true;
            this.buttonDeleteConn.Click += new System.EventHandler(this.buttonDeleteConn_Click);
            // 
            // comboBoxAuthMechanism
            // 
            this.comboBoxAuthMechanism.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAuthMechanism.Items.AddRange(new object[] {
            "(默认)",
            "SCRAM-SHA-1",
            "SCRAM-SHA-256",
            "MONGODB-CR"});
            this.comboBoxAuthMechanism.Location = new System.Drawing.Point(120, 250);
            this.comboBoxAuthMechanism.Name = "comboBoxAuthMechanism";
            this.comboBoxAuthMechanism.Size = new System.Drawing.Size(200, 23);
            this.comboBoxAuthMechanism.TabIndex = 25;
            // 
            // checkBoxAuthSource
            // 
            this.checkBoxAuthSource.Location = new System.Drawing.Point(39, 287);
            this.checkBoxAuthSource.Name = "checkBoxAuthSource";
            this.checkBoxAuthSource.Size = new System.Drawing.Size(100, 20);
            this.checkBoxAuthSource.TabIndex = 26;
            this.checkBoxAuthSource.Text = "使用authSource";
            // 
            // textBoxAuthSource
            // 
            this.textBoxAuthSource.Enabled = false;
            this.textBoxAuthSource.Location = new System.Drawing.Point(149, 285);
            this.textBoxAuthSource.Name = "textBoxAuthSource";
            this.textBoxAuthSource.Size = new System.Drawing.Size(170, 25);
            this.textBoxAuthSource.TabIndex = 27;
            // 
            // buttonCreateDatabase
            // 
            this.buttonCreateDatabase.Location = new System.Drawing.Point(797, 278);
            this.buttonCreateDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCreateDatabase.Name = "buttonCreateDatabase";
            this.buttonCreateDatabase.Size = new System.Drawing.Size(84, 29);
            this.buttonCreateDatabase.TabIndex = 28;
            this.buttonCreateDatabase.Text = "创建数据库";
            this.buttonCreateDatabase.UseVisualStyleBackColor = true;
            this.buttonCreateDatabase.Click += new System.EventHandler(this.buttonCreateDatabase_Click);
            // 
            // buttonOpenCompass
            // 
            this.buttonOpenCompass.Location = new System.Drawing.Point(120, 347);
            this.buttonOpenCompass.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOpenCompass.Name = "buttonOpenCompass";
            this.buttonOpenCompass.Size = new System.Drawing.Size(200, 29);
            this.buttonOpenCompass.TabIndex = 30;
            this.buttonOpenCompass.Text = "确定";
            this.buttonOpenCompass.UseVisualStyleBackColor = true;
            this.buttonOpenCompass.Click += new System.EventHandler(this.buttonOpenCompass_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 417);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(912, 22);
            this.statusStrip1.TabIndex = 29;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 16);
            // 
            // ConnListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 439);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.buttonOpenCompass);
            this.Controls.Add(this.comboBoxAuthMechanism);
            this.Controls.Add(this.checkBoxAuthSource);
            this.Controls.Add(this.textBoxAuthSource);
            this.Controls.Add(this.buttonCreateDatabase);
            this.Controls.Add(this.buttonDeleteConn);
            this.Controls.Add(this.buttonEditConn);
            this.Controls.Add(this.buttonCopyConn);
            this.Controls.Add(this.buttonAddConn);
            this.Controls.Add(this.listBoxConnections);
            this.Controls.Add(this.labelHost);
            this.Controls.Add(this.textBoxHost);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.labelDatabase);
            this.Controls.Add(this.textBoxDatabase);
            this.Controls.Add(this.labelUsername);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.labelStatus);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ConnListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MongoDB连接配置";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelHost;
        private System.Windows.Forms.TextBox textBoxHost;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelDatabase;
        private System.Windows.Forms.TextBox textBoxDatabase;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ListBox listBoxConnections;
        private System.Windows.Forms.Button buttonAddConn;
        private System.Windows.Forms.Button buttonCopyConn;
        private System.Windows.Forms.Button buttonEditConn;
        private System.Windows.Forms.Button buttonDeleteConn;
        private System.Windows.Forms.ComboBox comboBoxAuthMechanism;
        private System.Windows.Forms.CheckBox checkBoxAuthSource;
        private System.Windows.Forms.TextBox textBoxAuthSource;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button buttonOpenCompass;
        private System.Windows.Forms.Button buttonBackup;
        private System.Windows.Forms.Button buttonRestore;
        private System.Windows.Forms.Button buttonCreateDatabase;

    }
}

