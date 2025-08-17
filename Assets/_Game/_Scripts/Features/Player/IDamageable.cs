
using UnityEngine;

public interface IDamageable 
{
    // source – объект, наносящий урон (игрок, враг и т.д.)
    // damage – базовое значение урона исходя из источника
    // можно добавить DamageType, если нужно (физический, магический), но пока не обязательно

    void TakeDamage(GameObject source, float damage);

    float CurrentHealth { get; }
    bool IsDead { get; }
}
