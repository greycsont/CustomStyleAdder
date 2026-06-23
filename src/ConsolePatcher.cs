using GameConsole;
using HarmonyLib;

namespace CustomStyleAdder;

[HarmonyPatch(typeof(Console))]
public class ConsolePatcher
{
    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    public static void AddConsoleCommands(Console __instance)
        => __instance.RegisterCommand(new CommandsToRegister(__instance));
}