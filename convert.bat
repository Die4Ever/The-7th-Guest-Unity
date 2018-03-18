@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
t7g-toolkit\VDXExt\VDXExt.exe "%1*.vdx" /avi /sync
del /Q "%1*.vdx"