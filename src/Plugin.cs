using System;
using BepInEx;
using BepInEx.Logging;
using CustomStyleAdder.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomStyleAdder;

//
// A Profile-Based Custom Style Adder
// 
// 
//
//
//
//

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; private set; } = null!;

    // move to CsaConfig once a KeyCode bindable exists in the fucking future
    private const KeyCode ToggleKey = KeyCode.F7;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        LogHelper.Init(Logger);
        CsaConfig.Init();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        gameObject.hideFlags = HideFlags.DontSaveInEditor;

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        TriggerEngine.Init(harmony);
        ProfileManager.Init();
        UIBundle.Load();

        SceneManager.sceneLoaded += HandleSceneChanging;
    }

    private void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
            MainPanel.Instance?.Toggle();
    }

    public void HandleSceneChanging(Scene scene, LoadSceneMode mode)
    {
        StyleSender.RefreshState();
    }
    
    /// <summary>
    /// Perfectionism lmao
    /// </summary>
    public void OnDestroy()
    {
        TriggerEngine.UnpatchAll();
    }
}