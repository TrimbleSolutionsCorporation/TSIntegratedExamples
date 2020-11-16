namespace JoistArea.Tools
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// http://stackoverflow.com/questions/1127647/convert-system-drawing-icon-to-system-media-imagesource
    /// </summary>
    public static class IconUtilities
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Gets image source from icon
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource ToImageSource(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,IntPtr.Zero,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap)) throw new Win32Exception();
            return wpfBitmap;
        }

        /// <summary>
        /// Gets image source for corresponding temp color
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource GetImageSource(this Image icon)
        {
            var converter = new ImageSourceConverter();
            return (ImageSource)converter.ConvertFrom(icon);
        }

        /// <summary>
        /// Gets image source from Icon
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        private static ImageSource GetImageSource(this Icon icon)
        {
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}