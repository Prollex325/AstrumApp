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
using System.IO;

namespace AstrumApp.Apps.Settings.MenuControls
{//TODO : улучшить стили
    public partial class ChangelogPage : UserControl
    {
        public ChangelogPage()
        {
            InitializeComponent();

            string path = AppPaths.ChangelogPath;

            if (File.Exists(path))
            {
                ChangelogViewer.Markdown = File.ReadAllText(path);

                if (ChangelogViewer.Document is { } document)
                {
                    foreach (var list in document.Blocks.OfType<List>())
                    {
                        list.MarkerStyle = TextMarkerStyle.Disc;
                        list.MarkerOffset = 10;
                    }
                }
            }
            else
            {
                ChangelogViewer.Markdown = "# История обновлений\n\nФайл не найден.";
            }
        }
    }
}
