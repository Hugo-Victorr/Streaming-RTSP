using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Streaming_RTSP.Views;
using System.Windows;

namespace Streaming_RTSP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<INLogService, NLogService>();
            //containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
        }
    }
}
