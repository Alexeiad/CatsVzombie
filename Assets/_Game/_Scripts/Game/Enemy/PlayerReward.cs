using DG.Tweening;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerReward : MonoBehaviour
{
    public RewardDataSO rewardDataSO;
    public RewardType rewardType;
    public bool isCollecting;
    private SpriteRenderer _spriteRenderer;
    private RewardItem _rewardItem;

    

    private void Start()
    {
        
        

        _rewardItem = GetWeightedRandomReward(rewardDataSO.RewardData);
        if (_rewardItem == null)
        {
            Destroy(gameObject);
            return;
        }

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
        List<RewardItem> rewards = new List<RewardItem>();

        foreach (var rewardItem in rewardItems)
        {
            // Если Probability от 1 до 10, то 10 - Probability дает обратную вероятность
            // Например: Probability=1 -> 10-1=9 -> шанс 1 из 9
            // Probability=10 -> 10-10=0 -> шанс 1 из 0 (всегда)
            int range = 10 - rewardItem.Probability;

            // Защита от деления на 0
            if (range == 0)
            {
                rewards.Add(rewardItem);
            }
            else if (Random.Range(0, range) == 0)
            {
                rewards.Add(rewardItem);
            }
        }

        if (rewards.Count > 0)
        {
            if (rewards.Count == 1)
            {
                return rewards[0];
            }
            else
            {
                
                return rewards.OrderByDescending(r => r.Probability).FirstOrDefault();
            }
        }

        return null;
    }
}
