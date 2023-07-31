using System.Drawing;
using System.Drawing.Imaging;

public static class ImageExtensions
{
    public static Avalonia.Media.Imaging.Bitmap ConvertToAvaloniaBitmap(this Image bitmap)
    {
        if (bitmap == null) return null;

        Bitmap bitmapTmp = new Bitmap(bitmap);

        BitmapData bitmapdata = bitmapTmp.LockBits(new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        Avalonia.Media.Imaging.Bitmap bitmap1 = new(
            Avalonia.Platform.PixelFormat.Bgra8888, 
            Avalonia.Platform.AlphaFormat.Unpremul,
            bitmapdata.Scan0,
            new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
            new Avalonia.Vector(96, 96),
            bitmapdata.Stride);

        bitmapTmp.UnlockBits(bitmapdata);
        bitmapTmp.Dispose();

        return bitmap1;
    }
}