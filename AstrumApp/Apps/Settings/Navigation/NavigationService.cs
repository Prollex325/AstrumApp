using AstrumApp.Apps.Settings;
using AstrumApp.Apps.Settings.MenuControls;
using AstrumApp.Apps.Settings.MenuControls.ProfileOptions;
using AstrumApp.Apps.Settings.MenuControls.Security;
using AstrumApp.Controls;
using AstrumApp.Interfaces;
using AstrumApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AstrumApp.Apps.Settings.Navigation
{
    public enum NavigationPage
    {
        None,
        Profile,
        ProfileEdit,
        AvatarEdit,
        SwitchProfilePage,
        DeleteProfilePage,

        Security,
        PinSettingsPage,
        CreatePinPage,
        ChangePinPage,
        DeletePinPage,
        ChangePasswordPage,

        UpdatesPage,

        SystemInfo,

        NotFoundPage
    }

    public record NavigationNode(NavigationPage? Parent, string Title);

    public static class NavigationPageExtensions
    {
        public static string GetTitle(this NavigationPage page)
        {
            return App.Navigation.GetPageTitle(page);
        }
    }

    public class NavigationService
    {
        private ContentControl _host;

        private readonly Dictionary<NavigationPage, UserControl> _pages = new();

        private readonly Dictionary<NavigationPage, NavigationNode> _tree = new()
        {
            [NavigationPage.Profile] = new(null, "Профиль"),
            [NavigationPage.ProfileEdit] = new(NavigationPage.Profile, "Редактирование профиля"),
            [NavigationPage.AvatarEdit] = new(NavigationPage.ProfileEdit, "Аватар"),
            [NavigationPage.SwitchProfilePage] = new(NavigationPage.Profile, "Смена профиля"),
            [NavigationPage.DeleteProfilePage] = new(NavigationPage.Profile, "Удаления профиля"),

            [NavigationPage.Security] = new(null, "Безопасность"),
            [NavigationPage.ChangePasswordPage] = new(NavigationPage.Security, "Изменить пароль"),
            [NavigationPage.PinSettingsPage] = new(NavigationPage.Security, "PIN"),
            [NavigationPage.CreatePinPage] = new(NavigationPage.PinSettingsPage, "Создание"),
            [NavigationPage.ChangePinPage] = new(NavigationPage.PinSettingsPage, "Изменение"),
            [NavigationPage.DeletePinPage] = new(NavigationPage.PinSettingsPage, "Удаление"),

            [NavigationPage.SystemInfo] = new(null, "О системе"),

            [NavigationPage.UpdatesPage] = new(null, "Центр обновлений"),

            [NavigationPage.NotFoundPage] = new(null, "Страница не найдена"),
        };

        private readonly List<NavigationPage> _backHistory = [];
        private readonly List<NavigationPage> _forwardHistory = [];
        private List<NavigationPage> _currentTreeHist = [];

        public event Action<List<NavigationPage>>? NavigationChanged;

        public event Action<NavigationPage>? NavigationNewMenu;

        public NavigationPage CurrentPage { get; private set; } = NavigationPage.None;

        public void Initialize(ContentControl host, NavigationPage? startPage)
        {
            _host = host;

            RegisterPages();

            if (startPage is { } page)
            { // NavigationPage? -> NavigationPage
                _host.Content = _pages[page]; 
                NavigateMenu(page);
            }
        }

        private void RegisterPages()
        {
            _pages[NavigationPage.Profile] = new ProfilePage();
            _pages[NavigationPage.SwitchProfilePage] = new SwitchProfilePage();
            _pages[NavigationPage.ProfileEdit] = new ProfileEdit();
            _pages[NavigationPage.AvatarEdit] = new AvatarEdit();
            _pages[NavigationPage.DeleteProfilePage] = new DeleteProfilePage();


            _pages[NavigationPage.Security] = new SecurityPage();
            _pages[NavigationPage.ChangePasswordPage] = new ChangePasswordPage();

            _pages[NavigationPage.PinSettingsPage] = new PinSettingsPage();
            _pages[NavigationPage.CreatePinPage] = new CreatePinPage();
            _pages[NavigationPage.ChangePinPage] = new ChangePinPage();
            _pages[NavigationPage.DeletePinPage] = new DeletePinPage();

            _pages[NavigationPage.SystemInfo] = new SystemInfo();

            _pages[NavigationPage.UpdatesPage] = new UpdatesPage();

            _pages[NavigationPage.NotFoundPage] = new NotFoundPage();
        }

        public string GetPageTitle(NavigationPage page)
        {
            return _tree[page].Title;
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

                CurrentPage = page;

                SetPage(_pages[page], "Right");

                ChangeTreeHist(page);

                SendHistory();
            }
        }

        public void NavigateMenu(NavigationPage page)
        {
            if (CurrentPage == page)
                return;

            if (!(CurrentPage is NavigationPage.None)) _backHistory.Add(CurrentPage);

            var previous = CurrentPage;

            CurrentPage = page;

            if (previous != NavigationPage.None)
            {
                if (GetParent(previous) == page)
                {
                    SetPage(_pages[page], "Right");
                }
                else SetMenu(_pages[page]);
            }
            else SetMenu(_pages[page]);


            ChangeTreeHist(page);

            SendHistory();
        }

        public void NavigatePage(NavigationPage page)
        {
            if (CurrentPage == page) return;

            if (!(CurrentPage is NavigationPage.None)) _backHistory.Add(CurrentPage);

            CurrentPage = page;

            SetPage(_pages[page], "Left");

            ChangeTreeHist(page);

            SendHistory();
        }

        public void ChangeTreeHist(NavigationPage page)
        {
            _currentTreeHist.Clear();
            NavigationPage lastPage = GetTreePath(page);
            _currentTreeHist.Add(lastPage);
        }

        public void Back()
        {
            if (_backHistory.Count == 0) return;

            if (_pages[CurrentPage] is ICanClose closable && !closable.CanClose()) return;

            NavigationPage lastPage = _backHistory.Last();

            _backHistory.RemoveAt(_backHistory.Count - 1);
            _forwardHistory.Add(CurrentPage);

            if (_forwardHistory.Count > 20) _backHistory.RemoveAt(0);

            if (_tree[lastPage].Parent == null) NavigationNewMenu?.Invoke(lastPage);

            CurrentPage = lastPage;

            SetPage(_pages[lastPage], "Right");

            ChangeTreeHist(lastPage);

            SendHistory();
        }

        public void Forward()
        {
            if (_forwardHistory.Count == 0) return;

            var next = _forwardHistory.Last();
            var current = _pages[CurrentPage];

            _forwardHistory.RemoveAt(_forwardHistory.Count - 1);
            _backHistory.Add(CurrentPage);

            if (_backHistory.Count > 20) _backHistory.RemoveAt(0);

            if (_tree[next].Parent == null) NavigationNewMenu?.Invoke(next);

            CurrentPage = next;

            SetPage(_pages[next], "Left");

            ChangeTreeHist(next);

            SendHistory();
        }

        public List<NavigationPage> SendHistory()
        {
            var history = _currentTreeHist.ToList();
            NavigationChanged?.Invoke(history);
            return history;
        }

        private NavigationPage GetParent(NavigationPage page)
        {
            if (_tree[page].Parent != null)
            {
                if (_tree[page].Parent is { } curPage)
                {
                    return GetParent(curPage);
                }
                else return NavigationPage.None;
            }
            else return page;
        }

        private NavigationPage GetTreePath(NavigationPage currentPage)
        {
            if (_tree[currentPage].Parent != null)
            {
                NavigationPage parent = _tree[currentPage].Parent ?? NavigationPage.NotFoundPage;
                NavigationPage newPage = GetTreePath(parent);
                _currentTreeHist.Add(newPage);
            }

            return currentPage; 
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

        private void SetPage(UserControl page, String direction)
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
