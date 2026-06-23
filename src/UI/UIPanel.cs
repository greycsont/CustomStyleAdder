using UnityEngine;
using Cysharp.Threading.Tasks;


namespace CustomStyleAdder.UI;

public abstract class UIPanel : MonoBehaviour
{
    public virtual UniTask Show()
    {
        gameObject.SetActive(true);
        return UniTask.CompletedTask;
    }

    public virtual UniTask Hide()
    {
        gameObject.SetActive(false);
        return UniTask.CompletedTask;
    }
}