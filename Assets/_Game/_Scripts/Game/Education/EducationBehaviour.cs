using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EducationBehaviour : MonoBehaviour
{
    [SerializeField] private List<EducationTrigger> _triggers;
    [SerializeField] private EducationDataSO _educationDataSO;

    [SerializeField] private GameObject _educationWindow;
    [SerializeField] private TextMeshProUGUI _educationText;
    [SerializeField] private StoryBackgroundSlides _story;

    private List<EducationTheme> _openedThemes;

    public void PointerDown()
    {
        _educationWindow.SetActive(false);
        ExitPause();
    }
   

    private void OnEnable()
    {
        _triggers.ForEach(trigger => trigger.OnEnter += ShowEducationWindow);
        _triggers.ForEach(trigger => trigger.OnExit += CloseEducationWindow);
        _story.OnFinished += SetPause;
    }
    private void OnDisable()
    {
        _triggers.ForEach(trigger => trigger.OnEnter -= ShowEducationWindow);
        _triggers.ForEach(trigger => trigger.OnExit -= CloseEducationWindow);
        _story.OnFinished -= SetPause;
        ExitPause();
    }
    private void ShowEducationWindow(EducationTheme theme)
    {
        var result = _educationDataSO.data
            .Select(data => new
            {
                Data = data,
                CanShow = data.theme == theme &&
                         (_openedThemes == null || !_openedThemes.Any(t => t == theme))
            })
            .Where(x => x.CanShow)
            .Select(x => x.Data)
            .DefaultIfEmpty(null)
            .FirstOrDefault();

        if (result == null) return;

        _educationWindow.SetActive(true);
        _educationText.text = result.text;

        _openedThemes = (_openedThemes ?? Enumerable.Empty<EducationTheme>())
            .Append(theme)
            .Distinct()
            .ToList();

        SetPause();
    }

    private void CloseEducationWindow(EducationTheme theme)
    {
        _educationWindow?.SetActive(false);
        ExitPause();
    }
        

    private void ExitPause()
    {
        Time.timeScale = 1;
    }
    private void SetPause()
    {
        Time.timeScale = 0;
    }
}
