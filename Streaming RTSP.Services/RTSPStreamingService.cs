using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using Prism.Events;
using Streaming_RTSP.Core.Events;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private readonly IEventAggregator _eventAggregator;

        public event Action<BitmapSource> FrameReady;

        public RTSPStreamingService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void StartStream(string rtspUrl)
        {
            if(_file != null)
            {
                StopStream();
                return;
            }

            _rtspUrl = rtspUrl;
            _cts = new CancellationTokenSource();
            _decodingTask = Task.Run(DecodingLoop);
        }

        public void StopStream()
        {
            _cts?.Cancel();
            _file?.Dispose();
        }

        /// <summary>
        /// Decodifica o frame da transmissão da midia.
        /// </summary>
        private void DecodingLoop()
        {
            try
            {
                _file = MediaFile.Open(@$"{_rtspUrl}", new MediaOptions() { VideoPixelFormat = ImagePixelFormat.Rgba32 });

                while (!_cts.IsCancellationRequested)
                {
                    if (_file.Video.TryGetNextFrame(out var frame))
                    {
                        var bitmapSource = ConvertFrameToBitmapSource(frame);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            FrameReady?.Invoke(bitmapSource);
                            _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Publish(bitmapSource);
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no stream RTSP: {ex.Message}");
            }
        }

        /// <summary>
        /// Converte o objeto BitmapData em um BitmapSource.
        /// </summary>
        private BitmapSource ConvertFrameToBitmapSource(ImageData frame)
        {
            var bitmap = BitmapSource.Create(
                            frame.ImageSize.Width,
                            frame.ImageSize.Height,
                            96,
                            96,
                            PixelFormats.Bgra32,
                            null,
                            frame.Data.ToArray(),
                            frame.ImageSize.Width * 4
                        );

            return bitmap;
        }

        public void Dispose()
        {
            StopStream();
        }
    }
}
