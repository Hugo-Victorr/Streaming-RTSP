using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Media.Imaging;
using Streaming_RTSP.Core.Enums;

namespace Streaming_RTSP.Services.Interfaces
{
    public interface ILocalImageService
    {
        /// <summary>
        /// Salva o BitmapSource fornecido no formato especificado no caminho local.
        /// </summary>
        /// <param name="source">O BitmapSource a ser salvo.</param>
        /// <param name="format">O formato desejado (PNG ou JPEG).</param>
        /// <returns>O caminho completo do arquivo salvo.</returns>
        string SaveFrame(BitmapSource source, Core.Enums.ImageFormat format);

        /// <summary>
        /// Recupera a lista de caminhos de arquivos de imagem no diretório de armazenamento.
        /// </summary>
        /// <returns>Uma lista de strings com os caminhos completos dos arquivos.</returns>
        List<string> GetSavedImagesPaths();
    }
}
