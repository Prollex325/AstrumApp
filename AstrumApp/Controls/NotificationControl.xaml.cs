using AstrumApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace AstrumApp.Controls
{
    public partial class NotificationControl : UserControl
    {
        private readonly NotificationService notificationService = App.Notifications;
        private readonly NotificationAnimator _animator;

        public NotificationControl()
        {
            InitializeComponent();

            _animator = new NotificationAnimator(NotificationItemsControl);
            DataContext = notificationService;

            notificationService.BeforeNotificationChanged += () =>
            {
                _animator.Capture();
            }; 

        }

        private void Notification_Loaded(object sender, RoutedEventArgs e)
        {
            Border notification = (Border)sender;

            _animator.Register((NotificationCard)notification.DataContext, notification);

            PlayOnNextLayout(true);

            Animation(notification, true);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border notification = (Border)sender;

            Animation(notification, false);
        }

        private async void Animation(Border notification, bool isAppearing)
        {
            var transform = GetUnfrozen((TranslateTransform)notification.RenderTransform);
            notification.RenderTransform = transform;

            DoubleAnimation animation = new DoubleAnimation
            {
                To = isAppearing ? 0 : 250,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            if (isAppearing)
            {
                animation.BeginTime = TimeSpan.FromMilliseconds(180); // чтоб карточки не наезжали друг на друга
                animation.Completed += (s, a) =>
                {
                    NotificationTimer(notification);
                };
            }

            transform.BeginAnimation(TranslateTransform.XProperty, animation);

            if (!isAppearing)
            {
                animation.Completed -= (s, a) =>
                {
                    notificationService.Remove((NotificationCard)notification.DataContext);
                };

                await Task.Delay(60);
                _animator.Capture();
                BorderParentAnimation(notification);
            }
        }
        private void PlayOnNextLayout(bool isUpping)
        {
            void Handler(object? s, EventArgs e)
            {
                NotificationItemsControl.LayoutUpdated -= Handler;
                _animator.Play(isUpping);
            }

            NotificationItemsControl.LayoutUpdated += Handler;
        }

        private void BorderParentAnimation(Border notification)
        {
            Grid container = (Grid)notification.Parent;
            NotificationCard card = (NotificationCard)notification.DataContext;

            double currentHeight = container.ActualHeight;

            container.Margin = new Thickness(0, -currentHeight, 0, 0);

            _animator.Unregister(card);

            PlayOnNextLayout(false);

        }

        private async void NotificationTimer(Border notification)
        {
            await Task.Delay(3000);
            Animation(notification, false);
        }
        private static T GetUnfrozen<T>(T obj) where T : Freezable
        {
            return obj.IsFrozen ? (T)obj.Clone() : obj;
        }
        private static T? FindVisualChild<T>(DependencyObject parent)
                where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                T? descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }
    }
}
