@echo off
cls

echo Creating the module zip file..
powershell -command "Compress-Archive -Path .\Views\, .\module.config -DestinationPath .\Modules\_protected\EPiDebugViewLinks\EPiDebugViewLinks -CompressionLevel Optimal -Force"
echo.

echo Creating NuGet package..
echo.
nuget pack Swapcode.EPiDebugViewLinks.csproj -Build -Properties Configuration=Release -Verbosity detailed

echo.
echo.
echo So Long, and Thanks for All the Fish