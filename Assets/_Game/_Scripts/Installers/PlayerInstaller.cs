
using System;
using System.Collections.Generic;
using UnityEngine;

using Zenject;

public class PlayerInstaller : MonoInstaller
{

    [SerializeField] private GameObject _playerPrefab;

    [SerializeField] private DynamicJoystick _joystick;
    [SerializeField] private Transform _spawnPoint;

    [Inject] private EntityList _entities;
    

    public override void InstallBindings()
    {


        _entities.Clear();

        //регистрация джойтика
        Container.Bind<DynamicJoystick>()
            .FromInstance(_joystick)
            .AsSingle();

        

        if (!IsMobilePlatform())
            _joystick.gameObject.SetActive(false);//если это пк


        //спавн и регистрация игрока
        var player = Container.InstantiatePrefab(_playerPrefab, _spawnPoint!=null ? _spawnPoint.position:
            Vector3.zero, Quaternion.identity, null);

        Container.Bind<PlayerMovement>().FromInstance(player.GetComponent<PlayerMovement>()).AsSingle().NonLazy();

        //Container.Bind<PlayerMovement>().AsSingle().NonLazy();

        _entities.Add(player.transform);

        

    }
    
    private bool IsMobilePlatform()
    {

        return MobileChecker.IsMobileDevice;

    }
}

