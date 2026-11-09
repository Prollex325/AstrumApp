using AstrumApp.AuthManager;
using AstrumApp.Security;
using AstrumApp.Services;
using AstrumApp.Session;
using AstrumApp.SettingsManager;
using DotNetEnv;
using System.Diagnostics;
using System.Windows;


namespace AstrumApp
{
    public partial class App : Application
    {
        public static NotificationService Notifications { get; } = new();

        public static UserSession Session { get; } = new();

        public static SupabaseService Supabase { get; private set; } = new();

        public static NavigationService Navigation { get; } = new();

        public static SecurityManager Security { get; } = new();

        public static AppSettingsService SettingsService { get; } = new();

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
    }
}
