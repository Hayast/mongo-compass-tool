# 导出MsSql和Mysql脚本功能说明

## 功能概述

在树控件的右键菜单中，为集合节点添加了两个新的导出选项：
- **导出MsSql脚本**：将MongoDB集合数据导出为Microsoft SQL Server脚本
- **导出Mysql脚本**：将MongoDB集合数据导出为MySQL脚本

## 功能特性

### 1. 智能表结构分析
- 自动分析MongoDB文档结构，确定合适的SQL数据类型
- 支持多种BSON数据类型的映射转换
- 智能处理混合数据类型，选择最通用的类型

### 2. 完整的SQL脚本生成
- **删除表语句**：检查并删除已存在的同名表
- **创建表语句**：根据文档结构自动生成CREATE TABLE语句
- **插入数据语句**：生成INSERT语句插入所有数据

### 3. 数据类型映射

#### MsSql数据类型映射
- `Int32` → `INT`
- `Int64` → `BIGINT`
- `Double` → `FLOAT`
- `Boolean` → `BIT`
- `DateTime` → `DATETIME2`
- `String/ObjectId` → `NVARCHAR(MAX)`

#### MySql数据类型映射
- `Int32` → `INT`
- `Int64` → `BIGINT`
- `Double` → `DOUBLE`
- `Boolean` → `BOOLEAN`
- `DateTime` → `DATETIME`
- `String/ObjectId` → `TEXT`

### 4. 数据值处理
- 自动转义特殊字符，确保SQL语句的正确性
- 正确处理NULL值
- 日期时间格式化
- 布尔值转换为0/1

## 使用方法

1. 在树控件中右键点击任意集合节点
2. 选择"导出集合"菜单
3. 选择"导出MsSql脚本"或"导出Mysql脚本"
4. 选择保存位置和文件名
5. 系统自动生成SQL脚本文件

## 生成的脚本示例

### MsSql脚本示例
```sql
-- 删除已存在的表
IF OBJECT_ID('users', 'U') IS NOT NULL
    DROP TABLE users;

-- 创建表
CREATE TABLE users (
    _id NVARCHAR(MAX),
    name NVARCHAR(MAX),
    age INT,
    email NVARCHAR(MAX),
    isActive BIT,
    createdAt DATETIME2
);

-- 插入数据
INSERT INTO users (_id, name, age, email, isActive, createdAt) VALUES (N'507f1f77bcf86cd799439011', N'John Doe', 30, N'john@example.com', 1, '2023-01-01 10:00:00');
```

### MySql脚本示例
```sql
-- 删除已存在的表
DROP TABLE IF EXISTS `users`;

-- 创建表
CREATE TABLE `users` (
    `_id` TEXT,
    `name` TEXT,
    `age` INT,
    `email` TEXT,
    `isActive` BOOLEAN,
    `createdAt` DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 插入数据
INSERT INTO `users` (`_id`, `name`, `age`, `email`, `isActive`, `createdAt`) VALUES ('507f1f77bcf86cd799439011', 'John Doe', 30, 'john@example.com', 1, '2023-01-01 10:00:00');
```

## 技术实现

### 核心方法
- `MenuItemExportMsSql_Click()`: MsSql导出事件处理
- `MenuItemExportMySql_Click()`: MySql导出事件处理
- `GenerateMsSqlScript()`: 生成MsSql脚本
- `GenerateMySqlScript()`: 生成MySql脚本
- `AnalyzeDocumentStructure()`: 分析文档结构
- `GetMoreGeneralType()`: 获取更通用的数据类型

### 语言支持
- 支持中文、英文、繁体中文三种语言
- 新增语言键值：
  - `text_export_mssql`: 导出MsSql脚本
  - `text_export_mysql`: 导出Mysql脚本

## 注意事项

1. **大数据量处理**：对于包含大量文档的集合，生成脚本可能需要较长时间
2. **数据类型兼容性**：某些MongoDB特有的数据类型（如ObjectId）会被转换为字符串
3. **字符编码**：生成的脚本使用UTF-8编码，确保中文字符正确显示
4. **SQL注入防护**：所有字符串值都经过适当的转义处理

## 更新内容

### 修改的文件
1. `CompassForm.cs` - 添加导出功能实现
2. `language/zhcn.ini` - 添加中文语言键值
3. `language/en.ini` - 添加英文语言键值
4. `language/zhtw.ini` - 添加繁体中文语言键值

### 新增功能
- 在导出菜单中添加分隔符和两个新选项
- 实现完整的SQL脚本生成逻辑
- 支持智能数据类型映射和转换
- 添加字符串转义和SQL注入防护 