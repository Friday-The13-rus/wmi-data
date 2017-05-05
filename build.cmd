@ECHO OFF

set PATH=%windir%\Microsoft.net\Framework\v4.0.30319;%PATH%

cd %~dp0

msbuild build.proj /tv:4.0
pause
