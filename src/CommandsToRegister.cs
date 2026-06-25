using GameConsole;
using GameConsole.CommandTree;
using plog;
using System;
using System.Linq;
using Console = GameConsole.Console;

namespace CustomStyleAdder;

public sealed class CommandsToRegister(Console con) : CommandRoot(con), IConsoleLogger
{
    public override string Name => "csa";
    public override string Description => "CustomStyleAdder";

    protected override Branch BuildTree(Console con)
        => Branch(Name,
                      GetProfileBranches(),
                      GetRuleBranches(),
                      GetSettingBranches(),
                      Leaf("help", PrintHelp));
    
    private Branch GetProfileBranches()
        => Branch("-p",
            Leaf("ls", () =>
            {
                if (ProfileManager.profiles.Count == 0) { Log.Info("No profiles."); return; }
                foreach (var name in ProfileManager.profiles.Keys)
                {
                    var mark = ProfileManager.Current?.name == name ? " *" : "";
                    Log.Info($"  {name}{mark}");
                }
            }),

            Leaf("whoami", () =>
                Log.Info($"Current profile: {ProfileManager.Current?.name ?? "<none>"}")),

            Leaf<string>("touch", name =>
            {
                var p = ProfileManager.Create(name);
                Log.Info($"Created profile: {p.name}");
            }),

            Leaf<string>("cd", name =>
            {
                ProfileManager.Switch(name);
                Log.Info($"Current profile: {ProfileManager.Current?.name ?? "<none>"}");
            }),

            Leaf<string>("cp", name =>
            {
                var p = ProfileManager.Duplicate(name);
                Log.Info($"Duplicated into: {p.name}");
            }),

            Leaf<string>("rm", name =>
            {
                ProfileManager.Delete(name);
                Log.Info($"Deleted profile: {name}");
            }),
            
            Leaf<string[]>("rename", args =>
            {
                if (args.Length < 2) { Log.Warning("usage: csa p rename <old> <new>"); return; }
                ProfileManager.Rename(args[0], args[1]);
                Log.Info($"Renamed {args[0]} -> {args[1]}");
            })
        );
    
    private Branch GetRuleBranches()
        => Branch("-r",
            Leaf("ls", () =>
            {
                if (!TryGetCurrent(out var p)) return;
                if (p.rules.Count == 0) { Log.Info("No rules in current profile."); return; }
                foreach (var r in p.rules)
                    Log.Info($"  {r.id}: {r.style.styleName} (+{r.style.stylePoints})" +
                             $" <- {r.trigger.className}.{r.trigger.methodName}");
            }),
            
            Branch("add",
                new Leaf((Action<string, string, int, string, string>)
                    ((id, styleName, points, className, methodName) =>
                        DoAdd(id, styleName, points, className, methodName, null))),
                new Leaf((Action<string, string, int, string, string, string>)
                    ((id, styleName, points, className, methodName, a1) =>
                        DoAdd(id, styleName, points, className, methodName, new[] { a1 }))),
                new Leaf((Action<string, string, int, string, string, string, string>)
                    ((id, styleName, points, className, methodName, a1, a2) =>
                        DoAdd(id, styleName, points, className, methodName, new[] { a1, a2 })))
            ),

            Leaf<string>("rm", id =>
            {
                if (!TryGetCurrent(out var p)) return;
                var rule = p.rules.FirstOrDefault(r => r.id == id);
                if (rule.id != id) { Log.Warning($"No rule with id: {id}"); return; }
                p.RemoveRule(rule);
                RebindIfCurrent(p);
                Log.Info($"Removed rule: {id}");
            }),

            Leaf("clear", () =>
            {
                if (!TryGetCurrent(out var p)) return;
                p.ClearRules();
                RebindIfCurrent(p);
                Log.Info("Cleared all rules in current profile.");
            })
        );

    private Branch GetSettingBranches()
        => Branch("-s",
            Leaf("lock", () =>
            {
                CsaConfig.Instance.Set(CsaSetting.StylePointLock,
                    !CsaConfig.Instance.Get<bool>(CsaSetting.StylePointLock));
            }),
            Leaf("status", () => 
                LogHelper.Info($"StylePointLock = {CsaConfig.Instance.Get<bool>(CsaSetting.StylePointLock)}"))
        );
    
    private void DoAdd(string id, string styleName, int points,
                       string className, string methodName, string[]? argTypes)
    {
        if (!TryGetCurrent(out var p)) return;

        var rule = StyleTemplates.OnMethod(id, styleName, points, className, methodName, argTypes);
        p.AddRule(rule);
        RebindIfCurrent(p);
        Log.Info($"Added rule: {rule.id}");
    }

    private bool TryGetCurrent(out Profile p)
    {
        p = ProfileManager.Current;
        if (p == null) { Log.Warning("No current profile. Use: csa -p touch <name>"); return false; }
        return true;
    }

    // live-apply: rebuild patches only when we touched the active profile
    private static void RebindIfCurrent(Profile p)
    {
        if (ProfileManager.Current == p)
            TriggerEngine.RebindAll(p.rules);
    }

    private void PrintHelp()
    {
        Log.Info("=== CustomStyleAdder Command Reference ===");
        Log.Info("");
        Log.Info("[Profile] csa -p");
        Log.Info("  ls                                - list all profiles (* = current)");
        Log.Info("  whoami                            - show current profile");
        Log.Info("  touch  <name>                     - create a new profile");
        Log.Info("  cd     <name>                     - switch current profile");
        Log.Info("  cp     <name>                     - duplicate a profile");
        Log.Info("  rm     <name>                     - delete a profile");
        Log.Info("  rename <old> <new>                - rename a profile");
        Log.Info("");
        Log.Info("[Rule] csa -r   (operates on current profile)");
        Log.Info("  ls                                - list rules in current profile");
        Log.Info("  add <id> <styleName> <points> <className> <methodName> [argType] [argType]");
        Log.Info("  rm  <id>                          - remove a rule by id");
        Log.Info("  clear                             - remove all rules");
        Log.Info("");
        Log.Info("[Setting] csa -s");
        Log.Info("  lock                              - change the value of styleLock");
        Log.Info("  status                            - list the status of setting");
        
    }

    public Logger Log { get; } = new("csa");
}
