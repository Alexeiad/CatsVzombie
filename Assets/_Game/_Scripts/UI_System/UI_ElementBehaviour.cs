using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(EventTrigger))]
public class UI_ElementBehaviour : MonoBehaviour
{
    public Action OnClick { get; set; }

    [field: SerializeField] public PauseState PauseState { get; set; }
    [field: SerializeField] public PanelState PanelState { get; set; }
    [field: SerializeField] public ElementType ElementType { get; private set; }
    
    [field: SerializeField] public ButtonType ButtonType { get; private set; }
    [field: SerializeField] public bool SetPause { get; set; }

    private EventTrigger _eventTrigger;

    private void Awake()
    {
        EventTrigger.Entry entry = new();


        _eventTrigger = GetComponent<EventTrigger>();
        
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) =>
        {
            OnPointerDownDelegate((PointerEventData)data);
        });

  
        _eventTrigger.triggers.Add(entry);

        
    }
    private void OnPointerDownDelegate(PointerEventData data)
    {
        OnClick?.Invoke();

    }

}
