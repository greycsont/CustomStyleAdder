// One row in the rule list. Prefab authored in Editor, filled at runtime by Bind().

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomStyleAdder.UI;

public class RuleRow : MonoBehaviour
{
    [SerializeField] private TMP_Text? idLabel;      // rule id
    [SerializeField] private TMP_Text? styleLabel;   // "<styleName> (+points)"
    [SerializeField] private TMP_Text? triggerLabel; // "<class>.<method>"
    [SerializeField] private Button? deleteButton;

    public void Bind(StyleRule rule, RuleListView owner)
    {
        if (idLabel != null)      idLabel.text      = rule.id;
        if (styleLabel != null)   styleLabel.text   = $"{rule.style.styleName} (+{rule.style.stylePoints})";
        if (triggerLabel != null) triggerLabel.text = $"{rule.trigger.className}.{rule.trigger.methodName}";

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => owner.Remove(rule));
        }
    }
}
