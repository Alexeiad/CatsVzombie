using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMyScriptableObject", menuName = "My Scriptable Objects/MyScriptableObject", order = 1)]
public class InfoDataSO : ScriptableObject
{
    [TextArea]
    public List<string> buidingInfo;
}
