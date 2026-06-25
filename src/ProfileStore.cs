using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace CustomStyleAdder;

/// <summary>
/// Persists each profile to its own JSON file under a profiles directory.
/// </summary>
public static class ProfileStore
{
    private static readonly string _dir =
        Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.profiles");

    /// <summary>
    /// We need a cool ass name without dealing with illegal character in path.
    /// That's why this exsit
    /// </summary>
    /// <param name="name">Original name of the profile</param>
    /// <returns></returns>
    private static string PathFor(string name) => Path.Combine(_dir, Uri.EscapeDataString(name) + ".json");

    /// <summary>
    /// Writes a single profile to its own file.
    /// </summary>
    /// <param name="p">Profiles</param>
    public static void Save(Profile p)
    {
        try
        {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(PathFor(p.name), JsonConvert.SerializeObject(p, Formatting.Indented));
        }
        catch (Exception e)
        {
            LogHelper.Warn($"[Profiles] save '{p.name}' failed: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes a single profile's file.
    /// </summary>
    /// <param name="name">Original name of the profile</param>
    public static void Delete(string name)
    {
        try
        {
            var path = PathFor(name);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception e)
        {
            LogHelper.Warn($"[Profiles] delete '{name}' failed: {e.Message}");
        }
    }

    /// <summary>
    /// Reads every profile from the profiles' directory.
    /// </summary>
    public static IEnumerable<Profile> LoadAll()
    {
        if (!Directory.Exists(_dir)) yield break;

        foreach (var file in Directory.GetFiles(_dir, "*.json"))
        {
            Profile? p = null;
            try
            {
                p = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(file));
            }
            catch (Exception e)
            {
                LogHelper.Warn($"[Profiles] load '{Path.GetFileName(file)}' failed: {e.Message}");
            }

            if (p != null) yield return p;
        }
    }
}
