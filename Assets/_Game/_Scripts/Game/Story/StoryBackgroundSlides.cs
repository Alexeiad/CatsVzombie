using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StoryBackgroundSlides : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _targetImage;
    [SerializeField] private GameObject _canvasStory;

    [Header("Slides")]
    [SerializeField] private List<Sprite> _slides = new List<Sprite>();

    [Header("Auto")]
    [SerializeField] private float _autoNextDelay = 3f;
    [SerializeField] private int _stopOnIn = 3;

    public UnityAction OnFinished;

    public delegate void UnityAction();

    private int _index;
    private bool _isActive;
    private Coroutine _nextSlideCoroutine;

    public void NextSlide()
    {
        if (!_isActive) return;
        _targetImage.sprite = NextSprite();
    }

    public void StartWith(int startIndex = 0, int stopOnIn = 0)
    {
        // Останавливаем предыдущую сессию, если она была активна
        if (_isActive)
        {
            StopSlides();
        }

        _stopOnIn = stopOnIn > 0 ? stopOnIn : _stopOnIn;
        _index = startIndex;
        _isActive = true;

        Time.timeScale = 0;
        _canvasStory.SetActive(true);

        if (_slides != null && _slides.Count > 0 && startIndex < _slides.Count)
        {
            _targetImage.sprite = _slides[startIndex];
        }

        _nextSlideCoroutine = StartCoroutine(NextTo());
    }

    public void StopSlides()
    {
        if (_nextSlideCoroutine != null)
        {
            StopCoroutine(_nextSlideCoroutine);
            _nextSlideCoroutine = null;
        }

        if (_isActive)
        {
            _isActive = false;
            Time.timeScale = 1;
            _canvasStory.SetActive(false);
            OnFinished?.Invoke();
        }
    }

    private void Start()
    {
        // Не запускаем автоматически в Start, 
        // чтобы избежать неожиданного поведения
         StartWith();
    }

    private void OnDisable()
    {
        // Принудительно восстанавливаем timeScale при отключении
        if (_isActive)
        {
            _isActive = false;
            Time.timeScale = 1;
            OnFinished?.Invoke();
        }
    }

    private void OnDestroy()
    {
        // Гарантированно восстанавливаем timeScale при уничтожении объекта
        Time.timeScale = 1;
    }

    private IEnumerator NextTo()
    {
        while (_isActive)
        {
            yield return new WaitForSecondsRealtime(_autoNextDelay);

            if (_isActive) // Проверяем, не остановили ли слайды во время ожидания
            {
                _targetImage.sprite = NextSprite();
            }
        }
    }

    private Sprite NextSprite()
    {
        if (_index < _stopOnIn && _index < _slides.Count)
        {
            return _slides[_index++];
        }
        else
        {
            return SpriteFinish();
        }
    }

    private Sprite SpriteFinish()
    {
        StopSlides();
        return null;
    }

    // Публичные свойства для проверки состояния
    public bool IsActive => _isActive;
    public int CurrentIndex => _index;
    public int TotalSlides => _slides?.Count ?? 0;
}