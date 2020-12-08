using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Класс, организующий пулл объектов конкретного типа
/// </summary>
public class ObjectPooling
{
    public ObjectType type { get; private set; }
    private List<PoolObject> objects = new List<PoolObject>();
    private Transform objectsParent = null;
    private GameObject template = null;
    /// <summary>
    ///  Конструктор класса, входные параметры: префаб, родитель и тип объекта
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="parent"></param>
    /// <param name="poolType"></param>
    public ObjectPooling(GameObject temp, Transform parent, ObjectType poolType)
    {
        type = poolType;
        objectsParent = parent;
        template = temp;
    }
    /// <summary>
    /// Добавляет объект в пулл
    /// </summary>
    private void AddObject()
    {
        GameObject temp = GameObject.Instantiate(template) as GameObject;
        temp.transform.parent = objectsParent;
        objects.Add(temp.GetComponent<PoolObject>());
        temp.SetActive(false);
    }
    /// <summary>
    /// Возвращает объект из пула
    /// </summary>
    /// <returns></returns>
    public PoolObject GetObject()
    {        
        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i].gameObject.activeSelf)
            {
                objects[i].gameObject.SetActive(true);
                return objects[i];
            }
        }
        AddObject();
        objects[objects.Count - 1].gameObject.SetActive(true);
        return objects[objects.Count-1];
    }
}
