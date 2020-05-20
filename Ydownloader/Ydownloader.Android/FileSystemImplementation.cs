using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Ydownloader.Droid;

[assembly: Dependency(typeof(FileSystemImplementation))]
namespace Ydownloader.Droid
{
    public class FileSystemImplementation : IFileSystem
    {
        public string GetExternalStorage()
        {
            Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            Java.IO.File dir = new Java.IO.File(sdCard.AbsolutePath + (Java.IO.File.Separator + "YDownload"));
            dir.Mkdirs();

            return dir.Path;
        }
    }
    
}