
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UI_ElementsTasks : MonoBehaviour
{
    private List<UI_ElementBehaviour> ui_elements = new();
    private List<UI_ElementBehaviour> openUi_elements = new();
    private UI_Animation ui_animation;
    private bool isSafeTask,exceptSwith;

    private void Awake()
    {
        ui_animation = new();

        ui_elements = FindObjectsByType<UI_ElementBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        ).ToList();
        
    }
    private void OnEnable()
    {
        if (ui_elements.Count <= 0)
        {
            Debug.LogError("элементов в списке: " + ui_elements.Count);
        }
        ui_elements
            .Where(ui => ui.ElementType == ElementType.Button).ToList()
            .ForEach(button => {
                button.OnClick += () => HandleButtonClick(button.ButtonType);
                
            });
    }




    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame 
            || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
        {
            isSafeTask = !isSafeTask;
            exceptSwith = true;
        }

        if (exceptSwith)
        {
            var lastUi = openUi_elements?.LastOrDefault();

            if (lastUi != null)
            {
                lastUi.PanelState = isSafeTask ? PanelState.Open : PanelState.Close;

                lastUi.gameObject.SetActive(isSafeTask);

                lastUi.PauseState = isSafeTask ? SetLevelPause()
                    : OutLevelPause();

                lastUi.PauseState = isSafeTask ? lastUi.PauseState=PauseState.Pause
                    : lastUi.PauseState = PauseState.Play;

                exceptSwith = false;
            }
        }
           
    }
    private void HandleButtonClick(ButtonType clickedButtonType)
    {

        var relatedPanels = ui_elements
            .Where(ui => ui.ElementType == ElementType.Panel &&
                         ui.ButtonType == clickedButtonType).ToList();

        relatedPanels.ForEach(panel => {

            panel.gameObject.SetActive(ActiveState(panel));

            

            openUi_elements.Add(panel);
        });
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
    private bool ActiveState(UI_ElementBehaviour panel)
    {
        var sprite = panel.GetComponent<Image>();

        switch (panel.PanelState)
        {
            case PanelState.Close:
                panel.PanelState = PanelState.Open;
                panel.PauseState = panel.SetPause ? SetLevelPause() 
                    : panel.PauseState = PauseState.Play;
                return true;

            case PanelState.Open:
                
                panel.PanelState = PanelState.Close;
                panel.PauseState = OutLevelPause();

                return false;

            default:
                return false;
        }
    }

    private async Task HandleButtonClickAsync(ButtonType clickedButtonType)
    {
         
        var relatedPanels = ui_elements
            .Where(ui => ui.ElementType == ElementType.Panel &&
                         ui.ButtonType == clickedButtonType).ToList();

        relatedPanels.ForEach(async panel => {
            
            panel.gameObject.SetActive(await ActiveStateAsync(panel));
            openUi_elements.Add(panel);
            });
    }
    private async Task<bool> ActiveStateAsync(UI_ElementBehaviour panel)
    {
        var sprite = panel.GetComponent<Image>();

        switch (panel.PanelState)
        {
            case PanelState.Close:
                ui_animation.ToAppear(sprite);
                panel.PanelState = PanelState.Open;
                return true;

            case PanelState.Open:
                
                var tcs = new TaskCompletionSource<bool>();

                ui_animation.ToDesappear(sprite, () =>
                {
                    panel.PanelState = PanelState.Close;
                    tcs.SetResult(false);
                });

                return await tcs.Task;

            default:
                return false;
        }
    }
}
