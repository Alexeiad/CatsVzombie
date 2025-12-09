
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CostBuildingType
{
    public BuildingType buildingType;
    public BuildingLevelType buildingLevelType;
    public int cost;
}


[CreateAssetMenu(fileName = "CostOfBuildingSO")]
public class CostOfBuildingSO : ScriptableObject 
{
    public List<CostBuildingType> costBuildings;
}
