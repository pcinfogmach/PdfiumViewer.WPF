using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfiumViewer.Core;

namespace PdfiumViewer.Helpers
{
    internal static class BitmapHelper
    {
        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// Note: This was the original implementation, but has performance penalty to save the bitmap to a memory stream
        /// </summary>
        /// <param name="bitmap">The Source Bitmap</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource0(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // not a mistake - see below
                bitmapImage.EndInit();
                bitmap.Dispose();
            }
            // Why BitmapCacheOption.OnLoad?
            // It seems counter intuitive, but this flag has two effects:
            // It enables caching if caching is possible, and it causes the load to happen at EndInit().
            // In our case caching is impossible, so all it does it cause the load to happen immediately.

            return bitmapImage;
        }

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// Note: Performance is better than ToBitmapSource0
        /// </summary>
        /// <param name="bitmap">The Source Bitmap</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource1(this System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) return null;

            using (var source = (System.Drawing.Bitmap)bitmap.Clone())
            {
                IntPtr hBitmap = source.GetHbitmap(); //obtain the Hbitmap
                var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                NativeMethods.DeleteObject(hBitmap); //release the HBitmap
                bs.Freeze();
                return bs;
            }
        }

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// Note: Performance is much better than the ToBitmapSource1 (at least 4 times faster which gives about 15...20% overall performance improvement)
        /// </summary>
        /// <param name="bitmap">The Source Bitmap</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource2(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        //// <summary>
        //// Convert an IImage to a WPF BitmapSource.The result can be used in the Set Property of Image.Source
        //// Note: Performance is similar to ToBitmapSource2 but it involves a native call
        //// </summary>
        //// <param name = "bitmap" > The Source Bitmap</param>
        //// <returns>The equivalent BitmapSource</returns>
        //public static BitmapSource ToBitmapSource3(System.Drawing.Bitmap bitmap)
        //{
        //    var bitmapData = bitmap.LockBits(
        //        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
        //        ImageLockMode.ReadOnly, bitmap.PixelFormat);

        //    WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Bgra32, null);
        //    writeableBitmap.Lock();
        //    CopyMemory(writeableBitmap.BackBuffer, bitmapData.Scan0, (uint)(writeableBitmap.BackBufferStride * bitmap.Height));
        //    writeableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, bitmap.Width, bitmap.Height));
        //    writeableBitmap.Unlock();

        //    bitmap.UnlockBits(bitmapData);

        //    return writeableBitmap;
        //}
    }
}
