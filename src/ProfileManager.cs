using System.Collections.Generic;
using System.Linq;
using CustomStyleAdder.UI;

namespace CustomStyleAdder;

public static class ProfileManager
{
    // key = profile.name
    public static readonly Dictionary<string, Profile> profiles = new();
    public static Profile? Current { get; private set; }

    // bumped whenever the current profile or its rules change; UI binds to it to rebuild
    public static readonly Bindable<int> Revision = new();
    internal static void NotifyChanged() => Revision.Value++;

    private static void SetCurrent(Profile? p)
    {
        Current = p;
        TriggerEngine.RebindAll(p?.rules ?? Enumerable.Empty<StyleRule>());
        NotifyChanged();
    }

    public static void Init()
    {
        foreach (var p in ProfileStore.LoadAll())
            profiles[p.name] = p;

        if (profiles.Count > 0)
            Switch(profiles.Keys.First());
        else
            Create("Default", switchTo: true);
    }

    /// <summary>
    /// Switch Profiles according to the name
    /// </summary>
    /// <param name="name"></param>
    public static void Switch(string name)
    {
        if (profiles.TryGetValue(name, out var p))
            SetCurrent(p);
        else
            LogHelper.Warn($"[Profile] 找不到 profile: {name}");
    }
    
    /// <summary>
    /// Create a new Profile, if current name is not unique it will add number as postfix
    /// Will switch profile it's the only profile or wants to switch to
    /// </summary>
    /// <param name="name"></param>
    /// <param name="switchTo"></param>
    /// <returns></returns>
    public static Profile Create(string name = "New Profile", bool switchTo = false)
    {
        name = UniqueName(name);
        var p = new Profile { name = name };
        profiles[name] = p;
        if (switchTo || profiles.Count == 1) SetCurrent(p);
        ProfileStore.Save(p);
        return p;
    }

    /// <summary>
    /// Duplicate current Profile with unique name
    /// </summary>
    /// <param name="sourceName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public static Profile Duplicate(string sourceName, string? newName = null)
    {
        var src = profiles[sourceName];
        var copy = new Profile
        {
            name  = UniqueName(newName ?? src.name + " Copy"),
            rules = new List<StyleRule>(src.rules)
        };
        profiles[copy.name] = copy;
        ProfileStore.Save(copy);
        return copy;
    }
    
    /// <summary>
    /// Delete a Profile
    /// </summary>
    /// <param name="name"></param>
    public static void Delete(string name)
    {
        if (!profiles.Remove(name)) return;
        if (Current?.name == name)
            SetCurrent(profiles.Values.FirstOrDefault());
        ProfileStore.Delete(name);
    }
    
    /// <summary>
    /// Rename a Profile
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    public static void Rename(string oldName, string newName)
    {
        if (!profiles.TryGetValue(oldName, out var p)) return;
        newName = UniqueName(newName);
        profiles.Remove(oldName);
        p.name = newName;
        profiles[newName] = p; // key = p.name as said in the profiles in the f top
        ProfileStore.Delete(oldName);
        ProfileStore.Save(p);
    }

    private static string UniqueName(string baseName)
    {
        if (!profiles.ContainsKey(baseName)) return baseName;
        for (int i = 2; ; i++)
        {
            var candidate = $"{baseName} ({i})";
            if (!profiles.ContainsKey(candidate)) return candidate;
        }
    }
}
