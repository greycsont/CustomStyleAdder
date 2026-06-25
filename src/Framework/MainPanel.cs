// The mod's root overlay: a set of switchable panels (one visible at a time).
// Layout/visuals live in the Editor; buttons call Hide()/ShowPanel() via their OnClick.

using UnityEngine;

namespace CustomStyleAdder.UI;

public class MainPanel : Overlay
{
    // Set once the prefab is in the scene, so the hotkey in Plugin can reach it.
    public static MainPanel? Instance { get; private set; }

    [SerializeField] private UIPanel[] panels = new UIPanel[0];  // the switchable screens
    [SerializeField] private UIPanel? defaultPanel;              // shown when the overlay opens

    private void Awake() => Instance = this;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this) Instance = null;
    }

    public override void Show()
    {
        base.Show();
        ShowPanel(defaultPanel != null ? defaultPanel
                : panels.Length > 0   ? panels[0]
                : null);
    }

    // Show one panel, hide the rest. Wire a tab button's OnClick to this with the target panel dragged in.
    public void ShowPanel(UIPanel? target)
    {
        foreach (var p in panels)
        {
            if (p == null) continue;
            if (p == target) p.Show();
            else p.Hide();
        }
    }
}
