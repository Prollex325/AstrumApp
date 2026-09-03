using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Services
{
    using MediaColor = System.Windows.Media.Color;
    public static class SwitchBehavior
    {
        public static readonly DependencyProperty EnableAnimationProperty =
            DependencyProperty.RegisterAttached(
                "EnableAnimation",
                typeof(bool),
                typeof(SwitchBehavior),
                new PropertyMetadata(false, OnChanged));

        public static void SetEnableAnimation(DependencyObject obj, bool value)
            => obj.SetValue(EnableAnimationProperty, value);

        public static bool GetEnableAnimation(DependencyObject obj)
            => (bool)obj.GetValue(EnableAnimationProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleButton btn && (bool)e.NewValue)
            {
                btn.Checked += OnChecked;
                btn.Unchecked += OnUnchecked;
                btn.Loaded += OnLoaded;
                btn.SizeChanged += OnSizeChanged;
            }
        }

        private static void OnChecked(object sender, RoutedEventArgs e)
        {
            AnimateSwitch(sender as ToggleButton, true);
        }

        private static void OnUnchecked(object sender, RoutedEventArgs e)
        {
            AnimateSwitch(sender as ToggleButton, false);
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            Update(sender as ToggleButton);
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update(sender as ToggleButton);
        }

        private static void AnimateSwitch(ToggleButton btn, bool isOn)
        {
            AnimatePosition(btn, isOn);
            AnimateColor(btn, isOn);
        }

        private static void AnimatePosition(ToggleButton btn, bool isOn)
        {
            var transform = btn.Template.FindName("ThumbTransform", btn) as TranslateTransform;

            double offset = btn.ActualWidth - btn.ActualHeight;

            var anim = new DoubleAnimation
            {
                To = isOn ? offset : 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            transform?.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private static void AnimateColor(ToggleButton btn, bool isOn)
        {
            var bg = btn.Template.FindName("SwitchBackground", btn) as Border;
            var thumb = btn.Template.FindName("SwitchThumb", btn) as Border;

            if (bg == null || thumb == null) return;

            var bgColor = new ColorAnimation
            {
                To = isOn ? GetActiveBackground(btn) : GetInActiveBackground(btn),
                Duration = TimeSpan.FromMilliseconds(200)
            };

            var thumbColor = new ColorAnimation
            {
                To = isOn ? GetActiveThumb(btn) : GetInActiveThumb(btn),
                Duration = TimeSpan.FromMilliseconds(200)
            };

            (bg.Background as SolidColorBrush)?.BeginAnimation(SolidColorBrush.ColorProperty, bgColor);
            (thumb.Background as SolidColorBrush)?.BeginAnimation(SolidColorBrush.ColorProperty, thumbColor);
        }

        private static void Update(ToggleButton btn)
        {
            if (btn == null) return;

            var transform = btn.Template.FindName("ThumbTransform", btn) as TranslateTransform;
            if (transform == null) return;

            double offset = btn.ActualWidth - btn.ActualHeight;

            // запоминаем где "включено"
            btn.Tag = offset;
        }

        public static readonly DependencyProperty ActiveBackgroundProperty = DependencyProperty.RegisterAttached(
            "ActiveBackground",
            typeof(MediaColor),
            typeof(SwitchBehavior),
            new PropertyMetadata(Colors.DodgerBlue));

        public static void SetActiveBackground(DependencyObject element, MediaColor value)
        {
            element.SetValue(ActiveBackgroundProperty, value);
        }

        public static MediaColor GetActiveBackground(DependencyObject element)
        {
            return (MediaColor)element.GetValue(ActiveBackgroundProperty);
        }

        public static readonly DependencyProperty InActiveBackgroundProperty = DependencyProperty.RegisterAttached(
            "InActiveBackground",
            typeof(MediaColor),
            typeof(SwitchBehavior),
            new PropertyMetadata(MediaColor.FromRgb(20, 27, 34)));

        public static void SetInActiveBackground(DependencyObject element, MediaColor value)
        {
            element.SetValue(InActiveBackgroundProperty, value);
        }

        public static MediaColor GetInActiveBackground(DependencyObject element)
        {
            return (MediaColor)element.GetValue(InActiveBackgroundProperty);
        }

        public static readonly DependencyProperty ActiveThumbProperty = DependencyProperty.RegisterAttached(
            "ActiveThumb",
            typeof(MediaColor),
            typeof(SwitchBehavior),
            new PropertyMetadata(Colors.White));

        public static void SetActiveThumb(DependencyObject element, MediaColor value)
        {
            element.SetValue(ActiveThumbProperty, value);
        }

        public static MediaColor GetActiveThumb(DependencyObject element)
        {
            return (MediaColor)element.GetValue(ActiveThumbProperty);
        }


        public static readonly DependencyProperty InActiveThumbProperty = DependencyProperty.RegisterAttached(
            "InActiveThumb",
            typeof(MediaColor),
            typeof(SwitchBehavior),
            new PropertyMetadata(MediaColor.FromRgb(128, 128, 128)));

        public static void SetInActiveThumb(DependencyObject element, MediaColor value)
        {
            element.SetValue(InActiveThumbProperty, value);
        }

        public static MediaColor GetInActiveThumb(DependencyObject element)
        {
            return (MediaColor)element.GetValue(InActiveThumbProperty);
        }
    }
}
