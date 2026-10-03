#!/usr/bin/env python3
"""Validate UPM packaging; deliberately not a substitute for Unity tests."""
import json
import pathlib
import re

root = pathlib.Path(__file__).resolve().parents[1]
package = json.loads((root / 'package.json').read_text())
assert package['name'] == 'com.kruty1918.gameplay-viewport'
assert re.fullmatch(r'\d+\.\d+\.\d+', package['version'])
assert package['unity'] == '6000.0'
assert package['license'] == 'MIT'
for name in ('README.md', 'LICENSE', 'CHANGELOG.md', 'CONTRIBUTING.md',
             'Documentation~/api.md', 'Documentation~/quick-start.md',
             'Documentation~/integration.md', 'Documentation~/verification.md'):
    assert (root / name).is_file(), name
for sample in package['samples']:
    assert (root / sample['path']).is_dir()
    assert (root / sample['path'] / 'README.md').is_file()
guids = set()
for meta in root.rglob('*.meta'):
    match = re.search(r'^guid: ([a-f0-9]{32})$', meta.read_text(), re.M)
    assert match, meta
    assert match[1] not in guids, meta
    guids.add(match[1])
for folder in ('Runtime', 'Editor', 'Tests'):
    for source in (root / folder).rglob('*'):
        if source.is_file() and source.suffix in ('.cs', '.asmdef'):
            assert pathlib.Path(str(source) + '.meta').is_file(), source
for definition in root.rglob('*.asmdef'):
    json.loads(definition.read_text())
for doc in [root / 'README.md', *(root / 'Documentation~').glob('*.md')]:
    for link in re.findall(r'\]\(([^)#]+)(?:#[^)]*)?\)', doc.read_text()):
        if ':' not in link:
            assert (doc.parent / link).exists(), (doc, link)
print('PASS: package manifest, sample, metadata, assembly definitions and documentation links')
