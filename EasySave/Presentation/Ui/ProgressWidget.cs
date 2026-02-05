using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

public class ProgressWidget
{
    private int width;
    private string progressChar;
    private string emptyChar;
    private IConsole _console;

    public ProgressWidget(IConsole console, int width = 50, char progressChar = '#', char emptyChar = '-')
    {
        this.width = width;
        this.progressChar = progressChar.ToString();
        this.emptyChar = emptyChar.ToString();
        _console = console;
    }

    public void UpdateProgress(double percentage)
    {
        // Ensure the percentage is between 0 and 100
        percentage = Math.Max(0, Math.Min(percentage, 100));

        // Calculate the filled width of the progress bar
        int filledWidth = (int)(percentage / 100 * width);
        int emptyWidth = width - filledWidth;

        // Construct the progress bar string
        string progressBar = $"{new string(progressChar[0], filledWidth)}{new string(emptyChar[0], emptyWidth)}";

        // Move the cursor back to the previous line and overwrite
        _console.Write($"\r[{progressBar}] ({percentage:F2} %)");
    }


    public void Complete()
    {
        _console.WriteLine(string.Empty);
    }
}