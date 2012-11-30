using System;

namespace Imgu
{
    public class FileProperties
    {
        public DateTime DateTaken { get; set; }
        public string FullImagePath { get; set; }
        public string FolderPath { get; set; }
        public string FileName { get; set; }

        FileTypes _fileType = FileTypes.Image;
        public FileTypes FileType
        {
            get { return _fileType; }
            set { _fileType = value; }
        }

        public enum FileTypes
        {
            Image,
            Video
        }
        
    }
}
