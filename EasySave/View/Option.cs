namespace EasySave.View;

public class Option
{
    public string Description { get; }
    public Action Selected { get; }
    public Option(string description, Action selected)
    {
        Description = description;
        Selected = selected;
    }
}