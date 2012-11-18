using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using Imgu.Properties;
using System.Windows.Threading;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace Imgu.Menu
{
	public partial class MainMenu : System.Windows.Controls.UserControl, ISwitchable
	{
		public MainMenu()
		{
			InitializeComponent();
            DataContext = this;
            textBlockCounter.Text = "";
            labelCopyingStatus.Content = "";
		}
        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
        #endregion

		private void optionsButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Switcher.Switch(new LoadGame());
		}

		private void optionButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Switcher.Switch(new Option());
		}


        #region Fields
        public string TargetFolder { get; set; }
        public string NoDataFolder { get; set; }
        public int Current;
        FileProperties _fp;
        readonly SetFolderIcon _sfi = new SetFolderIcon();
        readonly ObservableCollection<FileProperties> _theImages = new ObservableCollection<FileProperties>();
        #endregion

        #region Choose-Files-button
        private void ChooseFilesClick(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TargetFolder))
            {
                using (var open = new System.Windows.Forms.OpenFileDialog())
                {
                    open.Multiselect = true;
                    open.ShowDialog();

                    foreach (string file in open.FileNames)
                    {
                        SortFile(file, open.SafeFileName);
                    }
                }
            }
            else
            {
                openSettings();
            }
            CountUploadedFiles(Current = 0, true, "");
            listBoxChosenFiles.DataContext = _theImages;
        }
        #endregion

        #region SortFile-function
        private void SortFile(string file, string filename)
        {
            string format = file.Substring(file.Length - 3).ToLower();
            string[] imageFormats = { "jpg", "peg", "gif", "png" };
            string[] videoFormats = { "avi", "3gp", "mov", "mp4", "mpg", "mkv", "wmv", ".rm", "vob", "amr", "bmp" };
            if (imageFormats.Contains(format))
            {
                CreateImageObject(file, filename);
                listBoxChosenFiles.Items.Add(file);
                CountUploadedFiles(++Current, false, "");
            }
            else if (videoFormats.Contains(format))
            {
                CreateFileObject(file);
                listBoxChosenFiles.Items.Add(file);
                CountUploadedFiles(++Current, false, "");
            }
            else
                ShowProblemFiles(file);
        }
        #endregion

        #region Create File Object
        FileInfo _fi;
        private void CreateFileObject(string file)
        {
            _fp = new FileProperties();
            _fi = new FileInfo(file);
            _fp.DateTaken = _fi.LastWriteTime;
            _fp.FullImagePath = _fi.FullName;
            _fp.FileName = _fi.Name;
            _fp.FolderPath = _fi.DirectoryName;
            _theImages.Add(_fp);
        }
        #endregion

        #region Create Image Object
        FileStream _fs;
        BitmapDecoder _decoder;
        BitmapFrame _frame;
        BitmapMetadata _metadata;
        public void CreateImageObject(string img, string filename)
        {
            _fp = new FileProperties();
            _fs = new FileStream(img, FileMode.Open);
            _decoder = BitmapDecoder.Create(_fs, BitmapCreateOptions.None, BitmapCacheOption.Default);
            _frame = _decoder.Frames[0];
            _metadata = _frame.Metadata as BitmapMetadata;
            if (_metadata != null && _metadata.DateTaken != null)
            {
                _fp.DateTaken = DateTime.Parse(_metadata.DateTaken);
                _fp.FullImagePath = img;
            }
            else
            {
                _fp.FullImagePath = img;
            }
            _fp.FileName = filename;
            _fp.FolderPath = Path.GetDirectoryName(img);
            _theImages.Add(_fp);
            _fs.Close();
        }
        #endregion

        #region Flytta filer
        private void MoveClick(object sender, RoutedEventArgs e)
        {
            foreach (FileProperties img in _theImages)
            {
                if (img.DateTaken != DateTime.Parse("0001-01-01 00:00:00"))
                {
                    CopyFiles(img);
                }

                if (img.DateTaken == DateTime.Parse("0001-01-01 00:00:00"))
                {
                    CopyNoMetaDataFiles(img);
                }
                CountUploadedFiles(++Current, true, img.FileName);
            }
            _theImages.Clear();
            Current = 0;
            listBoxChosenFiles.Items.Clear();
            labelCopyingStatus.Content = "";
        }
        #endregion

        #region Window loaded
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //Ifall det finns en målmapp sparad.
            if (Settings.Default.DestinationDirectory != null)
            {
                TargetFolder = Settings.Default.DestinationDirectory;
                NoDataFolder = Settings.Default.MiscFolder;
                //Title = "Destination: " + TargetFolder;
            }
            else
            {
                //var sp = new SettingsPage();
                //sp.Show();
            }
            expanderShowFolder.IsExpanded = Settings.Default.ShowCreatedFolderExpander;
        }

        #endregion

        #region Count uploaded files
        public void CountUploadedFiles(int counter, bool result, string fileName)
        {
            #region Needed for counter
            var dispatcherFrame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
            DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object arg)
            {
                var f = arg as DispatcherFrame;
                if (f != null) f.Continue = false;
            },
            dispatcherFrame
            );
            Dispatcher.PushFrame(dispatcherFrame);
            #endregion
            if (result)
            {
                textBlockCounter.Text = counter.ToString(CultureInfo.InvariantCulture) + " / " + _theImages.Count;
                if (fileName != "")
                    labelCopyingStatus.Content = "Kopierar " + fileName;
                else
                    labelCopyingStatus.Content = "";
            }
            else
                textBlockCounter.Text = counter.ToString(CultureInfo.InvariantCulture);
        }
        #endregion

        #region Copy Files
        void CopyNoMetaDataFiles(FileProperties file)
        {
            string targetYear = TargetFolder + "\\" + DateTime.Now.Year;
            string month = DateTime.Now.ToString("MMMM").UppercaseFirst();
            string monthNumber = DateTime.Now.Month.ToString(CultureInfo.InvariantCulture);
            string targetMonth = targetYear + "\\" + monthNumber + ". " + month;
            targetMonth.Substring(0, 1).ToUpper();
            string whatEver = targetMonth + "\\" + NoDataFolder;
            //Om ÅR inte finns
            if (!Directory.Exists(targetYear))
            {
                Directory.CreateDirectory(targetYear);
                _sfi.setFolderIcon(targetYear, 43);
                CreatedFolder(DateTime.Now.Year.ToString(CultureInfo.InvariantCulture));
            }

            //om MÅNAD inte finns
            if (!Directory.Exists(targetMonth))
            {
                Directory.CreateDirectory(targetMonth);
                _sfi.setFolderIcon(targetMonth, 0);
                CreatedFolder(monthNumber + ". " + month);
            }

            //om WHATEVA-mappen inte finns
            if (!Directory.Exists(whatEver))
            {
                Directory.CreateDirectory(whatEver);
                CreatedFolder(NoDataFolder);
            }

            //Om dag finns, kopiera in fil
            if (Directory.Exists(whatEver))
            {
                file.FileName = Path.GetFileName(file.FileName);
                if (file.FileName != null) whatEver = Path.Combine(whatEver, file.FileName);
                File.Copy(file.FullImagePath, whatEver, true);
            }
        }
        void CopyFiles(FileProperties file)
        {
            string targetYear = TargetFolder + "\\" + file.DateTaken.Year;
            string month = file.DateTaken.ToString("MMMM").UppercaseFirst();
            string monthNumber = file.DateTaken.Month.ToString(CultureInfo.InvariantCulture);
            string targetMonth = targetYear + "\\" + monthNumber + ". " + month;
            string targetDay;

            string day;

            //For images taken on late nights ex. on a party. Copies the image to the same day as the party started.
            if (file.DateTaken.Hour < 4 && file.DateTaken.Day != 1)
            {
                int lateNight = file.DateTaken.Day - 1;
                targetDay = targetMonth + "\\" + lateNight;
                day = lateNight.ToString(CultureInfo.InvariantCulture) + " --Sen kväll";
            }
            else
            {
                targetDay = targetMonth + "\\" + file.DateTaken.Day;
                day = file.DateTaken.Day.ToString(CultureInfo.InvariantCulture);
            }

            //Om ÅR inte finns
            if (!Directory.Exists(targetYear))
            {
                Directory.CreateDirectory(targetYear);
                _sfi.setFolderIcon(targetYear, 43);
                CreatedFolder(file.DateTaken.Year.ToString(CultureInfo.InvariantCulture));
            }

            //om MÅNAD inte finns
            if (!Directory.Exists(targetMonth))
            {
                Directory.CreateDirectory(targetMonth);
                _sfi.setFolderIcon(targetMonth, 0);
                CreatedFolder(monthNumber + ". " + month);
            }

            //om DAG inte finns, skapa mappen
            if (!Directory.Exists(targetDay))
            {
                Directory.CreateDirectory(targetDay);
                CreatedFolder(day);
            }

            //Om dag finns, kopiera in fil
            if (Directory.Exists(targetDay))
            {
                file.FileName = Path.GetFileName(file.FullImagePath);
                if (file.FileName != null) targetDay = Path.Combine(targetDay, file.FileName);
                File.Copy(file.FullImagePath, targetDay, true);
            }
        }
        void CreatedFolder(string folder)
        {
            listBoxCreatedFolders.Items.Add(folder);
        }
        void ShowProblemFiles(string file)
        {
            if (!file.EndsWith("db"))
                listBoxChosenFiles.Items.Add(file + "---------FILEN KAN INTE FLYTTAS---------");
        }
        #endregion

        #region Sort Folder
        private void ButtonChooseFolderClick(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TargetFolder))
            {
                var folder = new FolderBrowserDialog();
                folder.ShowDialog();
                if (folder.SelectedPath != "")
                {
                    SortFolder(folder.SelectedPath);
                }
            }
            else
            {
                openSettings();
            }
        }
        void SortFolder(string root)
        {
            // Data structure to hold names of subfolders to be
            // examined for files.
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
                    SortFile(file, fi.Name);
                }
                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (var str in subDirs)
                    dirs.Push(str);
            }
            CountUploadedFiles(Current = 0, true, "");
        }
        #endregion

        #region Open Settings
        private void ButtonSettingsClick(object sender, RoutedEventArgs e)
        {
            openSettings();
        }
        void openSettings()
        {
            //var settingsPage = new SettingsPage();
            //settingsPage.Show();
        }
        #endregion

        #region Drag-n-Drop
        private void ListBoxChosenFilesDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                if (!String.IsNullOrEmpty(TargetFolder))
                {
                    var droppedFilePaths =
                    e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];

                    if (droppedFilePaths != null)
                    {
                        var attr = File.GetAttributes(droppedFilePaths[0]);

                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            SortFolder(droppedFilePaths[0]);
                        }
                        else
                        {
                            foreach (var droppedFilePath in droppedFilePaths)
                            {
                                new ListBoxItem();
                                var fi = new FileInfo(droppedFilePath);
                                SortFile(droppedFilePath, fi.Name);
                            }
                        }
                    }
                    if (droppedFilePaths != null) Array.Clear(droppedFilePaths, 0, droppedFilePaths.Length);
                    CountUploadedFiles(Current = 0, true, "");
                    LsImageGallery.DataContext = _theImages.OrderBy(f=>f.DateTaken);
                }
                else
                {
                    openSettings();
                }
            }
        }
        #endregion

        #region Expander for showing created folders
        private void ExpanderShowFolderExpanded(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowCreatedFolderExpander = true;
            Settings.Default.Save();
        }
        private void ExpanderShowFolderCollapsed(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowCreatedFolderExpander = false;
            Settings.Default.Save();
        }
        #endregion
		
	}
}