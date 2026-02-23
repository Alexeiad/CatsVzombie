using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EducationTrigger : MonoBehaviour
{
    public UnityAction<EducationTheme> OnEnter,OnExit;
    [SerializeField] private EducationTheme _theme;
    [SerializeField] private bool _startStory;
    [SerializeField] private StoryBackgroundSlides _storyBackgroundSlides;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            if (_startStory)
            {
                _storyBackgroundSlides?.StartWith(5,6);
            }
            else
            {
                OnEnter?.Invoke(_theme);
            }    
            

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            OnExit?.Invoke(_theme);

        }
    }
}
