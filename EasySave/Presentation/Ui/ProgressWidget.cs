using EasySave.Presentation.Ui.Console;

namespace EasySave.Presentation.Ui;

/// <summary>
/// Represents a console-based widget for displaying progress information.
/// </summary>
public class ProgressWidget
{
    private int width;
    private string progressChar;
    private string emptyChar;
    private IConsole _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressWidget"/> class.
    /// </summary>
    /// <param name="console">An instance of <see cref="IConsole"/> used for displaying output to the console.</param>
    /// <param name="width">The width of the progress bar.</param>
    /// <param name="progressChar">The character used to represent the filled part of the progress bar.</param>
    /// <param name="emptyChar">The character used to represent the unfilled part of the progress bar.</param>
    public ProgressWidget(IConsole console, int width = 50, char progressChar = '#', char emptyChar = '-')
    {
        this.width = width;
        this.progressChar = progressChar.ToString();
        this.emptyChar = emptyChar.ToString();
        _console = console;
    }

    /// <summary>
    /// Updates the progress bar based on the provided percentage.
    /// </summary>
    /// <param name="percentage">A double value representing the completion percentage (0 to 100).</param>
    /// <remarks>
    /// This method ensures that the percentage is clamped within the range of 0 to 100.
    /// It calculates the filled and empty widths of the progress bar and displays
    /// the updated bar in the console.
    /// </remarks>
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
}