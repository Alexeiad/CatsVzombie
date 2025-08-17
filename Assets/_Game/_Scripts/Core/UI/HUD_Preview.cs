using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class HUD_Preview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _food, _water, _materials;


    [Inject]
    private ResourceManager _resources;


    private void Update()
    {
        _food.text = "еда: " + _resources.Food.ToString();
        _water.text = "вода: " + _resources.Water.ToString();
        _materials.text = "материалы: " + _resources.Materials.ToString();
    }
}
