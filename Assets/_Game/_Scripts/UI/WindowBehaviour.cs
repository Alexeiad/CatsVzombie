using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class WindowBehaviour : MonoBehaviour
{
    public BuildingType buildingType { get; set; }
    public BuildingLevelType buildingLevelType { get; set; }

    [SerializeField] private BuildingsBuilder _buildingBuilder;
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _window, _elementWindow;

    public void ShowImage(BuildingType buildingType,BuildingLevelType buildingLevelType,CallbackType callback)
    {
        this.buildingType = buildingType;

        _image.sprite = _sprites[(int)buildingType];
        _image.transform.localScale = Vector3.one;
        _window.SetActive(true);

        if (callback == CallbackType.UI)
        {
            _elementWindow.SetActive(false);
        }
        if (callback == CallbackType.Base)
        {
            _elementWindow.SetActive(true);
        }
        _buildingBuilder.SelectType(buildingType, buildingLevelType);
    }

    public void ArrayRight()
    {
        MoveToNextType(1);
    }

    public void ArrayLeft()
    {
        MoveToNextType(-1);
    }

    private void MoveToNextType(int direction)
    {
        int typeCount = Enum.GetValues(typeof(BuildingLevelType)).Length;
        int currentIndex = (int)buildingLevelType;

        int newIndex = (currentIndex + direction + typeCount) % typeCount;

        BuildingLevelType newType = (BuildingLevelType)newIndex;

   
        //_image.sprite = _sprites[newIndex];
        _image.transform.localScale = new Vector2(1+newIndex * 0.2f,1);

        _buildingBuilder.SelectType(buildingType, newType);

        buildingLevelType = newType;
    }

}
public enum CallbackType 
{
    UI,
    Base
}

