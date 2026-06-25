namespace CustomStyleAdder;

/// <summary>
/// What the style looks like in StyleHUD
/// count, prefix and postfix just a placeholder
/// </summary>
public record struct CustomStyle
{
    public string styleName;
    public int stylePoints;
    
    /// <summary>
    /// if it's great or equal to 0
    /// it will add count as postfix
    /// </summary>
    public int count = -1; 
    
    public string prefix;
    public string postfix;

    public CustomStyle(string styleName, int stylePoints)
    {
        this.styleName = styleName;
        this.stylePoints = stylePoints;
    }
}
