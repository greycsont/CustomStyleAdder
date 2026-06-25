using System.Collections.Generic;

namespace CustomStyleAdder;

public class Profile
{
    public string name { get; set; }
    public List<StyleRule> rules = new();

    public void AddRule(StyleRule rule)
    {
        if (rules.Contains(rule)) return;
        rules.Add(rule);
        ProfileStore.Save(this);
        ProfileManager.NotifyChanged();
    }

    public void RemoveRule(StyleRule rule)
    {
        if (rules.Remove(rule))
        {
            ProfileStore.Save(this);
            ProfileManager.NotifyChanged();
        }
    }

    public void ClearRules()
    {
        rules.Clear();
        ProfileStore.Save(this);
        ProfileManager.NotifyChanged();
    }
}
