using EasySave.Application;

namespace EasySave;

internal static class Program
{
    private static void Main(string[] args)
    {
        IApplication app = new EasySaveApplication();
        Environment.ExitCode = app.Run(args);
    }
}
