@echo off
echo Starting Work Intake System in Development Mode...

echo.
echo Starting API server...
start "Work Intake API" cmd /k "cd src\WorkIntakeSystem.API && dotnet run"

echo.
echo Waiting for API to start...
timeout /t 5 /nobreak > nul

echo.
echo Starting Frontend development server...
start "Work Intake Frontend" cmd /k "cd src\WorkIntakeSystem.Web && npm run dev"

echo.
echo Both servers are starting...
echo API: https://localhost:7000
echo Frontend: http://localhost:3000
echo.
echo Press any key to close this window (servers will continue running)
pause > nul