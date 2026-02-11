
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class StatsPreview : MonoBehaviour
{
    public static Action OnClick;
    public List<Enemy> enemys;

    [SerializeField] private StatsBar _playerStats;
    [SerializeField] private StatsBar _kentStats;
    [SerializeField] private StatsBar _enemyStats;
    [SerializeField] private GameObject _enemyStatsBar;
    [SerializeField] private TextMeshProUGUI _enemyText,_playerText;
    [SerializeField] private TextMeshProUGUI _enemyHealthCount,_playerHealthCount;

    [Inject] private EntityList _entities;

    private PlayerMovement _playerMovement;
    private bool isStart;

    private float _enemyDistance = 15f;



    private float _playerHealth;

    [Inject]
    private void Construct(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }
    private void Start()
    {
        _enemyStatsBar.SetActive(false);
        StartCoroutine(Init());

    }
    private IEnumerator Init()
    {
        yield return new WaitForSeconds(1);
        
        enemys = _entities
        .Select(entity => entity.GetComponent<Enemy>())
        .Where(enemyAI => enemyAI != null)
        .ToList();

        _playerStats.sliderHealth.maxValue = _playerMovement.CurrentHealth.Value;

        isStart=true;
    }

    private void Update()
    {
        if (!isStart) return;

        int maxHealth = _playerMovement.MaxHealth;
        int health = _playerMovement.CurrentHealth.Value;

        _playerStats.sliderHealth.maxValue = maxHealth;
        _playerStats.sliderHealth.value = health;

        _playerHealthCount.text = health.ToString() + "/" +
            maxHealth.ToString();


        if (enemys == null || enemys.Count == 0)
        {
            _enemyStatsBar.SetActive(false);
            return;
        }

        var validEnemies = enemys
            .Where(enemy => enemy != null && enemy.gameObject != null)
            .ToList();

        if (validEnemies.Count == 0)
        {
            _enemyStatsBar.SetActive(false);
            return;
        }

        Enemy closestEnemy = enemys.Where(enemy=>enemy!=null)
            .Select(enemy => enemy.GetComponent<Enemy>())
            .OfType<Enemy>()
            .Where(enemyAI => Vector3.Distance(enemyAI.transform.position, _playerMovement.transform.position) < _enemyDistance)
            .OrderBy(enemyAI => Vector3.Distance(enemyAI.transform.position, _playerMovement.transform.position))
            .FirstOrDefault();
        if (closestEnemy != null && closestEnemy.gameObject != null)
        {
            _enemyStatsBar.SetActive(true);
            _enemyStats.sliderHealth.maxValue = closestEnemy.MaxHealth;
            _enemyStats.sliderHealth.value = closestEnemy.Health;
            _enemyText.text = closestEnemy.CharacterName;

            _enemyHealthCount.text=closestEnemy.Health.ToString()+"/"+
                closestEnemy.MaxHealth.ToString();

        }
        else
        {
            _enemyStatsBar.SetActive(false);
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


