using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonForConstruction : MonoBehaviour
{
    [SerializeField] private GameObject _selectWindow;
    

    public BuildingType buildingType;
    public BuildingLevelType buildingLevelType;

    public void ActivateWindow()
    {
        _selectWindow.SetActive(true);

        var buttonForConstruction = _selectWindow.GetComponentInChildren<ButtonForConstruction>();

        buttonForConstruction.buildingType = buildingType;
    }
}
