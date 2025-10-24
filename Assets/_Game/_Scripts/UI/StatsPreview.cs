
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class StatsPreview : MonoBehaviour
{
    public static Action OnClick;

    [SerializeField] private StatsBar _playerStats;
    [SerializeField] private StatsBar _kentStats;
    [SerializeField] private StatsBar _enemyStats;

    [Inject] private EntityList _entities;

    private PlayerMovement _playerMovement;

    private List<EnemyAI> _enemys;

    private float _playerHealth;

    [Inject]
    private void Construct(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }
    private void Start()
    {
         _enemys = _entities
        .Select(entity => entity.GetComponent<EnemyAI>())
        .Where(enemyAI => enemyAI != null)
        .ToList();
        _enemyStats.sliderHealth.gameObject.SetActive(false);
    }

    private void Update()
    {
        _playerStats.sliderHealth.value = _playerMovement.CurrentHealth.Value * 0.01f;

        if (_enemys == null || _enemys.Count == 0)
        {
            _enemyStats.sliderHealth.gameObject.SetActive(false);
            return;
        }

        var validEnemies = _enemys
            .Where(enemy => enemy != null && enemy.gameObject != null)
            .ToList();

        if (validEnemies.Count == 0)
        {
            _enemyStats.sliderHealth.gameObject.SetActive(false);
            return;
        }

        EnemyAI closestEnemy = validEnemies
            .Select(enemy => enemy.GetComponent<EnemyAI>())
            .OfType<EnemyAI>()
            .Where(enemyAI => Vector3.Distance(enemyAI.transform.position, _playerMovement.transform.position) <= 10f)
            .OrderBy(enemyAI => Vector3.Distance(enemyAI.transform.position, _playerMovement.transform.position))
            .FirstOrDefault();
        if (closestEnemy != null && closestEnemy.gameObject != null)
        {
            _enemyStats.sliderHealth.gameObject.SetActive(true);
            _enemyStats.sliderHealth.value =closestEnemy.currentHealth.Value * 0.01f;
        }
        else
        {
            _enemyStats.sliderHealth.gameObject.SetActive(false);
        }

        
    }

    public void PanelClick()
    {
        OnClick?.Invoke();
    }




    [System.Serializable]
    private class StatsBar
    {
        public Slider sliderArmor,sliderHealth;

    }

}


