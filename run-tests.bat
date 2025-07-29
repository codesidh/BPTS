@echo off
echo ========================================
echo Work Intake System - Test Runner
echo ========================================
echo.

echo Starting authentication flow tests...
echo.

cd src\WorkIntakeSystem.Tests

echo Running Authentication Tests...
dotnet test --filter "FullyQualifiedName~AuthenticationTests" --verbosity normal
if %ERRORLEVEL% NEQ 0 (
    echo Authentication tests failed!
    pause
    exit /b 1
)

echo.
echo Running Integration Tests...
dotnet test --filter "FullyQualifiedName~IntegrationTests" --verbosity normal
if %ERRORLEVEL% NEQ 0 (
    echo Integration tests failed!
    pause
    exit /b 1
)

echo.
echo Running All Tests...
dotnet test --verbosity normal
if %ERRORLEVEL% NEQ 0 (
    echo Some tests failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo All tests completed successfully!
echo ========================================
echo.

echo Next steps:
echo 1. Configure email settings in appsettings.json
echo 2. Set up SSL certificate for production
echo 3. Configure user roles and permissions
echo 4. Test all protected endpoints
echo.

pause 