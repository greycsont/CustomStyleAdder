using UnityEngine;

namespace CustomStyleAdder.UI.Animation;

public static class Easing
{
    public static float Linear(float t)    => t;
    public static float OutQuint(float t)  => 1f - Mathf.Pow(1f - t, 5f);
    public static float OutCubic(float t)  => 1f - Mathf.Pow(1f - t, 3f);
    public static float InOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
}
