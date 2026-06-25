using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using CustomStyleAdder.UI;
using Newtonsoft.Json;

namespace CustomStyleAdder;

public enum CsaSetting
{
    StylePointLock,
}

/// <summary>
/// Enum-keyed config manager (<c>ConfigManager&lt;TLookup&gt;</c> slop from osu-framework).
/// Editor-authored controls store a <see cref="CsaSetting"/> and call <see cref="GetBindable{T}"/>
/// at runtime to resolve it to the real bindable — so this doubles as their lookup registry.
/// </summary>
public class CsaConfig
{
    public static CsaConfig Instance { get; } = new();

    /// <summary>Forces the singleton to construct early (defaults + load).</summary>
    public static void Init() => _ = Instance;

    private static readonly string _path =
        Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.json");

    private readonly Dictionary<CsaSetting, IBindable> _store = new();
    private bool _loading;

    private CsaConfig()
    {
        // osu order: defaults first, then overlay from disk.
        InitialiseDefaults();
        Load();
    }

    /// <summary>Registers every setting with its default. Add new settings here.</summary>
    private void InitialiseDefaults()
    {
        SetDefault(CsaSetting.StylePointLock, false);
    }

    /// <summary>Creates a setting's backing bindable and wires auto-save to its changes.</summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The default value (also fixes the bindable's type).</param>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>The newly created authority bindable.</returns>
    private Bindable<T> SetDefault<T>(CsaSetting key, T value)
    {
        var bindable = new Bindable<T>(value);
        _store[key] = bindable;
        bindable.ValueChanged += _ => QueueSave();
        return bindable;
    }

    /// <summary>The authority bindable. Use <see cref="GetBindable{T}"/> for UI instead.</summary>
    /// <param name="key">The setting key.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public Bindable<T> GetOriginal<T>(CsaSetting key) => (Bindable<T>)_store[key];

    /// <summary>A weakly-bound copy for consumers aka UI slop; caller unbinds on destroy (<see cref="UI.Drawable"/>).</summary>
    /// <param name="key">The setting key.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public Bindable<T> GetBindable<T>(CsaSetting key) => GetOriginal<T>(key).GetBoundCopy();

    /// <summary>Reads a setting's current value.</summary>
    /// <param name="key">The setting key.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public T Get<T>(CsaSetting key) => GetOriginal<T>(key).Value;

    /// <summary>Writes a setting's value, notifying all subscribers.</summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The new value.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public void Set<T>(CsaSetting key, T value) => GetOriginal<T>(key).Value = value;

    /// <summary>
    /// Serializes all settings to the backing JSON file.
    /// </summary>
    public void Save()
    {
        try
        {
            var data = new Dictionary<string, object?>();
            foreach (var kv in _store)
                data[kv.Key.ToString()] = kv.Value.GetValue();

            File.WriteAllText(_path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        catch (Exception e)
        {
            LogHelper.Warn($"[Config] save failed: {e.Message}");
        }
    }

    /// <summary>
    /// Overlays disk values onto defaults; unknown/bad keys are skipped and logged.
    /// </summary>
    public void Load()
    {
        if (!File.Exists(_path)) return;

        _loading = true;
        try
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(_path));
            if (data == null) return;

            foreach (var kv in data)
            {
                if (!Enum.TryParse<CsaSetting>(kv.Key, out var key)) continue;
                if (!_store.TryGetValue(key, out var bindable)) continue;

                try { bindable.Parse(kv.Value, CultureInfo.InvariantCulture); }
                catch (Exception e) { LogHelper.Warn($"[Config] parse '{key}' failed: {e.Message}"); }
            }
        }
        catch (Exception e)
        {
            LogHelper.Warn($"[Config] load failed: {e.Message}");
        }
        finally { _loading = false; }
    }

    private int _saveGen;

    /// <summary>
    /// Debounced save (osu's <c>QueueBackgroundSave</c>); coalesces bursts like slider drags.
    /// </summary>
    private void QueueSave()
    {
        if (_loading) return;
        int gen = ++_saveGen;
        Task.Delay(200).ContinueWith(_ => { if (gen == _saveGen) Save(); });
    }
}
