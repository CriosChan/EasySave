namespace EasySave.Utils;

public static class PathTools
{
    public static string ToFullUncLikePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        // Keep UNC paths as-is.
        if (OperatingSystem.IsWindows() && path.StartsWith("\\\\"))
            return path;

        string full = Path.GetFullPath(path);
        if (OperatingSystem.IsWindows())
            full = full.Replace('/', '\\');

        return full;
    }

    public static string GetRelativePath(string basePath, string fullPath)
    {
        try
        {
            return Path.GetRelativePath(basePath, fullPath);
        }
        catch
        {
            // Fallback: if relative path fails, return file name.
            return Path.GetFileName(fullPath);
        }
    }
}
