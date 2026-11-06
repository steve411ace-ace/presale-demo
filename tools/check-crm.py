"""Static checks only: no .NET runtime or database required."""
from pathlib import Path
import re, subprocess, tempfile
root=Path(__file__).resolve().parent.parent
# Remove C# strings/comments before checking delimiters, including raw SQL literals.
def tokens(s):
    pattern=r'"""[\s\S]*?"""|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*[\s\S]*?\*/'
    return re.sub(pattern, '', s)
for path in root.glob('*.cs'):
    stack=[]
    for c in tokens(path.read_text()):
        if c in '({[': stack.append(c)
        elif c in ')}]':
            assert stack and stack.pop()=={')':'(',']':'[','}':'{'}[c],path.name
    assert not stack,path.name
    print('PASS C# delimiters:',path.name)
for path in (root/'wwwroot').glob('*.js'):
    subprocess.run(['node','--check',str(path)],check=True)
    print('PASS node --check:',path.name)
html=(root/'wwwroot/index.html').read_text()
for i,script in enumerate(re.findall(r'<script(?:\s[^>]*)?>([\s\S]*?)</script>',html)):
    if not script.strip():continue
    with tempfile.NamedTemporaryFile(suffix='.js',mode='w') as f:
        f.write(script);f.flush();subprocess.run(['node','--check',f.name],check=True)
    print('PASS inline JS:',i)
nav=re.search(r'<nav class="nav">([\s\S]*?)</nav>',html).group(1)
assert len(re.findall(r'<a ',nav))==11
assert 'scroll-margin-top:132px' in html.replace(' ','')
assert '#0a7a5e' not in html.lower()
assert 'StoryEdited' in (root/'Program.cs').read_text()
assert '<script src="crm.js"></script>' in html
assert '<script src="builder-media.js"></script>' in html
print('PASS preserved 11-item nav, 132px scroll margin, StoryEdited; no old teal')
cs=(root/'CrmApi.cs').read_text();sql=(root/'sql/CreateDatabase.sql').read_text()
embedded=re.search(r'public const string Schema = """\n([\s\S]*?)\n""";',cs).group(1).strip()
canonical=sql.split('-- BEGIN CRM TABLES\n')[1].split('-- END CRM TABLES')[0].strip()
assert embedded==canonical
print('PASS CRM schema parity: CreateDatabase.sql == CrmApi.Schema')
assert 'profile.Status="MoiKhaoSat"' in (root/'SurveyApi.cs').read_text()
assert 'db.SurveyResponses.RemoveRange(surveys)' in (root/'Program.cs').read_text()
print('PASS survey initial status + cascade delete code present (static only)')
