using Supabase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AstrumApp.Services
{
    public class SupabaseService
    {
        private const string SupabaseUrl = "https://nkcwhcqwdsizpravcjfu.supabase.co";
        private const string SupabaseKey = "sb_publishable__32x-8CMC_O_tUIS7EwpcQ_wMbwsvIY";

        private string _email = string.Empty;
        private string _password = string.Empty;

        public Client Client { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Client = new Client(
                SupabaseUrl,
                SupabaseKey
            );

            await Client.InitializeAsync();
        }

        public void SaveUserData(string email, string password)
        {
            _email = email;
            _password = password;
        }

        public async Task<AuthResult> UserRegistration(string name, string bio)
        {
            try
            {
                var options = new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        ["display_name"] = name,
                        ["bio"] = bio
                    }
                };

                var response = await Client.Auth.SignUp(
                    _email,
                    _password,
                    options
                );

                if (response.User != null)
                {
                    App.Session.LoadFromSupabase(response.User);
                }

                    return new AuthResult
                {
                    Success = response.User != null
                };
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                Debug.WriteLine($"Supabase: {ex.Message}"); // viktor.uryadnov03@gmail.com

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
                } else
                {
                    errorMessage = ex.Message;
                }
                

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

        public async Task<AuthResult> UserLogin(string email, string password)
        {
            try
            {
                var response = await Client.Auth.SignIn(
                    email,
                    password
                );

                if (response.User != null)
                {
                    App.Session.LoadFromSupabase(response.User);
                }

                    return new AuthResult
                {
                    Success = response.User != null
                };
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
                else
                {
                    errorMessage = ex.Message;
                }

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
