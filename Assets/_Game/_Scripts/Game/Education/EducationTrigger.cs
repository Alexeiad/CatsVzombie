using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EducationTrigger : MonoBehaviour
{
    public UnityAction<EducationTheme> OnEnter,OnExit;
    [SerializeField] private EducationTheme _theme;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            OnEnter?.Invoke(_theme);

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
