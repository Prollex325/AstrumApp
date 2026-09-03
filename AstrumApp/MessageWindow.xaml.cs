using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AstrumApp
{
    public enum MessageBoxType
    {
        Info,
        Warning,
        Error
    }

    public enum MessageBoxButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        None,
        Yes,
        No,
        Cancel
    }

    public static class MessageBox
    {
        public static MessageBoxResult Show(string messageText = "Текст ошибки", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxType type = MessageBoxType.Error)
        {
            var messageWindow = new MessageWindow(messageText, buttons, type);
            messageWindow.Owner = Application.Current.MainWindow;
            return messageWindow.ShowWindow();
        }
    }

    public partial class MessageWindow : Window
    {
        private MessageBoxType _messageType;
        private MessageBoxButton _messageButtons;
        private MainWindow? _mainWindow;

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public MessageWindow() : this("Текст ошибки", MessageBoxButton.OK, MessageBoxType.Error)
        {
        }

        public MessageWindow(string messageText, MessageBoxButton buttons, MessageBoxType type)
        {
            InitializeComponent();

            _messageType = type;
            _messageButtons = buttons;
            MessageTextBlock.Text = messageText;

            _mainWindow = Application.Current.MainWindow as MainWindow;
        }

        public MessageBoxResult ShowWindow() 
        {
            SelectMessageType();
            CreateButtons();

            _mainWindow?.ShowBlackoutMask();

            ShowDialog();

            return Result;
        }

        private void CreateButtons()
        {
            switch (_messageButtons)
            {
                case MessageBoxButton.OK:
                    PositiveButton.Visibility = Visibility.Visible;

                    PositiveButton.Content = "OK";
                    break;
                case MessageBoxButton.OKCancel:
                    PositiveButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;

                    PositiveButton.Content = "OK";
                    break;
                case MessageBoxButton.YesNo:
                    PositiveButton.Visibility = Visibility.Visible;
                    NegativeButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    PositiveButton.Visibility = Visibility.Visible;
                    NegativeButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
            }       
        }

        private void SelectMessageType()
        {
            string iconPath = string.Empty;
            string iconText = string.Empty;

            switch (_messageType) 
            {
                case MessageBoxType.Info:
                    iconPath = "/Assets/dialogs/info.png";
                    iconText = "Информация";
                    break;
                case MessageBoxType.Warning:
                    iconPath = "/Assets/dialogs/warning.png";
                    iconText = "Предупреждение";
                    break;
                case MessageBoxType.Error:
                    iconPath = "/Assets/dialogs/error.png";
                    iconText = "Ошибка";
                    break;
            }

            HeaderIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Relative));
            HeaderTextBlock.Text = iconText;
        }

        private void PositiveButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void NegativeButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mainWindow?.HideBlackoutMask();
        }
    }
}
