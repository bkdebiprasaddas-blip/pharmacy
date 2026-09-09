[CmdletBinding()]
param([string]$SitePath = (Join-Path $PSScriptRoot '..\OnlineGroceryStore'))
$ErrorActionPreference = 'Stop'
$SitePath = [IO.Path]::GetFullPath($SitePath)
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = Join-Path $framework 'aspnet_compiler.exe'
if (-not (Test-Path $compiler)) { throw 'Install .NET Framework 4.8 and ASP.NET 4.x on Windows.' }
$output = Join-Path $env:TEMP ('GroceryBuild-' + [Guid]::NewGuid().ToString('N'))
try {
    & $compiler -v /OnlineGroceryStore -p $SitePath -f $output
    if ($LASTEXITCODE -ne 0) { throw 'ASP.NET precompilation failed. Fix the reported source/markup errors.' }
    Write-Host 'PASS: ASP.NET page markup and VB.NET source precompiled.'
} finally {
    if (Test-Path $output) { Remove-Item -Recurse -Force $output }
}
Write-Host 'Next run the runtime test matrix in docs/TESTING.md against an isolated test database.'
