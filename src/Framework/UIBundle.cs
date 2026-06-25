// Loads the single UI AssetBundle shipped next to the dll, instantiates MainPanel,
// and parks it under DontDestroyOnLoad. No manifest — one bundle, one prefab, done.

using System.IO;
using UnityEngine;

namespace CustomStyleAdder.UI;

public static class UIBundle
{
    private const string BundleFile = "maimaidx.bundle";
    private const string PrefabName = "MainPanel";

    private static AssetBundle? _bundle;

    public static void Load()
    {
        if (_bundle != null) return;

        var dir  = Path.GetDirectoryName(typeof(UIBundle).Assembly.Location)!;
        var path = Path.Combine(dir, BundleFile);

        _bundle = AssetBundle.LoadFromFile(path);
        if (_bundle == null) { LogHelper.Warn($"[UI] bundle not in the dir fuck: {path}"); return; }

        var prefab = _bundle.LoadAsset<GameObject>(PrefabName);
        if (prefab == null) { LogHelper.Warn($"[UI] prefab not in bundle fuck: {PrefabName}"); return; }

        var go = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(go);
    }
}
