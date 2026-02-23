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
    [SerializeField] private EducationTheme _startWith=EducationTheme.Clear;
    [SerializeField] private bool _isNotEducationScene;

    private List<EducationTheme> _openedThemes;

    public void PointerDown()
    {
        _educationWindow.SetActive(false);

        CloseEducationWindow(_startWith);
    }
    public void NextWindow()
    {
        //if ((int)_startWith < (int)EducationTheme.BuildPoints) return;

        StartCoroutine(ShowEducation((EducationTheme)(int)_startWith++));

    }

    private void OnEnable()
    {
        _triggers.ForEach(trigger => trigger.OnEnter += ShowEducationWindow);
        _triggers.ForEach(trigger => trigger.OnExit += CloseEducationWindow);
        if (_story == null) return;
        _story.OnFinished += SetPause;
    }
    private void OnDisable()
    {
        _triggers.ForEach(trigger => trigger.OnEnter -= ShowEducationWindow);
        _triggers.ForEach(trigger => trigger.OnExit -= CloseEducationWindow);
        ExitPause();
        if (_story == null) return;
        _story.OnFinished -= SetPause;
    }

    private IEnumerator ShowEducation(EducationTheme theme)
    {
        float seconds = _educationDataSO.data
            .Where(e=>e.theme==theme)
            .Select(e=>e.timeBefore).FirstOrDefault();

        yield return new WaitForSecondsRealtime(seconds);
        ShowEducationWindow(theme);
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
        //_educationWindow?.SetActive(false);
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
