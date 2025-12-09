using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Streaming_RTSP.Core.Events;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Windows.Media.Imaging;

namespace Streaming_RTSP.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        public string Title => "RTSP Streaming Viewer";

        private BitmapSource _imageSource;
        public BitmapSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public DelegateCommand StartStreamCommand { get; }

        private readonly IRTSPStreamingService _rtspService;
        private readonly IEventAggregator _eventAggregator;

        public MainWindowViewModel(IEventAggregator eventAggreggator, IRTSPStreamingService rtspService)
        {
            _rtspService = rtspService;
            _eventAggregator = eventAggreggator;

            _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Subscribe(OnFrameReady);

            StartStreamCommand = new DelegateCommand(ExecuteStartStream);
        }

        private void ExecuteStartStream()
        {
            string rtspUrl = "rtsp://localhost:8554/stream";
            _rtspService.StartStream(rtspUrl);
        }

        private void OnFrameReady(BitmapSource newFrame)
        {
            ImageSource = newFrame;
        }

        public void Dispose()
        {
            ImageSource = null;
            _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Unsubscribe(OnFrameReady);
        }
    }
}
