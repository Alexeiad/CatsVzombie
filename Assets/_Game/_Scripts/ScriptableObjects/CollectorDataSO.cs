using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResoursesToGive 
{
    public BuildingType buildingType;
    public BuildingLevelType buildingLevelType;

    public int materialsToGive;
    public int waterToGive;
    public int foodToGive;
    public int diamondToGive;
}

[CreateAssetMenu(fileName = "CollecorDataSO")]
public class CollectorDataSO : ScriptableObject
{
    public List<ResoursesToGive> resoursesToGives;

}
