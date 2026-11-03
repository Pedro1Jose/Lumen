# Como a lista de versículos foi gerada

1. Clone o texto da Bíblia Matos Soares: `git clone https://github.com/Dancrf/biblia-db` ao lado desta pasta (o script espera `../biblia-db/biblia.json`).
2. `referencias.txt` tem as 366 referências escolhidas (Salmos na numeração da Vulgata).
3. `python build.py` gera `versiculos.json` (um versículo por dia, chave `MM-DD`), limpando notas e títulos dos Salmos.
