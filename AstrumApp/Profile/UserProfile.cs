using AstrumApp.Apps.Settings;
using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AstrumApp.Profile
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private bool isEditing = false;

        private UserProfile _profile = new UserProfile();
        private UserProfile _draft = new UserProfile();

        public string Username => _profile.Username;
        public string Bio => _profile.Bio;
        public string Email => _profile.Email;
        public ImageSource Avatar => _profile.Avatar;
        public string RegisteredAt { get; set; } = "NULL";
        public string UpdatedAt { get; set; } = "NULL";

        public bool HasChanges { get; set; } = false;

        public string PendingUsername
        {
            get => _draft.Username;
            set
            {
                if (value != _draft.Username)
                {
                    _draft.Username = value;
                    OnPropertyChanged(nameof(PendingUsername));
                    CheckChanges();
                }
            }
        }

        public string PendingBio
        {
            get => _draft.Bio;
            set
            {
                if (value != _draft.Bio)
                {
                    _draft.Bio = value;
                    OnPropertyChanged(nameof(PendingBio));
                    CheckChanges();
                }
            }
        }

        public string PendingEmail
        {
            get => _draft.Email;
            set
            {
                if (value != _draft.Email)
                {
                    _draft.Email = value;
                    OnPropertyChanged(nameof(PendingEmail));
                    CheckChanges();
                }
            }
        }

        public ImageSource PendingAvatar
        {
            get => _draft.Avatar;
            set
            {
                if (value != _draft.Avatar) { 
                    _draft.Avatar = value;
                    OnPropertyChanged(nameof(PendingAvatar));
                }
            }
        }

        public UserProfileViewModel(string username, string bio, string email, ImageSource avatar)
        {
            _profile = new UserProfile
            {
                Username = username,
                Bio = bio,
                Email = email,
                Avatar = avatar
            };
            _draft = Clone(_profile);
        }

        public void UpdateAvatar(string avatarPath, BitmapSource avatar)
        {
            PendingAvatar = avatar;
            //_draft.AvatarPath = avatarPath;
            CheckChanges();
        }

        public void BeginEdit() 
        {
            isEditing = true;
            _draft = Clone(_profile);
            OnPropertyChanged();
        }

        public void Save()
        {
            _profile = Clone(_draft);

            UpdatedAt = StringFormatService.FormatTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            OnPropertyChanged();

            SaveBitmap((BitmapSource)_profile.Avatar, AppPaths.AvatarPath); 

            EndEdit();
        }

        public void Cancel()
        {
            _draft = Clone(_profile);
            EndEdit();
        }

        private void EndEdit()
        {
            isEditing = false;
            HasChanges = false;
        }

        public bool IsEditing()
        {
            return isEditing;
        }

        public void SaveBitmap(BitmapSource source, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        private void CheckChanges()
        {
            HasChanges = !AreEqual(_profile, _draft);
            OnPropertyChanged(nameof(HasChanges));
        }

        private static bool AreEqual(UserProfile a, UserProfile b)
        {
            return a.Username == b.Username &&
                   a.Bio == b.Bio &&
                   a.Email == b.Email &&
                   a.Avatar == b.Avatar;
        }

        private static UserProfile Clone(UserProfile m)
        {
            return new UserProfile
            {
                Username = m.Username,
                Bio = m.Bio,
                Email = m.Email,
                Avatar = m.Avatar
            };
        }
    }
    public class UserProfile
    {
        private string _username = "Имя пользователя";
        private string _bio = "биография";
        private string _email = "example@example.com";
        private ImageSource _avatar = new BitmapImage(new Uri(AppPaths.DefaultAvatarPath));

        public string Username 
        { 
            get => _username; 
            set => _username = value;
        }

        public string Bio
        {
            get => _bio;
            set => _bio = value;
        }

        public string Email
        {
            get => _email;
            set => _email = value;
        }

        public ImageSource Avatar
        {
            get => _avatar;
            set => _avatar = value;
        }

        private static BitmapImage CreateFrozenImage(string uri)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(uri);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }
    }
}
