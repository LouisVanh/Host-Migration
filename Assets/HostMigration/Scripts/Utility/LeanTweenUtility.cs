using UnityEngine;

public static class LeanTweenUtility
{
    /// <summary>
    /// Scales a GameObject to the specified scale.
    /// </summary>
    /// <param name="target">The target GameObject.</param>
    /// <param name="targetScale">The scale to animate to.</param>
    /// <param name="duration">The duration of the scale animation.</param>
    public static void ScaleTo(GameObject target, Vector3 targetScale, float duration)
    {
        if (target == null) return;

        LeanTween.scale(target, targetScale, duration).setEase(LeanTweenType.easeInOutQuad);
    }

    /// <summary>
    /// Scales a GameObject in (to its normal size) and activates it if not already active.
    /// </summary>
    /// <param name="target">The target GameObject.</param>
    /// <param name="normalScale">The normal scale of the GameObject.</param>
    /// <param name="duration">The duration of the scale-in animation.</param>
    public static void ScaleIn(GameObject target, Vector3 normalScale, float duration)
    {
        if (target == null) return;

        target.SetActive(true);
        ScaleTo(target, normalScale, duration);
    }

    /// <summary>
    /// Scales a GameObject out (to zero) and optionally deactivates it afterward.
    /// </summary>
    /// <param name="target">The target GameObject.</param>
    /// <param name="duration">The duration of the scale-out animation.</param>
    /// <param name="deactivateAfter">Whether to deactivate the GameObject after scaling out.</param>
    public static void ScaleOut(GameObject target, float duration, bool deactivateAfter = true)
    {
        if (target == null) return;

        LeanTween.scale(target, Vector3.zero, duration).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            if (deactivateAfter)
            {
                target.SetActive(false);
            }
        });
    }

    /// <summary>
    /// Performs a pop-up animation by scaling the GameObject to a larger size and back to its normal scale.
    /// </summary>
    /// <param name="target">The target GameObject.</param>
    /// <param name="normalScale">The normal scale of the GameObject.</param>
    /// <param name="popUpScale">The larger pop-up scale.</param>
    /// <param name="duration">The total duration of the pop-up effect.</param>
    public static void PopUp(GameObject target, Vector3 normalScale, Vector3 popUpScale, float duration)
    {
        if (target == null) return;

        target.SetActive(true);
        target.transform.localScale = normalScale; // Reset to normal scale before animating
        LeanTween.scale(target, popUpScale, duration / 2).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
            LeanTween.scale(target, normalScale, duration / 2).setEase(LeanTweenType.easeInQuad);
        });
    }
}
