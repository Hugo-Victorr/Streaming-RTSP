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
        private WriteableBitmap _writeableBitmap;

        private readonly IEventAggregator _eventAggregator;

        public RTSPStreamingService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        private void Initialize(string rtspUrl)
        {
            _rtspUrl = rtspUrl;
            _file = MediaFile.Open(@$"{_rtspUrl}", new MediaOptions() { VideoPixelFormat = ImagePixelFormat.Bgra32 });
            _writeableBitmap = new WriteableBitmap(_file.Video.Info.FrameSize.Width, _file.Video.Info.FrameSize.Height, 96, 96, PixelFormats.Bgra32, null);
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
            _decodingTask = Task.Run(() => DecodingLoopAsync(_cts.Token), _cts.Token);
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

        /// <summary>
        /// Decodifica o frame da transmissão da midia.
        /// </summary>
        private async Task DecodingLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _writeableBitmap.Lock();
                        var success = _file.Video.TryGetNextFrame(_writeableBitmap.BackBuffer, _writeableBitmap.BackBufferStride);
                        if (success)
                            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _file.Video.Info.FrameSize.Width, _file.Video.Info.FrameSize.Height));

                        _writeableBitmap.Unlock();

                        if (success)
                            _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Publish(_writeableBitmap);
                        else
                            StopStream();
                    });

                    await Task.Delay(32, ct);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Decoding loop cancelado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no stream RTSP: {ex.Message} - {ex.InnerException} - StackTrace: {ex.StackTrace}");
            }
        }

        public void Dispose(Task completedTask)
        {
            _file?.Dispose();
            _file = null;

            _decodingTask = null;

            _writeableBitmap = null;

            _cts?.Dispose();
        }
    }
}
