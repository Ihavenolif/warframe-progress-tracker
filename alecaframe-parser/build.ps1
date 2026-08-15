python -m PyInstaller --clean --onefile --windowed --name alecaframe-parser --specpath build --add-data "$PSScriptRoot\transparent.ico;." --icon="$PSScriptRoot\transparent.ico" app.py
