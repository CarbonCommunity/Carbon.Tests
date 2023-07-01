@echo off

if "%1" EQU "" (
	set TAG=preview
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

rem Download latest development build of Carbon
echo Downloading Carbon from the '%TAG%' tag for %BUILD% build
powershell -Command "(New-Object Net.WebClient).DownloadFile('%url%', '%root%\carbon.zip')"

rem Extract it in the server folder
cd %server%
echo Extracting Carbon
powershell -Command "Expand-Archive '%root%\carbon.zip' -DestinationPath '%server%'" -Force

rem Download & extract Steam it in the steam folder
if not exist "%steam%" (
	mkdir "%steam%"
	cd "%steam%"
	
	echo Downloading Steam
	powershell -Command "(New-Object Net.WebClient).DownloadFile('%steamCmd%', '%root%\steam.zip')"
	echo Extracting Steam
	powershell -Command "Expand-Archive '%root%\steam.zip' -DestinationPath '%steam%'" -Force

	del "%root%\steam.zip"
)

rem Cleanup
del "%root%\carbon.zip"

rem Download the server
cd "%steam%"
echo Downloading Rust server on %BRANCH% branch...
steamcmd.exe +force_install_dir "%server%" ^
			 +login anonymous ^
             +app_update 258550 ^
			 -beta %BRANCH% ^
             validate ^
             +quit ^
		
cd "%server%"
echo Staring server...		
RustDedicated.exe -nographics -batchmode -logs -silent-crashes ^
                  -server.hostname "Legit Server" ^
                  -server.identity "main" ^
                  -server.port 29850 ^
                  -server.queryport 29851 ^
                  -server.saveinterval 400 ^
                  -server.maxplayers 1 ^
                  -chat.serverlog 1 ^
                  -global.asyncwarmup t ^
                  +server.seed 123123 ^
                  +server.worldsize 3000 ^
                  -logfile "main_log.txt" ^
			 
exit /b 0