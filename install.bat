@echo off
set "ONI_PATH=D:\SteamLibrary\steamapps\common\OxygenNotIncluded"
set /p "ONI_PATH=Enter Oxygen Not Included installation dir [%ONI_PATH%]"

set "TARGET=%ONI_PATH%\OxygenNotIncluded_Data\StreamingAssets"

xcopy StreamingAssets "%TARGET%" /i /s /e /y
