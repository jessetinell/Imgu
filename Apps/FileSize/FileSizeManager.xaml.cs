using System;
using Imgu.Menu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.ComponentModel;

namespace Imgu.Apps.FileSize
{
	public partial class FileSizeManager : ISwitchable
	{
        #region Fields
        private string _root;
        private string _mbOrGb = "MB";
        public ObservableCollection<File> Files
        {
            get { return (ObservableCollection<File>)GetValue(FileProperties); }
            set { SetValue(FileProperties, value); }
        }

        public static readonly DependencyProperty FileProperties =
            DependencyProperty.Register("Files", typeof(ObservableCollection<File>),
            typeof(FileSizeManager), new UIPropertyMetadata(new ObservableCollection<File>()));
        #endregion
		public FileSizeManager()
		{
            InitializeComponent();
            SetSliderMaxValues();
            sliderSelectSize.Value = 500;
		}

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	Switcher.Switch(new Imgu.Menu.MainMenu());
        }
        #endregion

        #region Methods
        void SortFolder(string root, long bytes)
        {
            if (root != null)
            {
                // Data structure to hold names of subfolders to be examined for files.
                var dirs = new Stack<string>();
                dirs.Push(root);
                while (dirs.Count > 0)
                {
                    var currentDir = dirs.Pop();
                    var subDirs = Directory.GetDirectories(currentDir);
                    var files = Directory.GetFiles(currentDir);
                    foreach (var file in files)
                    {
                        var fi = new FileInfo(file);
                        if (fi.Length > bytes)
                            Files.Add(new File
                            {
                                Bytes = fi.Length,
                                DirectoryName = fi.DirectoryName,
                                Size = fi.Length.ToFileSize(),
                                FileName = fi.Name
                            });
                    }
                    foreach (var str in subDirs)
                        dirs.Push(str);
                }
            }
            if (Files.Count == 0)
            {
                Files.Add(new File { FileName = "Nada" });
            }
        }

        void SetSliderMaxValues()
        {
            if (checkBoxGigabyte != null && checkBoxGigabyte.IsChecked == true)
            {
                //Gigabyte
                sliderSelectSize.Maximum = 10;
            }
            else
            {
                //Megabyte
                sliderSelectSize.Maximum = 1000;
            }
        }

        private void SortListView()
        {
            var view = CollectionViewSource.GetDefaultView(Files);
            var sortDescription = new SortDescription("Bytes", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }
        #endregion

        #region Eventshandlers
        private void CheckBoxGigabyteClick(object sender, RoutedEventArgs e)
        {
            _mbOrGb = _mbOrGb == "MB" ? "GB" : "MB";
            labelSize.Content = sliderSelectSize.Value + " " + _mbOrGb;
            SetSliderMaxValues();
        }

        private void ButtonRefreshClick(object sender, RoutedEventArgs e)
        {
            Files.Clear();
            var selectedSize = (long)sliderSelectSize.Value;
            var bytes = checkBoxGigabyte.IsChecked == true ? selectedSize.GigabytesToBytes() : selectedSize.MegabytesToBytes();
            SortFolder(_root, bytes);
        }

        private void ListViewShowFilesMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItems = listViewShowFiles.SelectedItems;
            foreach (File file in selectedItems)
            {
                Process.Start(file.DirectoryName);
            }
        }

        private void ButtonChooseFolderClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _root = dialog.SelectedPath;
                var selectedSize = (long)sliderSelectSize.Value;
                var bytes = checkBoxGigabyte.IsChecked == true ? selectedSize.GigabytesToBytes() : selectedSize.MegabytesToBytes();
                SortFolder(_root, bytes);
            }
            SortListView();
        }

        private void SliderSelectSizeValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelSize.Content = sliderSelectSize.Value + " " + _mbOrGb;
        }
        #endregion

        private void ListViewShowFilesDrop(object sender, System.Windows.DragEventArgs e)
        {
            Files.Clear();
            var folder = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, false);
            if (folder == null) return;
            var fa = System.IO.File.GetAttributes(folder[0]);
            if ((fa & FileAttributes.Directory) != FileAttributes.Directory) return; //Check if is folder or file
            _root = folder[0];
            var selectedSize = (long)sliderSelectSize.Value;
            var bytes = checkBoxGigabyte.IsChecked == true ? selectedSize.GigabytesToBytes() : selectedSize.MegabytesToBytes();
            SortFolder(_root, bytes);
            SortListView();
        }
	}
}