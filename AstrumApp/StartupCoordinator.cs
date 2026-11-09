using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AstrumApp.AuthManager;
using AstrumApp.Views;
using AstrumApp.Apps.Authentication;


namespace AstrumApp
{
    public enum StartupDestination
    {
        Main,
        Auth,
        Welcome
    }
    public class StartupCoordinator
    {
        private SupabaseService supabase = App.Supabase;
        private StartupView? StartupView;
        private WelcomeView? WelcomeView;
        private MainWindow window;

        public StartupCoordinator(StartupView StartupViewControl, WelcomeView welcomeControl)
        {
            StartupView = StartupViewControl;
            WelcomeView = welcomeControl;

            if (App.Current.MainWindow is MainWindow window1) 
            {
                window = window1;
            }
            else return;

            WelcomeView.StartRegistrationRequested += (_, _) =>
            {
                window.NavigateToAuth(NavigationPage.Registration);
            };

            WelcomeView.SignInRequested += (_, _) =>
            {
                window.NavigateToAuth(NavigationPage.Login);
            };

        }

        public async Task StartUpScenario()
        {
            if (StartupView == null || WelcomeView == null) return;

            StartupView.BeginStep(StartupStep.Supabase);

            bool done = supabase.PrepareClient();

            if (done)
                StartupView.CompleteStep(StartupStep.Supabase);
            else
                StartupView.FailStep(StartupStep.Supabase);


            // Восстанавливаем сессию
            StartupView.BeginStep(StartupStep.Session);

            var user = await supabase.RestoreSessionAsync();

            if (user != null)
                StartupView.CompleteStep(StartupStep.Session);
            else
                StartupView.FailStep(StartupStep.Session);


            // Загружаем профиль только при наличии пользователя
            if (user != null)
            {
                StartupView.BeginStep(StartupStep.Profile);

                await supabase.ApplyUserAsync(user);

                StartupView.CompleteStep(StartupStep.Profile);
            }
            else
            {
                App.Session.Clear();

                StartupView.SkipStep(StartupStep.Profile);
            }


            // Подготовка интерфейса
            StartupView.BeginStep(StartupStep.Interface);

            await Task.Delay(150);
            var destination = user != null ? StartupDestination.Main : StartupDestination.Auth;

            const int CurrentOnboardingVersion = 1;

            if (App.SettingsService.Settings.OnboardingVersion < CurrentOnboardingVersion)
            {
                destination = StartupDestination.Welcome;
            }

            StartupView.CompleteStep(StartupStep.Interface);
            StartupView.ShowReady();

            await Task.Delay(300);

            await StartupView.CoverWithFlashAsync();

            if (destination == StartupDestination.Main)
            {
                window.NavigateToMain();

                await StartupView.RevealAsync();
                window.CloseStartupView();
            }
            else if (destination == StartupDestination.Auth)
            {
                window.NavigateToAuth(NavigationPage.Login, "Сессия истекла");
                await StartupView.RevealAsync();
                window.CloseStartupView();
            } else
            {
                window.NavigateToWelcome();
                await StartupView.RevealIntoStarAsync();
                window.CloseStartupView();
                await WelcomeView.PlayIntroAsync();
            }
        }
    }
}
