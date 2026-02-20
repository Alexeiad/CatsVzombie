using System;
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

    public UnityAction OnFinished;

    public delegate void UnityAction();

    private int _index;

    public void NextSlide()
    {
        _targetImage.sprite = NextSprite();
    }
    public void StartWith(int startIndex = 0)
    {

        Time.timeScale = 0;
        _canvasStory.SetActive(true);
        _targetImage.sprite = _slides[startIndex];
        StartCoroutine(NextTo());
    }

    private void Start()
    {
        StartWith();
    }
    
    private IEnumerator NextTo()
    {
       
        while (true)
        {
            _targetImage.sprite = NextSprite();
            yield return new WaitForSecondsRealtime(_autoNextDelay);
            
        }
    }
    private Sprite NextSprite() =>
    
         (_index < _slides.Count) ? _slides[_index++] : SpriteFinish();
    
    
    private Sprite SpriteFinish()
    {
        Time.timeScale = 1;
        _canvasStory.SetActive(false); 
        _targetImage.sprite = _slides.First();

        OnFinished?.Invoke();
        return null;
    }

}
