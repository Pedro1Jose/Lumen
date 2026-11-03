# Código do Lumen

App em C# (.NET 8, Windows Forms).

- `Program.cs`: inicia o app, ícone na bandeja e menu.
- `WidgetForm.cs`: a janela transparente do widget (arrastar, ficar atrás das janelas ou por cima).
- `CardRenderer.cs`: desenha o cartão do versículo (também será usado para a tela de bloqueio).
- `Verses.cs`: lê `Assets/versiculos.json`, embutido no .exe.
- `Settings.cs`: salva posição e preferências em `%APPDATA%\Lumen\config.json`.
- `Startup.cs`: liga e desliga "Iniciar com o Windows".

Para compilar no Windows (com o .NET 8 SDK instalado): `dotnet publish -c Release -o out`
