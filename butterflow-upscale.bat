@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
set path=%1
set path2=%2
set path=!path:"=!
set path2=!path2:"=!
mkdir upscaled-butterflow
mkdir "upscaled-butterflow\%path2%"
mkdir temp
for /f "tokens=*" %%f in ('dir /b "!path!*.avi"') do (
  echo "upscaling !path!%%f to upscaled-butterflow\\!path2!%%f"
  "butterflow-0.2.3\butterflow.exe" -audio -r 4x -l --pyr-scale=0.6 --levels=5 --poly-n 7 --iters=5 --winsize=33 --poly-s=0.4 "!path!\%%f" -o "extractedtemp\\%%f-butterflow.mp4"
  ffmpeg -i "extractedtemp\\%%f-butterflow.mp4" -b:a 256k -b:v 40M -filter_complex "[0:v]deband='r=16:1thr=0.02:2thr=0.02:3thr=0.02:4thr=0.02', xbr=3, hqdn3d, unsharp[v];[0:a]acopy[a]" -map "[v]" -map "[a]" "upscaled-butterflow\\!path2!%%f"
  del /Q "extractedtemp\\%%f-butterflow.mp4"
  del /Q "!path!%%f"
)
