using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Imgu.Menu
{
    public partial class Edit : ISwitchable
    {
        List<FileProperties> _files = new List<FileProperties>();
        readonly List<FileProperties> _conflictedFiles = new List<FileProperties>();
        public Edit(IEnumerable<FileProperties> notSelected, IEnumerable<FileProperties> selectedItems)
        {
            InitializeComponent();
            UpdateTextBlocks(selectedItems);
            LabelError.Visibility = Visibility.Hidden;
        }

        private void UpdateTextBlocks(IEnumerable selectedItems)
        {
            _files = selectedItems as List<FileProperties>;
            listBoxFiles.DataContext = _files;
        }

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void ButtonSetDateClick(object sender, RoutedEventArgs e)
        {
            if (CalendarChooseDate.SelectedDate == null) return;

            var chosenTime = (DateTime)CalendarChooseDate.SelectedDate.Value.AddHours(15);

            foreach (var file in _files)
            {
                var time = file.DateTaken.ToLongTimeString().DenyMidnight();
                var timeString = time;

                switch (file.FileType)
                {
                    case FileProperties.FileTypes.Image:
                        {
                            if (File.Exists(file.FullImagePath))
                            {
                                return;
                            }
                                if(file.FullImagePath.EndsWith("png"))
                                {
                                    MessageBox.Show("Only jpg and videos is currently supported");
                                }
                                using (Stream jpegStreamIn = File.Open(file.FullImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    try
                                    {
                                        BitmapDecoder decoder = new JpegBitmapDecoder(jpegStreamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                                        var bitmapFrame = decoder.Frames[0];

                                        if (bitmapFrame != null)
                                        {
                                            var writer = bitmapFrame.CreateInPlaceBitmapMetadataWriter();
                                            if (writer != null)
                                            {
                                                writer.SetQuery("/app1/ifd/exif:{uint=306}", "2001:01:01 01:01:01");
                                                if (!writer.TrySave())
                                                {
                                                    if (bitmapFrame.Metadata != null)
                                                    {
                                                        var metaData = (BitmapMetadata)bitmapFrame.Metadata.Clone();
                                                        {
                                                            // Change DateTaken                                   2001:01:01 01:01:01
                                                            metaData.SetQuery("/app1/ifd/exif/subifd:{uint=36867}", timeString);

                                                            jpegStreamIn.Close();

                                                            var encoder = new JpegBitmapEncoder();
                                                            encoder.Frames.Add(BitmapFrame.Create(bitmapFrame, bitmapFrame.Thumbnail, metaData, bitmapFrame.ColorContexts));

                                                            using (Stream jpegStreamOut = File.Open(file.FullImagePath, FileMode.Create, FileAccess.Write))
                                                            {
                                                                encoder.Save(jpegStreamOut);
                                                                jpegStreamOut.Close();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }

                                }
                            
                        }
                        break;
                    case FileProperties.FileTypes.Video:
                        File.SetLastWriteTime(file.FullImagePath, chosenTime);
                        File.SetCreationTime(file.FullImagePath, chosenTime);
                        File.SetLastAccessTime(file.FullImagePath, chosenTime);
                        break;
                }

            }
            Done();
        }

        void Done()
        {
            listBoxFiles.DataContext = _conflictedFiles;
            _files.Clear();

            if (_conflictedFiles.Count <= 0) return;

            LabelError.Visibility = Visibility.Visible;
            var singPlural = _conflictedFiles.Count == 1 ? "file" : "files";
            LabelError.Text = String.Format("Could not change date for {0} {1}", _conflictedFiles.Count, singPlural);
        }

        private void ButtonBackClick(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new MainMenu());
        }
    }
}