using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using Imgu.Properties;

namespace Imgu
{
    class SetFolderIcon
    {
        public void setFolderIcon(string targetPath, int icon)
        {
            /* Remove any existing desktop.ini */
            if (File.Exists(targetPath + @"\desktop.ini")) File.Delete(targetPath + @"\desktop.ini");

            string iconPath = @"C:\Windows\system32\SHELL32.dll," + icon;

            /* Write the desktop.ini */
            StreamWriter sw = File.CreateText(targetPath + @"\desktop.ini");

            //Years
            if (icon != 0)
            {
                sw.WriteLine("[.ShellClassInfo]");
                sw.WriteLine("IconResource=" + iconPath);
                sw.WriteLine("IconIndex=0");
            }

            //Months
            if (icon == 0)
            {
                sw.WriteLine("[.ShellClassInfo]");
                sw.WriteLine("IconResource=" + Settings.Default.MonthIcon);
                sw.WriteLine("IconIndex=0");
            }

            //Days
            else
            {
                sw.WriteLine("[ViewState]");
                sw.WriteLine("FolderType=Pictures");
            }

            /* Set the desktop.ini to be hidden */
            File.SetAttributes(targetPath + @"\desktop.ini", File.GetAttributes(targetPath + @"\desktop.ini") | FileAttributes.Hidden);

            /* Set the path to system */
            File.SetAttributes(targetPath, File.GetAttributes(targetPath) | FileAttributes.System);
            sw.Close();
            sw.Dispose();
        }
    }
}
