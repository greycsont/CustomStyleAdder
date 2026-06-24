// The whole concept is stolen from osu-framework
// <https://github.com/ppy/osu-framework/blob/master/osu.Framework/Bindables/Bindable.cs>
// So it's kinda copyrighted to ppy Pty Ltd right?

using System;
using System.Collections.Generic;
using System.Globalization;

namespace CustomStyleAdder.UI;

/// <summary>Non-generic base (osu-style) so a config store can hold heterogeneous <see cref="Bindable{T}"/>.</summary>
public interface IBindable : IParseable, IUnbindable
{
    /// <summary>Boxes the current value for serialization.</summary>
    object? GetValue();
}

/// <summary>A value container that broadcasts changes and can bind to other bindables.</summary>
/// <typeparam name="T">The value type.</typeparam>
public interface IBindable<T> : IBindable
{
    T Value { get; }
    bool Disabled { get; }
    event Action<ValueChangedEvent<T>> ValueChanged;
    event Action<bool>? DisabledChanged;
    void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediately = false);
    Bindable<T> GetBoundCopy();
}

public class Bindable<T> : IBindable<T>
{
    public event Action<ValueChangedEvent<T>>? ValueChanged;
    public event Action<bool>? DisabledChanged;

    private T _value;
    private T _defaultValue;
    private bool _disabled;

    private List<WeakReference<Bindable<T>>>? _bindings = new();

    public Bindable(T defaultValue = default!)
    {
        _value = Default = defaultValue;
    }

    /// <summary>The current value; setting it notifies subscribers and the bound network.</summary>
    public virtual T Value
    {
        get => _value;
        set
        {
            if (Disabled)
                throw new InvalidOperationException($"Can not set value to \"{value?.ToString()}\" as bindable is disabled.");

            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            SetValue(_value, value);
        }
    }

    /// <summary>The default value, used by <see cref="IsDefault"/> and <see cref="SetToDefault"/>.</summary>
    public T Default
    {
        get => _defaultValue;
        set => _defaultValue = value;
    }

    /// <summary>Whether the current value equals the default.</summary>
    public bool IsDefault => EqualityComparer<T>.Default.Equals(_value, _defaultValue);

    /// <summary>Resets the value to the default.</summary>
    public void SetToDefault() => Value = Default;

    /// <summary>Boxes the current value for the JSON serializer.</summary>
    public object? GetValue() => _value;

    /// <summary>Coerces a loosely-typed input (JSON object / console string) back into <typeparamref name="T"/>.</summary>
    /// <param name="input">The raw input value.</param>
    /// <param name="provider">Culture-specific formatting information.</param>
    public virtual void Parse(object? input, IFormatProvider provider)
    {
        switch (input)
        {
            case T t:               // already the right type (e.g. bool/string from JSON)
                Value = t;
                break;
            case null:
                Value = default!;
                break;
            default:
                var type = typeof(T);
                Value = type.IsEnum
                    ? (T)Enum.Parse(type, input.ToString()!)
                    : (T)Convert.ChangeType(input, type, provider);
                break;
        }
    }

    public override string ToString() => Convert.ToString(_value, CultureInfo.InvariantCulture) ?? "";

    /// <summary>Whether writes are rejected. Propagates through the bound network.</summary>
    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (_disabled == value) return;
            SetDisabled(value, this);
        }
    }

    private void SetValue(T previous, T value, Bindable<T>? source = null)
    {
        _value = value;

        T beforePropagation = _value;

        if (_bindings != null)
            propagate(source, b => b.SetValue(previous, value, this));

        // If a propagated callback changed the value again, this event is stale — skip it.
        if (EqualityComparer<T>.Default.Equals(beforePropagation, value))
            ValueChanged?.Invoke(new ValueChangedEvent<T>(previous, value));
    }

    private void SetDisabled(bool value, Bindable<T>? source)
    {
        _disabled = value;
        if (_bindings != null)
            propagate(source, b => b.SetDisabled(value, this));

        DisabledChanged?.Invoke(value);
    }

    /// <summary>Binds to another bindable, syncing value/disabled and joining its network. Override to copy extra state.</summary>
    public virtual void BindTo(Bindable<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        Value = other.Value;
        Disabled = other.Disabled;

        addWeakReference(other, this);
        addWeakReference(this, other);
    }

    /// <summary>Subscribes to value changes, optionally firing once immediately with the current value.</summary>
    public void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediately = false)
    {
        ValueChanged += onChange;
        if (runOnceImmediately)
            onChange(new ValueChangedEvent<T>(_value, _value));
    }

    /// <summary>Subscribes to disabled changes, optionally firing once immediately.</summary>
    public void BindDisabledChanged(Action<bool> onChange, bool runOnceImmediately = false)
    {
        DisabledChanged += onChange;
        if (runOnceImmediately)
            onChange(_disabled);
    }

    /// <summary>Creates a blank instance of the most-derived type. Override in subclasses for <see cref="GetBoundCopy"/>.</summary>
    protected virtual Bindable<T> CreateInstance() => new Bindable<T>(_defaultValue);

    /// <summary>A new copy bound to this one. Hold a local reference and unbind on destroy.</summary>
    public Bindable<T> GetBoundCopy()
    {
        var copy = CreateInstance();
        copy.BindTo(this);
        return copy;
    }

    /// <summary>A new copy with the same value/disabled but not bound to this one.</summary>
    public Bindable<T> GetUnboundCopy()
    {
        var copy = CreateInstance();
        copy.Value = _value;
        copy.Disabled = _disabled;
        return copy;
    }

    /// <summary>Clears all event subscriptions (call on UI destroy to avoid leaks).</summary>
    public void UnbindEvents()
    {
        ValueChanged = null;
        DisabledChanged = null;
    }

    /// <summary>Removes this bindable from every bound network (both directions).</summary>
    public void UnbindBindings()
    {
        if (_bindings == null) return;
        foreach (var weak in _bindings)
            if (weak.TryGetTarget(out var other))
                removeWeakReference(other, this);

        _bindings.Clear();
    }

    /// <summary>Severs the link with a single other bindable, both directions.</summary>
    public void UnbindFrom(IUnbindable them)
    {
        if (them is not Bindable<T> other)
            throw new InvalidOperationException("Can't unbind from a non-matching bindable.");

        removeWeakReference(this, other);
        removeWeakReference(other, this);
    }

    /// <summary>Calls <see cref="UnbindEvents"/> and <see cref="UnbindBindings"/>.</summary>
    public void UnbindAll()
    {
        UnbindEvents();
        UnbindBindings();
    }

    // Propagate an action to every bound bindable except the source, clearing dead weak refs.
    private void propagate(Bindable<T>? source, Action<Bindable<T>> action)
    {
        var bindings = _bindings!;
        for (int i = bindings.Count - 1; i >= 0; i--)
        {
            if (!bindings[i].TryGetTarget(out var target))
            {
                bindings.RemoveAt(i);
                continue;
            }

            if (ReferenceEquals(target, source))
                continue;
            action(target);
        }
    }

    private static void addWeakReference(Bindable<T> owner, Bindable<T> reference)
        => (owner._bindings ??= new()).Add(new WeakReference<Bindable<T>>(reference));

    private static void removeWeakReference(Bindable<T> owner, Bindable<T> reference)
    {
        var bindings = owner._bindings;
        if (bindings == null) return;

        for (int i = bindings.Count - 1; i >= 0; i--)
        {
            if (!bindings[i].TryGetTarget(out var target) || ReferenceEquals(target, reference))
                bindings.RemoveAt(i);
        }
    }
}
