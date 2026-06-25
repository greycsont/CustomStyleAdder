// Rule list -> UI. Few rules, so on any change we just clear and re-instantiate the rows.
// Binds to ProfileManager.Revision (a Bindable) so console edits refresh an open panel too.

using UnityEngine;

namespace CustomStyleAdder.UI;

public class RuleListView : MonoBehaviour
{
    [SerializeField] private RuleRow? rowPrefab;   // row template, authored in Editor
    [SerializeField] private Transform? container; // parent for the rows (e.g. a VerticalLayoutGroup)

    private Bindable<int>? _revision;

    private void OnEnable()
    {
        _revision = ProfileManager.Revision.GetBoundCopy();
        _revision.BindValueChanged(_ => Rebuild(), runOnceImmediately: true);
    }

    private void OnDisable()
    {
        _revision?.UnbindAll();
        _revision = null;
    }

    public void Rebuild()
    {
        if (rowPrefab == null || container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        var profile = ProfileManager.Current;
        if (profile == null) return;

        foreach (var rule in profile.rules)
        {
            var row = Instantiate(rowPrefab, container);
            row.gameObject.SetActive(true);
            row.Bind(rule, this);
        }
    }

    // Called by a row's delete button. Goes through the same path as `csa -r rm`.
    public void Remove(StyleRule rule)
    {
        var p = ProfileManager.Current;
        if (p == null) return;

        p.RemoveRule(rule);            // saves + bumps Revision -> Rebuild
        TriggerEngine.RebindAll(p.rules); // live-apply, mirrors the console path
    }
}
