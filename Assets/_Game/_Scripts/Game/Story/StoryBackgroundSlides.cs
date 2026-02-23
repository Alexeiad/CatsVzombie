using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class StoryBackgroundSlides : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _targetImage;
    [SerializeField] private GameObject _canvasStory;

    [Header("Slides")]
    [SerializeField] private List<Sprite> _slides = new List<Sprite>();

    [Header("Auto")]
    [SerializeField] private float _autoNextDelay = 3f;
    [SerializeField] private int _stopOnIn = 4;
    [SerializeField] private int _sceneBase=1;

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
        if (_isActive)
        {
            StopSlides();
        }

        _stopOnIn = stopOnIn > 0 ? stopOnIn : _stopOnIn;
        _index = startIndex;
        _isActive = true;

        Time.timeScale = 0;
        _canvasStory.SetActive(true);

        _targetImage.sprite = _slides[startIndex];
        
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

         StartWith();
    }
    private void Update()
    {
        if (_targetImage.sprite == _slides[_slides.Count - 1])
        {
            SceneManager.LoadScene(_sceneBase);
        }
    }
    private void OnDisable()
    {

        if (_isActive)
        {
            _isActive = false;
            Time.timeScale = 1;
            OnFinished?.Invoke();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
    }

    private IEnumerator NextTo()
    {
        while (_isActive)
        {
            yield return new WaitForSecondsRealtime(_autoNextDelay);

            if (_isActive) 
            {
                _targetImage.sprite = NextSprite();
            }
        }
    }

    private Sprite NextSprite()
    {


        
        if (_index <_stopOnIn)
        {
            return _slides[_index++];
        }
        else if (_index == _stopOnIn)
        {
            return SpriteFinish();
        }
        else return null;
        

           

        
    }

    private Sprite SpriteFinish()
    {
        StopSlides();
        return null;
    }
    public bool IsActive => _isActive;
    public int CurrentIndex => _index;
    public int TotalSlides => _slides?.Count ?? 0;
}