using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Controls
{
    public partial class SecurityCard : UserControl
    {
        public SecurityCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SecurityCard),
            new PropertyMetadata("TITLE/NF")
        );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(string),
            typeof(SecurityCard),
            new PropertyMetadata("/Assets/icons/default-icon.png")
        );

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty AccentColorProperty = DependencyProperty.Register(
            nameof(AccentColor),
            typeof(Color),
            typeof(SecurityCard),
            new PropertyMetadata(Colors.White)
        );

        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public static readonly DependencyProperty NavButtonProperty = DependencyProperty.Register(
            nameof(NavButton),
            typeof(string),
            typeof(SecurityCard),
            new PropertyMetadata("Настройки")
        );

        public string NavButton
        {
            get => (string)GetValue(NavButtonProperty) == null ? string.Empty : (string)GetValue(NavButtonProperty);
            set => SetValue(NavButtonProperty, value);
        }

        public static readonly DependencyProperty CardContentProperty = DependencyProperty.Register(
            nameof(CardContent),
            typeof(object),
            typeof(SecurityCard)
        );

        public object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        public static readonly DependencyProperty NavPageProperty = DependencyProperty.Register(
            nameof(NavPage),
            typeof(NavigationPage),
            typeof(SecurityCard));

        public NavigationPage NavPage
        {
            get => (NavigationPage)GetValue(NavPageProperty);
            set => SetValue(NavPageProperty, value);
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigatePage(NavPage);
        }
    }
}
