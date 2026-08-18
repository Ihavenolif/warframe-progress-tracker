$ErrorActionPreference = "Stop"

$version = @(git tag --points-at HEAD --sort=-version:refname)[0]
if (-not $version) {
    $version = git describe --tags --always --dirty
}
if ($LASTEXITCODE -ne 0 -or -not $version) {
    throw "Could not determine parser version from Git"
}

$versionPath = Join-Path $PSScriptRoot "version.txt"
try {
    [System.IO.File]::WriteAllText($versionPath, $version.Trim())
    python -m PyInstaller --clean --onefile --windowed --name alecaframe-parser --specpath build --add-data "$PSScriptRoot\transparent.ico;." --add-data "$versionPath;." --icon="$PSScriptRoot\transparent.ico" app.py
    if ($LASTEXITCODE -ne 0) {
        throw "PyInstaller build failed"
    }
}
finally {
    Remove-Item -LiteralPath $versionPath -ErrorAction SilentlyContinue
}
