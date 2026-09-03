using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Controls
{
    public partial class InputField : UserControl
    {
        public string Text { get; private set; } = string.Empty;
        private bool _hasErrored = false;
        public InputField()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (AcceptsReturn)
                {
                    IsPassword = false;
                    CounterTextBlock.Text = MaxLength.ToString();
                    CounterTextBlock.Visibility = Visibility.Visible;
                }

                if (IsPassword)
                {
                    HidePasswordBox.Visibility = Visibility.Visible;
                    VisiblePasswordBox.Visibility = Visibility.Collapsed;

                    HidePasswordBox.Password = Text;
                }
                else
                {
                    HidePasswordBox.Visibility = Visibility.Collapsed;
                    VisiblePasswordBox.Visibility = Visibility.Visible;
                }

                PlaceholderTextBlock.Visibility = string.IsNullOrEmpty(Placeholder) ? Visibility.Collapsed : Visibility.Visible;

                PasswordButton.Visibility = ShowPasswordButton ? Visibility.Visible : Visibility.Collapsed;

                CounterTextBlock.Text = (MaxLength - VisiblePasswordBox.Text.Length).ToString();
            };
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            bool visible = PasswordButton.IsChecked ?? false;

            if (visible)
            {
                HidePasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;

                VisiblePasswordBox.Text = HidePasswordBox.Password;
                VisiblePasswordBox.Focus();
                VisiblePasswordBox.CaretIndex = VisiblePasswordBox.Text.Length; // отправляем в конец строки
            }
            else
            {
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                HidePasswordBox.Visibility = Visibility.Visible;

                HidePasswordBox.Password = VisiblePasswordBox.Text;
                HidePasswordBox.Focus();

                MoveCaretToEnd(HidePasswordBox); // у passwordbox нет такого метода поэтому так
            }
        }

        private void VisiblePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = VisiblePasswordBox.Text;
            CounterTextBlock.Text = (MaxLength - VisiblePasswordBox.Text.Length).ToString();
        }

        private void HidePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Text = HidePasswordBox.Password;
        }

        public void RaiseError(string errorText)
        {
            Color errorTextBlock = (Color)ColorConverter.ConvertFromString("#FF7A85");
            /*Color errorBorderBrush = (Color)ColorConverter.ConvertFromString("#E05260");

            var passwordBoxBorderBrush = new SolidColorBrush(HidePasswordBox.BorderBrush.);
            var visibleTextBoxBorderBrush = VisiblePasswordBox.BorderBrush.Clone() as SolidColorBrush;

            HidePasswordBox.BorderBrush = passwordBoxBorderBrush;
            VisiblePasswordBox.BorderBrush = visibleTextBoxBorderBrush;

            ColorAnimation borderBrushAnim = new ColorAnimation
            {
                To = errorBorderBrush,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            

            //passwordBoxBorderBrush?.BeginAnimation(SolidColorBrush.ColorProperty, borderBrushAnim);
            visibleTextBoxBorderBrush?.BeginAnimation(SolidColorBrush.ColorProperty, borderBrushAnim);*/

            var textOpacityAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(50)
            };

            InfoText.BeginAnimation(UIElement.OpacityProperty, textOpacityAnim);

            InfoText.Foreground = new SolidColorBrush(errorTextBlock);
            InfoText.Visibility = Visibility.Visible;
            InfoText.Text = errorText;

            _hasErrored = true;
        }

        public async Task RaiseSuccess(string successText)
        {
            if (!_hasErrored) return;

            Color errorTextBlock = (Color)ColorConverter.ConvertFromString("#65D99A"); // ##3DBE78

            var textOpacityAnim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(50)
            };

            InfoText.BeginAnimation(UIElement.OpacityProperty, textOpacityAnim);

            InfoText.Foreground = new SolidColorBrush(errorTextBlock);
            //InfoText.Text = successText;

            _hasErrored = false;

            await Task.Delay(2000);

            var textOpacityAnimHide = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(50)
            };

            InfoText.BeginAnimation(UIElement.OpacityProperty, textOpacityAnimHide);
        }

        private void MoveCaretToEnd(PasswordBox passwordBox)
        {
            var selectMethod = passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);

            selectMethod?.Invoke(passwordBox, [passwordBox.Password.Length, 0]);
        }


        public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(
            nameof(IsPassword),
            typeof(bool),
            typeof(InputField),
            new PropertyMetadata(false)
        );

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public static readonly DependencyProperty ShowPasswordButtonProperty = DependencyProperty.Register(
            nameof(ShowPasswordButton),
            typeof(bool),
            typeof(InputField),
            new PropertyMetadata(false)
        );

        public bool ShowPasswordButton
        {
            get => (bool)GetValue(ShowPasswordButtonProperty);
            set => SetValue(ShowPasswordButtonProperty, value);
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(InputField),
            new PropertyMetadata("")
        );

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public static readonly DependencyProperty TextContentProperty = DependencyProperty.Register(
            nameof(TextContent),
            typeof(string),
            typeof(InputField),
            new FrameworkPropertyMetadata(
                "",
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string TextContent
        {
            get => (string)GetValue(TextContentProperty);
            set => SetValue(TextContentProperty, value);
        }

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
            nameof(MaxLength),
            typeof(int),
            typeof(InputField),
            new PropertyMetadata(254)
        );

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(
            nameof(AcceptsReturn),
            typeof(bool),
            typeof(InputField),
            new PropertyMetadata(false));

        public bool AcceptsReturn
        {
            get => (bool)GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            nameof(TextWrapping),
            typeof(TextWrapping),
            typeof(InputField),
            new PropertyMetadata(TextWrapping.NoWrap));

        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(
            nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(InputField),
            new PropertyMetadata(ScrollBarVisibility.Hidden));

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }
    }
}
