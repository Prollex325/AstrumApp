using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AstrumApp
{
    public enum MessageBoxType
    {
        Info,
        Warning,
        Error,
        Destructive
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
        public static MessageBoxResult Show(string messageText = "Текст ошибки", string headerText = "", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxType type = MessageBoxType.Error)
        {
            var messageWindow = new MessageWindow(messageText, headerText, buttons, type);
            messageWindow.Owner = Application.Current.MainWindow;
            return messageWindow.ShowWindow();
        }
    }

    public partial class MessageWindow : Window
    {
        public MessageBoxType MessageType => _messageType;
        private MessageBoxType _messageType;
        private MessageBoxButton _messageButtons;
        private MainWindow? _mainWindow;
        DispatcherTimer timer = new DispatcherTimer();
        private int _counter = 7;

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public MessageWindow() : this("Текст ошибки", "", MessageBoxButton.OK, MessageBoxType.Error)
        {
        }

        public MessageWindow(string messageText, string headerText, MessageBoxButton buttons, MessageBoxType type)
        {
            _messageType = type;
            _messageButtons = buttons;

            InitializeComponent();

            timer.Tick += StartTimer;
            timer.Interval = TimeSpan.FromSeconds(1);

            MessageTextBlock.Text = messageText;
            if (!string.IsNullOrEmpty(headerText)) HeaderTextBlock.Text = headerText;
            _mainWindow = Application.Current.MainWindow as MainWindow;
        }

        public MessageBoxResult ShowWindow()
        {
            if (_messageType == MessageBoxType.Destructive)
            {
                NormalDialog.Visibility = Visibility.Collapsed;
                DestructiveDialog.Visibility = Visibility.Visible;

                Width = 550;
                Height = 350;

                _counter = 7;

                DestructiveMessageTextBlock.Text = MessageTextBlock.Text;

                timer.Start();
            }
            else
            {
                NormalDialog.Visibility = Visibility.Visible;
                DestructiveDialog.Visibility = Visibility.Collapsed;

                SelectMessageType();
                CreateButtons();
            }

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
                case MessageBoxType.Destructive:
                    //iconPath = "/Assets/dialogs/error.png";
                    //iconText = "Опасное действие";
                    break;
            }

            HeaderIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Relative));
            HeaderTextBlock.Text = iconText;
        }

        private void StartTimer(object? sender, EventArgs e)
        {
            if (_counter == 0)
            {
                DestructiveConfirmButton.Content = "Удалить";
                DestructiveConfirmButton.IsEnabled = true;
                timer.Stop();
                return;
            }

            DestructiveConfirmButton.Content = $"Удалить ({_counter})";
            _counter--;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_messageButtons == MessageBoxButton.OKCancel || _messageButtons == MessageBoxButton.YesNoCancel)
                {
                    CancelButton_Click(new(), new());
                }
                else if (_messageButtons == MessageBoxButton.YesNo)
                {
                    NegativeButton_Click(new(), new());
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && _messageType != MessageBoxType.Destructive)
            {
                PositiveButton_Click(new(), new());
                e.Handled = true;
            }
        }
    }
}
