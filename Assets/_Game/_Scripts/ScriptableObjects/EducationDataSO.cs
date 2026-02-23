using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class EducationDataItem
{
    [TextArea]
    public string text;
    public float timeBefore;
    public Sprite sprite;
    public EducationTheme theme;
}



[CreateAssetMenu(fileName ="EducatonData")] 
public class EducationDataSO : ScriptableObject
{
   public List<EducationDataItem> data;
}
public enum EducationTheme 
{
    Controls,
    Shoot,
    Spawn,
    Clear,
    Building,
    BuildPoints,
    BuildButton,
    Food,
    Water,
    Production,
    Resources,
    Health,
    Zombie,
    ZombieCount
}

