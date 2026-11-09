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
using System.Windows.Shapes;

namespace AstrumApp.Profile
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private UserProfile _profile = new();
        private UserProfile _draft = new();
        private bool _isEditing;

        public string Username => _profile.Username;
        public string Bio => _profile.Bio;
        public string Email => _profile.Email;
        public ImageSource DefaultAvatar => FrozenImage.CreateFromUri(AppPaths.DefaultAvatarPath);
        public ImageSource Avatar => _profile.Avatar;

        public string RegisteredAt { get; private set; } = string.Empty;
        public string UpdatedAt { get; private set; } = string.Empty;

        public bool IsPremium => _profile.IsPremium;

        public bool HasChanges { get; private set; }

        public string PendingUsername
        {
            get => _draft.Username;
            set
            {
                if (_draft.Username == value) return;
                _draft.Username = value;
                OnPropertyChanged(nameof(PendingUsername));
                CheckChanges();
            }
        }

        public string PendingBio
        {
            get => _draft.Bio;
            set
            {
                if (_draft.Bio == value) return;
                _draft.Bio = value;
                OnPropertyChanged(nameof(PendingBio));
                CheckChanges();
            }
        }

        public string PendingEmail
        {
            get => _draft.Email;
            set
            {
                if (_draft.Email == value) return;
                _draft.Email = value;
                OnPropertyChanged(nameof(PendingEmail));
                CheckChanges();
            }
        }

        public ImageSource PendingAvatar
        {
            get => _draft.Avatar ?? DefaultAvatar;
            set
            {
                if (ReferenceEquals(_draft.Avatar, value)) return;
                _draft.Avatar = value;
                OnPropertyChanged(nameof(PendingAvatar));
                CheckChanges();
            }
        }

        public UserProfileViewModel() { }

        public UserProfileViewModel(string username, string bio, string email, ImageSource? avatar)
        {
            Load(username, bio, email, avatar, string.Empty, string.Empty, false);
        }

        public void Load(string username, string bio, string email, ImageSource? avatar, string registeredAt, string updatedAt,
            bool isPremium)
        {
            var incoming = new UserProfile
            {
                Username = username,
                Bio = bio,
                Email = email,
                IsPremium = isPremium
            };

            if (avatar != null)
                incoming.Avatar = avatar;

            _profile = incoming;
            _draft = Clone(_profile);
            RegisteredAt = registeredAt;
            UpdatedAt = updatedAt;
            EndEdit();
            OnPropertyChanged();
        }

        public void BeginEdit()
        {
            _isEditing = true;
            _draft = Clone(_profile);
            SetHasChanges(false);
            OnPropertyChanged();
        }

        public string? Save()
        {
            if (!_isEditing) return null;

            string? needsDeleting = null;

            if (_draft.Avatar == null)
            {
                _draft.Avatar = DefaultAvatar;
                
                string avatarPath = AppPaths.AvatarPath;
                if (File.Exists(avatarPath))
                {
                    File.Delete(avatarPath);
                }
                needsDeleting = string.Empty;
            } else if (_draft.Avatar is BitmapSource bitmap)
            {
                if (!ReferenceEquals(bitmap, _profile.Avatar))
                {
                    string avatarPath = FrozenImage.SaveToFile(bitmap, AppPaths.AvatarPath);
                    needsDeleting = avatarPath;
                }

            }

            _profile = Clone(_draft);
            
            EndEdit();
            OnPropertyChanged();

            return needsDeleting; 
        }

        public void Cancel()
        {
            _draft = Clone(_profile);
            EndEdit();
            OnPropertyChanged();
        }

        public bool IsEditing() => _isEditing;

        public void Reset()
        {
            _profile = new UserProfile();
            _draft = Clone(_profile);
            RegisteredAt = string.Empty;
            UpdatedAt = string.Empty;
            EndEdit();
            OnPropertyChanged();
        }

        private void EndEdit()
        {
            _isEditing = false;
            SetHasChanges(false);
        }

        private void CheckChanges() => SetHasChanges(!AreEqual(_profile, _draft));

        private void SetHasChanges(bool value)
        {
            if (HasChanges == value) return;
            HasChanges = value;
            OnPropertyChanged(nameof(HasChanges));
        }

        private static bool AreEqual(UserProfile a, UserProfile b) =>
            a.Username == b.Username &&
            a.Bio == b.Bio &&
            a.Email == b.Email &&
            ReferenceEquals(a.Avatar, b.Avatar);

        private static UserProfile Clone(UserProfile source) => new()
        {
            Username = source.Username,
            Bio = source.Bio,
            Email = source.Email,
            Avatar = source.Avatar
        };
    }

    public class UserProfile
    {
        private string _username = "Имя пользователя";
        private string _bio = "биография";
        private string _email = "example@example.com";
        private bool _isPremium = false;
        private static readonly ImageSource DefaultAvatar = FrozenImage.CreateFromUri(AppPaths.DefaultAvatarPath);

        private ImageSource _avatar = DefaultAvatar;

        public UserProfile(ImageSource? avatar = null)
        {
            if (avatar != null)
                _avatar = avatar;
            else if (File.Exists(AppPaths.AvatarPath))
                _avatar = FrozenImage.CreateFromUri(AppPaths.AvatarPath);
            else
                _avatar = DefaultAvatar;
        }

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

        public bool IsPremium
        {
            get => _isPremium;
            set => _isPremium = value;
        }

        public ImageSource Avatar
        {
            get => _avatar;
            set => _avatar = value;
        }
    }
}
