@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
set path=%1
set path2=%2
set path=!path:"=!
set path2=!path2:"=!
mkdir upscaled
mkdir "upscaled\%path2%"
for /f "tokens=*" %%f in ('dir /b "!path!*.avi"') do (
  echo "upscaling !path!%%f to upscaled\\!path2!%%f"
  ffmpeg -i "!path!\%%f" -b:a 256k -b:v 40M -filter_complex "[0:v]deband='r=16:1thr=0.02:2thr=0.02:3thr=0.02:4thr=0.02', minterpolate='fps=30:mi_mode=mci:scd=none:vsbmc=1', xbr=3, unsharp[v];[0:a]acopy[a]" -map "[v]" -map "[a]" "upscaled\\!path2!%%f"
  del /Q "!path!%%f"
)
