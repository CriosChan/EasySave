namespace EasySave.Presentation.Ui;

/// <summary>
/// Describes a selectable option in a menu.
/// </summary>
public class Option
{
    public string Description { get; }
    public Action Selected { get; }
    /// <summary>
    /// Builds a menu option.
    /// </summary>
    /// <param name="description">Displayed text.</param>
    /// <param name="selected">Action to execute.</param>
    public Option(string description, Action selected)
    {
        Description = description;
        Selected = selected;
    }
}
