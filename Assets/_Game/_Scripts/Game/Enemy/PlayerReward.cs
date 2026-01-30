using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Zenject;
using Zenject.ReflectionBaking.Mono.Cecil;

public class PlayerReward : MonoBehaviour
{
    public RewardDataSO rewardDataSO;
    public RewardType rewardType;
    public bool isCollecting;
    private SpriteRenderer _spriteRenderer;
    private RewardItem _rewardItem;

    

    private void Start()
    {
        
        float nothingChance = 0.7f;

        if (Random.Range(0f, 1f) < nothingChance)
        {
            Destroy(gameObject);
            return;
        }

        int randomIndex = Random.Range(0, rewardDataSO.RewardData.Count);

        _rewardItem = GetWeightedRandomReward(rewardDataSO.RewardData);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = new Color(1, 1, 1, 0);
        _spriteRenderer.DOFade(1, 0.1f);
        
       

        _spriteRenderer.sprite = _rewardItem.InBox ? rewardDataSO.Box : _rewardItem.Sprite;

        rewardType = _rewardItem.RevardType;

        transform.DOScale(0.7f, 0.5f)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);

    }




    private RewardItem GetWeightedRandomReward(List<RewardItem> rewardItems)
    {
        if (rewardItems == null || rewardItems.Count == 0)
            return null;

        // Проверяем, чтобы вероятности были неотрицательными
        var validItems = rewardItems.Where(item => item.Probability > 0).ToList();

        if (validItems.Count == 0)
            return rewardItems.First(); // Возвращаем первый если все вероятности 0

        // Используем кумулятивный метод
        float totalProbability = validItems.Sum(item => item.Probability);

        // Если сумма вероятностей слишком мала или равна 0
        if (totalProbability <= 0.0001f)
        {
            // Равномерное распределение
            int randomIndex = Random.Range(0, validItems.Count);
            return validItems[randomIndex];
        }

        float randomPoint = Random.Range(0f, totalProbability);
        float cumulative = 0f;

        foreach (var item in validItems)
        {
            cumulative += item.Probability;
            if (randomPoint <= cumulative)
                return item;
        }

        return validItems.Last();
    }
}
