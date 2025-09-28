using System.Collections;
using TMPro;
using UnityEngine;

public class prompt : MonoBehaviour
{
    [SerializeField] float visibleDuration = 2f;
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] bool deactivateOnFadeComplete = true;

    TextMeshProUGUI _text;
    Coroutine _fadeRoutine;
    float _defaultVisibleDuration;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _defaultVisibleDuration = visibleDuration;
    }

    void OnEnable()
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        SetAlpha(1f);
        BeginFadeSequence(_defaultVisibleDuration);
    }

    void OnDisable()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
    }

    public void Show(string message, float? overrideVisibleDuration = null)
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        if (_text == null)
        {
            return;
        }

        _text.text = message;
        SetAlpha(1f);

        float waitTime = overrideVisibleDuration ?? _defaultVisibleDuration;
        BeginFadeSequence(waitTime);
    }

    void BeginFadeSequence(float waitTime)
    {
        if (_text == null)
        {
            return;
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(waitTime));
    }

    IEnumerator FadeRoutine(float waitTime)
    {
        if (waitTime > 0f)
        {
            yield return new WaitForSecondsRealtime(waitTime);
        }

        if (fadeDuration <= 0f)
        {
            SetAlpha(0f);
            if (deactivateOnFadeComplete)
            {
                gameObject.SetActive(false);
            }
            _fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        SetAlpha(0f);

        if (deactivateOnFadeComplete)
        {
            gameObject.SetActive(false);
        }

        _fadeRoutine = null;
    }

    void SetAlpha(float alpha)
    {
        if (_text == null)
        {
            return;
        }

        Color color = _text.color;
        color.a = alpha;
        _text.color = color;
    }
}
