using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Класс юнита дальнего боя
/// </summary>
public class RangeUnit : Unit
{
    [Header("Префаб пули")]
    [SerializeField] private GameObject bulletPrefab;
    [Header("Трансформы оружия")]
    [SerializeField] private List<Transform> weaponTransforms;
    
    protected override void CheckAtackDistanceToTarget()
    {
        if (enemyTarget != null)
        {

            float distance = levelPresenter.CalcDistanceBetweenCells(centerPos.gridPosition, enemyTarget.GridCell.gridPosition);
            if (distance <= UnitData.atackRange)
            {
                rotatedToWayPoint = RotateToPoint(enemyTarget.GridCell);
                SnapToGridByCurrentCell();
                unitState = UnitState.Attacking;

                AtackTarget();
            }
            else
            {                
                FindWayTotarget();
            }
        }
        else
        {
            unitState = UnitState.SearchingEnemy;
        }
    }
    protected override void AtackTarget()
    {       
        canAtack = true;
        StartCoroutine(AtackLoop());
    }
    /// <summary>
    /// Цикл атаки
    /// </summary>
    /// <returns></returns>
    private IEnumerator AtackLoop()
    {
        
        while (canAtack && (unitState == UnitState.Attacking))
        {            
            yield return new WaitForSeconds(1f/UnitData.atackSpeed);
            
            if (enemyTarget == null)
            {                
                canAtack = false;                
                unitState = UnitState.SearchingEnemy;
            }
            else
            {                
                for (int i = 0; i < weaponTransforms.Count; i++)
                {
                    CreateBullet(weaponTransforms[i]);
                }
                rotatedToWayPoint = RotateToPoint(enemyTarget.GridCell);
            }
        }
    }
    /// <summary>
    /// Создание пули
    /// </summary>
    /// <param name="weaponTransform"></param>
    private void CreateBullet(Transform weaponTransform)
    {
        if (bulletPrefab)
        {
            PoolObject poolObject = null;
            PoolManager.main.GetObject(ObjectType.Bullet, out poolObject);
            GameObject bullet = poolObject.gameObject;
            bullet.SetActive(true);
            bullet.transform.position = weaponTransform.position;
            bullet.transform.rotation = transform.rotation;
            Bullet script = bullet.GetComponent<Bullet>();
            switch (UnitData.side)
            {
                case UnitSide.None:
                    script.SetMaterial(neutralMaterial);
                    break;
                case UnitSide.Blue:
                    script.SetMaterial(blueTeamMaterial);
                    break;
                case UnitSide.Red:
                    script.SetMaterial(redTeamMaterial);
                    break;
            }
            script.SetDamageValue(UnitData.atackDamage / weaponTransforms.Count);
            script.SetEnemy(enemyTarget);
            bullet.transform.forward = weaponTransform.forward;
            script.StartMoving();
            if (unitAnimator)
            {
                unitAnimator.Play(UnitData.attackAnimationName);
            }
            
        }
    }    
}
