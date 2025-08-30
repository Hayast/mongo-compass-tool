# 贡献指南

感谢您对MongoDB Compass类似工具项目的关注！我们欢迎所有形式的贡献，包括但不限于：

- 🐛 Bug报告
- ✨ 新功能建议
- 📝 文档改进
- 🔧 代码优化
- 🌐 翻译贡献

## 📋 贡献前准备

### 开发环境要求
- **Visual Studio 2019** 或更高版本
- **.NET Framework 4.8 SDK**
- **MongoDB服务器**（用于测试）
- **Git** 版本控制工具

### 获取项目代码
1. Fork 本项目到您的GitHub账户
2. 克隆您的Fork到本地：
   ```bash
   git clone https://github.com/your-username/mongo-compass-tool.git
   cd mongo-compass-tool
   ```
3. 添加上游仓库：
   ```bash
   git remote add upstream https://github.com/original-owner/mongo-compass-tool.git
   ```

## 🔧 开发流程

### 1. 创建功能分支
```bash
# 确保主分支是最新的
git checkout main
git pull upstream main

# 创建新的功能分支
git checkout -b feature/your-feature-name
```

### 2. 开发规范

#### 代码风格
- 遵循C#编码规范
- 使用有意义的变量和函数名
- 添加适当的注释
- 保持代码简洁和可读性

#### 命名约定
- **类名**: PascalCase (如: `MongoConnInfo`)
- **方法名**: PascalCase (如: `ConnectToDatabase`)
- **变量名**: camelCase (如: `connectionString`)
- **常量名**: UPPER_CASE (如: `DEFAULT_PORT`)

#### 文件组织
- 每个类一个文件
- 文件名与类名一致
- 相关的类放在同一目录下

### 3. 提交规范

使用清晰的提交信息，格式如下：
```
<类型>(<范围>): <描述>

[可选的详细描述]

[可选的脚注]
```

#### 提交类型
- **feat**: 新功能
- **fix**: Bug修复
- **docs**: 文档更新
- **style**: 代码格式调整
- **refactor**: 代码重构
- **test**: 测试相关
- **chore**: 构建过程或辅助工具的变动

#### 示例
```
feat(connection): 添加连接池功能

- 实现连接池管理
- 优化连接性能
- 添加连接状态监控

Closes #123
```

### 4. 测试要求
- 确保代码能够正常编译
- 测试新功能是否按预期工作
- 确保不会破坏现有功能
- 添加单元测试（如果适用）

## 🐛 Bug报告

### 报告格式
使用GitHub Issues报告Bug，请包含以下信息：

1. **Bug描述**: 详细描述Bug的现象
2. **重现步骤**: 如何重现这个Bug
3. **预期行为**: 期望的正确行为
4. **实际行为**: 实际发生的错误行为
5. **环境信息**:
   - 操作系统版本
   - .NET Framework版本
   - MongoDB版本
   - 应用程序版本
6. **错误日志**: 如果有错误信息，请提供完整的错误日志
7. **截图**: 如果适用，请提供截图

### 示例Bug报告
```markdown
## Bug描述
在连接MongoDB时出现连接超时错误

## 重现步骤
1. 打开应用程序
2. 点击"连接配置"
3. 输入错误的连接字符串
4. 点击"连接"

## 预期行为
应该显示连接失败的错误信息

## 实际行为
应用程序无响应，没有错误提示

## 环境信息
- Windows 10 21H2
- .NET Framework 4.8
- MongoDB 5.0
- 应用程序版本: 1.0.0

## 错误日志
[在此粘贴错误日志]
```

## ✨ 功能建议

### 建议格式
1. **功能描述**: 详细描述新功能的需求
2. **使用场景**: 在什么情况下需要使用这个功能
3. **实现思路**: 如何实现这个功能（可选）
4. **优先级**: 高/中/低
5. **相关链接**: 如果有相关的参考资料

### 示例功能建议
```markdown
## 功能描述
添加数据导出为CSV格式的功能

## 使用场景
用户需要将查询结果导出为CSV文件，用于在Excel中进一步分析

## 实现思路
- 在查询结果界面添加"导出CSV"按钮
- 支持选择导出字段
- 处理特殊字符转义
- 支持大数据量分批导出

## 优先级
中

## 相关链接
- [CSV格式规范](https://tools.ietf.org/html/rfc4180)
```

## 🌐 翻译贡献

### 翻译文件位置
- 简体中文: `language/zhcn.ini`
- 繁体中文: `language/zhtw.ini`
- 英文: `language/en.ini`

### 翻译规范
1. 保持键名不变，只翻译值
2. 保持占位符格式（如 `{0}`, `{1}`）
3. 确保翻译的准确性和一致性
4. 测试翻译后的界面显示效果

### 添加新语言
1. 在 `LanguageManager.cs` 中添加新语言支持
2. 创建对应的语言文件（如 `ja.ini` 用于日语）
3. 实现默认字符串方法
4. 更新语言选择界面

## 📝 文档贡献

### 文档类型
- **README.md**: 项目介绍和使用说明
- **CHANGELOG.md**: 版本更新日志
- **API文档**: 代码注释和文档
- **用户指南**: 详细的使用教程

### 文档规范
- 使用清晰的标题结构
- 提供代码示例
- 包含截图说明（如果适用）
- 保持文档的时效性

## 🔄 Pull Request流程

### 1. 准备PR
```bash
# 确保代码是最新的
git pull upstream main

# 提交您的更改
git add .
git commit -m "feat: 添加新功能"

# 推送到您的Fork
git push origin feature/your-feature-name
```

### 2. 创建Pull Request
1. 在GitHub上创建Pull Request
2. 选择正确的目标分支（通常是 `main`）
3. 填写PR标题和描述
4. 添加相关的Issue链接

### 3. PR描述模板
```markdown
## 更改类型
- [ ] Bug修复
- [ ] 新功能
- [ ] 文档更新
- [ ] 代码重构
- [ ] 性能优化
- [ ] 其他

## 更改描述
详细描述您的更改内容

## 测试
- [ ] 已测试新功能
- [ ] 已测试现有功能
- [ ] 已更新文档

## 相关Issue
Closes #123

## 截图（如果适用）
[在此添加截图]
```

### 4. 代码审查
- 响应审查者的评论
- 根据反馈进行修改
- 保持PR的整洁性

## 🏷️ 版本发布

### 版本号规范
遵循[语义化版本](https://semver.org/lang/zh-CN/)规范：
- **主版本号**: 不兼容的API修改
- **次版本号**: 向下兼容的功能性新增
- **修订号**: 向下兼容的问题修正

### 发布流程
1. 更新 `CHANGELOG.md`
2. 更新版本号
3. 创建Git标签
4. 发布到GitHub Releases

## 📞 联系方式

如果您有任何问题或需要帮助：

- **GitHub Issues**: [项目Issues页面](https://github.com/your-username/mongo-compass-tool/issues)
- **GitHub Discussions**: [项目讨论页面](https://github.com/your-username/mongo-compass-tool/discussions)
- **邮箱**: your-email@example.com

## 🙏 致谢

感谢所有为这个项目做出贡献的开发者！您的贡献让这个项目变得更好。

---

**注意**: 通过提交Pull Request，您同意您的贡献将在MIT许可证下发布。 