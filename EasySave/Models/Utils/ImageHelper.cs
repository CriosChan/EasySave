using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace EasySave.Models.Utils;

public class ImageHelper
{
        public static Bitmap? LoadFromResource(Uri resourceUri)
        {
            try
            {
                return new Bitmap(AssetLoader.Open(resourceUri));
            }
            catch (Exception e)
            {
                return null;
            }
        }
}