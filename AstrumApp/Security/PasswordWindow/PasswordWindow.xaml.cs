using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AstrumApp.Security.MessageWindow
{
    public partial class PasswordWindow : Window
    {
        private MessageBoxType _messageType;
        private MessageBoxButton _messageButtons;
        private MainWindow? _mainWindow;

        public PasswordWindow()
        {
            InitializeComponent();
        }
        /*public static class PasswordBox
        {
            public static MessageBoxResult Show(string messageText = "Текст ошибки", MessageBoxButton buttons = MessageBoxButton.OK,
                MessageBoxType type = MessageBoxType.Error)
            {
                var messageWindow = new PasswordWindow(messageText, buttons, type);
                messageWindow.Owner = Application.Current.MainWindow;
                return messageWindow.ShowWindow();
            }
        }
        public PasswordWindow() : this("Текст ошибки", MessageBoxButton.OK, MessageBoxType.Error)
        {
        }

        public PasswordWindow(string messageText, MessageBoxButton buttons, MessageBoxType type)
        {
            InitializeComponent();

            _messageType = type;
            _messageButtons = buttons;
            MessageTextBlock.Text = messageText;

            _mainWindow = Application.Current.MainWindow as MainWindow;
        }*/
    }
}
