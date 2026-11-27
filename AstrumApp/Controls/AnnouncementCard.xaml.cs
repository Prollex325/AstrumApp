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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Controls
{
    public enum AnnouncementCategory
    {
        News,
        Update,
        Event,
        Important,
        Maintenance
    }
    public enum ActionTypes
    {
        None = 0,
        OpenUrl,
        OpenUpdates,
        OpenApp,
        OpenDetails
    }

    public partial class AnnouncementCard : UserControl
    {
        public event Action<ActionTypes, string>? ActionRequested;

        public AnnouncementCard()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(AnnouncementCard),
            new PropertyMetadata("TITLE/NF")
        );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(AnnouncementCard),
            new PropertyMetadata(string.Empty)
        );

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(AnnouncementCard)
        );

        public ImageSource? ImageSource
        {
            get => (ImageSource?)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
           nameof(Category),
           typeof(AnnouncementCategory),
           typeof(AnnouncementCard),
           new PropertyMetadata(AnnouncementCategory.News)
        );

        public AnnouncementCategory Category
        {
            get => (AnnouncementCategory)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        public static readonly DependencyProperty PublishedAtProperty = DependencyProperty.Register(
            nameof(PublishedAt),
            typeof(DateTime),
            typeof(AnnouncementCard),
            new PropertyMetadata(DateTime.MinValue)
        );

        public DateTime PublishedAt
        {
            get => (DateTime)GetValue(PublishedAtProperty);
            set => SetValue(PublishedAtProperty, value);
        }

        public static readonly DependencyProperty ActionTextProperty = DependencyProperty.Register(
            nameof(ActionText),
            typeof(string),
            typeof(AnnouncementCard),
            new PropertyMetadata(string.Empty)
        );

        public string ActionText
        {
            get => (string)GetValue(ActionTextProperty);
            set => SetValue(ActionTextProperty, value);
        }

        public static readonly DependencyProperty ActionParameterProperty = DependencyProperty.Register(
            nameof(ActionParameter),
            typeof(string),
            typeof(AnnouncementCard),
            new PropertyMetadata(string.Empty)
        );

        public string ActionParameter
        {
            get => (string)GetValue(ActionParameterProperty);
            set => SetValue(ActionParameterProperty, value);
        }

        public static readonly DependencyProperty ActionTypeProperty = DependencyProperty.Register(
           nameof(ActionType),
           typeof(ActionTypes),
           typeof(AnnouncementCard),
           new PropertyMetadata(ActionTypes.None)
        );

        public ActionTypes ActionType
        {
            get => (ActionTypes)GetValue(ActionTypeProperty);
            set => SetValue(ActionTypeProperty, value);
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionRequested?.Invoke(ActionType, ActionParameter);
        }
    }
}
