using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectWindow : MonoBehaviour
{
    [SerializeField] private List<GameObject> _uiElementsActivate;
    [SerializeField] private List<GameObject> _uiElementsDisactive;
    [SerializeField] private BuildingsBuilder _buildingsBuilder;
    [SerializeField] private ArraySelector _arraySelector;
    public void Container(BuildingType buildingType)
    {
        _uiElementsActivate.ForEach(ui => ui.SetActive(true));
        _uiElementsDisactive.ForEach(ui => ui.SetActive(false));
        _arraySelector.SelectBuildingType(buildingType);
    }
    public void Delete()
    {
        _buildingsBuilder.DeleteBuilding();
    }
    public void LevelUp()
    {

    }
}
