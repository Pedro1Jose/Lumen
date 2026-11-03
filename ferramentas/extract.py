import json,sys
b=json.load(open(sys.argv[1]))
ab="Gn Ex Lv Nm Dt Js Jz Rt 1Sm 2Sm 1Rs 2Rs 1Pa 2Pa Esd Ne Tb Jt Est 1Mc 2Mc Jó Sl Pr Ecl Ct Sb Eclo Is Jr Lm Br Ez Dn Os Jl Am Ab Jn Mq Na Hc Sf Ag Zc Ml Mt Mc Lc Jo At Rm 1Cor 2Cor Gl Ef Fl Cl 1Ts 2Ts 1Tm 2Tm Tt Fm Hb Tg 1Pd 2Pd 1Jo 2Jo 3Jo Jd Ap".split()
nome="Gênesis|Êxodo|Levítico|Números|Deuteronômio|Josué|Juízes|Rute|1 Samuel|2 Samuel|1 Reis|2 Reis|1 Crônicas|2 Crônicas|Esdras|Neemias|Tobias|Judite|Ester|1 Macabeus|2 Macabeus|Jó|Salmo|Provérbios|Eclesiastes|Cântico dos Cânticos|Sabedoria|Eclesiástico|Isaías|Jeremias|Lamentações|Baruc|Ezequiel|Daniel|Oseias|Joel|Amós|Abdias|Jonas|Miqueias|Naum|Habacuc|Sofonias|Ageu|Zacarias|Malaquias|Mateus|Marcos|Lucas|João|Atos|Romanos|1 Coríntios|2 Coríntios|Gálatas|Efésios|Filipenses|Colossenses|1 Tessalonicenses|2 Tessalonicenses|1 Timóteo|2 Timóteo|Tito|Filêmon|Hebreus|Tiago|1 Pedro|2 Pedro|1 João|2 João|3 João|Judas|Apocalipse".split("|")
assert len(ab)==73==len(nome)
out=[]
for line in open(sys.argv[2]):
    line=line.strip()
    if not line: continue
    a,r=line.split(); c,v=r.split(','); c=int(c)
    v1,v2=(map(int,v.split('-')) if '-' in v else (int(v),int(v)))
    i=ab.index(a); vs=b[i]['capitulos'][c-1]['versiculos']
    t=' '.join(x['texto'].strip() for x in vs if v1<=x['numero']<=v2)
    out.append({"referencia":f"{nome[i]} {c},{v}","texto":t,"livro":i,"cap":c,"v1":v1,"v2":v2})
json.dump(out,open(sys.argv[3],'w'),ensure_ascii=False,indent=1)
for k,o in enumerate(out): print(k+1,o['referencia'],'|',o['texto'])
