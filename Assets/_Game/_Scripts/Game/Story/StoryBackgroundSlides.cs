using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryBackgroundSlides : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _targetImage;
    [SerializeField] private GameObject _canvasStory;

    [Header("Slides")]
    [SerializeField] private List<Sprite> _slides = new List<Sprite>();

    [Header("Auto")]
    [SerializeField] private float _autoNextDelay = 3f;

    /// <summary>
    /// —обытие после последнего слайда (после последнего клика/автопролистывани€).
    /// Ќазначать извне: story.OnFinished += ...
    /// </summary>
    public UnityAction OnFinished;

    private int _index;
    private float _lastInteractionTime;
    private bool _finished;

    // UnityAction без UnityEngine.Events (как ты и попросил)
    public delegate void UnityAction();

    private void Start()
    {
        // Start запускает публичный метод, который стартует показ
        StartShow();
        ShowNext();
        Time.timeScale = 0;
    }

    private void Update()
    {
        if (_finished) return;

        // автопролистывание если давно не нажимали
        if (Time.unscaledTime - _lastInteractionTime > _autoNextDelay)
        {
            ShowNext();
            _index++;
            _lastInteractionTime = Time.unscaledTime; // сбрасываем таймер, чтобы не листало каждый кадр
        }
    }
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
    /// <summary>
    /// ѕубличный метод старта (можно дергать извне).
    /// </summary>
    public void StartShow()
    {
        _finished = false;
        _index = 0;
        _lastInteractionTime = Time.unscaledTime;
        _canvasStory.SetActive(true);
        ShowNext(); // показать первый слайд
    }

    /// <summary>
    /// ѕубличный метод дл€ клика/тапа.
    /// ѕодключи к кнопке OnClick или EventTrigger PointerDown.
    /// </summary>
    public void PointerDown()
    {
        if (_finished) return;

        _lastInteractionTime = Time.unscaledTime;
        ShowNext();
    }

    /// <summary>
    /// ѕоказ следующего слайда. ≈сли слайды закончились Ч вызывает OnFinished.
    /// </summary>
    public void ShowNext()
    {
        if (_finished) return;

        if (_index == _slides.Count)
        {
            Finish();
            return;
        }

        _targetImage.sprite = _slides[_index];

        
    }

    private void Finish()
    {
        _finished = true;
        Time.timeScale = 1f;
        _canvasStory.SetActive(false);
        OnFinished?.Invoke();
    }
}
