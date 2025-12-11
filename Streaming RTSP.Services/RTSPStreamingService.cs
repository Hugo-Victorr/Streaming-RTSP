using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using OpenCvSharp;
using OpenCvSharp.Dnn;
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
        private CascadeClassifier _faceCascade = new CascadeClassifier(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenCvFiles", "haarcascade_frontalface_default.xml"));

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

        private bool _detectFace = false;
        public bool DetectFace
        {
            get => _detectFace;
            set => _detectFace = value;
        }

        private int _frameCounter = 0;
        private int _detectInterval = 5;
        private OpenCvSharp.Rect[] _lastFaces = Array.Empty<OpenCvSharp.Rect>();

        private readonly IEventAggregator _eventAggregator;

        public RTSPStreamingService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void StartStream(string rtspUrl)
        {
            try
            {
                if (_file != null)
                    return;

                if (!Initialize(rtspUrl))
                    return;

                _cts = new CancellationTokenSource();
                _decodingTask = Task.Run(() => DecodingLoopWithOpenCvAsync(_cts.Token), _cts.Token);
                _decodingTask.ContinueWith(Dispose,
                    TaskContinuationOptions.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao iniciar o stream:\n{ex.Message}",
                    "Erro RTSP",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public void StopStream()
        {
            CancelStream();
        }

        private bool Initialize(string rtspUrl)
        {
            try
            {
                _rtspUrl = rtspUrl;
                _file = MediaFile.Open(@$"{_rtspUrl}", new MediaOptions() { VideoPixelFormat = ImagePixelFormat.Bgra32 });
                _writeableBitmap = new WriteableBitmap(_file.Video.Info.FrameSize.Width, _file.Video.Info.FrameSize.Height, 96, 96, PixelFormats.Bgra32, null);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar ao RTSP: {ex.Message} - {ex?.InnerException} - {ex?.StackTrace}");
                MessageBox.Show(
                    $"Erro ao conectar ao RTSP: \nVerifique se o servidor está ativo.",
                    "Falha na Conexão",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                _file = null;
                return false;
            }
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
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                    $"Streaming parado",
                    "Parar streaming",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decoding loop error: {ex.Message} - {ex?.InnerException} - {ex?.StackTrace}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                    $"Erro na leitura do stream:\n Verifique se a midia é valida.",
                    "Erro de Decodificação",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
                });
            }
        }

        private void ApplyImageFilters(Mat frame)
        {
            if (_detectFace)
                ApplyDetectFace(frame);

            if (_sharp)
                ApplySharp(frame);

            if (_blur)
                ApplyBlur(frame);

            if (_grayscale)
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

        private void ApplyDetectFace(Mat frame)
        {
            try
            {
                _frameCounter++;

                // Só detecta a cada N frames
                if (_frameCounter % _detectInterval == 0)
                {
                    using var gray = new Mat();
                    Cv2.CvtColor(frame, gray, ColorConversionCodes.BGRA2GRAY);

                    var faces = _faceCascade.DetectMultiScale(
                        gray,
                        scaleFactor: 1.1,
                        minNeighbors: 5,
                        flags: HaarDetectionTypes.ScaleImage,
                        minSize: new OpenCvSharp.Size(50, 50)
                    );

                    _lastFaces = faces;
                }

                if (frame.Channels() == 1)
                {
                    Cv2.CvtColor(frame, frame, ColorConversionCodes.GRAY2BGRA);
                }

                foreach (var f in _lastFaces)
                {
                    Cv2.Rectangle(
                        frame,
                        new OpenCvSharp.Rect(f.X, f.Y, f.Width, f.Height),
                        Scalar.Green,
                        thickness: 3
                    );
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                    $"Erro na detecção de rosto",
                    "Erro OpenCV",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
                });
            }
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
