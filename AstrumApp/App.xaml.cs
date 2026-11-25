using AstrumApp.Apps.Settings.Navigation;
using AstrumApp.AuthManager;
using AstrumApp.Security;
using AstrumApp.Services;
using AstrumApp.Services.Updates;
using AstrumApp.Session;
using AstrumApp.SettingsManager;
using DotNetEnv;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Velopack;
using Velopack.Sources;


namespace AstrumApp
{
    public partial class App : Application
    {
        public static UpdateService UpdateService { get; private set; } = null!;
        public static UpdateCoordinator Updates { get; private set; } = null!;

        public static UserSession Session { get; } = new();
        public static AccountManager AccountManager { get; } = new();
        public static NotificationService Notifications { get; } = new();

        public static SupabaseService Supabase { get; private set; } = new();

        public static NavigationService Navigation { get; } = new();

        public static SecurityManager Security { get; } = new();

        public static AppSettingsService SettingsService { get; } = new();


        public static void InitializeUpdates()
        {
            UpdateService = new UpdateService(GetSource());
            Updates = new UpdateCoordinator(UpdateService);
        }

        public static void Unlock()
        {
            if (Current.MainWindow is MainWindow main)
                main.Unlock();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Env.Load();

            if (SettingsService != null)
                await SettingsService.LoadAsync();
        }

        private static GithubSource GetSource()
        {
            return new GithubSource("https://github.com/Prollex325/AstrumApp",
                                    accessToken: null,
                                    prerelease: true
                                    );
        }
    }
}
