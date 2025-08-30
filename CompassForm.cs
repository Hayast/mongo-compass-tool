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
using Microsoft.VisualBasic;

namespace mongo
{
    public partial class CompassForm : Form
    {
        private IMongoClient mongoClient;
        private Dictionary<string, IMongoDatabase> connectedDatabases = new Dictionary<string, IMongoDatabase>();
        // 缓存所有数据库和集合名，便于过滤恢复
        private Dictionary<string, List<string>> allCollections = new Dictionary<string, List<string>>();
        // 记忆上次选择的导入目录
        private string lastImportDirectory = string.Empty;
        // 记忆上次连接的参数
        private string lastConnectionString = string.Empty;
        private string lastDatabaseName = string.Empty;
        private DateTime lastConnectionTime = DateTime.MinValue;
        
        // 定义需要跳过的字段（用于导出SQL脚本时过滤）
        private static readonly HashSet<string> SkipFields = new HashSet<string> { "_id", "_v", "__v", "TDtUpdate", "updatedAt", "createdAt", "createTime", "updateTime", "MenuType2" };
        
        // 语言管理 - 使用共享的LanguageManager

        public CompassForm()
        {
            InitializeComponent();
            InitializeTreeView();
            InitializeContextMenu();
            InitializeTabEvents();
            // 初始化语言系统
            LanguageManager.Initialize();
            ApplyLanguageToUI();
            // 绑定语言切换事件
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            // 绑定集合过滤事件
            textBoxCollectionFilter.TextChanged += TextBoxCollectionFilter_TextChanged;
            // 加载上次选择的导入目录
            LoadLastImportDirectory();
            // 加载上次连接参数
            LoadLastConnectionParams();
            // 如果有上次连接参数，提示用户是否自动重连
            ShowAutoReconnectPrompt();
        }

        private void InitializeTreeView()
        {
            try
            {
                SetStatus("正在初始化树形控件...");
                treeViewCollections.Nodes.Clear();
                
                // 美化树形控件
                treeViewCollections.BackColor = Color.White;
                treeViewCollections.BorderStyle = BorderStyle.None;
                treeViewCollections.Font = new Font("Microsoft YaHei", 9);
                treeViewCollections.ForeColor = Color.FromArgb(64, 64, 64);
                treeViewCollections.LineColor = Color.FromArgb(224, 224, 224);
                
                treeViewCollections.Refresh();
                SetStatus("树形控件初始化完成");
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("树形控件初始化失败: {0}", ex.Message), Color.Red);
            }
        }

        // 连接到MongoDB
        public void ConnectToMongoDB(string connectionString, string databaseName)
        {
            try
            {
                SetStatus("正在连接到MongoDB...");
                mongoClient = new MongoClient(connectionString);
                // 加载所有数据库并缓存集合
                var dbNames = mongoClient.ListDatabaseNames().ToList();
                foreach (var dbName in dbNames)
                {
                    connectedDatabases[dbName] = mongoClient.GetDatabase(dbName);
                    LoadCollections(dbName);
                }
                
                // 保存连接参数
                lastConnectionString = connectionString;
                lastDatabaseName = databaseName;
                lastConnectionTime = DateTime.Now;
                SaveLastConnectionParams();
                
                SetStatus("已连接并缓存所有数据库集合", Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_connection_failed", "连接失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("连接失败: {0}", ex.Message), Color.Red);
            }
        }

        // 加载集合列表
        private void LoadCollections(string databaseName)
        {
            try
            {
                SetStatus(string.Format("正在加载数据库 {0} 的集合列表...", databaseName));
                
                // 检查树形控件是否正确初始化
                if (treeViewCollections.Nodes.Count == 0)
                {
                    SetStatus("树形控件未初始化，正在重新初始化...", Color.Orange);
                    InitializeTreeView();
                }
                
                var database = connectedDatabases[databaseName];
                var collections = database.ListCollectionNames().ToList();
                
                // 对集合名称进行排序
                collections.Sort();

                // 同步缓存到 allCollections，供过滤用
                allCollections[databaseName] = collections;
                
                SetStatus(string.Format("获取到 {0} 个集合，已按名称排序", collections.Count));
                
                // 确保在UI线程上执行
                if (treeViewCollections.InvokeRequired)
                {
                    treeViewCollections.Invoke(new Action(() => LoadCollections(databaseName)));
                    return;
                }
                
                // 查找或创建数据库节点
                TreeNode dbNode = null;
                foreach (TreeNode node in treeViewCollections.Nodes)
                {
                    if (node.Text == databaseName)
                    {
                        dbNode = node;
                        break;
                    }
                }
                
                if (dbNode == null)
                {
                    SetStatus(string.Format("创建数据库节点: {0}", databaseName));
                    dbNode = new TreeNode(databaseName);
                    dbNode.Tag = string.Format("database:{0}", databaseName);
                    dbNode.NodeFont = new Font("Microsoft YaHei", 9, FontStyle.Bold);
                    dbNode.ForeColor = Color.FromArgb(0, 122, 204);
                    treeViewCollections.Nodes.Add(dbNode);
                }
                
                dbNode.Nodes.Clear();
                SetStatus(string.Format("正在添加 {0} 个集合节点...", collections.Count));
                
                foreach (var collectionName in collections)
                {
                    TreeNode collectionNode = new TreeNode(collectionName);
                    collectionNode.Tag = string.Format("collection:{0}.{1}", databaseName, collectionName);
                    collectionNode.NodeFont = new Font("Microsoft YaHei", 9);
                    collectionNode.ForeColor = Color.FromArgb(64, 64, 64);
                    
                    // 为每个集合添加"索引"子节点
                    TreeNode indexNode = new TreeNode(LanguageManager.GetString("text_index", "索引"));
                    indexNode.Tag = string.Format("indexes:{0}.{1}", databaseName, collectionName);
                    indexNode.NodeFont = new Font("Microsoft YaHei", 9);
                    indexNode.ForeColor = Color.FromArgb(128, 128, 128);
                    
                    // 为索引节点创建右键菜单
                    CreateIndexesNodeContextMenu(indexNode, databaseName, collectionName);
                    
                    collectionNode.Nodes.Add(indexNode);
                    
                    dbNode.Nodes.Add(collectionNode);
                }
                
                // 强制刷新UI
                treeViewCollections.Refresh();
                // 只展开数据库节点，不展开集合节点
                foreach (TreeNode databaseNode in treeViewCollections.Nodes)
                {
                    databaseNode.Expand();
                }
                
                SetStatus(string.Format("已加载 {0} 个集合到树形控件", collections.Count), Color.Green);
                
                // 验证节点是否正确添加
                SetStatus(string.Format("树形控件当前有 {0} 个数据库节点，{1} 个集合节点", 
                    treeViewCollections.Nodes.Count, 
                    dbNode.Nodes.Count));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("加载集合失败: {0}", ex.Message), Color.Red);
            }
        }

        // 处理节点点击事件
        private void treeViewCollections_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node.Tag != null)
            {
                string tag = e.Node.Tag.ToString();
                
                // 处理索引节点点击
                if (tag.StartsWith("indexes:"))
                {
                    string[] parts = tag.Split(':')[1].Split('.');
                    string databaseName = parts[0];
                    string collectionName = parts[1];
                    
                    // 检查是否已经加载了索引
                    bool hasLoadedIndexes = false;
                    foreach (TreeNode childNode in e.Node.Nodes)
                    {
                        if (childNode.Tag != null && childNode.Tag.ToString().StartsWith("index:"))
                        {
                            hasLoadedIndexes = true;
                            break;
                        }
                    }
                    
                    // 如果没有加载索引，则自动加载
                    if (!hasLoadedIndexes)
                    {
                        LoadCollectionIndexes(databaseName, collectionName, e.Node);
                    }
                }
            }
        }

        // 双击打开集合标签页
        private void treeViewCollections_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Tag.ToString().StartsWith("collection:"))
            {
                string[] parts = e.Node.Tag.ToString().Split(':')[1].Split('.');
                string databaseName = parts[0];
                string collectionName = parts[1];
                
                // 防止重复双击
                if (e.Button == MouseButtons.Left)
                {
                    OpenCollectionTab(databaseName, collectionName);
                }
            }
        }

        // 打开集合菜单项点击事件
        private void MenuItemOpen_Click(object sender, EventArgs e)
        {
            // 获取当前右键点击的节点
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null && menuItem.Owner is ContextMenuStrip contextMenu)
            {
                // 从上下文菜单的SourceControl获取TreeView
                if (contextMenu.SourceControl is TreeView treeView)
                {
                    // 获取当前选中的节点
                    TreeNode selectedNode = treeView.SelectedNode;
                    if (selectedNode != null && selectedNode.Tag != null && selectedNode.Tag.ToString().StartsWith("collection:"))
                    {
                        string[] parts = selectedNode.Tag.ToString().Split(':')[1].Split('.');
                        string databaseName = parts[0];
                        string collectionName = parts[1];
                        
                        // 调用打开集合标签页的方法
                        OpenCollectionTab(databaseName, collectionName);
                    }
                }
            }
        }

        // 打开集合标签页
        private void OpenCollectionTab(string databaseName, string collectionName)
        {
            try
            {
                // 检查是否已经存在该标签页
                string tabTitle = collectionName; // 只显示集合名称，更简洁
                string fullTitle = string.Format("{0}.{1}", databaseName, collectionName);
                string collectionIdentifier = string.Format("collection:{0}.{1}", databaseName, collectionName);
                
                // 检查是否已经存在该集合的标签页
                foreach (TabPage tab in tabControlData.TabPages)
                {
                    if (tab.Name != null && tab.Name.Equals(collectionIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        // 如果找到已存在的标签页，切换到该标签页并返回
                        tabControlData.SelectedTab = tab;
                        SetStatus(string.Format("已切换到集合: {0}.{1}", databaseName, collectionName), Color.Blue);
                        return;
                    }
                }
                
                // 如果没有找到已存在的标签页，创建新的标签页
                SetStatus(string.Format("正在创建集合标签页: {0}.{1}", databaseName, collectionName));
                TabPage newTab = new TabPage(tabTitle);
                newTab.Name = collectionIdentifier; // 使用Name属性存储集合标识
                newTab.ToolTipText = fullTitle; // 鼠标悬停显示完整名称
                
                // 创建查询界面
                CreateQueryInterface(newTab, databaseName, collectionName);
                
                tabControlData.TabPages.Add(newTab);
                tabControlData.SelectedTab = newTab;
                
                SetStatus(string.Format("已创建并切换到集合: {0}.{1}", databaseName, collectionName), Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("打开集合标签页失败: {0}", ex.Message), Color.Red);
            }
        }

        // 创建查询界面
        private void CreateQueryInterface(TabPage tab, string databaseName, string collectionName)
        {
            // 使用TableLayoutPanel创建三层布局
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // 工具栏
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // 查询区域
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 数据显示区域
            
            // 第一层：操作工具栏
            Panel toolbarPanel = new Panel();
            toolbarPanel.Dock = DockStyle.Fill;
            toolbarPanel.BackColor = Color.FromArgb(240, 240, 240);
            toolbarPanel.Padding = new Padding(10, 10, 10, 5);
            
            // 创建工具栏按钮
            CreateToolbarButtons(toolbarPanel, databaseName, collectionName);
            
            // 第二层：查询控件
            Panel queryPanel = new Panel();
            queryPanel.Dock = DockStyle.Fill;
            queryPanel.BackColor = Color.FromArgb(245, 245, 245);
            queryPanel.Padding = new Padding(10);
            
            TextBox textBoxQuery = new TextBox();
            textBoxQuery.Multiline = true;
            textBoxQuery.ScrollBars = ScrollBars.Vertical;
            textBoxQuery.Location = new Point(10, 10);
            textBoxQuery.Size = new Size(850, 80);
            textBoxQuery.Text = "{}";
            textBoxQuery.Font = new Font("Consolas", 9);
            textBoxQuery.BorderStyle = BorderStyle.FixedSingle;
            textBoxQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            queryPanel.Controls.Add(textBoxQuery);
            
            // 第三层：数据显示
            DataGridView dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.GridColor = Color.FromArgb(224, 224, 224);
            dataGridView.BorderStyle = BorderStyle.None;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
            dataGridView.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 添加右键菜单
            ContextMenuStrip rowContextMenu = new ContextMenuStrip();
            rowContextMenu.Font = new Font("Microsoft YaHei", 9);
            ToolStripMenuItem menuItemEdit = new ToolStripMenuItem(LanguageManager.GetString("text_edit_record", "修改记录"));
            menuItemEdit.Tag = "text_edit_record";
            menuItemEdit.Click += (s, e) => EditRecord_Click(dataGridView, databaseName, collectionName);
            ToolStripMenuItem menuItemCopy = new ToolStripMenuItem(LanguageManager.GetString("text_copy_record", "复制记录"));
            menuItemCopy.Tag = "text_copy_record";
            menuItemCopy.Click += (s, e) => CopyRecord_Click(dataGridView, databaseName, collectionName);
            ToolStripMenuItem menuItemDelete = new ToolStripMenuItem(LanguageManager.GetString("text_delete_record", "删除记录"));
            menuItemDelete.Tag = "text_delete_record";
            menuItemDelete.Click += (s, e) => DeleteRecord_Click(dataGridView, databaseName, collectionName);
            rowContextMenu.Items.Add(menuItemEdit);
            rowContextMenu.Items.Add(menuItemCopy);
            rowContextMenu.Items.Add(menuItemDelete);
            dataGridView.ContextMenuStrip = rowContextMenu;
            dataGridView.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = dataGridView.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        dataGridView.ClearSelection();
                        dataGridView.Rows[hit.RowIndex].Selected = true;
                    }
                }
            };
            
            // 将DataGridView存储到Tab的Tag中，供工具栏按钮使用
            // 注意：这里不能直接覆盖Tag，因为Tag已经用于标识集合
            // 我们需要将界面数据存储到其他地方
            tab.Name = string.Format("collection:{0}.{1}", databaseName, collectionName); // 使用Name属性存储集合标识
            tab.Tag = new { 
                DatabaseName = databaseName, 
                CollectionName = collectionName, 
                DataGridView = dataGridView,
                QueryTextBox = textBoxQuery
            };
            
            // 将控件添加到布局
            mainLayout.Controls.Add(toolbarPanel, 0, 0);
            mainLayout.Controls.Add(queryPanel, 0, 1);
            mainLayout.Controls.Add(dataGridView, 0, 2);
            
            tab.Controls.Add(mainLayout);
            
            // 初始加载数据
            ExecuteQuery(databaseName, collectionName, "{}", dataGridView);
        }

        // 创建工具栏按钮
        private void CreateToolbarButtons(Panel toolbarPanel, string databaseName, string collectionName)
        {
            int buttonWidth = 100;
            int buttonHeight = 30;
            int spacing = 10;
            int currentX = 0;
            
            // 执行查询按钮 - 移到最前面
            Button btnExecute = CreateToolbarButton(LanguageManager.GetString("text_execute", "执行查询"), currentX, buttonWidth, buttonHeight, Color.FromArgb(0, 122, 204));
            btnExecute.Click += (s, e) => {
                var currentTab = tabControlData.SelectedTab;
                if (currentTab != null && currentTab.Tag != null)
                {
                    var tabData = currentTab.Tag as dynamic;
                    if (tabData != null)
                    {
                        ExecuteQuery(databaseName, collectionName, tabData.QueryTextBox.Text, tabData.DataGridView);
                    }
                }
            };
            toolbarPanel.Controls.Add(btnExecute);
            currentX += buttonWidth + spacing;
            
            // 新增记录按钮
            Button btnAdd = CreateToolbarButton(LanguageManager.GetString("text_add_record", "新增记录"), currentX, buttonWidth, buttonHeight, Color.FromArgb(40, 167, 69));
            btnAdd.Click += (s, e) => ShowAddRecordDialog(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnAdd);
            currentX += buttonWidth + spacing;
            
            // 批量新增按钮
            Button btnBatchAdd = CreateToolbarButton(LanguageManager.GetString("text_batch_add", "批量新增"), currentX, buttonWidth, buttonHeight, Color.FromArgb(23, 162, 184));
            btnBatchAdd.Click += (s, e) => ShowBatchAddDialog(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnBatchAdd);
            currentX += buttonWidth + spacing;
            
            // 导入JSON按钮
            Button btnImportJson = CreateToolbarButton(LanguageManager.GetString("text_import_json", "导入JSON"), currentX, buttonWidth, buttonHeight, Color.FromArgb(255, 193, 7));
            btnImportJson.ForeColor = Color.Black;
            btnImportJson.Click += (s, e) => ShowImportJsonDialog(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnImportJson);
            currentX += buttonWidth + spacing;
            
            // 导入CSV按钮
            Button btnImportCsv = CreateToolbarButton(LanguageManager.GetString("text_import_csv", "导入CSV"), currentX, buttonWidth, buttonHeight, Color.FromArgb(255, 193, 7));
            btnImportCsv.ForeColor = Color.Black;
            btnImportCsv.Click += (s, e) => ShowImportCsvDialog(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnImportCsv);
            currentX += buttonWidth + spacing;
            
            // 修改记录按钮
            Button btnEdit = CreateToolbarButton(LanguageManager.GetString("text_edit_record", "修改记录"), currentX, buttonWidth, buttonHeight, Color.FromArgb(0, 123, 255));
            btnEdit.Click += (s, e) => EditSelectedRecord(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnEdit);
            currentX += buttonWidth + spacing;
            
            // 删除记录按钮
            Button btnDelete = CreateToolbarButton(LanguageManager.GetString("text_delete_record", "删除记录"), currentX, buttonWidth, buttonHeight, Color.FromArgb(220, 53, 69));
            btnDelete.Click += (s, e) => DeleteSelectedRecord(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnDelete);
            currentX += buttonWidth + spacing;
            
            // 批量删除按钮
            Button btnBatchDelete = CreateToolbarButton(LanguageManager.GetString("text_batch_delete", "批量删除"), currentX, buttonWidth, buttonHeight, Color.FromArgb(220, 53, 69));
            btnBatchDelete.Click += (s, e) => BatchDeleteSelectedRecords(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnBatchDelete);
            currentX += buttonWidth + spacing;
            
            // 刷新按钮
            Button btnRefresh = CreateToolbarButton(LanguageManager.GetString("text_refresh_data", "刷新数据"), currentX, buttonWidth, buttonHeight, Color.FromArgb(108, 117, 125));
            btnRefresh.Click += (s, e) => RefreshCollectionData(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnRefresh);
            currentX += buttonWidth + spacing;
            
            // 删除属性按钮
            Button btnDeleteField = CreateToolbarButton(LanguageManager.GetString("text_delete_field", "删除属性"), currentX, buttonWidth, buttonHeight, Color.FromArgb(255, 69, 0));
            btnDeleteField.Click += (s, e) => ShowDeleteFieldDialog(databaseName, collectionName);
            toolbarPanel.Controls.Add(btnDeleteField);
        }
        
        // 创建工具栏按钮的辅助方法
        private Button CreateToolbarButton(string text, int x, int width, int height, Color backColor)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(x, 5);
            button.Size = new Size(width, height);
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Microsoft YaHei", 9);
            button.FlatAppearance.BorderSize = 0;
            
            // 从文本中提取语言键（如果文本是通过LanguageManager.GetString获取的）
            // 这里我们通过检查文本是否包含特定的语言键来推断
            string languageKey = GetLanguageKeyFromText(text);
            if (!string.IsNullOrEmpty(languageKey))
            {
                button.Tag = languageKey;
            }
            
            return button;
        }
        
        // 从文本中推断语言键
        private string GetLanguageKeyFromText(string text)
        {
            // 根据文本内容推断对应的语言键
            switch (text)
            {
                case "执行查询":
                case "Execute Query":
                    return "text_execute";
                case "新增记录":
                case "Add Record":
                    return "text_add_record";
                case "批量新增":
                case "Batch Add":
                    return "text_batch_add";
                case "导入JSON":
                case "Import JSON":
                    return "text_import_json";
                case "导入CSV":
                case "Import CSV":
                    return "text_import_csv";
                case "修改记录":
                case "Edit Record":
                    return "text_edit_record";
                case "删除记录":
                case "Delete Record":
                    return "text_delete_record";
                case "批量删除":
                case "Batch Delete":
                    return "text_batch_delete";
                case "刷新数据":
                case "Refresh Data":
                    return "text_refresh_data";
                default:
                    return null;
            }
        }

        // 执行查询
        private void ExecuteQuery(string databaseName, string collectionName, string queryText, DataGridView dataGridView)
        {
            try
            {
                // 如果没有传入DataGridView，从当前选中的Tab获取
                if (dataGridView == null)
                {
                    var currentTab = tabControlData.SelectedTab;
                    if (currentTab != null && currentTab.Tag != null)
                    {
                        var tabData = currentTab.Tag as dynamic;
                        if (tabData != null)
                        {
                            dataGridView = tabData.DataGridView;
                        }
                    }
                }
                
                if (dataGridView == null)
                {
                    SetStatus("无法找到数据显示控件", Color.Red);
                    return;
                }
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                BsonDocument filter;
                try
                {
                    filter = BsonDocument.Parse(queryText);
                }
                catch
                {
                    MessageBox.Show(LanguageManager.GetString("msg_invalid_query", "查询语句格式错误，请检查JSON格式"), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var documents = collection.Find(filter).Limit(1000).ToList();
                
                // 清空数据网格
                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();
                
                if (documents.Count > 0)
                {
                    // 获取所有字段名
                    HashSet<string> allFields = new HashSet<string>();
                    foreach (var doc in documents)
                    {
                        foreach (var element in doc.Elements)
                        {
                            allFields.Add(element.Name);
                        }
                    }
                    
                    // 创建列
                    foreach (string fieldName in allFields.OrderBy(f => f))
                    {
                        dataGridView.Columns.Add(fieldName, fieldName);
                    }
                    
                    // 填充数据
                    foreach (var doc in documents)
                    {
                        var row = new object[allFields.Count];
                        int colIndex = 0;
                        foreach (string fieldName in allFields.OrderBy(f => f))
                        {
                            if (doc.Contains(fieldName))
                            {
                                row[colIndex] = doc[fieldName].ToString();
                            }
                            else
                            {
                                row[colIndex] = "";
                            }
                            colIndex++;
                        }
                        int rowIndex = dataGridView.Rows.Add(row);
                        // 关键：保存原始BsonDocument到Tag
                        dataGridView.Rows[rowIndex].Tag = doc;
                    }
                }
                
                SetStatus(string.Format("查询完成，共返回 {0} 条记录", documents.Count), Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void treeViewCollections_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 可以在这里添加选中节点时的处理逻辑
        }

        // 初始化右键菜单
        private void InitializeContextMenu()
        {
            // 绑定右键菜单事件到树形控件
            treeViewCollections.MouseClick += TreeViewCollections_MouseClick;
        }

        // 树形控件右键点击事件
        private void TreeViewCollections_MouseClick(object sender, MouseEventArgs e)
        {
            // 只处理右键点击，避免干扰双击事件
            if (e.Button == MouseButtons.Right)
            {
                TreeNode clickedNode = treeViewCollections.GetNodeAt(e.X, e.Y);
                if (clickedNode != null)
                {
                    // 选中被点击的节点
                    treeViewCollections.SelectedNode = clickedNode;
                    
                    // 根据节点类型创建不同的右键菜单
                    ContextMenuStrip contextMenu = CreateContextMenuForNode(clickedNode);
                    if (contextMenu != null)
                    {
                        contextMenu.Show(treeViewCollections, e.Location);
                    }
                }
            }
        }

        // 根据节点类型创建右键菜单
        private ContextMenuStrip CreateContextMenuForNode(TreeNode node)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Font = new Font("Microsoft YaHei", 9);

            if (node.Tag != null)
            {
                string tag = node.Tag.ToString();
                
                if (tag.StartsWith("database:"))
                {
                    // 数据库节点的右键菜单
                    CreateDatabaseContextMenu(contextMenu, node);
                }
                else if (tag.StartsWith("collection:"))
                {
                    // 集合节点的右键菜单
                    CreateCollectionContextMenu(contextMenu, node);
                }
                else if (tag.StartsWith("index:"))
                {
                    // 索引节点的右键菜单
                    CreateIndexContextMenu(node);
                    return node.ContextMenuStrip; // 返回已创建的索引菜单
                }
            }

            return contextMenu;
        }

        // 创建数据库节点的右键菜单
        private void CreateDatabaseContextMenu(ContextMenuStrip contextMenu, TreeNode dbNode)
        {
            string databaseName = dbNode.Text;
            
            // 新建集合菜单项
            ToolStripMenuItem menuItemNewCollection = new ToolStripMenuItem(LanguageManager.GetString("text_create_collection", "创建集合"));
            menuItemNewCollection.Tag = "text_create_collection";
            menuItemNewCollection.Click += (s, e) => CreateNewCollection(databaseName);
            contextMenu.Items.Add(menuItemNewCollection);
            
            // 导入目录数据菜单项
            ToolStripMenuItem menuItemImportDirectory = new ToolStripMenuItem(LanguageManager.GetString("text_import_directory", "导入目录数据"));
            menuItemImportDirectory.Tag = "text_import_directory";
            menuItemImportDirectory.Click += (s, e) => ImportDirectoryData(databaseName);
            contextMenu.Items.Add(menuItemImportDirectory);
            
            // 备份数据库菜单项
            ToolStripMenuItem menuItemBackupDatabase = new ToolStripMenuItem(LanguageManager.GetString("text_backup_database", "备份数据库"));
            menuItemBackupDatabase.Tag = "text_backup_database";
            menuItemBackupDatabase.Click += (s, e) => BackupDatabase(databaseName);
            contextMenu.Items.Add(menuItemBackupDatabase);
            
            // 添加分隔符
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // 关闭数据库连接菜单项
            ToolStripMenuItem menuItemCloseConnection = new ToolStripMenuItem(LanguageManager.GetString("text_close_database", "关闭数据库连接"));
            menuItemCloseConnection.Tag = "text_close_database";
            menuItemCloseConnection.Click += (s, e) => CloseDatabaseConnection(databaseName);
            contextMenu.Items.Add(menuItemCloseConnection);
        }

        // 创建集合节点的右键菜单
        private void CreateCollectionContextMenu(ContextMenuStrip contextMenu, TreeNode collNode)
        {
            
            // 打开集合菜单项
            ToolStripMenuItem menuItemOpen = new ToolStripMenuItem(LanguageManager.GetString("text_open_collection", "打开集合"));
            menuItemOpen.Tag = "text_open_collection";
            menuItemOpen.Click += MenuItemOpen_Click;
            contextMenu.Items.Add(menuItemOpen);
            
            // 添加分隔符
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // 清空集合菜单项
            ToolStripMenuItem menuItemClear = new ToolStripMenuItem(LanguageManager.GetString("text_clear_collection", "清空集合"));
            menuItemClear.Tag = "text_clear_collection";
            menuItemClear.Click += MenuItemClear_Click;
            contextMenu.Items.Add(menuItemClear);
            
            // 复制集合菜单项
            ToolStripMenuItem menuItemCopy = new ToolStripMenuItem(LanguageManager.GetString("text_copy_collection", "复制集合"));
            menuItemCopy.Tag = "text_copy_collection";
            menuItemCopy.Click += MenuItemCopy_Click;
            contextMenu.Items.Add(menuItemCopy);
            
            // 删除集合菜单项
            ToolStripMenuItem menuItemDelete = new ToolStripMenuItem(LanguageManager.GetString("text_delete_collection", "删除集合"));
            menuItemDelete.Tag = "text_delete_collection";
            menuItemDelete.Click += MenuItemDelete_Click;
            contextMenu.Items.Add(menuItemDelete);

            // 导出子菜单
            ToolStripMenuItem menuItemExport = new ToolStripMenuItem(LanguageManager.GetString("text_export_collection", "导出集合"));
            menuItemExport.Tag = "text_export_collection";
            ToolStripMenuItem menuItemExportJson = new ToolStripMenuItem(LanguageManager.GetString("text_export_json", "导出JSON"));
            menuItemExportJson.Tag = "text_export_json";
            menuItemExportJson.Click += MenuItemExportJson_Click;
            ToolStripMenuItem menuItemExportBson = new ToolStripMenuItem(LanguageManager.GetString("text_export_bson", "导出BSON"));
            menuItemExportBson.Tag = "text_export_bson";
            menuItemExportBson.Click += MenuItemExportBson_Click;
            ToolStripMenuItem menuItemExportCsv = new ToolStripMenuItem(LanguageManager.GetString("text_export_csv", "导出CSV"));
            menuItemExportCsv.Tag = "text_export_csv";
            menuItemExportCsv.Click += MenuItemExportCsv_Click;
            ToolStripMenuItem menuItemExportMsSql = new ToolStripMenuItem(LanguageManager.GetString("text_export_mssql", "导出MsSql脚本"));
            menuItemExportMsSql.Tag = "text_export_mssql";
            menuItemExportMsSql.Click += MenuItemExportMsSql_Click;
            ToolStripMenuItem menuItemExportMySql = new ToolStripMenuItem(LanguageManager.GetString("text_export_mysql", "导出Mysql脚本"));
            menuItemExportMySql.Tag = "text_export_mysql";
            menuItemExportMySql.Click += MenuItemExportMySql_Click;
            menuItemExport.DropDownItems.Add(menuItemExportJson);
            menuItemExport.DropDownItems.Add(menuItemExportBson);
            menuItemExport.DropDownItems.Add(menuItemExportCsv);
            menuItemExport.DropDownItems.Add(new ToolStripSeparator());
            menuItemExport.DropDownItems.Add(menuItemExportMsSql);
            menuItemExport.DropDownItems.Add(menuItemExportMySql);
            contextMenu.Items.Add(menuItemExport);

            // 导入子菜单
            ToolStripMenuItem menuItemImport = new ToolStripMenuItem(LanguageManager.GetString("text_import_collection", "导入集合"));
            menuItemImport.Tag = "text_import_collection";
            ToolStripMenuItem menuItemImportJson = new ToolStripMenuItem(LanguageManager.GetString("text_import_json", "导入JSON"));
            menuItemImportJson.Tag = "text_import_json";
            menuItemImportJson.Click += MenuItemImportJson_Click;
            ToolStripMenuItem menuItemImportBson = new ToolStripMenuItem(LanguageManager.GetString("text_import_bson", "导入BSON"));
            menuItemImportBson.Tag = "text_import_bson";
            menuItemImportBson.Click += MenuItemImportBson_Click;
            ToolStripMenuItem menuItemImportCsv = new ToolStripMenuItem(LanguageManager.GetString("text_import_csv", "导入CSV"));
            menuItemImportCsv.Tag = "text_import_csv";
            menuItemImportCsv.Click += MenuItemImportCsv_Click;
            menuItemImport.DropDownItems.Add(menuItemImportJson);
            menuItemImport.DropDownItems.Add(menuItemImportBson);
            menuItemImport.DropDownItems.Add(menuItemImportCsv);
            contextMenu.Items.Add(menuItemImport);
        }

        // 初始化Tab事件
        private void InitializeTabEvents()
        {
            // 绑定Tab选择事件
            tabControlData.SelectedIndexChanged += TabControlData_SelectedIndexChanged;
            
            // 为TabControl添加右键菜单
            ContextMenuStrip tabContextMenu = new ContextMenuStrip();
            tabContextMenu.Font = new Font("Microsoft YaHei", 9);
            
            // 关闭当前Tab菜单项
            ToolStripMenuItem menuItemCloseTab = new ToolStripMenuItem(LanguageManager.GetString("text_close_tab", "关闭标签页"));
            menuItemCloseTab.Tag = "text_close_tab";
            menuItemCloseTab.Click += MenuItemCloseTab_Click;
            tabContextMenu.Items.Add(menuItemCloseTab);
            
            // 关闭所有Tab菜单项
            ToolStripMenuItem menuItemCloseAllTabs = new ToolStripMenuItem(LanguageManager.GetString("text_close_all_tabs", "关闭所有标签页"));
            menuItemCloseAllTabs.Tag = "text_close_all_tabs";
            menuItemCloseAllTabs.Click += MenuItemCloseAllTabs_Click;
            tabContextMenu.Items.Add(menuItemCloseAllTabs);
            
            // 绑定右键菜单到TabControl
            tabControlData.ContextMenuStrip = tabContextMenu;
        }

        // Tab选择事件处理
        private void TabControlData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlData.SelectedTab != null)
            {
                // 高亮对应的集合节点
                HighlightCollectionNode(tabControlData.SelectedTab);
            }
        }

        // 高亮对应的集合节点
        private void HighlightCollectionNode(TabPage selectedTab)
        {
            try
            {
                if (selectedTab.Tag != null && selectedTab.Tag.ToString().StartsWith("collection:"))
                {
                    string[] parts = selectedTab.Tag.ToString().Split(':')[1].Split('.');
                    string databaseName = parts[0];
                    string collectionName = parts[1];
                    
                    // 查找并选中对应的集合节点
                    foreach (TreeNode dbNode in treeViewCollections.Nodes[0].Nodes)
                    {
                        if (dbNode.Text == databaseName)
                        {
                            foreach (TreeNode collectionNode in dbNode.Nodes)
                            {
                                if (collectionNode.Text == collectionName)
                                {
                                    // 展开数据库节点
                                    dbNode.Expand();
                                    // 选中集合节点
                                    treeViewCollections.SelectedNode = collectionNode;
                                    // 确保节点可见
                                    collectionNode.EnsureVisible();
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("高亮节点失败: {0}", ex.Message), Color.Orange);
            }
        }

        // 关闭当前Tab事件处理
        private void MenuItemCloseTab_Click(object sender, EventArgs e)
        {
            if (tabControlData.SelectedTab != null)
            {
                string tabTitle = tabControlData.SelectedTab.Text;
                tabControlData.TabPages.Remove(tabControlData.SelectedTab);
                SetStatus(string.Format("已关闭Tab: {0}", tabTitle), Color.Blue);
            }
        }

        // 关闭所有Tab事件处理
        private void MenuItemCloseAllTabs_Click(object sender, EventArgs e)
        {
            if (tabControlData.TabPages.Count > 0)
            {
                if (MessageBox.Show(LanguageManager.GetString("msg_close_all_tabs_confirm", "确定要关闭所有Tab吗？"), 
                LanguageManager.GetString("dialog_confirm", "确认关闭"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int tabCount = tabControlData.TabPages.Count;
                    tabControlData.TabPages.Clear();
                    SetStatus(string.Format("已关闭所有 {0} 个Tab", tabCount), Color.Blue);
                }
            }
        }

        // 清空集合事件处理
        private void MenuItemClear_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode != null && 
                treeViewCollections.SelectedNode.Tag != null && 
                treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
                string databaseName = parts[0];
                string collectionName = parts[1];
                
                            if (MessageBox.Show(string.Format(LanguageManager.GetString("msg_clear_collection_confirm", "确定要清空集合 '{0}' 吗？\n此操作不可恢复！"), 
                $"{databaseName}.{collectionName}"),
                LanguageManager.GetString("dialog_confirm", "确认清空"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    ClearCollection(databaseName, collectionName);
                }
            }
        }

        // 复制集合事件处理
        private void MenuItemCopy_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode != null && 
                treeViewCollections.SelectedNode.Tag != null && 
                treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
                string databaseName = parts[0];
                string collectionName = parts[1];
                
                string newCollectionName = Microsoft.VisualBasic.Interaction.InputBox(
                    string.Format("请输入新集合名称 (原集合: {0})", collectionName),
                    "复制集合",
                    collectionName + "_copy");
                
                if (!string.IsNullOrEmpty(newCollectionName))
                {
                    CopyCollection(databaseName, collectionName, newCollectionName);
                }
            }
        }

        // 删除集合事件处理
        private void MenuItemDelete_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode != null && 
                treeViewCollections.SelectedNode.Tag != null && 
                treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
                string databaseName = parts[0];
                string collectionName = parts[1];
                
                            if (MessageBox.Show(string.Format(LanguageManager.GetString("msg_delete_collection_confirm", "确定要删除集合 '{0}' 吗？\n此操作不可恢复！"), 
                $"{databaseName}.{collectionName}"),
                LanguageManager.GetString("dialog_confirm", "确认删除"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DeleteCollection(databaseName, collectionName);
                }
            }
        }

        // 导出为 .json
        private void MenuItemExportJson_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_select_collection", "请先选择要操作的集合节点"), 
                    LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON文件 (*.json)|*.json";
            sfd.FileName = collectionName + ".json";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        foreach (var doc in documents)
                        {
                            sw.WriteLine(doc.ToJson());
                        }
                    }
                    SetStatus($"集合 {databaseName}.{collectionName} 已导出为 JSON，共 {documents.Count} 条记录", Color.Green);
                    MessageBox.Show(LanguageManager.GetString("msg_export_success_simple", "导出成功！"), 
                        LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}").Replace("{0}", ex.Message), 
                        LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 导出为 .bson
        private void MenuItemExportBson_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_select_collection", "请先选择要操作的集合节点"), 
                    LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "BSON文件 (*.bson)|*.bson";
            sfd.FileName = collectionName + ".bson";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                    {
                        foreach (var doc in documents)
                        {
                            var bytes = doc.ToBson();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                    SetStatus($"集合 {databaseName}.{collectionName} 已导出为 BSON，共 {documents.Count} 条记录", Color.Green);
                    MessageBox.Show(LanguageManager.GetString("msg_export_success_simple", "导出成功！"), 
                        LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 从 .json 导入
        private void MenuItemImportJson_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show("请先选择要导入的集合节点。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON文件 (*.json)|*.json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var lines = File.ReadAllLines(ofd.FileName);
                    var docs = new List<BsonDocument>();
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            docs.Add(BsonDocument.Parse(line));
                        }
                    }
                    if (docs.Count > 0)
                    {
                        collection.InsertMany(docs);
                        SetStatus($"已从 JSON 导入 {docs.Count} 条记录到 {databaseName}.{collectionName}", Color.Green);
                        MessageBox.Show("导入成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("文件内容为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 从 .bson 导入
        private void MenuItemImportBson_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show("请先选择要导入的集合节点。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BSON文件 (*.bson)|*.bson";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var docs = new List<BsonDocument>();
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        while (fs.Position < fs.Length)
                        {
                            var doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(fs);
                            docs.Add(doc);
                        }
                    }
                    if (docs.Count > 0)
                    {
                        collection.InsertMany(docs);
                        SetStatus($"已从 BSON 导入 {docs.Count} 条记录到 {databaseName}.{collectionName}", Color.Green);
                        MessageBox.Show("导入成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("文件内容为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 清空集合
        private void ClearCollection(string databaseName, string collectionName)
        {
            try
            {
                SetStatus(string.Format("正在清空集合 {0}.{1}...", databaseName, collectionName));
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                // 删除所有文档
                var result = collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
                
                SetStatus(string.Format("集合 {0}.{1} 已清空，删除了 {2} 条记录", databaseName, collectionName, result.DeletedCount), Color.Green);
                
                // 刷新集合列表
                LoadCollections(databaseName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("清空集合失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("清空集合失败: {0}", ex.Message), Color.Red);
            }
        }

        // 复制集合
        private void CopyCollection(string databaseName, string sourceCollectionName, string targetCollectionName)
        {
            try
            {
                SetStatus(string.Format("正在复制集合 {0}.{1} 到 {0}.{2}...", databaseName, sourceCollectionName, targetCollectionName));
                
                var database = connectedDatabases[databaseName];
                var sourceCollection = database.GetCollection<BsonDocument>(sourceCollectionName);
                var targetCollection = database.GetCollection<BsonDocument>(targetCollectionName);
                
                // 获取所有文档
                var documents = sourceCollection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                
                if (documents.Count > 0)
                {
                    // 插入到目标集合
                    targetCollection.InsertMany(documents);
                    SetStatus(string.Format("集合复制完成，复制了 {0} 条记录", documents.Count), Color.Green);
                }
                else
                {
                    SetStatus(string.Format("集合 {0}.{1} 为空，无需复制", databaseName, sourceCollectionName), Color.Blue);
                }
                
                // 刷新集合列表
                LoadCollections(databaseName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("复制集合失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("复制集合失败: {0}", ex.Message), Color.Red);
            }
        }

        // 删除集合
        private void DeleteCollection(string databaseName, string collectionName)
        {
            try
            {
                SetStatus(string.Format("正在删除集合 {0}.{1}...", databaseName, collectionName));
                
                var database = connectedDatabases[databaseName];
                
                // 删除集合
                database.DropCollection(collectionName);
                
                SetStatus(string.Format("集合 {0}.{1} 已删除", databaseName, collectionName), Color.Green);
                
                // 刷新集合列表
                LoadCollections(databaseName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("删除集合失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("删除集合失败: {0}", ex.Message), Color.Red);
            }
        }

        private void SetStatus(string text, Color? color = null)
        {
            // 更新状态栏
            toolStripStatusLabel1.Text = text;
            if (color != null)
            {
                toolStripStatusLabel1.ForeColor = color.Value;
            }
        }

        // 工具条连接配置按钮事件
        private void toolStripButtonConnect_Click(object sender, EventArgs e)
        {
            ShowConnectDialog();
        }

        // 工具条重连按钮事件
        private void toolStripButtonReconnect_Click(object sender, EventArgs e)
        {
            ManualReconnectToLastDatabase();
        }

        // 工具条刷新按钮事件
        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            RefreshCollections();
        }



        // 显示连接配置对话框
        private void ShowConnectDialog()
        {
            try
            {
                // 创建连接配置窗口
                ConnListForm configForm = new ConnListForm();
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // 获取连接信息并连接到数据库
                    string connStr = configForm.connectionString;
                    string database = configForm.DatabaseName;
                    
                    if (!string.IsNullOrEmpty(connStr) && !string.IsNullOrEmpty(database))
                    {
                        ConnectToMongoDB(connStr, database);
                        SetStatus("连接配置完成", Color.Green);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}"), ex.Message), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 刷新集合列表
        private void RefreshCollections()
        {
            try
            {
                // 刷新所有数据库的集合列表
                foreach (var kvp in connectedDatabases)
                {
                    LoadCollections(kvp.Key);
                }
                SetStatus("集合列表已刷新", Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("刷新失败: {0}", ex.Message), Color.Red);
            }
        }



        // 记录右键菜单事件处理
        private void EditRecord_Click(DataGridView dataGridView, string databaseName, string collectionName)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_select_record_to_edit", "请先选择要修改的记录。"), 
                    LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var row = dataGridView.SelectedRows[0];
            // 用Tag里的原始BsonDocument
            BsonDocument doc = row.Tag as BsonDocument;
            if (doc == null || !doc.Contains("_id"))
            {
                MessageBox.Show(LanguageManager.GetString("msg_cannot_get_record_id", "无法获取记录的 _id 字段，无法修改。"), 
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string json = doc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true });

            // 自定义弹窗：大窗口+多行TextBox
            Form editForm = new Form();
            editForm.Text = LanguageManager.GetString("dialog_edit_record", "修改记录");
            editForm.Size = new Size(600, 500);
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;

            Label label = new Label();
            label.Text = LanguageManager.GetString("text_please_edit_json_format", "请编辑JSON（注意格式）:");
            label.Location = new Point(10, 10);
            label.Size = new Size(560, 20);

            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Font = new Font("Consolas", 11);
            textBox.Location = new Point(10, 40);
            textBox.Size = new Size(560, 360);
            textBox.Text = json;
            textBox.AcceptsTab = true;

            Button btnOK = new Button();
            btnOK.Text = LanguageManager.GetString("btn_ok", "确定");
            btnOK.Location = new Point(380, 420);
            btnOK.Size = new Size(80, 32);
            btnOK.DialogResult = DialogResult.OK;
            btnOK.BackColor = Color.FromArgb(40, 167, 69);
            btnOK.ForeColor = Color.White;
            btnOK.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);

            Button btnCancel = new Button();
            btnCancel.Text = LanguageManager.GetString("btn_cancel", "取消");
            btnCancel.Location = new Point(490, 420);
            btnCancel.Size = new Size(80, 32);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.BackColor = Color.FromArgb(220, 53, 69);
            btnCancel.ForeColor = Color.White;
            btnCancel.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);

            editForm.Controls.Add(label);
            editForm.Controls.Add(textBox);
            editForm.Controls.Add(btnOK);
            editForm.Controls.Add(btnCancel);
            editForm.AcceptButton = btnOK;
            editForm.CancelButton = btnCancel;

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                string input = textBox.Text;
                if (!string.IsNullOrWhiteSpace(input) && input != json)
                {
                    try
                    {
                        var newDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(input);
                        var database = connectedDatabases[databaseName];
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                        collection.ReplaceOne(filter, newDoc);
                        SetStatus("记录已修改", Color.Green);
                        MessageBox.Show(LanguageManager.GetString("msg_edit_success", "修改成功！"), 
                            LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // 自动刷新当前查询结果
                        ExecuteQuery(databaseName, collectionName, "{}", dataGridView);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(LanguageManager.GetString("msg_edit_failed", "修改失败: ") + ex.Message, 
                            LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CopyRecord_Click(DataGridView dataGridView, string databaseName, string collectionName)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要复制的记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var row = dataGridView.SelectedRows[0];
            var doc = new BsonDocument();
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn.Name == "_id") continue; // 不复制 _id
                doc[cell.OwningColumn.Name] = BsonValue.Create(cell.Value);
            }
            try
            {
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                collection.InsertOne(doc);
                SetStatus("记录已复制（新文档已插入）", Color.Green);
                MessageBox.Show("复制成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("复制失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRecord_Click(DataGridView dataGridView, string databaseName, string collectionName)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var row = dataGridView.SelectedRows[0];
            // 修正：从Tag获取原始BsonDocument的_id
            BsonValue idValue = null;
            if (row.Tag is BsonDocument doc && doc.Contains("_id"))
            {
                idValue = doc["_id"];
            }
            else
            {
                MessageBox.Show("无法获取记录的 _id 字段，无法删除。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确定要删除该记录吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", idValue);
                    var result = collection.DeleteOne(filter);
                    if (result.DeletedCount > 0)
                    {
                        dataGridView.Rows.Remove(row);
                        SetStatus("记录已删除", Color.Green);
                    }
                    else
                    {
                        MessageBox.Show("未能删除记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("删除失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ToolStripButtonNativeQuery_Click(object sender, EventArgs e)
        {
            // 新建Tab页
            TabPage tab = new TabPage(LanguageManager.GetString("menu_native_query", "原生语句"));
            tab.ToolTipText = LanguageManager.GetString("text_native_query_execution", "原生MongoDB语句执行");
            tabControlData.TabPages.Add(tab);
            tabControlData.SelectedTab = tab;

            // 使用TableLayoutPanel自适应布局
            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 1;
            table.RowCount = 6;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // 数据库选择
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 语句label
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 25));  // 语句输入
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // 执行按钮
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // 结果label
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 75));  // 结果区
            tab.Controls.Add(table);

            // 数据库选择区
            Panel dbPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 5, 10, 5) };
            Label labelDb = new Label { Text = LanguageManager.GetString("text_database_label", "数据库:"), AutoSize = true, Location = new Point(0, 10), Font = new Font("Microsoft YaHei", 10) };
            ComboBox comboBoxDb = new ComboBox { Location = new Point(70, 7), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Microsoft YaHei", 10) };
            comboBoxDb.Items.AddRange(connectedDatabases.Keys.ToArray());
            if (comboBoxDb.Items.Count > 0) comboBoxDb.SelectedIndex = 0;
            dbPanel.Controls.Add(labelDb);
            dbPanel.Controls.Add(comboBoxDb);
            table.Controls.Add(dbPanel, 0, 0);

            // 语句label
            Label labelInput = new Label { Text = LanguageManager.GetString("text_native_operation_statement", "原生操作语句 (如 db.collection.find({})):"), Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10), Padding = new Padding(10, 0, 0, 0) };
            table.Controls.Add(labelInput, 0, 1);

            // 语句输入区
            TextBox textBox = new TextBox { Multiline = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 12), Dock = DockStyle.Fill, AcceptsTab = true, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(8, 0, 8, 0) };
            table.Controls.Add(textBox, 0, 2);
            textBox.Text = "db..find({})";

            // 执行按钮区
            Panel btnPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 5, 10, 5) };
            Button btnExec = new Button { Text = LanguageManager.GetString("text_execute_button", "执行"), Size = new Size(100, 36), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 11, FontStyle.Bold) };
            btnExec.Dock = DockStyle.Right;
            btnExec.Margin = new Padding(0, 0, 10, 0); // 右侧留10像素
            btnPanel.Controls.Add(btnExec);
            table.Controls.Add(btnPanel, 0, 3);

            // 结果label
            Label labelResult = new Label { Text = LanguageManager.GetString("text_execution_result", "执行结果:"), Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10), Padding = new Padding(10, 0, 0, 0) };
            table.Controls.Add(labelResult, 0, 4);

            // 结果区（文本框和表格重叠，切换显示）
            Panel resultPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 10) };
            TextBox textResult = new TextBox { Multiline = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 11), Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle, Visible = true };
            DataGridView dataGridView = new DataGridView { Dock = DockStyle.Fill, Visible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, GridColor = Color.FromArgb(224, 224, 224), BorderStyle = BorderStyle.None };
            resultPanel.Controls.Add(textResult);
            resultPanel.Controls.Add(dataGridView);
            table.Controls.Add(resultPanel, 0, 5);

            // 执行按钮事件
            btnExec.Click += (s, ev) =>
            {
                textResult.Text = "";
                textResult.Visible = true;
                dataGridView.Visible = false;
                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();
                if (comboBoxDb.SelectedItem == null)
                {
                    textResult.Text = LanguageManager.GetString("text_please_select_database", "请选择数据库");
                    return;
                }
                string dbName = comboBoxDb.SelectedItem.ToString();
                var db = connectedDatabases[dbName];
                string input = textBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(input))
                {
                    textResult.Text = LanguageManager.GetString("text_please_enter_native_statement", "请输入原生MongoDB语句，如 db.collection.find({})");
                    return;
                }
                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(input, @"db\.(\w+)\.(\w+)\((.*)\)");
                    if (!match.Success)
                    {
                        textResult.Text = LanguageManager.GetString("text_only_support_db_collection_form", "暂仅支持 db.collection.method(args) 形式");
                        return;
                    }
                    string coll = match.Groups[1].Value;
                    string method = match.Groups[2].Value.ToLower();
                    string arg = match.Groups[3].Value;
                    var collection = db.GetCollection<BsonDocument>(coll);
                    if (method == "find")
                    {
                        var filter = string.IsNullOrWhiteSpace(arg) ? "{}" : arg;
                        var docs = collection.Find(BsonDocument.Parse(filter)).Limit(1000).ToList();
                        if (docs.Count == 0)
                        {
                            textResult.Text = LanguageManager.GetString("text_no_records", "无记录");
                            return;
                        }
                        // 显示到DataGridView
                        HashSet<string> allFields = new HashSet<string>();
                        foreach (var d in docs) foreach (var el in d.Elements) allFields.Add(el.Name);
                        foreach (string field in allFields.OrderBy(f => f)) dataGridView.Columns.Add(field, field);
                        foreach (var d in docs)
                        {
                            var row = new object[allFields.Count];
                            int colIndex = 0;
                            foreach (string field in allFields.OrderBy(f => f))
                                row[colIndex++] = d.Contains(field) ? d[field].ToString() : "";
                            dataGridView.Rows.Add(row);
                        }
                        dataGridView.Visible = true;
                        textResult.Visible = false;
                        textResult.Text = $"共 {docs.Count} 条记录";
                    }
                    else if (method == "deletemany" || method == "deleteone")
                    {
                        var filter = string.IsNullOrWhiteSpace(arg) ? "{}" : arg;
                        var result = method == "deletemany" ? collection.DeleteMany(BsonDocument.Parse(filter)) : collection.DeleteOne(BsonDocument.Parse(filter));
                        textResult.Text = $"删除操作完成，删除数量: {result.DeletedCount}";
                    }
                    else if (method == "updatemany" || method == "updateone")
                    {
                        var args = SplitArgs(arg);
                        if (args.Length != 2)
                        {
                            textResult.Text = "update语句参数应为2个：过滤条件和更新内容";
                            return;
                        }
                        var filter = BsonDocument.Parse(args[0]);
                        var update = BsonDocument.Parse(args[1]);
                        var result = method == "updatemany" ? collection.UpdateMany(filter, update) : collection.UpdateOne(filter, update);
                        textResult.Text = $"更新操作完成，匹配: {result.MatchedCount}，修改: {result.ModifiedCount}";
                    }
                    else if (method == "insertone" || method == "insertmany")
                    {
                        var docs = new List<BsonDocument>();
                        if (method == "insertone")
                            docs.Add(BsonDocument.Parse(arg));
                        else
                        {
                            var arr = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(arg);
                            foreach (var d in arr) docs.Add(d.AsBsonDocument);
                        }
                        collection.InsertMany(docs);
                        textResult.Text = $"插入操作完成，插入数量: {docs.Count}";
                    }
                    else if (method == "aggregate")
                    {
                        var pipeline = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(arg);
                        var stages = pipeline.Values.Select(v => v.AsBsonDocument).ToArray();
                        var docs = collection.Aggregate<BsonDocument>(stages).ToList();
                        if (docs.Count == 0)
                        {
                            textResult.Text = LanguageManager.GetString("text_no_records", "无记录");
                            return;
                        }
                        HashSet<string> allFields = new HashSet<string>();
                        foreach (var d in docs) foreach (var el in d.Elements) allFields.Add(el.Name);
                        foreach (string field in allFields.OrderBy(f => f)) dataGridView.Columns.Add(field, field);
                        foreach (var d in docs)
                        {
                            var row = new object[allFields.Count];
                            int colIndex = 0;
                            foreach (string field in allFields.OrderBy(f => f))
                                row[colIndex++] = d.Contains(field) ? d[field].ToString() : "";
                            dataGridView.Rows.Add(row);
                        }
                        dataGridView.Visible = true;
                        textResult.Visible = false;
                        textResult.Text = $"共 {docs.Count} 条记录";
                    }
                    else
                    {
                        textResult.Text = LanguageManager.GetString("text_operation_not_supported", "暂不支持该操作");
                    }
                }
                catch (Exception ex)
                {
                    textResult.Text = LanguageManager.GetString("text_execution_failed", "执行失败: ") + ex.Message;
                }
            };
        }

        // 辅助：分割update等的参数
        private string[] SplitArgs(string arg)
        {
            // 简单分割，假设参数用逗号分隔且无嵌套逗号
            var args = new List<string>();
            int depth = 0, last = 0;
            for (int i = 0; i < arg.Length; i++)
            {
                if (arg[i] == '{' || arg[i] == '[') depth++;
                if (arg[i] == '}' || arg[i] == ']') depth--;
                if (arg[i] == ',' && depth == 0)
                {
                    args.Add(arg.Substring(last, i - last).Trim());
                    last = i + 1;
                }
            }
            if (last < arg.Length)
                args.Add(arg.Substring(last).Trim());
            return args.ToArray();
        }

        // 集合过滤逻辑（只显示匹配节点，支持无关键词恢复树和无匹配提示）
        private void TextBoxCollectionFilter_TextChanged(object sender, EventArgs e)
        {
            string keyword = textBoxCollectionFilter.Text.Trim().ToLower();
            treeViewCollections.BeginUpdate();
            
            // 清空所有节点
            treeViewCollections.Nodes.Clear();
            
            bool hasMatch = false;
            if (string.IsNullOrEmpty(keyword))
            {
                // 恢复完整树
                foreach (var kv in allCollections)
                {
                    string dbName = kv.Key;
                    var allColls = kv.Value;
                    if (allColls.Count == 0) continue;
                    TreeNode dbNode = new TreeNode(dbName)
                    {
                        NodeFont = new Font("Microsoft YaHei", 9, FontStyle.Bold),
                        Tag = "database:" + dbName
                    };
                    foreach (var coll in allColls)
                    {
                        TreeNode collNode = new TreeNode(coll)
                        {
                            Tag = "collection:" + dbName + "." + coll
                        };
                        
                        // 为每个集合添加"索引"子节点
                        TreeNode indexNode = new TreeNode(LanguageManager.GetString("text_index", "索引"));
                        indexNode.Tag = string.Format("indexes:{0}.{1}", dbName, coll);
                        indexNode.NodeFont = new Font("Microsoft YaHei", 9);
                        indexNode.ForeColor = Color.FromArgb(128, 128, 128);
                        
                        // 为索引节点创建右键菜单
                        CreateIndexesNodeContextMenu(indexNode, dbName, coll);
                        
                        collNode.Nodes.Add(indexNode);
                        dbNode.Nodes.Add(collNode);
                    }
                    if (dbNode.Nodes.Count > 0)
                    {
                        treeViewCollections.Nodes.Add(dbNode);
                        hasMatch = true;
                    }
                }
            }
            else
            {
                foreach (var kv in allCollections)
                {
                    string dbName = kv.Key;
                    var allColls = kv.Value;
                    var matchColls = allColls.Where(c => c.ToLower().Contains(keyword)).ToList();
                    if (matchColls.Count == 0) continue;
                    TreeNode dbNode = new TreeNode(dbName)
                    {
                        NodeFont = new Font("Microsoft YaHei", 9, FontStyle.Bold),
                        Tag = "database:" + dbName
                    };
                    foreach (var coll in matchColls)
                    {
                        TreeNode collNode = new TreeNode(coll)
                        {
                            Tag = "collection:" + dbName + "." + coll
                        };
                        
                        // 为每个集合添加"索引"子节点
                        TreeNode indexNode = new TreeNode(LanguageManager.GetString("text_index", "索引"));
                        indexNode.Tag = string.Format("indexes:{0}.{1}", dbName, coll);
                        indexNode.NodeFont = new Font("Microsoft YaHei", 9);
                        indexNode.ForeColor = Color.FromArgb(128, 128, 128);
                        
                        // 为索引节点创建右键菜单
                        CreateIndexesNodeContextMenu(indexNode, dbName, coll);
                        
                        collNode.Nodes.Add(indexNode);
                        dbNode.Nodes.Add(collNode);
                    }
                    if (dbNode.Nodes.Count > 0)
                    {
                        treeViewCollections.Nodes.Add(dbNode);
                        hasMatch = true;
                    }
                }
            }
            if (!hasMatch)
            {
                TreeNode emptyNode = new TreeNode(LanguageManager.GetString("text_no_matching_collections", "无匹配集合"))
                {
                    ForeColor = Color.Gray
                };
                treeViewCollections.Nodes.Add(emptyNode);
            }
            // 自动展开所有数据库节点
            foreach (TreeNode dbNode in treeViewCollections.Nodes)
            {
                dbNode.Expand();
            }
            treeViewCollections.EndUpdate();
        }





        // 加载集合索引到指定节点
        private void LoadCollectionIndexes(string databaseName, string collectionName, TreeNode indexNode)
        {
            try
            {
                SetStatus(string.Format("正在加载集合 {0}.{1} 的索引...", databaseName, collectionName));
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                // 获取集合的索引列表
                var indexes = collection.Indexes.List().ToList();
                
                // 清空现有节点
                indexNode.Nodes.Clear();
                
                if (indexes.Count == 0)
                {
                    // 如果没有索引，添加一个提示节点
                    TreeNode noIndexNode = new TreeNode(LanguageManager.GetString("text_no_indexes", "暂无索引"));
                    noIndexNode.Tag = "no_index";
                    noIndexNode.NodeFont = new Font("Microsoft YaHei", 9);
                    noIndexNode.ForeColor = Color.FromArgb(150, 150, 150);
                    indexNode.Nodes.Add(noIndexNode);
                }
                else
                {
                    // 添加索引节点
                    foreach (var index in indexes)
                    {
                        string indexName = index["name"].AsString;
                        string indexKey = index["key"].ToJson();
                        string displayName = indexName;
                        
                        // 如果是默认的_id索引，显示为"_id (默认)"
                        if (indexName == "_id_")
                        {
                            displayName = "_id (" + LanguageManager.GetString("text_default", "(默认)") + ")";
                        }
                        
                        TreeNode indexItemNode = new TreeNode(displayName);
                        indexItemNode.Tag = string.Format("index:{0}.{1}.{2}", databaseName, collectionName, indexName);
                        indexItemNode.NodeFont = new Font("Microsoft YaHei", 9);
                        indexItemNode.ForeColor = Color.FromArgb(64, 64, 64);
                        indexItemNode.ToolTipText = string.Format(LanguageManager.GetString("text_index_key", "索引键") + ": {0}", indexKey);
                        
                        // 为索引节点创建右键菜单
                        CreateIndexContextMenu(indexItemNode);
                        
                        indexNode.Nodes.Add(indexItemNode);
                    }
                }
                
                // 为索引节点创建右键菜单
                CreateIndexesNodeContextMenu(indexNode, databaseName, collectionName);
                
                // 展开索引节点
                indexNode.Expand();
                
                SetStatus(string.Format("已加载 {0} 个索引", indexes.Count), Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("加载索引失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("加载索引失败: {0}", ex.Message), Color.Red);
            }
        }

        // 显示集合索引
        private void ShowCollectionIndexes(string databaseName, string collectionName)
        {
            try
            {
                SetStatus(string.Format("正在加载集合 {0}.{1} 的索引...", databaseName, collectionName));
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                // 获取索引列表
                var indexes = collection.Indexes.List().ToList();
                
                // 在树形控件中添加索引节点
                TreeNode collectionNode = null;
                foreach (TreeNode dbNode in treeViewCollections.Nodes)
                {
                    if (dbNode.Text == databaseName)
                    {
                        foreach (TreeNode collNode in dbNode.Nodes)
                        {
                            if (collNode.Text == collectionName)
                            {
                                collectionNode = collNode;
                                break;
                            }
                        }
                        break;
                    }
                }
                
                if (collectionNode != null)
                {
                    // 清除现有的索引节点
                    var nodesToRemove = new List<TreeNode>();
                    foreach (TreeNode node in collectionNode.Nodes)
                    {
                        if (node.Tag != null && node.Tag.ToString().StartsWith("index:"))
                        {
                            nodesToRemove.Add(node);
                        }
                    }
                    foreach (var node in nodesToRemove)
                    {
                        collectionNode.Nodes.Remove(node);
                    }
                    
                    // 添加索引节点
                    foreach (var index in indexes)
                    {
                        string indexName = index["name"].AsString;
                        string displayName = indexName;
                        
                        // 如果是默认的_id索引，显示为"_id (默认)"
                        if (indexName == "_id_")
                        {
                            displayName = "_id (" + LanguageManager.GetString("text_default", "(默认)") + ")";
                        }
                        
                        TreeNode indexNode = new TreeNode(displayName);
                        indexNode.Tag = string.Format("index:{0}.{1}.{2}", databaseName, collectionName, indexName);
                        indexNode.NodeFont = new Font("Microsoft YaHei", 8);
                        indexNode.ForeColor = Color.FromArgb(100, 100, 100);
                        indexNode.ImageIndex = 1; // 可以设置不同的图标
                        collectionNode.Nodes.Add(indexNode);
                        
                        // 为索引节点创建右键菜单
                        CreateIndexContextMenu(indexNode);
                    }
                    
                    // 展开集合节点
                    collectionNode.Expand();
                    SetStatus(string.Format("已加载 {0} 个索引", indexes.Count), Color.Green);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("加载索引失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("加载索引失败: {0}", ex.Message), Color.Red);
            }
        }

        // 为索引节点创建右键菜单
        private void CreateIndexesNodeContextMenu(TreeNode indexNode, string databaseName, string collectionName)
        {
            ContextMenuStrip indexContextMenu = new ContextMenuStrip();
            indexContextMenu.Font = new Font("Microsoft YaHei", 9);
            
            // 刷新索引列表
            ToolStripMenuItem menuItemRefresh = new ToolStripMenuItem(LanguageManager.GetString("text_refresh_indexes", "刷新索引"));
            menuItemRefresh.Tag = "text_refresh_indexes";
            menuItemRefresh.Click += (s, e) => LoadCollectionIndexes(databaseName, collectionName, indexNode);
            indexContextMenu.Items.Add(menuItemRefresh);
            
            // 添加分隔符
            indexContextMenu.Items.Add(new ToolStripSeparator());
            
            // 创建新索引
            ToolStripMenuItem menuItemCreate = new ToolStripMenuItem(LanguageManager.GetString("text_create_index", "创建索引"));
            menuItemCreate.Tag = "text_create_index";
            menuItemCreate.Click += (s, e) => CreateIndex(databaseName, collectionName);
            indexContextMenu.Items.Add(menuItemCreate);
            
            indexNode.ContextMenuStrip = indexContextMenu;
        }

        // 为索引节点创建右键菜单
        private void CreateIndexContextMenu(TreeNode indexNode)
        {
            if (indexNode.Tag == null || !indexNode.Tag.ToString().StartsWith("index:"))
                return;
                
            ContextMenuStrip indexContextMenu = new ContextMenuStrip();
            indexContextMenu.Font = new Font("Microsoft YaHei", 9);
            
            // 查看索引详情
            ToolStripMenuItem menuItemView = new ToolStripMenuItem(LanguageManager.GetString("text_view_details", "查看详情"));
            menuItemView.Tag = "text_view_details";
            menuItemView.Click += (s, e) => ViewIndexDetails(indexNode);
            indexContextMenu.Items.Add(menuItemView);
            
            // 获取索引名称
            string[] parts = indexNode.Tag.ToString().Split(':')[1].Split('.');
            string indexName = parts[2];
            
            // 如果不是默认的_id索引，才显示删除选项
            if (indexName != "_id_")
            {
                // 添加分隔符
                indexContextMenu.Items.Add(new ToolStripSeparator());
                
                // 删除索引
                ToolStripMenuItem menuItemDelete = new ToolStripMenuItem(LanguageManager.GetString("text_delete_index", "删除索引"));
                menuItemDelete.Tag = "text_delete_index";
                menuItemDelete.Click += (s, e) => DeleteIndex(indexNode);
                indexContextMenu.Items.Add(menuItemDelete);
            }
            
            indexNode.ContextMenuStrip = indexContextMenu;
        }

        // 查看索引详情
        private void ViewIndexDetails(TreeNode indexNode)
        {
            try
            {
                string[] parts = indexNode.Tag.ToString().Split(':')[1].Split('.');
                string databaseName = parts[0];
                string collectionName = parts[1];
                string indexName = parts[2];
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                // 获取索引详情
                var indexes = collection.Indexes.List().ToList();
                var indexDetails = indexes.FirstOrDefault(idx => idx["name"].AsString == indexName);
                
                if (indexDetails != null)
                {
                    string details = indexDetails.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true });
                    
                    Form detailsForm = new Form();
                    detailsForm.Text = string.Format("索引详情 - {0}", indexName);
                    detailsForm.Size = new Size(600, 400);
                    detailsForm.StartPosition = FormStartPosition.CenterParent;
                    detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    detailsForm.MaximizeBox = false;
                    detailsForm.MinimizeBox = false;
                    
                    TextBox textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Font = new Font("Consolas", 10);
                    textBox.Dock = DockStyle.Fill;
                    textBox.Text = details;
                    textBox.ReadOnly = true;
                    textBox.BackColor = Color.WhiteSmoke;
                    
                    detailsForm.Controls.Add(textBox);
                    detailsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("查看索引详情失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 删除索引
        private void DeleteIndex(TreeNode indexNode)
        {
            try
            {
                string[] parts = indexNode.Tag.ToString().Split(':')[1].Split('.');
                string databaseName = parts[0];
                string collectionName = parts[1];
                string indexName = parts[2];
                
                // 保护默认的_id索引
                if (indexName == "_id_")
                {
                    MessageBox.Show("不能删除默认的 _id 索引！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (MessageBox.Show(string.Format("确定要删除索引 '{0}' 吗？\n此操作不可恢复！", indexName), 
                    "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    
                    // 删除索引
                    collection.Indexes.DropOne(indexName);
                    
                    // 从树形控件中移除节点
                    indexNode.Remove();
                    
                    SetStatus(string.Format("索引 '{0}' 已删除", indexName), Color.Green);
                    MessageBox.Show("索引删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 刷新索引列表
                    ShowCollectionIndexes(databaseName, collectionName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("删除索引失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("删除索引失败: {0}", ex.Message), Color.Red);
            }
        }

        // 创建索引
        private void CreateIndex(string databaseName, string collectionName)
        {
            try
            {
                Form createForm = new Form();
                createForm.Text = string.Format("创建索引 - {0}.{1}", databaseName, collectionName);
                createForm.Size = new Size(700, 550);
                createForm.StartPosition = FormStartPosition.CenterParent;
                createForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                createForm.MaximizeBox = false;
                createForm.MinimizeBox = false;
                
                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 1;
                layout.RowCount = 7;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 索引名称标签
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // 索引名称输入框
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 字段定义标签
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));   // 字段定义输入框
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 索引选项标签
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));   // 索引选项输入框 + 按钮
                
                // 索引名称
                Label labelName = new Label { Text = LanguageManager.GetString("text_index_name_label", "索引名称:"), AutoSize = true, Font = new Font("Microsoft YaHei", 9, FontStyle.Bold), Margin = new Padding(5, 5, 5, 0) };
                TextBox textBoxName = new TextBox { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 9), Margin = new Padding(5, 0, 5, 5) };
                textBoxName.Text = "idx_" + collectionName + "_"; // 默认前缀
                
                // 字段定义
                Label labelFields = new Label { Text = LanguageManager.GetString("text_field_definition_json", "字段定义 (JSON):"), AutoSize = true, Font = new Font("Microsoft YaHei", 9, FontStyle.Bold), Margin = new Padding(5, 5, 5, 0) };
                TextBox textBoxFields = new TextBox { Multiline = true, Font = new Font("Consolas", 10), Text = "{\n  \"field1\": 1,\n  \"field2\": -1\n}", Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Margin = new Padding(5, 0, 5, 5) };
                
                // 选项
                Label labelOptions = new Label { Text = LanguageManager.GetString("text_index_options_json", "索引选项 (JSON):"), AutoSize = true, Font = new Font("Microsoft YaHei", 9, FontStyle.Bold), Margin = new Padding(5, 5, 5, 0) };
                
                // 创建选项区域的Panel，包含文本框和按钮
                Panel optionsPanel = new Panel { Dock = DockStyle.Fill };
                TextBox textBoxOptions = new TextBox { Multiline = true, Font = new Font("Consolas", 10), Text = "{\n  \"unique\": false,\n  \"sparse\": false,\n  \"background\": true\n}", Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Margin = new Padding(5, 0, 5, 5) };
                
                // 按钮面板 - 放在选项区域内部
                Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(245, 245, 245) };
                Button btnCreate = new Button { Text = LanguageManager.GetString("text_create_index_button", "创建索引"), Size = new Size(100, 32), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
                btnCreate.Location = new Point(10, 10);
                Button btnCancel = new Button { Text = LanguageManager.GetString("btn_cancel", "取消"), Size = new Size(80, 32), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10) };
                btnCancel.Location = new Point(120, 10);
                buttonPanel.Controls.Add(btnCreate);
                buttonPanel.Controls.Add(btnCancel);
                
                // 将文本框和按钮面板添加到选项Panel
                optionsPanel.Controls.Add(textBoxOptions);
                optionsPanel.Controls.Add(buttonPanel);
                
                // 添加所有控件到布局
                layout.Controls.Add(labelName, 0, 0);
                layout.Controls.Add(textBoxName, 0, 1);
                layout.Controls.Add(labelFields, 0, 2);
                layout.Controls.Add(textBoxFields, 0, 3);
                layout.Controls.Add(labelOptions, 0, 4);
                layout.Controls.Add(optionsPanel, 0, 5);
                
                createForm.Controls.Add(layout);
                
                // 绑定按钮事件
                btnCreate.Click += (s, e) => {
                    string indexName = textBoxName.Text.Trim();
                    string fieldsJson = textBoxFields.Text.Trim();
                    string optionsJson = textBoxOptions.Text.Trim();
                    
                    if (string.IsNullOrEmpty(indexName) || string.IsNullOrEmpty(fieldsJson))
                    {
                        MessageBox.Show("索引名称和字段定义不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    try
                    {
                        var database = connectedDatabases[databaseName];
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        
                        // 解析字段定义
                        var keys = BsonDocument.Parse(fieldsJson);
                        
                        // 创建索引选项
                        var indexOptions = new CreateIndexOptions();
                        if (!string.IsNullOrEmpty(optionsJson))
                        {
                            var options = BsonDocument.Parse(optionsJson);
                            if (options.Contains("unique"))
                                indexOptions.Unique = options["unique"].AsBoolean;
                            if (options.Contains("sparse"))
                                indexOptions.Sparse = options["sparse"].AsBoolean;
                            if (options.Contains("background"))
                                indexOptions.Background = options["background"].AsBoolean;
                        }
                        
                        // 创建索引
                        var indexModel = new CreateIndexModel<BsonDocument>(keys, indexOptions);
                        collection.Indexes.CreateOne(indexModel);
                        
                        SetStatus(string.Format("索引 '{0}' 创建成功", indexName), Color.Green);
                        MessageBox.Show("索引创建成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // 刷新索引列表
                        ShowCollectionIndexes(databaseName, collectionName);
                        createForm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("创建索引失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                
                btnCancel.Click += (s, e) => createForm.Close();
                
                createForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("创建索引失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("创建索引失败: {0}", ex.Message), Color.Red);
            }
        }

        // 新建集合
        private void CreateNewCollection(string databaseName)
        {
            try
            {
                string collectionName = Microsoft.VisualBasic.Interaction.InputBox(
                    "请输入新集合名称:",
                    "新建集合",
                    "new_collection");
                
                if (!string.IsNullOrEmpty(collectionName))
                {
                    // 验证集合名称格式
                    if (!IsValidCollectionName(collectionName))
                    {
                        MessageBox.Show("集合名称格式无效！集合名称只能包含字母、数字、下划线和连字符。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    var database = connectedDatabases[databaseName];
                    
                    // 检查集合是否已存在
                    var collections = database.ListCollectionNames().ToList();
                    if (collections.Contains(collectionName))
                    {
                        MessageBox.Show(string.Format("集合 '{0}' 已存在！", collectionName), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // 创建新集合（通过插入一个文档来创建）
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var dummyDoc = new BsonDocument { { "_id", ObjectId.GenerateNewId() }, { "created_at", DateTime.Now } };
                    collection.InsertOne(dummyDoc);
                    
                    // 删除创建的文档
                    collection.DeleteOne(dummyDoc);
                    
                    SetStatus(string.Format("集合 '{0}' 创建成功", collectionName), Color.Green);
                    MessageBox.Show("集合创建成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 刷新集合列表
                    LoadCollections(databaseName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("创建集合失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("创建集合失败: {0}", ex.Message), Color.Red);
            }
        }

        // 关闭数据库连接
        private void CloseDatabaseConnection(string databaseName)
        {
            try
            {
                if (MessageBox.Show(string.Format("确定要关闭数据库 '{0}' 的连接吗？\n关闭后需要重新连接才能访问该数据库。", databaseName), 
                    "确认关闭", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // 关闭所有相关的Tab
                    var tabsToClose = new List<TabPage>();
                    foreach (TabPage tab in tabControlData.TabPages)
                    {
                        if (tab.Tag != null && tab.Tag.ToString().StartsWith("collection:"))
                        {
                            string[] parts = tab.Tag.ToString().Split(':')[1].Split('.');
                            if (parts[0] == databaseName)
                            {
                                tabsToClose.Add(tab);
                            }
                        }
                    }
                    
                    foreach (var tab in tabsToClose)
                    {
                        tabControlData.TabPages.Remove(tab);
                    }
                    
                    // 从连接字典中移除数据库
                    connectedDatabases.Remove(databaseName);
                    allCollections.Remove(databaseName);
                    
                    // 从树形控件中移除数据库节点
                    foreach (TreeNode dbNode in treeViewCollections.Nodes)
                    {
                        if (dbNode.Text == databaseName)
                        {
                            treeViewCollections.Nodes.Remove(dbNode);
                            break;
                        }
                    }
                    
                    SetStatus(string.Format("数据库 '{0}' 连接已关闭", databaseName), Color.Blue);
                    MessageBox.Show("数据库连接已关闭！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("关闭数据库连接失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("关闭数据库连接失败: {0}", ex.Message), Color.Red);
            }
        }

        // 验证集合名称格式
        private bool IsValidCollectionName(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                return false;
                
            // MongoDB集合名称规则：
            // 1. 不能为空字符串
            // 2. 不能包含空字符
            // 3. 不能以"system."开头（系统集合）
            // 4. 不能包含保留字符：$、/、\、\0
            
            if (collectionName.StartsWith("system."))
                return false;
                
            foreach (char c in collectionName)
            {
                if (c == '$' || c == '/' || c == '\\' || c == '\0')
                    return false;
            }
            
            return true;
        }

        // 重写树形控件的节点点击事件，为索引节点添加右键菜单
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // 为树形控件添加节点展开事件
            treeViewCollections.AfterExpand += TreeViewCollections_AfterExpand;
        }

        private void TreeViewCollections_AfterExpand(object sender, TreeViewEventArgs e)
        {
            // 为展开的节点中的索引节点创建右键菜单
            foreach (TreeNode node in e.Node.Nodes)
            {
                if (node.Tag != null && node.Tag.ToString().StartsWith("index:"))
                {
                    CreateIndexContextMenu(node);
                }
            }
        }

        // 导入目录数据
        private void ImportDirectoryData(string databaseName)
        {
            try
            {
                // 显示文件夹选择对话框
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "选择包含数据文件的根目录";
                    folderDialog.ShowNewFolderButton = false;
                    
                    // 设置初始目录为上次选择的目录
                    if (!string.IsNullOrEmpty(lastImportDirectory) && Directory.Exists(lastImportDirectory))
                    {
                        folderDialog.SelectedPath = lastImportDirectory;
                        SetStatus(string.Format("已定位到上次选择的目录: {0}", lastImportDirectory), Color.Blue);
                    }
                    else if (!string.IsNullOrEmpty(lastImportDirectory))
                    {
                        // 如果上次的目录不存在了，尝试使用其父目录
                        string parentDir = Path.GetDirectoryName(lastImportDirectory);
                        if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
                        {
                            folderDialog.SelectedPath = parentDir;
                            SetStatus(string.Format("上次目录不存在，已定位到父目录: {0}", parentDir), Color.Orange);
                        }
                    }
                    
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string rootPath = folderDialog.SelectedPath;
                        
                        // 保存本次选择的目录，供下次使用
                        lastImportDirectory = rootPath;
                        SaveLastImportDirectory();
                        
                        string databasePath = Path.Combine(rootPath, databaseName);
                        
                        if (!Directory.Exists(databasePath))
                        {
                            MessageBox.Show(string.Format("在选择的目录中未找到数据库目录 '{0}'", databaseName), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        // 显示导入确认对话框
                        if (ShowImportConfirmDialog(databaseName, databasePath))
                        {
                            // 执行导入操作
                            ImportDataFromDirectory(databaseName, databasePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("导入目录数据失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("导入目录数据失败: {0}", ex.Message), Color.Red);
            }
        }

        // 显示导入确认对话框
        private bool ShowImportConfirmDialog(string databaseName, string databasePath)
        {
            try
            {
                // 扫描目录中的文件，只处理.bson文件，排除.metadata.json文件
                var allJsonFiles = Directory.GetFiles(databasePath, "*.json", SearchOption.TopDirectoryOnly);
                var jsonFiles = allJsonFiles.Where(f => !Path.GetFileName(f).EndsWith(".metadata.json", StringComparison.OrdinalIgnoreCase)).ToArray();
                var bsonFiles = Directory.GetFiles(databasePath, "*.bson", SearchOption.TopDirectoryOnly);
                var metadataFiles = allJsonFiles.Where(f => Path.GetFileName(f).EndsWith(".metadata.json", StringComparison.OrdinalIgnoreCase)).ToArray();
                
                if (jsonFiles.Length == 0 && bsonFiles.Length == 0)
                {
                    MessageBox.Show("在数据库目录中未找到任何可导入的文件\n(.bson文件或非.metadata.json文件)", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                
                // 创建确认对话框
                Form confirmForm = new Form();
                confirmForm.Text = LanguageManager.GetString("text_import_data_confirmation", "导入数据确认");
                confirmForm.Size = new Size(500, 400);
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                confirmForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                confirmForm.MaximizeBox = false;
                confirmForm.MinimizeBox = false;
                
                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 1;
                layout.RowCount = 4;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
                
                // 标题标签
                Label titleLabel = new Label();
                titleLabel.Text = string.Format("将要导入数据到数据库: {0}", databaseName);
                titleLabel.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
                titleLabel.Dock = DockStyle.Fill;
                titleLabel.TextAlign = ContentAlignment.MiddleCenter;
                
                // 文件列表
                ListBox fileListBox = new ListBox();
                fileListBox.Dock = DockStyle.Fill;
                fileListBox.Font = new Font("Microsoft YaHei", 9);
                
                // 显示数据文件
                foreach (var file in jsonFiles.Concat(bsonFiles))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string fileExt = Path.GetExtension(file);
                    fileListBox.Items.Add(string.Format("📄 {0} ({1} 数据文件)", fileName, fileExt.ToUpper()));
                }
                
                // 显示metadata文件
                foreach (var file in metadataFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file).Replace(".metadata", "");
                    fileListBox.Items.Add(string.Format("🔧 {0} (索引配置文件)", fileName));
                }
                
                // 警告标签
                Label warningLabel = new Label();
                warningLabel.Text = "⚠️ 如果集合已存在，将先删除后再导入！\n📋 将同时导入数据和索引配置信息";
                warningLabel.Font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
                warningLabel.ForeColor = Color.Red;
                warningLabel.Dock = DockStyle.Fill;
                warningLabel.TextAlign = ContentAlignment.MiddleCenter;
                
                // 按钮面板
                Panel buttonPanel = new Panel();
                buttonPanel.Dock = DockStyle.Fill;
                
                Button btnConfirm = new Button();
                btnConfirm.Text = LanguageManager.GetString("text_confirm_import", "确认导入");
                btnConfirm.Size = new Size(100, 32);
                btnConfirm.BackColor = Color.FromArgb(40, 167, 69);
                btnConfirm.ForeColor = Color.White;
                btnConfirm.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
                btnConfirm.Location = new Point(150, 10);
                btnConfirm.DialogResult = DialogResult.OK;
                
                Button btnCancel = new Button();
                btnCancel.Text = LanguageManager.GetString("btn_cancel", "取消");
                btnCancel.Size = new Size(80, 32);
                btnCancel.BackColor = Color.FromArgb(108, 117, 125);
                btnCancel.ForeColor = Color.White;
                btnCancel.Font = new Font("Microsoft YaHei", 10);
                btnCancel.Location = new Point(260, 10);
                btnCancel.DialogResult = DialogResult.Cancel;
                
                buttonPanel.Controls.Add(btnConfirm);
                buttonPanel.Controls.Add(btnCancel);
                
                layout.Controls.Add(titleLabel, 0, 0);
                layout.Controls.Add(fileListBox, 0, 1);
                layout.Controls.Add(warningLabel, 0, 2);
                layout.Controls.Add(buttonPanel, 0, 3);
                
                confirmForm.Controls.Add(layout);
                
                return confirmForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("显示导入确认对话框失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // 从目录导入数据
        private void ImportDataFromDirectory(string databaseName, string databasePath)
        {
            try
            {
                SetStatus(string.Format("开始导入数据到数据库 {0}...", databaseName));
                
                var database = connectedDatabases[databaseName];
                // 过滤掉.metadata.json文件，只处理.bson文件和普通.json文件
                var allJsonFiles = Directory.GetFiles(databasePath, "*.json", SearchOption.TopDirectoryOnly);
                var jsonFiles = allJsonFiles.Where(f => !Path.GetFileName(f).EndsWith(".metadata.json", StringComparison.OrdinalIgnoreCase)).ToArray();
                var bsonFiles = Directory.GetFiles(databasePath, "*.bson", SearchOption.TopDirectoryOnly);
                
                int totalFiles = jsonFiles.Length + bsonFiles.Length;
                int processedFiles = 0;
                int successCount = 0;
                int errorCount = 0;
                
                // 处理JSON文件
                foreach (var jsonFile in jsonFiles)
                {
                    try
                    {
                        string collectionName = Path.GetFileNameWithoutExtension(jsonFile);
                        SetStatus(string.Format("正在导入 {0}.json ({1}/{2})", collectionName, processedFiles + 1, totalFiles));
                        
                        // 检查集合是否存在，如果存在则删除
                        var existingCollections = database.ListCollectionNames().ToList();
                        if (existingCollections.Contains(collectionName))
                        {
                            database.DropCollection(collectionName);
                            SetStatus(string.Format("已删除现有集合: {0}", collectionName), Color.Orange);
                        }
                        
                        // 读取JSON文件并导入
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        var lines = File.ReadAllLines(jsonFile, Encoding.UTF8);
                        var docs = new List<BsonDocument>();
                        
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                docs.Add(BsonDocument.Parse(line));
                            }
                        }
                        
                        if (docs.Count > 0)
                        {
                            collection.InsertMany(docs);
                            SetStatus(string.Format("成功导入 {0} 条记录到集合 {1}", docs.Count, collectionName), Color.Green);
                            
                            // 检查并导入对应的metadata.json中的索引
                            ImportCollectionIndexes(databasePath, collectionName, collection);
                            
                            successCount++;
                        }
                        else
                        {
                            SetStatus(string.Format("文件 {0} 为空，跳过导入", jsonFile), Color.Orange);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetStatus(string.Format("导入文件 {0} 失败: {1}", jsonFile, ex.Message), Color.Red);
                        errorCount++;
                    }
                    
                    processedFiles++;
                }
                
                // 处理BSON文件
                foreach (var bsonFile in bsonFiles)
                {
                    try
                    {
                        string collectionName = Path.GetFileNameWithoutExtension(bsonFile);
                        SetStatus(string.Format("正在导入 {0}.bson ({1}/{2})", collectionName, processedFiles + 1, totalFiles));
                        
                        // 检查集合是否存在，如果存在则删除
                        var existingCollections = database.ListCollectionNames().ToList();
                        if (existingCollections.Contains(collectionName))
                        {
                            database.DropCollection(collectionName);
                            SetStatus(string.Format("已删除现有集合: {0}", collectionName), Color.Orange);
                        }
                        
                        // 读取BSON文件并导入
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        var docs = new List<BsonDocument>();
                        
                        using (FileStream fs = new FileStream(bsonFile, FileMode.Open, FileAccess.Read))
                        {
                            while (fs.Position < fs.Length)
                            {
                                var doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(fs);
                                docs.Add(doc);
                            }
                        }
                        
                        if (docs.Count > 0)
                        {
                            collection.InsertMany(docs);
                            SetStatus(string.Format("成功导入 {0} 条记录到集合 {1}", docs.Count, collectionName), Color.Green);
                            
                            // 检查并导入对应的metadata.json中的索引
                            ImportCollectionIndexes(databasePath, collectionName, collection);
                            
                            successCount++;
                        }
                        else
                        {
                            SetStatus(string.Format("文件 {0} 为空，跳过导入", bsonFile), Color.Orange);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetStatus(string.Format("导入文件 {0} 失败: {1}", bsonFile, ex.Message), Color.Red);
                        errorCount++;
                    }
                    
                    processedFiles++;
                }
                
                // 刷新集合列表
                LoadCollections(databaseName);
                
                // 显示导入结果
                string resultMessage = string.Format("导入完成！\n成功: {0} 个文件\n失败: {1} 个文件\n总计: {2} 个文件", 
                    successCount, errorCount, totalFiles);
                
                MessageBox.Show(resultMessage, "导入结果", MessageBoxButtons.OK, 
                    errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                
                SetStatus(string.Format("目录数据导入完成: 成功 {0}, 失败 {1}", successCount, errorCount), 
                    errorCount > 0 ? Color.Orange : Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("导入数据失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("导入数据失败: {0}", ex.Message), Color.Red);
            }
        }

        // 保存上次选择的导入目录
        private void SaveLastImportDirectory()
        {
            try
            {
                string configPath = GetConfigFilePath();
                string configDir = Path.GetDirectoryName(configPath);
                
                // 确保配置目录存在
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // 保存到配置文件
                File.WriteAllText(configPath, lastImportDirectory, Encoding.UTF8);
                SetStatus(string.Format("已保存导入目录配置: {0}", lastImportDirectory), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("保存导入目录配置失败: {0}", ex.Message), Color.Orange);
            }
        }

        // 加载上次选择的导入目录
        private void LoadLastImportDirectory()
        {
            try
            {
                string configPath = GetConfigFilePath();
                
                if (File.Exists(configPath))
                {
                    lastImportDirectory = File.ReadAllText(configPath, Encoding.UTF8).Trim();
                    if (!string.IsNullOrEmpty(lastImportDirectory))
                    {
                        SetStatus(string.Format("已加载上次的导入目录配置: {0}", lastImportDirectory), Color.Blue);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("加载导入目录配置失败: {0}", ex.Message), Color.Orange);
                lastImportDirectory = string.Empty;
            }
        }

        // 获取配置文件路径
        private string GetConfigFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appConfigDir = Path.Combine(appDataPath, "MongoCompass");
            return Path.Combine(appConfigDir, "last_import_directory.txt");
        }

        // 清除保存的导入目录配置
        private void ClearLastImportDirectory()
        {
            try
            {
                string configPath = GetConfigFilePath();
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                    lastImportDirectory = string.Empty;
                    SetStatus("已清除导入目录配置", Color.Blue);
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("清除导入目录配置失败: {0}", ex.Message), Color.Orange);
            }
        }

        // 保存上次连接参数
        private void SaveLastConnectionParams()
        {
            try
            {
                string configPath = GetConnectionConfigFilePath();
                string configDir = Path.GetDirectoryName(configPath);
                
                // 确保配置目录存在
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // 创建连接配置内容
                var connectionConfig = new StringBuilder();
                connectionConfig.AppendLine("[LastConnection]");
                connectionConfig.AppendLine(string.Format("ConnectionString={0}", lastConnectionString));
                connectionConfig.AppendLine(string.Format("DatabaseName={0}", lastDatabaseName));
                connectionConfig.AppendLine(string.Format("ConnectionTime={0}", lastConnectionTime.ToString("yyyy-MM-dd HH:mm:ss")));
                
                // 保存到配置文件
                File.WriteAllText(configPath, connectionConfig.ToString(), Encoding.UTF8);
                SetStatus(string.Format("已保存连接参数配置: {0}", GetConnectionDisplayName()), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("保存连接参数配置失败: {0}", ex.Message), Color.Orange);
            }
        }

        // 加载上次连接参数
        private void LoadLastConnectionParams()
        {
            try
            {
                string configPath = GetConnectionConfigFilePath();
                
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath, Encoding.UTF8);
                    bool inLastConnectionSection = false;
                    
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine == "[LastConnection]")
                        {
                            inLastConnectionSection = true;
                            continue;
                        }
                        
                        if (inLastConnectionSection && trimmedLine.Contains("="))
                        {
                            var parts = trimmedLine.Split(new[] { '=' }, 2);
                            if (parts.Length == 2)
                            {
                                switch (parts[0])
                                {
                                    case "ConnectionString":
                                        lastConnectionString = parts[1];
                                        break;
                                    case "DatabaseName":
                                        lastDatabaseName = parts[1];
                                        break;
                                    case "ConnectionTime":
                                        if (DateTime.TryParse(parts[1], out DateTime connectionTime))
                                        {
                                            lastConnectionTime = connectionTime;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(lastConnectionString) && !string.IsNullOrEmpty(lastDatabaseName))
                    {
                        SetStatus(string.Format("已加载上次连接配置: {0} (时间: {1})", 
                            GetConnectionDisplayName(), lastConnectionTime.ToString("yyyy-MM-dd HH:mm:ss")), Color.Blue);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("加载连接参数配置失败: {0}", ex.Message), Color.Orange);
                lastConnectionString = string.Empty;
                lastDatabaseName = string.Empty;
                lastConnectionTime = DateTime.MinValue;
            }
        }

        // 获取连接配置文件路径
        private string GetConnectionConfigFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appConfigDir = Path.Combine(appDataPath, "MongoCompass");
            return Path.Combine(appConfigDir, "last_connection.ini");
        }

        // 获取连接显示名称（隐藏敏感信息）
        private string GetConnectionDisplayName()
        {
            if (string.IsNullOrEmpty(lastConnectionString) || string.IsNullOrEmpty(lastDatabaseName))
                return "无";
                
            try
            {
                // 从连接字符串中提取主机和端口信息，隐藏用户名密码
                var uri = new Uri(lastConnectionString.Split('?')[0]); // 去掉查询参数
                string host = uri.Host;
                int port = uri.Port;
                return string.Format("{0}:{1}/{2}", host, port, lastDatabaseName);
            }
            catch
            {
                return string.Format("数据库: {0}", lastDatabaseName);
            }
        }

        // 显示自动重连提示
        private void ShowAutoReconnectPrompt()
        {
            try
            {
                // 只有在有上次连接参数且连接时间在24小时内时才提示
                if (!string.IsNullOrEmpty(lastConnectionString) && 
                    !string.IsNullOrEmpty(lastDatabaseName) && 
                    lastConnectionTime != DateTime.MinValue &&
                    (DateTime.Now - lastConnectionTime).TotalHours <= 24)
                {
                    // 延迟显示提示，避免在窗口初始化时显示
                    this.Load += (sender, e) =>
                    {
                        var result = MessageBox.Show(
                            string.Format("检测到上次连接记录:\n{0}\n连接时间: {1}\n\n是否要重新连接到上次的数据库？", 
                                GetConnectionDisplayName(), 
                                lastConnectionTime.ToString("yyyy-MM-dd HH:mm:ss")),
                            "自动重连",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            ReconnectToLastDatabase();
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("显示自动重连提示失败: {0}", ex.Message), Color.Orange);
            }
        }

        // 重连到上次的数据库
        private void ReconnectToLastDatabase()
        {
            try
            {
                if (!string.IsNullOrEmpty(lastConnectionString) && !string.IsNullOrEmpty(lastDatabaseName))
                {
                    SetStatus("正在使用上次连接参数重新连接...", Color.Blue);
                    ConnectToMongoDB(lastConnectionString, lastDatabaseName);
                }
                else
                {
                    SetStatus("上次连接参数无效", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("自动重连失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("自动重连失败: {0}", ex.Message), Color.Red);
            }
        }

        // 清除保存的连接参数配置
        private void ClearLastConnectionParams()
        {
            try
            {
                string configPath = GetConnectionConfigFilePath();
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                    lastConnectionString = string.Empty;
                    lastDatabaseName = string.Empty;
                    lastConnectionTime = DateTime.MinValue;
                    SetStatus("已清除连接参数配置", Color.Blue);
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("清除连接参数配置失败: {0}", ex.Message), Color.Orange);
            }
        }

        // 手动重连到上次数据库的公共方法
        public void ManualReconnectToLastDatabase()
        {
            try
            {
                if (!string.IsNullOrEmpty(lastConnectionString) && !string.IsNullOrEmpty(lastDatabaseName))
                {
                    var result = MessageBox.Show(
                        string.Format("确认要重新连接到:\n{0}\n连接时间: {1}",
                            GetConnectionDisplayName(),
                            lastConnectionTime.ToString("yyyy-MM-dd HH:mm:ss")),
                        "确认重连",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                        
                    if (result == DialogResult.Yes)
                    {
                        // 先断开当前连接
                        DisconnectFromMongoDB();
                        // 重新连接
                        ReconnectToLastDatabase();
                    }
                }
                else
                {
                    MessageBox.Show("没有找到上次连接的参数记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("手动重连失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 断开MongoDB连接
        private void DisconnectFromMongoDB()
        {
            try
            {
                if (mongoClient != null)
                {
                    connectedDatabases.Clear();
                    allCollections.Clear();
                    treeViewCollections.Nodes.Clear();
                    
                    // 关闭所有数据标签页
                    tabControlData.TabPages.Clear();
                    
                    mongoClient = null;
                    SetStatus("已断开数据库连接", Color.Blue);
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("断开连接时出错: {0}", ex.Message), Color.Orange);
            }
        }
        
        // 备份数据库
        private void BackupDatabase(string databaseName)
        {
            try
            {
                // 创建备份目录
                string backupDir = Path.Combine(Application.StartupPath, "backup", databaseName);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
                
                SetStatus(string.Format("正在备份数据库: {0}", databaseName), Color.Blue);
                
                var database = connectedDatabases[databaseName];
                var collections = database.ListCollectionNames().ToList();
                
                if (collections.Count == 0)
                {
                    MessageBox.Show("数据库中没有集合，无需备份。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                int totalCollections = collections.Count;
                int processedCollections = 0;
                int successCount = 0;
                int failedCount = 0;
                
                foreach (string collectionName in collections)
                {
                    try
                    {
                        processedCollections++;
                        SetStatus(string.Format("正在备份集合 {0} ({1}/{2})", collectionName, processedCollections, totalCollections), Color.Blue);
                        
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        
                        // 导出数据为.bson文件
                        string bsonFilePath = Path.Combine(backupDir, collectionName + ".bson");
                        ExportCollectionToBson(collection, bsonFilePath);
                        
                        // 导出索引信息为.metadata.json文件
                        string metadataFilePath = Path.Combine(backupDir, collectionName + ".metadata.json");
                        ExportCollectionMetadata(collection, metadataFilePath);
                        
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        SetStatus(string.Format("备份集合 {0} 失败: {1}", collectionName, ex.Message), Color.Red);
                    }
                }
                
                string resultMessage = string.Format("数据库备份完成！\n成功: {0} 个集合\n失败: {1} 个集合\n备份位置: {2}", 
                    successCount, failedCount, backupDir);
                
                MessageBox.Show(resultMessage, "备份完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus(string.Format("数据库 {0} 备份完成，共备份 {1} 个集合", databaseName, successCount), Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("备份数据库失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("备份数据库失败: {0}", ex.Message), Color.Red);
            }
        }
        
        // 导出集合数据为.bson文件
        private void ExportCollectionToBson(IMongoCollection<BsonDocument> collection, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var documents = collection.Find(new BsonDocument()).ToList();
                foreach (var doc in documents)
                {
                    var bytes = doc.ToBson();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
        
        // 导出集合元数据为.metadata.json文件
        private void ExportCollectionMetadata(IMongoCollection<BsonDocument> collection, string filePath)
        {
            try
            {
                // 获取集合的索引信息
                var indexes = collection.Indexes.List().ToList();
                
                // 创建元数据文档
                var metadata = new BsonDocument();
                metadata.Add("indexes", new BsonArray(indexes));
                
                // 获取集合统计信息
                var stats = collection.Database.RunCommand<BsonDocument>(new BsonDocument("collStats", collection.CollectionNamespace.CollectionName));
                if (stats.Contains("count"))
                {
                    metadata.Add("count", stats["count"]);
                }
                if (stats.Contains("size"))
                {
                    metadata.Add("size", stats["size"]);
                }
                if (stats.Contains("avgObjSize"))
                {
                    metadata.Add("avgObjSize", stats["avgObjSize"]);
                }
                
                // 写入文件
                string jsonContent = metadata.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true });
                File.WriteAllText(filePath, jsonContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // 如果获取统计信息失败，只导出索引信息
                var indexes = collection.Indexes.List().ToList();
                var metadata = new BsonDocument();
                metadata.Add("indexes", new BsonArray(indexes));
                
                string jsonContent = metadata.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true });
                File.WriteAllText(filePath, jsonContent, Encoding.UTF8);
            }
        }

        // 导入集合的索引信息
        private void ImportCollectionIndexes(string databasePath, string collectionName, IMongoCollection<BsonDocument> collection)
        {
            try
            {
                string metadataFilePath = Path.Combine(databasePath, collectionName + ".metadata.json");
                
                if (!File.Exists(metadataFilePath))
                {
                    SetStatus(string.Format("未找到集合 {0} 的metadata.json文件，跳过索引导入", collectionName), Color.Orange);
                    return;
                }
                
                SetStatus(string.Format("正在分析 {0}.metadata.json 中的索引信息...", collectionName), Color.Blue);
                
                // 读取metadata.json文件
                string metadataContent = File.ReadAllText(metadataFilePath, Encoding.UTF8);
                var metadataDoc = BsonDocument.Parse(metadataContent);
                
                // 检查是否有indexes字段
                if (metadataDoc.Contains("indexes") && metadataDoc["indexes"].IsBsonArray)
                {
                    var indexesArray = metadataDoc["indexes"].AsBsonArray;
                    int indexCount = 0;
                    int createdCount = 0;
                    int skippedCount = 0;
                    
                    foreach (BsonDocument indexDoc in indexesArray)
                    {
                        indexCount++;
                        
                        try
                        {
                            // 跳过默认的_id索引
                            if (indexDoc.Contains("name") && indexDoc["name"].AsString == "_id_")
                            {
                                SetStatus(string.Format("跳过默认索引: _id_"), Color.Blue);
                                skippedCount++;
                                continue;
                            }
                            
                            // 提取索引定义
                            if (indexDoc.Contains("key") && indexDoc["key"].IsBsonDocument)
                            {
                                var keyDoc = indexDoc["key"].AsBsonDocument;
                                IndexKeysDefinition<BsonDocument> indexKeys = null;
                                
                                try
                                {
                                    // 使用BsonDocument直接创建索引
                                    indexKeys = new BsonDocumentIndexKeysDefinition<BsonDocument>(keyDoc);
                                    SetStatus(string.Format("使用BsonDocument创建索引定义: {0}", keyDoc.ToJson()), Color.Blue);
                                }
                                catch (Exception ex)
                                {
                                    SetStatus(string.Format("创建索引定义失败: {0}", ex.Message), Color.Red);
                                    continue;
                                }
                                
                                if (indexKeys != null)
                                {
                                    // 创建索引选项
                                    var createIndexOptions = new CreateIndexOptions();
                                    string indexName = "未命名索引";
                                    
                                    try
                                    {
                                        // 设置索引名称
                                        if (indexDoc.Contains("name") && indexDoc["name"].IsString)
                                        {
                                            createIndexOptions.Name = indexDoc["name"].AsString;
                                            indexName = createIndexOptions.Name;
                                        }
                                        
                                        // 设置唯一性
                                        if (indexDoc.Contains("unique") && indexDoc["unique"].IsBoolean)
                                        {
                                            createIndexOptions.Unique = indexDoc["unique"].AsBoolean;
                                            SetStatus(string.Format("设置唯一索引: {0}", createIndexOptions.Unique), Color.Blue);
                                        }
                                        
                                        // 设置稀疏索引
                                        if (indexDoc.Contains("sparse") && indexDoc["sparse"].IsBoolean)
                                        {
                                            createIndexOptions.Sparse = indexDoc["sparse"].AsBoolean;
                                            SetStatus(string.Format("设置稀疏索引: {0}", createIndexOptions.Sparse), Color.Blue);
                                        }
                                        
                                        // 尝试设置后台创建（如果支持）
                                        if (indexDoc.Contains("background") && indexDoc["background"].IsBoolean)
                                        {
                                            try
                                            {
                                                var backgroundProperty = createIndexOptions.GetType().GetProperty("Background");
                                                if (backgroundProperty != null && backgroundProperty.CanWrite)
                                                {
                                                    backgroundProperty.SetValue(createIndexOptions, indexDoc["background"].AsBoolean);
                                                    SetStatus(string.Format("设置后台创建: {0}", indexDoc["background"].AsBoolean), Color.Blue);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                SetStatus(string.Format("设置后台创建失败: {0}", ex.Message), Color.Orange);
                                            }
                                        }
                                        
                                        // 尝试设置TTL（如果支持）
                                        if (indexDoc.Contains("expireAfterSeconds") && indexDoc["expireAfterSeconds"].IsNumeric)
                                        {
                                            try
                                            {
                                                var expireAfterProperty = createIndexOptions.GetType().GetProperty("ExpireAfter");
                                                if (expireAfterProperty != null && expireAfterProperty.CanWrite)
                                                {
                                                    expireAfterProperty.SetValue(createIndexOptions, TimeSpan.FromSeconds(indexDoc["expireAfterSeconds"].AsDouble));
                                                    SetStatus(string.Format("设置TTL过期时间: {0}秒", indexDoc["expireAfterSeconds"].AsDouble), Color.Blue);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                SetStatus(string.Format("设置TTL过期时间失败: {0}", ex.Message), Color.Orange);
                                            }
                                        }
                                        
                                        // 尝试设置部分索引过滤器（如果支持）
                                        if (indexDoc.Contains("partialFilterExpression") && indexDoc["partialFilterExpression"].IsBsonDocument)
                                        {
                                            try
                                            {
                                                var partialFilterProperty = createIndexOptions.GetType().GetProperty("PartialFilterExpression");
                                                if (partialFilterProperty != null && partialFilterProperty.CanWrite)
                                                {
                                                    partialFilterProperty.SetValue(createIndexOptions, indexDoc["partialFilterExpression"].AsBsonDocument);
                                                    SetStatus(string.Format("设置部分索引过滤器: {0}", indexDoc["partialFilterExpression"].ToJson()), Color.Blue);
                                                }
                                                else
                                                {
                                                    SetStatus("当前MongoDB.Driver版本不支持PartialFilterExpression，跳过该选项", Color.Orange);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                SetStatus(string.Format("设置部分索引过滤器失败: {0}", ex.Message), Color.Orange);
                                            }
                                        }
                                        
                                        // 创建索引模型
                                        var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, createIndexOptions);
                                        
                                        // 创建索引
                                        collection.Indexes.CreateOne(indexModel);
                                        
                                        SetStatus(string.Format("成功创建索引: {0} (集合: {1})", indexName, collectionName), Color.Green);
                                        createdCount++;
                                    }
                                    catch (Exception optionEx)
                                    {
                                        SetStatus(string.Format("设置索引选项失败，尝试使用基本选项创建索引 {0}: {1}", indexName, optionEx.Message), Color.Orange);
                                        
                                        try
                                        {
                                            // 回退到最基本的索引创建
                                            var basicOptions = new CreateIndexOptions();
                                            if (indexDoc.Contains("name") && indexDoc["name"].IsString)
                                            {
                                                basicOptions.Name = indexDoc["name"].AsString;
                                            }
                                            var basicModel = new CreateIndexModel<BsonDocument>(indexKeys, basicOptions);
                                            collection.Indexes.CreateOne(basicModel);
                                            
                                            SetStatus(string.Format("使用基本选项成功创建索引: {0} (集合: {1})", basicOptions.Name ?? "未命名索引", collectionName), Color.Green);
                                            createdCount++;
                                        }
                                        catch (Exception basicEx)
                                        {
                                            SetStatus(string.Format("创建基本索引也失败: {0}", basicEx.Message), Color.Red);
                                            skippedCount++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception indexEx)
                        {
                            string indexName = "未知索引";
                            if (indexDoc.Contains("name") && indexDoc["name"].IsString)
                            {
                                indexName = indexDoc["name"].AsString;
                            }
                            SetStatus(string.Format("创建索引 {0} 失败: {1}", indexName, indexEx.Message), Color.Red);
                            skippedCount++;
                        }
                    }
                    
                    SetStatus(string.Format("集合 {0} 索引导入完成: 总计 {1}, 创建 {2}, 跳过 {3}", 
                        collectionName, indexCount, createdCount, skippedCount), 
                        createdCount > 0 ? Color.Green : Color.Orange);
                }
                else
                {
                    SetStatus(string.Format("metadata.json 中未找到索引定义 (集合: {0})", collectionName), Color.Orange);
                }
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("导入集合 {0} 的索引失败: {1}", collectionName, ex.Message), Color.Red);
            }
        }

        // ==================== 集合CRUD操作方法 ====================
        
        // 显示新增记录对话框
        private void ShowAddRecordDialog(string databaseName, string collectionName)
        {
            try
            {
                Form addForm = new Form();
                addForm.Text = string.Format("新增记录 - {0}.{1}", databaseName, collectionName);
                addForm.Size = new Size(600, 500);
                addForm.StartPosition = FormStartPosition.CenterParent;
                addForm.FormBorderStyle = FormBorderStyle.Sizable;
                addForm.MinimizeBox = false;
                addForm.MaximizeBox = false;
                
                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 1;
                layout.RowCount = 3;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
                
                // 标题标签
                Label titleLabel = new Label();
                titleLabel.Text = LanguageManager.GetString("text_please_enter_json_document", "请输入JSON格式的文档数据：");
                titleLabel.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
                titleLabel.Dock = DockStyle.Fill;
                titleLabel.TextAlign = ContentAlignment.MiddleLeft;
                titleLabel.Padding = new Padding(10, 5, 10, 5);
                
                // JSON输入框
                TextBox jsonTextBox = new TextBox();
                jsonTextBox.Multiline = true;
                jsonTextBox.ScrollBars = ScrollBars.Vertical;
                jsonTextBox.Dock = DockStyle.Fill;
                jsonTextBox.Font = new Font("Consolas", 10);
                jsonTextBox.Text = "{\n  \"_id\": ObjectId(),\n  \"name\": \"示例名称\",\n  \"value\": 123,\n  \"created\": new Date()\n}";
                jsonTextBox.Margin = new Padding(10);
                
                // 按钮面板
                Panel buttonPanel = new Panel();
                buttonPanel.Dock = DockStyle.Fill;
                
                Button btnAdd = new Button();
                btnAdd.Text = LanguageManager.GetString("text_add_record_button", "添加记录");
                btnAdd.Size = new Size(100, 32);
                btnAdd.BackColor = Color.FromArgb(40, 167, 69);
                btnAdd.ForeColor = Color.White;
                btnAdd.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
                btnAdd.Location = new Point(200, 10);
                btnAdd.Click += (s, e) => {
                    try
                    {
                        string jsonText = jsonTextBox.Text.Trim();
                        if (string.IsNullOrEmpty(jsonText))
                        {
                            MessageBox.Show("请输入JSON数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        // 处理特殊的MongoDB语法
                        jsonText = ProcessMongoDBSyntax(jsonText);
                        
                        var document = BsonDocument.Parse(jsonText);
                        var database = connectedDatabases[databaseName];
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        
                        collection.InsertOne(document);
                        
                        MessageBox.Show("记录添加成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        addForm.DialogResult = DialogResult.OK;
                        addForm.Close();
                        
                        // 刷新数据显示
                        RefreshCollectionData(databaseName, collectionName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("添加记录失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                
                Button btnCancel = new Button();
                btnCancel.Text = LanguageManager.GetString("btn_cancel", "取消");
                btnCancel.Size = new Size(80, 32);
                btnCancel.BackColor = Color.FromArgb(108, 117, 125);
                btnCancel.ForeColor = Color.White;
                btnCancel.Font = new Font("Microsoft YaHei", 10);
                btnCancel.Location = new Point(320, 10);
                btnCancel.Click += (s, e) => addForm.Close();
                
                buttonPanel.Controls.Add(btnAdd);
                buttonPanel.Controls.Add(btnCancel);
                
                layout.Controls.Add(titleLabel, 0, 0);
                layout.Controls.Add(jsonTextBox, 0, 1);
                layout.Controls.Add(buttonPanel, 0, 2);
                
                addForm.Controls.Add(layout);
                addForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("显示新增对话框失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 显示批量新增对话框
        private void ShowBatchAddDialog(string databaseName, string collectionName)
        {
            try
            {
                Form batchForm = new Form();
                batchForm.Text = string.Format("批量新增记录 - {0}.{1}", databaseName, collectionName);
                batchForm.Size = new Size(700, 600);
                batchForm.StartPosition = FormStartPosition.CenterParent;
                batchForm.FormBorderStyle = FormBorderStyle.Sizable;
                batchForm.MinimizeBox = false;
                batchForm.MaximizeBox = false;
                
                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 1;
                layout.RowCount = 3;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
                
                // 标题标签
                Label titleLabel = new Label();
                titleLabel.Text = LanguageManager.GetString("text_please_enter_json_array", "请输入JSON数组格式的多条文档数据：");
                titleLabel.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
                titleLabel.Dock = DockStyle.Fill;
                titleLabel.TextAlign = ContentAlignment.MiddleLeft;
                titleLabel.Padding = new Padding(10, 5, 10, 5);
                
                // JSON输入框
                TextBox jsonTextBox = new TextBox();
                jsonTextBox.Multiline = true;
                jsonTextBox.ScrollBars = ScrollBars.Vertical;
                jsonTextBox.Dock = DockStyle.Fill;
                jsonTextBox.Font = new Font("Consolas", 10);
                jsonTextBox.Text = "[\n  {\n    \"name\": \"记录1\",\n    \"value\": 100\n  },\n  {\n    \"name\": \"记录2\",\n    \"value\": 200\n  }\n]";
                jsonTextBox.Margin = new Padding(10);
                
                // 按钮面板
                Panel buttonPanel = new Panel();
                buttonPanel.Dock = DockStyle.Fill;
                
                Button btnAdd = new Button();
                btnAdd.Text = LanguageManager.GetString("text_batch_add_button", "批量添加");
                btnAdd.Size = new Size(100, 32);
                btnAdd.BackColor = Color.FromArgb(23, 162, 184);
                btnAdd.ForeColor = Color.White;
                btnAdd.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
                btnAdd.Location = new Point(250, 10);
                btnAdd.Click += (s, e) => {
                    try
                    {
                        string jsonText = jsonTextBox.Text.Trim();
                        if (string.IsNullOrEmpty(jsonText))
                        {
                            MessageBox.Show("请输入JSON数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        var database = connectedDatabases[databaseName];
                        var collection = database.GetCollection<BsonDocument>(collectionName);
                        var documents = new List<BsonDocument>();
                        
                        // 处理特殊的MongoDB语法
                        jsonText = ProcessMongoDBSyntax(jsonText);
                        
                        // 尝试解析为数组
                        if (jsonText.Trim().StartsWith("["))
                        {
                            var array = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(jsonText);
                            foreach (BsonValue value in array)
                            {
                                if (value.IsBsonDocument)
                                {
                                    documents.Add(value.AsBsonDocument);
                                }
                            }
                        }
                        else
                        {
                            // 尝试按行解析多个JSON对象
                            var lines = jsonText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in lines)
                            {
                                var trimmedLine = line.Trim();
                                if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                                {
                                    var doc = BsonDocument.Parse(trimmedLine);
                                    documents.Add(doc);
                                }
                            }
                        }
                        
                        if (documents.Count == 0)
                        {
                            MessageBox.Show("没有找到有效的JSON文档", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        collection.InsertMany(documents);
                        
                        MessageBox.Show(string.Format("成功批量添加 {0} 条记录！", documents.Count), "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        batchForm.DialogResult = DialogResult.OK;
                        batchForm.Close();
                        
                        // 刷新数据显示
                        RefreshCollectionData(databaseName, collectionName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("批量添加记录失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                
                Button btnCancel = new Button();
                btnCancel.Text = LanguageManager.GetString("btn_cancel", "取消");
                btnCancel.Size = new Size(80, 32);
                btnCancel.BackColor = Color.FromArgb(108, 117, 125);
                btnCancel.ForeColor = Color.White;
                btnCancel.Font = new Font("Microsoft YaHei", 10);
                btnCancel.Location = new Point(370, 10);
                btnCancel.Click += (s, e) => batchForm.Close();
                
                buttonPanel.Controls.Add(btnAdd);
                buttonPanel.Controls.Add(btnCancel);
                
                layout.Controls.Add(titleLabel, 0, 0);
                layout.Controls.Add(jsonTextBox, 0, 1);
                layout.Controls.Add(buttonPanel, 0, 2);
                
                batchForm.Controls.Add(layout);
                batchForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("显示批量新增对话框失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 显示导入JSON文件对话框
        private void ShowImportJsonDialog(string databaseName, string collectionName)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择要导入的JSON文件";
                openFileDialog.Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                openFileDialog.Multiselect = false;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    
                    var result = MessageBox.Show(
                        string.Format("确认要将文件 '{0}' 导入到集合 '{1}.{2}' 吗？", 
                            Path.GetFileName(filePath), databaseName, collectionName),
                        "确认导入",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                        
                    if (result == DialogResult.Yes)
                    {
                        ImportJsonFile(databaseName, collectionName, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("显示导入对话框失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 显示导入CSV文件对话框
        private void ShowImportCsvDialog(string databaseName, string collectionName)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择要导入的CSV文件";
                openFileDialog.Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                openFileDialog.Multiselect = false;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    
                    var result = MessageBox.Show(
                        string.Format("确认要将文件 '{0}' 导入到集合 '{1}.{2}' 吗？", 
                            Path.GetFileName(filePath), databaseName, collectionName),
                        "确认导入",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                        
                    if (result == DialogResult.Yes)
                    {
                        ImportCsvFile(databaseName, collectionName, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("显示导入对话框失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 导入CSV文件
        private void ImportCsvFile(string databaseName, string collectionName, string filePath)
        {
            try
            {
                SetStatus(string.Format("正在导入CSV文件: {0}", Path.GetFileName(filePath)), Color.Blue);
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                var documents = new List<BsonDocument>();
                
                // 读取CSV文件
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("CSV文件为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // 解析标题行
                List<string> headers = ParseCsvLine(lines[0]);
                if (headers.Count == 0)
                {
                    MessageBox.Show("CSV文件格式错误：无法解析标题行", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // 解析数据行
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    
                    List<string> values = ParseCsvLine(line);
                    if (values.Count > 0)
                    {
                        var doc = new BsonDocument();
                        
                        // 确保values数组长度与headers匹配
                        for (int j = 0; j < headers.Count; j++)
                        {
                            string fieldName = headers[j];
                            string fieldValue = j < values.Count ? values[j] : "";
                            
                            // 解析字段值
                            doc[fieldName] = ParseStringToBsonValue(fieldValue);
                        }
                        
                        documents.Add(doc);
                    }
                }
                
                if (documents.Count == 0)
                {
                    MessageBox.Show("CSV文件中没有找到有效的数据行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                collection.InsertMany(documents);
                
                MessageBox.Show(string.Format("成功导入 {0} 条记录！", documents.Count), "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus(string.Format("成功导入 {0} 条记录", documents.Count), Color.Green);
                
                // 刷新当前标签页的数据
                RefreshCollectionData(databaseName, collectionName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("导入CSV文件失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("导入CSV文件失败: {0}", ex.Message), Color.Red);
            }
        }
        
        // 导入JSON文件
        private void ImportJsonFile(string databaseName, string collectionName, string filePath)
        {
            try
            {
                SetStatus(string.Format("正在导入文件: {0}", Path.GetFileName(filePath)), Color.Blue);
                
                string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                var documents = new List<BsonDocument>();
                
                // 处理特殊的MongoDB语法
                jsonContent = ProcessMongoDBSyntax(jsonContent);
                
                // 尝试解析为数组
                if (jsonContent.Trim().StartsWith("["))
                {
                    var array = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(jsonContent);
                    foreach (BsonValue value in array)
                    {
                        if (value.IsBsonDocument)
                        {
                            documents.Add(value.AsBsonDocument);
                        }
                    }
                }
                else
                {
                    // 尝试按行解析多个JSON对象（JSON Lines格式）
                    var lines = jsonContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                        {
                            try
                            {
                                var doc = BsonDocument.Parse(trimmedLine);
                                documents.Add(doc);
                            }
                            catch
                            {
                                // 跳过无效的行
                                continue;
                            }
                        }
                    }
                }
                
                if (documents.Count == 0)
                {
                    MessageBox.Show("文件中没有找到有效的JSON文档", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                collection.InsertMany(documents);
                
                MessageBox.Show(string.Format("成功导入 {0} 条记录！", documents.Count), "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus(string.Format("成功导入 {0} 条记录", documents.Count), Color.Green);
                
                // 刷新数据显示
                RefreshCollectionData(databaseName, collectionName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("导入文件失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("导入文件失败: {0}", ex.Message), Color.Red);
            }
        }
        
        // 修改选中的记录
        private void EditSelectedRecord(string databaseName, string collectionName)
        {
            try
            {
                var currentTab = tabControlData.SelectedTab;
                if (currentTab == null || currentTab.Tag == null)
                {
                    MessageBox.Show("请先选择一个集合标签页", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var tabData = currentTab.Tag as dynamic;
                if (tabData == null)
                {
                    MessageBox.Show("无法获取标签页数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                DataGridView dataGridView = tabData.DataGridView;
                
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选择要修改的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                EditRecord_Click(dataGridView, databaseName, collectionName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("修改记录失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 删除选中的记录
        private void DeleteSelectedRecord(string databaseName, string collectionName)
        {
            try
            {
                var currentTab = tabControlData.SelectedTab;
                if (currentTab == null || currentTab.Tag == null)
                {
                    MessageBox.Show("请先选择一个集合标签页", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var tabData = currentTab.Tag as dynamic;
                if (tabData == null)
                {
                    MessageBox.Show("无法获取标签页数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                DataGridView dataGridView = tabData.DataGridView;
                
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选择要删除的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                DeleteRecord_Click(dataGridView, databaseName, collectionName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("删除记录失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 刷新集合数据
        private void RefreshCollectionData(string databaseName, string collectionName)
        {
            try
            {
                var currentTab = tabControlData.SelectedTab;
                if (currentTab == null || currentTab.Tag == null)
                {
                    return;
                }
                
                var tabData = currentTab.Tag as dynamic;
                if (tabData == null)
                {
                    return;
                }
                
                DataGridView dataGridView = tabData.DataGridView;
                TextBox queryTextBox = tabData.QueryTextBox;
                
                string queryText = queryTextBox != null ? queryTextBox.Text : "{}";
                ExecuteQuery(databaseName, collectionName, queryText, dataGridView);
                
                SetStatus(string.Format("已刷新集合数据: {0}.{1}", databaseName, collectionName), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("刷新集合数据失败: {0}", ex.Message), Color.Red);
            }
        }
        
        // 处理MongoDB特殊语法
        private string ProcessMongoDBSyntax(string jsonText)
        {
            try
            {
                // 处理ObjectId()
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, 
                    @"ObjectId\(\s*\)", 
                    "\"" + ObjectId.GenerateNewId().ToString() + "\"");
                    
                // 处理ObjectId("...")
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, 
                    @"ObjectId\(\s*""([^""]+)""\s*\)", 
                    "\"$1\"");
                    
                // 处理new Date()
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, 
                    @"new\s+Date\(\s*\)", 
                    "\"" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\"");
                    
                // 处理ISODate()
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, 
                    @"ISODate\(\s*\)", 
                    "\"" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\"");
                    
                // 处理ISODate("...")
                jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, 
                    @"ISODate\(\s*""([^""]+)""\s*\)", 
                    "\"$1\"");
                
                return jsonText;
            }
            catch
            {
                return jsonText; // 如果处理失败，返回原文本
            }
        }

        // 批量删除选中的记录
        private void BatchDeleteSelectedRecords(string databaseName, string collectionName)
        {
            try
            {
                var currentTab = tabControlData.SelectedTab;
                if (currentTab == null || currentTab.Tag == null)
                {
                    MessageBox.Show("请先选择一个集合标签页", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var tabData = currentTab.Tag as dynamic;
                if (tabData == null)
                {
                    MessageBox.Show("无法获取标签页数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                DataGridView dataGridView = tabData.DataGridView;
                
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选择要删除的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (dataGridView.SelectedRows.Count == 1)
                {
                    MessageBox.Show("只选择了一条记录，请使用删除记录按钮删除单条记录，或选择多条记录进行批量删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // 确认删除
                string confirmMessage = string.Format("确定要删除选中的 {0} 条记录吗？\n\n此操作不可撤销！", dataGridView.SelectedRows.Count);
                if (MessageBox.Show(confirmMessage, "确认批量删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
                
                // 收集所有选中记录的_id
                List<BsonValue> idValues = new List<BsonValue>();
                List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
                
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    if (row.Tag is BsonDocument doc && doc.Contains("_id"))
                    {
                        idValues.Add(doc["_id"]);
                        selectedRows.Add(row);
                    }
                }
                
                if (idValues.Count == 0)
                {
                    MessageBox.Show("无法获取选中记录的 _id 字段，无法删除。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // 执行批量删除
                var database = connectedDatabases[databaseName];
                var collection = database.GetCollection<BsonDocument>(collectionName);
                
                // 构建批量删除过滤器
                var filter = Builders<BsonDocument>.Filter.In("_id", idValues);
                
                // 执行删除操作
                var result = collection.DeleteMany(filter);
                
                if (result.DeletedCount > 0)
                {
                    // 从DataGridView中移除已删除的行
                    foreach (var row in selectedRows)
                    {
                        if (dataGridView.Rows.Contains(row))
                        {
                            dataGridView.Rows.Remove(row);
                        }
                    }
                    
                    string successMessage = string.Format("成功删除 {0} 条记录", result.DeletedCount);
                    SetStatus(successMessage, Color.Green);
                    MessageBox.Show(successMessage, "批量删除完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("未能删除任何记录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("批量删除失败: {0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus(string.Format("批量删除失败: {0}", ex.Message), Color.Red);
            }
        }

        // 导出为 .csv
        private void MenuItemExportCsv_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show("请先选择要导出的集合节点。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV文件 (*.csv)|*.csv";
            sfd.FileName = collectionName + ".csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                    
                    if (documents.Count == 0)
                    {
                        MessageBox.Show("集合为空，没有数据可导出。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 获取所有字段名
                    HashSet<string> allFields = new HashSet<string>();
                    foreach (var doc in documents)
                    {
                        foreach (var element in doc.Elements)
                        {
                            allFields.Add(element.Name);
                        }
                    }
                    
                    // 按字段名排序
                    var sortedFields = allFields.OrderBy(f => f).ToList();

                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 写入BOM，确保Excel正确识别UTF-8编码
                        sw.Write('\uFEFF');
                        
                        // 写入表头
                        sw.WriteLine(string.Join(",", sortedFields.Select(field => EscapeCsvField(field))));
                        
                        // 写入数据行
                        foreach (var doc in documents)
                        {
                            List<string> row = new List<string>();
                            foreach (var field in sortedFields)
                            {
                                if (doc.Contains(field))
                                {
                                    string value = ConvertBsonValueToString(doc[field]);
                                    row.Add(EscapeCsvField(value));
                                }
                                else
                                {
                                    row.Add("");
                                }
                            }
                            sw.WriteLine(string.Join(",", row));
                        }
                    }
                    
                    SetStatus($"集合 {databaseName}.{collectionName} 已导出为 CSV，共 {documents.Count} 条记录", Color.Green);
                    MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 从 .csv 导入
        private void MenuItemImportCsv_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show("请先选择要导入的集合节点。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV文件 (*.csv)|*.csv";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    
                    var lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                    if (lines.Length < 2)
                    {
                        MessageBox.Show("CSV文件格式不正确，至少需要表头行和一行数据。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 解析表头
                    var headers = ParseCsvLine(lines[0]);
                    
                    var docs = new List<BsonDocument>();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(lines[i]))
                        {
                            var values = ParseCsvLine(lines[i]);
                            var doc = new BsonDocument();
                            
                            for (int j = 0; j < Math.Min(headers.Count, values.Count); j++)
                            {
                                if (!string.IsNullOrEmpty(headers[j]) && !string.IsNullOrEmpty(values[j]))
                                {
                                    doc.Add(headers[j], ParseStringToBsonValue(values[j]));
                                }
                            }
                            
                            if (doc.ElementCount > 0)
                            {
                                docs.Add(doc);
                            }
                        }
                    }
                    
                    if (docs.Count > 0)
                    {
                        collection.InsertMany(docs);
                        SetStatus($"已从 CSV 导入 {docs.Count} 条记录到 {databaseName}.{collectionName}", Color.Green);
                        MessageBox.Show("导入成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("没有有效的数据行可导入。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 转义CSV字段
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            // 如果字段包含逗号、双引号或换行符，需要用双引号包围
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // 将字段中的双引号替换为两个双引号
                field = field.Replace("\"", "\"\"");
                return "\"" + field + "\"";
            }
            return field;
        }

        // 解析CSV行
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            bool inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 双引号转义
                        current += '"';
                        i++; // 跳过下一个双引号
                    }
                    else
                    {
                        // 切换引号状态
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            
            result.Add(current);
            return result;
        }

        // 将BsonValue转换为字符串
        private string ConvertBsonValueToString(BsonValue value)
        {
            if (value == null || value.IsBsonNull)
                return "";
            
            switch (value.BsonType)
            {
                case BsonType.ObjectId:
                    return value.AsObjectId.ToString();
                case BsonType.DateTime:
                    return value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                case BsonType.Boolean:
                    return value.AsBoolean.ToString().ToLower();
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.Double:
                case BsonType.Decimal128:
                    return value.ToString();
                case BsonType.Array:
                    return value.AsBsonArray.ToJson();
                case BsonType.Document:
                    return value.AsBsonDocument.ToJson();
                default:
                    return value.ToString();
            }
        }

        // 将字符串解析为BsonValue
        private BsonValue ParseStringToBsonValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return BsonNull.Value;
            
            // 尝试解析为数字
            if (int.TryParse(value, out int intValue))
                return new BsonInt32(intValue);
            
            if (long.TryParse(value, out long longValue))
                return new BsonInt64(longValue);
            
            if (double.TryParse(value, out double doubleValue))
                return new BsonDouble(doubleValue);
            
            // 尝试解析为布尔值
            if (bool.TryParse(value, out bool boolValue))
                return new BsonBoolean(boolValue);
            
            // 尝试解析为日期时间
            if (DateTime.TryParse(value, out DateTime dateValue))
                return new BsonDateTime(dateValue);
            
            // 尝试解析为ObjectId
            if (ObjectId.TryParse(value, out ObjectId objectId))
                return new BsonObjectId(objectId);
            
            // 尝试解析为JSON对象或数组
            if (value.StartsWith("{") || value.StartsWith("["))
            {
                try
                {
                    return BsonDocument.Parse(value);
                }
                catch
                {
                    // 如果解析失败，作为普通字符串处理
                }
            }
            
            // 默认为字符串
            return new BsonString(value);
        }

        #region 语言管理

        // 应用语言到UI
        private void ApplyLanguageToUI()
        {
            try
            {
                // 更新主菜单
                menuItemConnection.Text = LanguageManager.GetString("menu_connection", "连接");
                menuItemConnect.Text = LanguageManager.GetString("menu_connect", "连接配置");
                menuItemReconnect.Text = LanguageManager.GetString("menu_reconnect", "重新连接");
                menuItemRefresh.Text = LanguageManager.GetString("menu_refresh", "刷新");
                menuItemTools.Text = LanguageManager.GetString("menu_tools", "工具");
                menuItemNativeQuery.Text = LanguageManager.GetString("menu_native_query", "原生语句");
                menuItemLanguage.Text = LanguageManager.GetString("menu_language", "语言");
                menuItemEnglish.Text = LanguageManager.GetString("menu_english", "英语");
                menuItemSimplifiedChinese.Text = LanguageManager.GetString("menu_simplified_chinese", "简体中文");
                menuItemTraditionalChinese.Text = LanguageManager.GetString("menu_traditional_chinese", "繁体中文");
                menuItemLanguageSettings.Text = LanguageManager.GetString("dialog_language_settings", "语言设置");

                // 更新窗体标题
                this.Text = LanguageManager.GetString("form_title", "MongoDB Compass");

                // 更新过滤框占位符（.NET Framework 4.8不支持PlaceholderText，使用Text属性）
                if (textBoxCollectionFilter != null && string.IsNullOrEmpty(textBoxCollectionFilter.Text))
                {
                    textBoxCollectionFilter.Text = LanguageManager.GetString("placeholder_filter", "输入过滤条件...");
                }

                // 更新状态栏
                SetStatus(LanguageManager.GetString("status_ready", "就绪"));
                
                // 更新所有打开的标签页标题
                UpdateAllTabTitles();
                
                // 更新动态创建的工具栏按钮
                UpdateToolbarButtons();
                
                // 更新TabControl的右键菜单
                UpdateTabContextMenu();
                
                // 更新DataGridView的右键菜单
                UpdateDataGridViewContextMenus();
                
                // 更新树形控件的右键菜单
                UpdateTreeViewContextMenus();
                
                // 更新树形控件中的索引节点文本
                UpdateTreeViewIndexNodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用语言到UI失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 更新工具栏按钮
        private void UpdateToolbarButtons()
        {
            try
            {
                foreach (TabPage tab in tabControlData.TabPages)
                {
                    if (tab.Controls.Count > 0 && tab.Controls[0] is TableLayoutPanel mainLayout)
                    {
                        if (mainLayout.Controls.Count > 0 && mainLayout.Controls[0] is Panel toolbarPanel)
                        {
                            foreach (Control control in toolbarPanel.Controls)
                            {
                                if (control is Button button)
                                {
                                    // 根据按钮的原始文本更新为对应的语言
                                    UpdateButtonText(button);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新工具栏按钮失败: {ex.Message}");
            }
        }
        
        // 更新按钮文本
        private void UpdateButtonText(Button button)
        {
            // 由于按钮创建时已经使用了LanguageManager.GetString，我们需要重新获取对应的文本
            // 这里我们通过按钮的Tag来存储语言键，如果没有Tag则通过文本内容推断
            string languageKey = button.Tag as string;
            string newText = "";
            
            if (!string.IsNullOrEmpty(languageKey))
            {
                // 如果有Tag存储的语言键，直接使用
                newText = LanguageManager.GetString(languageKey, button.Text);
            }
            else
            {
                // 否则通过当前文本推断语言键
                string currentText = button.Text;
                languageKey = GetLanguageKeyFromText(currentText);
                if (!string.IsNullOrEmpty(languageKey))
                {
                    newText = LanguageManager.GetString(languageKey, currentText);
                }
            }
            
            if (!string.IsNullOrEmpty(newText))
            {
                button.Text = newText;
            }
        }
        
        // 更新TabControl的右键菜单
        private void UpdateTabContextMenu()
        {
            try
            {
                if (tabControlData.ContextMenuStrip != null)
                {
                    foreach (ToolStripItem item in tabControlData.ContextMenuStrip.Items)
                    {
                        if (item is ToolStripMenuItem menuItem)
                        {
                            UpdateMenuItemText(menuItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新Tab右键菜单失败: {ex.Message}");
            }
        }
        
        // 更新DataGridView的右键菜单
        private void UpdateDataGridViewContextMenus()
        {
            try
            {
                foreach (TabPage tab in tabControlData.TabPages)
                {
                    if (tab.Controls.Count > 0 && tab.Controls[0] is TableLayoutPanel mainLayout)
                    {
                        if (mainLayout.Controls.Count > 2 && mainLayout.Controls[2] is DataGridView dataGridView)
                        {
                            if (dataGridView.ContextMenuStrip != null)
                            {
                                foreach (ToolStripItem item in dataGridView.ContextMenuStrip.Items)
                                {
                                    if (item is ToolStripMenuItem menuItem)
                                    {
                                        UpdateMenuItemText(menuItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新DataGridView右键菜单失败: {ex.Message}");
            }
        }
        
        // 更新树形控件的右键菜单
        private void UpdateTreeViewContextMenus()
        {
            try
            {
                // 更新所有节点的右键菜单
                UpdateTreeNodeContextMenus(treeViewCollections.Nodes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新树形控件右键菜单失败: {ex.Message}");
            }
        }
        
        // 更新树形控件中的索引节点文本
        private void UpdateTreeViewIndexNodes()
        {
            try
            {
                UpdateTreeNodeTexts(treeViewCollections.Nodes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新树形控件索引节点文本失败: {ex.Message}");
            }
        }
        
        // 递归更新树节点的文本
        private void UpdateTreeNodeTexts(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                // 更新索引节点文本
                if (node.Tag != null && node.Tag.ToString().StartsWith("indexes:"))
                {
                    node.Text = LanguageManager.GetString("text_index", "索引");
                }
                // 更新"无匹配集合"节点文本
                else if (node.Text == "无匹配集合" || node.Text == "No matching collections" || node.Text == "無匹配集合")
                {
                    node.Text = LanguageManager.GetString("text_no_matching_collections", "无匹配集合");
                }
                // 更新"暂无索引"节点文本
                else if (node.Text == "暂无索引" || node.Text == "No indexes" || node.Text == "暫無索引")
                {
                    node.Text = LanguageManager.GetString("text_no_indexes", "暂无索引");
                }
                // 更新默认索引显示文本
                else if (node.Text.StartsWith("_id (") && node.Text.EndsWith(")"))
                {
                    string defaultText = LanguageManager.GetString("text_default", "(默认)");
                    node.Text = "_id (" + defaultText + ")";
                }
                
                // 递归更新子节点
                if (node.Nodes.Count > 0)
                {
                    UpdateTreeNodeTexts(node.Nodes);
                }
            }
        }
        
        // 递归更新树节点的右键菜单
        private void UpdateTreeNodeContextMenus(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.ContextMenuStrip != null)
                {
                    foreach (ToolStripItem item in node.ContextMenuStrip.Items)
                    {
                        if (item is ToolStripMenuItem menuItem)
                        {
                            UpdateMenuItemText(menuItem);
                        }
                    }
                }
                
                // 递归处理子节点
                if (node.Nodes.Count > 0)
                {
                    UpdateTreeNodeContextMenus(node.Nodes);
                }
            }
        }
        
        // 更新菜单项文本
        private void UpdateMenuItemText(ToolStripMenuItem menuItem)
        {
            // 通过菜单项的Tag来存储语言键，如果没有Tag则通过文本内容推断
            string languageKey = menuItem.Tag as string;
            string newText = "";
            
            if (!string.IsNullOrEmpty(languageKey))
            {
                // 如果有Tag存储的语言键，直接使用
                newText = LanguageManager.GetString(languageKey, menuItem.Text);
            }
            else
            {
                // 否则通过当前文本推断语言键
                string currentText = menuItem.Text;
                languageKey = GetLanguageKeyFromMenuItemText(currentText);
                if (!string.IsNullOrEmpty(languageKey))
                {
                    newText = LanguageManager.GetString(languageKey, currentText);
                }
            }
            
            if (!string.IsNullOrEmpty(newText))
            {
                menuItem.Text = newText;
            }
            
            // 递归处理子菜单项
            if (menuItem.DropDownItems.Count > 0)
            {
                foreach (ToolStripItem item in menuItem.DropDownItems)
                {
                    if (item is ToolStripMenuItem subMenuItem)
                    {
                        UpdateMenuItemText(subMenuItem);
                    }
                }
            }
        }
        
        // 从菜单项文本中推断语言键
        private string GetLanguageKeyFromMenuItemText(string text)
        {
            // 根据文本内容推断对应的语言键
            switch (text)
            {
                case "打开集合":
                case "Open Collection":
                    return "text_open_collection";
                case "清空集合":
                case "Clear Collection":
                    return "text_clear_collection";
                case "复制集合":
                case "Copy Collection":
                    return "text_copy_collection";
                case "删除集合":
                case "Delete Collection":
                    return "text_delete_collection";
                case "导出集合":
                case "Export Collection":
                    return "text_export_collection";
                case "导入集合":
                case "Import Collection":
                    return "text_import_collection";
                case "导出JSON":
                case "Export JSON":
                    return "text_export_json";
                case "导出BSON":
                case "Export BSON":
                    return "text_export_bson";
                case "导出CSV":
                case "Export CSV":
                    return "text_export_csv";
                case "导入JSON":
                case "Import JSON":
                    return "text_import_json";
                case "导入BSON":
                case "Import BSON":
                    return "text_import_bson";
                case "导入CSV":
                case "Import CSV":
                    return "text_import_csv";
                case "创建集合":
                case "Create Collection":
                    return "text_create_collection";
                case "导入目录数据":
                case "Import Directory Data":
                    return "text_import_directory";
                case "备份数据库":
                case "Backup Database":
                    return "text_backup_database";
                case "关闭数据库连接":
                case "Close Database Connection":
                    return "text_close_database";
                case "关闭标签页":
                case "Close Tab":
                    return "text_close_tab";
                case "关闭所有标签页":
                case "Close All Tabs":
                    return "text_close_all_tabs";
                case "修改记录":
                case "Edit Record":
                    return "text_edit_record";
                case "复制记录":
                case "Copy Record":
                    return "text_copy_record";
                case "删除记录":
                case "Delete Record":
                    return "text_delete_record";
                case "查看索引":
                case "View Indexes":
                    return "text_view_indexes";
                case "创建索引":
                case "Create Index":
                    return "text_create_index";
                case "删除索引":
                case "Delete Index":
                    return "text_delete_index";
                case "刷新索引":
                case "Refresh Indexes":
                    return "text_refresh_indexes";
                case "查看详情":
                case "View Details":
                    return "text_view_details";
                default:
                    return null;
            }
        }

        // 更新所有标签页标题
        private void UpdateAllTabTitles()
        {
            try
            {
                foreach (TabPage tab in tabControlData.TabPages)
                {
                    if (tab.Tag != null && tab.Tag is string[] tagData && tagData.Length >= 2)
                    {
                        string databaseName = tagData[0];
                        string collectionName = tagData[1];
                        tab.Text = $"{databaseName}.{collectionName}";
                    }
                }
            }
            catch (Exception ex)
            {
                // 静默处理，不影响主要功能
                Console.WriteLine($"更新标签页标题失败: {ex.Message}");
            }
        }

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

        private void MenuItemEnglish_Click(object sender, EventArgs e)
        {
            LanguageManager.SwitchLanguage("en");
        }

        private void MenuItemSimplifiedChinese_Click(object sender, EventArgs e)
        {
            LanguageManager.SwitchLanguage("zhcn");
        }

        private void MenuItemTraditionalChinese_Click(object sender, EventArgs e)
        {
            LanguageManager.SwitchLanguage("zhtw");
        }
        
        // 打开语言设置对话框
        private void MenuItemLanguageSettings_Click(object sender, EventArgs e)
        {
            try
            {
                var languageSettingsForm = new LanguageSettingsForm();
                languageSettingsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开语言设置失败: {ex.Message}", 
                    LanguageManager.GetString("dialog_error", "错误"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        // 导出MsSql脚本
        private void MenuItemExportMsSql_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_select_collection", "请先选择要操作的集合节点"), 
                    LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "SQL文件 (*.sql)|*.sql";
            sfd.FileName = collectionName + "_mssql.sql";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                    
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 生成MsSql脚本
                        GenerateMsSqlScript(sw, collectionName, documents);
                    }
                    
                    SetStatus($"集合 {databaseName}.{collectionName} 已导出为 MsSql 脚本，共 {documents.Count} 条记录", Color.Green);
                    MessageBox.Show(LanguageManager.GetString("msg_export_success_simple", "导出成功！"), 
                        LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}").Replace("{0}", ex.Message), 
                        LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 导出Mysql脚本
        private void MenuItemExportMySql_Click(object sender, EventArgs e)
        {
            if (treeViewCollections.SelectedNode == null || treeViewCollections.SelectedNode.Tag == null || !treeViewCollections.SelectedNode.Tag.ToString().StartsWith("collection:"))
            {
                MessageBox.Show(LanguageManager.GetString("msg_please_select_collection", "请先选择要操作的集合节点"), 
                    LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = treeViewCollections.SelectedNode.Tag.ToString().Split(':')[1].Split('.');
            string databaseName = parts[0];
            string collectionName = parts[1];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "SQL文件 (*.sql)|*.sql";
            sfd.FileName = collectionName + "_mysql.sql";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var database = connectedDatabases[databaseName];
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                    
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 生成Mysql脚本
                        GenerateMySqlScript(sw, collectionName, documents);
                    }
                    
                    SetStatus($"集合 {databaseName}.{collectionName} 已导出为 Mysql 脚本，共 {documents.Count} 条记录", Color.Green);
                    MessageBox.Show(LanguageManager.GetString("msg_export_success_simple", "导出成功！"), 
                        LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.GetString("msg_operation_failed", "操作失败: {0}").Replace("{0}", ex.Message), 
                        LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 生成MsSql脚本
        private void GenerateMsSqlScript(StreamWriter writer, string tableName, List<BsonDocument> documents)
        {
            if (documents.Count == 0) return;

            // 分析文档结构，确定列名和数据类型
            var columnInfo = AnalyzeDocumentStructure(documents);
            
            // 生成删除表的语句
            writer.WriteLine($"-- 删除已存在的表");
            writer.WriteLine($"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL");
            writer.WriteLine($"    DROP TABLE {tableName};");
            writer.WriteLine();
            
            // 生成创建表的语句
            writer.WriteLine($"-- 创建表");
            writer.WriteLine($"CREATE TABLE {tableName} (");
            
            var columnDefinitions = new List<string>();
            // 添加自增主键id列
            columnDefinitions.Add("    id INT IDENTITY(1,1) PRIMARY KEY");
            foreach (var column in columnInfo)
            {
                string columnDef = $"    {column.Key} {GetMsSqlDataType(column.Value)}";
                columnDefinitions.Add(columnDef);
            }
            
            writer.WriteLine(string.Join(",\n", columnDefinitions));
            writer.WriteLine(");");
            writer.WriteLine();
            
            // 生成插入数据的语句
            if (documents.Count > 0)
            {
                writer.WriteLine($"-- 插入数据");
                
                foreach (var doc in documents)
                {
                    var values = new List<string>();
                    foreach (var column in columnInfo)
                    {
                        // 再次确认跳过指定字段
                        if (SkipFields.Contains(column.Key))
                            continue;
                            
                        string value = GetMsSqlValue(doc.GetValue(column.Key, BsonNull.Value));
                        values.Add(value);
                    }
                    
                    // 过滤掉跳过的字段名
                    var filteredColumns = columnInfo.Keys.Where(key => !SkipFields.Contains(key)).ToList();
                    writer.WriteLine($"INSERT INTO {tableName} ({string.Join(", ", filteredColumns)}) VALUES ({string.Join(", ", values)});");
                }
            }
        }

        // 生成Mysql脚本
        private void GenerateMySqlScript(StreamWriter writer, string tableName, List<BsonDocument> documents)
        {
            if (documents.Count == 0) return;

            // 分析文档结构，确定列名和数据类型
            var columnInfo = AnalyzeDocumentStructure(documents);
            
            // 生成删除表的语句
            writer.WriteLine($"-- 删除已存在的表");
            writer.WriteLine($"DROP TABLE IF EXISTS `{tableName}`;");
            writer.WriteLine();
            
            // 生成创建表的语句
            writer.WriteLine($"-- 创建表");
            writer.WriteLine($"CREATE TABLE `{tableName}` (");
            
            var columnDefinitions = new List<string>();
            // 添加自增主键id列
            columnDefinitions.Add("    `id` INT AUTO_INCREMENT PRIMARY KEY");
            foreach (var column in columnInfo)
            {
                string columnDef = $"    `{column.Key}` {GetMySqlDataType(column.Value)}";
                columnDefinitions.Add(columnDef);
            }
            
            writer.WriteLine(string.Join(",\n", columnDefinitions));
            writer.WriteLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;");
            writer.WriteLine();
            
            // 生成插入数据的语句
            if (documents.Count > 0)
            {
                writer.WriteLine($"-- 插入数据");
                
                foreach (var doc in documents)
                {
                    var values = new List<string>();
                    foreach (var column in columnInfo)
                    {
                        // 再次确认跳过指定字段
                        if (SkipFields.Contains(column.Key))
                            continue;
                            
                        string value = GetMySqlValue(doc.GetValue(column.Key, BsonNull.Value));
                        values.Add(value);
                    }
                    
                    // 过滤掉跳过的字段名
                    var filteredColumns = columnInfo.Keys.Where(key => !SkipFields.Contains(key)).ToList();
                    writer.WriteLine($"INSERT INTO `{tableName}` (`{string.Join("`, `", filteredColumns)}`) VALUES ({string.Join(", ", values)});");
                }
            }
        }

        // 分析文档结构
        private Dictionary<string, BsonType> AnalyzeDocumentStructure(List<BsonDocument> documents)
        {
            var columnInfo = new Dictionary<string, BsonType>();

            foreach (var doc in documents)
            {
                foreach (var element in doc)
                {
                    // 跳过指定的字段
                    if (SkipFields.Contains(element.Name))
                        continue;
                    
                    if (!columnInfo.ContainsKey(element.Name))
                    {
                        columnInfo[element.Name] = element.Value.BsonType;
                    }
                    else
                    {
                        // 如果已存在，选择更通用的类型
                        columnInfo[element.Name] = GetMoreGeneralType(columnInfo[element.Name], element.Value.BsonType);
                    }
                }
            }
            
            return columnInfo;
        }

        // 获取更通用的类型
        private BsonType GetMoreGeneralType(BsonType type1, BsonType type2)
        {
            if (type1 == type2) return type1;
            
            // 如果一个是字符串，另一个是数值，选择字符串
            if ((type1 == BsonType.String && (type2 == BsonType.Int32 || type2 == BsonType.Int64 || type2 == BsonType.Double)) ||
                (type2 == BsonType.String && (type1 == BsonType.Int32 || type1 == BsonType.Int64 || type1 == BsonType.Double)))
            {
                return BsonType.String;
            }
            
            // 如果一个是Int32，另一个是Int64，选择Int64
            if ((type1 == BsonType.Int32 && type2 == BsonType.Int64) ||
                (type2 == BsonType.Int32 && type1 == BsonType.Int64))
            {
                return BsonType.Int64;
            }
            
            // 如果一个是数值，另一个是Double，选择Double
            if ((type1 == BsonType.Int32 || type1 == BsonType.Int64) && type2 == BsonType.Double)
            {
                return BsonType.Double;
            }
            if ((type2 == BsonType.Int32 || type2 == BsonType.Int64) && type1 == BsonType.Double)
            {
                return BsonType.Double;
            }
            
            // 默认返回第一个类型
            return type1;
        }

        // 获取MsSql数据类型
        private string GetMsSqlDataType(BsonType bsonType)
        {
            switch (bsonType)
            {
                case BsonType.Int32:
                    return "INT";
                case BsonType.Int64:
                    return "BIGINT";
                case BsonType.Double:
                    return "FLOAT";
                case BsonType.Boolean:
                    return "BIT";
                case BsonType.DateTime:
                    return "DATETIME2";
                case BsonType.ObjectId:
                case BsonType.String:
                default:
                    return "NVARCHAR(MAX)";
            }
        }

        // 获取MySql数据类型
        private string GetMySqlDataType(BsonType bsonType)
        {
            switch (bsonType)
            {
                case BsonType.Int32:
                    return "INT";
                case BsonType.Int64:
                    return "BIGINT";
                case BsonType.Double:
                    return "DOUBLE";
                case BsonType.Boolean:
                    return "BOOLEAN";
                case BsonType.DateTime:
                    return "DATETIME";
                case BsonType.ObjectId:
                case BsonType.String:
                default:
                    return "TEXT";
            }
        }

        // 获取MsSql值
        private string GetMsSqlValue(BsonValue value)
        {
            if (value.IsBsonNull)
                return "NULL";
            
            switch (value.BsonType)
            {
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.Double:
                    return value.ToString();
                case BsonType.Boolean:
                    return value.AsBoolean ? "1" : "0";
                case BsonType.DateTime:
                    return $"'{value.ToUniversalTime():yyyy-MM-dd HH:mm:ss}'";
                case BsonType.ObjectId:
                case BsonType.String:
                default:
                    return $"N'{EscapeMsSqlString(value.ToString())}'";
            }
        }

        // 获取MySql值
        private string GetMySqlValue(BsonValue value)
        {
            if (value.IsBsonNull)
                return "NULL";
            
            switch (value.BsonType)
            {
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.Double:
                    return value.ToString();
                case BsonType.Boolean:
                    return value.AsBoolean ? "1" : "0";
                case BsonType.DateTime:
                    return $"'{value.ToUniversalTime():yyyy-MM-dd HH:mm:ss}'";
                case BsonType.ObjectId:
                case BsonType.String:
                default:
                    return $"'{EscapeMySqlString(value.ToString())}'";
            }
        }

        // 转义MsSql字符串
        private string EscapeMsSqlString(string str)
        {
            return str.Replace("'", "''");
        }

        // 转义MySql字符串
        private string EscapeMySqlString(string str)
        {
            return str.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        // 显示删除属性对话框
        private void ShowDeleteFieldDialog(string databaseName, string collectionName)
        {
            try
            {
                // 获取集合中的所有字段
                var collection = connectedDatabases[databaseName].GetCollection<BsonDocument>(collectionName);
                var sampleDocuments = collection.Find(new BsonDocument()).Limit(100).ToList();
                
                if (sampleDocuments.Count == 0)
                {
                    MessageBox.Show(LanguageManager.GetString("msg_no_documents", "集合中没有文档"), 
                        LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 收集所有字段名
                var allFields = new HashSet<string>();
                foreach (var doc in sampleDocuments)
                {
                    foreach (var element in doc)
                    {
                        allFields.Add(element.Name);
                    }
                }

                // 过滤掉系统字段
                var userFields = allFields.Where(field => !SkipFields.Contains(field)).ToList();
                
                if (userFields.Count == 0)
                {
                    MessageBox.Show(LanguageManager.GetString("msg_no_user_fields", "没有可删除的用户字段"), 
                        LanguageManager.GetString("dialog_info", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建对话框
                using (var dialog = new Form())
                {
                    dialog.Text = LanguageManager.GetString("dialog_delete_field", "删除属性");
                    dialog.Size = new Size(450, 350);
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dialog.MaximizeBox = false;
                    dialog.MinimizeBox = false;

                    // 创建标签
                    var label = new Label();
                    label.Text = LanguageManager.GetString("text_select_field_to_delete", "请选择要删除的属性:");
                    label.Location = new Point(20, 20);
                    label.Size = new Size(400, 20);
                    dialog.Controls.Add(label);

                    // 创建下拉框（支持手动输入）
                    var comboBox = new ComboBox();
                    comboBox.Location = new Point(20, 50);
                    comboBox.Size = new Size(400, 25);
                    comboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
                    comboBox.Items.AddRange(userFields.ToArray());
                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = 0;
                    dialog.Controls.Add(comboBox);

                    // 创建提示标签
                    var hintLabel = new Label();
                    hintLabel.Text = LanguageManager.GetString("text_field_input_hint", "提示：可以从下拉列表中选择现有字段，也可以直接输入要删除的字段名");
                    hintLabel.Location = new Point(20, 85);
                    hintLabel.Size = new Size(400, 40);
                    hintLabel.ForeColor = Color.Gray;
                    hintLabel.Font = new Font(hintLabel.Font.FontFamily, 9);
                    dialog.Controls.Add(hintLabel);

                    // 创建确认按钮
                    var btnOK = new Button();
                    btnOK.Text = LanguageManager.GetString("text_ok", "确定");
                    btnOK.Location = new Point(250, 270);
                    btnOK.Size = new Size(80, 30);
                    btnOK.DialogResult = DialogResult.OK;
                    dialog.Controls.Add(btnOK);

                    // 创建取消按钮
                    var btnCancel = new Button();
                    btnCancel.Text = LanguageManager.GetString("text_cancel", "取消");
                    btnCancel.Location = new Point(340, 270);
                    btnCancel.Size = new Size(80, 30);
                    btnCancel.DialogResult = DialogResult.Cancel;
                    dialog.Controls.Add(btnCancel);

                    // 显示对话框
                    if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(comboBox.Text))
                    {
                        string fieldToDelete = comboBox.Text.Trim();
                        
                        // 确认删除
                        var result = MessageBox.Show(
                            string.Format(LanguageManager.GetString("msg_confirm_delete_field", "确定要删除属性 '{0}' 吗？\n此操作将删除集合中所有文档的该属性，且不可恢复。"), fieldToDelete),
                            LanguageManager.GetString("dialog_confirm", "确认"),
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            DeleteFieldFromCollection(databaseName, collectionName, fieldToDelete);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_delete_field_failed", "删除属性失败: {0}"), ex.Message),
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 从集合中删除指定属性
        private void DeleteFieldFromCollection(string databaseName, string collectionName, string fieldName)
        {
            try
            {
                SetStatus(string.Format("正在删除属性 '{0}'...", fieldName));
                
                var collection = connectedDatabases[databaseName].GetCollection<BsonDocument>(collectionName);
                
                // 构建更新操作，使用 $unset 操作符删除字段
                var update = Builders<BsonDocument>.Update.Unset(fieldName);
                var filter = new BsonDocument(); // 匹配所有文档
                
                // 执行批量更新
                var result = collection.UpdateMany(filter, update);
                
                SetStatus(string.Format("成功删除属性 '{0}'，影响了 {1} 个文档", fieldName, result.ModifiedCount), Color.Green);
                
                // 刷新当前标签页的数据
                RefreshCurrentTabData();
                
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_delete_field_success", "成功删除属性 '{0}'，影响了 {1} 个文档"), fieldName, result.ModifiedCount),
                    LanguageManager.GetString("dialog_success", "成功"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetStatus(string.Format("删除属性失败: {0}", ex.Message), Color.Red);
                MessageBox.Show(string.Format(LanguageManager.GetString("msg_delete_field_failed", "删除属性失败: {0}"), ex.Message),
                    LanguageManager.GetString("dialog_error", "错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 刷新当前标签页的数据
        private void RefreshCurrentTabData()
        {
            var currentTab = tabControlData.SelectedTab;
            if (currentTab != null && currentTab.Tag != null)
            {
                var tabData = currentTab.Tag as dynamic;
                if (tabData != null && tabData.DataGridView != null)
                {
                    // 重新执行查询以刷新数据
                    if (tabData.QueryTextBox != null && !string.IsNullOrEmpty(tabData.QueryTextBox.Text))
                    {
                        ExecuteQuery(tabData.DatabaseName, tabData.CollectionName, tabData.QueryTextBox.Text, tabData.DataGridView);
                    }
                    else
                    {
                        // 如果没有查询条件，执行默认查询
                        ExecuteQuery(tabData.DatabaseName, tabData.CollectionName, "{}", tabData.DataGridView);
                    }
                }
            }
        }
    }
} 