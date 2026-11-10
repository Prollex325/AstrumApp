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
using System.Windows.Shapes;
using AstrumApp.Services;

namespace AstrumApp.Security.PasswordWindowDir
{
    public enum PasswordDialogResult
    {
        None,
        Success,
        Failure,
        Cancel,
    }

    public static class PasswordDialog
    {
        public static PasswordDialogResult Show(string message)
        {
            PasswordWindow passwordWindow = new(message);
            passwordWindow.Owner = Application.Current.MainWindow;
            return passwordWindow.ShowWindow();
        }
    }


    public partial class PasswordWindow : Window
    {
        private MainWindow? _mainWindow;

        public PasswordDialogResult Result { get; private set; } = PasswordDialogResult.None;


        /*public PasswordWindow() : this("")
        {
        }*/

        public PasswordWindow(string message)
        {
            InitializeComponent();

            _mainWindow = Application.Current.MainWindow as MainWindow;

            if (!string.IsNullOrEmpty(message))
            {
                InfoText.Text = message; 
            }

            PasswordInput.EnterPressed += () =>
            {
                ConfirmButton_Click(new object(), new RoutedEventArgs());
            };
        }

        public PasswordDialogResult ShowWindow()
        {
            _mainWindow?.ShowBlackoutMask();

            ShowDialog();

            return Result;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordInput.Text;

            ConfirmButton.IsEnabled = false;
            PasswordInput.RaiseError("");

            try
            {
                bool isValid = await App.Supabase.VerifyPasswordAsync(password);

                if (!isValid)
                {
                    App.Notifications.Show("Неверный пароль", CardTypes.Error);
                    PasswordInput.RaiseError("*Неверный пароль. Попробуйте снова");

                    PasswordInput.Text = "";
                    PasswordInput.Focus();

                    return; // Окно остаётся открытым
                }

                Result = PasswordDialogResult.Success;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                ConfirmButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = PasswordDialogResult.Cancel;
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mainWindow?.HideBlackoutMask();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(new object(), new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}
