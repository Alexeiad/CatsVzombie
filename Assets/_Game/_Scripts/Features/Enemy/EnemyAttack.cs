using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

    [SerializeField] private Gun _gun;
    [SerializeField] private float _attackCooldown = 1f;


    private Transform _player;
    private float _nextAttackTime;
    private bool _shoot;

    public void SetShoot(Transform player, bool shoot)
    {
        _player = player;
        _shoot = shoot;
        _gun.Container(true);
    }

    private void Update()
    {
        if (_shoot && Time.time >= _nextAttackTime)
        {
            Shoot();
            _nextAttackTime = Time.time + _attackCooldown;
        }
    }

    private void Shoot()
    {
        _gun.Shoot(_player);
    }
}