[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$publishDirectory = Join-Path $repositoryRoot "publish"
$launcherPath = Join-Path $publishDirectory "GenLauncherGO.exe"
$artifactsDirectory = Join-Path $repositoryRoot "artifacts"
$packageDirectory = Join-Path $artifactsDirectory "GenLauncherGO"
$archivePath = Join-Path $artifactsDirectory "GenLauncherGO.zip"

Push-Location $repositoryRoot
try {
    & dotnet publish ".\GenLauncherGO.UI\GenLauncherGO.UI.csproj" `
        "-p:PublishProfile=WinX64SelfContained" `
        "-o" $publishDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

if (-not (Test-Path -LiteralPath $launcherPath -PathType Leaf)) {
    throw "Expected the published launcher executable at $launcherPath."
}

if (Test-Path -LiteralPath $packageDirectory) {
    Remove-Item -LiteralPath $packageDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null
Copy-Item -LiteralPath $launcherPath -Destination $packageDirectory

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

Compress-Archive `
    -LiteralPath $packageDirectory `
    -DestinationPath $archivePath `
    -CompressionLevel Optimal

Write-Host "Created release package: $archivePath"
