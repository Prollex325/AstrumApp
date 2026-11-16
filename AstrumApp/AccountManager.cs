using AstrumApp.AuthManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace AstrumApp
{
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

        public SessionStorage CreateStorage()
        {
            string? sessionPath = SessionPath();
            if (sessionPath != null) return new(sessionPath);
            else return new();
        }

        private string? SessionPath()
        {
            string? activeID = LoadActiveAccount();
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
