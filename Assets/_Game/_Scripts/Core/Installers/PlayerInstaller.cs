
using UnityEngine;

using Zenject;

public class PlayerInstaller : MonoInstaller
{

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _canvasesPrefab;

    private GameObject _joystick;


    public override void InstallBindings()
    {
        //спавн канвас
        var canvas = Container.InstantiatePrefab(_canvasesPrefab, Vector3.zero, Quaternion.identity, null);

        //джойтик в этом канвасе
        _joystick = canvas.transform.GetChild(4).GetChild(0).gameObject;

        //регистрация джойтика
        Container.Bind<DynamicJoystick>().FromInstance(_joystick.GetComponent<DynamicJoystick>()).AsSingle();

        if (!IsMobilePlatform())
            _joystick.SetActive(false);//если это пк


        //спавн и регистрация игрока
        var player = Container.InstantiatePrefab(_playerPrefab, Vector3.zero, Quaternion.identity, null);
        Container.Bind<PlayerBehaviour>().FromInstance(player.GetComponent<PlayerBehaviour>()).AsSingle();
    }


    private bool IsMobilePlatform()
    {

        return MobileChecker.IsMobileDevice;

    }
}