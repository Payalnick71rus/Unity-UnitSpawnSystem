using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Класс описывает движение пули и ее взаимодействие с целью
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Расстояние до цели для взаимодействия")]
    [SerializeField] private float destroyDistance = 0.1f;
    [Header("Скорость пули")]
    [SerializeField] private float bulletSpeed = 10f;
    [Header("Рендерер меша пули")]
    [SerializeField] private MeshRenderer bodyRenderer;
    [Header("Максимальная дистанция полета пули")]
    [SerializeField] private float maxTraveledDistance = 50f;

    private float bulletDamage;
    private Unit enemy=null;
    private Transform enemyTransform = null;
    private bool canMove = false;
    
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
    }
    private void CheckBulletPosition()
    {
        if (enemyTransform)
        {
            if (enemyTransform.gameObject.activeSelf)
            {
                if (Vector3.Distance(transform.position, enemyTransform.position) <= destroyDistance)
                {
                    enemy.ProcessDamage(bulletDamage);
                    gameObject.SetActive(false);
                    canMove = false;
                }
            }

            if (Vector3.Distance(transform.position, startPos) > maxTraveledDistance)
            {
                gameObject.SetActive(false);
                canMove = false;
            }
        }
        else
        {
            gameObject.SetActive(false);
            canMove = false;
        }
    }
    
    void Update()
    {
        if(canMove)
        {
            transform.position += transform.right * bulletSpeed * Time.deltaTime;
            CheckBulletPosition();
        }
    }
   /// <summary>
   /// Задает материал пули
   /// </summary>
   /// <param name="mat"></param>
    public void SetMaterial(Material mat)
    {
        if(bodyRenderer) bodyRenderer.material = mat;
    }
    /// <summary>
    /// Указывает юнита-цель
    /// </summary>
    /// <param name="unit"></param>
    public void SetEnemy(Unit unit)
    {        
        enemy = unit;
        enemyTransform = unit.gameObject.transform;
    }
    /// <summary>
    /// Задает урон пули
    /// </summary>
    /// <param name="value"></param>
    public void SetDamageValue(float value)
    {
        bulletDamage = value;
    }
    /// <summary>
    /// Разрешает движение пули в пространстве
    /// </summary>
    public void StartMoving()
    {
        startPos = transform.position;
        canMove = true;
    }
}
