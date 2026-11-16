using AstrumApp.Profile;
using AstrumApp.Services;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AuthUser = Supabase.Gotrue.User;
using SupabaseClient = Supabase.Client;

namespace AstrumApp.AuthManager
{
    public partial class SupabaseService
    {
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

                try {
                    var response = await Client.Auth.SignUp(_email, _password, options);
                    Debug.WriteLine(response);
                    if (response?.User == null)
                        return new AuthResult { Success = false };

                    // При обязательном подтверждении почты регистрация НЕ равна входу.
                    // Подключаем профиль лишь тогда, когда SDK действительно имеет сессию.
                    if (Client.Auth.CurrentUser != null)
                        _ = ApplyUserAsync(Client.Auth.CurrentUser);

                    return new AuthResult { Success = true };
                }
                catch (GotrueException ex)
                {
                    if (ex.Message.Contains("User already registered"))
                    {
                        return new AuthResult { Success = false, Error = "Пользователь с такой почтой уже существует" };
                    }
                    else
                    {
                        return new AuthResult { Success = false };
                    }
                }
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
