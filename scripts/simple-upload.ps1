# MongoDB Compass Tool - Simple GitHub Upload Script
param(
    [Parameter(Mandatory=$true)]
    [string]$RepositoryName,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername,
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "MongoDB database management tool built with .NET Framework",
    
    [Parameter(Mandatory=$false)]
    [switch]$Public = $true
)

Write-Host "MongoDB Compass Tool - GitHub Upload Script" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# Check Git installation
try {
    $gitVersion = git --version
    Write-Host "Git installed: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "Git not installed. Please install Git first." -ForegroundColor Red
    Write-Host "Download: https://git-scm.com/downloads" -ForegroundColor Yellow
    exit 1
}

# Check current directory
$currentDir = Get-Location
Write-Host "Current directory: $currentDir" -ForegroundColor Cyan

# Check required files
$requiredFiles = @("mongo.sln", "README.md", "LICENSE", "mongo.csproj")
$missingFiles = @()

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "Found file: $file" -ForegroundColor Green
    } else {
        Write-Host "Missing file: $file" -ForegroundColor Red
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Host "Missing required files. Please run this script from project root directory." -ForegroundColor Red
    exit 1
}

# Initialize Git repository
Write-Host "`nInitializing Git repository..." -ForegroundColor Cyan

if (Test-Path ".git") {
    Write-Host "Git repository already exists" -ForegroundColor Yellow
} else {
    git init
    Write-Host "Git repository initialized" -ForegroundColor Green
}

# Add .gitignore file
if (Test-Path ".gitignore") {
    Write-Host ".gitignore file exists" -ForegroundColor Green
} else {
    Write-Host "Missing .gitignore file" -ForegroundColor Red
    exit 1
}

# Add all files
git add .
Write-Host "Files added to Git staging area" -ForegroundColor Green

# Initial commit
git commit -m "Initial commit: MongoDB Compass Tool v1.0.0

- Database connection management
- Data query and display
- Complete CRUD operations
- Data import/export
- Index management
- Multi-language support
- Advanced features"
Write-Host "Initial commit completed" -ForegroundColor Green

# Create GitHub repository
Write-Host "`nCreating GitHub repository..." -ForegroundColor Cyan

$repositoryUrl = "https://github.com/$GitHubUsername/$RepositoryName"

try {
    $visibility = if ($Public) { "public" } else { "private" }
    gh repo create $RepositoryName --description $Description --$visibility --source=. --remote=origin --push
    Write-Host "GitHub repository created successfully: $repositoryUrl" -ForegroundColor Green
} catch {
    Write-Host "Failed to create repository with GitHub CLI. Please create manually." -ForegroundColor Yellow
    Write-Host "Visit: https://github.com/new" -ForegroundColor Cyan
    Write-Host "Repository name: $RepositoryName" -ForegroundColor Cyan
    Write-Host "Description: $Description" -ForegroundColor Cyan
    Write-Host "Visibility: $(if ($Public) { 'Public' } else { 'Private' })" -ForegroundColor Cyan
    
    Write-Host "`nAfter creating repository, run these commands:" -ForegroundColor Yellow
    Write-Host "git remote add origin $repositoryUrl" -ForegroundColor White
    Write-Host "git branch -M main" -ForegroundColor White
    Write-Host "git push -u origin main" -ForegroundColor White
    
    $continue = Read-Host "`nContinue? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 0
    }
}

# Set remote repository
Write-Host "`nSetting up remote repository..." -ForegroundColor Cyan

try {
    git remote add origin $repositoryUrl
    Write-Host "Remote repository set up" -ForegroundColor Green
} catch {
    Write-Host "Remote repository may already exist" -ForegroundColor Yellow
}

# Push to GitHub
Write-Host "`nPushing to GitHub..." -ForegroundColor Cyan

try {
    git branch -M main
    git push -u origin main
    Write-Host "Code pushed successfully" -ForegroundColor Green
} catch {
    Write-Host "Push failed. Please check network connection and permissions." -ForegroundColor Red
    exit 1
}

# Create Release
Write-Host "`nCreating Release..." -ForegroundColor Cyan

$releaseTag = "v1.0.0"
$releaseTitle = "MongoDB Compass Tool v1.0.0"
$releaseNotes = @"
## Initial Release

### Main Features
- Database connection management
- Data query and display
- Complete CRUD operations
- Data import/export
- Index management
- Multi-language support
- Advanced features

### Technical Features
- Built with .NET Framework 4.8
- MongoDB.Driver for database operations
- Windows Forms modern interface
- Modular design architecture
- Event-driven interface updates
- Configuration persistence

### System Requirements
- Windows 7/8/10/11
- .NET Framework 4.8
- MongoDB server (local or remote)

### Download
Download the latest version and start using MongoDB Compass Tool!
"@

try {
    gh release create $releaseTag --title $releaseTitle --notes $releaseNotes
    Write-Host "Release created successfully" -ForegroundColor Green
} catch {
    Write-Host "Release creation failed. Please create manually." -ForegroundColor Yellow
    Write-Host "Visit: https://github.com/$GitHubUsername/$RepositoryName/releases/new" -ForegroundColor Cyan
}

# Display success information
Write-Host "`nProject upload completed!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "Project Information:" -ForegroundColor Cyan
Write-Host "   Repository URL: $repositoryUrl" -ForegroundColor White
Write-Host "   Project Name: $RepositoryName" -ForegroundColor White
Write-Host "   Description: $Description" -ForegroundColor White
Write-Host "   Visibility: $(if ($Public) { 'Public' } else { 'Private' })" -ForegroundColor White

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Visit project page: $repositoryUrl" -ForegroundColor White
Write-Host "2. Edit GitHub links in README.md" -ForegroundColor White
Write-Host "3. Set project description and tags" -ForegroundColor White
Write-Host "4. Invite collaborators (if needed)" -ForegroundColor White
Write-Host "5. Create Issues and Projects" -ForegroundColor White

Write-Host "`nUseful Links:" -ForegroundColor Cyan
Write-Host "   README Edit: $repositoryUrl/edit/main/README.md" -ForegroundColor White
Write-Host "   Settings: $repositoryUrl/settings" -ForegroundColor White
Write-Host "   Issues: $repositoryUrl/issues" -ForegroundColor White
Write-Host "   Releases: $repositoryUrl/releases" -ForegroundColor White

Write-Host "`nCongratulations! Your MongoDB Compass Tool project has been successfully uploaded to GitHub!" -ForegroundColor Green 