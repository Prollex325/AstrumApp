using Supabase;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AuthSession = Supabase.Gotrue.Session;
using AuthUser = Supabase.Gotrue.User;
using SupabaseClient = Supabase.Client;

namespace AstrumApp.AuthManager
{
    public class SupabaseService
    {
        private string SupabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")!;
        private string SupabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY")!;
        public SupabaseClient? Client { get; private set; }
        private string _email = string.Empty;
        private string _password = string.Empty;

        public async Task<bool> Initialize()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            Client = new(
                SupabaseUrl,
                SupabaseKey,
                options
            );

            Client.Auth.SetPersistence(new SessionStorage());

            Client.Auth.LoadSession();

            await Client.InitializeAsync();

            AuthUser? user = Client.Auth.CurrentUser;
            bool isAuthenticated = user != null;

            if (isAuthenticated)
            {
                ImageSource avatarImage = LoadAvatar(user);

                App.Session.LoadFromSupabase(user, avatarImage);
            }

            return isAuthenticated;
        }

        public async Task<AuthResult> Login(string email, string password)
        {
            try
            {
                if (Client == null)
                    throw new InvalidOperationException("Supabase client is not initialized.");

                var response = await Client.Auth.SignIn(email, password);

                if (response.User != null)
                {
                    AuthUser user = response.User;
                    ImageSource avatarImage = LoadAvatar(user);
                    App.Session.LoadFromSupabase(user, avatarImage);
                }

                return new AuthResult{ Success = response.User != null };
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                Debug.WriteLine($"Supabase: {ex.Message}");

                var error = JsonSerializer.Deserialize<SupabaseError>(ex.Message);

                string errorMessage;

                if (error != null)
                {
                    switch (error.ErrorCode)
                    {
                        case "invalid_credentials":
                            errorMessage = "Неверная почта или пароль";
                            break;

                        case "email_not_confirmed":
                            errorMessage = "Почта не подтверждена";
                            break;

                        case "user_not_found":
                            errorMessage = "Пользователь не найден";
                            break;

                        default:
                            errorMessage = $"Код {error.Code}, {error.ErrorCode}";
                            break;
                    }
                }
                else errorMessage = ex.Message;

                return new AuthResult
                {
                    Success = false,
                    ErrorCode = error?.ErrorCode,
                    Error = errorMessage
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");

                return new AuthResult
                {
                    Success = false,
                    ErrorCode = "410(INNER)",
                    Error = ex.Message
                };
            }
        }

        public async Task<AuthResult> Registration(string name, string bio)
        {
            try
            {
                var options = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        ["display_name"] = name,
                        ["bio"] = bio,
                        ["registered_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        ["updated_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        ["avatar_path"] = string.Empty
                    }
                };

                var response = await Client.Auth.SignUp(
                    _email,
                    _password,
                    options
                );

                if (response.User != null)
                {
                    AuthUser user = response.User;
                    ImageSource avatarImage = LoadAvatar(user);
                    App.Session.LoadFromSupabase(user, avatarImage);
                }

                    return new AuthResult { Success = response.User != null };
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                Debug.WriteLine($"Supabase: {ex.Message}");

                // Supabase: {"code":400,"error_code":"validation_failed","msg":"Signup requires a valid password"}
                // {"code":422,"error_code":"weak_password","msg":"Password should be at least 6 characters.","weak_password":{"reasons":["length"]}}

                var error = JsonSerializer.Deserialize<SupabaseError>(ex.Message);

                string errorMessage = string.Empty;

                if (error != null)
                {
                    switch (error.Code)
                    {
                        case 400:
                            errorMessage = "Пароль недействительный";
                            break;
                        case 422:
                            errorMessage = "Минимальная длина пароля 6 символов";
                            break;
                        default:
                            errorMessage = $"Код {error.Code}, {error.ErrorCode}";
                            break;
                    }
                }
                else errorMessage = ex.Message;

                return new AuthResult
                {
                    Success = false,
                    Error = errorMessage
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");

                return new AuthResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public void SaveData(string email, string password)
        {
            _email = email;
            _password = password;
        }

        public async void UpdateData(string newEmail, string newName, string newBio, long timestamp)
        {
            if (Client == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            var attributes = new UserAttributes
            {
                Email = newEmail,

                Data = new Dictionary<string, object>
                {
                    ["display_name"] = newName,
                    ["bio"] = newBio,
                    ["updated_at"] = timestamp
                }
            };

            var user = await Client.Auth.Update(attributes);
        }

        public async Task<string> UploadAvatar(string imagePath)
        {
            if (Client == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            var user = Client.Auth.CurrentUser;

            string extension = Path.GetExtension(imagePath).ToLowerInvariant();

            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => throw new ArgumentException("Неподдерживаемый формат изображения")
            };

            if (new FileInfo(imagePath).Length > 2 * 1024 * 1024)
                throw new ArgumentException("Изображение больше 2 МБ");

            string avatarPath = user.UserMetadata?.GetValueOrDefault("avatar_path")?.ToString() ?? string.Empty;
            Debug.WriteLine($"Текущий путь аватара: {avatarPath}");
            if (!string.IsNullOrWhiteSpace(avatarPath))
            {
                await Client.Storage
                    .From("avatars")
                    .Remove(new List<string> { avatarPath });
            }

            avatarPath = $"{user.Id}/{Guid.NewGuid():N}{extension}";

            // Загружаем изображение
            try
            {
                await Client.Storage
                    .From("avatars")
                    .Upload(imagePath, avatarPath, new Supabase.Storage.FileOptions
                    {
                        ContentType = contentType,
                        Upsert = false
                    });

                Debug.WriteLine("Аватар успешно загружен!");
            }
            catch (Supabase.Storage.Exceptions.SupabaseStorageException ex)
            {
                Debug.WriteLine($"Ошибка Storage: {ex.Message}");
                Debug.WriteLine($"Подробности: {ex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Другая ошибка: {ex}");
            }

            await Client.Auth.Update(new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    ["avatar_path"] = avatarPath
                }
            });

            // Получаем публичный URL
            return Client.Storage
                .From("avatars")
                .GetPublicUrl(avatarPath);
        }

        public ImageSource? LoadAvatar(AuthUser user)
        {
            string avatarPath = user.UserMetadata?.GetValueOrDefault("avatar_path")?.ToString() ?? string.Empty;

            string defPath = Path.Exists(AppPaths.AvatarPath) ? AppPaths.AvatarPath : AppPaths.DefaultAvatarPath;

            string url = string.IsNullOrWhiteSpace(avatarPath)
                ? defPath
                : Client.Storage.From("avatars").GetPublicUrl(avatarPath);

            var img = new BitmapImage();

            img.DownloadCompleted += (s, e) =>
            {
                Debug.WriteLine("Аватар успешно загружен!");
            };

            img.DownloadFailed += (s, e) =>
            {
                Debug.WriteLine($"Ошибка загрузки: {e.ErrorException.Message}");
            };

            img.BeginInit();
            img.UriSource = new Uri(url, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();

            return img;
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