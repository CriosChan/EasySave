namespace EasySave.Application;

/// <summary>
/// Small abstraction to keep Program focused on the entry point.
/// </summary>
internal interface IApplication
{
    int Run(string[] args);
}
