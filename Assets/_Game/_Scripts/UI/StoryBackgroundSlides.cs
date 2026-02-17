using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryBackgroundSlides : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image targetImage;

    [Header("Slides")]
    [SerializeField] private List<Sprite> slides = new List<Sprite>();

    [Header("Auto")]
    [SerializeField] private float autoNextDelay = 3f;

    /// <summary>
    /// —обытие после последнего слайда (после последнего клика/автопролистывани€).
    /// Ќазначать извне: story.OnFinished += ...
    /// </summary>
    public UnityAction OnFinished;

    private int index = -1;
    private float lastInteractionTime;
    private bool finished;

    // UnityAction без UnityEngine.Events (как ты и попросил)
    public delegate void UnityAction();

    private void Start()
    {
        // Start запускает публичный метод, который стартует показ
        StartShow();
    }

    private void Update()
    {
        if (finished) return;
        if (slides == null || slides.Count == 0) return;

        // автопролистывание если давно не нажимали
        if (Time.time - lastInteractionTime >= autoNextDelay)
        {
            ShowNext();
            lastInteractionTime = Time.time; // сбрасываем таймер, чтобы не листало каждый кадр
        }
    }

    /// <summary>
    /// ѕубличный метод старта (можно дергать извне).
    /// </summary>
    public void StartShow()
    {
        finished = false;
        index = -1;
        lastInteractionTime = Time.time;

        ShowNext(); // показать первый слайд
    }

    /// <summary>
    /// ѕубличный метод дл€ клика/тапа.
    /// ѕодключи к кнопке OnClick или EventTrigger PointerDown.
    /// </summary>
    public void PointerDown()
    {
        if (finished) return;

        lastInteractionTime = Time.time;
        ShowNext();
    }

    /// <summary>
    /// ѕоказ следующего слайда. ≈сли слайды закончились Ч вызывает OnFinished.
    /// </summary>
    public void ShowNext()
    {
        if (finished) return;

        if (slides == null || slides.Count == 0)
        {
            Finish();
            return;
        }

        index++;

        if (index >= slides.Count)
        {
            Finish();
            return;
        }

        if (targetImage != null)
            targetImage.sprite = slides[index];
    }

    private void Finish()
    {
        finished = true;
        OnFinished?.Invoke();
    }
}
