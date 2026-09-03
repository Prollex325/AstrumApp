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

namespace AstrumApp.Apps.Authentication
{
    public partial class MainAuthWindow : Page
    {
        public static MainAuthWindow Instance { get; private set; }
        public MainAuthWindow()
        {
            InitializeComponent();

            Instance = this;

            NavigateToLogin();
        }

        public void NavigateToBio()
        {
            Main.Navigate(new BioPage());
        }

        public void NavigateToReg()
        {
            Main.Navigate(new RegistrationPage());
        }

        public void NavigateToLogin()
        {
            Main.Navigate(new LoginPage());
        }
    }
}
