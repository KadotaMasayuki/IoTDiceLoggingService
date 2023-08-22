@echo off
@rem install
@rem need administrator

@setlocal

set binpath="c:\IoTDice\bin\IotDiceLoggingService.exe"
set setpath="c:\IoTDice\bin\Settings.txt"

dir %binpath%
if errorlevel 1 goto notfound
goto install

:notfound
echo Place program to %binpath%
goto end

:install
sc  create  IoTDiceLoggingService  type= own  start= auto  error= normal  binpath= %binpath%  displayname= "IoT Dice Logging Service"  obj= "LocalSystem"

:end
pause
