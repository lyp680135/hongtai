@echo off
set PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\;C:\Windows\system32\

REM øΩ±¥≈‰÷√
mkdir tmp
echo f | xcopy ..\WpfQualityCertPrinter\WpfQualityCertPrinter\ModelAccess\BaseAccess.cs tmp\BaseAccess.cs.bak /y
echo f | xcopy ..\WpfQualityCertPrinter\WpfQualityCertPrinter\Common\CommonHouse.cs tmp\CommonHouse.cs.bak /y

echo f | xcopy CertBaseAccess.cs ..\WpfQualityCertPrinter\WpfQualityCertPrinter\ModelAccess\BaseAccess.cs /y
echo f | xcopy CommonHouse.cs ..\WpfQualityCertPrinter\WpfQualityCertPrinter\Common\CommonHouse.cs /y

MSBuild.exe ..\WpfQualityCertPrinter\WpfQualityCertPrinter.sln /p:Configuration=Release

echo f | xcopy ..\WpfQualityCertPrinter\Setup\Setup\Express\SingleImage\DiskImages\DISK1\setup.exe cert_setup.exe /y

REM ∏¥‘≠
echo f | xcopy tmp\BaseAccess.cs.bak ..\WpfQualityCertPrinter\WpfQualityCertPrinter\ModelAccess\BaseAccess.cs /y
echo f | xcopy tmp\CommonHouse.cs.bak ..\WpfQualityCertPrinter\WpfQualityCertPrinter\Common\CommonHouse.cs /y
pause