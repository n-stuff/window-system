@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0build-doc.ps1""" %*"
