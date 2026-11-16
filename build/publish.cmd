@echo off
REM ===========================================================================
REM  UniPad - single-file portable release build
REM
REM  Produces build\out\AnderFlowControle.exe: one self-contained executable with no
REM  installer and no .NET runtime prerequisite.
REM
REM  Trimming is deliberately OFF. ViGEm and SDL3 both reach native code
REM  through P/Invoke and the trimmer cannot see those paths; enable it only
REM  after re-testing every device and mapping scenario.
REM ===========================================================================

setlocal
cd /d "%~dp0.."

echo.
echo === Restoring ===
dotnet restore || goto :failed

echo.
echo === Publishing win-x64 single file ===
dotnet publish src\UniPad.App\UniPad.App.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebugType=none ^
    -o build\out || goto :failed

echo.
echo === Done ===
echo Output: %CD%\build\out\AnderFlowControle.exe
echo.
echo Na primeira execucao, instale o ViGEmBus para criar controles virtuais.
echo 
goto :eof

:failed
echo.
echo BUILD FAILED - see the messages above.
exit /b 1
