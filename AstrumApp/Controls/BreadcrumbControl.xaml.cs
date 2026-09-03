using AstrumApp.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AstrumApp.Controls
{
    public partial class BreadcrumbControl : UserControl
    {
        public ObservableCollection<BreadcrumbItem> BreadcrumbItems { get; set; }
        public BreadcrumbControl()
        {
            InitializeComponent();
            App.Navigation.NavigationChanged += RefreshBreadcrumbs;

            BreadcrumbItems = new ObservableCollection<BreadcrumbItem>();

            DataContext = this;
        }

        private void RefreshBreadcrumbs(List<NavigationPage> history)
        {
            BreadcrumbItems.Clear();

            foreach (var page in history)
            {
                Visibility visibility = page == history.Last() ? Visibility.Collapsed : Visibility.Visible;
                Brush color = page == history.Last() ? Brushes.White : (Brush)new BrushConverter().ConvertFrom("#CFCFCF");
                bool isHitVisible = page != history.Last();
                BreadcrumbItems.Add(new BreadcrumbItem { Name = page.GetTitle(), Visible = visibility, Page = page, Foreground = color, IsHitTestVisible = isHitVisible });
            }
        }

        public void OpenPage(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is BreadcrumbItem item)
            {
                App.Navigation.BreadcrumbNavigate(item.Page);
            }
        }
    }
    public class BreadcrumbItem
    {
        public object Visible { get; set; } = Visibility.Visible;
        public string Name { get; set; } = "Пусто";
        public NavigationPage Page { get; set; }
        public Brush Foreground { get; set; } = Brushes.White;
        public bool IsHitTestVisible { get; set; } = true;
    }
}
