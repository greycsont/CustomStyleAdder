// uGUI Toggle bound to a CsaSetting bool. (Editor/bundle path not tested yet)

using UnityEngine;
using UnityEngine.UI;

namespace CustomStyleAdder.UI;

[RequireComponent(typeof(Toggle))]
public class BoolToggle : Drawable
{
    [SerializeField] private CsaSetting settingKey;   // Inspector dropdown
    [SerializeField] private Toggle? toggle;          // dragged in Editor, or auto-grabbed

    private Bindable<bool>? _bound;

    private void Reset() => toggle = GetComponent<Toggle>();

    private void Awake()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        if (toggle == null) return;

        _bound = Register(CsaConfig.Instance.GetBindable<bool>(settingKey));        // auto-unbound by Drawable.OnDestroy
        _bound.BindValueChanged(e => toggle!.isOn = e.NewValue, runOnceImmediately: true);  // data -> UI
        toggle.onValueChanged.AddListener(v => _bound!.Value = v);               // UI -> data
    }
}
