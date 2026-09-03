using AstrumApp.Profile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AstrumApp.Session
{
    public class UserSession
    {
        public string? UserId { get; private set; }
        public string? Email { get; private set; }

        public UserProfileViewModel Profile { get; } = new();

        public void LoadFromSupabase(Supabase.Gotrue.User user)
        {
            UserId = user.Id;
            Email = user.Email;

            Profile.BeginEdit();
            if (user.UserMetadata.TryGetValue("display_name", out var name))
                Profile.PendingUsername = name?.ToString() ?? "";

            if (user.UserMetadata.TryGetValue("bio", out var bio))
                Profile.PendingBio = bio?.ToString() ?? "";

            if (user.Email != null)
                Profile.PendingEmail = user.Email;
            Profile.Save();

            Debug.WriteLine($"User session loaded: UserId={UserId}, Email={Email}, Username={Profile.Username}, Bio={Profile.PendingBio}");
        }

        public void Clear()
        {
            UserId = null;
            Email = null;
            //Profile.Clear();
        }
    }
}
