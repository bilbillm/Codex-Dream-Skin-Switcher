@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start-dream-skin.ps1" -PromptRestart
if errorlevel 1 pause
