
using System;
using UnityEngine;
using Zenject;

public class LosingWindow : MonoBehaviour
{
    [SerializeField] private GameObject _losingWindow;

    [Inject] private PlayerMovement _playerMovement;
    [Inject] private ResourceManager _resourceManager;


    private void OnEnable()
    {
        _playerMovement.OnDie += ResetLevelResources;
    }
    private void OnDisable()
    {
        _playerMovement.OnDie -= ResetLevelResources;
    }

    private void ResetLevelResources()
    {
        _losingWindow.SetActive(true);

        _resourceManager.AddWater(-_resourceManager.LevelWater);
        _resourceManager.AddFood(-_resourceManager.LevelFood);
        _resourceManager.AddMaterials(-_resourceManager.LevelMaterials);
        _resourceManager.AddDiamonds(-_resourceManager.LevelDiamond);

        Time.timeScale=0;
    }
}
