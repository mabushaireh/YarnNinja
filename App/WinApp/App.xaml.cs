using Microsoft.UI.Xaml;
using System;
using System.IO;
using Windows.Storage;
using YarnNinja.App.WinApp.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace YarnNinja.App.WinApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Shell shell;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            

        }

        public INavigation Navigation => shell;

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            shell = new Shell();

            if (arguments.Length > 1)
            {
                var path = arguments[1];

                if (!File.Exists(path))
                {
                }
                else
                {
                    shell.OpenYarnAppFile(path);
                }
            }

            shell.Activate();

            WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(shell);
            PInvoke.User32.ShowWindow(WindowHandle, PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
        }

        public static IntPtr WindowHandle { get; private set; }
    }

}
