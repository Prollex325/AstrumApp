using AstrumApp.Controls;
using AstrumApp.Interfaces;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AstrumApp.Apps.Settings.MenuControls
{
    public partial class AvatarEdit : UserControl, ICanClose
    {
        public AvatarEdit()
        {
            InitializeComponent();

            this.DataContext = App.Session.Profile;

            AvatarPicker.AvatarSaved += image =>
            {
                App.Session.SetAvatar(image);
                App.Navigation.Back();
            };

            AvatarPicker.AvatarDeleted += () =>
            {
                bool res = MessageBox.Show("Вы уверены, что хотите сбросить аватар?", MessageBoxButton.YesNo, MessageBoxType.Warning) == MessageBoxResult.Yes;
                if (!res) return;
                App.Session.ClearAvatar();
                App.Navigation.Back();
            };

            AvatarPicker.EditCancelled += () =>
            {
                App.Navigation.Back();
            };
        }

        public bool CanClose()
        {
            if (AvatarPicker.CanClose()) return true;

            bool res = MessageBox.Show("Изменения могут не сохраниться!\nВы уверены, что хотите выйти?", MessageBoxButton.YesNo, MessageBoxType.Warning) == MessageBoxResult.Yes;

            return res;
        }
    }
}
