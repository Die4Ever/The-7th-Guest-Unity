@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
SET t7gpath=D:\steam games\steamapps\common\The 7th Guest\
rmdir /Q /S extractedtemp
rmdir /Q /S upscaled
REM compact /c /i /s
REM compact /c /i /s "!t7Gpath!"
REM compact /c /i /s "!t7Gpath!*"
mkdir extractedtemp
compact /C /I /S extractedtemp
echo "t7gpath == !t7gpath!*.gjd"
dir /b "!t7gpath!*.gjd"

for /f "tokens=*" %%f in ('dir /b "!t7gpath!DR.gjd" 2^>nul') do (
  SET rlfile=%%f
  SET rlfile=!rlfile:.gjd=.rl!
  SET outpath=!rlfile:.rl=!\
  echo "extracting %%f and !rlfile! to extractedtemp\!outpath!"
  mkdir "extractedtemp\!outpath!"
  t7g-toolkit\T7GGrvEx\T7GGrvEx.exe "!t7gpath!!rlfile!" "!t7gpath!%%f"
  move /Y *.vdx "extractedtemp\!outpath!"
  call convert.bat "extractedtemp\!outpath!"
  start /B /low call upscale15fps.bat "extractedtemp\!outpath!" "!outpath!"
  REM pause
)

mkdir wavs
move *.wav wavs\
mkdir xmis
move *.xmi xmis\
mkdir oggs
copy "!t7gpath!*.ogg" "oggs\"
mkdir midis
copy "!t7gpath!Bonus Content\Soundtrack\The 7th Guest MIDIs\*" "midis\"
pause
