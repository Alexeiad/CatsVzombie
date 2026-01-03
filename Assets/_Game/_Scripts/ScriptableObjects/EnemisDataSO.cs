using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EnemiesData", menuName = "Game Data/Enemies Data")]
public class EnemiesDataSO : ScriptableObject
{
    [SerializeField] private EnemyTableRow[] _enemyRows = new EnemyTableRow[0];

    public EnemyTableRow[] EnemyRows => _enemyRows;

    [Serializable]
    public class EnemyTableRow
    {
        [SerializeField] private string _character = "Новый враг";
        [SerializeField] private Vector2 _movementSpeed = Vector2.zero;
        [SerializeField] private Vector2 _attackSpeed = Vector2.zero;
        [SerializeField] private int _meleeDamage = 0;
        [SerializeField] private Vector2 _shootSpeed = Vector2.zero;
        [SerializeField] private int _shootDamage = 0;
        [SerializeField] private int _health = 100;
        [SerializeField] private int _armor = 0;

        public string Character => _character;
        public Vector2 MovementSpeed => _movementSpeed;
        public Vector2 AttackSpeed => _attackSpeed;
        public int MeleeDamage => _meleeDamage;
        public Vector2 ShootSpeed => _shootSpeed;
        public int ShootDamage => _shootDamage;
        public int Health => _health;
        public int Armor => _armor;

        public void SetCharacter(string value) => _character = value;
        public void SetMovementSpeed(Vector2 value) => _movementSpeed = value;
        public void SetAttackSpeed(Vector2 value) => _attackSpeed = value;
        public void SetMeleeDamage(int value) => _meleeDamage = value;
        public void SetShootSpeed(Vector2 value) => _shootSpeed = value;
        public void SetShootDamage(int value) => _shootDamage = value;
        public void SetHealth(int value) => _health = value;
        public void SetArmor(int value) => _armor = value;
    }
}