using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryBackgroundSlides : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _targetImage;
    [SerializeField] private GameObject _canvasStory;

    [Header("Slides")]
    [SerializeField] private List<Sprite> _slides = new List<Sprite>();

    [Header("Timing")]
    [SerializeField] private float _slideDelay = 3f;        // задержка между слайдами
    [SerializeField] private float _lastSlideHold = 10f;    // сколько висит ПОСЛЕДНИЙ слайд перед загрузкой

    [Header("Default Range (auto-start)")]
    [SerializeField] private int _defaultFrom = 0;
    [SerializeField] private int _defaultTo = 3;

    [Header("Scene")]
    [SerializeField] private int _sceneToLoad = 1;

    public delegate void UnityAction();
    public UnityAction OnFinished;

    private int _currentFrom;
    private int _currentTo;
    private int _index;
    private bool _isActive;
    private bool _sceneLoading;
    private Coroutine _slideCoroutine;

    // ─────────────────────────────────────────────────────────
    //  Публичный запуск: можно вызвать с любыми индексами
    // ─────────────────────────────────────────────────────────
    public void StartWith(int from, int to)
    {
        // Защита от кривых аргументов
        from = Mathf.Clamp(from, 0, _slides.Count - 1);
        to = Mathf.Clamp(to, from, _slides.Count - 1);

        if (_isActive) ForceStop();

        _currentFrom = from;
        _currentTo = to;
        _index = from;
        _isActive = true;
        _sceneLoading = false;

        Time.timeScale = 0f;
        _canvasStory.SetActive(true);

        _slideCoroutine = StartCoroutine(SlideLoop());
    }

    // Остановить принудительно (без OnFinished и без загрузки сцены)
    public void ForceStop()
    {
        if (_slideCoroutine != null)
        {
            StopCoroutine(_slideCoroutine);
            _slideCoroutine = null;
        }
        _isActive = false;
        Time.timeScale = 1f;
        _canvasStory.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────
    private void Start()
    {
        StartWith(_defaultFrom, _defaultTo);
    }

    private void OnDisable()
    {
        if (_isActive)
        {
            _isActive = false;
            Time.timeScale = 1f;
            OnFinished?.Invoke();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // ─────────────────────────────────────────────────────────
    //  Основная корутина
    // ─────────────────────────────────────────────────────────
    private IEnumerator SlideLoop()
    {
        bool isLastSlideInList = (_currentTo == _slides.Count - 1);

        while (_isActive)
        {
            // Показываем текущий слайд
            _targetImage.sprite = _slides[_index];

            bool isLastInRange = (_index == _currentTo);

            if (isLastInRange)
            {
                // Если это финальный слайд всего списка — ждём дольше, потом грузим сцену
                if (isLastSlideInList)
                {
                    yield return new WaitForSecondsRealtime(_lastSlideHold);
                    LoadNextScene();
                }
                else
                {
                    // Обычный финал диапазона — ждём обычную задержку, вызываем OnFinished
                    yield return new WaitForSecondsRealtime(_slideDelay);
                    FinishSlides();
                }
                yield break;
            }

            // Не последний — просто ждём и двигаемся дальше
            yield return new WaitForSecondsRealtime(_slideDelay);

            if (!_isActive) yield break;
            _index++;
        }
    }

    // ─────────────────────────────────────────────────────────
    private void FinishSlides()
    {
        _isActive = false;
        Time.timeScale = 1f;
        _canvasStory.SetActive(false);
        _slideCoroutine = null;
        OnFinished?.Invoke();
    }

    private void LoadNextScene()
    {
        if (_sceneLoading) return;
        _sceneLoading = true;
        _isActive = false;
        Time.timeScale = 1f;

 

        OnFinished?.Invoke();

        DG.Tweening.DOTween.KillAll();

        SceneManager.LoadScene(_sceneToLoad);
    }

    // ─────────────────────────────────────────────────────────
    public bool IsActive => _isActive;
    public int CurrentIndex => _index;
    public int TotalSlides => _slides?.Count ?? 0;
}
