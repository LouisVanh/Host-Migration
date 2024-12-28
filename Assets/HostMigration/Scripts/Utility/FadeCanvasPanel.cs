using UnityEngine;

public class FadePanel : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Get the CanvasGroup attached to this GameObject
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component missing. Please add a CanvasGroup component to this GameObject.");
        }
    }

    /// <summary>
    /// Fades the panel in.
    /// </summary>
    /// <param name="duration">The duration of the fade effect.</param>
    public void FadeIn(float duration)
    {
        StartCoroutine(Fade(0, 1, duration));
    }

    /// <summary>
    /// Fades the panel out.
    /// </summary>
    /// <param name="duration">The duration of the fade effect.</param>
    public void FadeOut(float duration)
    {
        StartCoroutine(Fade(1, 0, duration));
    }

    /// <summary>
    /// Fades the panel from a given start alpha to an end alpha. If duration is 0, do it instantly.
    /// </summary>
    private System.Collections.IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if(duration == 0)
        {
            _canvasGroup.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        // Ensure the final alpha is set
        _canvasGroup.alpha = endAlpha;
    }
}
