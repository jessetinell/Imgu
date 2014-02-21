using System;
using System.Windows;
using System.Windows.Forms;
using Imgu.Properties;

namespace Imgu.Menu
{
	public partial class AppSettings : ISwitchable
	{
		public AppSettings()
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

        private void ButtonBackClick(object sender, RoutedEventArgs e)
        {
        	Switcher.Switch(new MainMenu());
        }
        #endregion

        private void ButtonSelectFolderClick(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog();
            folder.ShowDialog();
            if (folder.SelectedPath == "") return;
            Settings.Default.DestinationDirectory = folder.SelectedPath;
            UpdateTextBlocks();
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.MiscFolder = textBoxNoDataFolder.Text == "" ? "Other" : textBoxNoDataFolder.Text;
            Settings.Default.Save();
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        void UpdateTextBlocks()
        {
            textBlockTargetFolder.Text = Settings.Default.DestinationDirectory;
            textBoxNoDataFolder.Text = Settings.Default.MiscFolder;
            textBoxNoDataFolder.Text = !String.IsNullOrEmpty(Settings.Default.MiscFolder) ? Settings.Default.MiscFolder : "Other";
        }

	}
}