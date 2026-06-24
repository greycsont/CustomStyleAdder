// The whole concept is steal from osu-framework
// <https://github.com/ppy/osu-framework/blob/master/osu.Framework/Bindables/Bindable.cs>
// So it's kinda copyrighted to ppy Pty Ltd right?

using System;
using System.Collections.Generic;
using System.Globalization;

namespace CustomStyleAdder.UI;

// Non-generic base (osu-style) so a config store can hold heterogeneous Bindable<T>.
public interface IBindable : IParseable, IUnbindable
{
    object? GetValue();
}

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

    public T Value
    {
        get => _value;
        set
        {
            if (Disabled)
                throw new InvalidOperationException($"Can not set _value to \"{value?.ToString()}\" as bindable is disabled.");

            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            SetValue(this._value, value);
        } 
    }

    public T Default
    {
        get => _defaultValue;
        set => _defaultValue = value;
    }
    
    public bool IsDefault => EqualityComparer<T>.Default.Equals(_value, _defaultValue);
    public void SetToDefault() => Value = Default;

    // ── Serialization (osu: IParseable + ToString) ──
    // GetValue boxes the current value for the JSON serializer; Parse coerces a
    // loosely-typed input (JSON object / console string) back into T.
    public object? GetValue() => _value;
    
    // IParseable
    public virtual void Parse(object input, IFormatProvider provider)
    {
        switch (input)
        {
            case T t:           // already the right type (e.g. bool/string from JSON)
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

    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (_disabled == value) return;
            SetDisabled(value,this);
        }
    }
    
    private void SetValue(T previous, T value, Bindable<T>? source = null)
    {
        this._value = value;

        T beforePropagation = this._value;
        
        if (_bindings != null)
            propagate(source, b => b.SetValue(previous, value, this));
        
        if (EqualityComparer<T>.Default.Equals(beforePropagation, value))
            ValueChanged?.Invoke(new ValueChangedEvent<T>(previous, value));
    }
    
    private void SetDisabled(bool value, Bindable<T>? source)
    {
        this._disabled = value;
        if (_bindings != null)
            propagate(source, b => b.SetDisabled(value, this));
        
        DisabledChanged?.Invoke(value); 
    }
    
    // Override this when create child class
    public virtual void BindTo(Bindable<T> other)
    {
        if (other == null) throw new ArgumentException(nameof(other));

        Value = other.Value;
        Disabled = other.Disabled;

        addWeakReference(other, this);
        addWeakReference(this, other);
    }

    public void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediately = false)
    {
        ValueChanged += onChange;
        if (runOnceImmediately)
            onChange(new ValueChangedEvent<T>(_value, _value));
    }

    public void BindDisabledChanged(Action<bool> onChange, bool runOnceImmediately = false)
    {
        DisabledChanged += onChange;
        if (runOnceImmediately)
            onChange(_disabled);
    }
    
    // Override this when create child class
    protected virtual Bindable<T> CreateInstance() => new Bindable<T>(_defaultValue);

    public Bindable<T> GetBoundCopy()
    {
        var copy = CreateInstance();
        copy.BindTo(this);
        return copy;
    }

    public Bindable<T> GetUnboundCopy()
    {
        var copy = CreateInstance();
        copy.Value = this._value;
        copy.Disabled = this._disabled;
        return copy;
    }
    
    // Use this after UI is destoryed
    // otherwise it'll memory leaking
    public void UnbindEvents()
    {
        ValueChanged = null;
        DisabledChanged = null;
    }
    
    // Remove itself from all binding's list
    public void UnbindBindings()
    {
        if (_bindings == null) return;
        foreach (var weak in _bindings)
            if (weak.TryGetTarget(out var other))
                removeWeakReference(other, this);

        _bindings.Clear();
    }

    public void UnbindFrom(IUnbindable them)                                                                                                                                                                                    
    {                                                                                                                                                                                                                           
        if (them is not Bindable<T> other)                                                                                                                                                                                      
            throw new InvalidOperationException($"Can't unbind from a non-matching bindable.");                                                                                                                                 
        removeWeakReference(this, other);                                                                                                                                                                                       
        removeWeakReference(other, this);                                                                                                                                                                                       
    }

    public void UnbindAll()
    {
        UnbindEvents();
        UnbindBindings();
    }
    
    
    // propagate the action to other bindings, and clear some null reference
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