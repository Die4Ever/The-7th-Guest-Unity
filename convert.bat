@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
for /f "tokens=*" %%f in ('dir /b data\\*.vdx 2^>nul') do (
  echo "converting %%f"
  t7g-toolkit\VDXExt\VDXExt.exe /avi /sync "data\%%f"
)
pause
