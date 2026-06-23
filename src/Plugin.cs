using System;
using BepInEx;
using BepInEx.Logging;
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

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        LogHelper.Init(Logger);
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        gameObject.hideFlags = HideFlags.DontSaveInEditor;

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        TriggerEngine.Init(harmony);
        ProfileManager.Init();
        
        SceneManager.sceneLoaded += HandleSceneChanging;
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