using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class ClearSaveData : MonoBehaviour
{
    [Inject] private ResourceManager _resourceManager;
    [Inject] private BuildingsList _bases;
    [Inject] private BaseSaveManager _baseSaveManager;
    private bool _clearKeys;
    public void ClearFolder()
    {
        _resourceManager.ClearData();

        _bases = _baseSaveManager.GetBases();
        _bases.Clear();
        _clearKeys=true;
    }
    private void OnDisable()
    {
        if (_clearKeys)
            PlayerPrefs.DeleteKey("base");
        else if (SceneManager.GetActiveScene().buildIndex==1)
            PlayerPrefs.SetString("base", "base");
        
    }
}
