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
        event Action<BitmapSource> FrameReady;

        void StartStream(string rtspUrl);

        void StopStream();
    }
}
