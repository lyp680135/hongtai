@echo off
set PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\;C:\Windows\system32\

REM øΩ±¥≈‰÷√
echo f | xcopy ..\WpfCardPrinter\ModelAccess\BaseAccess.cs BaseAccess.cs.bak /y
echo f | xcopy BaseAccess.cs ..\WpfCardPrinter\ModelAccess\BaseAccess.cs /y
MSBuild.exe ../WpfCardPrinter/WpfCardPrinter.sln /p:Configuration=Release

echo f | xcopy ..\Setup\Setup\Express\SingleImage\DiskImages\DISK1\setup.exe card_setup.exe /y

REM ∏¥‘≠
echo f | xcopy BaseAccess.cs.bak ..\WpfCardPrinter\ModelAccess\BaseAccess.cs /y
pause