using System.Runtime.CompilerServices;
using CustomStyleAdder.UI;

namespace CustomStyleAdder;

public static class Setting
{
    /// <summary>
    /// With this enabled
    /// All custom style gives 0 points cuz cheating reason
    /// </summary>
    public static Bindable<bool> stylePointLock = new(false);

    public static void Init()
    {
        stylePointLock.BindValueChanged(e =>
                LogHelper.Info($"[Setting] stylePointLock: {e.OldValue} -> {e.NewValue}"),
            runOnceImmediately: true);
    }
}