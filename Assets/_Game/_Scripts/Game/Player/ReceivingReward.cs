using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;



public class ReceivingReward : MonoBehaviour
{
    [Inject] private ResourceManager _resourceManager;
    private List<PlayerReward> _rewardList;

    private void Start()
    {
        StartCoroutine(CheckRewards());
    }
    private void Update()
    {
        // Пройдёмся по всем rewards
        for (int i = _rewardList.Count - 1; i >= 0; i--)
        {
            var r = _rewardList[i];

            if (r == null) // уничтоженный объект
            {
                _rewardList.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(r.transform.position, transform.position);

            if (dist < 2f && !r.isCollecting)
            {
                // Захватываем данные заранее
                RewardType type = r.rewardType;
                Transform rewardTransform = r.transform;
                GameObject rewardGO = r.gameObject;

                // Помечаем, что уже собираем
                r.isCollecting = true;

                // Анимация

                float duration = 0.2f;

                rewardTransform.GetComponent<SpriteRenderer>().DOFade(0, duration);

                rewardTransform.DOMove(transform.position, duration)
                    .OnComplete(() =>
                    {
                        GetReward(type);
                        Destroy(rewardGO);
                    });
            }
        }
    }
    private void GetReward(RewardType rewardType)
    {
        

        switch (rewardType)
        { 
            case RewardType.Water:_resourceManager.AddWater(1);
                _resourceManager.LevelWater++;
                break;
            case RewardType.Food: _resourceManager.AddFood(1);
                _resourceManager.LevelFood++;
                break;
            case RewardType.Materials: _resourceManager.AddMaterials(1);
                _resourceManager.LevelMaterials++;
                break;
            case RewardType.Diamond: _resourceManager.AddDiamonds(1);
                _resourceManager.LevelDiamond++;
                break;

                
        }

    }
    private IEnumerator CheckRewards()
    {
        while (true)
        {
            _rewardList = FindObjectsByType<PlayerReward>(FindObjectsSortMode.None).ToList();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
