
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName= "RewardDataSO")]
public class RewardDataSO : ScriptableObject
{
    [field:SerializeField] public List<RewardItem> RewardData { get; set; }
    [field: SerializeField] public Sprite Box { get; set; }
}
[Serializable]
public class RewardItem
{
    [field: SerializeField] public RewardType RevardType { get; set; }
    [field: SerializeField] public Sprite Sprite { get; set; }
    [field: SerializeField] public bool InBox { get; set; }
    [field: SerializeField] public Ease Ease { get; set; } = Ease.OutElastic;

    [Range(0, 10)] [SerializeField] public int Probability;
}