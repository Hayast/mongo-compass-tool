namespace mongo
{
    partial class CompassForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompassForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.treeViewCollections = new System.Windows.Forms.TreeView();
            this.textBoxCollectionFilter = new System.Windows.Forms.TextBox();
            this.panelRight = new System.Windows.Forms.Panel();
            this.tabControlData = new System.Windows.Forms.TabControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuItemConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemReconnect = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemNativeQuery = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEnglish = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSimplifiedChinese = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemTraditionalChinese = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemLanguageSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemLanguageSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelLeft);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelRight);
            this.splitContainer1.Size = new System.Drawing.Size(1200, 654);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.TabIndex = 2;
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.White;
            this.panelLeft.Controls.Add(this.treeViewCollections);
            this.panelLeft.Controls.Add(this.textBoxCollectionFilter);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(300, 654);
            this.panelLeft.TabIndex = 0;
            // 
            // treeViewCollections
            // 
            this.treeViewCollections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewCollections.Location = new System.Drawing.Point(0, 29);
            this.treeViewCollections.Name = "treeViewCollections";
            this.treeViewCollections.Size = new System.Drawing.Size(300, 625);
            this.treeViewCollections.TabIndex = 0;
            this.treeViewCollections.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCollections_AfterSelect);
            this.treeViewCollections.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewCollections_NodeMouseClick);
            this.treeViewCollections.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewCollections_NodeMouseDoubleClick);
            // 
            // textBoxCollectionFilter
            // 
            this.textBoxCollectionFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBoxCollectionFilter.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.textBoxCollectionFilter.Location = new System.Drawing.Point(0, 0);
            this.textBoxCollectionFilter.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxCollectionFilter.Name = "textBoxCollectionFilter";
            this.textBoxCollectionFilter.Size = new System.Drawing.Size(300, 29);
            this.textBoxCollectionFilter.TabIndex = 1;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.tabControlData);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(896, 654);
            this.panelRight.TabIndex = 0;
            // 
            // tabControlData
            // 
            this.tabControlData.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlData.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.tabControlData.ItemSize = new System.Drawing.Size(120, 30);
            this.tabControlData.Location = new System.Drawing.Point(0, 0);
            this.tabControlData.Name = "tabControlData";
            this.tabControlData.SelectedIndex = 0;
            this.tabControlData.Size = new System.Drawing.Size(896, 654);
            this.tabControlData.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlData.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemConnection,
            this.menuItemTools,
            this.menuItemLanguage});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1200, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuItemConnection
            // 
            this.menuItemConnection.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemConnect,
            this.menuItemReconnect,
            this.menuItemRefresh});
            this.menuItemConnection.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemConnection.Name = "menuItemConnection";
            this.menuItemConnection.Size = new System.Drawing.Size(44, 20);
            this.menuItemConnection.Text = "连接";
            // 
            // menuItemConnect
            // 
            this.menuItemConnect.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemConnect.Name = "menuItemConnect";
            this.menuItemConnect.Size = new System.Drawing.Size(100, 22);
            this.menuItemConnect.Text = "连接配置";
            this.menuItemConnect.Click += new System.EventHandler(this.toolStripButtonConnect_Click);
            // 
            // menuItemReconnect
            // 
            this.menuItemReconnect.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemReconnect.Name = "menuItemReconnect";
            this.menuItemReconnect.Size = new System.Drawing.Size(100, 22);
            this.menuItemReconnect.Text = "重新连接";
            this.menuItemReconnect.Click += new System.EventHandler(this.toolStripButtonReconnect_Click);
            // 
            // menuItemRefresh
            // 
            this.menuItemRefresh.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemRefresh.Name = "menuItemRefresh";
            this.menuItemRefresh.Size = new System.Drawing.Size(100, 22);
            this.menuItemRefresh.Text = "刷新";
            this.menuItemRefresh.Click += new System.EventHandler(this.toolStripButtonRefresh_Click);
            // 
            // menuItemTools
            // 
            this.menuItemTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNativeQuery});
            this.menuItemTools.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemTools.Name = "menuItemTools";
            this.menuItemTools.Size = new System.Drawing.Size(44, 20);
            this.menuItemTools.Text = "工具";
            // 
            // menuItemNativeQuery
            // 
            this.menuItemNativeQuery.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemNativeQuery.Name = "menuItemNativeQuery";
            this.menuItemNativeQuery.Size = new System.Drawing.Size(100, 22);
            this.menuItemNativeQuery.Text = "原生语句";
            this.menuItemNativeQuery.Click += new System.EventHandler(this.ToolStripButtonNativeQuery_Click);
            // 
            // menuItemLanguage
            // 
            this.menuItemLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemEnglish,
            this.menuItemSimplifiedChinese,
            this.menuItemTraditionalChinese,
            this.menuItemLanguageSeparator,
            this.menuItemLanguageSettings});
            this.menuItemLanguage.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemLanguage.Name = "menuItemLanguage";
            this.menuItemLanguage.Size = new System.Drawing.Size(44, 20);
            this.menuItemLanguage.Text = "语言";
            // 
            // menuItemEnglish
            // 
            this.menuItemEnglish.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemEnglish.Name = "menuItemEnglish";
            this.menuItemEnglish.Size = new System.Drawing.Size(100, 22);
            this.menuItemEnglish.Text = "英语";
            this.menuItemEnglish.Click += new System.EventHandler(this.MenuItemEnglish_Click);
            // 
            // menuItemSimplifiedChinese
            // 
            this.menuItemSimplifiedChinese.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemSimplifiedChinese.Name = "menuItemSimplifiedChinese";
            this.menuItemSimplifiedChinese.Size = new System.Drawing.Size(100, 22);
            this.menuItemSimplifiedChinese.Text = "简体中文";
            this.menuItemSimplifiedChinese.Click += new System.EventHandler(this.MenuItemSimplifiedChinese_Click);
            // 
            // menuItemTraditionalChinese
            // 
            this.menuItemTraditionalChinese.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemTraditionalChinese.Name = "menuItemTraditionalChinese";
            this.menuItemTraditionalChinese.Size = new System.Drawing.Size(100, 22);
            this.menuItemTraditionalChinese.Text = "繁体中文";
            this.menuItemTraditionalChinese.Click += new System.EventHandler(this.MenuItemTraditionalChinese_Click);
            // 
            // menuItemLanguageSeparator
            // 
            this.menuItemLanguageSeparator.Name = "menuItemLanguageSeparator";
            this.menuItemLanguageSeparator.Size = new System.Drawing.Size(100, 6);
            // 
            // menuItemLanguageSettings
            // 
            this.menuItemLanguageSettings.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.menuItemLanguageSettings.Name = "menuItemLanguageSettings";
            this.menuItemLanguageSettings.Size = new System.Drawing.Size(100, 22);
            this.menuItemLanguageSettings.Text = "语言设置";
            this.menuItemLanguageSettings.Click += new System.EventHandler(this.MenuItemLanguageSettings_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.statusStrip1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 678);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 16);
            // 
            // CompassForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CompassForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MongoDB Compass";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.TreeView treeViewCollections;
        private System.Windows.Forms.TabControl tabControlData;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuItemConnection;
        private System.Windows.Forms.ToolStripMenuItem menuItemConnect;
        private System.Windows.Forms.ToolStripMenuItem menuItemReconnect;
        private System.Windows.Forms.ToolStripMenuItem menuItemRefresh;
        private System.Windows.Forms.ToolStripMenuItem menuItemTools;
        private System.Windows.Forms.ToolStripMenuItem menuItemNativeQuery;
        private System.Windows.Forms.ToolStripMenuItem menuItemLanguage;
        private System.Windows.Forms.ToolStripMenuItem menuItemEnglish;
        private System.Windows.Forms.ToolStripMenuItem menuItemSimplifiedChinese;
        private System.Windows.Forms.ToolStripMenuItem menuItemTraditionalChinese;
        private System.Windows.Forms.ToolStripSeparator menuItemLanguageSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuItemLanguageSettings;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox textBoxCollectionFilter;
    }
} 