using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MainMenu = Imgu.Menu.MainMenu;

namespace Imgu
{
    public partial class Edit : ISwitchable
    {
        List<FileProperties> _files = new List<FileProperties>();
        readonly List<FileProperties> _conflictedFiles = new List<FileProperties>(); 
        public Edit(IEnumerable<FileProperties> notSelected,IEnumerable<FileProperties> selectedItems)
        {
            InitializeComponent();
            UpdateTextBlocks(selectedItems);
            LabelError.Visibility=Visibility.Hidden;
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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.Switch(new MainMenu());
        }
        #endregion

        private void ButtonSetDateClick(object sender, RoutedEventArgs e)
        {
            if (CalendarChooseDate.SelectedDate == null) return;

            var chosenTime = (DateTime)CalendarChooseDate.SelectedDate;
            var timeString = chosenTime.ToString("yyyy:MM:dd ");
            foreach (var file in _files)
            {
                var time = file.DateTaken.ToLongTimeString();
                timeString += time;
                switch (file.FileType)
                {
                    case FileProperties.FileTypes.Image:
                        {
                            if (File.Exists(file.FullImagePath))
                            {
                                BitmapDecoder decoder;
                                using (Stream jpegStreamIn = File.Open(file.FullImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    decoder = new JpegBitmapDecoder(jpegStreamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                                }

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
                                                    //   
                                                    // Ändra DateTaken                                   2001:01:01 01:01:01
                                                    metaData.SetQuery("/app1/ifd/exif/subifd:{uint=36867}", timeString);

                                                    var encoder = new JpegBitmapEncoder();
                                                    encoder.Frames.Add(BitmapFrame.Create(bitmapFrame, bitmapFrame.Thumbnail, metaData, bitmapFrame.ColorContexts));
                                                    using (Stream jpegStreamOut = File.Open(file.FullImagePath, FileMode.Create, FileAccess.Write))
                                                    {
                                                        encoder.Save(jpegStreamOut);
                                                    }
          
                                                    //Kontrollera att datumet ändrades
                                                    using (Stream jpegStreamIn = File.Open(file.FullImagePath, FileMode.Open, FileAccess.ReadWrite))
                                                    {
                                                        decoder = new JpegBitmapDecoder(jpegStreamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                                                        var frame = decoder.Frames[0];
                                                        var bmd = (BitmapMetadata)frame.Metadata;
                                                        if (bmd != null)
                                                        {
                                                            var checkTime = (string)bmd.GetQuery("/app1/ifd/exif/subifd:{uint=36867}");
                                                            if(checkTime != timeString)
                                                            {
                                                                _conflictedFiles.Add(file);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
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

            LabelError.Visibility=Visibility.Visible;
            var singPlural= _conflictedFiles.Count == 1 ? "fil" : "filer";
            LabelError.Text = String.Format("Kunde inte ändra datum på {0} {1}", _conflictedFiles.Count, singPlural);
        }

        private void ButtonBackClick(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new MainMenu());
        }
    }
}

#region

//if (CalendarChooseDate.SelectedDate == null) return ;



//var timeToSet = (DateTime)CalendarChooseDate.SelectedDate;

//    foreach (var file in _files)
//{
//switch (
//file.FileType)
//{
//case
//FileProperties.FileTypes.Image:
//{
//    private var jpegPath = file.FullImagePath;
//    private var jpegDirectory = Path.GetDirectoryName(jpegPath);

//if (
//    private File.Exists 
//(
//    private jpegPath 
//))
//{
//    BitmapDecoder decoder = null;
//    using (Stream jpegStreamIn = File.Open(jpegPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
//    {
//        decoder = new JpegBitmapDecoder(jpegStreamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
//    }

//    var bitmapFrame = decoder.Frames[0];

//    if (bitmapFrame != null)
//    {
//        var writer = bitmapFrame.CreateInPlaceBitmapMetadataWriter();
//        if (writer != null)
//        {
//            writer.SetQuery("/app1/ifd/exif:{uint=306}", "2001:01:01 01:01:01");
//            if (!writer.TrySave())
//            {
//                if (bitmapFrame.Metadata != null)
//                {
//                    var metaData = (BitmapMetadata) bitmapFrame.Metadata.Clone();
//                    {
//                        //   
//                        //Ändra DateTaken 
//                        metaData.SetQuery("/app1/ifd/exif/subifd:{uint=36867}", "2008-05-01T07:34:42-5:00");
//                        //metaData.SetQuery("/app1/ifd/exif/subifd:{uint=306}", "2001:01:01 01:01:01");
//                        //metaData.SetQuery("/app1/ifd/exif/subifd:{uint=36868}", "2002:02:02 02:02:02");


//                        // get an encoder to create a new jpg file with the addit'l metadata.   
//                        var encoder = new JpegBitmapEncoder();
//                        encoder.Frames.Add(BitmapFrame.Create(bitmapFrame, bitmapFrame.Thumbnail, metaData,
//                                                              bitmapFrame.ColorContexts));
//                        if (jpegDirectory != null)
//                        {
//                            var jpegNewFileName = Path.Combine(jpegDirectory, "JpegTemp.jpg");
//                            using (
//                                Stream jpegStreamOut = File.Open(jpegNewFileName, FileMode.CreateNew,
//                                                                 FileAccess.ReadWrite))
//                            {
//                                encoder.Save(jpegStreamOut);
//                            }

//                            //Kontrollera att metadatat ändrades
//                            using (Stream jpegStreamIn = File.Open(jpegNewFileName, FileMode.Open, FileAccess.ReadWrite)
//                                )
//                            {
//                                decoder = new JpegBitmapDecoder(jpegStreamIn, BitmapCreateOptions.PreservePixelFormat,
//                                                                BitmapCacheOption.OnLoad);
//                                var frame = decoder.Frames[0];
//                                var bmd = (BitmapMetadata) frame.Metadata;
//                                var a1 = (string) bmd.GetQuery("/app1/ifd/exif/subifd:{uint=36867}");
//                                var asd = DateTime.Parse(a1);

//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
//}
//break
//;
//    case FileProperties.FileTypes.Video:
//    File.SetLastWriteTime(file.FullImagePath, timeToSet);
//    File.SetCreationTime(file.FullImagePath, timeToSet);
//    File.SetLastAccessTime(file.FullImagePath, timeToSet);
//    _fs =
//new

//FileStream(file.FullImagePath, FileMode.Open, FileAccess.ReadWrite);
//    _decoder = BitmapDecoder.Create(_fs, BitmapCreateOptions.None, BitmapCacheOption.Default);
//    _frame = _decoder.Frames
//[


//0];
//    _metadata = _frame.Metadata as BitmapMetadata;
//    break;
//}

//    }

#endregion
