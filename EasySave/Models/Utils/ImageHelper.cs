using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace EasySave.Models.Utils;

/// <summary>
/// Provides utilities for image operations, specifically loading images from resources.
/// </summary>
public class ImageHelper
{
    /// <summary>
    /// Loads a bitmap image from a specified resource URI.
    /// </summary>
    /// <param name="resourceUri">The URI of the resource to load the image from.</param>
    /// <returns>A Bitmap object if successful; otherwise, null.</returns>
    public static Bitmap? LoadFromResource(Uri resourceUri)
    {
        try
        {
            // Attempt to load the bitmap image from the provided resource URI.
            return new Bitmap(AssetLoader.Open(resourceUri));
        }
        catch (Exception e)
        {
            // Log or handle the exception as needed. 
            // Currently, it simply returns null if any exception occurs.
            return null;
        }
    }
}