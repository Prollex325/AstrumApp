using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AstrumApp.Services.Updates
{
    public class UpdatePolicy
    {
        public string MinimumSupportedVersion { get; set; } = "0.0.0";
    }
    public class UpdatePolicyService
    {
        private static readonly HttpClient _http = new();

        public async Task<UpdatePolicy> GetPolicyAsync()
        {
            string url = "https://raw.githubusercontent.com/Prollex325/AstrumApp/main/AstrumApp/update-policy.json";

            string json = await _http.GetStringAsync(url);

            return JsonSerializer.Deserialize<UpdatePolicy>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? throw new InvalidOperationException("Некорректная политика обновлений.");
        }
    }

}
