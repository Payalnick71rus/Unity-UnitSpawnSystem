using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EpPathFinding.cs;
using System;
/// <summary>
/// Перечисление - тип юнита
/// </summary>
public enum UnitType
{
    None,
    Melee,
    Range
}
/// <summary>
/// Сторона юнита
/// </summary>
public enum UnitSide
{
    None,
    Red,
    Blue
}
/// <summary>
/// Состояние юнита
/// </summary>
public enum UnitState
{
    None,
    MovingToPoint,          // двигается на точку
    MovingToEnemyForAtack,  // двигается к врагу для атаки
    Attacking,              // атакует
    SearchingEnemy          // ищет врага
}

/// <summary>
/// Структура начальных данных юнита
/// </summary>
[System.Serializable]
public struct UnitParameters
{
    public float walkHeight;        
    public float maxHealth;
    public bool canAtack;
    public float atackDamage;
    public float atackSpeed;
    public float atackRange;
    public float viewDistance;
    public float moveSpeed;
    public float rotationSpeed;
    public string attackAnimationName;
    public UnitSide side;
}


/// <summary>
/// Базовый класс для описания любого юнита на карте(дальний, ближний, здание)
/// </summary>
public class Unit : MonoBehaviour, IGridSnap, IDamageble
{

    #region EditorValues   
    
    [SerializeField]
    protected Image healthBarImage;
    [SerializeField]
    protected Animator unitAnimator;
    [SerializeField]
    protected int widthInCells, heightInCells;
    [SerializeField]
    protected Material redTeamMaterial, blueTeamMaterial, neutralMaterial;
    [SerializeField]
    protected List<MeshRenderer> renderers = new List<MeshRenderer>();
    [SerializeField]
    protected float rangeToChangeMovePoint = 0.05f;
    #endregion

    #region private values
    protected UnitParameters UnitData;
    protected GridCellContainer centerPos;
    protected LevelPresenter levelPresenter = null;
    protected UnitManager unitManager = null;
    protected UnitSide enemyTeam;
    protected UnitState unitState= UnitState.None;
    protected Unit enemyTarget = null;    
    protected List<GridPos> wayPoints = new List<GridPos>();
    protected GridCellContainer movePoint = null;
    protected bool rotatedToWayPoint = false;
    protected bool canAtack = false;
    protected float currentHealth = 0;
    
    #endregion

    #region private Actions
    protected System.Action SnapedToGrid=null;
    protected System.Action EnemyFound = null;
    protected System.Action UnitOnPosition = null;
    protected System.Action WayFound = null;
    
    #endregion

    #region Unity functions
    private void Awake()
    {        
        SnapedToGrid = OnSnaped;
        OnAwake();
    }
    /// <summary>
    /// Для отладки
    /// </summary>
    private void OnDrawGizmos()
    {
        if (enemyTarget)
        {
            Gizmos.DrawLine(transform.position, enemyTarget.GridCell.position);
        }
    }
    private void Update()
    {
        if (unitState == UnitState.None) return;
        float deltaTime = Time.deltaTime;
        switch (unitState)
        {
            case UnitState.SearchingEnemy:
                SearchEnemy();
                break;
            case UnitState.MovingToEnemyForAtack:
                MoveToPoint(deltaTime);
                break;
            case UnitState.Attacking:

                break;
        }
        if (enemyTarget)
        {
            if (!enemyTarget.gameObject.activeSelf)
            {
                enemyTarget = null;
                unitState = UnitState.SearchingEnemy;
            }
        }
    }
    #endregion

    #region private functions
    /// <summary>
    /// Получает углы поворота на указанную позицию
    /// </summary>
    /// <param name="startpos"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    protected Vector3 GetEulerRotationToPos(Vector2 startpos, Vector2 pos)
    {
        Vector3 toRet = Vector3.zero;
        float a = pos.y - startpos.y;
        float b = pos.x - startpos.x;

        float angle = Mathf.Atan2(a, b) * Mathf.Rad2Deg;
        if (a > 0) angle = -angle;
        if (a < 0 && angle < 0) angle = -angle;   

        toRet = new Vector3(0, angle, 0);
        return toRet;
    }
    /// <summary>
    /// Функция поиска врага
    /// </summary>
    protected void SearchEnemy()
    {
        if(unitManager && enemyTarget==null)
        {
            wayPoints.Clear();
            List<Unit> unitList = unitManager.GetUnitsOfSide(enemyTeam);
            float minDistance = float.MaxValue;
            float distance = 0;
            int index = 0;            
            if (unitList.Count > 0)
            {
                for (int i = 0; i < unitList.Count; i++)
                {
                    distance = Vector3.Distance(centerPos.position, unitList[i].GridCell.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        index = i;
                    }
                }
                enemyTarget = unitList[index];
                if (enemyTarget)
                {
                    EnemyFound?.Invoke();
                }
            }
        }
    }
    /// <summary>
    /// Функция поиска пути к цели
    /// </summary>
    protected void FindWayTotarget()
    {
        if (levelPresenter)
        {            
            levelPresenter.SetCellEmpty(centerPos.gridPosition);
            wayPoints = levelPresenter.FindWay(centerPos.gridPosition,enemyTarget.GridCell.gridPosition);
            
            WayFound?.Invoke();
        }
    }
    /// <summary>
    /// Шаблон функции для проверки дистанции до цели
    /// </summary>
    protected virtual void CheckAtackDistanceToTarget()
    {
        
    }
    /// <summary>
    /// Обработчик события найденного пути
    /// </summary>
    protected virtual void OnWayFound()
    {
        unitState = UnitState.MovingToEnemyForAtack;
    }
    /// <summary>
    /// Обработчик события найденного врага
    /// </summary>
    protected void OnEnemyFound()
    {
        CheckAtackDistanceToTarget();
    }
    /// <summary>
    /// Дополнительная функция, вызываемая во время Awake
    /// </summary>
    protected virtual void OnAwake()
    {
        EnemyFound = OnEnemyFound;
        WayFound = OnWayFound;
    }
    /// <summary>
    /// Возвращет true после поворота к указанной позиции
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected bool RotateToPoint(Vector3 position)
    {
        if (transform.position != position)
        {
            Vector3 rot = GetEulerRotationToPos(new Vector2(transform.position.x, transform.position.y), new Vector2(position.x, position.y));
            transform.eulerAngles = rot;
        }
        return true;
    }
    /// <summary>
    /// Возвращет true после поворота к указанной позиции
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    protected bool RotateToPoint(GridCellContainer cell)
    {
        if (cell.gridPosition != centerPos.gridPosition)
        {
            Vector3 rot = GetEulerRotationToPos(new Vector2(centerPos.gridPosition.x, centerPos.gridPosition.y), new Vector2(cell.gridPosition.x, cell.gridPosition.y));
            transform.eulerAngles = rot;
        }
        return true;
    }
    /// <summary>
    /// Функция движения по точкам пути
    /// </summary>
    /// <param name="deltaTime"></param>
    protected virtual void MoveToPoint(float deltaTime)
    {
        if (wayPoints.Count > 0)
        {
            if (movePoint == null)
            {
                levelPresenter.GetGridCellContainer(wayPoints[0], out movePoint);
            }
            if (!rotatedToWayPoint)
            {
                rotatedToWayPoint = RotateToPoint(movePoint);
            }
            else
            {
                Vector3 pointPos = new Vector3(movePoint.position.x, transform.position.y, movePoint.position.z);
                Vector3 pos = Vector3.MoveTowards(transform.position, pointPos, deltaTime * UnitData.moveSpeed);
                pos.Set(pos.x, transform.position.y, pos.z);
                transform.position = pos;
                                

                if (enemyTarget == null)
                {
                    unitState = UnitState.SearchingEnemy;
                }
                if (Vector3.Distance(transform.position, pointPos) < rangeToChangeMovePoint)
                {
                    centerPos = movePoint;
                    wayPoints.RemoveAt(0);
                    if (wayPoints.Count > 0)
                    {
                        levelPresenter.GetGridCellContainer(wayPoints[0], out movePoint);                        
                        rotatedToWayPoint = false;
                    }
                    CheckAtackDistanceToTarget();
                }
            }

        }
        else
        {
            //Debug.Log("On position");
        }
    }
    /// <summary>
    /// Шаблон функции атаки цели
    /// </summary>
    protected virtual void AtackTarget()
    {

    }
    /// <summary>
    /// Обработчик события привязки к сетке
    /// </summary>
    protected virtual void OnSnaped()
    {
        unitState = UnitState.SearchingEnemy;
    }
    #endregion

    #region public functions  
    /// <summary>
    /// Задает начальный конфиг
    /// </summary>
    /// <param name="data"></param>
    public void SetUnitData(UnitParameters data)
    {
        UnitData = data;

    }
    /// <summary>
    /// Выключает юнита
    /// </summary>
    public void DeactivateUnit()
    {
        wayPoints.Clear();
        enemyTarget = null;
        movePoint = null;
        canAtack = false;
        unitState = UnitState.None;
    }
    /// <summary>
    /// Возвращает текущий материал
    /// </summary>
    /// <returns></returns>
    public Material GetCurrentMaterial()
    {
        switch (UnitData.side)
        {
            case UnitSide.None:
                return neutralMaterial;                
            case UnitSide.Blue:
                return blueTeamMaterial;                
            case UnitSide.Red:
                return redTeamMaterial;
            default: return neutralMaterial;
        }
    }
    /// <summary>
    /// Задает команду врага
    /// </summary>
    /// <param name="team"></param>
    public void SetEnemyTeam(UnitSide team)
    {
        enemyTeam = team;
    }
    public int WidthInCells
    {
        get { return widthInCells; }
    }
    public int HeightInCells
    {
        get { return heightInCells; }
    }
    public float WalkHeight
    {
        get { return UnitData.walkHeight; }
    }
    /// <summary>
    /// Задает сторону
    /// </summary>
    /// <param name="side"></param>
    public void SetSide(UnitSide side)
    {
        UnitData.side = side;
        Material tempMaterial = null;
        switch (UnitData.side)
        {
            case UnitSide.None:
                tempMaterial = neutralMaterial;
                break;
            case UnitSide.Blue:
                tempMaterial = blueTeamMaterial;
                break;
            case UnitSide.Red:
                tempMaterial = redTeamMaterial;
                break;
        }
        for(int i=0;i<renderers.Count;i++)
        {
            renderers[i].material = tempMaterial;
        }
    }
    /// <summary>
    /// Инициализация юнита
    /// </summary>
    public void Init()
    {        
        if(healthBarImage) healthBarImage.fillAmount = 1;
        currentHealth = UnitData.maxHealth;
        levelPresenter = LevelPresenter.main;
        unitManager = UnitManager.main;
        IsInited = true;
        enemyTarget = null;
        switch(UnitData.side)
        {
            case UnitSide.None:
                enemyTeam = UnitSide.None;
                break;
            case UnitSide.Blue:
                enemyTeam = UnitSide.Red;
                break;
            case UnitSide.Red:
                enemyTeam = UnitSide.Blue;
                break;
        }
    }
    /// <summary>
    /// Привязка к сетке по текущей клетке
    /// </summary>
    public void SnapToGridByCurrentCell()
    {
        if (levelPresenter)
        {
            levelPresenter.SetCellOccupied(centerPos.gridPosition);
        }
    }
    /// <summary>
    /// Привязка к сетке по текущему мировому положению
    /// </summary>
    public void SnapToGrid()
    {        
        if (levelPresenter)
        {
            GridCellContainer cell = null;
            levelPresenter.GetClosestGridCell(transform.position, out cell);            
            if (cell != null)
            {
                transform.position = new Vector3(cell.position.x, transform.position.y, cell.position.z);
                centerPos = cell;
                levelPresenter.SetCellOccupied(cell.gridPosition);
                SnapedToGrid?.Invoke();
            }
        }
    }
    /// <summary>
    /// Задает клетку юнита
    /// </summary>
    /// <param name="cell"></param>
    public void SetGridCell(GridCellContainer cell)
    {
        if (cell != null)
        {
            centerPos = cell;
            if (levelPresenter) levelPresenter.SetCellOccupied(cell.gridPosition);
            SnapedToGrid?.Invoke();
        }
    }
    /// <summary>
    /// Обработка получнного урона
    /// </summary>
    /// <param name="damage"></param>
    public void ProcessDamage(float damage)
    {
        currentHealth -= damage;
        healthBarImage.fillAmount = currentHealth / UnitData.maxHealth;
        if (currentHealth<=0)
        {
            currentHealth = 0;
            unitManager.DestroyUnit(this);
        }
    }
    #endregion

    #region public values
    public bool IsInited { get; private set; }
    public GridCellContainer GridCell{ get { return centerPos; } }
    public UnitSide UnitSide
    {
        get { return UnitData.side; }
    }
    #endregion

}
/// <summary>
/// Интейрфейс для юнитов, получаемых урон
/// </summary>
public interface IDamageble
{
    void ProcessDamage(float damage);
}
/// <summary>
/// Интерфейс для юнитов, которых можно привязать к сетке
/// </summary>
public interface IGridSnap
{
    void SnapToGrid();
}
