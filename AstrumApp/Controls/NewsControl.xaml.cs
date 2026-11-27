using AstrumApp.Models;
using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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

namespace AstrumApp.Controls
{
    public partial class NewsControl : UserControl
    {
        private static readonly HttpClient _httpClient = new();
        public event Action<ActionTypes, string>? ActionRequested;
        List<Task> imageTasks = new();
        private bool _isLoading = false;
        public NewsControl()
        {
            InitializeComponent();

            //Loaded += UpdateNews;

            RefreshNewsButton.Visibility = Visibility.Collapsed;
            NewsLoadingIndicator.Visibility = Visibility.Visible;
        }

        public async void UpdateNews(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            _isLoading = true;

            NewsLoadingIndicator.Visibility = Visibility.Visible;
            RefreshNewsButton.Visibility = Visibility.Collapsed;

            try
            {
                var announcements = await App.Supabase.LoadAnnouncementsAsync();

                announcements = announcements
                    .OrderByDescending(x => x.IsPinned)
                    .ThenByDescending(x => x.PublishedAt)
                    .ToList();

                NewsContainer.Children.Clear();


                foreach (Announcement announcement in announcements)
                {
                    AnnouncementCard announcementCard = new()
                    {
                        Title = announcement.Title,
                        Description = announcement.Description ?? string.Empty,
                        PublishedAt = announcement.PublishedAt,
                        ActionText = announcement.ActionText ?? string.Empty,
                        ActionParameter = announcement.ActionParameter ?? string.Empty
                    };

                    if (Enum.TryParse<AnnouncementCategory>(announcement.Category, true, out var category))
                    {
                        announcementCard.Category = category;
                    }

                    if (Enum.TryParse<ActionTypes>(announcement.ActionType.Replace("_", ""), true, out var actionType))
                    {
                        announcementCard.ActionType = actionType;
                    }

                    NewsContainer.Children.Add(announcementCard);

                    if (!string.IsNullOrWhiteSpace(announcement.ImagePath))
                    {
                        imageTasks.Add(LoadImage(announcement.ImagePath, announcementCard));
                    }

                    announcementCard.ActionRequested += (type, parameter) =>
                    {
                        ActionRequested?.Invoke(type, parameter);
                    };
                }

                await Task.WhenAll(imageTasks);

                NewsErrorState.Visibility = Visibility.Collapsed;
                EmptyNewsState.Visibility = NewsContainer.Children.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                NewsContainer.Visibility = NewsContainer.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            } catch (Exception ex)
            {
                EmptyNewsState.Visibility = Visibility.Collapsed;
                NewsContainer.Visibility = Visibility.Collapsed;
                NewsErrorState.Visibility = Visibility.Visible;
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                NewsLoadingIndicator.Visibility = Visibility.Collapsed;
                RefreshNewsButton.Visibility = Visibility.Visible;
                _isLoading = false; 
            }
        }

        private async Task LoadImage(string imagePath, AnnouncementCard card)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.AnnouncementsCache);

                string localPath = Path.Combine(
                    AppPaths.AnnouncementsCache,
                    Path.GetFileName(imagePath)
                );

                ImageSource? imageSource = null;

                if (File.Exists(localPath))
                {
                    imageSource = FrozenImage.CreateFromUri(localPath);
                }

                if (imageSource == null)
                {
                    imageSource = await LoadImageAsync(imagePath, localPath);
                }

                card.ImageSource = imageSource;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        private async Task<ImageSource?> LoadImageAsync(string imagePath, string localPath)
        {
            var url = App.Supabase.GetAnnouncementImageUrl(imagePath);

            if (url == null) return null;

            byte[] bytes = await _httpClient.GetByteArrayAsync(url);

            ImageSource? image = FrozenImage.CreateFromBytes(bytes);

            if (image == null) return null;

            await File.WriteAllBytesAsync(localPath, bytes);

            return image;
        }

        private void Trigger_MouseEnter(object sender, MouseEventArgs e)
        {
            OpenNewsPanel();
        }

        private async void Trigger_MouseLeave(object sender, MouseEventArgs e)
        {
            await Task.Delay(200);

            if (NewsPanel.IsMouseOver || Trigger.IsMouseOver) return;

            CloseNewsPanel();
        }

        private async void NewsControl_MouseLeave(object sender, MouseEventArgs e)
        {
            await Task.Delay(200);

            if (NewsPanel.IsMouseOver || Trigger.IsMouseOver) return;

            CloseNewsPanel();
        }

        private async void NewsControl_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void OpenNewsPanel()
        {
            var transform = NewsPanel.RenderTransform as TranslateTransform;

            DoubleAnimation animation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            transform?.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CloseNewsPanel()
        {
            var transform = NewsPanel.RenderTransform as TranslateTransform;

            DoubleAnimation animation = new DoubleAnimation
            {
                To = 470,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            transform?.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void NewsScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            scrollViewer.ApplyTemplate();

            if (scrollViewer.Template.FindName(
                "PART_ScrollContentPresenter", scrollViewer)
                is ScrollContentPresenter presenter)
            {
                presenter.OpacityMask = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),

                    GradientStops = new GradientStopCollection
            {
                new(Colors.Transparent, 0),
                new(Colors.White, 0.025),
                new(Colors.White, 0.975),
                new(Colors.Transparent, 1)
            }
                };
            }
        }
    }
}
