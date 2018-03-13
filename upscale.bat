@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
mkdir upscaled
for /f "tokens=*" %%f in ('dir /b *.avi') do (
  echo "upscaling %%fupscaled\\%%f"
  ffmpeg -i "%%f" -b:a 256k -b:v 20M -filter_complex "[0:v]minterpolate='fps=60:mi_mode=mci:scd=none:vsbmc=1', xbr=4[v];[0:a]acopy[a]" -map "[v]" -map "[a]" "upscaled\\%%f"
)
pause
