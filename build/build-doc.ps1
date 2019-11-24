
. $PSScriptRoot\install-docfx.ps1

$RepositoryPath = Split-Path $PSScriptRoot -Parent
$DocFxJsonPath = Join-Path $RepositoryPath "doc\docfx.json"

Invoke-Expression "& `"$DocFxPath`" `"$DocFxJsonPath`" "
