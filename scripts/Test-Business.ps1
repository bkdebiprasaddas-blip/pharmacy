# Integration tests need Windows PowerShell 5.1, .NET 4.8 and ACE of matching bitness.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot
$frameworkFolder = if ([Environment]::Is64BitProcess) { 'Framework64' } else { 'Framework' }
$framework = Join-Path $env:WINDIR "Microsoft.NET\$frameworkFolder\v4.0.30319"
$vbc = Join-Path $framework 'vbc.exe'
if (-not (Test-Path $vbc)) { throw 'Install .NET Framework 4.8 on Windows.' }
$temp = Join-Path $env:TEMP ('GroceryTests-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $temp
try {
    $password = ConvertTo-SecureString ('Test-' + [Guid]::NewGuid().ToString('N')) -AsPlainText -Force
    & (Join-Path $PSScriptRoot 'Initialize-Database.ps1') -DatabasePath (Join-Path $temp 'GroceryDB.accdb') -AdminEmail 'admin@example.test' -AdminPassword $password
    $sources = @(Get-ChildItem (Join-Path $repo 'OnlineGroceryStore\App_Code\*.vb') | ForEach-Object { $_.FullName })
    $sources += Join-Path $repo 'tests\BusinessTests.vb'
    $exe = Join-Path $temp 'BusinessTests.exe'
    $platform = if ([Environment]::Is64BitProcess) { 'x64' } else { 'x86' }
    & $vbc /nologo /target:exe "/platform:$platform" /optionstrict+ /optionexplicit+ /optioninfer+ "/out:$exe" /reference:System.dll,System.Core.dll,System.Data.dll,System.Web.dll,System.Configuration.dll @sources
    if ($LASTEXITCODE -ne 0) { throw 'VB.NET integration test compilation failed.' }
    $config = @'
<configuration>
  <startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" /></startup>
  <connectionStrings><add name="GroceryDB" connectionString="Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\GroceryDB.accdb;Persist Security Info=False;" /></connectionStrings>
</configuration>
'@
    Set-Content -Path "$exe.config" -Value $config -Encoding UTF8
    & $exe $temp
    if ($LASTEXITCODE -ne 0) { throw 'Business integration tests failed.' }
} finally {
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
    if (Test-Path $temp) { Remove-Item -Recurse -Force $temp }
}
