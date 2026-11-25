using System;
using System.Collections.Generic;
using System.Text;
using Velopack;

namespace AstrumApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            VelopackApp.Build().SetAutoApplyOnStartup(false).Run();

            App.InitializeUpdates();

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
