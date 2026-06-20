using System.Collections.Generic;

namespace CustomStyleAdder;

public class Profile
{
    public string name { get; internal set; }
    public List<StyleRule> rules = new();

    public void AddRule(StyleRule rule)
    {
        if (rules.Contains(rule)) return;
        rules.Add(rule);
    }

    public void RemoveRule(StyleRule rule)
    {
        if (rules.Contains(rule)) rules.Remove(rule);
    }
}
