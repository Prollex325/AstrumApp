using AstrumApp.Services;
using AstrumApp.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AstrumApp.Apps.Settings.MenuControls
{//TODO : улучшить стили
    public partial class UpdatesPage : UserControl
    {
        private string version = App.UpdateService.CurrentVersion?.ToString() ?? 
                                    typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?
                                               .Split('+')[0] ?? "0.0.0";
        private bool _starSpinning = false;
        private UpdateStatus? _lastVisualStatus;
        private int _easterEggClicks = 0;

        public UpdatesPage()
        {
            InitializeComponent();

            Action progressHandler = OnProgressChanged;

            Loaded += async (s, e) =>
            {
                CurrentVersionText.Text = $"Версия {version}";

                App.Updates.StatusChanged += OnStatusChanged;
                App.Updates.ProgressChanged += progressHandler;

                UpdateVisualState();

                if (App.Updates.Status == UpdateStatus.Idle)
                    await App.Updates.CheckAsync();
            };

            Unloaded += (s, e) =>
            {
                App.Updates.StatusChanged -= OnStatusChanged;
                App.Updates.ProgressChanged -= progressHandler;
            };

            LoadChangeLog();
        }

        private void SetData(string resource, string update, string buttonText)
        {
            UpdateStatusIndicator.Fill = (Brush)FindResource(resource);

            UpdateStatusText.Text = update;

            ActionButton.Content = buttonText;
        }


        private void InstallUpdate_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Для установки обнолвние приложение необходимо перезапустить.", "Обновление",
                                                        MessageBoxButton.YesNo, MessageBoxType.Info);

            if (result == MessageBoxResult.Yes) App.Updates.InstallAndRestart();
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            switch (App.Updates.Status)
            {
                case UpdateStatus.Idle:
                case UpdateStatus.UpToDate:
                case UpdateStatus.Failed:
                    await App.Updates.CheckAsync();
                    break;

                case UpdateStatus.Available:
                    await App.Updates.DownloadAsync();
                    break;

                case UpdateStatus.ReadyToInstall:
                    InstallUpdate_Click(sender, e);
                    break;
            }

            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            UpdateStatus status = App.Updates.Status;
            ActionButton.IsEnabled = status is not (UpdateStatus.Checking or UpdateStatus.Downloading or UpdateStatus.Installing);
            string version = App.Updates.AvailableUpdate?.TargetFullRelease.Version.ToString() ?? "неизвестна";

            bool checking = status == UpdateStatus.Checking;
            bool downloading = status == UpdateStatus.Downloading;

            ProgressStack.Visibility =
                checking || downloading
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            UpdateProgressBar.IsIndeterminate = checking;

            ProgressBarPercents.Visibility = downloading ? Visibility.Visible : Visibility.Collapsed;

            bool shouldSpin = status is UpdateStatus.Checking or UpdateStatus.Downloading or UpdateStatus.Installing;

            if (shouldSpin && !_starSpinning)
            {
                StartStarRotation();
                _starSpinning = true;
            }
            else if (!shouldSpin && _starSpinning)
            {
                StopStarRotation();
                _starSpinning = false;
            }

            switch (status)
            {
                case UpdateStatus.Idle:
                    SetData("UpdateIdleBrush", "Обновления ещё не проверялись", "Проверить");
                    break;
                case UpdateStatus.Checking:
                    SetData("UpdateCheckingBrush", "Проверка...", "Проверка...");
                    break;
                case UpdateStatus.UpToDate:
                    SetData("UpdateUpToDateBrush", "Установлена актуальная версия", "Проверить");
                    break;
                case UpdateStatus.Available:
                    SetData("UpdateAvailableBrush", $"Доступна новая версия {version}", "Обновить");
                    break;
                case UpdateStatus.Downloading:
                    if (_lastVisualStatus != UpdateStatus.Downloading)
                    {
                        UpdateProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
                        UpdateProgressBar.Value = 0;
                        ProgressBarPercents.Text = "0%";
                    }
                    SetData("UpdateDownloadingBrush", "Загрузка обновления...", "Скачивание...");
                    break;
                case UpdateStatus.ReadyToInstall:
                    SetData("UpdateReadyBrush", "Обновление готово к установке", "Установить");
                    break;
                case UpdateStatus.Installing:
                    SetData("UpdateInstallingBrush", "Установка обновления...", "Установка...");
                    break;
                case UpdateStatus.Failed:
                    SetData("UpdateFailedBrush", "Не удалось проверить обновления", "Повторить");
                    break;
            }

            _lastVisualStatus = status;
        }

        private void StartStarRotation()
        {
            double angle = StarRotation.Angle;

            var animation = new DoubleAnimation
            {
                From = angle,
                To = angle + 360,
                Duration = TimeSpan.FromSeconds(1.6),
                RepeatBehavior = RepeatBehavior.Forever
            };

            StarRotation.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void StopStarRotation()
        {
            double currentAngle = StarRotation.Angle;

            double targetAngle = Math.Ceiling(currentAngle / 90.0) * 90;

            var animation = new DoubleAnimation
            {
                From = currentAngle,
                To = targetAngle,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            StarRotation.BeginAnimation( RotateTransform.AngleProperty, animation);
        }

        private void OnStatusChanged()
        {
            Dispatcher.InvokeAsync(UpdateVisualState);
        }

        private void OnProgressChanged()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int progress = App.Updates.DownloadProgress;

                ProgressBarPercents.Text = $"{progress}%";

                double current = UpdateProgressBar.Value;

                UpdateProgressBar.Value = progress;

                var animation = new DoubleAnimation
                {
                    From = current,
                    To = progress,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase
                    {
                        EasingMode = EasingMode.EaseOut
                    },
                    FillBehavior = FillBehavior.Stop
                };

                UpdateProgressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
            }));
        }

        private void LoadChangeLog()
        {
            string path = AppPaths.ChangelogPath;

            if (File.Exists(path))
            {
                ChangelogViewer.Markdown = File.ReadAllText(path);

                if (ChangelogViewer.Document is { } document)
                {
                    foreach (var list in document.Blocks.OfType<List>())
                    {
                        list.MarkerStyle = TextMarkerStyle.Disc;
                        list.MarkerOffset = 10;
                    }
                }
            }
            else
            {
                ChangelogViewer.Markdown = "# История обновлений\n\nФайл не найден.";
            }
        }

        private void StarEasterEgg_Click(object sender, MouseButtonEventArgs e)
        {
            _easterEggClicks++;

            if (_easterEggClicks < 5) return;

            _easterEggClicks = 0;

            MessageBox.Show(
                "Говорят, зимой звёзды светят немного иначе...\n" +
                "Возможно, Astrum скоро это докажет. ❄️",
                "Маленькая тайна Astrum",
                MessageBoxButton.OK,
                MessageBoxType.Info
            );

            e.Handled = true;
        }
    }
}
