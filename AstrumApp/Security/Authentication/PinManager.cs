using AstrumApp.Interfaces;
using AstrumApp.Security.Crypto;
using AstrumApp.Security.Models;
using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;

namespace AstrumApp.Security.Authentication
{
    public class PinManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private IPinData _data;
        private HashService _hashService;
        private RusStringFormatService rusFromat = new RusStringFormatService();

        private Brush _green = new SolidColorBrush(Color.FromRgb(46, 204, 113));
        private Brush _red = new SolidColorBrush(Color.FromRgb(224, 82, 82));

        public DateTime LastChanged => _data.PinChanged;
        public string LastChangedText => LastChangedFormatted();
        internal PinManager(IPinData data, HashService hashService)
        {
            _data = data;
            _hashService = hashService;
        }

        public string LastChangedFormatted()
        {
            if (!HasPin()) return "PIN не установлен";
            TimeSpan timeSinceChange = DateTime.UtcNow - _data.PinChanged;
            if (timeSinceChange.TotalHours <1)
            {
                return $"{timeSinceChange.Minutes} {rusFromat.Format(timeSinceChange.Minutes, "минута", "минуты", "минут")} назад";
            } else if (timeSinceChange.TotalDays < 1)
            {
                return $"{timeSinceChange.Hours} {rusFromat.Format(timeSinceChange.Hours, "час", "часа", "часов")} назад";
            }
            else if (timeSinceChange.TotalDays < 30)
            {
                return $"{timeSinceChange.Days} {rusFromat.Format(timeSinceChange.Days, "день", "дня", "дней")} назад";
            }
            else
            {
                return $"{_data.PinChanged.ToLocalTime().ToString("g")}";
            }
        }

        public string PinStatusText => HasPin() ? "PIN установлен" : "PIN не установлен";
        public Brush PinStatusBrush => HasPin() ? _green : _red;

        public void Set(string pin)
        {
            HashData hashData = _hashService.Hash(pin);
            _data.PinHash = hashData.Hash;
            _data.Salt = hashData.Salt;
            _data.PinChanged = DateTime.UtcNow;
            _data.Iterations = hashData.Iterations;
            
            onPinChanged();
        }

        public bool Change(string oldpin, string newPin)
        {
            bool verified = Verify(oldpin);
            if (verified)
            {
                Set(newPin);
            }

            onPinChanged();
            return verified;
        }

        public void Remove(string pin)
        {
            if (!Verify(pin)) return;

            _data.PinHash = String.Empty;
            _data.Salt = Array.Empty<byte>();
            _data.PinChanged = DateTime.MinValue;
            _data.Iterations = 300_000;

            onPinChanged();
        }

        public bool Verify(string pin)
        {
            if (!HasPin()) return false;

            HashData hashData = new HashData
            {
                Hash = _data.PinHash,
                Salt = _data.Salt,
                Iterations = _data.Iterations
            };
            return _hashService.Verify(pin, hashData);
        }

        public void onPinChanged()
        {
            OnPropertyChanged(nameof(PinStatusText));
            OnPropertyChanged(nameof(PinStatusBrush));
            OnPropertyChanged(nameof(LastChanged));
            OnPropertyChanged(nameof(LastChangedText));
        }

        public bool HasPin()
        {
            return !String.IsNullOrEmpty(_data.PinHash);
        }
    }
}
