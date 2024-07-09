@echo off

if "%1" EQU "" (
	set TAG=edge
) else (
	set TAG=%1
)

if "%TAG%" EQU "production" (
	set BUILD=Release
) else (
	SET BUILD=Debug
)

if "%2" EQU "" (
	set BRANCH=public
) else (
	set BRANCH=%2
)

SET root=%cd%
SET server=%root%\server
SET steam=%root%\steam
SET url=https://github.com/CarbonCommunity/Carbon.Core/releases/download/%TAG%_build/Carbon.Windows.%BUILD%.zip	
SET steamCmd=https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip

echo Server directory: %server%
echo Steam directory: %steam%
echo Root directory: %root%
echo Branch: %BRANCH%

rem Ensure folders are created
if not exist "%server%" mkdir "%server%"
		
cd "%server%"
echo Staring server...		
RustDedicated.exe -nographics -batchmode -logs -silent-crashes ^
                  -server.hostname "Carbon Test Server" ^
                  -server.identity "main" ^
                  -server.port 29850 ^
                  -server.queryport 29851 ^
                  -server.saveinterval 400 ^
                  -server.maxplayers 1 ^
                  -chat.serverlog 1 ^
                  -global.asyncwarmup 1 ^
                  -global.skipassetwarmup_crashes 1 ^
		  -aimanager.nav_disable 1 ^
                  +server.seed 123123 ^
                  +server.worldsize 1500 ^
                  -logfile "main_log.txt" ^
			 
exit /b 0
