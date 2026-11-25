param(
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $repoRoot 'src/DesktopWorldClock.App/DesktopWorldClock.App.csproj'
$releaseDir = Join-Path $repoRoot 'artifacts/release'
$portableDir = Join-Path $releaseDir 'portable'
$exeName = 'DesktopWorldClock.exe'

$version = ([xml](Get-Content -Raw $appProject)).Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Version is missing from $appProject."
}

if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}

Write-Host "Publishing $version ($Runtime)..."
$publishArgs = @(
    'publish', $appProject,
    '-c', 'Release',
    '-r', $Runtime,
    '--self-contained',
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:EnableCompressionInSingleFile=true',
    '-p:PublishReadyToRun=true',
    '-p:DebugType=None',
    '-p:DebugSymbols=false',
    '-o', $portableDir
)
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet publish failed.'
}

$portableExe = Join-Path $portableDir $exeName
if (-not (Test-Path $portableExe)) {
    throw "Published executable not found at $portableExe."
}

# XML documentation files are only useful for IntelliSense; the portable build ships the exe alone.
Get-ChildItem $portableDir -Filter '*.xml' -File | Remove-Item -Force

$zipName = "DesktopWorldClock-$version-$Runtime-portable.zip"
Write-Host "Creating $zipName..."
Compress-Archive -Path $portableExe -DestinationPath (Join-Path $releaseDir $zipName) -CompressionLevel Optimal -Force

$iscc = @(
    (Join-Path $env:ProgramFiles 'Inno Setup 7\ISCC.exe'),
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 7\ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe')
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $iscc) {
    throw 'Inno Setup compiler (ISCC.exe) not found. Install Inno Setup 6 or 7.'
}

Write-Host "Building installer with $iscc..."
& $iscc "/DAppVersion=$version" (Join-Path $repoRoot 'installer/DesktopWorldClock.iss')
if ($LASTEXITCODE -ne 0) {
    throw 'ISCC failed.'
}

$checksumLines = Get-ChildItem $releaseDir -File |
    Where-Object { $_.Extension -in '.exe', '.zip' } |
    Sort-Object Name |
    ForEach-Object {
        '{0}  {1}' -f (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name
    }
$checksumLines | Set-Content -Path (Join-Path $releaseDir 'SHA256SUMS.txt') -Encoding ascii

Write-Host ''
Write-Host "Release files in ${releaseDir}:"
Get-ChildItem $releaseDir -File | Select-Object Name, @{ n = 'SizeMB'; e = { [math]::Round($_.Length / 1MB, 1) } } | Format-Table -AutoSize
