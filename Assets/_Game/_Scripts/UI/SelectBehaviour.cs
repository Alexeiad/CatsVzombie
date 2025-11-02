
using UnityEngine;

public class SelectBehaviour : MonoBehaviour
{
    [SerializeField] private WindowBehaviour _windowBehaviour;
    [SerializeField] private BuildingType _buildingType;

    public void OnClickDown()
    {
        _windowBehaviour.ShowImage(_buildingType,BuildingLevelType.low,CallbackType.UI);
    }

}
