using AstrumApp.Services;
using AstrumApp.Session;
using AstrumApp.Security;
using System.Windows;
using DotNetEnv;


namespace AstrumApp
{
    public partial class App : Application
    {
        public static NotificationService Notifications { get; } = new();

        public static NavigationService Navigation { get; } = new();

        public static UserSession Session { get; } = new();

        public static SecurityManager Security { get; } = new();

        public static SupabaseService Supabase { get; private set; } = new();

        public static void LiftLockScreen()
        {
            if (Current.MainWindow is MainWindow main)
                main.LockScreen_Lifting();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Env.Load();

            await Supabase.InitializeAsync();

            //await Supabase.UserLogin();
        }
    }
}
