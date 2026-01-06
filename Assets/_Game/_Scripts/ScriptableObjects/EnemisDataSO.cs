using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EnemiesData", menuName = "Game Data/Enemies Data")]
public class EnemiesDataSO : ScriptableObject
{
    [field: SerializeField] public EnemyTableRow[] EnemyRows { get; private set; }

    [Serializable]
    public class EnemyTableRow
    {
        
        [field: SerializeField] public string Character { get; set; }
        [field: SerializeField] public Vector2 MovementSpeed { get; set; } 
        [field: SerializeField] public Vector2 AttackSpeed { get; set; }
        [field: SerializeField] public Vector2 ShootSpeed { get; set; }
        [field: SerializeField] public int ShootDamage { get; set; }
        [field: SerializeField] public int MeleeDamage { get; set; }
        [field: SerializeField] public int Health { get; set; }
        [field: SerializeField][field: Range(0, 1)] public float Armor { get; set; }
    }
}