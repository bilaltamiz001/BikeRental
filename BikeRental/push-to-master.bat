@echo off
REM BikeRental Architecture Enhancement - Git Push Script (Batch Version)
REM This script automates the git push process

setlocal enabledelayedexpansion

set "REPO_PATH=C:\Source\BikeRentalWeb_dotnet48"
set "DRY_RUN=0"
set "SKIP_TESTS=0"

echo.
echo ================================
echo BikeRental Git Push Automation
echo ================================
echo.

REM Change to repository directory
cd /d "%REPO_PATH%" 2>nul
if errorlevel 1 (
	echo Error: Failed to change directory to %REPO_PATH%
	pause
	exit /b 1
)

echo [OK] Changed to repository: %REPO_PATH%
echo.

REM Verify it's a git repository
if not exist ".git" (
	echo Error: Not a git repository
	echo Please ensure you're in the correct directory
	pause
	exit /b 1
)

echo [OK] Confirmed git repository
echo.

REM Show current branch
echo Current Status:
for /f "tokens=*" %%i in ('git rev-parse --abbrev-ref HEAD') do set "BRANCH=%%i"
echo   Branch: %BRANCH%
echo.

REM Show changes
echo Files to be committed:
git status --porcelain
echo.

REM Confirm before proceeding
set /p CONFIRM="Press Y to continue or N to cancel (Y/N): "
if /i not "%CONFIRM%"=="Y" (
	echo Cancelled.
	pause
	exit /b 0
)

echo.
echo Staging changes...
git add .
if errorlevel 1 (
	echo Error: Failed to stage changes
	pause
	exit /b 1
)
echo [OK] Changes staged
echo.

REM Show staged files
echo Staged files:
git diff --cached --name-only
echo.

REM Commit changes
echo Committing changes...
git commit -m "feat: Implement comprehensive architecture enhancements - Add distributed caching (Redis/In-Memory) support - Implement async/await patterns throughout services and repositories - Add generic repository pattern to eliminate code duplication - Implement Unit of Work pattern for transaction coordination - Add API versioning support (v1.0 and v1.1) - Add request/response caching strategies with HTTP headers - Implement rate limiting middleware - Create async service layer with full scalability support"

if errorlevel 1 (
	echo Error: Failed to commit changes
	pause
	exit /b 1
)
echo [OK] Changes committed
echo.

REM Show commit info
echo Commit Information:
git log -1 --stat
echo.

REM Push to master
echo Pushing to remote master branch...
git push origin master

if errorlevel 1 (
	echo Error: Failed to push to remote
	pause
	exit /b 1
)
echo [OK] Successfully pushed to origin/master
echo.

REM Verify push
echo Verifying push...
git log origin/master -1 --oneline
echo.

echo ================================
echo [OK] Push completed successfully!
echo ================================
echo.
echo Next steps:
echo 1. Verify changes on GitHub
echo 2. Review the commit and changes
echo 3. Check CI/CD pipeline status
echo 4. Create release notes if needed
echo.
echo Repository: https://github.com/bilaltamiz001/BikeRental
echo.

pause
