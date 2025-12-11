using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Streaming_RTSP.Services;
using Streaming_RTSP.Services.Interfaces;
using Streaming_RTSP.ViewModels;
using Streaming_RTSP.Views;
using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;

namespace Streaming_RTSP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            FFMediaToolkit.FFmpegLoader.FFmpegPath = @"C:\ffmpeg\bin";
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            //containerRegistry.RegisterSingleton<IYunetDnnDetectorService>(
            //    () => new YunetDnnDetectorService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenCvFiles", "face_detection_yunet_2023mar.onnx")));
            containerRegistry.RegisterSingleton<IRTSPStreamingService, RTSPStreamingService>();
            containerRegistry.RegisterSingleton<ILocalImageService, LocalImageService>();


            containerRegistry.Register<MainWindowViewModel>();
        }
    }
}
