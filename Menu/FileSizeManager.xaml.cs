using System;
using Imgu.Menu;

namespace Imgu
{
	public partial class FileSizeManager : ISwitchable
	{
		public FileSizeManager()
		{
			InitializeComponent();
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
	}
}