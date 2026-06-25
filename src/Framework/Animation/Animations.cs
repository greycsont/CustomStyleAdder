using System;
using System.Collections;
using UnityEngine;

namespace CustomStyleAdder.UI.Animation;

public static class Animations
{
    public static IEnumerator FadeTo(this CanvasGroup cg, float alpha, float ms, Func<float, float>? easing = null)
        => Tween.To(cg.alpha, alpha, ms, a => cg.alpha = a, easing ?? Easing.OutQuint);

    public static IEnumerator MoveToY(this RectTransform rt, float y, float ms, Func<float, float>? easing = null)
    {
        var start = rt.anchoredPosition;
        return Tween.To(start.y, y, ms, v => rt.anchoredPosition = new Vector2(start.x, v), easing ?? Easing.OutQuint);
    }

    public static IEnumerator ScaleTo(this Transform tr, float scale, float ms, Func<float, float>? easing = null)
    {
        float start = tr.localScale.x;
        return Tween.To(start, scale, ms, s => tr.localScale = Vector3.one * s, easing ?? Easing.OutQuint);
    }
}
