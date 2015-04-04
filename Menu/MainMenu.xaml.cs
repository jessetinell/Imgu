using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using Imgu.Properties;
using System.Windows.Threading;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using File = System.IO.File;

namespace Imgu.Menu
{
    public partial class MainMenu : ISwitchable
    {
        public MainMenu()
        {
            InitializeComponent();
            DataContext = this;
            textBlockCounter.Text = "";
            labelCopyingStatus.Content = "";
            ButtonClear.Visibility = Visibility.Hidden;
            ButtonEdit.Visibility = Visibility.Hidden;
            ShowFailedFiles.Visibility = Visibility.Hidden;
        }
        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new AppSettings());
        }


        #region Fields
        public string TargetFolder { get; set; }
        public string NoDataFolder { get; set; }
        public int Current;
        FileProperties _fp;
        readonly ObservableCollection<FileProperties> _theImages = new ObservableCollection<FileProperties>();
        readonly List<FileProperties> _failedFiles = new List<FileProperties>();
        #endregion

        #region SortFile-function
        private void SortFile(string file, string filename)
        {
            string format = file.Substring(file.Length - 3).ToLower();
            string[] imageFormats = { "jpg", "peg", "png" };
            string[] videoFormats = { "avi", "3gp", "mov", "mp4", "mpg", "mkv", "wmv", ".rm", "vob", "amr", "bmp" };
            if (imageFormats.Contains(format))
            {
                CreateImageObject(file, filename);
                CountUploadedFiles(++Current, false, "");
            }
            else if (videoFormats.Contains(format))
            {
                CreateFileObject(file);
                CountUploadedFiles(++Current, false, "");
            }
            else
                CreateProblemFileObject(file);
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
            _fp.FileType = FileProperties.FileTypes.Video;
            _theImages.Add(_fp);
        }
        void CreateProblemFileObject(string file)
        {
            if (!file.EndsWith("db") && !file.EndsWith("desktop.ini"))
            {
                _fp = new FileProperties();
                _fi = new FileInfo(file);
                _fp.FileName = _fi.FullName;
                _failedFiles.Add(_fp);
                ShowFailedFiles.Visibility = Visibility.Visible;
            }
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
            _theImages.Clear();
            listBoxChosenFiles.ItemsSource = _theImages;
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
                Switcher.Switch(new AppSettings());
            }
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
                    labelCopyingStatus.Content = "Copying " + fileName;
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
            }

            //om MÅNAD inte finns
            if (!Directory.Exists(targetMonth))
            {
                Directory.CreateDirectory(targetMonth);
            }

            //om WHATEVA-mappen inte finns
            if (!Directory.Exists(whatEver))
            {
                Directory.CreateDirectory(whatEver);
            }

            file.FileName = Path.GetFileName(file.FileName);
            if (file.FileName != null) whatEver = Path.Combine(whatEver, file.FileName);
            File.Copy(file.FullImagePath, whatEver, true);
        }

        void CopyFiles(FileProperties file)
        {
            var targetYear = TargetFolder + "\\" + file.DateTaken.Year;
            var month = file.DateTaken.ToString("MMMM").UppercaseFirst();
            var monthNumber = file.DateTaken.Month.ToString(CultureInfo.InvariantCulture);
            var targetMonth = targetYear + "\\" + monthNumber + ". " + month;

            //Om ÅR inte finns
            if (!Directory.Exists(targetYear))
            {
                Directory.CreateDirectory(targetYear);
            }

            //om MÅNAD inte finns
            if (!Directory.Exists(targetMonth))
            {
                Directory.CreateDirectory(targetMonth);
            }

            var filesThisDay = _theImages.Count(i => i.DateTaken.Year == file.DateTaken.Year && i.DateTaken.Month == file.DateTaken.Month && i.DateTaken.Day == file.DateTaken.Day);
            if (filesThisDay > 20)
            {
                string targetDay;

                //For images taken on late nights ex. on a party. Copies the image to the same day as the party started.
                if (file.DateTaken.Hour < 4 && file.DateTaken.Day != 1)
                {
                    var lateNight = file.DateTaken.Day - 1;
                    targetDay = targetMonth + "\\" + lateNight;
                }
                else
                {
                    targetDay = targetMonth + "\\" + file.DateTaken.Day;
                }

                //om DAG inte finns, skapa mappen
                if (!Directory.Exists(targetDay))
                {
                    Directory.CreateDirectory(targetDay);
                }

                file.FileName = Path.GetFileName(file.FullImagePath);
                if (file.FileName != null) targetDay = Path.Combine(targetDay, file.FileName);
                File.Copy(file.FullImagePath, targetDay, true);
            }
            else
            {
                file.FileName = Path.GetFileName(file.FullImagePath);
                if (file.FileName != null) targetMonth = Path.Combine(targetMonth, file.FileName);
                File.Copy(file.FullImagePath, targetMonth, true);
            }

        }
        #endregion

        #region Sort Folder
        void SortFolder(string root)
        {
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
            Done();
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
                    Done();
                }
                else
                {
                    Switcher.Switch(new AppSettings());
                }
            }
        }
        #endregion

        #region Done
        void Done()
        {
            CountUploadedFiles(Current = 0, true, "");
            if (_theImages.Count > 0)
            {
                listBoxChosenFiles.DataContext = _theImages.OrderBy(f => f.DateTaken);
                ButtonClear.Visibility = Visibility.Visible;
            }
            else
            {
                listBoxChosenFiles.DataContext = _theImages;
                ButtonClear.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region Visa/Dölj problemfiler
        private bool _show;
        private void ShowFailedFilesMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_show)
            {
                ShowFailedFiles.Content = "Visa filer som inte kunde flyttas";
                listBoxChosenFiles.ItemsSource = _theImages.OrderBy(f => f.DateTaken);
                _show = false;
            }
            else
            {
                ShowFailedFiles.Content = "Dölj";
                listBoxChosenFiles.ItemsSource = _failedFiles;
                _show = true;
            }

        }
        #endregion

        #region Misc buttons
        private void ClearClick(object sender, RoutedEventArgs e)
        {
            _theImages.Clear();
            Done();
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = listBoxChosenFiles.SelectedItems;

            var items = (IList<FileProperties>)selectedItems.Cast<FileProperties>().ToList();

            Switcher.Switch(new Edit(_theImages, items));
        }

        private void ListBoxChosenFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonEdit.Visibility = listBoxChosenFiles.SelectedItems.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            listBoxChosenFiles.SelectAll();
        }

        private void DropboxSyncClick(object sender, RoutedEventArgs e)
        {
            var folderPath = Settings.Default.DropboxFolderPath;
            if (string.IsNullOrEmpty(folderPath))
            {
                var dropboxFolder = new FolderBrowserDialog();
                dropboxFolder.ShowDialog();
                if (dropboxFolder.SelectedPath == "") return;
                Settings.Default.DropboxFolderPath = dropboxFolder.SelectedPath;
                LabelDropboxCameraUploads.Visibility = Visibility.Hidden;
                Settings.Default.Save();
            }
            else
            {
                SortFolder(folderPath);
            }
        }
        #endregion

    }
}