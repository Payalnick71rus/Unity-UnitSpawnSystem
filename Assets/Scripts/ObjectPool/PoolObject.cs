using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Перечисление типов объектов
/// </summary>
public enum ObjectType
{
    MeleeUnit,
    RangeUnit,
    Bullet,
    DeathEffect
}
/// <summary>
/// Класс для указания, какой объект в пуле а какой нет, своего рода лейбл
/// </summary>
public class PoolObject : MonoBehaviour
{
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
    }

    public void ReturnToPoolDelayed(float time)
    {
        Invoke("ReturnToPool", time);
    }
}
