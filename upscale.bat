@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
mkdir upscaled
for /f "tokens=*" %%f in ('dir /b *.avi') do (
  echo "upscaling %%fupscaled\\%%f"
  start /B /low ffmpeg -i "%%f" -b:a 256k -b:v 20M -filter_complex "[0:v]deband='r=16:1thr=0.02:2thr=0.02:3thr=0.02:4thr=0.02', minterpolate='fps=60:mi_mode=mci:scd=none:vsbmc=1', xbr=4[v];[0:a]acopy[a]" -map "[v]" -map "[a]" "upscaled\\%%f"
)
pause
