from pathlib import Path
import shutil
import zipfile

base = Path(__file__).resolve().parent
if not (base / 'addon/manifest.ini').exists():
    (base / 'addon/globalPlugins').mkdir(parents=True, exist_ok=True)
    (base / 'addon/doc/es').mkdir(parents=True, exist_ok=True)
    shutil.copyfile(base / 'autoCalc.py', base / 'addon/globalPlugins/autoCalc.py')
    shutil.copyfile(base / 'manifest.ini', base / 'addon/manifest.ini')
    shutil.copyfile(base / 'readme.html', base / 'addon/doc/es/readme.html')
payload = base / 'addon/globalPlugins/autoCalc'
payload.mkdir(parents=True, exist_ok=True)
shutil.copyfile(base / 'AutoCalc.exe', payload / 'AutoCalc.exe')
shutil.copytree(base / 'sounds', payload / 'sounds', dirs_exist_ok=True)
target = base / 'downloads/AutoCalc-0.1.2.nvda-addon'
target.parent.mkdir(exist_ok=True)
with zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED) as archive:
    for path in sorted((base / 'addon').rglob('*')):
        if path.is_file() and '__pycache__' not in path.parts:
            archive.write(path, path.relative_to(base / 'addon').as_posix())
print(target)
