using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace mongo
{
    public partial class LanguageSettingsForm : Form
    {
        private ComboBox comboBoxLanguage;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelLanguage;
        private Label labelDescription;

        public LanguageSettingsForm()
        {
            InitializeComponent();
            LoadCurrentLanguage();
        }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.GetString("dialog_language_settings", "语言设置");
            this.Width = 400;
            this.Height = 200;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建控件
            labelLanguage = new Label
            {
                Text = LanguageManager.GetString("label_language", "语言："),
                Location = new Point(20, 20),
                Size = new Size(80, 20),
                Font = new Font("Microsoft YaHei", 9)
            };

            comboBoxLanguage = new ComboBox
            {
                Location = new Point(100, 20),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft YaHei", 9)
            };

            labelDescription = new Label
            {
                Text = LanguageManager.GetString("text_language_description", "选择界面显示语言，更改后需要重启应用程序生效。"),
                Location = new Point(20, 60),
                Size = new Size(350, 40),
                Font = new Font("Microsoft YaHei", 9),
                ForeColor = Color.Gray
            };

            buttonOK = new Button
            {
                Text = LanguageManager.GetString("btn_ok", "确定"),
                Location = new Point(200, 120),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK,
                Font = new Font("Microsoft YaHei", 9)
            };

            buttonCancel = new Button
            {
                Text = LanguageManager.GetString("btn_cancel", "取消"),
                Location = new Point(290, 120),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                Font = new Font("Microsoft YaHei", 9)
            };

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] 
            { 
                labelLanguage, 
                comboBoxLanguage, 
                labelDescription, 
                buttonOK, 
                buttonCancel 
            });

            // 绑定事件
            buttonOK.Click += ButtonOK_Click;
            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;
        }

        private void LoadCurrentLanguage()
        {
            comboBoxLanguage.Items.Clear();
            
            // 添加支持的语言
            foreach (var lang in LanguageManager.SupportedLanguages)
            {
                comboBoxLanguage.Items.Add(new LanguageItem
                {
                    Code = lang.Key,
                    DisplayName = lang.Value
                });
            }

            // 选择当前语言
            string currentLang = LanguageManager.GetCurrentLanguage();
            for (int i = 0; i < comboBoxLanguage.Items.Count; i++)
            {
                var item = comboBoxLanguage.Items[i] as LanguageItem;
                if (item != null && item.Code == currentLang)
                {
                    comboBoxLanguage.SelectedIndex = i;
                    break;
                }
            }

            if (comboBoxLanguage.SelectedIndex == -1 && comboBoxLanguage.Items.Count > 0)
            {
                comboBoxLanguage.SelectedIndex = 0;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxLanguage.SelectedItem is LanguageItem selectedLang)
            {
                if (selectedLang.Code != LanguageManager.GetCurrentLanguage())
                {
                    LanguageManager.SwitchLanguage(selectedLang.Code);
                }
            }
        }

        public string GetSelectedLanguage()
        {
            if (comboBoxLanguage.SelectedItem is LanguageItem selectedLang)
            {
                return selectedLang.Code;
            }
            return LanguageManager.GetCurrentLanguage();
        }

        private class LanguageItem
        {
            public string Code { get; set; }
            public string DisplayName { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
} 