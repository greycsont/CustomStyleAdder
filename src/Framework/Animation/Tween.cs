using System;
using System.Collections;
using UnityEngine;

namespace CustomStyleAdder.UI.Animation;

public static class Tween
{
    // Per-frame lerp via coroutine (the NuGet UniTask has no Unity PlayerLoop).
    // unscaledDeltaTime so it keeps running while the game is paused.
    public static IEnumerator To(float from, float to, float durationMs,
        Action<float> setter, Func<float, float> easing)
    {
        if (durationMs <= 0f)
        {
            setter(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < durationMs)
        {
            elapsed += Time.unscaledDeltaTime * 1000f;
            float t = Mathf.Clamp01(elapsed / durationMs);
            setter(Mathf.LerpUnclamped(from, to, easing(t)));
            yield return null;
        }
        setter(to);
    }
}
