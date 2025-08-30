# GitHub上传指南

本指南将帮助您将MongoDB Compass类似工具项目上传到GitHub，并设置为开源项目。

## 📋 准备工作

### 1. 必需工具
- **Git**: 版本控制工具
- **GitHub账户**: 用于托管代码
- **GitHub CLI** (可选): 命令行工具，简化操作

### 2. 安装工具

#### 安装Git
1. 访问 [Git官网](https://git-scm.com/downloads)
2. 下载适合您系统的版本
3. 安装并配置用户信息：
   ```bash
   git config --global user.name "您的姓名"
   git config --global user.email "您的邮箱"
   ```

#### 安装GitHub CLI (推荐)
1. 访问 [GitHub CLI官网](https://cli.github.com/)
2. 下载并安装
3. 登录GitHub账户：
   ```bash
   gh auth login
   ```

## 🚀 快速上传 (推荐方式)

### 使用自动化脚本

我们提供了一个PowerShell脚本来自动化上传过程：

```powershell
# 在项目根目录运行
.\scripts\upload-to-github.ps1 -RepositoryName "mongo-compass-tool" -GitHubUsername "your-username"
```

#### 脚本参数说明
- `-RepositoryName`: GitHub仓库名称
- `-GitHubUsername`: 您的GitHub用户名
- `-Description`: 项目描述 (可选)
- `-Public`: 是否公开 (默认true)
- `-InitializeGit`: 是否初始化Git (默认true)

#### 示例
```powershell
# 创建公开仓库
.\scripts\upload-to-github.ps1 -RepositoryName "mongo-compass-tool" -GitHubUsername "your-username"

# 创建私有仓库
.\scripts\upload-to-github.ps1 -RepositoryName "mongo-compass-tool" -GitHubUsername "your-username" -Public:$false

# 自定义描述
.\scripts\upload-to-github.ps1 -RepositoryName "mongo-compass-tool" -GitHubUsername "your-username" -Description "我的MongoDB管理工具"
```

## 🔧 手动上传方式

如果您更喜欢手动操作，请按照以下步骤进行：

### 1. 初始化Git仓库
```bash
# 在项目根目录
git init
git add .
git commit -m "Initial commit: MongoDB Compass类似工具 v1.0.0"
```

### 2. 创建GitHub仓库
1. 访问 [GitHub新建仓库页面](https://github.com/new)
2. 填写仓库信息：
   - **Repository name**: `mongo-compass-tool`
   - **Description**: `一个功能强大的MongoDB数据库管理工具，基于.NET Framework开发的Windows Forms应用程序`
   - **Visibility**: 选择Public或Private
   - **不要**勾选"Add a README file"等选项
3. 点击"Create repository"

### 3. 连接并推送代码
```bash
# 添加远程仓库
git remote add origin https://github.com/your-username/mongo-compass-tool.git

# 设置主分支名称
git branch -M main

# 推送代码
git push -u origin main
```

### 4. 创建Release
1. 访问仓库的Releases页面
2. 点击"Create a new release"
3. 填写Release信息：
   - **Tag version**: `v1.0.0`
   - **Release title**: `MongoDB Compass类似工具 v1.0.0`
   - **Description**: 复制CHANGELOG.md中的内容
4. 点击"Publish release"

## 📝 项目配置

### 1. 更新README.md
上传成功后，需要更新README.md中的GitHub链接：

```markdown
# 将以下链接中的 "your-username" 替换为您的GitHub用户名
git clone https://github.com/your-username/mongo-compass-tool.git
```

### 2. 设置项目描述
1. 访问仓库设置页面
2. 在"Description"字段中添加项目描述
3. 添加相关标签 (Topics)

### 3. 配置GitHub Pages (可选)
如果需要项目网站：
1. 访问仓库设置
2. 找到"Pages"选项
3. 选择源分支 (通常是main)
4. 保存设置

## 🏷️ 项目徽章

### 添加徽章到README.md
在README.md顶部添加徽章，显示项目状态：

```markdown
<div align="center">

# MongoDB Compass 类似工具

[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/your-username/mongo-compass-tool/releases)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Build Status](https://github.com/your-username/mongo-compass-tool/workflows/Build%20and%20Release/badge.svg)](https://github.com/your-username/mongo-compass-tool/actions)

</div>
```

### 自定义徽章
使用 [Shields.io](https://shields.io/) 创建自定义徽章。

## 🔄 持续集成

### GitHub Actions
项目已包含GitHub Actions工作流文件 (`.github/workflows/build.yml`)，用于：
- 自动构建项目
- 运行测试
- 创建Release
- 上传发布文件

### 启用Actions
1. 推送代码后，Actions会自动启用
2. 访问Actions页面查看构建状态
3. 在仓库设置中配置Actions权限

## 📋 项目管理

### 1. Issues管理
- 创建Bug报告模板
- 设置功能请求模板
- 配置Issue标签

### 2. Projects管理
- 创建项目看板
- 设置里程碑
- 管理任务进度

### 3. Wiki (可选)
- 创建详细的使用文档
- 添加开发指南
- 维护FAQ

## 🔒 安全设置

### 1. 分支保护
1. 访问仓库设置
2. 找到"Branches"选项
3. 添加分支保护规则
4. 启用代码审查要求

### 2. 安全扫描
- 启用Dependabot安全更新
- 配置代码扫描
- 设置安全策略

## 📊 项目统计

### 1. 访问统计
- 查看仓库访问量
- 监控下载统计
- 分析用户行为

### 2. 贡献统计
- 查看贡献者列表
- 监控代码提交
- 分析项目活跃度

## 🆘 常见问题

### Q: 推送代码时出现权限错误
**A**: 检查GitHub账户认证，确保有仓库的写入权限。

### Q: Actions构建失败
**A**: 检查工作流文件配置，确保.NET Framework环境正确。

### Q: Release创建失败
**A**: 确保GitHub CLI已正确安装并登录。

### Q: 徽章显示不正确
**A**: 检查徽章URL中的用户名和仓库名是否正确。

## 📞 获取帮助

如果遇到问题，可以：
1. 查看GitHub文档
2. 在项目Issues中提问
3. 联系项目维护者

## 🎉 完成

恭喜！您的MongoDB Compass类似工具项目已成功上传到GitHub并设置为开源项目。

### 下一步建议
1. 分享项目链接给其他开发者
2. 邀请感兴趣的贡献者
3. 定期更新和维护项目
4. 收集用户反馈并改进功能

---

**提示**: 记得定期更新README.md和CHANGELOG.md，保持项目文档的时效性。 