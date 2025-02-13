@echo off

set EXE_PATH=..\Builds\PixelArena.exe    
set SCRIPT_PATH=%~dp0
set RELATIVE_LOG_FILE_PATH=..\Builds\Logs\LogFile.txt
set GAMELIFT_PORT=7777
set GAMELIFT_WEBSOCKET_URL=wss://ap-south-1.api.amazongamelift.com
set GAMELIFT_PROCESS_ID=PID1234
set GAMELIFT_HOST_ID=AaronLaptop
set GAMELIFT_FLEET_ID=arn:aws:gamelift:ap-south-1:253490756336:fleet/fleet-730eed55-6afe-4d5c-a491-e260bed076d3
set GAMELIFT_AUTH_TOKEN=%1
set UNITY_SERVER_MODE = 1

echo port:%GAMELIFT_PORT%
echo webscoket url:%GAMELIFT_WEBSOCKET_URL%
echo process id:%GAMELIFT_PROCESS_ID%
echo host id:%GAMELIFT_HOST_ID%
echo fleet id:%GAMELIFT_FLEET_ID%
echo auth_token:%GAMELIFT_AUTH_TOKEN%

REM -----------------> Combines the Script directory and the Log directory
for %%F in ("%SCRIPT_PATH%%RELATIVE_LOG_FILE_PATH%") do set LOG_FILE=%%~fF

if not exist "%LOG_FILE%" (
    echo Log file not found : %LOG_FILE%
)

REM -----------------> Run the Unity executable with logging enabled

start "UnityApp" %EXE_PATH% -batchmode -logFile %RELATIVE_LOG_FILE_PATH% ^
-serverMode %UNITY_SERVER_MODE%^
-port %GAMELIFT_PORT% ^
-webSocket %GAMELIFT_WEBSOCKET_URL% ^
-processId %GAMELIFT_PROCESS_ID% ^
-hostId %GAMELIFT_HOST_ID% ^
-fleetId %GAMELIFT_FLEET_ID% ^
-authToken %GAMELIFT_AUTH_TOKEN%

for /f "tokens=2 delims=," %%A in ('tasklist /FI "IMAGENAME eq PixelArena.exe" /FO CSV /NH') do set APP_PID=%%~A

echo Started PixelArena.exe with PID: %APP_PID%

wt -w 0 nt cmd /k powershell -NoProfile -Command "Get-Content -Path '%LOG_FILE%' -Tail 10 -Wait"

REM -----------------> Register Input

:inputLoop
set /p userInput=Enter your Server input:
if /i "%userInput%"=="exit" goto end
if /i "%userInput%"=="createSession" goto createSession

goto inputLoop
1
:end
if defined APP_PID (
    taskkill /PID %APP_PID% /F
    pause
)