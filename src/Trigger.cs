using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CustomStyleAdder;

/// <summary>
/// The definition of "Triggers"
/// Harmony Variables for Harmony Patch
/// A Condition will add later
/// </summary>
public record struct StyleTrigger
{
    public string className;
    public string methodName;
    public string[]? argTypes;

    // 之后在这里加条件求值var/op/value 之类
    // 可能是Expression还是REPL什么的
}

/// <summary>
/// One rule = id + when (trigger) + what to show (style)
/// id lives here: dedup / Unbind / future UI selection all key off it
/// CustomStyle is just reusable display data, no identity
/// </summary>
public record struct StyleRule
{
    public string id;
    public CustomStyle style;
    public StyleTrigger trigger;

    public StyleRule(string id, CustomStyle style, StyleTrigger trigger)
    {
        this.id = id;
        this.style = style;
        this.trigger = trigger;
    }

    // equality by id, so one profile won't keep two rules sharing the same id
    // reason: fuck you rider
    public bool Equals(StyleRule other) => id == other.id;
    public override int GetHashCode() => id?.GetHashCode() ?? 0;
}

/// <summary>
/// The execution layer: turns a StyleTrigger into an actual Harmony patch
/// Each target method is patched once, the shared postfix uses __originalMethod to look its rules back up
/// </summary>
public static class TriggerEngine
{
    private static Harmony? _harmony;
    private static readonly Dictionary<MethodBase, List<StyleRule>> _rules = new();
    private static readonly HashSet<MethodBase> _patched = new();

    public static void Init(Harmony harmony) => _harmony = harmony;

    /// <summary>
    /// Replace all rules with new rules
    /// (Used by Switching Profiles)
    /// </summary>
    /// <param name="rules"></param>
    public static void RebindAll(IEnumerable<StyleRule> rules)
    {
        _rules.Clear();
        foreach (var rule in rules)
            Bind(rule);
    }
    
    /// <summary>
    /// Bind a rule to a method
    /// (One method only patch once cuz why you f need to patch twice waste process power)
    /// </summary>
    /// <param name="rule"></param>
    public static void Bind(StyleRule rule)
    {
        var method = Resolve(rule.trigger);
        if (method == null) return; // Resolve already logged the why

        if (!_rules.TryGetValue(method, out var list))
            _rules[method] = list = new List<StyleRule>();
        list.Add(rule);

        // one patch per method, later rules just reuse the same postfix
        if (_patched.Add(method))
        {
            var postfix = new HarmonyMethod(typeof(TriggerEngine), nameof(Bridge));
            _harmony!.Patch(method, postfix: postfix);
        }
    }

    /// <summary>
    /// Remove a rule in the rules dictionary
    /// (The patch itself will remain cuz the reason ahead)
    /// </summary>
    /// <param name="rule"></param>
    public static void Unbind(StyleRule rule)
    {
        var method = Resolve(rule.trigger);
        if (method == null) return;

        if (_rules.TryGetValue(method, out var list))
        {
            list.Remove(rule);
            if (list.Count == 0) _rules.Remove(method);
        }
    }
    
    public static void UnpatchAll()
    {
        _harmony?.UnpatchSelf();
        _rules.Clear();
        _patched.Clear();
    }

    private static MethodBase? Resolve(StyleTrigger t)
    {
        if (string.IsNullOrEmpty(t.className) || string.IsNullOrEmpty(t.methodName))
        {
            LogHelper.Warn("[Trigger] className/methodName 为空，跳过人生了");
            return null;
        }

        var type = AccessTools.TypeByName(t.className);
        if (type == null)
        { 
            LogHelper.Warn($"[Trigger] 找不到类型: {t.className}");
            return null;
        }

        MethodBase? method;
        if (t.argTypes is { Length: > 0 })
        {
            var paramTypes = t.argTypes.Select(AccessTools.TypeByName).ToArray();
            if (paramTypes.Any(p => p == null))
            {
                LogHelper.Warn($"[Trigger] {t.className}.{t.methodName} 的 argTypes 有无法解析的类型");
                return null;
            }
            method = AccessTools.Method(type, t.methodName, paramTypes!);
        }
        else
        {
            method = AccessTools.Method(type, t.methodName);
        }

        if (method == null)
            LogHelper.Warn($"[Trigger] 找不到方法: {t.className}.{t.methodName}（重载请填 argTypes）");

        return method;
    }

    // Shared bridge for every patched method. __originalMethod is injected by Harmony to look the rules back up.
    // __instance / __args will be needed once condition eval lands
    private static void Bridge(MethodBase __originalMethod)
    {
        if (!_rules.TryGetValue(__originalMethod, out var rules)) return;
        foreach (var rule in rules)
        {
            try
            {
                StyleSender.TrySendStyleToStyleHUD(rule.style);
            }
            catch (Exception e)
            {
                LogHelper.Warn($"[Trigger] 规8fq: {rule.id}");
            }
        }
    }
}
