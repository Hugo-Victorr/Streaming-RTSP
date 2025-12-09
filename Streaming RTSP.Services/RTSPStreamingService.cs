using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using Prism.Events;
using Streaming_RTSP.Core.Events;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            _decodingTask = Task.Run(() => DecodingLoopAsync(_cts.Token), _cts.Token);
        }

        public void StopStream()
        {
            if(!_cts.IsCancellationRequested)
                _cts?.Cancel();
        }

        /// <summary>
        /// Decodifica o frame da transmissão da midia.
        /// </summary>
        private async Task DecodingLoopAsync(CancellationToken ct)
        {
            try
            {
                _file = MediaFile.Open(@$"{_rtspUrl}", new MediaOptions() { VideoPixelFormat = ImagePixelFormat.Rgba32 });
                
                while (!ct.IsCancellationRequested)
                {
                    if (_file.Video.TryGetNextFrame(out var frame))
                    {
                        var bitmapSource = ConvertFrameToBitmapSource(frame);

                        if (bitmapSource.CanFreeze)
                            bitmapSource.Freeze();

                        _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Publish(bitmapSource);
                        continue;
                    }

                    StopStream();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Decoding loop cancelado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no stream RTSP: {ex.Message}");
            }
            finally
            {
                Dispose();
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
            _file?.Dispose();
            _file = null;
        }
    }
}
