// uGUI Slider bound to a CsaSetting float. (Editor/bundle path not tested yet)

using UnityEngine;
using UnityEngine.UI;

namespace CustomStyleAdder.UI;

[RequireComponent(typeof(Slider))]
public class SliderControl : Drawable
{
    [SerializeField] private CsaSetting settingKey;   // Inspector dropdown
    [SerializeField] private Slider? slider;          // dragged in Editor, or auto-grabbed

    private Bindable<float>? _bound;

    private void Reset() => slider = GetComponent<Slider>();

    private void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        if (slider == null) return;

        _bound = Register(CsaConfig.Instance.GetBindable<float>(settingKey));

        // Mirror the bindable's range onto the slider, if it carries one.
        if (_bound is BindableNumber<float> num)
        {
            if (num.MinValue is float min) slider.minValue = min;
            if (num.MaxValue is float max) slider.maxValue = max;
        }

        _bound.BindValueChanged(e => slider!.value = e.NewValue, runOnceImmediately: true);  // data -> UI
        slider.onValueChanged.AddListener(v => _bound!.Value = v);                            // UI -> data
    }
}
