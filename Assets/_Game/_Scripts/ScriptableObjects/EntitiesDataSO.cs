using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EnemiesData", menuName = "Game Data/Enemies Data")]
public class EntitiesDataSO : ScriptableObject
{
    [field: SerializeField] public EnemyTableRow[] EnemyRows { get; private set; }

    [Serializable]
    public class EnemyTableRow: IEntity
    {
        
        [field: SerializeField] public string CharacterName { get; set; }
        [field: SerializeField] public GameObject EntityPrefab { get; set; }
        [field: SerializeField] public EntityType EntityType { get; set; }
        [field: SerializeField] public Vector2 MovementSpeed { get; set; } 
        [field: SerializeField] public Vector2 AttackSpeed { get; set; }
        [field: SerializeField] public Vector2 ShootSpeed { get; set; }
        [field: SerializeField] public int ShootDamage { get; set; }
        [field: SerializeField] public int MeleeDamage { get; set; }
        [field: SerializeField] public int Health { get; set; }
        [field: SerializeField] public float DetectionRadius { get; set; } = 6f;
        [field: SerializeField] public float AttackRange { get; set; } = 2f;
        [field: SerializeField] public float ShootRange { get; set; } = 5f;
        [field: SerializeField][field: Range(0, 1)] public float Armor { get; set; }
    }
}