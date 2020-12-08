using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Структура для начальных данных пула объектов
/// </summary>
[System.Serializable]
public struct PoolObjectData
{
    public GameObject prefab;
    public Transform parent;
    public ObjectType type;
}
/// <summary>
/// Менеджер пула объектов, предназначен для инициализации пула и работы с пулом
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager main = null;
    [Header("Параметры объектов в пуле")]
    [SerializeField] private List<PoolObjectData> poolsData;

    private List<ObjectPooling> pools = new List<ObjectPooling>();
    private void Awake()
    {
        if (main == null)
        {
            main = this;
            DontDestroyOnLoad(gameObject);
            // создаем список из пулов для каждого типа объектов
            for(int i=0;i< poolsData.Count;i++)
            {
                ObjectPooling pool = new ObjectPooling(poolsData[i].prefab, poolsData[i].parent, poolsData[i].type);
                pools.Add(pool);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        main = null;
    }
    /// <summary>
    /// Возрвращает true, если объект в пуле найден
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool GetObject(ObjectType type, out PoolObject obj)
    {
        obj = null;
        for (int i=0;i< pools.Count;i++)
        {
            if(pools[i].type == type)
            {
                obj = pools[i].GetObject();
                return true;
            }
        }
        return false;
    }
}
