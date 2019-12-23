using System;
using Tono.Gui.Uwp;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JitStreamDesigner
{
    public sealed partial class App : Application
    {
        public App()
        {
            Mes.ChangeLanguage(Mes.En); // Tono.Uwp.Mes language initial setting

            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override void OnFileActivated(FileActivatedEventArgs e)
        {
            base.OnFileActivated(e);

            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    OnResuming();
                }
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), null);
            }
            Window.Current.Activate();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    OnResuming();
                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Backup application data here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {

            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        /// <summary>
        /// Restore application data from backup of suspended timing
        /// </summary>
        private void OnResuming()
        {
        }
    }
}
