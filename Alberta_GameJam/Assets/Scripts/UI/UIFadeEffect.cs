using System;
using System.Collections;
using UnityEngine;

public class UIFadeEffect : MonoBehaviour
{
    [SerializeField] float _fadeDuration;
    SpriteRenderer _spriteRenderer;
    Coroutine _activeFade;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        SetAlpha(0);
    }

    public void FadeIn(Action onComplete = null)
    {
        StartFade(1f, onComplete);
    }

    public void FadeOut(Action onComplete = null)
    {
        StartFade(0f, onComplete);
    }

    void StartFade(float targetAlpha, Action onComplete)
    {
        if (!gameObject.activeInHierarchy)
        {
            SetAlpha(targetAlpha);
            onComplete?.Invoke();
            return;
        }

        if (_activeFade != null)
        {
            StopCoroutine(_activeFade);
        }

        if (_fadeDuration <= 0f)
        {
            SetAlpha(targetAlpha);
            onComplete?.Invoke();
            _activeFade = null;
            return;
        }

        _activeFade = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        float startAlpha = _spriteRenderer.color.a;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetAlpha(targetAlpha);
        _activeFade = null;
        onComplete?.Invoke();
    }

    void SetAlpha(float alpha)
    {
        Color c = _spriteRenderer.color;
        c.a = alpha;
        _spriteRenderer.color = c;
    }
}
