using AstrumApp.Profile;
using AstrumApp.Services;
using Supabase;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AuthUser = Supabase.Gotrue.User;
using SupabaseClient = Supabase.Client;

namespace AstrumApp.AuthManager
{
    public class SupabaseService
    {
        public SupabaseClient? Client { get; private set; }

        private AvatarStorageService? _avatarStorage;
        private Dictionary<string, object> _userMetadata = new();


        public bool PrepareClient()
        {
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL")
                ?? throw new InvalidOperationException("SUPABASE_URL не задан.");

            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY")
                ?? throw new InvalidOperationException("SUPABASE_KEY не задан.");

            try
            {
                Client = new SupabaseClient(url, key, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                });

                Client.Auth.SetPersistence(new SessionStorage());
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        public async Task<User?> RestoreSessionAsync()
        {
            if (Client == null)
                return null;

            Client.Auth.LoadSession();

            await Client.InitializeAsync();

            return Client.Auth.CurrentUser;
        }


        public async Task ApplyUserAsync(User user)
        {
            _avatarStorage = new AvatarStorageService(Client);

            await ApplyUser(user);
        }

        public async Task<AuthResult> Login(string email, string password)
        {
            try
            {
                if (Client == null)
                    throw new InvalidOperationException("Supabase client is not initialized.");

                var response = await Client.Auth.SignIn(email, password);
                if (response?.User == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorCode = "401",
                        Error = "Ошибка аутентификации"
                    };
                }

                await ApplyUserAsync(response.User);
                return new AuthResult { Success = true };
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                var error = TryParseError(ex.Message);
                var message = error?.ErrorCode switch
                {
                    "invalid_credentials" => "Неверная почта или пароль",
                    "email_not_confirmed" => "Почта не подтверждена",
                    "user_not_found" => "Пользователь не найден",
                    _ => error == null ? ex.Message : $"Код {error.Code}, {error.ErrorCode}"
                };

                return new AuthResult
                {
                    Success = false,
                    ErrorCode = error?.ErrorCode,
                    Error = message
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка входа: {ex}");
                return new AuthResult
                {
                    Success = false,
                    ErrorCode = "410(INNER)",
                    Error = ex.Message
                };
            }
        }

        public async Task<string> FirstUploadAvatarAsync(string avatarPath)
        {
            string appPath = File.Exists(avatarPath) ? avatarPath : string.Empty;

            string newStoragePath = string.Empty;
            if (!string.IsNullOrEmpty(appPath))
            {
                newStoragePath = await UpdateAvatarAsync(appPath);
            }

            return newStoragePath;
        }

        public async Task<bool> UpdateAvatarPathAsync(string avatarPath)
        {
            if (Client == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            var attributes = new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    ["avatar_path"] = avatarPath ?? string.Empty
                }
            };

            var user = await Client.Auth.Update(attributes);

            return user != null;
        }

        public async Task<AuthResult> Registration()
        {
            try
            {
                if (Client == null)
                    throw new InvalidOperationException("Supabase client is not initialized.");

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string _email = _userMetadata.GetValueOrDefault("email")?.ToString() ?? string.Empty;
                string _password = _userMetadata.GetValueOrDefault("password")?.ToString() ?? string.Empty;
                

                _userMetadata["registered_at"] = now;
                _userMetadata["updated_at"] = now;
                _userMetadata["avatar_path"] = string.Empty;


                _userMetadata.Remove("email");
                _userMetadata.Remove("password");

                var options = new SignUpOptions
                {
                    Data = _userMetadata
                };

                Debug.WriteLine($"Attempting to register user with email: {_email}, metadata: {JsonSerializer.Serialize(_userMetadata)}");

                var response = await Client.Auth.SignUp(_email, _password, options);
                if (response?.User == null)
                    return new AuthResult { Success = false };

                // При обязательном подтверждении почты регистрация НЕ равна входу.
                // Подключаем профиль лишь тогда, когда SDK действительно имеет сессию.
                if (Client.Auth.CurrentUser != null)
                    _ = ApplyUserAsync(Client.Auth.CurrentUser);

                return new AuthResult { Success = true };
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                var error = TryParseError(ex.Message);
                var message = error?.Code switch
                {
                    400 => "Пароль недействительный",
                    422 => "Минимальная длина пароля 6 символов",
                    _ => error == null ? ex.Message : $"Код {error.Code}, {error.ErrorCode}"
                };

                return new AuthResult
                {
                    Success = false,
                    Error = message
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка регистрации: {ex}");
                return new AuthResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }


        private async Task ApplyUser(AuthUser user)
        {
            if (_avatarStorage == null || user == null) return;

            var metadata = user.UserMetadata;
            var appMetadata = user.AppMetadata;

            string name = metadata?.GetValueOrDefault("username")?.ToString() ?? string.Empty;
            string bio = metadata?.GetValueOrDefault("bio")?.ToString() ?? string.Empty;
            string email = user.Email ?? string.Empty;
            string profileAvatarPath = metadata?.GetValueOrDefault("avatar_path")?.ToString() ?? string.Empty;

            long ReadTimestamp(string key) => long.TryParse(metadata?.GetValueOrDefault(key)?.ToString(), out var value) ? value : 0;

            string registeredAt = StringFormatService.FormatTimestamp(ReadTimestamp("registered_at"));
            string updatedAt = StringFormatService.FormatTimestamp(ReadTimestamp("updated_at"));

            string isPremiumString = appMetadata?.GetValueOrDefault("is_premium")?.ToString() ?? "0";

            bool isPremium = isPremiumString == "1";

            Debug.WriteLine(isPremium);

            App.Session.SetIdentity(user.Id, profileAvatarPath);

            ImageSource? avatar = null;
            if (!string.IsNullOrEmpty(profileAvatarPath))
            {
                avatar = await _avatarStorage.DownloadAvatarAsync(profileAvatarPath);
                if (avatar != null)
                {
                    BitmapSource? localAvatar = FrozenImage.CreateFromUri(AppPaths.AvatarPath);

                    if (avatar is BitmapSource bitmap)
                    {
                        if (!File.Exists(AppPaths.AvatarPath) || !ReferenceEquals(localAvatar, bitmap))
                        {
                            FrozenImage.SaveToFile(bitmap, AppPaths.AvatarPath);
                        }
                    }
                }
            }
            App.Session.Profile.Load(name, bio, email, avatar, registeredAt, updatedAt, isPremium);
        }

        public async Task UpdateUserData(UserProfileViewModel profile, long timestamp, string? avatarPath)
        {
            if (Client == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            var attributes = new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    ["display_name"] = profile.Username,
                    ["bio"] = profile.Bio,
                    ["updated_at"] = timestamp,
                    ["avatar_path"] = avatarPath ?? string.Empty
                }
            };

            if (profile.Email != Client.Auth.CurrentUser?.Email)
            {
                //attributes.Email = profile.Email;
            }

            var user = await Client.Auth.Update(attributes); // здесь ошибка че то с почтой ваще хз
        }

        public async Task<string> UpdateAvatarAsync(string filePath)
        {
            if (Client == null || Client.Auth.CurrentUser == null || Client.Auth.CurrentUser.Id == null || _avatarStorage == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            string oldPath = Client.Auth.CurrentUser.UserMetadata.GetValueOrDefault("avatar_path")?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(filePath))
            {
                await _avatarStorage.DeleteAvatarAsync(oldPath);
                return string.Empty;
            }
            return await _avatarStorage.UploadAvatarAsync(Client.Auth.CurrentUser.Id, filePath, oldPath);
        }

        public void SetData(string key, string value) => _userMetadata[key] = value;
        public object? GetData(string key) => _userMetadata.GetValueOrDefault(key);

        private static SupabaseError? TryParseError(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<SupabaseError>(message);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public class RegistrationResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
    }

    public class AuthResult
    {
        public bool Success { get; init; }
        public string? ErrorCode { get; init; }
        public string? Error { get; init; }
    }

    public class SupabaseError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("msg")]
        public string? Message { get; set; }

        [JsonPropertyName("weak_password")]
        public WeakPasswordInfo? WeakPassword { get; set; }
    }

    public class WeakPasswordInfo
    {
        [JsonPropertyName("reasons")]
        public List<string>? Reasons { get; set; }
    }
}
