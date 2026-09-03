using AstrumApp.Apps.Settings;
using AstrumApp.Apps.Settings.MenuControls;
using AstrumApp.Apps.Settings.MenuControls.Security;
using AstrumApp.Controls;
using AstrumApp.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Services
{
    public enum NavigationPage
    {
        NotFoundPage,
        Profile,
        ProfileEdit,
        SystemInfo,
        AvatarEdit,
        Security,

        PinSettingsPage,
        CreatePinPage,
        ChangePinPage,
        DeletePinPage
    }

    public static class NavigationPageExtensions
    {
        public static string GetTitle(this NavigationPage page)
        {
            return page switch
            {
                NavigationPage.Profile => "Профиль",
                NavigationPage.ProfileEdit => "Редактирование профиля",
                NavigationPage.SystemInfo => "О системе",
                NavigationPage.AvatarEdit => "Аватар",
                NavigationPage.Security => "Безопасность",
                NavigationPage.PinSettingsPage => "Настройки PIN",
                NavigationPage.CreatePinPage => "Создание PIN",
                NavigationPage.ChangePinPage => "Изменение PIN",
                NavigationPage.DeletePinPage => "Удаление PIN",
                NavigationPage.NotFoundPage => "Страница не найдена",

                _ => "Неизвестно"
            };
        }
    }

    public class NavigationService
    {
        private ContentControl _host;

        private readonly Dictionary<NavigationPage, UserControl> _pages = new();

        private readonly List<NavigationPage> _backHistory = [];
        private readonly List<NavigationPage> _forwardHistory = [];

        public event Action<List<NavigationPage>>? NavigationChanged;

        public NavigationPage CurrentPage { get; private set; } = NavigationPage.NotFoundPage;

        public void Initialize(ContentControl host)
        {
            _host = host;

            RegisterPages();
        }

        private void RegisterPages()
        {
            _pages[NavigationPage.Profile] = new ProfilePage();
            _pages[NavigationPage.ProfileEdit] = new ProfileEdit();
            _pages[NavigationPage.SystemInfo] = new SystemInfo();
            _pages[NavigationPage.AvatarEdit] = new AvatarEdit();
            _pages[NavigationPage.Security] = new SecurityPage();
            _pages[NavigationPage.PinSettingsPage] = new PinSettingsPage();
            _pages[NavigationPage.CreatePinPage] = new CreatePinPage();
            _pages[NavigationPage.ChangePinPage] = new ChangePinPage();
            _pages[NavigationPage.DeletePinPage] = new DeletePinPage();

            _pages[NavigationPage.NotFoundPage] = new NotFoundPage();
        }

        public void BreadcrumbNavigate(NavigationPage page)
        {
            if (CurrentPage == page) 
                return;

            bool canClose = true;

            if (_pages[CurrentPage] is ICanClose closable) {
                canClose = closable.CanClose();
            }

            if (canClose)
            {
                int index = _backHistory.IndexOf(page);

                if (index != -1)
                {
                    int countToRemove = _backHistory.Count - index;
                    _backHistory.RemoveRange(index, countToRemove);
                }
                _forwardHistory.Add(CurrentPage);

                CurrentPage = page;

                SetPage(_pages[page], "Right");

                SendHistory();
            }
        }

        public void NavigateMenu(NavigationPage page)
        {
            if (CurrentPage == page)
                return;
            CurrentPage = page;

            _backHistory.Clear();
            _forwardHistory.Clear();

            SetMenu(_pages[page]);

            SendHistory();
        }

        public void NavigatePage(NavigationPage page)
        {
            if (CurrentPage == page) return;

            _backHistory.Add(CurrentPage);
            _forwardHistory.Clear();

            CurrentPage = page;

            SetPage(_pages[page], "Left");

            SendHistory();
        }

        public void Back()
        {
            if (_backHistory.Count == 0)
                return;

            if (_pages[CurrentPage] is ICanClose closable && !closable.CanClose())
                return;

            var previous = _backHistory[^1];

            _backHistory.RemoveAt(_backHistory.Count - 1);
            _forwardHistory.Add(CurrentPage);

            CurrentPage = previous;

            SetPage(_pages[CurrentPage], "Right");

            SendHistory();
        }

        public void Forward()
        {
            if (_forwardHistory.Count == 0) return;

            var next = _forwardHistory.Last();
            var current = _pages[CurrentPage];

            if (current is ProfileEdit && _pages[next] is AvatarEdit)
            {
                if (current is ICanClose closable)
                {
                    if (!closable.CanClose()) return;
                }
            }

            _forwardHistory.RemoveAt(_forwardHistory.Count - 1);
            _backHistory.Add(CurrentPage);

            CurrentPage = next;

            SetPage(_pages[next], "Left");

            SendHistory();
        }

        public List<NavigationPage> SendHistory()
        {
            var history = _backHistory.ToList();
            history.Add(CurrentPage);
            NavigationChanged?.Invoke(history);
            return history;
        }

        private void SetMenu(UserControl page)
        {
            AnimateMenu(page);

            if (CheckProfileEdititng(page))
            {
                App.Session.Profile.Cancel();
                App.Notifications.Show("Настройки отменены!", CardTypes.Error);
            }
        }

        private async Task SetPage(UserControl page, String direction)
        {
            AnimatePage(page, direction);

            if (CheckProfileEdititng(page))
            {
                App.Session.Profile.Cancel();
                App.Notifications.Show("Настройки отменены!", CardTypes.Error);
            }
        }

        private void AnimateMenu(UserControl page)
        {
            _host.RenderTransform = new ScaleTransform(0.98, 0.98);
            _host.Opacity = 0;

            _host.Content = page;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            var scale = new DoubleAnimation(0.98, 1, TimeSpan.FromMilliseconds(150));

            scale.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };

            fade.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };

            _host.BeginAnimation(UIElement.OpacityProperty, fade);
            ((ScaleTransform)_host.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleXProperty, scale);
            ((ScaleTransform)_host.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleYProperty, scale);
        }

        private async void AnimatePage(UserControl page, String direction)
        {
            int to = direction switch
            {
                "Left" => -20,
                "Right" => 60,
                _ => 0
            };
            int from = direction switch
            {
                "Left" => 60,
                "Right" => -20,
                _ => 0
            };
            var oldTransform = new TranslateTransform();

            _host.RenderTransform = oldTransform;

            // Уход текущей страницы
            var hideAnimation = new DoubleAnimation(0, to,
                TimeSpan.FromMilliseconds(150));

            hideAnimation.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseIn
            };

            oldTransform.BeginAnimation(
                TranslateTransform.XProperty,
                hideAnimation);

            var fadeOut = new DoubleAnimation(1, 0,
                TimeSpan.FromMilliseconds(150));

            _host.BeginAnimation(
                UIElement.OpacityProperty,
                fadeOut);

            await Task.Delay(130);

            // Меняем страницу
            _host.Content = page;

            // Начальная позиция новой
            var newTransform = new TranslateTransform();

            _host.RenderTransform = newTransform;

            _host.Opacity = 0;

            // Появление новой страницы
            var showAnimation = new DoubleAnimation(from, 0,
                TimeSpan.FromMilliseconds(180));

            showAnimation.EasingFunction = new QuarticEase
            {
                EasingMode = EasingMode.EaseOut
            };

            var fadeIn = new DoubleAnimation(0, 1,
                TimeSpan.FromMilliseconds(180));

            newTransform.BeginAnimation(
                TranslateTransform.XProperty,
                showAnimation);

            _host.BeginAnimation(
                UIElement.OpacityProperty,
                fadeIn);
        }

        private bool CheckProfileEdititng(UserControl newPage)
        {
            if (newPage != null) {
                if (!(newPage is ProfileEdit || newPage is AvatarEdit) && App.Session.Profile.IsEditing() && App.Session.Profile.HasChanges) {
                    return true;
                }
            }
            return false;
        }
    }
}
