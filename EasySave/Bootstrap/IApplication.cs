namespace EasySave.Bootstrap;

/// <summary>
/// Small abstraction to keep Program focused on the entry point.
/// </summary>
internal interface IApplication
{
    /// <summary>
    /// Executes the application with the provided arguments.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Process exit code.</returns>
    int Run(string[] args);
}
