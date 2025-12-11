# Streaming-RTSP
Projeto para capturar midia de uma transmissão RTSP (Real Time Stream Protocol)
# Teste Técnico – Aplicativo .NET Core com Streaming RTSP e Captura de Frame

Resumo
-----
Aplicativo Windows XAML-based que exibe um stream RTSP em tempo real e permite capturar frames do vídeo. Projeto organizado em MVVM com separação entre UI, serviços, tipos compartilhados e testes.

Estrutura do repositório
------------------------
- `Streaming RTSP` - Projeto WPF (Views, ViewModels, XAML).
- `Streaming RTSP.Core` - Tipos, enums e eventos compartilhados.
- `Streaming RTSP.Services` - Serviço de streaming e processamento de frames (decodificação, filtros, captura).
- `Streaming RTSP.Services.Interfaces` - Interfaces para serviços, possibilitando injeção de dependência.
- `Streaming RTSP.Services.Tests` - Testes unitários para lógica não dependente da UI.

Principais bibliotecas
---------------------
- .NET 10
- WPF (XAML)
- Prism (EventAggregator / MVVM helpers)
- FFMediaToolkit (decodificação de mídia)
- OpenCvSharp (filtros e detecção) — opcional
- Vlc.DotNet.Wpf (referência no projeto)

Requisitos
---------
- Windows 10/11
- .NET 10 SDK
- Visual Studio 2022/2023 ou `dotnet` CLI
- Visual C++ Redistributable (x64) se usar binários nativos do OpenCV/FFmpeg

Baixar FFmpeg e *.dlls
---------
1. Faça download da versão 7.* gpl Win64 shared (https://github.com/BtbN/FFmpeg-Builds/releases)
2. Extraia o zip;
3. renomeie a pasta interna para "ffmpeg";
4. Mova a pasta "ffmpeg" para o diretório C:\;
5. Dentro da pasta ffmpeg existe a pasta bin, onde estão as *.dlls. Copie o caminho para essa pasta (ex: C:\ffmpeg\bin)
6. Adicione o caminho copiado como uma variavel de ambiente;
7. Execute no cmd "ffmpeg -version" e verifique se foi configurado corretamente;

Baixar MediaMTX
---------
1. faça download da versão para Windows (https://github.com/bluenviron/mediamtx/releases)
2. Extraia o arquivo;
3. Execute o mediamtx.exe
4. Servidor RTSP está executando e escutando na porta 8554;

Como rodar
---------
- Clone/ Visual Studio:
1. Clonar o repositório:

   `git clone https://github.com/Hugo-Victorr/Streaming-RTSP.git`
   `cd "Streaming RTSP"`

2. Restaurar e compilar:

   `dotnet restore`
   `dotnet build`

3. Executar a aplicação (CLI):

   `dotnet run --project "Streaming RTSP/Streaming RTSP.csproj"`

Ou abrir a solução no Visual Studio e executar o projeto `Streaming RTSP`.

- Versão Release:
1. Baixar a versão Release Latest (https://github.com/Hugo-Victorr/Streaming-RTSP/releases/tag/Latest)
2. Extrair o conteudo;
3. Executar Streaming RTSP.exe

Uso
---
1. Envie para o servidor MediaMTX a mídia deseja via comando ffmpeg
* Video mp4: ffmpeg -re -stream_loop -1 -i {NOME_D0_VIDEO} -fflags +genpts -c copy -f rtsp rtsp://localhost:8554/stream
- Ex: ffmpeg -re -stream_loop -1 -i streaming.mp4 -fflags +genpts -c copy -f rtsp rtsp://localhost:8554/stream
* Imagem Webcam: ffmpeg -f dshow -i video="{NOME_DA_WEBCAM}" -f rtsp rtsp://localhost:8554/webcam
- Ex: ffmpeg -f dshow -i video="HD Webcam eMeet C960" -f rtsp rtsp://localhost:8554/webcam
2. Informe uma URL RTSP na interface (ou utilize a URL padrão, se houver).
3. Inicie o streaming.
4. Para capturar o frame atual, clique no botão "Capturar". A imagem é exibida e salva localmente (PNG/JPEG) conforme configuração.

Assets
------
- Arquivo de cascade para detecção de rosto: `OpenCvFiles/haarcascade_frontalface_default.xml`. Inclua esta pasta ao publicar.

Diferenciais implementados
-------------------------
- Integração com `OpenCvSharp` para filtros (grayscale, blur, sharpen).
- Detecção de faces com `CascadeClassifier` e desenho de retângulos.
- MVVM com `Prism.Events` (EventAggregator) para comunicação entre serviço e ViewModel.
- Tratamento de erros no `RTSPStreamingService`: validações na inicialização, proteções contra nulls, try/catch em pontos de conexão/parse/cast e exibição de `MessageBox` ao usuário quando apropriado.
- Testes unitários básicos para lógica de serviço.

Observações e troubleshooting
---------------------------
- Se houver falha ao abrir o stream, verifique a URL RTSP e disponibilidade de rede.
- Mensagens de erro são exibidas via `MessageBox` e logs em console ajudam no diagnóstico.
- Ao usar bindings nativos (OpenCV/FFmpeg), inclua DLLs nativas no PATH ou na pasta do executável.

Testes
------
- Executar testes via CLI:

  `dotnet test "Streaming RTSP.Services.Tests/Streaming RTSP.Services.Tests.csproj"`

Limitações conhecidas
---------------------
- Decodificação depende de `FFMediaToolkit` e codecs nativos; pode ser necessário fornecer binários adicionais.
- Ajustes de caminho para salvar imagens podem ser desejados em produção.

