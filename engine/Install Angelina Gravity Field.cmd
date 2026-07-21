@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\install-dream-skin.ps1"
if errorlevel 1 pause
