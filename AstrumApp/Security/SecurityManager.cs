using AstrumApp.Security.Authentication;
using AstrumApp.Security.Crypto;
using AstrumApp.Security.Models;
using AstrumApp.Security.Storage;
using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace AstrumApp.Security
{
    public class SecurityManager
    {
        private SecurityData _data = new();
        private StorageManager _storage = new();
        private PasswordStrengthServicecs _passwordStrengthService = new();
        private HashService _hashService = new();

        public PinManager Pin { get; }

        /*public void CheckPasswordStrength(string password)
        {
            _passwordStrengthService.CheckPasswordStrength(password);
        }*/

        public SecurityManager()
        {
            Load();
            Pin = new PinManager(_data, _hashService);
        }

        public void Load()
        {
            SecurityData? data = _storage.Load();
            if (data != null) _data = data;
        }

        public void Save()
        {
            _storage.Save(_data);
        }

        public void RequestUserData()
        {

        }
    }
}
