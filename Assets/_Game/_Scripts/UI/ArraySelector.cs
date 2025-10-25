using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ArraySelector : MonoBehaviour
{
    [SerializeField] private ButtonForConstruction _buttonForConstruction;
    [SerializeField] private List<Sprite> _images;
    [SerializeField] private Image _image;

    private void Update()
    {
        int buildingTypeIndex = (int)_buttonForConstruction.buildingType;

        _image.sprite = _images[buildingTypeIndex];
    }
    public void SelectNext()
    {
        ChangeSelection(1);
    }

    public void SelectPrevious()
    {
        ChangeSelection(-1);
    }

    private void ChangeSelection(int direction)
    {
        if (_buttonForConstruction == null) return;

        var currentType = _buttonForConstruction.buildingLevelType;
        var allTypes = System.Enum.GetValues(typeof(BuildingLevelType));
        int currentIndex = (int)currentType;

        int newIndex = (currentIndex + direction + allTypes.Length) % allTypes.Length;
        _buttonForConstruction.buildingLevelType = (BuildingLevelType)newIndex;

        float targetScale = 1 + newIndex * 0.25f;

        _image.transform.DOScale(new Vector3(targetScale, targetScale, 1), 0.3f);
    }
}