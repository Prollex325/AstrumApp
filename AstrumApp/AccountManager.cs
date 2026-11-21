using AstrumApp.Apps.Settings.MenuControls.ProfileOptions;
using AstrumApp.AuthManager;
using AstrumApp.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AstrumApp
{
    public class AccountCard
    {
        public string ID { get; set; } = string.Empty;
        public string Username { get; set; } = "N/A";
        public string Email { get; set; } = "N/A";
        public ImageSource Avatar
        {
            get => _avatar;
            set => _avatar = value ?? FrozenImage.CreateFromUri(AppPaths.DefaultAvatarPath)!;
        }
        public bool IsActive { get; set; } = false;

        private ImageSource _avatar = FrozenImage.CreateFromUri(AppPaths.DefaultAvatarPath)!;

        public AccountCard(string id)
        {
            ID = id;
        }
    }

    public class AccountManager
    {
        //private SessionStorage storage = new();

        /*
         Проверяем active
         Если нету активного - на экран выбора сессий (если нет сессий на авторизацию)
         Если есть проверяем есть ли сессия этого пользователя
         Если нет его сессии отправляем на выбор аккаунта
         Иначе создаем хранилище и передаем путь
         */

        public bool RememberMe 
        { 
            get => _currentStorage.RememberMe;

            set
            {
                if (_currentStorage != null)  _currentStorage.RememberMe = value;
            }
        }

        public bool RememberChoice
        {
            get => _currentStorage.RememberChoice;

            set
            {
                if (_currentStorage != null) _currentStorage.RememberChoice = value;
            }
        }

        private string? _activeID = string.Empty;
        private SessionStorage _currentStorage = new();
        public string? ActiveId => _activeID;
        private ObservableCollection<AccountCard> _items = new();

        public AccountManager()
        {
            _activeID = LoadActiveAccount();
            LoadCache();
        }

        public void DeleteAccount()
        {
            string authAccountPath = Path.Combine(AppPaths.AuthFolder, $"{_activeID}.dat");
            string activeAccountPath = Path.Combine(AppPaths.AuthFolder, "active.json");
            string cacheAccountPath = Path.Combine(AppPaths.CacheFolder, $"{_activeID}");

            if (Path.Exists(authAccountPath)) File.Delete(authAccountPath);
            if (Path.Exists(activeAccountPath)) File.Delete(activeAccountPath);
            if (Directory.Exists(cacheAccountPath) && !string.IsNullOrWhiteSpace(_activeID)) Directory.Delete(cacheAccountPath, recursive: true);

            if (App.Current.MainWindow is MainWindow window)
            {
                window.NavigateToAuth(Apps.Authentication.NavigationPage.SelectProfile, "");;
            }
        }

        public async Task SwitchAccount(string? userId)
        {
            if (App.Current.MainWindow is MainWindow window)
            {
                window.Lock();
            }
            // если null - добавить новый аккаунт
            _activeID = userId;
            bool res = await App.Supabase.ActivateAccountAsync(userId, "");

            if (res)
            {
                if (_activeID != null) App.Notifications.Show("Успешный вход", CardTypes.Success);
            }
            else App.Notifications.Show("Произошла ошибка", CardTypes.Error);
        }

        public async Task SelectAccount(string userId)
        {
            _activeID = userId;
            bool res = await App.Supabase.ActivateAccountAsync(userId, "");

            if (App.Current.MainWindow is MainWindow window && res)
            {
                window.CloseAuth();
            }

            if (res) App.Notifications.Show("Успешный вход", CardTypes.Success);
            else App.Notifications.Show("Произошла ошибка", CardTypes.Error);
        }

        public ObservableCollection<AccountCard> LoadAllCacheProfiles(bool withActiveID)
        {
            _items.Clear();
            string[] directories = Directory.GetDirectories(AppPaths.CacheFolder);
            string[] files = Directory.GetFiles(AppPaths.AuthFolder);

            List<string> IDs = new();

            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".dat")
                {
                    string userId = Path.GetFileNameWithoutExtension(file);
                    IDs.Add(userId);

                    int index = IDs.IndexOf(_activeID);

                    if (index > 0 && withActiveID)
                    {
                        IDs.RemoveAt(index);
                        IDs.Insert(0, _activeID);
                    }
                }
            }

            foreach (string userId in IDs)
            {
                string directory = Path.Combine(AppPaths.CacheFolder, userId);

                AccountCard card = new(userId);
                if (Directory.Exists(directory))
                {
                    string activeIdCacheJson = Path.Combine(directory, "profile.json");
                    string activeIdCacheAvatar = Path.Combine(directory, "avatar.png");

                    string username = "N/A", email = "N/A";

                    if (Path.Exists(activeIdCacheJson))
                    {
                        var data = JObject.Parse(File.ReadAllText(activeIdCacheJson));

                        username = data["username"]?.ToString() ?? "N/A";
                        email = data["email"]?.ToString() ?? "N/A";
                    }

                    BitmapSource? avatar = null;
                    if (Path.Exists(activeIdCacheAvatar))
                    {
                        avatar = FrozenImage.CreateFromUri(activeIdCacheAvatar)!;
                    }
                    card = new AccountCard(userId)
                    { 
                        Username = username,
                        Email = email,
                        Avatar = avatar!,
                        IsActive = (userId == _activeID && withActiveID)
                    };
                }

                _items.Add(card);
            }
                
            return _items;
        }

        public void SaveCache(string userId, string username, string email, ImageSource? avatar)
        {
            if (!RememberMe) return;
            if (!Directory.Exists(AppPaths.CacheFolder)) Directory.CreateDirectory(AppPaths.CacheFolder);

            string userCachePath = Path.Combine(AppPaths.CacheFolder, userId);
            if (!Directory.Exists(userCachePath)) Directory.CreateDirectory(userCachePath);

            string profilePath = Path.Combine(userCachePath, "profile.json");
            if (!Directory.Exists(userCachePath)) Directory.CreateDirectory(userCachePath);

            var profile = new
            {
                username,
                email
            };
            string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(profilePath, json);

            string avatarCachePath = Path.Combine(userCachePath, "avatar.png");
            if (avatar != null) FrozenImage.SaveToFile((BitmapSource)avatar, avatarCachePath);
        }

        public void SaveActiveID(string activeID)
        {
            if (!RememberMe || !RememberChoice) return;

            if (!Directory.Exists(AppPaths.AuthFolder)) Directory.CreateDirectory(AppPaths.AuthFolder);

            string activePath = Path.Combine(AppPaths.AuthFolder, "active.json");

            var profile = new
            {
                activeID
            };
            string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(activePath, json);
        }

        public void LoadCache()
        {
            string cachePath = AppPaths.CacheFolder;
            if (!Directory.Exists(cachePath) || _activeID == null) return;

            string activeIdCacheJson = Path.Combine(cachePath, _activeID, "profile.json");
            string activeIdCacheAvatar = Path.Combine(cachePath, _activeID, "avatar.png");

            string username = string.Empty, email = string.Empty;
            if (activeIdCacheJson != null) {
                var data = JObject.Parse(File.ReadAllText(activeIdCacheJson));

                username = data["username"]?.ToString() ?? string.Empty;
                email = data["email"]?.ToString() ?? string.Empty;
            }

            BitmapSource? avatar = null;
            if (activeIdCacheAvatar != null) 
            {
                avatar = FrozenImage.CreateFromUri(activeIdCacheAvatar)!;
            }

            App.Session.SetProfileFromCache(_activeID, username, email, avatar);
        }

        public async Task DeleteCurrentSession()
        {
            await App.Supabase.SignOut();

            string path = Path.Combine(AppPaths.AuthFolder, "active.json");
            if (File.Exists(path)) 
                File.Delete(path);

            bool res = await App.Supabase.ActivateAccountAsync(null, "Сессия удалена");
        }

        public SessionStorage CreateStorage()
        {
            string? sessionPath = SessionPath();
            SessionStorage tempStorage = new();
            if (sessionPath != null) tempStorage = new(sessionPath);

            tempStorage.RememberChoice = RememberChoice;
            tempStorage.RememberMe = RememberMe;
            _currentStorage = tempStorage;

            return _currentStorage;
        }

        private string? SessionPath()
        {
            string? activeID = _activeID;
            if (activeID == null) return null; // На окно выбора профилей

            string path = Path.Combine(AppPaths.AuthFolder, $"{activeID}.dat");
            if (File.Exists(path)) return path;
            else return null; // На окно выбора профилей
        }

        private string? LoadActiveAccount()
        {
            string activeIDPath = Path.Combine(AppPaths.AuthFolder, "active.json");
            if (!File.Exists(activeIDPath)) return null;

            try
            {
                var data = JObject.Parse(File.ReadAllText(activeIDPath));
                string? userID = data["activeID"]?.ToString();
                return data["activeID"]?.ToString();
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
