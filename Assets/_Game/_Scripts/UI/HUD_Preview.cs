using TMPro;
using UnityEngine;
using Zenject;

public class HUD_Preview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _food, _water, _materials, _diamonds, _catsCount;

    [Inject]
    private ResourceManager _resources;

    private void Update()
    {

        
        // Обновление UI с конкретными значениями
        _food.text = _resources.Food.ToString();
        _water.text = _resources.Water.ToString();
        _materials.text = _resources.Materials.ToString();
        _diamonds.text = _resources.Diamonds.ToString();
        _catsCount.text = _resources.CatsCount.ToString();
    }
}