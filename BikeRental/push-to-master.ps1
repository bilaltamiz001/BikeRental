#!/usr/bin/env powershell
# BikeRental Architecture Enhancement - Git Push Script
# This script automatically stages, commits, and pushes all changes to the master branch

param(
	[string]$CommitMessage = "feat: Implement comprehensive architecture enhancements",
	[switch]$DryRun = $false,
	[switch]$SkipTests = $false
)

$ErrorActionPreference = "Stop"
$RepositoryPath = "C:\Source\BikeRentalWeb_dotnet48"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "BikeRental Git Push Automation" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Change to repository directory
try {
	Set-Location $RepositoryPath
	Write-Host "✓ Changed to repository: $RepositoryPath" -ForegroundColor Green
}
catch {
	Write-Host "✗ Failed to change directory to $RepositoryPath" -ForegroundColor Red
	exit 1
}

# Verify it's a git repository
if (-not (Test-Path ".git")) {
	Write-Host "✗ Not a git repository. Please ensure you're in the correct directory." -ForegroundColor Red
	exit 1
}

Write-Host "✓ Confirmed git repository" -ForegroundColor Green
Write-Host ""

# Show current branch
Write-Host "Current Status:" -ForegroundColor Yellow
$CurrentBranch = & git rev-parse --abbrev-ref HEAD
Write-Host "  Branch: $CurrentBranch"

# Run tests if not skipped
if (-not $SkipTests) {
	Write-Host ""
	Write-Host "Running tests..." -ForegroundColor Yellow
	& dotnet test --configuration Release --logger "console;verbosity=minimal"

	if ($LASTEXITCODE -ne 0) {
		Write-Host "✗ Tests failed. Aborting push." -ForegroundColor Red
		exit 1
	}
	Write-Host "✓ All tests passed" -ForegroundColor Green
}

# Show changes that will be committed
Write-Host ""
Write-Host "Files to be committed:" -ForegroundColor Yellow
& git status --porcelain

# Confirm if dry-run
if ($DryRun) {
	Write-Host ""
	Write-Host "DRY RUN MODE - No changes will be committed or pushed" -ForegroundColor Cyan
	Write-Host ""
	Write-Host "Would stage all changes" -ForegroundColor Yellow
	Write-Host "Would commit with message: $CommitMessage" -ForegroundColor Yellow
	Write-Host "Would push to origin master" -ForegroundColor Yellow
	exit 0
}

# Confirm before proceeding
Write-Host ""
Write-Host "Press Enter to continue or Ctrl+C to cancel..."
Read-Host

# Stage all changes
Write-Host ""
Write-Host "Staging changes..." -ForegroundColor Yellow
& git add .

if ($LASTEXITCODE -ne 0) {
	Write-Host "✗ Failed to stage changes" -ForegroundColor Red
	exit 1
}
Write-Host "✓ Changes staged" -ForegroundColor Green

# Show staged files
Write-Host ""
Write-Host "Staged files:" -ForegroundColor Yellow
& git diff --cached --name-only

# Commit changes
Write-Host ""
Write-Host "Committing changes..." -ForegroundColor Yellow

$FullCommitMessage = @"
$CommitMessage

- Add distributed caching (Redis/In-Memory) support
- Implement async/await patterns throughout services and repositories
- Add generic repository pattern to eliminate code duplication
- Implement Unit of Work pattern for transaction coordination
- Add API versioning support (v1.0 and v1.1)
- Add request/response caching strategies with HTTP headers
- Implement rate limiting middleware (1000 req/min per client)
- Create async service layer with full scalability support
- Add new v1.1 async API controllers
- Configure Redis integration with fallback to in-memory cache
- Add comprehensive documentation and testing guides
"@

& git commit -m $FullCommitMessage

if ($LASTEXITCODE -ne 0) {
	Write-Host "✗ Failed to commit changes" -ForegroundColor Red
	exit 1
}
Write-Host "✓ Changes committed" -ForegroundColor Green

# Show commit info
Write-Host ""
Write-Host "Commit Information:" -ForegroundColor Yellow
& git log -1 --stat

# Push to master
Write-Host ""
Write-Host "Pushing to remote master branch..." -ForegroundColor Yellow
& git push origin master

if ($LASTEXITCODE -ne 0) {
	Write-Host "✗ Failed to push to remote" -ForegroundColor Red
	exit 1
}
Write-Host "✓ Successfully pushed to origin/master" -ForegroundColor Green

# Verify push
Write-Host ""
Write-Host "Verifying push..." -ForegroundColor Yellow
& git log origin/master -1 --oneline

Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "✓ Push completed successfully!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Verify changes on GitHub: https://github.com/bilaltamiz001/BikeRental"
Write-Host "2. Review the commit and changes"
Write-Host "3. Check CI/CD pipeline status"
Write-Host "4. Create release notes if needed"
Write-Host "5. Update documentation"
Write-Host ""
Write-Host "Repository URL: https://github.com/bilaltamiz001/BikeRental"
Write-Host ""
