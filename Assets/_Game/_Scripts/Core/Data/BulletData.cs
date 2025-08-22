using UnityEngine;

[System.Serializable]
public struct BulletData
{
    public float Speed;
    public int Damage;
    public float LifeTime;
    public Vector2 Direction;
    public LayerMask DamageLayers;
}