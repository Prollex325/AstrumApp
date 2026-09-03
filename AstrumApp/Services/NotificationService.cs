using AstrumApp.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Services
{
    public class NotificationService
    {
        public ObservableCollection<NotificationCard> Notifications { get; } = new();
        public event Action? BeforeNotificationChanged;

        public async void Show(string text, CardTypes type = CardTypes.None)
        {
            BeforeNotificationChanged?.Invoke();

            var card = new NotificationCard { Text = text, CardType = type };
            Notifications.Add(card);
        }

        public void Remove(NotificationCard card)
        {
            if (Notifications.Contains(card))
            {
                Notifications.Remove(card);
            }
        }

        public bool Has(NotificationCard card)
        {
            return Notifications.Contains(card);
        }
    }

    public enum CardTypes
    {
        None,
        Error,
        Warning,
        Success
    }
    public static class NotificationPalette
    {
        public static readonly (Color Start, Color End) Success =
        (
            (Color)ColorConverter.ConvertFromString("#2ECC71"),
            (Color)ColorConverter.ConvertFromString("#27AE60")
        );

        public static readonly (Color Start, Color End) Error =
        (
            (Color)ColorConverter.ConvertFromString("#E74C3C"),
            (Color)ColorConverter.ConvertFromString("#C0392B")
        );

        public static readonly (Color Start, Color End) Warning =
        (
            (Color)ColorConverter.ConvertFromString("#F1C40F"),
            (Color)ColorConverter.ConvertFromString("#F39C12")
        );
    }

    public class NotificationCard
    {
        public string Text { get; set; } = "Настройки сохранены";
        public CardTypes CardType 
        { 
            get => _cardType;
            set
            {
                _cardType = value;
                (StartColor, EndColor) = value switch
                {
                    CardTypes.Error => NotificationPalette.Error,
                    CardTypes.Warning => NotificationPalette.Warning,
                    CardTypes.Success => NotificationPalette.Success,
                    _ => NotificationPalette.Success
                };
            }
        }
        public Color StartColor { get; private set; }
        public Color EndColor { get; private set; }
        private CardTypes _cardType;

        public void SetColors(CardTypes type)
        {
            (StartColor, EndColor) = type switch
            {
                CardTypes.Error => NotificationPalette.Error,
                CardTypes.Warning => NotificationPalette.Warning,
                CardTypes.Success => NotificationPalette.Success, // если Info пока зеленый
                _ => NotificationPalette.Success
            };
        }
    }
}
