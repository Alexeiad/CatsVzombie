using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class Gun : MonoBehaviour
{
    

    [SerializeField] private Bullet _bulletPrefab;

    [Inject] private List<Transform> _entities;

    private EntityType _entityType = EntityType.Player;


    public void Container(EntityType entityType)
    {
        _entityType = entityType;
    }
    private void OnEnable()
    {
        StatsPreview.OnClick += PrepareShoot;
    }
    private void OnDisable()
    {
        StatsPreview.OnClick -= PrepareShoot;
    }

    private void Start()
    {
        Debug.Log("entity: " + _entities.Count);
    }

    private void PrepareShoot()
    {
        if (_entityType.Equals(EntityType.Player))
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
