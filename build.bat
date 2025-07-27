@echo off
echo Building Work Intake System...

echo.
echo ===== Building Backend =====
cd src\WorkIntakeSystem.API
dotnet restore
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Backend build failed!
    pause
    exit /b 1
)

echo.
echo ===== Installing Frontend Dependencies =====
cd ..\WorkIntakeSystem.Web
call npm install
if %ERRORLEVEL% neq 0 (
    echo Frontend dependency installation failed!
    pause
    exit /b 1
)

echo.
echo ===== Building Frontend =====
call npm run build
if %ERRORLEVEL% neq 0 (
    echo Frontend build failed!
    pause
    exit /b 1
)

echo.
echo ===== Build Complete =====
echo.
echo To run the application:
echo 1. Start the API: cd src\WorkIntakeSystem.API && dotnet run
echo 2. Start the Frontend: cd src\WorkIntakeSystem.Web && npm run dev
echo.
pause