using System.Drawing;
using System.IO;

namespace FileFlow.Services
{
    public interface IIconExtractorService
    {
        Bitmap GetIcon(string filePath);
    }
    public class IconExtractorService : IIconExtractorService
    {
        public Bitmap GetIcon(string filePath)
        {
            if (File.Exists(filePath) == false) return null;
            var icon = Icon.ExtractAssociatedIcon(filePath);
            return icon.ToBitmap();
        }
    }
}
