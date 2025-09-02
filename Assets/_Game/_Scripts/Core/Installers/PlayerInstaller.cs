
using System.Collections.Generic;
using UnityEngine;

using Zenject;

public class PlayerInstaller : MonoInstaller
{

    [SerializeField] private GameObject _playerPrefab;

    [SerializeField] private DynamicJoystick _joystick;

    [Inject] private List<Transform> _entities;


    public override void InstallBindings()
    {
        _entities.Clear();
        //регистрация джойтика
        Container.Bind<DynamicJoystick>().FromInstance(_joystick).AsSingle();

        if (!IsMobilePlatform())
            _joystick.gameObject.SetActive(false);//если это пк


        //спавн и регистрация игрока
        var player = Container.InstantiatePrefab(_playerPrefab, Vector3.zero, Quaternion.identity, null);
        Container.Bind<PlayerMovement>().FromInstance(player.GetComponent<PlayerMovement>()).AsSingle();
        _entities.Add(player.transform);
        
    }


    private bool IsMobilePlatform()
    {

        return MobileChecker.IsMobileDevice;

    }
}