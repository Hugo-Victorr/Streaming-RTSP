using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Streaming_RTSP.Services.Interfaces
{
    public interface IRTSPStreamingService
    {
        /// <summary>
        /// Iniciar a tarefa de decoding do stream RTSP.
        /// </summary>
        /// <param name="rtspUrl"></param>
        void StartStream(string rtspUrl);

        /// <summary>
        /// Cancela a tarefa de decoding do stream RTSP.
        /// </summary>
        void StopStream();

        bool Sharp { get; set; }

        bool Blur { get; set; }

        bool Grayscale { get; set; }

        bool DetectFace { get; set; }
    }
}
