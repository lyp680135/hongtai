@echo off
set PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\;C:\Windows\system32\

REM øΩ±¥≈‰÷√
mkdir tmp
echo f | xcopy ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter\ModelAccess\BaseAccess.cs tmp\BaseAccess.cs.bak /y
echo f | xcopy ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter\Common\CommonHouse.cs tmp\CommonHouseManual.cs.bak /y

echo f | xcopy CertBaseAccess.cs ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter\ModelAccess\BaseAccess.cs /y
echo f | xcopy CommonHouseManual.cs ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter\Common\CommonHouse.cs /y

MSBuild.exe ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter.sln /p:Configuration=Release

echo f | xcopy ..\WpfQualityCertPrinterManual\Setup\Setup\Express\SingleImage\DiskImages\DISK1\setup.exe cert_manual_setup.exe /y

REM ∏¥‘≠
echo f | xcopy tmp\BaseAccess.cs.bak ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter\ModelAccess\BaseAccess.cs /y
echo f | xcopy tmp\CommonHouseManual.cs.bak ..\WpfQualityCertPrinterManual\WpfQualityCertPrinter\Common\CommonHouse.cs /y
pause