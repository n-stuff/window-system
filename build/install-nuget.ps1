
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

$ToolsPath = Join-Path $PSScriptRoot "tools"
if (!(Test-Path $ToolsPath))
{
    New-Item -Path $ToolsPath -Type directory
}


$NugetPath = Join-Path $ToolsPath "nuget.exe"
if (!(Test-Path $NugetPath))
{
    (New-Object System.Net.WebClient).DownloadFile($NugetUrl, $NugetPath)
}
