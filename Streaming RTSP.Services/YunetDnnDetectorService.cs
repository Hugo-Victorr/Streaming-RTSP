using OpenCvSharp;
using OpenCvSharp.Dnn;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Streaming_RTSP.Services
{
    public class YunetDnnDetectorService : IYunetDnnDetectorService, IDisposable
    {
        private Net _net;
        private readonly int _inputW;
        private readonly int _inputH;
        private readonly float _confThreshold;

        public YunetDnnDetectorService(string onnxPath, int inputW = 640, int inputH = 640, float confThreshold = 0.5f)
        {
            _inputW = inputW;
            _inputH = inputH;
            _confThreshold = confThreshold;
            _net = CvDnn.ReadNetFromOnnx(onnxPath);
            // opcional: preferir backend/target (CPU, OpenVINO, CUDA se disponível)
            //_net.SetPreferableBackend(NetBackend.OPENCV);
            //_net.SetPreferableTarget(NetTarget.CPU);
        }

        public IEnumerable<Rect> Detect(Mat src)
        {
            using var bgr = new Mat();
            if (src.Channels() == 4) Cv2.CvtColor(src, bgr, ColorConversionCodes.BGRA2BGR);
            else if (src.Channels() == 1) Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
            else src.CopyTo(bgr);

            int origW = bgr.Cols, origH = bgr.Rows;
            float scaleX = (float)origW / _inputW;
            float scaleY = (float)origH / _inputH;
            Debug.WriteLine("src channels: " + src.Channels());
            Debug.WriteLine("src size: " + src.Width + "x" + src.Height);

            using var inputBlobImage = new Mat();
            Cv2.Resize(bgr, inputBlobImage, new Size(_inputW, _inputH));

            Debug.WriteLine("bgr empty? " + bgr.Empty());
            Debug.WriteLine("bgr size: " + bgr.Width + "x" + bgr.Height);

            var blob = CvDnn.BlobFromImage(
                inputBlobImage,
                scaleFactor: 1.0,
                size: new Size(), 
                mean: new Scalar(0, 0, 0),
                swapRB: false,
                crop: false);

            Debug.WriteLine($"inputBlobImage empty? {inputBlobImage.Empty()}");
            Debug.WriteLine($"inputBlobImage size: {inputBlobImage.Width}x{inputBlobImage.Height}");
            Debug.WriteLine($"blob shape: {blob.Size()}");
            Debug.WriteLine($"blob dims: {blob.Dims}");
            Debug.WriteLine($"blob empty? {blob.Empty()}");

            _net.SetInput(blob);
            using var outputs = _net.Forward();
            Mat dets = outputs;

            var results = new List<Rect>();

            if (dets.Dims == 3 && dets.Size(0) == 1 && dets.Size(1) == 1)
            {
                dets = dets.Reshape(1, dets.Size(2));
            }
            else if (dets.Dims == 4) 
            {
                dets = dets.Reshape(1, dets.Size(dets.Dims - 2));
            }

            for (int i = 0; i < dets.Rows; i++)
            {
                float score = dets.At<float>(i, 4);
                if (score < _confThreshold) continue;

                float x = dets.At<float>(i, 0);
                float y = dets.At<float>(i, 1);
                float w = dets.At<float>(i, 2);
                float h = dets.At<float>(i, 3);

                int rx = (int)Math.Round(x * scaleX);
                int ry = (int)Math.Round(y * scaleY);
                int rw = (int)Math.Round(w * scaleX);
                int rh = (int)Math.Round(h * scaleY);

                rx = Math.Max(0, Math.Min(rx, origW - 1));
                ry = Math.Max(0, Math.Min(ry, origH - 1));
                if (rx + rw > origW) rw = origW - rx;
                if (ry + rh > origH) rh = origH - ry;

                results.Add(new Rect(rx, ry, rw, rh));
            }

            return results;
        }

        public void Dispose()
        {
            _net?.Dispose();
            _net = null;
        }
    }
}
