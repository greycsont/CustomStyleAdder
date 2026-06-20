using System.ComponentModel;
using HarmonyLib;

using static CustomStyleAdder.StyleSender;

namespace CustomStyleAdder;

[Description("Patch from plonk, thanks :>")]
[HarmonyPatch]
public static class NoLeaderboard
{
    [HarmonyPatch(typeof(LeaderboardController), nameof(LeaderboardController.SubmitCyberGrindScore)), HarmonyPrefix]
    public static bool OnSubmitCyberGrindScore() => GetLockStatus();

    [HarmonyPatch(typeof(LeaderboardController), nameof(LeaderboardController.SubmitLevelScore)), HarmonyPrefix]
    public static bool OnSubmitLevelScore() => GetLockStatus();

    public static bool GetLockStatus()
    {
        return !leaderboardLock;
    }
}