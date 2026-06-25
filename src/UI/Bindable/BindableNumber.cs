using System;

namespace CustomStyleAdder.UI;

/// <summary>
/// A numeric <see cref="Bindable{T}"/> whose value is clamped into an optional
/// [<see cref="MinValue"/>, <see cref="MaxValue"/>] range.
/// it's not steal all from osu-framwork cuz it's complex as f
/// </summary>
/// <typeparam name="T">A comparable value type (e.g. int, float, double).</typeparam>
public class BindableNumber<T> : Bindable<T> where T : struct, IComparable<T>
{
    /// <summary>Lower bound; null means unbounded.</summary>
    public T? MinValue { get; set; }

    /// <summary>Upper bound; null means unbounded.</summary>
    public T? MaxValue { get; set; }

    public BindableNumber(T defaultValue = default) : base(defaultValue) { }

    /// <inheritdoc/>
    public override T Value
    {
        get => base.Value;
        set => base.Value = Clamp(value);
    }

    /// <summary>Clamps a value into the configured range.</summary>
    private T Clamp(T value)
    {
        if (MinValue is T min && value.CompareTo(min) < 0) return min;
        if (MaxValue is T max && value.CompareTo(max) > 0) return max;
        return value;
    }

    /// <inheritdoc/>
    protected override Bindable<T> CreateInstance() => new BindableNumber<T>();

    /// <summary>Also copies the range from the source before binding.</summary>
    public override void BindTo(Bindable<T> other)
    {
        if (other is BindableNumber<T> num)
        {
            MinValue = num.MinValue;
            MaxValue = num.MaxValue;
        }
        base.BindTo(other);
    }
}
