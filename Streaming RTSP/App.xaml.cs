using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Streaming_RTSP.Services;
using Streaming_RTSP.Services.Interfaces;
using Streaming_RTSP.Views;
using System.Globalization;
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
            FFMediaToolkit.FFmpegLoader.FFmpegPath = "ffmpeg";
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IRTSPStreamingService, RTSPStreamingService>();
        }
    }
}
