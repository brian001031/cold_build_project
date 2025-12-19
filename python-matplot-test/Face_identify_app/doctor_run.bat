@echo off
REM ============================================
REM Face_identify_app environment doctor
REM ============================================

SET PYTHON_EXE=C:\Users\bh205\AppData\Local\Programs\Python\Python311\python.exe

echo Using Python:
%PYTHON_EXE% -c "import sys; print(sys.executable)"
echo.

%PYTHON_EXE% doctor_imports.py

echo.
echo ============================================
echo Diagnosis finished
echo ============================================
pause