using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

public class ClearSaveData : MonoBehaviour
{
    [Inject] private ResourceManager _resourceManager;
    [Inject] private BuildingsList _buildingsList;
    public void ClearFolder()
    {
        _resourceManager.ClearData();
        _buildingsList.Clear();
    }
    
}
