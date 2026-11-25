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

            if (App.Updates.IsInstalled)
            {
                StartupView.BeginStep(StartupStep.Updates);
                UpdateStatus status = App.Updates.Status;

                if (status is not UpdateStatus.ReadyToInstall) { 
                    await App.Updates.CheckAsync(); 
                    status = App.Updates.Status;

                    switch (status)
                    {
                        case UpdateStatus.Available:
                            {
                                StartupView.CompleteStep(StartupStep.Updates);

                                var result = MessageBox.Show("Доступно обновление Astrum. Скачать?", "Обновление", MessageBoxButton.YesNo,
                                                             MessageBoxType.Info);

                                if (result != MessageBoxResult.Yes) break;

                                StartupView.BeginStep(StartupStep.Updates);
                                StartupView.SetCurrentStepText("Загрузка обновления Astrum");
                                StartupView.SetCurrentStepProgress(0);

                                Action progressHandler = () =>
                                {
                                    StartupView.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        StartupView.SetCurrentStepProgress(App.Updates.DownloadProgress);
                                    }));
                                };

                                App.Updates.ProgressChanged += progressHandler;

                                try
                                {
                                    await App.Updates.DownloadAsync();
                                }
                                finally
                                {
                                    App.Updates.ProgressChanged -= progressHandler;
                                }

                                if (App.Updates.Status != UpdateStatus.ReadyToInstall)
                                {
                                    StartupView.FailStep(StartupStep.Updates);

                                    MessageBox.Show(App.Updates.LastError ?? "Не удалось скачать обновление.", "Ошибка обновления",
                                                    MessageBoxButton.OK, MessageBoxType.Error);

                                    break;
                                }

                                StartupView.SetCurrentStepProgress(100);
                                StartupView.CompleteStep(StartupStep.Updates);

                                result = MessageBox.Show("Обновление скачано. Перезапустить Astrum для установки?", "Обновление",
                                                         MessageBoxButton.YesNo, MessageBoxType.Info);

                                if (result == MessageBoxResult.Yes)
                                {
                                    App.Updates.InstallAndRestart();
                                }
                                else
                                {
                                    StartupView.SetCurrentStepText("");
                                    StartupView.SetCurrentStepProgress(0);
                                }

                                break;
                            }
                        case UpdateStatus.UpToDate:
                            StartupView.CompleteStep(StartupStep.Updates);
                            break;
                        case UpdateStatus.Failed:
                            StartupView.FailStep(StartupStep.Updates);
                            break;
                        default:
                            StartupView.CompleteStep(StartupStep.Updates);
                            break;
                    }
                } else
                {
                    var result = MessageBox.Show("Обновление скачано. Перезапустить Astrum для установки?", "Обновление",
                                                         MessageBoxButton.YesNo, MessageBoxType.Info);

                    if (result == MessageBoxResult.Yes) App.Updates.InstallAndRestart();
                    StartupView.CompleteStep(StartupStep.Updates);
                }
            } else
            {
                StartupView.SkipStep(StartupStep.Updates);
            }


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
    }
}
