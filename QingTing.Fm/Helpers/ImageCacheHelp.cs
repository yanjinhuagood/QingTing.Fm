using QingTing.Fm.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QingTing.Fm.Helpers
{
    //三级缓存        By:Starlight
    public class ImageCacheHelp
    {
        public static async Task<BitmapImage> GetImageByUrl(string url)
        {
            BitmapImage bi = new BitmapImage(new Uri(url)); //GetImageFormMemory(url);
            if (bi != null) { return bi; }
            bi = GetImageFromFile(url);
            
            return await GetImageFromInternet(url);
        }
        public static BitmapImage GetImageFromFile(string url)
        {
            string file = Settings.CachePath + "\\Images\\" + GetFileName(url.Replace("!200", ""));
            if (File.Exists(file))
                return new System.Drawing.Bitmap(file).ToBitmapImage();
            else return null;
        }
        public static async void AddImageToFile(string url, BitmapImage data)
        {
            await Task.Run(() =>
            {
                string filename = GetFileName(url.Replace("!200", ""));
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(data));

                using (var fileStream = new FileStream(Settings.CachePath + "\\Images\\" + filename, FileMode.Create))
                    encoder.Save(fileStream);
            });
        }
        public static async Task<BitmapImage> GetImageFromInternet(string url)
        {
            WebClient wc = new WebClient();
            BitmapImage bi = (await wc.DownloadDataTaskAsync(url)).ToBitmapImage();
            AddImageToFile(url, bi);
            return bi;
        }
        public static string GetFileName(string path)
        {
            string str = string.Empty;
            int pos1 = path.LastIndexOf('/');
            int pos2 = path.LastIndexOf('\\');
            int pos = Math.Max(pos1, pos2);
            if (pos < 0)
                str = path;
            else
                str = path.Substring(pos + 1);

            return str;
        }
    }
}
