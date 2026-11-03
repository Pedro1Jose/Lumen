import json,re,random,datetime,subprocess,sys
subprocess.run([sys.executable,'-I','extract.py','../biblia-db/biblia.json','referencias.txt','raw.json'],stdout=subprocess.DEVNULL,check=True)
d=json.load(open('raw.json'))
pre=re.compile(r'^((Salmo|De Davide|Cântico das subidas|De Salomão|Miktãm|Aleluia|HET|TET|Cântico)\.\s*)+')
for x in d:
    t=x['texto'].replace('(ver nota)','').replace('(Ps. 4, 5)','')
    t=re.sub(r'\bIhe',"lhe",t); t=pre.sub('',t)
    t=re.sub(r'\s+([;:,.!?])',r'\1',t); t=re.sub(r'\s{2,}',' ',t).strip()
    t=re.sub(r'\b(HET|TET)\.\s*','',t).replace('pala ti','para ti').replace('Convém que ela cresça','Convém que ele cresça')
    if t.count('"')%2: t=t.strip('"').strip()
    if t.count('"')%2: t=t.replace('"','')
    t=re.sub(r'[,;:]$','.',t)
    if t[-1].isalpha(): t+='.'
    if t[0].islower(): t=t[0].upper()+t[1:]
    x['texto']=t
assert len(d)==366 and all(x['texto'] for x in d)
special={'01-01':'Números 6,24-26','12-25':'Lucas 2,10-11','12-24':'Isaías 9,6','03-25':'Lucas 1,38','08-15':'Lucas 1,46-47','11-02':'Sabedoria 3,1','10-12':'João 19,27'}
byref={x['referencia']:x for x in d}
for v in special.values(): assert v in byref,v
rest=[x for x in d if x['referencia'] not in special.values()]
random.Random(2026).shuffle(rest)
days=[(datetime.date(2024,1,1)+datetime.timedelta(i)).strftime('%m-%d') for i in range(366)]
out={}
for day in days:
    out[day]=byref[special[day]] if day in special else rest.pop()
json.dump({"traducao":"Bíblia Sagrada, trad. Pe. Matos Soares (1956)","dias":out},open('versiculos.json','w'),ensure_ascii=False,indent=1)
print(len(out)); [print(k,v) for k,v in list(out.items())[:5]]
