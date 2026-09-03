using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Services
{
    public class NotificationAnimator
    {
        private Dictionary<NotificationCard, Border> registered;
        private readonly ItemsControl _itemsControl;
        private Dictionary<NotificationCard, double> coords;

        public NotificationAnimator(ItemsControl itemsControl)
        {
            _itemsControl = itemsControl;
            registered = new Dictionary<NotificationCard, Border>();
            coords = new Dictionary<NotificationCard, double>();
        }

        public void Register(NotificationCard card, Border border)
        {
            if (!registered.ContainsKey(card)) registered.Add(card, border);
        }

        public void Unregister(NotificationCard card)
        {
            if (registered.ContainsKey(card)) registered.Remove(card);
        }

        public void Capture()
        {
            coords.Clear();
            foreach (var (card, border) in registered)
            {
                if (border.RenderTransform is TranslateTransform transform)
                {
                    double y = border.TranslatePoint(new Point(0, 0), _itemsControl).Y;
                    coords[card] = y;
                }
            }
        }

        public void Play(bool isUpping)
        {
            int index = 1;

            var ordered = isUpping
                    ? registered.OrderBy(x => x.Value.TranslatePoint(new Point(0, 0), _itemsControl).Y)
                    : registered.OrderByDescending(x => x.Value.TranslatePoint(new Point(0, 0), _itemsControl).Y);

            foreach (var (card, border) in ordered)
            {
                if (border.RenderTransform is TranslateTransform transform && coords.ContainsKey(card))
                {
                    double oldY = coords[card];
                    double newY = border.TranslatePoint(new Point(0, 0), _itemsControl).Y;
                    double deltaY = oldY - newY;

                    transform.Y = deltaY;

                    if (Math.Abs(deltaY) > 0.1)
                    {
                        var animation = new DoubleAnimation
                        {
                            From = deltaY,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(300 + 40 * index),
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                        };

                        transform.BeginAnimation(TranslateTransform.YProperty, animation);
                    }
                }
                index++;
            }
        }
    }
}
