using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Класс юнита ближнего боя
/// </summary>
public class MeleeUnit : Unit
{      
    protected override void CheckAtackDistanceToTarget()
    {
        if (enemyTarget != null)
        {
            if(wayPoints.Count==1)
            {
                SnapToGridByCurrentCell();
                RotateToPoint(enemyTarget.GridCell);                
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
    protected override void OnWayFound()
    {
        
        if(wayPoints.Count>=1)
        {
            wayPoints.RemoveAt(wayPoints.Count - 1);
            if (wayPoints.Count == 1)
            {
                SnapToGridByCurrentCell();
                RotateToPoint(enemyTarget.GridCell);                
                unitState = UnitState.Attacking;

                AtackTarget();
            }
            else unitState = UnitState.MovingToEnemyForAtack;
        }
        else unitState = UnitState.Attacking;
        

    }
    protected override void AtackTarget()
    {
        canAtack = true;
        StartCoroutine(AtackLoop());
    }
    private IEnumerator AtackLoop()
    {

        while (canAtack && (unitState == UnitState.Attacking))
        {
            yield return new WaitForSeconds(1f / UnitData.atackSpeed);

            if (enemyTarget == null)
            {
                canAtack = false;
                unitState = UnitState.SearchingEnemy;
            }
            else
            {
                AttackEnemy();
                if (levelPresenter.CalcDistanceBetweenCells(centerPos.gridPosition,enemyTarget.GridCell.gridPosition)> UnitData.atackRange)
                {
                    unitState = UnitState.MovingToEnemyForAtack;
                }
            }
        }
    }
   
    private void AttackEnemy()
    {
        if (enemyTarget != null)
        {
            if (unitAnimator)
            {
                unitAnimator.Play(UnitData.attackAnimationName);
            }
            
            enemyTarget.ProcessDamage(UnitData.atackDamage);
        }
    }
}
