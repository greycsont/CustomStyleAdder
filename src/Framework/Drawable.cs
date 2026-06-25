// Base for bound controls: unbinds everything it holds when destroyed.

using System.Collections.Generic;
using UnityEngine;

namespace CustomStyleAdder.UI;

public abstract class Drawable : MonoBehaviour
{
    private readonly List<IUnbindable> _managed = new();

    // Register a bindable to auto-unbind on destroy. TB = the bindable's own type.
    protected TB Register<TB>(TB bindable) where TB : IUnbindable
    {
        _managed.Add(bindable);
        return bindable;
    }

    protected virtual void OnDestroy()
    {
        foreach (var b in _managed)
            b.UnbindAll();
        _managed.Clear();
    }
}
