using System.Collections;
using UnityEngine;
using CustomStyleAdder.UI.Animation;

namespace CustomStyleAdder.UI;

public class AnimatedPanel : UIPanel
{
    [SerializeField] private CanvasGroup? canvasGroup;   // dragged in Editor
    [SerializeField] private float fadeMs = 200f;        // tuned in Editor

    private Coroutine? _anim;

    private void Reset() => canvasGroup = GetComponent<CanvasGroup>();

    public override void Show()
    {
        gameObject.SetActive(true);
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        StartAnim(canvasGroup.FadeTo(1f, fadeMs, Easing.OutQuint));
    }

    public override void Hide()
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }
        StartAnim(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        yield return canvasGroup!.FadeTo(0f, fadeMs, Easing.OutQuint);
        gameObject.SetActive(false);   // skipped if interrupted by Show (coroutine stopped)
    }

    // Starting a new animation cancels the previous one (StopCoroutine).
    private void StartAnim(IEnumerator routine)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(routine);
    }

    protected virtual void OnDestroy()
    {
        if (_anim != null) StopCoroutine(_anim);
    }
}
