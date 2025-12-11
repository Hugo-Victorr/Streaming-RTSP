using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Streaming_RTSP.Services.Interfaces
{
    public interface IYunetDnnDetectorService
    {
        IEnumerable<Rect> Detect(Mat frame);
    }
}
