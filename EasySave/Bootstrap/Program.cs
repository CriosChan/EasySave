namespace EasySave.Bootstrap;

/// <summary>
///     Main program entry point.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Starts the application and returns the exit code.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    private static void Main(string[] args)
    {
        IApplication app = new EasySaveApplication();
        Environment.ExitCode = app.Run(args);
    }
}