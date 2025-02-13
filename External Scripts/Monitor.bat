@echo off
REM Get the application PID from the argument
set APP_PID=%1

REM Get the PID of the parent Command Prompt
for /f "tokens=2 delims=," %%a in ('tasklist /FI "IMAGENAME eq cmd.exe" /FO CSV /NH') do set CMD_PID=%%a

REM Monitor the parent CMD process
:Monitor
tasklist /FI "PID eq %CMD_PID%" | find "%CMD_PID%" >nul
if errorlevel 1 (
    echo Parent Command Prompt has closed. Terminating the application...
    taskkill /PID %APP_PID% /F
    goto End
)
timeout /t 1 >nul
goto Monitor

:End
echo Monitor script exiting...
exit