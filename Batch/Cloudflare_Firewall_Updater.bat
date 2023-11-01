@echo off
setlocal enabledelayedexpansion

REM Check if Curl is installed
curl --version >NUL 2>&1
if %errorlevel%==0 (
    echo Curl is already installed.
) else (
    echo Curl is not installed. Installing...

    REM Download the latest Curl executable for Windows
    set "curlURL=https://curl.se/windows/dl-7.86.0/curl-7.86.0-win64-mingw.zip"
    set "curlDir=%TEMP%\curl"
    set "curlZip=%TEMP%\curl.zip"

    REM Create a temporary directory for Curl
    mkdir "%curlDir%"

    REM Download Curl
    powershell -command "(New-Object System.Net.WebClient).DownloadFile('%curlURL%', '%curlZip%')"

    REM Extract Curl from the zip file
    powershell -command "Expand-Archive -Path '%curlZip%' -DestinationPath '%curlDir%'"

    REM Add Curl to the system PATH
    setx PATH "%curlDir%\bin;%PATH%" /M

    echo Curl is now installed.
)

REM Clean up temporary files
if exist "%curlZip%" del "%curlZip%"
if exist "%curlDir%" rmdir /s /q "%curlDir%"

REM Check if save file exists
set "saveFile=%APPDATA%\MagnusLund\CloudFlareFirewall\ip.txt"

if not exist "%saveFile%" (
    REM Create the directory if it doesn't exist
    mkdir "%APPDATA%\MagnusLund\CloudFlareFirewall"
    
    REM Create the file
    echo.>"%saveFile%"
)


echo.>"Getting current public IP address"
REM Get current public ip address
for /f %%a in ('curl -s https://icanhazip.com') do (
    set "new_ip=%%a"
)

echo Your public IP address is: !new_ip!

REM Check if the file is empty
for %%A in ("%saveFile%") do set "fileSize=%%~zA"
if %fileSize%==0 (
    echo The file is empty. Press any key to continue... Write your current public ip address inside "%APPDATA%\MagnusLund\CloudFlareFirewall"
    pause > nul
) else (
    REM Read the file and store its content in the "old_ip" variable
    set /p "old_ip=" < "%saveFile%"
)

REM Display the old IP address
echo The old IP address is: %old_ip%

REM Overwrite ip address in the save file
echo !new_ip!>"%saveFile%"

endlocal

pause