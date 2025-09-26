using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    UIFadeEffect _fadeEffect;
    
    public override void Awake()
    {
        base.Awake();
        _fadeEffect = GetComponentInChildren<UIFadeEffect>();
    }

    public void StartGame()
    {
        _fadeEffect.FadeIn(() => LoadScene("Level01"));
    }

    void LoadScene(string levelName)
    {
        StartCoroutine(LoadSceneAsync(levelName, OnLoaded));
    }

    IEnumerator LoadSceneAsync(string levelName, Action onComplete)
    {
        var op = SceneManager.LoadSceneAsync(levelName);
        while (!op.isDone)
            yield return null;
        onComplete?.Invoke();
    }

    void OnLoaded()
    {
        _fadeEffect.FadeOut();
    }
}