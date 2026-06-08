@echo off
REM ============================================================
REM  PDF Label Sorter v2.4 – Windows Build Script
REM ============================================================

echo [1/4] Installing dependencies...
python.exe -m pip install --upgrade customtkinter pillow PyPDF2 pypdf reportlab pyinstaller

echo.
echo [2/4] Generating app icon...
python.exe generate_icon.py

echo.
echo [3/4] Building executable...
python.exe -m PyInstaller --clean --noconfirm PDF_Label_Sorter.spec

echo.
echo [4/5] Copying legacy processor folder...
set "LEGACY_SRC=%~dp0..\PDF Process Latest 3.0 2"
set "LEGACY_DST=%~dp0dist\PDF Process Latest 3.0 2"
if exist "%LEGACY_SRC%\lib\pdf-process-1.0.jar" (
    if not exist "%LEGACY_DST%" mkdir "%LEGACY_DST%" >nul 2>&1
    xcopy /E /I /Y "%LEGACY_SRC%" "%LEGACY_DST%" >nul
) else (
    echo WARNING: Legacy folder not found next to project. Skipping.
)

echo.
if exist "dist\PDF_Label_Sorter_Amazon_Temu_Etsy_V2.exe" (
    echo [5/5] SUCCESS!
    echo Output: dist\PDF_Label_Sorter_Amazon_Temu_Etsy_V2.exe
) else (
    echo [5/5] Build may have failed - check errors above.
)
pause
