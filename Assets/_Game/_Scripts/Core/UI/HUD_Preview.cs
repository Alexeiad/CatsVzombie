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

        if (Input.GetMouseButtonDown(0))
        {
            _resources.AddFood(10);
            _resources.SaveToJson();

        }
        if (Input.GetMouseButtonDown(1))
        {
            _resources.AddFood(-10);
            _resources.SaveToJson();

        }
        // Обновление UI с конкретными значениями
        _food.text = _resources.Food.ToString();
        _water.text = _resources.Water.ToString();
        _materials.text = _resources.Materials.ToString();
        _diamonds.text = _resources.Diamonds.ToString();
        _catsCount.text = _resources.CatsCount.ToString();
    }
}