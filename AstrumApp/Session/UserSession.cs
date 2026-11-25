using AstrumApp.Profile;
using AstrumApp.Services;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AstrumApp.Session { 

    public class UserSession
    {
        public string? UserId { get; private set; }
        private string _avatarPath = string.Empty;

        public string? Email => UserId is null ? null : Profile.Email;

        public UserProfileViewModel Profile { get; } = new();

        public void SetIdentity(string userId, string avatarPath)
        {
            UserId = userId;
            _avatarPath = avatarPath;
        }

        public void SetProfileFromCache(string userId, string username, string email, BitmapSource? avatar)
        {
            UserId = userId;
            Profile.Load(username, string.Empty, email, avatar, string.Empty, string.Empty, false);
        }

        public async Task SaveUserProfile()
        {
            string? needsDeleting = Profile.Save();

            if (!string.IsNullOrEmpty(needsDeleting))
            {
                _avatarPath = await App.Supabase.UpdateAvatarAsync(needsDeleting);
            }

            long timestamp = Profile.SetUpdateTime(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            await App.Supabase.UpdateUserData(Profile, timestamp, _avatarPath);
        }

        public void CancelChanges()
        {
            Profile.Cancel();
        }

        public void SetAvatar(BitmapSource image)
        {
            Profile.PendingAvatar = image;
        }

        public void ClearAvatar()
        {
            Profile.PendingAvatar = null;
        }

        public void Clear()
        {
            UserId = null;
            _avatarPath = string.Empty;
            Profile.Reset();
        }
    }
}
