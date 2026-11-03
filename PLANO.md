# Lumen (Versículos na Tela)

App para Windows que mostra o versículo do dia.

## O que o app deve fazer

- Widget sempre visível na área de trabalho.
- Versículo do dia também na tela de bloqueio.
- Abrir sozinho ao iniciar o PC.
- Uso pessoal, por enquanto.

## Tradução

Bíblia Sagrada, tradução do Pe. Manuel de Matos Soares (1956), católica, 73 livros.
Fonte do texto: https://github.com/Dancrf/biblia-db

- A Ave-Maria foi a primeira escolha, mas tem direitos autorais e não tem fonte aberta.
- Os Salmos seguem a numeração da Vulgata (ex.: "O Senhor é meu pastor" é o Salmo 22).

## Plano

1. **Lista de versículos** (pronto, 03/11/2026). Um versículo para cada dia do ano, incluindo 29/02, em `versiculos.json` (chave `MM-DD`). Versão legível em `lista-de-versiculos.md`.
   Datas especiais: 01/01, 25/03 (Anunciação), 15/08 (Assunção), 12/10, 02/11 (Finados), 24/12 e 25/12.
2. **Widget na área de trabalho.** App leve em .NET: caixinha transparente fixa na tela, troca o versículo à meia-noite e pode ser arrastada.
3. **Tela de bloqueio e inicialização.** O Windows não aceita widgets na tela de bloqueio, então o app gera uma imagem com o versículo do dia e a define como fundo da tela de bloqueio. Também se registra para abrir ao ligar o PC.
4. **Instalar e testar.** Entregar um .exe pronto e ajustar fonte, cores e posição.

## Arquivos

- `PLANO.md`: este resumo.
- `versiculos.json`: os 366 versículos que o app lê.
- `lista-de-versiculos.md`: a mesma lista, por mês, para leitura.

## Nome e logo

- Nome escolhido: **Lumen** (Salmo 118,105: "Lâmpada para os meus passos é a tua palavra").
- Logo escolhida: **A, lamparina acesa** sobre fundo azul-noite. Arquivos finais em `logo/final/` (`lumen.svg`, `lumen.ico` para o Windows, PNGs 256 e 512). Os outros rascunhos ficam em `logo/` como referência.
