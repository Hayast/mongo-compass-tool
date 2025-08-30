using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;

namespace mongo
{
    public partial class ConnListForm : Form
    {
        private string iniPath = Path.Combine(Application.StartupPath, "config.ini");

        private List<MongoConnInfo> connList = new List<MongoConnInfo>();
        private string connListIniPath { get { return Path.Combine(Application.StartupPath, "connections.ini"); } }

        public string connectionString = "";
        public string DatabaseName { get { return textBoxDatabase.Text.Trim(); } }

        // 加载/保存连接信息列表
        private void LoadConnList()
        {
            connList.Clear();
            if (!File.Exists(connListIniPath)) return;
            var lines = File.ReadAllLines(connListIniPath);
            MongoConnInfo info = null;
            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    if (info != null) connList.Add(info);
                    info = new MongoConnInfo { Name = line.Trim('[', ']') };
                }
                else if (info != null && line.Contains("="))
                {
                    var kv = line.Split(new[] { '=' }, 2);
                    switch (kv[0])
                    {
                        case "Host": info.Host = kv[1]; break;
                        case "Port": info.Port = kv[1]; break;
                        case "Database": info.Database = kv[1]; break;
                        case "Username": info.Username = kv[1]; break;
                        case "Password": info.Password = kv[1]; break;
                        case "AuthMechanism": info.AuthMechanism = kv[1]; break;
                        case "AuthSource": info.AuthSource = kv[1]; break;
                    }
                }
            }
            if (info != null) connList.Add(info);
            RefreshConnListBox();
        }
        private void SaveConnList()
        {
            var lines = new List<string>();
            foreach (var c in connList)
            {
                lines.Add(string.Format("[{0}]", c.Name));
                lines.Add(string.Format("Host={0}", c.Host));
                lines.Add(string.Format("Port={0}", c.Port));
                lines.Add(string.Format("Database={0}", c.Database));
                lines.Add(string.Format("Username={0}", c.Username));
                lines.Add(string.Format("Password={0}", c.Password));
                lines.Add(string.Format("AuthMechanism={0}", (c.AuthMechanism ?? "")));
                lines.Add(string.Format("AuthSource={0}", (c.AuthSource ?? "")));
            }
            File.WriteAllLines(connListIniPath, lines);
        }
        private void RefreshConnListBox()
        {
            listBoxConnections.Items.Clear();
            foreach (var c in connList) listBoxConnections.Items.Add(c);
        }

        // 选中列表项时填充输入框
        private void listBoxConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            // This method is no longer needed as listBoxConnections is removed
        }
        // 添加连接
        private void buttonAddConn_Click(object sender, EventArgs e)
        {
            var dlg = new ConnEditForm(LanguageManager.GetString("dialog_add_connection", "添加连接"));
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var info = dlg.GetConnInfo();
                if (string.IsNullOrWhiteSpace(info.Name)) return;
                // 只保存弹窗内容
                connList.Add(info);
                SaveConnList();
                RefreshConnListBox();
            }
        }
        // 复制连接
        private void buttonCopyConn_Click(object sender, EventArgs e)
        {
            if (listBoxConnections.SelectedIndex < 0) return;
            var src = (MongoConnInfo)listBoxConnections.SelectedItem;
            var dlg = new ConnEditForm(LanguageManager.GetString("dialog_copy_connection", "复制连接"), src);
            dlg.textBoxName.Text = src.Name + "_copy";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var info = dlg.GetConnInfo();
                if (string.IsNullOrWhiteSpace(info.Name)) return;
                // 只保存弹窗内容
                connList.Add(info);
                SaveConnList();
                RefreshConnListBox();
            }
        }
        // 修改连接
        private void buttonEditConn_Click(object sender, EventArgs e)
        {
            if (listBoxConnections.SelectedIndex < 0) return;
            var c = (MongoConnInfo)listBoxConnections.SelectedItem;
            var dlg = new ConnEditForm(LanguageManager.GetString("dialog_edit_connection", "编辑连接"), c);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var info = dlg.GetConnInfo();
                if (string.IsNullOrWhiteSpace(info.Name)) return;
                c.Name = info.Name;
                c.Host = info.Host;
                c.Port = info.Port;
                c.Database = info.Database;
                c.Username = info.Username;
                c.Password = info.Password;
                c.AuthMechanism = info.AuthMechanism;
                c.AuthSource = info.AuthSource;
                SaveConnList();
                RefreshConnListBox();
            }
        }
        // 删除连接
        private void buttonDeleteConn_Click(object sender, EventArgs e)
        {
            if (listBoxConnections.SelectedIndex < 0) return;
            var idx = listBoxConnections.SelectedIndex;
            connList.RemoveAt(idx);
            SaveConnList();
            RefreshConnListBox();
        }
        // 列表项双击弹出编辑并填充到输入框
        private void listBoxConnections_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int idx = listBoxConnections.IndexFromPoint(e.Location);
            if (idx >= 0)
            {
                listBoxConnections.SelectedIndex = idx;
                var c = (MongoConnInfo)listBoxConnections.Items[idx];
                textBoxHost.Text = c.Host;
                textBoxPort.Text = c.Port;
                textBoxDatabase.Text = c.Database;
                textBoxUsername.Text = c.Username;
                textBoxPassword.Text = c.Password;
                // 主界面控件赋值
                if (!string.IsNullOrEmpty(c.AuthMechanism))
                {
                    int mechIdx = comboBoxAuthMechanism.Items.IndexOf(c.AuthMechanism);
                    comboBoxAuthMechanism.SelectedIndex = mechIdx >= 0 ? mechIdx : 0;
                }
                else
                {
                    comboBoxAuthMechanism.SelectedIndex = 0;
                }
                if (!string.IsNullOrEmpty(c.AuthSource))
                {
                    checkBoxAuthSource.Checked = true;
                    textBoxAuthSource.Text = c.AuthSource;
                }
                else
                {
                    checkBoxAuthSource.Checked = false;
                    textBoxAuthSource.Text = "";
                }
                // 赋值connectionString
                connectionString = GetConnectionString(c.Host, c.Port, c.Database, c.Username, c.Password, c.AuthMechanism, c.AuthSource);
            }
        }

        public ConnListForm()
        {
            InitializeComponent();
            LoadIniToInputs();
            LoadConnList();
            listBoxConnections.MouseDoubleClick += listBoxConnections_MouseDoubleClick;
            checkBoxAuthSource.CheckedChanged += (s, e) => { textBoxAuthSource.Enabled = checkBoxAuthSource.Checked; };
            comboBoxAuthMechanism.SelectedIndex = 0;
            UpdateComboBoxItems();
            ApplyLanguageToUI();
            // 绑定语言切换事件
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
        }

        private void LoadIniToInputs()
        {
            textBoxHost.Text = IniHelper.ReadValue("MongoDB", "Host", iniPath);
            textBoxPort.Text = IniHelper.ReadValue("MongoDB", "Port", iniPath);
            textBoxDatabase.Text = IniHelper.ReadValue("MongoDB", "Database", iniPath);
            textBoxUsername.Text = IniHelper.ReadValue("MongoDB", "Username", iniPath);
            textBoxPassword.Text = IniHelper.ReadValue("MongoDB", "Password", iniPath);
            var authMech = IniHelper.ReadValue("MongoDB", "AuthMechanism", iniPath);
            if (!string.IsNullOrEmpty(authMech))
            {
                int idx = comboBoxAuthMechanism.Items.IndexOf(authMech);
                comboBoxAuthMechanism.SelectedIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                comboBoxAuthMechanism.SelectedIndex = 0;
            }
            var authSource = IniHelper.ReadValue("MongoDB", "AuthSource", iniPath);
            if (!string.IsNullOrEmpty(authSource))
            {
                checkBoxAuthSource.Checked = true;
                textBoxAuthSource.Text = authSource;
            }
            else
            {
                checkBoxAuthSource.Checked = false;
                textBoxAuthSource.Text = "";
            }
        }

        private void SaveInputsToIni()
        {
            IniHelper.WriteValue("MongoDB", "Host", textBoxHost.Text.Trim(), iniPath);
            IniHelper.WriteValue("MongoDB", "Port", textBoxPort.Text.Trim(), iniPath);
            IniHelper.WriteValue("MongoDB", "Database", textBoxDatabase.Text.Trim(), iniPath);
            IniHelper.WriteValue("MongoDB", "Username", textBoxUsername.Text.Trim(), iniPath);
            IniHelper.WriteValue("MongoDB", "Password", textBoxPassword.Text.Trim(), iniPath);
            var authMech = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : "";
            IniHelper.WriteValue("MongoDB", "AuthMechanism", authMech, iniPath);
            var authSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : "";
            IniHelper.WriteValue("MongoDB", "AuthSource", authSource, iniPath);
        }

        public string GetConnectionString(string host, string port, string database, string username, string password, string authMech, string authSource)
        {
            // 构建基础连接字符串
            string connStr;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                connStr = string.Format("mongodb://{0}:{1}@{2}:{3}/{4}", username, password, host, port, database);
            }
            else
            {
                connStr = string.Format("mongodb://{0}:{1}/{2}", host, port, database);
            }
            
            var options = new List<string>();
            
            // 添加认证相关选项
            if (!string.IsNullOrEmpty(authMech)) 
                options.Add(string.Format("authMechanism={0}", authMech));
            if (!string.IsNullOrEmpty(authSource)) 
                options.Add(string.Format("authSource={0}", authSource));
            
            // 添加连接优化选项
            options.Add("directConnection=true");
            options.Add("serverSelectionTimeoutMS=15000");
            options.Add("connectTimeoutMS=15000");
            options.Add("socketTimeoutMS=30000");
            options.Add("maxIdleTimeMS=30000");
            options.Add("maxPoolSize=10");
            options.Add("minPoolSize=1");
            
            if (options.Count > 0)
            {
                connStr += "?" + string.Join("&", options);
            }
            return connStr;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            SaveInputsToIni();
            string host = textBoxHost.Text.Trim();
            string port = textBoxPort.Text.Trim();
            string database = textBoxDatabase.Text.Trim();
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();
            string authMech = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : null;
            string authSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : null;

            // 验证必填字段
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database))
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_fill_required_fields", "请填写主机、端口和数据库名称"), 
                    LanguageManager.GetString("dialog_warning", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 直接使用connectionString变量，如果为空则重新拼接
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetConnectionString(host, port, database, username, password, authMech, authSource);
            }

            try
            {
                SetStatus(LanguageManager.GetString("status_testing_network", "正在测试网络连接..."), Color.Blue);
                
                // 首先测试网络连接
                if (!TestNetworkConnection(host, port))
                {
                    string errorMsg = string.Format("无法连接到 {0}:{1}，请检查：\n1. 网络连接是否正常\n2. 服务器地址和端口是否正确\n3. 防火墙设置", host, port);
                    SetStatus(LanguageManager.GetString("status_network_failed", "网络连接失败"), Color.Red);
                    MessageBox.Show(errorMsg, LanguageManager.GetString("dialog_error", "网络连接错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                SetStatus(LanguageManager.GetString("status_network_ok_connecting_mongo", "网络连接正常，正在连接MongoDB..."), Color.Blue);
                
                try
                {
                    // 使用优化的连接设置
                    var settings = MongoClientSettings.FromConnectionString(connectionString);
                    settings.ConnectTimeout = TimeSpan.FromSeconds(15);
                    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);
                    settings.SocketTimeout = TimeSpan.FromSeconds(30);
                    settings.MaxConnectionPoolSize = 10;
                    settings.MinConnectionPoolSize = 1;

                    var client = new MongoClient(settings);
                    var db = client.GetDatabase(database);
                    
                    // 测试连接
                    try
                    {
                        var command = new BsonDocument("ping", 1);
                        db.RunCommand<BsonDocument>(command);
                        SetStatus(LanguageManager.GetString("status_connection_success", "连接成功！"), Color.Green);
                    }
                    catch (Exception pingEx)
                    {
                        // 如果ping失败，尝试列出集合
                        try
                        {
                            db.ListCollectionNames().FirstOrDefault();
                            SetStatus(LanguageManager.GetString("status_connection_success", "连接成功！"), Color.Green);
                        }
                        catch (Exception listEx)
                        {
                            throw new Exception(string.Format(LanguageManager.GetString("msg_connection_test_failed_ping_list", "连接测试失败: Ping失败({0}), 列出集合失败({1})"), pingEx.Message, listEx.Message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format(LanguageManager.GetString("msg_connection_failed", "连接失败: {0}"), ex.Message);
                    
                    // 提供更详细的错误信息和建议
                    if (ex.Message.Contains("DnsClientWrapper"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_network", "1. 检查网络连接") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_server_address", "2. 确认MongoDB服务器地址正确") + "\n" +
                            LanguageManager.GetString("msg_suggest_use_ip_address", "3. 尝试使用IP地址替代域名") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_dns", "4. 检查DNS设置");
                    }
                    else if (ex.Message.Contains("timeout"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_mongo_service", "1. 检查MongoDB服务是否运行") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_port", "2. 确认端口号正确") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_firewall", "3. 检查防火墙设置") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_network_connection", "4. 检查网络连接");
                    }
                    else if (ex.Message.Contains("authentication"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_username_password", "1. 检查用户名和密码") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_auth_mechanism", "2. 确认认证机制设置正确") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_user_permissions", "3. 检查用户权限") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_auth_database", "4. 确认认证数据库");
                    }
                    else if (ex.Message.Contains("connection"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_mongo_status", "1. 检查MongoDB服务状态") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_port_open", "2. 确认端口是否开放") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_network_connection", "3. 检查网络连接") + "\n" +
                            LanguageManager.GetString("msg_suggest_restart_mongo", "4. 尝试重启MongoDB服务");
                    }
                    
                    SetStatus(LanguageManager.GetString("status_connection_failed", "连接失败"), Color.Red);
                    MessageBox.Show(errorMessage, LanguageManager.GetString("dialog_connection_error", "连接错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(LanguageManager.GetString("msg_connection_failed", "连接失败: {0}"), ex.Message);
                
                // 提供更详细的错误信息和建议
                if (ex.Message.Contains("DnsClientWrapper"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_network", "1. 检查网络连接") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_server_address", "2. 确认MongoDB服务器地址正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_use_ip_address", "3. 尝试使用IP地址替代域名");
                }
                else if (ex.Message.Contains("timeout"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_mongo_service", "1. 检查MongoDB服务是否运行") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_port", "2. 确认端口号正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_firewall", "3. 检查防火墙设置");
                }
                else if (ex.Message.Contains("authentication"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_username_password", "1. 检查用户名和密码") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_auth_mechanism", "2. 确认认证机制设置正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_user_permissions", "3. 检查用户权限");
                }
                
                SetStatus(errorMessage, Color.Red);
                MessageBox.Show(errorMessage, LanguageManager.GetString("dialog_connection_error", "连接错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonBackup_Click(object sender, EventArgs e)
        {
            SetStatus(LanguageManager.GetString("status_backing_up", "正在备份数据库..."), Color.Blue);
            await Task.Delay(100); // 让UI刷新

            string host = textBoxHost.Text.Trim();
            string port = textBoxPort.Text.Trim();
            string database = textBoxDatabase.Text.Trim();
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();
            string authMech = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : null;
            string authSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : null;

            // 直接使用connectionString变量，如果为空则重新拼接
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetConnectionString(host, port, database, username, password, authMech, authSource);
            }

            string backupDir = Path.Combine(Application.StartupPath, "backup"+"/"+database);
            if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);

            try
            {
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(database);
                var collections = await db.ListCollectionNames().ToListAsync();
                foreach (var collName in collections)
                {
                    var collection = db.GetCollection<BsonDocument>(collName);
                    var docs = await collection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();

                    // 保存bson
                    string bsonPath = Path.Combine(backupDir, collName + ".bson");
                    using (var fs = new FileStream(bsonPath, FileMode.Create, FileAccess.Write))
                    {
                        foreach (var doc in docs)
                        {
                            var bytes = doc.ToBson();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    // 保存json
                    string jsonPath = Path.Combine(backupDir, collName + ".json");
                    File.WriteAllText(jsonPath, string.Join("\n", docs.Select(d => d.ToJson())));
                }
                SetStatus(LanguageManager.GetString("status_backup_completed", "备份完成！"), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format(LanguageManager.GetString("status_backup_failed", "备份失败：{0}"), ex.Message), Color.Red);
            }
        }

        private async void buttonRestore_Click(object sender, EventArgs e)
        {
            SetStatus(LanguageManager.GetString("status_restoring", "正在恢复数据库..."), Color.Blue);
            await Task.Delay(100); // 让UI刷新

            string host = textBoxHost.Text.Trim();
            string port = textBoxPort.Text.Trim();
            string database = textBoxDatabase.Text.Trim();
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();
            string authMech = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : null;
            string authSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : null;

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetConnectionString(host, port, database, username, password, authMech, authSource);
            }

            string restoreDir = Path.Combine(Application.StartupPath, "restore", database);
            if (!Directory.Exists(restoreDir))
            {
                SetStatus(string.Format(LanguageManager.GetString("status_restore_dir_not_found", "未找到restore目录：{0}"), restoreDir), Color.Red);
                return;
            }

            try
            {
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(database);
                var jsonFiles = Directory.GetFiles(restoreDir, "*.json");
                var bsonFiles = Directory.GetFiles(restoreDir, "*.bson");
                int total = jsonFiles.Length + bsonFiles.Length;
                int current = 0;
                foreach (var file in jsonFiles)
                {
                    string collName = Path.GetFileNameWithoutExtension(file);
                    var collection = db.GetCollection<BsonDocument>(collName);
                    await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
                    var jsonLines = File.ReadAllLines(file);
                    var docs = jsonLines.Select(line => BsonDocument.Parse(line)).ToList();
                    if (docs.Count > 0)
                        await collection.InsertManyAsync(docs);
                    current++;
                    SetStatus(string.Format(LanguageManager.GetString("status_restore_progress", "恢复进度：{0}/{1} ({2})"), current, total, collName));
                    await Task.Delay(50);
                }
                foreach (var file in bsonFiles)
                {
                    string collName = Path.GetFileNameWithoutExtension(file);
                    var collection = db.GetCollection<BsonDocument>(collName);
                    await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        var docs = new List<BsonDocument>();
                        while (fs.Position < fs.Length)
                        {
                            var doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(fs);
                            docs.Add(doc);
                        }
                        if (docs.Count > 0)
                            await collection.InsertManyAsync(docs);
                    }
                    current++;
                    SetStatus(string.Format(LanguageManager.GetString("status_restore_progress", "恢复进度：{0}/{1} ({2})"), current, total, collName));
                    await Task.Delay(50);
                }
                SetStatus(LanguageManager.GetString("status_restore_completed", "恢复完成！"), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format(LanguageManager.GetString("status_restore_failed", "恢复失败：{0}"), ex.Message), Color.Red);
            }
        }

        private async void buttonCreateDatabase_Click(object sender, EventArgs e)
        {
            SetStatus(LanguageManager.GetString("status_creating_database", "正在创建数据库..."), Color.Blue);
            await Task.Delay(100); // 让UI刷新

            string host = textBoxHost.Text.Trim();
            string port = textBoxPort.Text.Trim();
            string database = textBoxDatabase.Text.Trim();
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();
            string authMech = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : null;
            string authSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : null;

            // 验证必填字段
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database))
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_fill_required_fields", "请填写主机、端口和数据库名称"), 
                    LanguageManager.GetString("dialog_warning", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 构建连接字符串（不包含数据库名称，因为我们要创建数据库）
            string baseConnStr;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                baseConnStr = string.Format("mongodb://{0}:{1}@{2}:{3}", username, password, host, port);
            }
            else
            {
                baseConnStr = string.Format("mongodb://{0}:{1}", host, port);
            }
            
            var options = new List<string>();
            
            // 添加认证相关选项
            if (!string.IsNullOrEmpty(authMech)) 
                options.Add(string.Format("authMechanism={0}", authMech));
            if (!string.IsNullOrEmpty(authSource)) 
                options.Add(string.Format("authSource={0}", authSource));
            
            // 添加连接优化选项
            options.Add("directConnection=true");
            options.Add("serverSelectionTimeoutMS=15000");
            options.Add("connectTimeoutMS=15000");
            options.Add("socketTimeoutMS=30000");
            options.Add("maxIdleTimeMS=30000");
            options.Add("maxPoolSize=10");
            options.Add("minPoolSize=1");
            
            if (options.Count > 0)
            {
                baseConnStr += "?" + string.Join("&", options);
            }

            try
            {
                // 首先测试网络连接
                if (!TestNetworkConnection(host, port))
                {
                    string errorMsg = string.Format("无法连接到 {0}:{1}，请检查：\n1. 网络连接是否正常\n2. 服务器地址和端口是否正确\n3. 防火墙设置", host, port);
                    SetStatus(LanguageManager.GetString("status_network_failed", "网络连接失败"), Color.Red);
                    MessageBox.Show(errorMsg, LanguageManager.GetString("dialog_network_error", "网络连接错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SetStatus(LanguageManager.GetString("status_network_ok_connecting_mongo", "网络连接正常，正在连接MongoDB..."), Color.Blue);

                // 连接到MongoDB服务器
                var client = new MongoClient(baseConnStr);
                
                // 检查数据库是否已存在
                var databaseNames = await client.ListDatabaseNames().ToListAsync();
                if (databaseNames.Contains(database))
                {
                    SetStatus(string.Format(LanguageManager.GetString("status_database_exists", "数据库 '{0}' 已存在"), database), Color.Orange);
                    MessageBox.Show(string.Format(LanguageManager.GetString("msg_database_already_exists", "数据库 '{0}' 已经存在，无需创建。"), database), 
                        LanguageManager.GetString("dialog_info", "信息"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建数据库（通过创建一个集合来创建数据库）
                var db = client.GetDatabase(database);
                var collection = db.GetCollection<BsonDocument>("_temp_collection");
                
                // 插入一个临时文档来创建数据库和集合
                var tempDoc = new BsonDocument { { "_id", ObjectId.GenerateNewId() }, { "created_at", DateTime.UtcNow } };
                await collection.InsertOneAsync(tempDoc);
                
                // 删除临时文档
                await collection.DeleteOneAsync(tempDoc);
                
                // 删除临时集合
                await db.DropCollectionAsync("_temp_collection");

                SetStatus(string.Format(LanguageManager.GetString("status_database_created", "数据库 '{0}' 创建成功！"), database), Color.Green);
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_database_created_success", "数据库 '{0}' 创建成功！"), database), 
                    LanguageManager.GetString("dialog_success", "成功"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(LanguageManager.GetString("msg_create_database_failed", "创建数据库失败: {0}"), ex.Message);
                
                // 提供更详细的错误信息和建议
                if (ex.Message.Contains("authentication"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_username_password", "1. 检查用户名和密码") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_auth_mechanism", "2. 确认认证机制设置正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_user_permissions", "3. 检查用户权限") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_auth_database", "4. 确认认证数据库");
                }
                else if (ex.Message.Contains("permission"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_user_permissions", "1. 检查用户权限") + "\n" +
                        LanguageManager.GetString("msg_suggest_use_admin_user", "2. 使用具有管理员权限的用户") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_database_name", "3. 检查数据库名称是否合法");
                }
                
                SetStatus(LanguageManager.GetString("status_create_database_failed", "创建数据库失败"), Color.Red);
                MessageBox.Show(errorMessage, LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetStatus(string text, Color? color = null)
        {
            //labelStatus.Text = text;
            toolStripStatusLabel1.Text = text;
            if (color != null)
            {
                //labelStatus.ForeColor = color.Value;
                toolStripStatusLabel1.ForeColor = color.Value;
            }
        }

        // 测试网络连接
        private bool TestNetworkConnection(string host, string port)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var result = client.BeginConnect(host, int.Parse(port), null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                    client.EndConnect(result);
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }



        // 确定按钮事件
        private void buttonOpenCompass_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取当前连接信息
                string host = textBoxHost.Text.Trim();
                string port = textBoxPort.Text.Trim();
                string database = textBoxDatabase.Text.Trim();
                string username = textBoxUsername.Text.Trim();
                string password = textBoxPassword.Text.Trim();
                string authMech = comboBoxAuthMechanism.SelectedIndex > 0 ? comboBoxAuthMechanism.SelectedItem.ToString() : null;
                string authSource = checkBoxAuthSource.Checked ? textBoxAuthSource.Text.Trim() : null;

                // 验证必填字段
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database))
                {
                    MessageBox.Show(LanguageManager.GetString("msg_please_fill_required_fields", "请填写主机、端口和数据库名称"), 
                        LanguageManager.GetString("dialog_warning", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 构建连接字符串
                string connStr = GetConnectionString(host, port, database, username, password, authMech, authSource);

                // 首先测试网络连接
                if (!TestNetworkConnection(host, port))
                {
                    string errorMsg = string.Format(LanguageManager.GetString("msg_cannot_connect_to_host", "无法连接到 {0}:{1}，请检查：\n1. 网络连接是否正常\n2. 服务器地址和端口是否正确\n3. 防火墙设置"), host, port);
                    MessageBox.Show(errorMsg, LanguageManager.GetString("dialog_network_error", "网络连接错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // 使用优化的连接设置
                    var settings = MongoClientSettings.FromConnectionString(connStr);
                    settings.ConnectTimeout = TimeSpan.FromSeconds(15);
                    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);
                    settings.SocketTimeout = TimeSpan.FromSeconds(30);
                    settings.MaxConnectionPoolSize = 10;
                    settings.MinConnectionPoolSize = 1;

                    var client = new MongoClient(settings);
                    var db = client.GetDatabase(database);
                    
                    // 测试连接
                    try
                    {
                        var command = new BsonDocument("ping", 1);
                        db.RunCommand<BsonDocument>(command);
                    }
                    catch (Exception pingEx)
                    {
                        // 如果ping失败，尝试列出集合
                        try
                        {
                            db.ListCollectionNames().FirstOrDefault();
                        }
                        catch (Exception listEx)
                        {
                            throw new Exception(string.Format(LanguageManager.GetString("msg_connection_test_failed_ping_list", "连接测试失败: Ping失败({0}), 列出集合失败({1})"), pingEx.Message, listEx.Message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format(LanguageManager.GetString("msg_connection_test_failed", "连接测试失败: {0}"), ex.Message);
                    
                    // 提供更详细的错误信息和建议
                    if (ex.Message.Contains("DnsClientWrapper"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_network", "1. 检查网络连接") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_server_address", "2. 确认MongoDB服务器地址正确") + "\n" +
                            LanguageManager.GetString("msg_suggest_use_ip_address", "3. 尝试使用IP地址替代域名") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_dns", "4. 检查DNS设置");
                    }
                    else if (ex.Message.Contains("timeout"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_mongo_service", "1. 检查MongoDB服务是否运行") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_port", "2. 确认端口号正确") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_firewall", "3. 检查防火墙设置") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_network_connection", "4. 检查网络连接");
                    }
                    else if (ex.Message.Contains("authentication"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_username_password", "1. 检查用户名和密码") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_auth_mechanism", "2. 确认认证机制设置正确") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_user_permissions", "3. 检查用户权限") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_auth_database", "4. 确认认证数据库");
                    }
                    else if (ex.Message.Contains("connection"))
                    {
                        errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_mongo_status", "1. 检查MongoDB服务状态") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_port_open", "2. 确认端口是否开放") + "\n" +
                            LanguageManager.GetString("msg_suggest_check_network_connection", "3. 检查网络连接") + "\n" +
                            LanguageManager.GetString("msg_suggest_restart_mongo", "4. 尝试重启MongoDB服务");
                    }
                    
                    MessageBox.Show(errorMessage, LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 保存连接信息到CompassForm
                this.connectionString = connStr;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(LanguageManager.GetString("msg_connection_test_failed", "连接测试失败: {0}"), ex.Message);
                
                // 提供更详细的错误信息和建议
                if (ex.Message.Contains("DnsClientWrapper"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_network", "1. 检查网络连接") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_server_address", "2. 确认MongoDB服务器地址正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_use_ip_address", "3. 尝试使用IP地址替代域名");
                }
                else if (ex.Message.Contains("timeout"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_mongo_service", "1. 检查MongoDB服务是否运行") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_port", "2. 确认端口号正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_firewall", "3. 检查防火墙设置");
                }
                else if (ex.Message.Contains("authentication"))
                {
                    errorMessage += "\n\n" + LanguageManager.GetString("msg_suggested_solutions", "建议解决方案：") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_username_password", "1. 检查用户名和密码") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_auth_mechanism", "2. 确认认证机制设置正确") + "\n" +
                        LanguageManager.GetString("msg_suggest_check_user_permissions", "3. 检查用户权限");
                }
                
                MessageBox.Show(errorMessage, LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        #region 语言管理

        // 语言切换事件处理
        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            // 在主线程中更新UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyLanguageToUI()));
            }
            else
            {
                ApplyLanguageToUI();
            }
        }

        // 更新ComboBox项目
        private void UpdateComboBoxItems()
        {
            try
            {
                int selectedIndex = comboBoxAuthMechanism.SelectedIndex;
                comboBoxAuthMechanism.Items.Clear();
                comboBoxAuthMechanism.Items.Add(LanguageManager.GetString("text_default", "(默认)"));
                comboBoxAuthMechanism.Items.Add("SCRAM-SHA-1");
                comboBoxAuthMechanism.Items.Add("SCRAM-SHA-256");
                comboBoxAuthMechanism.Items.Add("MONGODB-CR");
                if (selectedIndex >= 0 && selectedIndex < comboBoxAuthMechanism.Items.Count)
                {
                    comboBoxAuthMechanism.SelectedIndex = selectedIndex;
                }
                else
                {
                    comboBoxAuthMechanism.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_apply_language_failed", "应用语言到UI失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 应用语言到UI
        private void ApplyLanguageToUI()
        {
            try
            {
                // 更新窗体标题
                this.Text = LanguageManager.GetString("form_connection_list", "连接列表");

                // 更新标签
                labelHost.Text = LanguageManager.GetString("label_host", "主机：");
                labelPort.Text = LanguageManager.GetString("label_port", "端口：");
                labelDatabase.Text = LanguageManager.GetString("label_database", "数据库：");
                labelUsername.Text = LanguageManager.GetString("label_username", "用户名：");
                labelPassword.Text = LanguageManager.GetString("label_password", "密码：");

                // 更新按钮
                buttonConnect.Text = LanguageManager.GetString("btn_connect", "连接");
                buttonAddConn.Text = LanguageManager.GetString("btn_add", "添加");
                buttonCopyConn.Text = LanguageManager.GetString("btn_copy", "复制");
                buttonEditConn.Text = LanguageManager.GetString("btn_edit", "编辑");
                buttonDeleteConn.Text = LanguageManager.GetString("btn_delete", "删除");
                buttonBackup.Text = LanguageManager.GetString("btn_backup", "备份");
                buttonRestore.Text = LanguageManager.GetString("btn_restore", "恢复");
                buttonCreateDatabase.Text = LanguageManager.GetString("btn_create_database", "创建数据库");

                buttonOpenCompass.Text = LanguageManager.GetString("btn_open_compass", "打开Compass");

                // 更新复选框
                checkBoxAuthSource.Text = LanguageManager.GetString("text_use_auth_source", "使用authSource");

                // 更新ComboBox项目
                UpdateComboBoxItems();

                // 更新状态栏
                SetStatus(LanguageManager.GetString("status_ready", "就绪"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_apply_language_failed", "应用语言到UI失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
