using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using OpenCvSharp;
using Prism.Events;
using Streaming_RTSP.Core.Enums;
using Streaming_RTSP.Core.Events;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vlc.DotNet.Wpf;

namespace Streaming_RTSP.Services
{
    public class RTSPStreamingService : IRTSPStreamingService
    {
        private Task _decodingTask;
        private CancellationTokenSource _cts;
        private MediaFile _file;
        private string _rtspUrl;
        private WriteableBitmap _writeableBitmap;

        private bool _sharp = false;
        public bool Sharp
        {
            get => _sharp;
            set => _sharp = value;
        }

        private bool _blur = false;
        public bool Blur
        {
            get => _blur;
            set => _blur = value;
        }

        private bool _grayscale = false;
        public bool Grayscale
        {
            get => _grayscale;
            set => _grayscale = value;
        }

        private readonly IEventAggregator _eventAggregator;

        public RTSPStreamingService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Iniciar a tarefa de decoding do stream RTSP.
        /// </summary>
        /// <param name="rtspUrl"></param>
        public void StartStream(string rtspUrl)
        {
            if (_file != null)
                return;

            Initialize(rtspUrl);
            _cts = new CancellationTokenSource();
            _decodingTask = Task.Run(() => DecodingLoopWithOpenCvAsync(_cts.Token), _cts.Token);
            _decodingTask.ContinueWith(Dispose,
                TaskContinuationOptions.None);
        }

        /// <summary>
        /// Cancela a tarefa de decoding do stream RTSP.
        /// </summary>
        public void StopStream()
        {
            CancelStream();
        }

        private void Initialize(string rtspUrl)
        {
            _rtspUrl = rtspUrl;
            _file = MediaFile.Open(@$"{_rtspUrl}", new MediaOptions() { VideoPixelFormat = ImagePixelFormat.Bgra32 });
            _writeableBitmap = new WriteableBitmap(_file.Video.Info.FrameSize.Width, _file.Video.Info.FrameSize.Height, 96, 96, PixelFormats.Bgra32, null);
        }

        /// <summary>
        /// Cancela o CancellationToken.
        /// </summary>
        private void CancelStream()
        {
            if (_file == null)
                return;

            if (!_cts.IsCancellationRequested)
                _cts?.Cancel();
        }

        private async Task DecodingLoopWithOpenCvAsync(CancellationToken ct)
        {
            try
            {
                int width = _file.Video.Info.FrameSize.Width;
                int height = _file.Video.Info.FrameSize.Height;
                int stride = width * 4; // BGRA
                byte[] buffer = new byte[stride * height];

                while (!ct.IsCancellationRequested)
                {
                    bool ok;
                    unsafe
                    {
                        fixed (byte* ptr = buffer)
                        {
                            ok = _file.Video.TryGetNextFrame((nint)ptr, stride);
                        }
                    }

                    if (!ok)
                        continue;

                    using var frame = Mat.FromPixelData(height, width, MatType.CV_8UC4, buffer);
                    ApplyImageFilters(frame);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _writeableBitmap.Lock();
                        unsafe
                        {
                            void* src = frame.Data.ToPointer();
                            void* dst = _writeableBitmap.BackBuffer.ToPointer();
                            long bytes = stride * height;
                            Buffer.MemoryCopy(src, dst, bytes, bytes);
                        }

                        _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                        _writeableBitmap.Unlock();

                        _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Publish(_writeableBitmap);
                    });

                    await Task.Delay(33, ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decoding loop error: {ex.Message} - {ex?.InnerException} - {ex?.StackTrace}");
            }
        }

        private void ApplyImageFilters(Mat frame)
        {
            if(_sharp)
                ApplySharp(frame);
            
            if(_blur)
                ApplyBlur(frame);

            if(_grayscale)
                ApplyGrayscale(frame);
        }

        private void ApplyBlur(Mat frame)
        {
            Cv2.GaussianBlur(frame, frame, new OpenCvSharp.Size(15, 15), 0);
        }

        private void ApplyGrayscale(Mat frame)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGRA2GRAY);
            Cv2.CvtColor(frame, frame, ColorConversionCodes.GRAY2BGRA);
        }

        private void ApplySharp(Mat frame)
        {
            var kernel = new float[,]
            {
                { -1, -1, -1 },
                { -1,  9, -1 },
                { -1, -1, -1 }
            };

            using var k = InputArray.Create(kernel);
            Cv2.Filter2D(frame, frame, MatType.CV_8UC4, k);
        }

        private void Dispose(Task completedTask)
        {
            _file?.Dispose();
            _file = null;

            _decodingTask = null;

            _writeableBitmap = null;

            _cts?.Dispose();
        }
    }
}
