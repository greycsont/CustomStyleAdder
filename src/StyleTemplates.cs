namespace CustomStyleAdder;

public static class StyleTemplates
{
    public static StyleRule OnMethod(string id, string name, int points,
                                         string className, string methodName,
                                         string[]? argTypes = null)
            => new StyleRule(
                id,
                new CustomStyle(name, points),
                new StyleTrigger
                {
                    className  = className,
                    methodName = methodName,
                    argTypes   = argTypes
                });
}
