using AstrumApp.Apps.Authentication;
using AstrumApp.AuthManager;
using AstrumApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.XPath;


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
        private MainWindow window = null!;

        public StartupCoordinator(StartupView StartupViewControl, WelcomeView welcomeControl)
        {
            StartupView = StartupViewControl;
            WelcomeView = welcomeControl;

            StartupView.Visibility = System.Windows.Visibility.Visible;

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

        public async Task<bool> StartUpScenario()
        {
            if (StartupView == null || WelcomeView == null) return false;

            // ПОДГОТОВКА КЛИЕНТА

            StartupView.BeginStep(StartupStep.Supabase);

            bool done = supabase.PrepareClient();

            if (done)
                StartupView.CompleteStep(StartupStep.Supabase);
            else
                StartupView.FailStep(StartupStep.Supabase);


            // ПРОВЕРКА ПОДКЛЮЧЕНИЯ

            StartupView.BeginStep(StartupStep.Connection);

            bool connected = await App.Supabase.CheckConnectionAsync();

            if (connected)
                StartupView.CompleteStep(StartupStep.Connection);
            else
            {
                StartupView.ShowNotReady("Не удалось подключиться к Supabase. Проверьте интернет.");
                StartupView.FailStep(StartupStep.Connection);
                StartupView.SkipStep(StartupStep.Session);
                StartupView.SkipStep(StartupStep.Profile);

                await Task.Delay(1000);

                return false;
            }

            // ПРОВЕРКА ОБНОВЛЕНИЙ

            if (!await HandleUpdatesAsync())
                return false;


            // ПРОВЕРЯЕМ СУЩЕСТВУЮЩУЮ СЕССИЮ

            Supabase.Gotrue.User? user = null;
            StartupView.BeginStep(StartupStep.Session);

            user = await supabase.RestoreSessionAsync();

            if (user != null)
                StartupView.CompleteStep(StartupStep.Session);
            else
                StartupView.FailStep(StartupStep.Session);

            
            // ЕСЛИ ЕСТЬ ПОЛЬЗОВАТЕЛЬ ГРУЗИМ ПРОФИЛЬ

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


            // ГОТОВИМ ИНТЕРФЕЙС
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
                ObservableCollection<AccountCard> accounts = App.AccountManager.LoadAllCacheProfiles(false);

                if (accounts.Count < 1) window.NavigateToAuth(NavigationPage.Login, "Сессия истекла");
                else window.NavigateToAuth(NavigationPage.SelectProfile, "");
                await StartupView.RevealAsync();
                window.CloseStartupView();
            } else
            {
                window.NavigateToWelcome();
                await StartupView.RevealIntoStarAsync();
                window.CloseStartupView();
                await WelcomeView.PlayIntroAsync();
            }

            return true;
        }

        private async Task<bool> HandleUpdatesAsync()
        {
            if (StartupView == null) return false;

            if (!App.Updates.IsInstalled)
            {
                StartupView.SkipStep(StartupStep.Updates);
                return true;
            }

            StartupView.BeginStep(StartupStep.Updates);

            try
            {
                await App.Updates.RefreshPolicyAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                StartupView.FailStep(StartupStep.Updates);
                StartupView.ShowNotReady("Не удалось проверить требования к версии");
                return false;
            }

            bool pendingIsOutdated = 
                App.Updates.PendingVersion is { } pending && 
                App.Updates.MinimumSupportedVersion is { } minimum && 
                pending.CompareTo(minimum) < 0;

            if (pendingIsOutdated)
            {
                await App.Updates.CheckAsync(force: true);

                if (App.Updates.Status == UpdateStatus.Available &&
                    App.Updates.AvailableUpdate is { } update &&
                    App.Updates.MinimumSupportedVersion is { } min &&
                    update.TargetFullRelease.Version.CompareTo(min) >= 0)
                {
                    bool downloaded = await DownloadUpdateAsync();

                    if (downloaded &&
                        App.Updates.PendingVersion is { } newVersion &&
                        newVersion.CompareTo(min) >= 0)
                    {
                        return ConfirmUpdateInstallation();
                    }
                }

                StartupView.FailStep(StartupStep.Updates);
                StartupView.ShowNotReady("Не удалось получить поддерживаемое обновление");
                return false;
            }

            if (App.Updates.Status != UpdateStatus.ReadyToInstall)
                await App.Updates.CheckAsync();

            if (App.Updates.Status == UpdateStatus.ReadyToInstall)
                return ConfirmUpdateInstallation();

            if (App.Updates.Status == UpdateStatus.Available)
            {
                var result = MessageBox.Show(
                    App.Updates.IsRequired
                        ? "Доступно обязательное обновление Astrum. Скачать?"
                        : "Доступно обновление Astrum. Скачать?",
                    "Обновление",
                    MessageBoxButton.YesNo,
                    App.Updates.IsRequired
                        ? MessageBoxType.Warning
                        : MessageBoxType.Info
                );

                if (result == MessageBoxResult.Yes)
                {
                    bool downloaded = await DownloadUpdateAsync();

                    if (downloaded)
                        return ConfirmUpdateInstallation();

                    MessageBox.Show(
                        App.Updates.LastError ?? "Не удалось скачать обновление.",
                        "Ошибка обновления",
                        MessageBoxButton.OK,
                        MessageBoxType.Error
                    );
                }
            }

            if (App.Updates.IsRequired)
            {
                StartupView.FailStep(StartupStep.Updates);
                StartupView.ShowNotReady("Обновление обязательно для запуска");
                return false;
            }

            if (App.Updates.Status == UpdateStatus.Failed)
                StartupView.FailStep(StartupStep.Updates);
            else
                StartupView.CompleteStep(StartupStep.Updates);

            return true;
        }

        private async Task<bool> DownloadUpdateAsync()
        {
            if (StartupView == null) return false;

            StartupView.SetCurrentStepText("Загрузка обновления Astrum");
            StartupView.SetCurrentStepProgress(0);

            Action progressHandler = () =>
            {
                StartupView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    StartupView.SetCurrentStepProgress(
                        App.Updates.DownloadProgress
                    );
                }));
            };

            App.Updates.ProgressChanged += progressHandler;

            try
            {
                await App.Updates.DownloadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            finally
            {
                App.Updates.ProgressChanged -= progressHandler;
            }

            return App.Updates.Status == UpdateStatus.ReadyToInstall;
        }

        private bool ConfirmUpdateInstallation()
        {
            if (StartupView == null) return false;

            bool required = App.Updates.IsRequired;

            var result = MessageBox.Show(
                required
                    ? "Обязательное обновление загружено.\nУстановить сейчас?"
                    : "Обновление загружено.\nПерезапустить Astrum для установки?",
                "Обновление",
                MessageBoxButton.YesNo,
                required
                    ? MessageBoxType.Warning
                    : MessageBoxType.Info
            );

            if (result == MessageBoxResult.Yes)
            {
                App.Updates.InstallAndRestart();
                return false;
            }

            if (required)
            {
                StartupView.FailStep(StartupStep.Updates);
                StartupView.ShowNotReady("Обновление обязательно для запуска");
                return false;
            }

            StartupView.CompleteStep(StartupStep.Updates);
            return true;
        }
    }
}
