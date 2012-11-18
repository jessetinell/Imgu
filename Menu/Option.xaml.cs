using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Imgu.Properties;
using MainMenu = Imgu.Menu.MainMenu;
using UserControl = System.Windows.Controls.UserControl;

namespace Imgu
{
	public partial class Option : UserControl, ISwitchable
	{
		public Option()
		{
			// Required to initialize variables
			InitializeComponent();
            UpdateTextBlocks();
		}

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	Switcher.Switch(new MainMenu());
        }
        #endregion

        private void ButtonSelectFolderClick(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog();
            folder.ShowDialog();
            if (folder.SelectedPath != "")
            {
                //Title = "Destination: " + folder.SelectedPath;
                Settings.Default.DestinationDirectory = folder.SelectedPath;
                UpdateTextBlocks();
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.MiscFolder = textBoxNoDataFolder.Text == "" ? "Övrigt" : textBoxNoDataFolder.Text;
            Settings.Default.Save();
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private void ButtonSelectMonthIconClick(object sender, RoutedEventArgs e)
        {
            var openIcon = new OpenFileDialog { Filter = "Image Files(*.ico;)|*.ico;" };
            openIcon.ShowDialog();
            if (openIcon.FileName == "") return;
            Settings.Default.MonthIcon = openIcon.FileName;
            UpdateTextBlocks();
        }

        void UpdateTextBlocks()
        {
            textBlockTargetFolder.Text = Settings.Default.DestinationDirectory;
            textBoxNoDataFolder.Text = Settings.Default.MiscFolder;
            textBlockMonthIcon.Text = Settings.Default.MonthIcon;
            textBoxNoDataFolder.Text = !String.IsNullOrEmpty(Settings.Default.MiscFolder) ? Settings.Default.MiscFolder : "Övrigt";
        }
	}
}