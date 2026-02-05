namespace EasySave.Presentation.Ui;

/// <summary>
/// Decrit une option selectable dans un menu.
/// </summary>
public class Option
{
    public string Description { get; }
    public Action Selected { get; }
    /// <summary>
    /// Construit une option de menu.
    /// </summary>
    /// <param name="description">Texte affiche.</param>
    /// <param name="selected">Action a executer.</param>
    public Option(string description, Action selected)
    {
        Description = description;
        Selected = selected;
    }
}
