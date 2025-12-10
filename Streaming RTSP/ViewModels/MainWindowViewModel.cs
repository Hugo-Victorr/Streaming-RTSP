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

        private WriteableBitmap writeableBitmap;

        public DelegateCommand StartStreamCommand { get; }
        public DelegateCommand StopStreamCommand { get; }
        public DelegateCommand TakeFrameCommand { get; }

        private readonly IEventAggregator _eventAggregator;
        private readonly IRTSPStreamingService _rtspService;
        private readonly ILocalImageService _localImageService;

        public MainWindowViewModel(
            IEventAggregator eventAggreggator, 
            IRTSPStreamingService rtspService, 
            ILocalImageService localImageService)
        {
            _eventAggregator = eventAggreggator;
            _rtspService = rtspService;
            _localImageService = localImageService;



            _eventAggregator.GetEvent<UpdateFrameViewerEvent>()
                .Subscribe(OnFrameReady, ThreadOption.UIThread);

            StartStreamCommand = new DelegateCommand(ExecuteStartStream);
            StopStreamCommand = new DelegateCommand(ExecuteStopStream);
            TakeFrameCommand = new DelegateCommand(ExecuteTakeFrame, CanExecuteTakeFrame);
            _localImageService = localImageService;
        }

        private bool CanExecuteTakeFrame()
        {
            return ImageSource != null;
        }

        private void ExecuteTakeFrame()
        {
            _localImageService.SaveFrame(ImageSource, Core.Enums.ImageFormat.PNG);
        }

        private void ExecuteStartStream()
        {
            string rtspUrl = "rtsp://localhost:8554/stream";
            _rtspService.StartStream(rtspUrl);
        }

        private void ExecuteStopStream()
        {
            _rtspService.StopStream();
            ImageSource = null;
            TakeFrameCommand.RaiseCanExecuteChanged();
        }

        private void OnFrameReady(BitmapSource newFrame)
        {
            ImageSource = newFrame;
            TakeFrameCommand.RaiseCanExecuteChanged();
        }

        public void Dispose()
        {
            ImageSource = null;
            _eventAggregator.GetEvent<UpdateFrameViewerEvent>().Unsubscribe(OnFrameReady);
        }
    }
}
