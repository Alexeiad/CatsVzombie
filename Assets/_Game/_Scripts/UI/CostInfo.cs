
using UnityEngine;

public class CostInfo : MonoBehaviour
{
    [Header("CostOfLevel")]
    [SerializeField] private int _water, _food, _materials;
    [SerializeField] private CostOfLevel _costOfLevel;

    public void SelectThisLevel()
    {
        _costOfLevel.AddSpendResources(_water,_food,_materials);
    }
}
