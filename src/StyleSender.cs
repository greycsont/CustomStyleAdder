namespace CustomStyleAdder;

public static class StyleSender
{
    public static bool leaderboardLock;
    public static void TrySendStyleToStyleHUD(CustomStyle customStyle)
    {
        var pointsShouldAdded = CsaConfig.Instance.Get<bool>(CsaSetting.StylePointLock) ? 0 : customStyle.stylePoints;
        MonoSingleton<StyleHUD>.Instance?.AddPoints(pointsShouldAdded, customStyle.styleName, 
            count: customStyle.count, 
            prefix: customStyle.prefix, 
            postfix: customStyle.postfix);
        if (pointsShouldAdded > 0) leaderboardLock = true;
    }

    public static void RefreshState()
    {
        leaderboardLock = false;
    }
}