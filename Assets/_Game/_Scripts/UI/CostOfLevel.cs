
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CostOfLevel : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI _waterText, _foodText, _materialsText;
    [SerializeField] private Image _waterImage, _foodImage, _materialsImage;

    [SerializeField] private RewardDataSO _rewardDataSO;
    [SerializeField] private SceneLoader _sceneLoader;

    [Inject] private ResourceManager _resourceManager;

    private int _spendWater, _spendFood, _spendMaterials;
    private int _nextSceneIndex;

    public void AddSpendResources(int water,int food,int materials)
    {

        _spendWater = -water;
        _spendFood = -food;
        _spendMaterials = -materials;

        _waterText.text = water > 0 ? _spendWater.ToString() : "";
        _foodText.text = food > 0 ? _spendFood.ToString() : "";
        _materialsText.text = materials > 0 ? _spendMaterials.ToString() : "";

        _waterImage.sprite =  SelectSprite(RewardType.Water);
        _foodImage.sprite = SelectSprite(RewardType.Food);
        _materialsImage.sprite = SelectSprite(RewardType.Materials);

        _waterImage.gameObject.SetActive(water > 0);
        _foodImage.gameObject.SetActive(food > 0);
        _materialsImage.gameObject.SetActive(materials > 0);

    }
    public void SelectSceneIndex(int index)
    {
        _nextSceneIndex= index;
    }
    private Sprite SelectSprite(RewardType rewardType)
    {
        return _rewardDataSO.RewardData
            .Where(r => r.RevardType == rewardType)
            .Select(r => r.Sprite).FirstOrDefault();
    }
    public void SpendResources()
    {
        _resourceManager.AddWater(_spendWater);
        _resourceManager.AddFood(_spendFood);
        _resourceManager.AddMaterials(_spendMaterials);
        _resourceManager.SaveToJson();
        _sceneLoader.LoadScene(_nextSceneIndex);
    }

}
