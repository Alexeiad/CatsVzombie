using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ResultWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _foodText, _waterText, _materialsText, _diamondText;
    [SerializeField] private RewardDataSO _rewardData;
    [SerializeField] private Image _foodImage,_waterImage, _materialImage, _diamondImage;
    [Inject] private ResourceManager _resourceManager;
    private void OnEnable()
    {
        string x = "x";
        _foodImage.sprite =GetSprite(RewardType.Food);
        _waterImage.sprite = GetSprite(RewardType.Water);
        _materialImage.sprite = GetSprite(RewardType.Materials);
        _diamondImage.sprite = GetSprite(RewardType.Diamond);

        _foodText.text=x+ _resourceManager.LevelFood.ToString();
        _waterText.text =x+ _resourceManager.LevelWater.ToString();
        _materialsText.text =x+ _resourceManager.LevelMaterials.ToString();
        _diamondText.text = x + _resourceManager.LevelDiamond.ToString();
    }
    private Sprite GetSprite(RewardType type)
    {
        return _rewardData.RewardData
            .Where(r => r.RevardType == type)
            .Select(r => r.InBox ? _rewardData.Box : r.Sprite).FirstOrDefault();
    }
    
}


