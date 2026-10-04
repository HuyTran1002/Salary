@echo off
title Salary Calculator - Mobile Web Simulator
echo ======================================================
echo    SALARY CALCULATOR - TRINH GIA LAP DIEN THOAI WEB
echo ======================================================
echo.
echo Dang khoi dong Local Web Server tai cong 5050...
start http://localhost:5050/simulator.html
node server.js
pause
