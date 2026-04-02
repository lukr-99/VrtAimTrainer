@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Switch-VrtAimTrainerVersion.ps1" %*
