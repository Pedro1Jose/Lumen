<p align="center">
  <img src="logo/lumen-256.png" alt="Lumen" width="96">
</p>

<h1 align="center">Lumen</h1>

<p align="center"><i>"Lâmpada para os meus passos é a tua palavra." (Salmo 118,105)</i></p>

Lumen é um pequeno app para Windows que mostra o **versículo do dia** na área de trabalho e na tela de bloqueio. Os textos são da Bíblia católica, tradução do Pe. Manuel de Matos Soares (1956).

## Funcionalidades

- **Widget na área de trabalho:** o versículo do dia fica sempre visível, num estilo minimalista (lamparina, texto branco e referência dourada) que dá para ler em qualquer papel de parede.
- **Tela de bloqueio:** todo dia o Lumen gera uma imagem com o versículo e a coloca como fundo da tela de bloqueio, no canto inferior esquerdo, longe do relógio e da caixa do PIN.
- **Um versículo para cada dia do ano**, incluindo 29 de fevereiro, com versículos especiais no Ano Novo, Anunciação, Assunção, 12 de outubro, Finados, véspera de Natal e Natal.
- **Capítulo inteiro:** um clique no versículo abre o capítulo completo, com o versículo do dia em destaque. As setas mudam de capítulo.
- **Inicia com o Windows** e troca o versículo sozinho à meia-noite.
- **Tamanho do texto** pequeno, médio ou grande.
- **Arrastar** o widget para qualquer lugar; ele lembra a posição e se ajusta à escala de cada monitor.
- Não some com **Win+D** ("Mostrar área de trabalho").
- **Atualização sem fechar:** coloque um `Lumen-novo.exe` ao lado do `Lumen.exe` e o app se troca sozinho em poucos segundos.

## Como usar

1. Baixe o `Lumen.exe` na página de [Releases](../../releases), ou compile (veja abaixo).
2. Coloque o arquivo numa pasta fixa, por exemplo `Documentos\Lumen`. O "Iniciar com o Windows" aponta para esse local.
3. Dê dois cliques no `Lumen.exe`. Se o Windows pedir o **.NET 8 Desktop Runtime**, aceite a instalação.
4. Se o SmartScreen avisar que o app não é reconhecido, clique em **Mais informações** e depois em **Executar assim mesmo**.

### Menu da bandeja

Clique com o botão direito no ícone da lamparina, perto do relógio:

| Opção | O que faz |
|---|---|
| Mostrar / Ocultar o widget | Mostra ou esconde o versículo na área de trabalho |
| Ler o capítulo inteiro | Abre o capítulo do versículo do dia |
| Copiar versículo | Copia o texto e a referência |
| Tamanho do texto | Pequeno, Médio ou Grande |
| Sempre por cima das janelas | Deixa o widget na frente de tudo |
| Versículo na tela de bloqueio | Liga ou desliga a imagem da tela de bloqueio |
| Iniciar com o Windows | Abre o Lumen ao entrar no Windows |
| Sair | Fecha o app |

### Dicas

- **Tela de bloqueio:** em Configurações → Personalização → Tela de bloqueio, deixe a opção **"Imagem"** (o Destaque do Windows substitui a imagem do Lumen).
- **Tela do PIN desfocada:** em Configurações → Personalização → Cores, desligue **"Efeitos de transparência"**.
- As preferências ficam em `%APPDATA%\Lumen\config.json`.

## Compilar

Requer o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
cd codigo
dotnet publish -c Release -o out
```

O executável fica em `codigo/out/Lumen.exe`. No primeiro build, o projeto baixa o texto completo da Bíblia (`Assets/biblia.json`) do repositório de origem. Esse texto não é guardado neste repositório.

## Estrutura

```
codigo/            código do app (C#, .NET 8, Windows Forms)
ferramentas/       scripts que geraram a lista de versículos do ano
logo/              logo da lamparina (SVG, ICO e PNG)
versiculos.json    os 366 versículos, um por dia (chave MM-DD)
lista-de-versiculos.md   a mesma lista, por mês, para leitura
```

## Texto bíblico

Bíblia Sagrada, tradução do Pe. Manuel de Matos Soares (1956), católica, 73 livros. O texto vem de [Dancrf/biblia-db](https://github.com/Dancrf/biblia-db). Os Salmos seguem a numeração da Vulgata; por exemplo, "O Senhor é meu pastor" é o Salmo 22.

## Licença

O código, os scripts e a logo estão sob a [licença MIT](LICENSE). O texto bíblico não é coberto por ela e segue as condições da fonte original.
