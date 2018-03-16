@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
mkdir upscaled
mkdir temp
for /f "tokens=*" %%f in ('dir /b *.avi') do (
  echo "upscaling %%fupscaled\\%%f"
  "..\..\Downloads\butterflow-0.2.3\butterflow.exe" -audio -l --poly-s=0.1 -r60 "%%f" -o "temp\\%%f-butterflow.mp4"
  ffmpeg -i "temp\\%%f-butterflow.mp4" -b:a 256k -b:v 20M -filter_complex "[0:v]xbr=4, unsharp[v];[0:a]acopy[a]" -map "[v]" -map "[a]" "upscaled\\butterflow-%%f"
  del "temp\\%%f-butterflow.mp4"
)
pause
