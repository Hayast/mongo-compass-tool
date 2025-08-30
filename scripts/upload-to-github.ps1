# MongoDB Compass Tool - GitHub上传脚本
# 此脚本帮助您快速将项目上传到GitHub

param(
    [Parameter(Mandatory=$true)]
    [string]$RepositoryName,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername,
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "一个功能强大的MongoDB数据库管理工具，基于.NET Framework开发的Windows Forms应用程序",
    
    [Parameter(Mandatory=$false)]
    [switch]$Public = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$InitializeGit = $true
)

Write-Host "🚀 MongoDB Compass Tool - GitHub上传脚本" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# 检查Git是否安装
try {
    $gitVersion = git --version
    Write-Host "✅ Git已安装: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Git未安装，请先安装Git" -ForegroundColor Red
    Write-Host "下载地址: https://git-scm.com/downloads" -ForegroundColor Yellow
    exit 1
}

# 检查GitHub CLI是否安装
try {
    $ghVersion = gh --version
    Write-Host "✅ GitHub CLI已安装" -ForegroundColor Green
} catch {
    Write-Host "⚠️  GitHub CLI未安装，将使用手动方式创建仓库" -ForegroundColor Yellow
    Write-Host "建议安装GitHub CLI: https://cli.github.com/" -ForegroundColor Yellow
}

# 检查当前目录
$currentDir = Get-Location
Write-Host "📁 当前目录: $currentDir" -ForegroundColor Cyan

# 检查项目文件
$requiredFiles = @("mongo.sln", "README.md", "LICENSE", "mongo.csproj")
$missingFiles = @()

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✅ 找到文件: $file" -ForegroundColor Green
    } else {
        Write-Host "❌ 缺少文件: $file" -ForegroundColor Red
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Host "❌ 缺少必要文件，请确保在项目根目录运行此脚本" -ForegroundColor Red
    exit 1
}

# 初始化Git仓库
if ($InitializeGit) {
    Write-Host "`n🔧 初始化Git仓库..." -ForegroundColor Cyan
    
    if (Test-Path ".git") {
        Write-Host "⚠️  Git仓库已存在" -ForegroundColor Yellow
    } else {
        git init
        Write-Host "✅ Git仓库初始化完成" -ForegroundColor Green
    }
    
    # 添加.gitignore文件
    if (Test-Path ".gitignore") {
        Write-Host "✅ .gitignore文件已存在" -ForegroundColor Green
    } else {
        Write-Host "❌ 缺少.gitignore文件，请确保已创建" -ForegroundColor Red
        exit 1
    }
    
    # 添加所有文件
    git add .
    Write-Host "✅ 文件已添加到Git暂存区" -ForegroundColor Green
    
    # 初始提交
    git commit -m "Initial commit: MongoDB Compass类似工具 v1.0.0

🎉 初始版本发布
- 🔗 数据库连接管理
- 📊 数据查询和显示
- 🛠️ 完整的CRUD操作
- 📁 数据导入导出
- 🔍 索引管理
- 🌐 多语言支持
- 🔧 高级功能"
    
    Write-Host "✅ 初始提交完成" -ForegroundColor Green
}

# 创建GitHub仓库
Write-Host "`n🌐 创建GitHub仓库..." -ForegroundColor Cyan

$repositoryUrl = "https://github.com/$GitHubUsername/$RepositoryName"

try {
    # 尝试使用GitHub CLI创建仓库
    $visibility = if ($Public) { "public" } else { "private" }
    
    gh repo create $RepositoryName --description $Description --$visibility --source=. --remote=origin --push
    
    Write-Host "✅ GitHub仓库创建成功: $repositoryUrl" -ForegroundColor Green
} catch {
    Write-Host "⚠️  使用GitHub CLI创建仓库失败，请手动创建" -ForegroundColor Yellow
    Write-Host "请访问: https://github.com/new" -ForegroundColor Cyan
    Write-Host "仓库名称: $RepositoryName" -ForegroundColor Cyan
    Write-Host "描述: $Description" -ForegroundColor Cyan
    Write-Host "可见性: $(if ($Public) { 'Public' } else { 'Private' })" -ForegroundColor Cyan
    
    Write-Host "`n创建仓库后，请运行以下命令:" -ForegroundColor Yellow
    Write-Host "git remote add origin $repositoryUrl" -ForegroundColor White
    Write-Host "git branch -M main" -ForegroundColor White
    Write-Host "git push -u origin main" -ForegroundColor White
    
    $continue = Read-Host "`n是否继续？(y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 0
    }
}

# 设置远程仓库
Write-Host "`n🔗 设置远程仓库..." -ForegroundColor Cyan

try {
    git remote add origin $repositoryUrl
    Write-Host "✅ 远程仓库设置完成" -ForegroundColor Green
} catch {
    Write-Host "⚠️  远程仓库可能已存在" -ForegroundColor Yellow
}

# 推送到GitHub
Write-Host "`n📤 推送到GitHub..." -ForegroundColor Cyan

try {
    git branch -M main
    git push -u origin main
    Write-Host "✅ 代码推送成功" -ForegroundColor Green
} catch {
    Write-Host "❌ 推送失败，请检查网络连接和权限" -ForegroundColor Red
    exit 1
}

# 创建Release
Write-Host "`n🏷️  创建Release..." -ForegroundColor Cyan

$releaseTag = "v1.0.0"
$releaseTitle = "MongoDB Compass类似工具 v1.0.0"
$releaseNotes = @"
## 🎉 初始版本发布

### 🚀 主要功能
- 🔗 **数据库连接管理**: 支持多MongoDB连接配置，连接参数记忆，实时连接测试
- 📊 **数据查询和显示**: MongoDB查询语句实时执行，多标签页数据浏览
- 🛠️ **完整的CRUD操作**: 新增、修改、删除记录，支持批量操作
- 📁 **数据导入导出**: JSON文件导入，目录数据批量导入，数据库备份
- 🔍 **索引管理**: 索引查看、创建、删除，支持所有MongoDB索引类型
- 🌐 **多语言支持**: 简体中文、繁体中文、英文界面，运行时切换
- 🔧 **高级功能**: 数据库统计，连接状态监控，完善的错误处理

### 🛠️ 技术特性
- 基于.NET Framework 4.8
- 使用MongoDB.Driver进行数据库操作
- Windows Forms现代化界面
- 模块化设计架构
- 事件驱动界面更新
- 配置持久化存储

### 📋 系统要求
- Windows 7/8/10/11
- .NET Framework 4.8
- MongoDB服务器（本地或远程）

### 📥 下载
下载最新版本并开始使用MongoDB Compass类似工具！
"@

try {
    gh release create $releaseTag --title $releaseTitle --notes $releaseNotes
    Write-Host "✅ Release创建成功" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Release创建失败，请手动创建" -ForegroundColor Yellow
    Write-Host "访问: https://github.com/$GitHubUsername/$RepositoryName/releases/new" -ForegroundColor Cyan
}

# 显示成功信息
Write-Host "`n🎉 项目上传完成！" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "📋 项目信息:" -ForegroundColor Cyan
Write-Host "   仓库地址: $repositoryUrl" -ForegroundColor White
Write-Host "   项目名称: $RepositoryName" -ForegroundColor White
Write-Host "   描述: $Description" -ForegroundColor White
Write-Host "   可见性: $(if ($Public) { 'Public' } else { 'Private' })" -ForegroundColor White

Write-Host "`n📝 下一步操作:" -ForegroundColor Cyan
Write-Host "1. 访问项目页面: $repositoryUrl" -ForegroundColor White
Write-Host "2. 编辑README.md中的GitHub链接" -ForegroundColor White
Write-Host "3. 设置项目描述和标签" -ForegroundColor White
Write-Host "4. 邀请协作者（如果需要）" -ForegroundColor White
Write-Host "5. 创建Issues和Projects" -ForegroundColor White

Write-Host "`n🔗 有用的链接:" -ForegroundColor Cyan
Write-Host "   README编辑: $repositoryUrl/edit/main/README.md" -ForegroundColor White
Write-Host "   设置页面: $repositoryUrl/settings" -ForegroundColor White
Write-Host "   Issues页面: $repositoryUrl/issues" -ForegroundColor White
Write-Host "   Releases页面: $repositoryUrl/releases" -ForegroundColor White

Write-Host "`n✅ 恭喜！您的MongoDB Compass类似工具项目已成功上传到GitHub！" -ForegroundColor Green 