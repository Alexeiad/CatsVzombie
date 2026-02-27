using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ElementsTasks : MonoBehaviour
{
    private List<UI_ElementBehaviour> ui_elements = new();

    // Стек открытых панелей — вершина = текущая активная
    private readonly Stack<UI_ElementBehaviour> panelHistory = new();

    // Последняя закрытая панель — для возврата через Escape
    private UI_ElementBehaviour lastClosedPanel;

    private void Awake()
    {
        ui_elements = FindObjectsByType<UI_ElementBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        ).ToList();
    }

    private void OnEnable()
    {
        if (ui_elements.Count == 0)
        {
            Debug.LogError("UI элементов не найдено!");
            return;
        }

        foreach (var button in ui_elements.Where(ui => ui.ElementType == ElementType.Button))
        {
            button.OnClick += () => HandleButtonClick(button.ButtonType);
        }
    }

    private void OnDisable()
    {
        foreach (var button in ui_elements.Where(ui => ui.ElementType == ElementType.Button))
        {
            button.OnClick -= () => HandleButtonClick(button.ButtonType);
        }
    }

    private void Update()
    {
        bool backPressed =
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame ||
            Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (!backPressed) return;

        if (panelHistory.Count > 0)
        {
            // Закрыть текущую верхнюю панель
            lastClosedPanel = panelHistory.Pop();
            ClosePanel(lastClosedPanel);

            // Закрыть всё остальное в стеке (если вдруг что-то осталось)
            CloseAllPanelsInStack();
        }
        else if (lastClosedPanel != null)
        {
            // Все панели закрыты — открыть последнюю закрытую
            OpenPanel(lastClosedPanel);
            lastClosedPanel = null;
        }
    }

    private void HandleButtonClick(ButtonType clickedButtonType)
    {
        var targetPanel = ui_elements.FirstOrDefault(ui =>
            ui.ElementType == ElementType.Panel &&
            ui.ButtonType == clickedButtonType);

        if (targetPanel == null) return;

        // Найти кнопку которая вызвала клик — чтобы проверить isInner
        var sourceButton = ui_elements.FirstOrDefault(ui =>
            ui.ElementType == ElementType.Button &&
            ui.ButtonType == clickedButtonType);

        bool inner = sourceButton != null && sourceButton.isInner;

        if (targetPanel.PanelState == PanelState.Open)
        {
            // Повторный клик — закрыть панель
            lastClosedPanel = panelHistory.Count > 0 ? panelHistory.Pop() : null;
            if (!inner) CloseAllPanelsInStack();
            ClosePanel(targetPanel);
        }
        else
        {
            // Если вложенная — не трогаем родительские панели, просто открываем сверху
            if (!inner) CloseAllPanelsInStack();
            OpenPanel(targetPanel);
        }
    }

    // ─── Приватные методы ──────────────────────────────────────────────────────

    private void OpenPanel(UI_ElementBehaviour panel)
    {
        panel.PanelState = PanelState.Open;
        panel.gameObject.SetActive(true);
        panelHistory.Push(panel);

        panel.PauseState = panel.SetPause ? SetLevelPause() : OutLevelPause();
    }

    private void ClosePanel(UI_ElementBehaviour panel)
    {
        panel.PanelState = PanelState.Close;
        panel.gameObject.SetActive(false);

        // Снимаем паузу только если больше нет открытых панелей с паузой
        bool anyPausedOpen = panelHistory.Any(p => p.SetPause);
        panel.PauseState = anyPausedOpen ? SetLevelPause() : OutLevelPause();
    }

    private void CloseAllPanelsInStack()
    {
        while (panelHistory.Count > 0)
        {
            var panel = panelHistory.Pop();
            panel.PanelState = PanelState.Close;
            panel.gameObject.SetActive(false);
        }
        OutLevelPause();
    }

    private PauseState SetLevelPause()
    {
        Time.timeScale = 0f;
        return PauseState.Pause;
    }

    private PauseState OutLevelPause()
    {
        Time.timeScale = 1f;
        return PauseState.Play;
    }
}