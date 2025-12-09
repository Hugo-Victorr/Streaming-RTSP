using Prism.Events;
using System.Windows.Media.Imaging;

namespace Streaming_RTSP.Core.Events
{
    public class UpdateFrameViewerEvent : PubSubEvent<BitmapSource>
    {
    }
}
