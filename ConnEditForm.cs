using System.Windows.Forms;

namespace mongo
{
    public class ConnEditForm : Form
    {
        public TextBox textBoxName = new TextBox();
        public TextBox textBoxHost = new TextBox();
        public TextBox textBoxPort = new TextBox();
        public TextBox textBoxDatabase = new TextBox();
        public TextBox textBoxUsername = new TextBox();
        public TextBox textBoxPassword = new TextBox();
        public ComboBox comboBoxAuthMechanism = new ComboBox();
        public Button buttonOK = new Button();
        public Button buttonCancel = new Button();
        public CheckBox checkBoxAuthSource = new CheckBox();
        public TextBox textBoxAuthSource = new TextBox();
        public ConnEditForm(string title, MongoConnInfo info = null)
        {
            this.Text = title;
            this.Width = 320;
            this.Height = 440;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // 创建标签和控件
            Label labelName = new Label() { Text = LanguageManager.GetString("label_connection_name", "连接名称:"), Left = 20, Top = 20, Width = 70 };
            textBoxName.Left = 100; textBoxName.Top = 20; textBoxName.Width = 180;
            
            Label labelHost = new Label() { Text = LanguageManager.GetString("label_host", "主机:"), Left = 20, Top = 55, Width = 70 };
            textBoxHost.Left = 100; textBoxHost.Top = 55; textBoxHost.Width = 180;
            
            Label labelPort = new Label() { Text = LanguageManager.GetString("label_port", "端口:"), Left = 20, Top = 90, Width = 70 };
            textBoxPort.Left = 100; textBoxPort.Top = 90; textBoxPort.Width = 180;
            
            Label labelDatabase = new Label() { Text = LanguageManager.GetString("label_database", "数据库:"), Left = 20, Top = 125, Width = 70 };
            textBoxDatabase.Left = 100; textBoxDatabase.Top = 125; textBoxDatabase.Width = 180;
            
            Label labelUsername = new Label() { Text = LanguageManager.GetString("label_username", "用户名:"), Left = 20, Top = 160, Width = 70 };
            textBoxUsername.Left = 100; textBoxUsername.Top = 160; textBoxUsername.Width = 180;
            
            Label labelPassword = new Label() { Text = LanguageManager.GetString("label_password", "密码:"), Left = 20, Top = 195, Width = 70 };
            textBoxPassword.Left = 100; textBoxPassword.Top = 195; textBoxPassword.Width = 180; textBoxPassword.PasswordChar = '*';
            
            Label labelAuthMech = new Label() { Text = LanguageManager.GetString("label_auth_mechanism", "认证机制:"), Left = 20, Top = 230, Width = 70 };
            comboBoxAuthMechanism.Left = 100; comboBoxAuthMechanism.Top = 230; comboBoxAuthMechanism.Width = 180;
            comboBoxAuthMechanism.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxAuthMechanism.Items.AddRange(new object[] { 
                LanguageManager.GetString("text_default", "(默认)"), 
                "SCRAM-SHA-1", 
                "SCRAM-SHA-256", 
                "MONGODB-CR" 
            });
            comboBoxAuthMechanism.SelectedIndex = 0;
            
            checkBoxAuthSource.Text = LanguageManager.GetString("text_use_auth_source", "使用authSource");
            checkBoxAuthSource.Left = 20; checkBoxAuthSource.Top = 270; checkBoxAuthSource.Width = 120;
            textBoxAuthSource.Left = 140; textBoxAuthSource.Top = 268; textBoxAuthSource.Width = 140;
            textBoxAuthSource.Enabled = false;
            checkBoxAuthSource.CheckedChanged += (s, e) => { textBoxAuthSource.Enabled = checkBoxAuthSource.Checked; };
            
            buttonOK.Text = LanguageManager.GetString("btn_save", "保存"); 
            buttonOK.Left = 60; buttonOK.Top = 320; buttonOK.Width = 80; buttonOK.DialogResult = DialogResult.OK;
            
            buttonCancel.Text = LanguageManager.GetString("btn_cancel", "取消"); 
            buttonCancel.Left = 160; buttonCancel.Top = 320; buttonCancel.Width = 80; buttonCancel.DialogResult = DialogResult.Cancel;
            this.Controls.AddRange(new Control[] { labelName, textBoxName, labelHost, textBoxHost, labelPort, textBoxPort, labelDatabase, textBoxDatabase, labelUsername, textBoxUsername, labelPassword, textBoxPassword, labelAuthMech, comboBoxAuthMechanism, checkBoxAuthSource, textBoxAuthSource, buttonOK, buttonCancel });
            if (info != null)
            {
                textBoxName.Text = info.Name;
                textBoxHost.Text = info.Host;
                textBoxPort.Text = info.Port;
                textBoxDatabase.Text = info.Database;
                textBoxUsername.Text = info.Username;
                textBoxPassword.Text = info.Password;
                if (!string.IsNullOrEmpty(info.AuthMechanism))
                {
                    int idx = comboBoxAuthMechanism.Items.IndexOf(info.AuthMechanism);
                    comboBoxAuthMechanism.SelectedIndex = idx >= 0 ? idx : 0;
                }
                if (!string.IsNullOrEmpty(info.AuthSource))
                {
                    checkBoxAuthSource.Checked = true;
                    textBoxAuthSource.Text = info.AuthSource;
                }
            }
            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;
        }
        public MongoConnInfo GetConnInfo()
        {
            return new MongoConnInfo
            {
                Name = textBoxName.Text.Trim(),
                Host = textBoxHost.Text.Trim(),
                Port = textBoxPort.Text.Trim(),
                Database = textBoxDatabase.Text.Trim(),
                Username = textBoxUsername.Text.Trim(),
                Password = textBoxPassword.Text.Trim(),
                AuthMechanism = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : null,
                AuthSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : null
            };
        }
    }
} 