using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonForConstruction : MonoBehaviour
{
    [SerializeField] private GameObject _selectWindow;
    [SerializeField] private List<GameObject> _uiElementsActivate;
    [SerializeField] private List<GameObject> _uiElementsDisactive;
    [SerializeField] private ArraySelector _arraySelector;

    public BuildingType buildingType;
    public BuildingLevelType buildingLevelType;

    public void ActivateWindow()
    {
        _selectWindow.SetActive(true);
        _uiElementsActivate.ForEach(ui => ui.SetActive(true));
        _uiElementsDisactive.ForEach(ui => ui.SetActive(false));

        var buttonForConstruction = _selectWindow.GetComponentInChildren<ButtonForConstruction>();

        buttonForConstruction.buildingType = buildingType;

        if(_arraySelector != null)
        {
            _arraySelector.SelectBuildingType(buildingType);
        }
    }
}
