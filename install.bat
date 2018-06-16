@echo off
set "ONI_PATH=D:\SteamLibrary\steamapps\common\OxygenNotIncluded"
set /p "ONI_PATH=Enter Oxygen Not Included installation dir [%ONI_PATH%]"

xcopy StreamingAssets "%ONI_PATH%\OxygenNotIncluded_Data\StreamingAssets" /i /s /e /y
type NUL >> "%ONI_PATH%\OxygenNotIncluded_Data\debug_enable.txt"
