namespace CustomStyleAdder;

/// <summary>
/// What the style looks like in StyleHUD
/// count, prefix and postfix just a placeholder
/// </summary>
public record struct CustomStyle
{
    public string styleName;
    public int stylePoints;
    public int count;
    public string prefix;
    public string postfix;

    public CustomStyle(string styleName, int stylePoints)
    {
        this.styleName = styleName;
        this.stylePoints = stylePoints;
    }
}
