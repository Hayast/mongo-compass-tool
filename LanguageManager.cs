using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace mongo
{
    public static class LanguageManager
    {
        private static Dictionary<string, string> languageStrings = new Dictionary<string, string>();
        private static string currentLanguage = "zhcn"; // 默认简体中文
        
        // 语言切换事件
        public static event EventHandler LanguageChanged;
        
        // 支持的语言列表
        public static readonly Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>
        {
            {"zhcn", "简体中文"},
            {"zhtw", "繁體中文"},
            {"en", "English"}
        };

        // 初始化语言系统
        public static void Initialize()
        {
            try
            {
                // 加载上次的语言设置
                LoadLanguageSetting();
                
                // 创建语言目录
                string languageDir = Path.Combine(Application.StartupPath, "language");
                if (!Directory.Exists(languageDir))
                {
                    Directory.CreateDirectory(languageDir);
                    CreateDefaultLanguageFiles(languageDir);
                }

                // 加载当前语言
                LoadLanguage(currentLanguage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化语言系统失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 创建默认语言文件
        private static void CreateDefaultLanguageFiles(string languageDir)
        {
            // 简体中文
            CreateLanguageFile(Path.Combine(languageDir, "zhcn.ini"), GetDefaultChineseSimplifiedStrings());
            // 繁体中文
            CreateLanguageFile(Path.Combine(languageDir, "zhtw.ini"), GetDefaultChineseTraditionalStrings());
            // 英语
            CreateLanguageFile(Path.Combine(languageDir, "en.ini"), GetDefaultEnglishStrings());
        }

        // 创建语言文件
        private static void CreateLanguageFile(string filePath, Dictionary<string, string> strings)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var kvp in strings)
                {
                    writer.WriteLine($"{kvp.Key}={kvp.Value}");
                }
            }
        }

        // 获取默认简体中文字符串
        private static Dictionary<string, string> GetDefaultChineseSimplifiedStrings()
        {
            return new Dictionary<string, string>
            {
                // 主菜单
                {"menu_connection", "连接"},
                {"menu_connect", "连接配置"},
                {"menu_reconnect", "重新连接"},
                {"menu_refresh", "刷新"},
                {"menu_tools", "工具"},
                {"menu_native_query", "原生语句"},
                {"menu_language", "语言"},
                {"menu_english", "英语"},
                {"menu_simplified_chinese", "简体中文"},
                {"menu_traditional_chinese", "繁体中文"},
                
                // 状态栏
                {"status_ready", "就绪"},
                {"status_connecting", "正在连接..."},
                {"status_connected", "已连接"},
                {"status_loading", "正在加载..."},
                {"status_importing", "正在导入..."},
                {"status_exporting", "正在导出..."},
                {"status_backing_up", "正在备份..."},
                {"status_restoring", "正在恢复..."},
                {"status_testing", "正在测试..."},
                {"status_testing_network", "正在测试网络连接..."},
                
                // 对话框标题
                {"dialog_connect", "连接配置"},
                {"dialog_error", "错误"},
                {"dialog_warning", "警告"},
                {"dialog_info", "信息"},
                {"dialog_confirm", "确认"},
                {"dialog_add_connection", "添加连接"},
                {"dialog_edit_connection", "编辑连接"},
                {"dialog_copy_connection", "复制连接"},
                {"dialog_add_record", "添加记录"},
                {"dialog_edit_record", "编辑记录"},
                {"dialog_batch_add", "批量添加"},
                {"dialog_import_data", "导入数据"},
                {"dialog_export_data", "导出数据"},
                {"dialog_create_index", "创建索引"},
                {"dialog_backup_database", "备份数据库"},
                {"dialog_restore_database", "恢复数据库"},
                {"dialog_language_settings", "语言设置"},
                
                // 按钮文本
                {"btn_connect", "连接"},
                {"btn_cancel", "取消"},
                {"btn_ok", "确定"},
                {"btn_yes", "是"},
                {"btn_no", "否"},
                {"btn_close", "关闭"},
                {"btn_save", "保存"},
                {"btn_load", "加载"},
                {"btn_add", "添加"},
                {"btn_edit", "编辑"},
                {"btn_delete", "删除"},
                {"btn_copy", "复制"},
                {"btn_backup", "备份"},
                {"btn_restore", "恢复"},
                {"btn_test", "测试"},
                {"btn_open_compass", "打开Compass"},
                {"btn_clear", "清空"},
                {"btn_export", "导出"},
                {"btn_import", "导入"},
                {"btn_execute", "执行"},
                {"btn_create", "创建"},
                {"btn_view", "查看"},
                {"btn_refresh", "刷新"},
                {"btn_filter", "过滤"},
                {"btn_search", "搜索"},
                
                // 窗体标题
                {"form_title", "MongoDB Compass"},
                {"form_connection_list", "连接列表"},
                {"form_native_query", "原生查询"},
                {"form_index_manager", "索引管理"},
                {"form_data_viewer", "数据查看器"},
                
                // 标签文本
                {"label_host", "主机："},
                {"label_port", "端口："},
                {"label_database", "数据库："},
                {"label_username", "用户名："},
                {"label_password", "密码："},
                {"label_auth_mechanism", "认证机制："},
                {"label_auth_source", "认证源："},
                {"label_connection_name", "连接名称："},
                {"label_status", "状态："},
                {"label_language", "语言："},
                {"label_query", "查询语句："},
                {"label_filter", "过滤条件："},
                {"label_limit", "限制数量："},
                {"label_skip", "跳过数量："},
                {"label_sort", "排序："},
                {"label_projection", "投影："},
                {"label_index_name", "索引名称："},
                {"label_index_fields", "索引字段："},
                {"label_index_type", "索引类型："},
                {"label_unique", "唯一索引"},
                {"label_sparse", "稀疏索引"},
                {"label_background", "后台创建"},
                
                // 其他文本
                {"text_database", "数据库"},
                {"text_collection", "集合"},
                {"text_index", "索引"},
                {"text_query", "查询"},
                {"text_execute", "执行查询"},
                {"text_add_record", "新增记录"},
                {"text_batch_add", "批量新增"},
                {"text_import_json", "导入JSON"},
                {"text_import_csv", "导入CSV"},
                {"text_edit_record", "修改记录"},
                {"text_delete_record", "删除记录"},
                {"text_batch_delete", "批量删除"},
                {"text_refresh_data", "刷新数据"},
                {"text_export_json", "导出JSON"},
                {"text_export_csv", "导出CSV"},
                {"text_export_bson", "导出BSON"},
                {"text_clear_collection", "清空集合"},
                {"text_copy_collection", "复制集合"},
                {"text_delete_collection", "删除集合"},
                {"text_create_collection", "创建集合"},
                {"text_view_indexes", "查看索引"},
                {"text_create_index", "创建索引"},
                {"text_delete_index", "删除索引"},
                {"text_close_tab", "关闭标签页"},
                {"text_close_all_tabs", "关闭所有标签页"},
                {"text_copy_record", "复制记录"},
                {"text_export_record", "导出记录"},
                {"text_import_record", "导入记录"},
                {"text_open_collection", "打开集合"},
                {"text_export_collection", "导出集合"},
                {"text_import_collection", "导入集合"},
                {"text_import_directory", "导入目录数据"},
                {"text_backup_database", "备份数据库"},
                {"text_close_database", "关闭数据库连接"},
                {"text_refresh_indexes", "刷新索引"},
                {"text_view_details", "查看详情"},
                {"text_import_bson", "导入BSON"},
                
                
                {"label_host_address", "主机地址:"},
                
                // 原生查询相关
                {"text_native_query_execution", "原生MongoDB语句执行"},
                {"text_database_label", "数据库:"},
                {"text_native_operation_statement", "原生操作语句 (如 db.collection.find({})):"},
                {"text_execute_button", "执行"},
                {"text_execution_result", "执行结果:"},
                {"text_please_select_database", "请选择数据库"},
                {"text_please_enter_native_statement", "请输入原生MongoDB语句，如 db.collection.find({})"},
                {"text_only_support_db_collection_form", "暂仅支持 db.collection.method(args) 形式"},
                {"text_no_records", "无记录"},
                {"text_operation_not_supported", "暂不支持该操作"},
                {"text_execution_failed", "执行失败: "},
                
                // 索引创建相关
                {"text_index_name_label", "索引名称:"},
                {"text_field_definition_json", "字段定义 (JSON):"},
                {"text_index_options_json", "索引选项 (JSON):"},
                {"text_create_index_button", "创建索引"},
                
                // 导入确认相关
                {"text_import_data_confirmation", "导入数据确认"},
                {"text_confirm_import", "确认导入"},
                
                // 添加记录相关
                {"text_please_enter_json_document", "请输入JSON格式的文档数据："},
                {"text_add_record_button", "添加记录"},
                {"text_please_enter_json_array", "请输入JSON数组格式的多条文档数据："},
                {"text_batch_add_button", "批量添加"},
                
                // 编辑记录相关
                {"text_please_edit_json_format", "请编辑JSON（注意格式）:"},
                
                // 消息相关
                {"msg_please_select_record_to_edit", "请先选择要修改的记录。"},
                {"msg_cannot_get_record_id", "无法获取记录的 _id 字段，无法修改。"},
                {"msg_edit_success", "修改成功！"},
                {"msg_edit_failed", "修改失败: "},
                {"msg_please_select_record_to_copy", "请先选择要复制的记录。"},
                {"msg_copy_success", "复制成功！"},
                {"msg_copy_failed", "复制失败: "},
                {"msg_please_select_record_to_delete", "请先选择要删除的记录。"},
                {"msg_cannot_get_record_id_for_delete", "无法获取记录的 _id 字段，无法删除。"},
                {"msg_confirm_delete_record", "确定要删除该记录吗？"},
                {"msg_delete_record_failed", "未能删除记录。"},
                {"msg_delete_record_error", "删除失败: "},
                {"msg_load_indexes_failed", "加载索引失败: "},
                {"msg_view_index_details_failed", "查看索引详情失败: "},
                {"msg_cannot_delete_default_id_index", "不能删除默认的 _id 索引！"},
                {"msg_confirm_delete_index", "确定要删除索引 '{0}' 吗？\n此操作不可恢复！"},
                {"msg_index_delete_success", "索引删除成功！"},
                {"msg_delete_index_failed", "删除索引失败: "},
                {"msg_index_name_and_fields_required", "索引名称和字段定义不能为空"},
                {"msg_index_create_success", "索引创建成功！"},
                {"msg_create_index_failed", "创建索引失败: "},
                {"msg_invalid_collection_name", "集合名称格式无效！集合名称只能包含字母、数字、下划线和连字符。"},
                {"msg_collection_already_exists", "集合 '{0}' 已存在！"},
                {"msg_collection_create_success", "集合创建成功！"},
                {"msg_create_collection_failed", "创建集合失败: "},
                {"msg_confirm_close_database", "确定要关闭数据库 '{0}' 的连接吗？\n关闭后需要重新连接才能访问该数据库。"},
                {"msg_database_connection_closed", "数据库连接已关闭！"},
                {"msg_close_database_failed", "关闭数据库连接失败: "},
                {"msg_database_directory_not_found", "在选择的目录中未找到数据库目录 '{0}'"},
                {"msg_import_directory_failed", "导入目录数据失败: "},
                {"msg_no_importable_files", "在数据库目录中未找到任何可导入的文件\n(.bson文件或非.metadata.json文件)"},
                {"msg_show_import_confirm_failed", "显示导入确认对话框失败: "},
                {"msg_import_result", "导入结果"},
                {"msg_import_data_failed", "导入数据失败: "},
                {"msg_auto_reconnect_failed", "自动重连失败: "},
                {"msg_no_last_connection_params", "没有找到上次连接的参数记录"},
                {"msg_manual_reconnect_failed", "手动重连失败: "},
                {"msg_no_collections_to_backup", "数据库中没有集合，无需备份。"},
                {"msg_backup_completed", "备份完成"},
                {"msg_backup_database_failed", "备份数据库失败: "},
                {"msg_please_enter_json_data", "请输入JSON数据"},
                {"msg_add_record_success", "记录添加成功！"},
                {"msg_add_record_failed", "添加记录失败: "},
                {"msg_show_add_dialog_failed", "显示新增对话框失败: "},
                {"msg_no_valid_json_documents", "没有找到有效的JSON文档"},
                {"msg_batch_add_success", "成功批量添加 {0} 条记录！"},
                {"msg_batch_add_failed", "批量添加记录失败: "},
                {"msg_show_batch_add_dialog_failed", "显示批量新增对话框失败: "},
                {"msg_show_import_dialog_failed", "显示导入对话框失败: "},
                {"msg_csv_file_empty", "CSV文件为空"},
                {"msg_csv_format_error", "CSV文件格式错误：无法解析标题行"},
                {"msg_no_valid_csv_rows", "CSV文件中没有找到有效的数据行"},
                {"msg_import_csv_success", "成功导入 {0} 条记录！"},
                {"msg_import_csv_failed", "导入CSV文件失败: "},
                {"msg_no_valid_json_in_file", "文件中没有找到有效的JSON文档"},
                {"msg_import_json_success", "成功导入 {0} 条记录！"},
                {"msg_import_file_failed", "导入文件失败: "},
                {"msg_clear_collection_failed", "清空集合失败: "},
                {"msg_copy_collection_failed", "复制集合失败: "},
                {"msg_delete_collection_failed", "删除集合失败: "},
                {"msg_export_failed", "导出失败: "},
                {"msg_please_select_collection_to_import", "请先选择要导入的集合节点。"},
                {"msg_import_success", "导入成功！"},
                {"msg_file_content_empty", "文件内容为空。"},
                {"msg_import_failed", "导入失败: "},
                
                // 连接相关
                {"text_use_auth_source", "使用authSource"},
                {"text_default", "(默认)"},
                {"text_connections", "连接列表"},
                {"text_no_connections", "暂无连接"},
                {"text_connection_success", "连接成功"},
                {"text_connection_failed", "连接失败"},
                {"text_backup_database", "备份数据库"},
                {"text_restore_database", "恢复数据库"},
                
                // 消息
                {"msg_connection_success", "连接成功！"},
                {"msg_connection_failed", "连接失败：{0}"},

                {"msg_backup_success", "备份成功！"},
                {"msg_backup_failed", "备份失败：{0}"},
                {"msg_restore_success", "恢复成功！"},
                {"msg_restore_failed", "恢复失败：{0}"},
                {"msg_delete_confirm", "确定要删除连接 '{0}' 吗？"},
                {"msg_language_changed", "语言已切换到：{0}"},
                {"msg_delete_collection_confirm", "确定要删除集合 '{0}' 吗？"},
                {"msg_clear_collection_confirm", "确定要清空集合 '{0}' 吗？"},
                {"msg_delete_record_confirm", "确定要删除选中的记录吗？"},
                {"msg_batch_delete_confirm", "确定要删除选中的 {0} 条记录吗？"},
                {"msg_delete_index_confirm", "确定要删除索引 '{0}' 吗？"},
                {"msg_operation_success", "操作成功！"},
                {"msg_operation_failed", "操作失败：{0}"},
                {"msg_no_records_selected", "请先选择要操作的记录"},
                {"msg_invalid_query", "查询语句格式错误"},
                {"msg_invalid_json", "JSON格式错误"},
                {"msg_file_not_found", "文件未找到：{0}"},
                {"msg_import_success", "导入成功，共导入 {0} 条记录"},
                {"msg_export_success", "导出成功，共导出 {0} 条记录"},
                {"msg_auto_reconnect_prompt", "检测到上次连接参数，是否自动重连？"},
                {"msg_auto_reconnect_yes", "是"},
                {"msg_auto_reconnect_no", "否"},
                {"msg_auto_reconnect_remember", "记住选择"},
                {"msg_please_select_collection", "请先选择要操作的集合节点"},
                {"msg_export_success_simple", "导出成功！"},
                {"msg_import_success_simple", "导入成功！"},
                {"msg_file_empty", "文件内容为空"},
                {"msg_collection_exists", "集合 '{0}' 已存在！"},
                {"msg_collection_created", "集合创建成功！"},
                {"msg_collection_name_invalid", "集合名称格式无效！集合名称只能包含字母、数字、下划线和连字符"},
                {"msg_database_closed", "数据库连接已关闭！"},
                {"msg_no_last_connection", "没有找到上次连接的参数记录"},
                {"msg_no_collections_to_backup", "数据库中没有集合，无需备份"},
                {"msg_backup_completed", "备份完成"},
                {"msg_please_enter_json", "请输入JSON数据"},
                {"msg_record_added", "记录添加成功！"},
                {"msg_batch_add_success", "成功批量添加 {0} 条记录！"},
                {"msg_no_valid_json", "没有找到有效的JSON文档"},
                {"msg_csv_empty", "CSV文件为空"},
                {"msg_csv_format_error", "CSV文件格式错误：无法解析标题行"},
                {"msg_csv_no_data", "CSV文件中没有找到有效的数据行"},
                {"msg_csv_import_success", "成功导入 {0} 条记录！"},
                {"msg_close_all_tabs_confirm", "确定要关闭所有Tab吗？"},
                {"msg_close_database_confirm", "确定要关闭数据库 '{0}' 的连接吗？\n关闭后需要重新连接才能访问该数据库"},
                {"msg_cannot_delete_id_index", "不能删除默认的 _id 索引！"},
                {"msg_index_deleted", "索引删除成功！"},
                {"msg_index_created", "索引创建成功！"},
                {"msg_index_name_required", "索引名称和字段定义不能为空"},
                {"msg_record_modified", "修改成功！"},
                {"msg_record_copied", "复制成功！"},
                {"msg_record_deleted", "删除成功！"},
                {"msg_record_not_deleted", "未能删除记录"},
                {"msg_no_id_field", "无法获取记录的 _id 字段，无法修改"},
                {"msg_no_id_field_delete", "无法获取记录的 _id 字段，无法删除"},
                {"msg_please_select_record", "请先选择要修改的记录"},
                {"msg_please_select_record_copy", "请先选择要复制的记录"},
                {"msg_please_select_record_delete", "请先选择要删除的记录"},
                {"msg_delete_record_confirm_simple", "确定要删除该记录吗？"},
                {"msg_no_database_found", "在选择的目录中未找到数据库目录 '{0}'"},
                {"msg_no_import_files", "在数据库目录中未找到任何可导入的文件\n(.bson文件或非.metadata.json文件)"},
                {"msg_import_result", "导入结果"},
                {"msg_please_fill_required_fields", "请填写主机、端口和数据库名称"},
                {"msg_please_enter_host", "请输入主机地址"},
                {"msg_cannot_connect_to_host", "无法连接到 {0}:{1}，请检查：\n1. 网络连接是否正常\n2. 服务器地址和端口是否正确\n3. 防火墙设置"},

                {"msg_apply_language_failed", "应用语言到UI失败: {0}"},
                
                // 连接测试相关
                {"msg_connection_test_failed", "连接测试失败: {0}"},
                {"msg_connection_test_failed_ping_list", "连接测试失败: Ping失败({0}), 列出集合失败({1})"},
                {"msg_suggested_solutions", "建议解决方案："},
                {"msg_suggest_check_network", "1. 检查网络连接"},
                {"msg_suggest_check_server_address", "2. 确认MongoDB服务器地址正确"},
                {"msg_suggest_use_ip_address", "3. 尝试使用IP地址替代域名"},
                {"msg_suggest_check_dns", "4. 检查DNS设置"},
                {"msg_suggest_check_mongo_service", "1. 检查MongoDB服务是否运行"},
                {"msg_suggest_check_port", "2. 确认端口号正确"},
                {"msg_suggest_check_firewall", "3. 检查防火墙设置"},
                {"msg_suggest_check_network_connection", "4. 检查网络连接"},
                {"msg_suggest_check_username_password", "1. 检查用户名和密码"},
                {"msg_suggest_check_auth_mechanism", "2. 确认认证机制设置正确"},
                {"msg_suggest_check_user_permissions", "3. 检查用户权限"},
                {"msg_suggest_check_auth_database", "4. 确认认证数据库"},
                {"msg_suggest_check_mongo_status", "1. 检查MongoDB服务状态"},
                {"msg_suggest_check_port_open", "2. 确认端口是否开放"},
                {"msg_suggest_restart_mongo", "4. 尝试重启MongoDB服务"},
                
                // 状态消息

                {"status_network_failed", "网络连接失败"},
                {"status_network_ok_connecting_mongo", "网络连接正常，正在连接MongoDB..."},
                {"status_connection_success", "连接成功！"},
                {"status_connection_failed", "连接失败"},
                {"status_backing_up", "正在备份数据库..."},
                {"status_backup_completed", "备份完成！"},
                {"status_backup_failed", "备份失败：{0}"},
                {"status_restoring", "正在恢复数据库..."},
                {"status_restore_dir_not_found", "未找到restore目录：{0}"},
                {"status_restore_progress", "恢复进度：{0}/{1} ({2})"},
                {"status_restore_completed", "恢复完成！"},
                {"status_restore_failed", "恢复失败：{0}"},
                
                // 对话框标题
                {"dialog_connection_error", "连接错误"},
                {"dialog_network_error", "网络连接错误"},
                {"dialog_warning", "提示"},
                {"dialog_error", "错误"},
                
                // 语言设置相关
                {"text_language_description", "选择界面显示语言，更改后需要重启应用程序生效。"},
                
                // 错误消息
                {"error_network_connection", "网络连接失败，请检查网络设置"},
                {"error_mongodb_connection", "MongoDB连接失败，请检查服务器状态"},
                {"error_authentication", "认证失败，请检查用户名和密码"},
                {"error_permission_denied", "权限不足，请检查用户权限"},
                {"error_timeout", "连接超时，请检查网络和服务器状态"},
                {"error_invalid_connection_string", "连接字符串格式无效"},
                {"error_database_not_found", "数据库未找到"},
                {"error_collection_not_found", "集合未找到"},
                {"error_index_already_exists", "索引已存在"},
                {"error_invalid_index_definition", "索引定义无效"},
                
                // 提示信息
                {"tip_connection_string", "连接字符串格式：mongodb://[用户名:密码@]主机[:端口]/数据库"},
                {"tip_query_syntax", "查询语法：使用JSON格式，例如 {\"字段\": \"值\"}"},
                {"tip_filter_syntax", "过滤语法：使用JSON格式，支持正则表达式"},
                {"tip_sort_syntax", "排序语法：{\"字段\": 1} 升序，{\"字段\": -1} 降序"},
                {"tip_projection_syntax", "投影语法：{\"字段\": 1} 包含，{\"字段\": 0} 排除"},
                {"tip_index_creation", "索引创建可能需要一些时间，请耐心等待"},
                {"tip_backup_restore", "备份和恢复操作可能需要较长时间，请不要关闭应用程序"},
                {"tip_import_export", "导入/导出大量数据时，建议分批处理"},
                
                // 错误消息
                {"error_network_connection", "网络连接失败，请检查网络设置"},
                {"error_mongodb_connection", "MongoDB连接失败，请检查服务器状态"},
                {"error_authentication", "认证失败，请检查用户名和密码"},
                {"error_permission_denied", "权限不足，请检查用户权限"},
                {"error_timeout", "连接超时，请检查网络和服务器状态"},
                {"error_invalid_connection_string", "连接字符串格式错误"},
                {"error_database_not_found", "数据库不存在"},
                {"error_collection_not_found", "集合不存在"},
                {"error_index_already_exists", "索引已存在"},
                {"error_invalid_index_definition", "索引定义无效"},
                
                // 提示信息
                {"tip_connection_string", "连接字符串格式：mongodb://[username:password@]host[:port]/database"},
                {"tip_query_syntax", "查询语法：使用JSON格式，如 {\"field\": \"value\"}"},
                {"tip_filter_syntax", "过滤语法：使用JSON格式，支持正则表达式"},
                {"tip_sort_syntax", "排序语法：{\"field\": 1} 升序，{\"field\": -1} 降序"},
                {"tip_projection_syntax", "投影语法：{\"field\": 1} 包含，{\"field\": 0} 排除"},
                {"tip_index_creation", "索引创建可能需要一些时间，请耐心等待"},
                {"tip_backup_restore", "备份和恢复操作可能需要较长时间，请勿关闭程序"},
                {"tip_import_export", "导入导出大量数据时，建议分批处理"},
                
                // 占位符文本
                {"placeholder_filter", "输入过滤条件..."},
                {"placeholder_query", "输入查询语句..."},
                {"placeholder_sort", "输入排序条件..."},
                {"placeholder_projection", "输入投影条件..."},
                {"placeholder_index_name", "输入索引名称..."},
                {"placeholder_index_fields", "输入索引字段..."},
                {"placeholder_connection_name", "输入连接名称..."},
                {"placeholder_host", "localhost"},
                {"placeholder_port", "27017"},
                {"placeholder_database", "test"},
                {"placeholder_username", "用户名"},
                {"placeholder_password", "密码"},
                
                // 索引相关
                {"text_no_matching_collections", "无匹配集合"},
                {"text_no_indexes", "暂无索引"},
                {"text_index_key", "索引键"}
            };
        }

        // 获取默认繁体中文字符串
        private static Dictionary<string, string> GetDefaultChineseTraditionalStrings()
        {
            return new Dictionary<string, string>
            {
                // 主菜单
                {"menu_connection", "連接"},
                {"menu_connect", "連接配置"},
                {"menu_reconnect", "重新連接"},
                {"menu_refresh", "刷新"},
                {"menu_tools", "工具"},
                {"menu_native_query", "原生語句"},
                {"menu_language", "語言"},
                {"menu_english", "英語"},
                {"menu_simplified_chinese", "簡體中文"},
                {"menu_traditional_chinese", "繁體中文"},
                
                // 状态栏
                {"status_ready", "就緒"},
                {"status_connecting", "正在連接..."},
                {"status_connected", "已連接"},
                {"status_loading", "正在載入..."},
                {"status_importing", "正在匯入..."},
                {"status_exporting", "正在匯出..."},
                {"status_backing_up", "正在備份..."},
                {"status_restoring", "正在恢復..."},
                {"status_testing", "正在測試..."},
                
                // 对话框标题
                {"dialog_connect", "連接配置"},
                {"dialog_error", "錯誤"},
                {"dialog_warning", "警告"},
                {"dialog_info", "資訊"},
                {"dialog_confirm", "確認"},
                {"dialog_add_connection", "新增連接"},
                {"dialog_edit_connection", "編輯連接"},
                {"dialog_add_record", "新增記錄"},
                {"dialog_edit_record", "編輯記錄"},
                {"dialog_batch_add", "批次新增"},
                {"dialog_import_data", "匯入資料"},
                {"dialog_export_data", "匯出資料"},
                {"dialog_create_index", "建立索引"},
                {"dialog_backup_database", "備份資料庫"},
                {"dialog_restore_database", "恢復資料庫"},
                {"dialog_language_settings", "語言設定"},
                
                // 按钮文本
                {"btn_connect", "連接"},
                {"btn_cancel", "取消"},
                {"btn_ok", "確定"},
                {"btn_yes", "是"},
                {"btn_no", "否"},
                {"btn_close", "關閉"},
                {"btn_save", "儲存"},
                {"btn_load", "載入"},
                {"btn_add", "新增"},
                {"btn_edit", "編輯"},
                {"btn_delete", "刪除"},
                {"btn_copy", "複製"},
                {"btn_backup", "備份"},
                {"btn_restore", "恢復"},
                {"btn_test", "測試"},
                {"btn_open_compass", "開啟Compass"},
                {"btn_clear", "清空"},
                {"btn_export", "匯出"},
                {"btn_import", "匯入"},
                {"btn_execute", "執行"},
                {"btn_create", "建立"},
                {"btn_view", "檢視"},
                {"btn_refresh", "刷新"},
                {"btn_filter", "過濾"},
                {"btn_search", "搜尋"},
                
                // 窗体标题
                {"form_title", "MongoDB Compass"},
                {"form_connection_list", "連接列表"},
                {"form_native_query", "原生查詢"},
                {"form_index_manager", "索引管理"},
                {"form_data_viewer", "資料檢視器"},
                
                // 标签文本
                {"label_host", "主機："},
                {"label_port", "埠："},
                {"label_database", "資料庫："},
                {"label_username", "使用者名稱："},
                {"label_password", "密碼："},
                {"label_auth_mechanism", "認證機制："},
                {"label_auth_source", "認證源："},
                {"label_connection_name", "連接名稱："},
                {"label_status", "狀態："},
                {"label_language", "語言："},
                {"label_query", "查詢語句："},
                {"label_filter", "過濾條件："},
                {"label_limit", "限制數量："},
                {"label_skip", "跳過數量："},
                {"label_sort", "排序："},
                {"label_projection", "投影："},
                {"label_index_name", "索引名稱："},
                {"label_index_fields", "索引欄位："},
                {"label_index_type", "索引類型："},
                {"label_unique", "唯一索引"},
                {"label_sparse", "稀疏索引"},
                {"label_background", "背景建立"},
                
                // 其他文本
                {"text_database", "資料庫"},
                {"text_collection", "集合"},
                {"text_index", "索引"},
                {"text_query", "查詢"},
                {"text_execute", "執行查詢"},
                {"text_add_record", "新增記錄"},
                {"text_batch_add", "批次新增"},
                {"text_import_json", "匯入JSON"},
                {"text_import_csv", "匯入CSV"},
                {"text_edit_record", "修改記錄"},
                {"text_delete_record", "刪除記錄"},
                {"text_batch_delete", "批次刪除"},
                {"text_refresh_data", "重新整理資料"},
                {"text_export_json", "匯出JSON"},
                {"text_export_csv", "匯出CSV"},
                {"text_export_bson", "匯出BSON"},
                {"text_clear_collection", "清空集合"},
                {"text_copy_collection", "複製集合"},
                {"text_delete_collection", "刪除集合"},
                {"text_create_collection", "建立集合"},
                {"text_view_indexes", "檢視索引"},
                {"text_create_index", "建立索引"},
                {"text_delete_index", "刪除索引"},
                {"text_close_tab", "關閉標籤頁"},
                {"text_close_all_tabs", "關閉所有標籤頁"},
                {"text_copy_record", "複製記錄"},
                {"text_export_record", "匯出記錄"},
                {"text_import_record", "匯入記錄"},
                {"text_open_collection", "開啟集合"},
                {"text_export_collection", "匯出集合"},
                {"text_import_collection", "匯入集合"},
                {"text_import_directory", "匯入目錄資料"},
                {"text_backup_database", "備份資料庫"},
                {"text_close_database", "關閉資料庫連接"},
                {"text_refresh_indexes", "重新整理索引"},
                {"text_view_details", "檢視詳情"},
                {"text_import_bson", "匯入BSON"},
                
                // 测试相关


                {"label_host_address", "主機地址:"},
                
                // 原生查询相关
                {"text_native_query_execution", "原生MongoDB語句執行"},
                {"text_database_label", "資料庫:"},
                {"text_native_operation_statement", "原生操作語句 (如 db.collection.find({})):"},
                {"text_execute_button", "執行"},
                {"text_execution_result", "執行結果:"},
                {"text_please_select_database", "請選擇資料庫"},
                {"text_please_enter_native_statement", "請輸入原生MongoDB語句，如 db.collection.find({})"},
                {"text_only_support_db_collection_form", "暫僅支援 db.collection.method(args) 形式"},
                {"text_no_records", "無記錄"},
                {"text_operation_not_supported", "暫不支援該操作"},
                {"text_execution_failed", "執行失敗: "},
                
                // 索引创建相关
                {"text_index_name_label", "索引名稱:"},
                {"text_field_definition_json", "欄位定義 (JSON):"},
                {"text_index_options_json", "索引選項 (JSON):"},
                {"text_create_index_button", "建立索引"},
                
                // 导入确认相关
                {"text_import_data_confirmation", "匯入資料確認"},
                {"text_confirm_import", "確認匯入"},
                
                // 添加记录相关
                {"text_please_enter_json_document", "請輸入JSON格式的文件資料："},
                {"text_add_record_button", "新增記錄"},
                {"text_please_enter_json_array", "請輸入JSON陣列格式的多條文件資料："},
                {"text_batch_add_button", "批次新增"},
                
                // 编辑记录相关
                {"text_please_edit_json_format", "請編輯JSON（注意格式）:"},
                
                // 消息相关
                {"msg_please_select_record_to_edit", "請先選擇要修改的記錄。"},
                {"msg_cannot_get_record_id", "無法獲取記錄的 _id 欄位，無法修改。"},
                {"msg_edit_success", "修改成功！"},
                {"msg_edit_failed", "修改失敗: "},
                {"msg_please_select_record_to_copy", "請先選擇要複製的記錄。"},
                {"msg_copy_success", "複製成功！"},
                {"msg_copy_failed", "複製失敗: "},
                {"msg_please_select_record_to_delete", "請先選擇要刪除的記錄。"},
                {"msg_cannot_get_record_id_for_delete", "無法獲取記錄的 _id 欄位，無法刪除。"},
                {"msg_confirm_delete_record", "確定要刪除該記錄嗎？"},
                {"msg_delete_record_failed", "未能刪除記錄。"},
                {"msg_delete_record_error", "刪除失敗: "},
                {"msg_load_indexes_failed", "載入索引失敗: "},
                {"msg_view_index_details_failed", "檢視索引詳情失敗: "},
                {"msg_cannot_delete_default_id_index", "不能刪除預設的 _id 索引！"},
                {"msg_confirm_delete_index", "確定要刪除索引 '{0}' 嗎？\n此操作不可恢復！"},
                {"msg_index_delete_success", "索引刪除成功！"},
                {"msg_delete_index_failed", "刪除索引失敗: "},
                {"msg_index_name_and_fields_required", "索引名稱和欄位定義不能為空"},
                {"msg_index_create_success", "索引建立成功！"},
                {"msg_create_index_failed", "建立索引失敗: "},
                {"msg_invalid_collection_name", "集合名稱格式無效！集合名稱只能包含字母、數字、底線和連字符。"},
                {"msg_collection_already_exists", "集合 '{0}' 已存在！"},
                {"msg_collection_create_success", "集合建立成功！"},
                {"msg_create_collection_failed", "建立集合失敗: "},
                {"msg_confirm_close_database", "確定要關閉資料庫 '{0}' 的連接嗎？\n關閉後需要重新連接才能存取該資料庫。"},
                {"msg_database_connection_closed", "資料庫連接已關閉！"},
                {"msg_close_database_failed", "關閉資料庫連接失敗: "},
                {"msg_database_directory_not_found", "在選擇的目錄中未找到資料庫目錄 '{0}'"},
                {"msg_import_directory_failed", "匯入目錄資料失敗: "},
                {"msg_no_importable_files", "在資料庫目錄中未找到任何可匯入的檔案\n(.bson檔案或非.metadata.json檔案)"},
                {"msg_show_import_confirm_failed", "顯示匯入確認對話框失敗: "},
                {"msg_import_result", "匯入結果"},
                {"msg_import_data_failed", "匯入資料失敗: "},
                {"msg_auto_reconnect_failed", "自動重連失敗: "},
                {"msg_no_last_connection_params", "沒有找到上次連接的參數記錄"},
                {"msg_manual_reconnect_failed", "手動重連失敗: "},
                {"msg_no_collections_to_backup", "資料庫中沒有集合，無需備份。"},
                {"msg_backup_completed", "備份完成"},
                {"msg_backup_database_failed", "備份資料庫失敗: "},
                {"msg_please_enter_json_data", "請輸入JSON資料"},
                {"msg_add_record_success", "記錄新增成功！"},
                {"msg_add_record_failed", "新增記錄失敗: "},
                {"msg_show_add_dialog_failed", "顯示新增對話框失敗: "},
                {"msg_no_valid_json_documents", "沒有找到有效的JSON文件"},
                {"msg_batch_add_success", "成功批次新增 {0} 條記錄！"},
                {"msg_batch_add_failed", "批次新增記錄失敗: "},
                {"msg_show_batch_add_dialog_failed", "顯示批次新增對話框失敗: "},
                {"msg_show_import_dialog_failed", "顯示匯入對話框失敗: "},
                {"msg_csv_file_empty", "CSV檔案為空"},
                {"msg_csv_format_error", "CSV檔案格式錯誤：無法解析標題行"},
                {"msg_no_valid_csv_rows", "CSV檔案中沒有找到有效的資料行"},
                {"msg_import_csv_success", "成功匯入 {0} 條記錄！"},
                {"msg_import_csv_failed", "匯入CSV檔案失敗: "},
                {"msg_no_valid_json_in_file", "檔案中沒有找到有效的JSON文件"},
                {"msg_import_json_success", "成功匯入 {0} 條記錄！"},
                {"msg_import_file_failed", "匯入檔案失敗: "},
                {"msg_clear_collection_failed", "清空集合失敗: "},
                {"msg_copy_collection_failed", "複製集合失敗: "},
                {"msg_delete_collection_failed", "刪除集合失敗: "},
                {"msg_export_failed", "匯出失敗: "},
                {"msg_please_select_collection_to_import", "請先選擇要匯入的集合節點。"},
                {"msg_import_success", "匯入成功！"},
                {"msg_file_content_empty", "檔案內容為空。"},
                {"msg_import_failed", "匯入失敗: "},
                
                // 连接相关
                {"text_use_auth_source", "使用authSource"},
                {"text_default", "(預設)"},
                {"text_connections", "連接列表"},
                {"text_no_connections", "暫無連接"},
                {"text_connection_success", "連接成功"},
                {"text_connection_failed", "連接失敗"},
                {"text_backup_database", "備份資料庫"},
                {"text_restore_database", "恢復資料庫"},
                
                // 消息
                {"msg_connection_success", "連接成功！"},
                {"msg_connection_failed", "連接失敗：{0}"},

                {"msg_backup_success", "備份成功！"},
                {"msg_backup_failed", "備份失敗：{0}"},
                {"msg_restore_success", "恢復成功！"},
                {"msg_restore_failed", "恢復失敗：{0}"},
                {"msg_delete_confirm", "確定要刪除連接 '{0}' 嗎？"},
                {"msg_language_changed", "語言已切換到：{0}"},
                {"msg_delete_collection_confirm", "確定要刪除集合 '{0}' 嗎？"},
                {"msg_clear_collection_confirm", "確定要清空集合 '{0}' 嗎？"},
                {"msg_delete_record_confirm", "確定要刪除選中的記錄嗎？"},
                {"msg_batch_delete_confirm", "確定要刪除選中的 {0} 條記錄嗎？"},
                {"msg_delete_index_confirm", "確定要刪除索引 '{0}' 嗎？"},
                {"msg_operation_success", "操作成功！"},
                {"msg_operation_failed", "操作失敗：{0}"},
                {"msg_no_records_selected", "請先選擇要操作的記錄"},
                {"msg_invalid_query", "查詢語句格式錯誤"},
                {"msg_invalid_json", "JSON格式錯誤"},
                {"msg_file_not_found", "檔案未找到：{0}"},
                {"msg_import_success", "匯入成功，共匯入 {0} 條記錄"},
                {"msg_please_fill_required_fields", "請填寫主機、端口和資料庫名稱"},
                {"msg_please_enter_host", "請輸入主機地址"},
                {"msg_export_success", "匯出成功，共匯出 {0} 條記錄"},
                {"msg_auto_reconnect_prompt", "檢測到上次連接參數，是否自動重連？"},
                {"msg_auto_reconnect_yes", "是"},
                {"msg_auto_reconnect_no", "否"},
                {"msg_auto_reconnect_remember", "記住選擇"},
                
                // 语言设置相关
                {"text_language_description", "選擇介面顯示語言，更改後需要重新啟動應用程式生效。"},
                
                // 错误消息
                {"error_network_connection", "網路連接失敗，請檢查網路設定"},
                {"error_mongodb_connection", "MongoDB連接失敗，請檢查伺服器狀態"},
                {"error_authentication", "認證失敗，請檢查使用者名稱和密碼"},
                {"error_permission_denied", "權限不足，請檢查使用者權限"},
                {"error_timeout", "連接超時，請檢查網路和伺服器狀態"},
                {"error_invalid_connection_string", "連接字串格式錯誤"},
                {"error_database_not_found", "資料庫不存在"},
                {"error_collection_not_found", "集合不存在"},
                {"error_index_already_exists", "索引已存在"},
                {"error_invalid_index_definition", "索引定義無效"},
                
                // 提示信息
                {"tip_connection_string", "連接字串格式：mongodb://[username:password@]host[:port]/database"},
                {"tip_query_syntax", "查詢語法：使用JSON格式，如 {\"field\": \"value\"}"},
                {"tip_filter_syntax", "過濾語法：使用JSON格式，支援正則表達式"},
                {"tip_sort_syntax", "排序語法：{\"field\": 1} 升序，{\"field\": -1} 降序"},
                {"tip_projection_syntax", "投影語法：{\"field\": 1} 包含，{\"field\": 0} 排除"},
                {"tip_index_creation", "索引建立可能需要一些時間，請耐心等待"},
                {"tip_backup_restore", "備份和恢復操作可能需要較長時間，請勿關閉程式"},
                {"tip_import_export", "匯入匯出大量資料時，建議分批處理"},
                
                // 占位符文本
                {"placeholder_filter", "輸入過濾條件..."},
                {"placeholder_query", "輸入查詢語句..."},
                {"placeholder_sort", "輸入排序條件..."},
                {"placeholder_projection", "輸入投影條件..."},
                {"placeholder_index_name", "輸入索引名稱..."},
                {"placeholder_index_fields", "輸入索引欄位..."},
                {"placeholder_connection_name", "輸入連接名稱..."},
                {"placeholder_host", "localhost"},
                {"placeholder_port", "27017"},
                {"placeholder_database", "test"},
                {"placeholder_username", "使用者名稱"},
                {"placeholder_password", "密碼"},
                
                // 索引相關
                {"text_no_matching_collections", "無匹配集合"},
                {"text_no_indexes", "暫無索引"},
                {"text_index_key", "索引鍵"},
                
                // 新增的消息
                {"msg_cannot_connect_to_host", "無法連接到 {0}:{1}，請檢查：\n1. 網路連接是否正常\n2. 伺服器地址和端口是否正確\n3. 防火牆設定"},

                {"msg_apply_language_failed", "應用語言到UI失敗: {0}"},
                
                // 狀態消息

                {"status_network_failed", "網路連接失敗"},
                {"status_network_ok_connecting_mongo", "網路連接正常，正在連接MongoDB..."},
                {"status_connection_success", "連接成功！"},
                {"status_connection_failed", "連接失敗"},
                {"status_backing_up", "正在備份資料庫..."},
                {"status_backup_completed", "備份完成！"},
                {"status_backup_failed", "備份失敗：{0}"},
                {"status_restoring", "正在恢復資料庫..."},
                {"status_restore_dir_not_found", "未找到restore目錄：{0}"},
                {"status_restore_progress", "恢復進度：{0}/{1} ({2})"},
                {"status_restore_completed", "恢復完成！"},
                {"status_restore_failed", "恢復失敗：{0}"},
                
                // 對話框標題
                {"dialog_connection_error", "連接錯誤"},
                {"dialog_network_error", "網路連接錯誤"},
                {"dialog_warning", "提示"},
                {"dialog_error", "錯誤"},
                {"dialog_copy_connection", "複製連接"},
                
                // 連接測試相關
                {"msg_connection_test_failed", "連接測試失敗: {0}"},
                {"msg_connection_test_failed_ping_list", "連接測試失敗: Ping失敗({0}), 列出集合失敗({1})"},
                {"msg_suggested_solutions", "建議解決方案："},
                {"msg_suggest_check_network", "1. 檢查網路連接"},
                {"msg_suggest_check_server_address", "2. 確認MongoDB伺服器地址正確"},
                {"msg_suggest_use_ip_address", "3. 嘗試使用IP地址替代域名"},
                {"msg_suggest_check_dns", "4. 檢查DNS設定"},
                {"msg_suggest_check_mongo_service", "1. 檢查MongoDB服務是否運行"},
                {"msg_suggest_check_port", "2. 確認端口號正確"},
                {"msg_suggest_check_firewall", "3. 檢查防火牆設定"},
                {"msg_suggest_check_network_connection", "4. 檢查網路連接"},
                {"msg_suggest_check_username_password", "1. 檢查使用者名稱和密碼"},
                {"msg_suggest_check_auth_mechanism", "2. 確認認證機制設定正確"},
                {"msg_suggest_check_user_permissions", "3. 檢查使用者權限"},
                {"msg_suggest_check_auth_database", "4. 確認認證資料庫"},
                {"msg_suggest_check_mongo_status", "1. 檢查MongoDB服務狀態"},
                {"msg_suggest_check_port_open", "2. 確認端口是否開放"},
                {"msg_suggest_restart_mongo", "4. 嘗試重啟MongoDB服務"},
                
                // 狀態訊息
                {"status_testing_network", "正在測試網路連接..."},
                {"status_network_failed", "網路連接失敗"},
                {"status_network_ok_connecting_mongo", "網路連接正常，正在連接MongoDB..."},
                {"status_connection_success", "連接成功！"},
                {"status_connection_failed", "連接失敗"},
                {"status_backing_up", "正在備份資料庫..."},
                {"status_backup_completed", "備份完成！"},
                {"status_backup_failed", "備份失敗：{0}"},
                {"status_restoring", "正在恢復資料庫..."},
                {"status_restore_dir_not_found", "未找到restore目錄：{0}"},
                {"status_restore_progress", "恢復進度：{0}/{1} ({2})"},
                {"status_restore_completed", "恢復完成！"},
                {"status_restore_failed", "恢復失敗：{0}"}
            };
        }

        // 获取默认英文字符串
        private static Dictionary<string, string> GetDefaultEnglishStrings()
        {
            return new Dictionary<string, string>
            {
                // 主菜单
                {"menu_connection", "Connection"},
                {"menu_connect", "Connect"},
                {"menu_reconnect", "Reconnect"},
                {"menu_refresh", "Refresh"},
                {"menu_tools", "Tools"},
                {"menu_native_query", "Native Query"},
                {"menu_language", "Language"},
                {"menu_english", "English"},
                {"menu_simplified_chinese", "Simplified Chinese"},
                {"menu_traditional_chinese", "Traditional Chinese"},
                
                // 状态栏
                {"status_ready", "Ready"},
                {"status_connecting", "Connecting..."},
                {"status_connected", "Connected"},
                {"status_loading", "Loading..."},
                {"status_importing", "Importing..."},
                {"status_exporting", "Exporting..."},
                {"status_backing_up", "Backing up..."},
                {"status_restoring", "Restoring..."},
                {"status_testing", "Testing..."},
                
                // 对话框标题
                {"dialog_connect", "Connect"},
                {"dialog_error", "Error"},
                {"dialog_warning", "Warning"},
                {"dialog_info", "Information"},
                {"dialog_confirm", "Confirm"},
                {"dialog_add_connection", "Add Connection"},
                {"dialog_edit_connection", "Edit Connection"},
                {"dialog_add_record", "Add Record"},
                {"dialog_edit_record", "Edit Record"},
                {"dialog_batch_add", "Batch Add"},
                {"dialog_import_data", "Import Data"},
                {"dialog_export_data", "Export Data"},
                {"dialog_create_index", "Create Index"},
                {"dialog_backup_database", "Backup Database"},
                {"dialog_restore_database", "Restore Database"},
                {"dialog_language_settings", "Language Settings"},
                
                // 按钮文本
                {"btn_connect", "Connect"},
                {"btn_cancel", "Cancel"},
                {"btn_ok", "OK"},
                {"btn_yes", "Yes"},
                {"btn_no", "No"},
                {"btn_close", "Close"},
                {"btn_save", "Save"},
                {"btn_load", "Load"},
                {"btn_add", "Add"},
                {"btn_edit", "Edit"},
                {"btn_delete", "Delete"},
                {"btn_copy", "Copy"},
                {"btn_backup", "Backup"},
                {"btn_restore", "Restore"},
                {"btn_test", "Test"},
                {"btn_open_compass", "Open Compass"},
                {"btn_clear", "Clear"},
                {"btn_export", "Export"},
                {"btn_import", "Import"},
                {"btn_execute", "Execute"},
                {"btn_create", "Create"},
                {"btn_view", "View"},
                {"btn_refresh", "Refresh"},
                {"btn_filter", "Filter"},
                {"btn_search", "Search"},
                
                // 窗体标题
                {"form_title", "MongoDB Compass"},
                {"form_connection_list", "Connection List"},
                {"form_native_query", "Native Query"},
                {"form_index_manager", "Index Manager"},
                {"form_data_viewer", "Data Viewer"},
                
                // 标签文本
                {"label_host", "Host:"},
                {"label_port", "Port:"},
                {"label_database", "Database:"},
                {"label_username", "Username:"},
                {"label_password", "Password:"},
                {"label_auth_mechanism", "Auth Mechanism:"},
                {"label_auth_source", "Auth Source:"},
                {"label_connection_name", "Connection Name:"},
                {"label_status", "Status:"},
                {"label_language", "Language:"},
                {"label_query", "Query:"},
                {"label_filter", "Filter:"},
                {"label_limit", "Limit:"},
                {"label_skip", "Skip:"},
                {"label_sort", "Sort:"},
                {"label_projection", "Projection:"},
                {"label_index_name", "Index Name:"},
                {"label_index_fields", "Index Fields:"},
                {"label_index_type", "Index Type:"},
                {"label_unique", "Unique Index"},
                {"label_sparse", "Sparse Index"},
                {"label_background", "Background"},
                
                // 其他文本
                {"text_database", "Database"},
                {"text_collection", "Collection"},
                {"text_index", "Index"},
                {"text_query", "Query"},
                {"text_execute", "Execute Query"},
                {"text_add_record", "Add Record"},
                {"text_batch_add", "Batch Add"},
                {"text_import_json", "Import JSON"},
                {"text_import_csv", "Import CSV"},
                {"text_edit_record", "Edit Record"},
                {"text_delete_record", "Delete Record"},
                {"text_batch_delete", "Batch Delete"},
                {"text_refresh_data", "Refresh Data"},
                {"text_export_json", "Export JSON"},
                {"text_export_csv", "Export CSV"},
                {"text_export_bson", "Export BSON"},
                {"text_clear_collection", "Clear Collection"},
                {"text_copy_collection", "Copy Collection"},
                {"text_delete_collection", "Delete Collection"},
                {"text_create_collection", "Create Collection"},
                {"text_view_indexes", "View Indexes"},
                {"text_create_index", "Create Index"},
                {"text_delete_index", "Delete Index"},
                {"text_close_tab", "Close Tab"},
                {"text_close_all_tabs", "Close All Tabs"},
                {"text_copy_record", "Copy Record"},
                {"text_export_record", "Export Record"},
                {"text_import_record", "Import Record"},
                {"text_open_collection", "Open Collection"},
                {"text_export_collection", "Export Collection"},
                {"text_import_collection", "Import Collection"},
                {"text_import_directory", "Import Directory Data"},
                {"text_backup_database", "Backup Database"},
                {"text_close_database", "Close Database Connection"},
                {"text_refresh_indexes", "Refresh Indexes"},
                {"text_view_details", "View Details"},
                {"text_import_bson", "Import BSON"},
                

                {"label_host_address", "Host Address:"},
                
                // 原生查询相关
                {"text_native_query_execution", "Native MongoDB Statement Execution"},
                {"text_database_label", "Database:"},
                {"text_native_operation_statement", "Native operation statement (e.g. db.collection.find({})):"},
                {"text_execute_button", "Execute"},
                {"text_execution_result", "Execution result:"},
                {"text_please_select_database", "Please select a database"},
                {"text_please_enter_native_statement", "Please enter native MongoDB statement, e.g. db.collection.find({})"},
                {"text_only_support_db_collection_form", "Currently only supports db.collection.method(args) format"},
                {"text_no_records", "No records"},
                {"text_operation_not_supported", "Operation not supported yet"},
                {"text_execution_failed", "Execution failed: "},
                
                // 索引创建相关
                {"text_index_name_label", "Index name:"},
                {"text_field_definition_json", "Field definition (JSON):"},
                {"text_index_options_json", "Index options (JSON):"},
                {"text_create_index_button", "Create Index"},
                
                // 导入确认相关
                {"text_import_data_confirmation", "Import Data Confirmation"},
                {"text_confirm_import", "Confirm Import"},
                
                // 添加记录相关
                {"text_please_enter_json_document", "Please enter JSON format document data:"},
                {"text_add_record_button", "Add Record"},
                {"text_please_enter_json_array", "Please enter JSON array format for multiple documents:"},
                {"text_batch_add_button", "Batch Add"},
                
                // 编辑记录相关
                {"text_please_edit_json_format", "Please edit JSON (note format):"},
                
                // 消息相关
                {"msg_please_select_record_to_edit", "Please select a record to edit first."},
                {"msg_cannot_get_record_id", "Cannot get the _id field of the record, cannot edit."},
                {"msg_edit_success", "Edit successful!"},
                {"msg_edit_failed", "Edit failed: "},
                {"msg_please_select_record_to_copy", "Please select a record to copy first."},
                {"msg_copy_success", "Copy successful!"},
                {"msg_copy_failed", "Copy failed: "},
                {"msg_please_select_record_to_delete", "Please select a record to delete first."},
                {"msg_cannot_get_record_id_for_delete", "Cannot get the _id field of the record, cannot delete."},
                {"msg_confirm_delete_record", "Are you sure you want to delete this record?"},
                {"msg_delete_record_failed", "Failed to delete record."},
                {"msg_delete_record_error", "Delete failed: "},
                {"msg_load_indexes_failed", "Failed to load indexes: "},
                {"msg_view_index_details_failed", "Failed to view index details: "},
                {"msg_cannot_delete_default_id_index", "Cannot delete the default _id index!"},
                {"msg_confirm_delete_index", "Are you sure you want to delete index '{0}'?\nThis operation cannot be undone!"},
                {"msg_index_delete_success", "Index deleted successfully!"},
                {"msg_delete_index_failed", "Failed to delete index: "},
                {"msg_index_name_and_fields_required", "Index name and field definition cannot be empty"},
                {"msg_index_create_success", "Index created successfully!"},
                {"msg_create_index_failed", "Failed to create index: "},
                {"msg_invalid_collection_name", "Invalid collection name format! Collection names can only contain letters, numbers, underscores and hyphens."},
                {"msg_collection_already_exists", "Collection '{0}' already exists!"},
                {"msg_collection_create_success", "Collection created successfully!"},
                {"msg_create_collection_failed", "Failed to create collection: "},
                {"msg_confirm_close_database", "Are you sure you want to close the connection to database '{0}'?\nYou will need to reconnect to access this database after closing."},
                {"msg_database_connection_closed", "Database connection closed!"},
                {"msg_close_database_failed", "Failed to close database connection: "},
                {"msg_database_directory_not_found", "Database directory '{0}' not found in the selected directory"},
                {"msg_import_directory_failed", "Failed to import directory data: "},
                {"msg_no_importable_files", "No importable files found in the database directory\n(.bson files or non-.metadata.json files)"},
                {"msg_show_import_confirm_failed", "Failed to show import confirmation dialog: "},
                {"msg_import_result", "Import Result"},
                {"msg_import_data_failed", "Failed to import data: "},
                {"msg_auto_reconnect_failed", "Auto reconnect failed: "},
                {"msg_no_last_connection_params", "No last connection parameters found"},
                {"msg_manual_reconnect_failed", "Manual reconnect failed: "},
                {"msg_no_collections_to_backup", "No collections in database, no backup needed."},
                {"msg_backup_completed", "Backup Completed"},
                {"msg_backup_database_failed", "Failed to backup database: "},
                {"msg_please_enter_json_data", "Please enter JSON data"},
                {"msg_add_record_success", "Record added successfully!"},
                {"msg_add_record_failed", "Failed to add record: "},
                {"msg_show_add_dialog_failed", "Failed to show add dialog: "},
                {"msg_no_valid_json_documents", "No valid JSON documents found"},
                {"msg_batch_add_success", "Successfully batch added {0} records!"},
                {"msg_batch_add_failed", "Failed to batch add records: "},
                {"msg_show_batch_add_dialog_failed", "Failed to show batch add dialog: "},
                {"msg_show_import_dialog_failed", "Failed to show import dialog: "},
                {"msg_csv_file_empty", "CSV file is empty"},
                {"msg_csv_format_error", "CSV file format error: Cannot parse header row"},
                {"msg_no_valid_csv_rows", "No valid data rows found in CSV file"},
                {"msg_import_csv_success", "Successfully imported {0} records!"},
                {"msg_import_csv_failed", "Failed to import CSV file: "},
                {"msg_no_valid_json_in_file", "No valid JSON documents found in file"},
                {"msg_import_json_success", "Successfully imported {0} records!"},
                {"msg_import_file_failed", "Failed to import file: "},
                {"msg_clear_collection_failed", "Failed to clear collection: "},
                {"msg_copy_collection_failed", "Failed to copy collection: "},
                {"msg_delete_collection_failed", "Failed to delete collection: "},
                {"msg_export_failed", "Export failed: "},
                {"msg_please_select_collection_to_import", "Please select a collection node to import first."},
                {"msg_import_success", "Import successful!"},
                {"msg_file_content_empty", "File content is empty."},
                {"msg_import_failed", "Import failed: "},
                
                // 连接相关
                {"text_use_auth_source", "Use authSource"},
                {"text_default", "(Default)"},
                {"text_connections", "Connections"},
                {"text_no_connections", "No connections"},
                {"text_connection_success", "Connection successful"},
                {"text_connection_failed", "Connection failed"},

                {"text_backup_database", "Backup Database"},
                {"text_restore_database", "Restore Database"},

                
                // 消息
                {"msg_connection_success", "Connection successful!"},
                {"msg_connection_failed", "Connection failed: {0}"},

                {"msg_backup_success", "Backup successful!"},
                {"msg_backup_failed", "Backup failed: {0}"},
                {"msg_restore_success", "Restore successful!"},
                {"msg_restore_failed", "Restore failed: {0}"},
                {"msg_delete_confirm", "Are you sure you want to delete connection '{0}'?"},
                {"msg_language_changed", "Language changed to: {0}"},
                {"msg_delete_collection_confirm", "Are you sure you want to delete collection '{0}'?"},
                {"msg_clear_collection_confirm", "Are you sure you want to clear collection '{0}'?"},
                {"msg_delete_record_confirm", "Are you sure you want to delete the selected record?"},
                {"msg_batch_delete_confirm", "Are you sure you want to delete {0} selected records?"},
                {"msg_delete_index_confirm", "Are you sure you want to delete index '{0}'?"},
                {"msg_operation_success", "Operation successful!"},
                {"msg_operation_failed", "Operation failed: {0}"},
                {"msg_no_records_selected", "Please select records to operate on"},
                {"msg_invalid_query", "Invalid query format"},
                {"msg_invalid_json", "Invalid JSON format"},
                {"msg_file_not_found", "File not found: {0}"},
                {"msg_import_success", "Import successful, {0} records imported"},
                {"msg_export_success", "Export successful, {0} records exported"},
                {"msg_auto_reconnect_prompt", "Previous connection parameters detected. Auto-reconnect?"},
                {"msg_auto_reconnect_yes", "Yes"},
                {"msg_auto_reconnect_no", "No"},
                {"msg_auto_reconnect_remember", "Remember choice"},
                
                // 语言设置相关
                {"text_language_description", "Select interface display language. Changes will take effect after restarting the application."},
                
                // 错误消息
                {"error_network_connection", "Network connection failed. Please check network settings"},
                {"error_mongodb_connection", "MongoDB connection failed. Please check server status"},
                {"error_authentication", "Authentication failed. Please check username and password"},
                {"error_permission_denied", "Permission denied. Please check user privileges"},
                {"error_timeout", "Connection timeout. Please check network and server status"},
                {"error_invalid_connection_string", "Invalid connection string format"},
                {"error_database_not_found", "Database not found"},
                {"error_collection_not_found", "Collection not found"},
                {"error_index_already_exists", "Index already exists"},
                {"error_invalid_index_definition", "Invalid index definition"},
                
                // 提示信息
                {"tip_connection_string", "Connection string format: mongodb://[username:password@]host[:port]/database"},
                {"tip_query_syntax", "Query syntax: Use JSON format, e.g. {\"field\": \"value\"}"},
                {"tip_filter_syntax", "Filter syntax: Use JSON format, supports regular expressions"},
                {"tip_sort_syntax", "Sort syntax: {\"field\": 1} ascending, {\"field\": -1} descending"},
                {"tip_projection_syntax", "Projection syntax: {\"field\": 1} include, {\"field\": 0} exclude"},
                {"tip_index_creation", "Index creation may take some time, please be patient"},
                {"tip_backup_restore", "Backup and restore operations may take a long time, please don't close the application"},
                {"tip_import_export", "When importing/exporting large amounts of data, it's recommended to process in batches"},
                
                // 占位符文本
                {"placeholder_filter", "Enter filter conditions..."},
                {"placeholder_query", "Enter query statement..."},
                {"placeholder_sort", "Enter sort conditions..."},
                {"placeholder_projection", "Enter projection conditions..."},
                {"placeholder_index_name", "Enter index name..."},
                {"placeholder_index_fields", "Enter index fields..."},
                {"placeholder_connection_name", "Enter connection name..."},
                {"placeholder_host", "localhost"},
                {"placeholder_port", "27017"},
                {"placeholder_database", "test"},
                {"placeholder_username", "Username"},
                {"placeholder_password", "Password"},
                
                // Index related
                {"text_no_matching_collections", "No matching collections"},
                {"text_no_indexes", "No indexes"},
                {"text_index_key", "Index key"},
                
                // New messages
                {"msg_please_fill_required_fields", "Please fill in host, port and database name"},
                {"msg_please_enter_host", "Please enter host address"},
                {"msg_cannot_connect_to_host", "Cannot connect to {0}:{1}, please check:\n1. Network connection is normal\n2. Server address and port are correct\n3. Firewall settings"},

                {"msg_apply_language_failed", "Failed to apply language to UI: {0}"},
                
                // Status messages

                {"status_network_failed", "Network connection failed"},
                {"status_network_ok_connecting_mongo", "Network connection OK, connecting to MongoDB..."},
                {"status_connection_success", "Connection successful!"},
                {"status_connection_failed", "Connection failed"},
                {"status_backing_up", "Backing up database..."},
                {"status_backup_completed", "Backup completed!"},
                {"status_backup_failed", "Backup failed: {0}"},
                {"status_restoring", "Restoring database..."},
                {"status_restore_dir_not_found", "Restore directory not found: {0}"},
                {"status_restore_progress", "Restore progress: {0}/{1} ({2})"},
                {"status_restore_completed", "Restore completed!"},
                {"status_restore_failed", "Restore failed: {0}"},
                
                // Dialog titles
                {"dialog_connection_error", "Connection Error"},
                {"dialog_network_error", "Network Connection Error"},
                {"dialog_warning", "Warning"},
                {"dialog_error", "Error"},
                {"dialog_copy_connection", "Copy Connection"},
                
                // Connection test related
                {"msg_connection_test_failed", "Connection test failed: {0}"},
                {"msg_connection_test_failed_ping_list", "Connection test failed: Ping failed({0}), List collections failed({1})"},
                {"msg_suggested_solutions", "Suggested solutions:"},
                {"msg_suggest_check_network", "1. Check network connection"},
                {"msg_suggest_check_server_address", "2. Confirm MongoDB server address is correct"},
                {"msg_suggest_use_ip_address", "3. Try using IP address instead of domain name"},
                {"msg_suggest_check_dns", "4. Check DNS settings"},
                {"msg_suggest_check_mongo_service", "1. Check if MongoDB service is running"},
                {"msg_suggest_check_port", "2. Confirm port number is correct"},
                {"msg_suggest_check_firewall", "3. Check firewall settings"},
                {"msg_suggest_check_network_connection", "4. Check network connection"},
                {"msg_suggest_check_username_password", "1. Check username and password"},
                {"msg_suggest_check_auth_mechanism", "2. Confirm authentication mechanism settings are correct"},
                {"msg_suggest_check_user_permissions", "3. Check user permissions"},
                {"msg_suggest_check_auth_database", "4. Confirm authentication database"},
                {"msg_suggest_check_mongo_status", "1. Check MongoDB service status"},
                {"msg_suggest_check_port_open", "2. Confirm port is open"},
                {"msg_suggest_restart_mongo", "4. Try restarting MongoDB service"},
                
                // Status messages
                {"status_testing_network", "Testing network connection..."},
                {"status_network_failed", "Network connection failed"},
                {"status_network_ok_connecting_mongo", "Network connection OK, connecting to MongoDB..."},
                {"status_connection_success", "Connection successful!"},
                {"status_connection_failed", "Connection failed"},
                {"status_backing_up", "Backing up database..."},
                {"status_backup_completed", "Backup completed!"},
                {"status_backup_failed", "Backup failed: {0}"},
                {"status_restoring", "Restoring database..."},
                {"status_restore_dir_not_found", "Restore directory not found: {0}"},
                {"status_restore_progress", "Restore progress: {0}/{1} ({2})"},
                {"status_restore_completed", "Restore completed!"},
                {"status_restore_failed", "Restore failed: {0}"}
            };
        }

        // 加载语言文件
        public static void LoadLanguage(string languageCode)
        {
            try
            {
                string languageFile = Path.Combine(Application.StartupPath, "language", $"{languageCode}.ini");
                if (!File.Exists(languageFile))
                {
                    MessageBox.Show($"Language file not found: {languageFile}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                languageStrings.Clear();
                string[] lines = File.ReadAllLines(languageFile, Encoding.UTF8);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                    {
                        int equalIndex = trimmedLine.IndexOf('=');
                        if (equalIndex > 0)
                        {
                            string key = trimmedLine.Substring(0, equalIndex).Trim();
                            string value = trimmedLine.Substring(equalIndex + 1).Trim();
                            languageStrings[key] = value;
                        }
                    }
                }

                currentLanguage = languageCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load language file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 获取本地化字符串
        public static string GetString(string key, string defaultValue = "")
        {
            if (languageStrings.ContainsKey(key))
                return languageStrings[key];
            return defaultValue;
        }

        // 获取当前语言
        public static string GetCurrentLanguage()
        {
            return currentLanguage;
        }

        // 切换语言
        public static void SwitchLanguage(string languageCode)
        {
            try
            {
                LoadLanguage(languageCode);
                
                // 保存语言设置
                SaveLanguageSetting(languageCode);
                
                // 触发语言切换事件
                LanguageChanged?.Invoke(null, EventArgs.Empty);
                
                MessageBox.Show(string.Format(GetString("msg_language_changed", "Language changed to: {0}"), 
                    GetString($"menu_{languageCode}", languageCode)), 
                    GetString("dialog_info", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to switch language: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 获取语言显示名称
        public static string GetLanguageDisplayName(string languageCode)
        {
            if (SupportedLanguages.ContainsKey(languageCode))
                return SupportedLanguages[languageCode];
            return languageCode;
        }
        
        // 检查语言是否支持
        public static bool IsLanguageSupported(string languageCode)
        {
            return SupportedLanguages.ContainsKey(languageCode);
        }

        // 保存语言设置
        private static void SaveLanguageSetting(string languageCode)
        {
            try
            {
                string configFile = Path.Combine(Application.StartupPath, "language_config.ini");
                File.WriteAllText(configFile, $"Language={languageCode}", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // 静默处理，不影响主要功能
                Console.WriteLine($"Failed to save language setting: {ex.Message}");
            }
        }

        // 加载语言设置
        private static void LoadLanguageSetting()
        {
            try
            {
                string configFile = Path.Combine(Application.StartupPath, "language_config.ini");
                if (File.Exists(configFile))
                {
                    string content = File.ReadAllText(configFile, Encoding.UTF8);
                    string[] lines = content.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Language="))
                        {
                            string languageCode = line.Substring(9).Trim();
                            if (!string.IsNullOrEmpty(languageCode))
                            {
                                currentLanguage = languageCode;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 静默处理，使用默认语言
                Console.WriteLine($"Failed to load language setting: {ex.Message}");
            }
        }
    }
} 