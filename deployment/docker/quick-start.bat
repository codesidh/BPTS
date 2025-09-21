@echo off
REM Quick Start Script for BPTS Docker Deployment
REM This script provides a simple way to deploy the BPTS system

echo ========================================
echo BPTS Work Intake System - Quick Start
echo ========================================
echo.

REM Check if Docker is running
docker version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker is not running or not installed
    echo Please start Docker Desktop and try again
    pause
    exit /b 1
)

echo Docker is running...
echo.

REM Check if PowerShell is available
powershell -Command "Get-Host" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: PowerShell is not available
    pause
    exit /b 1
)

echo PowerShell is available...
echo.

REM Change to the project root directory
cd /d "%~dp0..\.."

echo Current directory: %CD%
echo.

REM Ask user for deployment type
echo Choose deployment type:
echo 1. Development (default)
echo 2. Production
echo 3. Development with monitoring
echo 4. Clean start (removes existing containers)
echo.
set /p choice="Enter your choice (1-4, default is 1): "

if "%choice%"=="" set choice=1

REM Set deployment parameters based on choice
set DEPLOY_PARAMS=-Environment Development
if "%choice%"=="2" set DEPLOY_PARAMS=-Environment Production
if "%choice%"=="3" set DEPLOY_PARAMS=-Environment Development -Monitoring
if "%choice%"=="4" set DEPLOY_PARAMS=-Environment Development -CleanStart -WaitForHealthy

echo.
echo Starting deployment with parameters: %DEPLOY_PARAMS%
echo.

REM Run the deployment script
powershell -ExecutionPolicy RemoteSigned -File "deployment\docker\Deploy-Docker.ps1" %DEPLOY_PARAMS%

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo Deployment completed successfully!
    echo ========================================
    echo.
    echo Your application is now running:
    echo   Web Application: http://localhost:3000
    echo   API Service: http://localhost:5000
    echo   Database: localhost:1433 ^(sa/YourStrong@Passw0rd^)
    echo   Redis: localhost:6379
    echo.
    echo Useful commands:
    echo   Health Check: deployment\docker\Health-Check.ps1
    echo   View Logs: docker-compose logs -f
    echo   Stop Services: docker-compose down
    echo.
) else (
    echo.
    echo ========================================
    echo Deployment failed!
    echo ========================================
    echo.
    echo Check the error messages above and try again.
    echo You can also run the deployment script directly:
    echo   powershell -File deployment\docker\Deploy-Docker.ps1
    echo.
)

pause
