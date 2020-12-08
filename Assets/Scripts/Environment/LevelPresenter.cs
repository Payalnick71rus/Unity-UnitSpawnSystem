using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EpPathFinding.cs;
/// <summary>
/// Класс предназначается для работы с сеткой и ее отображением
/// </summary>
public class LevelPresenter : MonoBehaviour
{
    // синглтон
    public static LevelPresenter main = null;
    public bool GridIsCreated { get; private set; }
    #region Editor fields
    [Header("Префаб клетки")]
    [SerializeField] private GameObject cellPrefab;
    [Header("Ширина сетки")]
    [SerializeField] private int levelWidth;
    [Header("Высота сетки")]
    [SerializeField] private int levelHeight;
    [Header("Размер склетки")]
    [Range(0.1f,100)]
    [SerializeField] private float cellSize;
    [Header("Стартовая позиция уровня (левый нижний угол)")]
    [SerializeField] private Vector3 startSpawnPosition;
    #endregion
    /// <summary>
    /// Срабатывает после генерации сетки
    /// </summary>
    #region Events
    public UnityEvent GridCreated;
    #endregion

    #region private variables
    private List<GridCellContainer> cells = new List<GridCellContainer>();
    private BaseGrid levelGrid = null;
    private JumpPointParam wayRouter = null;
    
    #endregion

    #region Unity Methods
    
    private void Awake()
    {
        if(main==null)
        {
            main = this;
            GridIsCreated = false;
            DontDestroyOnLoad(gameObject);
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
    private void Start()
    {
        GenerateCellsGrid(levelWidth, levelHeight);
    }
    #endregion

    #region private methods
    /// <summary>
    /// Генерирует сетку и создает объекты сетки
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void GenerateCellsGrid(int width, int height)
    {
        if (cellPrefab)
        {
            levelGrid = new StaticGrid(width, height);

            Vector3 spawnPosition = Vector3.zero;
            cells.Clear();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    levelGrid.SetWalkableAt(i, j, true);
                    spawnPosition.Set(startSpawnPosition.x+i* cellSize, startSpawnPosition.y, startSpawnPosition.z+j * cellSize);
                    GameObject cell = Instantiate(cellPrefab, spawnPosition, Quaternion.identity) as GameObject;
                    cell.transform.parent = transform;
                    GridCellContainer container = new GridCellContainer() { position = spawnPosition, gridCellObject = cell, gridPosition = new GridPos(i, j) };
                    cells.Add(container);
                }
            }
            GridPos startPos = new GridPos(10, 10);
            GridPos endPos = new GridPos(20, 10);
            wayRouter = new JumpPointParam(levelGrid,EndNodeUnWalkableTreatment.ALLOW, DiagonalMovement.OnlyWhenNoObstacles);            
            
            GridIsCreated = true;
            GridCreated.Invoke();
        }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Возвращает путь из клеток от старта до финиша
    /// </summary>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public List<GridPos> FindWay(GridPos start, GridPos stop)
    {
        
        wayRouter.Reset(start, stop);
        List<GridPos> list = JumpPointFinder.FindPath(wayRouter);
        
        return JumpPointFinder.GetFullPath(list);      
    }
    
    /// <summary>
    /// Возвращает дистанцию между клетками
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <returns></returns>
    public float CalcDistanceBetweenCells(GridPos pos1, GridPos pos2)
    {
        float distance = -1f;
        if(pos1!=null && pos2!=null)
        {
            Vector2 p1 = new Vector2(pos1.x, pos1.y);
            Vector2 p2 = new Vector2(pos2.x, pos2.y);
            distance = Vector2.Distance(p1, p2);
        }
        return distance;
    }
    /// <summary>
    /// Находит ближайшую клетку по мировой позиции
    /// </summary>
    /// <param name="position"></param>
    /// <param name="cell"></param>
    public void GetClosestGridCell(Vector3 position, out GridCellContainer cell)
    {
        cell = null;
        float minDistance = float.MaxValue;
        float distance = 0;
        int index = 0;
        for(int i = 0;i< cells.Count;i++)
        {
            distance = Vector3.Distance(position, cells[i].position);
            if(distance< minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }
        cell = cells[index];        
    }
    /// <summary>
    /// Возвращет мировую позицию по позиции клетки
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public bool GetWorldPosition(GridPos pos, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        for (int i = 0; i < cells.Count; i++)
        {            
            if (pos == cells[i].gridPosition)
            {
                worldPosition = cells[i].position;
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Возвращает контейнер клетки по ее позиции в сетке
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool GetGridCellContainer(GridPos pos, out GridCellContainer cell)
    {
        cell = null;
        for (int i = 0; i < cells.Count; i++)
        {
            if (pos == cells[i].gridPosition)
            {
                cell = cells[i];
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Устанавливает клетку занятой
    /// </summary>
    /// <param name="pos"></param>
    public void SetCellOccupied(GridPos pos)
    {
        if(levelGrid!=null)
        {
            levelGrid.SetWalkableAt(pos.x, pos.y, false);
        }
    }
    /// <summary>
    /// Освобождает клетку
    /// </summary>
    /// <param name="pos"></param>
    public void SetCellEmpty(GridPos pos)
    {
        if (levelGrid != null)
        {
            levelGrid.SetWalkableAt(pos.x, pos.y, true);
        }
    }
    /// <summary>
    /// Возрвращает true если клетка пустая
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsCellFree(GridPos pos)
    {
        if (levelGrid != null)
        {
            return levelGrid.IsWalkableAt(pos);
        }
        else return false;
    }
    /// <summary>
    /// Проверяет площать из клеток на пустое пространство, pos - центр площади
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool IsPlaceFree(GridPos pos, int width, int height)
    {
        if (!levelGrid.IsWalkableAt(pos)) return false;
        
        int startX = pos.x - (width - 1) / 2;
        int startY = pos.y - (height - 1) / 2;
        GridPos temPos = new GridPos();
        for(int i=0;i< width;i++)
        {
            for (int j = 0; j < height; j++)
            {
                temPos.Set(startX+i, startY+j);
                if (!levelGrid.IsWalkableAt(temPos)) return false;
            }
        }
        return true;
    }
    #endregion

    #region public data
    /// <summary>
    /// Возвращает объект класса Сетки уровня
    /// </summary>
    public BaseGrid LevelGrid
    {
        get { return levelGrid; }
    }
    #endregion
}
/// <summary>
///  Контейнер для хранения мирового положения клетки, положения в виртуальной сетке и хранения объекта клетки
/// </summary>
public class GridCellContainer
{
    public Vector3 position;
    public GameObject gridCellObject;
    public GridPos gridPosition;
}