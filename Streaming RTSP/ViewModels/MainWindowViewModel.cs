using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Streaming_RTSP.Core.Events;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
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

        private bool _sharp = false;
        public bool Sharp
        {
            get => _sharp;
            set
            {
                if (SetProperty(ref _sharp, value))
                {
                    _rtspService.Sharp = value;
                }
            }
        }

        private bool _blur = false;
        public bool Blur
        {
            get => _blur;
            set
            {
                if (SetProperty(ref _blur, value))
                {
                    _rtspService.Blur = value;
                }
            }
        }

        private bool _grayscale = false;
        public bool Grayscale
        {
            get => _grayscale;
            set
            {
                if (SetProperty(ref _grayscale, value))
                {
                    _rtspService.Grayscale = value;
                }
            }
        }

        private bool _detectFace = false;
        public bool DetectFace 
        {
            get => _detectFace;
            set
            {
                if (SetProperty(ref _detectFace, value))
                {
                    _rtspService.DetectFace = value;
                }
            }
        }

        private string _rtspUrl = "rtsp://localhost:8554/stream";
        public string RtspUrl
        {
            get => _rtspUrl;
            set
            {
                if (SetProperty(ref _rtspUrl, value));
            }
        }

        private ObservableCollection<string> _capturedImagePaths;
        public ObservableCollection<string> CapturedImagePaths
        {
            get => _capturedImagePaths;
            set => SetProperty(ref _capturedImagePaths, value);
        }

        private WriteableBitmap writeableBitmap;

        public DelegateCommand StartStreamCommand { get; }
        public DelegateCommand StopStreamCommand { get; }
        public DelegateCommand TakeFrameCommand { get; }
        public DelegateCommand RefreshImagesCommand { get; }

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

            CapturedImagePaths = new ObservableCollection<string>();

            _eventAggregator.GetEvent<UpdateFrameViewerEvent>()
                .Subscribe(OnFrameReady, ThreadOption.UIThread);

            StartStreamCommand = new DelegateCommand(ExecuteStartStream);
            StopStreamCommand = new DelegateCommand(ExecuteStopStream);
            TakeFrameCommand = new DelegateCommand(ExecuteTakeFrame, CanExecuteTakeFrame);
            RefreshImagesCommand = new DelegateCommand(ExecuteRefreshImages);

            // Carrega as imagens salvas ao inicializar
            ExecuteRefreshImages();
        }

        private bool CanExecuteTakeFrame()
        {
            return ImageSource != null;
        }

        private void ExecuteTakeFrame()
        {
            _localImageService.SaveFrame(ImageSource, Core.Enums.ImageFormat.PNG);
            ExecuteRefreshImages();
        }

        private void ExecuteRefreshImages()
        {
            var imagePaths = _localImageService.GetSavedImagesPaths();
            CapturedImagePaths.Clear();
            foreach (var path in imagePaths)
            {
                CapturedImagePaths.Add(path);
            }
        }

        private void ExecuteStartStream()
        {
            //RtspUrl = "rtsp://localhost:8554/webcam";
            _rtspService.StartStream(RtspUrl);
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
