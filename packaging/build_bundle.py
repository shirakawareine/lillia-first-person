"""Build the fresh-install ZIP from verified upstream files. No game installation is read."""
import argparse, configparser, hashlib, json, re, shutil, zipfile
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--upstream', type=Path, required=True)
parser.add_argument('--dll', type=Path, required=True)
parser.add_argument('--output', type=Path, required=True)
a = parser.parse_args()
repo = Path(__file__).resolve().parent.parent
up = a.upstream
out = a.output
out.mkdir(parents=True, exist_ok=True)
archive = up / 'BepInEx-788.zip'
assert hashlib.sha256(archive.read_bytes()).hexdigest() == 'f4cc496bd098a0df4164b81e3737297707f13a47c2478dba2f60eefab784817a'
assert hashlib.sha256(a.dll.read_bytes()).hexdigest() == '486b09f83693b26941792991a99a2b8900ee0f89e2216063e927956c81e67b4a'
stage = out / 'bundle'
if stage.exists():
    raise SystemExit('Staging directory exists; use a fresh output directory')
with zipfile.ZipFile(archive) as z:
    for name in z.namelist():
        assert not Path(name).is_absolute() and '..' not in Path(name).parts
    z.extractall(stage)
config = stage / 'BepInEx/config'; config.mkdir(exist_ok=True)
plugins = stage / 'BepInEx/plugins'; plugins.mkdir(exist_ok=True)
for name in ['local.lillia.firstpersontest.cfg', 'BepInEx.cfg']:
    shutil.copyfile(repo/'packaging'/(name+'.template'), config/name)
shutil.copyfile(a.dll, plugins/'FirstPersonTest.dll')
shutil.copyfile(repo/'packaging/START-HERE.txt', stage/'START-HERE.txt')
docs = stage/'LilliaFirstPerson-Docs'; docs.mkdir()
for p in repo.glob('*.md'): shutil.copyfile(p,docs/p.name)
third = docs/'ThirdParty'; third.mkdir()
shutil.copytree(up/'licenses',third/'licenses')
for name in ['dotnet-LICENSE.txt','dotnet-THIRD-PARTY-NOTICES.txt']:
    shutil.copyfile(up/name,third/'licenses'/name)
sources=third/'sources';sources.mkdir()
for name in ['BepInEx','Il2CppInterop','UnityDoorstop']:
    shutil.copyfile(up/'dependency-sources'/(name+'-source.zip'),sources/(name+'-source.zip'))
deps=json.loads((up/'dependencies.json').read_text())
assert all('error' not in d and d['licenses'] for d in deps)
notice='''THIRD-PARTY COMPONENTS\n\nBepInEx 6.0.0-be.788, commit 5b766a3b7f6c164d4798924a93f3acf4db769d06.\nOfficial unmodified Windows IL2CPP x64 binaries:\nhttps://builds.bepinex.dev/projects/bepinex_be\nArchive SHA256: f4cc496bd098a0df4164b81e3737297707f13a47c2478dba2f60eefab784817a\n\nAll upstream runtime files are preserved byte-for-byte. Added files are the plugin,\nconfiguration presets, documentation, license texts, and source archives.\nBepInEx, Il2CppInterop and UnityDoorstop are LGPL components: their corresponding\nsource archives are in sources/. Their license texts are in licenses/.\nThe shared components remain replaceable. Nothing in this bundle restricts\nmodification or reverse engineering of those components for debugging modifications.\nOriginal copyright, license terms, and warranty disclaimers apply to each component.\n\n.NET runtime 6.0.7: https://github.com/BepInEx/dotnet-runtime/releases/tag/6.0.7\n.NET source: https://github.com/dotnet/runtime/tree/v6.0.7\n.NET and associated Microsoft libraries: licenses/dotnet-LICENSE.txt and\nlicenses/dotnet-THIRD-PARTY-NOTICES.txt.\n\nComponent source references (see licenses/ for original notices):\n'''
notice+='\n'.join(d['name']+': '+d['source'] for d in deps)+'\n'
(third/'NOTICES.txt').write_text(notice,encoding='utf-8')
(third/'components.json').write_text(json.dumps(deps,indent=2),encoding='utf-8')
# Confirm parser-compatible presets and native loader paths.
c=configparser.ConfigParser();c.optionxform=str;c.read(config/'local.lillia.firstpersontest.cfg',encoding='utf-8-sig')
entries=c['EventView']['ClipLimits'].split(';')
assert len(entries)==3 and all(e.split('|')[1:]==['3','2','2'] for e in entries)
assert c['Body']['FollowCameraYaw']=='true' and c['Body']['FollowHeadPitch']=='true'
assert c['View']['LookUpDegrees']=='75' and c['View']['LookDownDegrees']=='85'
b=configparser.ConfigParser();b.read(config/'BepInEx.cfg',encoding='utf-8-sig');assert not b.getboolean('Logging.Console','Enabled')
door=configparser.ConfigParser();door.read(stage/'doorstop_config.ini')
assert door.getboolean('General','enabled')
for section,key in [('General','target_assembly'),('Il2Cpp','coreclr_path'),('Il2Cpp','corlib_dir')]:
    assert (stage/door[section][key].replace('\\','/')).exists()
with zipfile.ZipFile(archive) as z:
    for name in z.namelist():
        if not name.endswith('/'):assert z.read(name)==(stage/name).read_bytes(),name
for bad in ['interop','cache','unity-libs']:
    assert not (stage/'BepInEx'/bad).exists()
assert sorted(p.name for p in plugins.glob('*.dll'))==['FirstPersonTest.dll']
for p in stage.rglob('*'):
    if not p.is_file(): continue
    assert p.suffix.lower() not in ['.log','.pdb'],p
manifest=''.join(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+p.relative_to(stage).as_posix()+'\n' for p in sorted(stage.rglob('*')) if p.is_file())
(docs/'FILE-SHA256SUMS.txt').write_text(manifest,encoding='utf-8')
result=out/'FirstPersonTest-0.1.28-bundle-win-x64.zip'
with zipfile.ZipFile(result,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for p in sorted(stage.rglob('*')):
        if p.is_file():z.write(p,p.relative_to(stage).as_posix())
with zipfile.ZipFile(result) as z:
    assert z.testzip() is None
    assert 'winhttp.dll' in z.namelist() and 'BepInEx/plugins/FirstPersonTest.dll' in z.namelist()
(out/'BUNDLE-SHA256SUMS.txt').write_text(hashlib.sha256(result.read_bytes()).hexdigest()+'  '+result.name+'\n',encoding='ascii')
print(json.dumps({'bundle':str(result),'bytes':result.stat().st_size,'checks':'PASS: presets, original upstream bytes, DLL hash, loader paths, archive integrity','game_test':'Clean-install first launch not performed'},indent=2))
