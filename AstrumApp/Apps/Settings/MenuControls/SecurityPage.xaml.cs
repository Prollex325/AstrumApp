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

namespace AstrumApp.Apps.Settings.MenuControls
{
    public partial class SecurityPage : UserControl
    {
        public SecurityPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Debug.WriteLine(App.Security.Pin.PinStatusText);
            };
        }
    }
}
