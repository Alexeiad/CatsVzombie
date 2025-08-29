using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class Gun : MonoBehaviour
{
    [SerializeField] private Bullet _bulletPrefab;

    [Inject] private List<Transform> _entities;

    private bool _isEnemy;


    public void Container(bool isEnemy)
    {
        _isEnemy=isEnemy;
    }


    private void Start()
    {
        Debug.Log("entity: " + _entities.Count);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)&&_isEnemy==false)
        {
            Transform closestEntity = FindClosestEntity();
            if (closestEntity != null)
            {
                Shoot(closestEntity);
            }
        }
    }

    private Transform FindClosestEntity()
    {
        if (_entities == null || _entities.Count == 0)
            return null;

        Transform closestEntity = null;
        float closestDistance = Mathf.Infinity;
        Vector3 gunPosition = transform.position;
        const float maxSearchDistance = 10f; // Максимальное расстояние поиска

        foreach (Transform entity in _entities)
        {
            // Пропускаем самого себя и родительский объект
            if (entity == transform || entity == transform.parent)
                continue;

            // Пропускаем уничтоженные объекты
            if (entity == null)
                continue;

            float distance = Vector3.Distance(gunPosition, entity.position);

            // Пропускаем объекты дальше 10 метров
            if (distance > maxSearchDistance)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEntity = entity;
            }
        }

        return closestEntity;
    }

    public void Shoot(Transform entity)
    {

        var bullet = Instantiate(_bulletPrefab.gameObject, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().MoveToContainer(entity);
    }
}