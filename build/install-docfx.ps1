
$DocFxVersion = "2.48.0"

. $PSScriptRoot\install-nuget.ps1

$DocFxPath = Join-Path $ToolsPath "docfx.console.$DocFxVersion\tools\docfx.exe"
if (!(Test-Path $DocFxPath))
{
    Invoke-Expression "& `"$NugetPath`" install docfx.console -Version $DocFxVersion -OutputDirectory `"$ToolsPath`""
}
