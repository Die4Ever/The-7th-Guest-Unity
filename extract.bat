@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
for /f "tokens=*" %%f in ('dir /b ..\\*.gjd 2^>nul') do (
  SET rlfile=%%f
  SET rlfile=!rlfile:.gjd=.rl!
  echo "extracting %%f and !rlfile!"
  t7g-toolkit\T7GGrvEx\T7GGrvEx.exe "..\!rlfile!" "..\%%f"
)
pause
