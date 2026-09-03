using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AstrumApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class Desktop : UserControl
    {

        DispatcherTimer clock = new DispatcherTimer();

        public Desktop()
        {
            InitializeComponent();

            clock.Tick += Clock_Tick;
            clock.Interval = TimeSpan.FromSeconds(1);
            clock.Start();
        }

        private void Clock_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            Clock.Text = now.ToString("HH:mm");
            Date.Text = now.ToString("dddd, dd MMMM");
        }
    }
}
