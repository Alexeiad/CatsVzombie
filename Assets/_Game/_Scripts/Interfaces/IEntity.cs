using UnityEngine;

public interface IEntity 
{
    public int ID { get; set; }
    public string CharacterName { get; set; }
    public EntityType EntityType { get; set; }
    public Vector2 MovementSpeed { get; set; }
    public Vector2 AttackSpeed { get; set; }
    public Vector2 ShootSpeed { get; set; }
    public int ShootDamage { get; set; }
    public int MeleeDamage { get; set; }
    public int Health { get; set; }
    public float DetectionRadius { get; set; }
    public float AttackRange { get; set; }
    public float ShootRange { get; set; }
    public float Armor { get; set; }
}
