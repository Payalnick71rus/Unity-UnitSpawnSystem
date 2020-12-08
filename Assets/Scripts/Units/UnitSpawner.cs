using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EpPathFinding.cs;
/// <summary>
/// Класс - спавнер юнитов
/// </summary>
public class UnitSpawner : Unit
{
    [SerializeField] UnitSide side;
    [SerializeField] float health=20;
    [SerializeField] UnitsConfigData config;
    [SerializeField] private List<ObjectType> unitTypes = new List<ObjectType>();
    [SerializeField] private float spawnDelay = 1f;

    private PoolManager pool;
    private bool canSpawn = false;
    private int count = 0;  
    private void Start()
    {
        UnitData.side = side;
        UnitData.maxHealth = health;
        UnitData.canAtack = false;
        Init();
        canSpawn = true;
        pool = PoolManager.main;
    }
    protected override void OnSnaped()
    {
        StartCoroutine("SpawnLoop");
    }
    /// <summary>
    /// Цикл спавна
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnLoop()
    {
        while(canSpawn)
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnUnit();            
        }
    }
    /// <summary>
    /// Спавнит юнита из пула
    /// </summary>
    private void SpawnUnit()
    {
        if (unitTypes.Count > 0)
        {
            int index = Random.Range(0, unitTypes.Count);
            ObjectType type = unitTypes[index];            
            GridCellContainer spawnPos = null;
            PoolObject poolObject = null;
            pool.GetObject(type, out poolObject);
            GameObject prefab = poolObject.gameObject;
            prefab.SetActive(true);
            Unit unitScript = prefab.GetComponent<Unit>();
            unitScript.SetSide(UnitData.side);
            switch(type)
            {
                case ObjectType.MeleeUnit:
                    unitScript.SetUnitData(config.meleeUnitData);
                    break;
                case ObjectType.RangeUnit:
                    unitScript.SetUnitData(config.rangeUnitData);
                    break;
            }
            unitScript.Init();            

            float h = unitScript.WalkHeight;
            if (FindFreeSpawnCell(unitScript.WidthInCells, unitScript.HeightInCells, out spawnPos))
            {
                prefab.transform.position = spawnPos.position + new Vector3(0, h, 0);
                unitScript.SetGridCell(spawnPos);
                if (UnitManager.main) UnitManager.main.AddUnit(unitScript);
                
            }           
        }
    }
    /// <summary>
    /// Поиск свободной клетки для спавна
    /// </summary>
    /// <param name="unitWidth"></param>
    /// <param name="unitHeight"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    private bool FindFreeSpawnCell(int unitWidth, int unitHeight, out GridCellContainer cell)
    {
        int startDistance = 1;
        cell = null;
        bool ok = false;
        int startX = centerPos.gridPosition.x, startY = centerPos.gridPosition.y;
        int rectWidth = widthInCells;
        int rectHeight = heightInCells;
        GridPos tempPos = new GridPos(centerPos.gridPosition);
        int iterationsCount = 0;
        while (!ok)
        {
            iterationsCount++;
            rectWidth = rectWidth + 2;
            rectHeight = rectHeight + 2;
            startX = centerPos.gridPosition.x - (rectWidth - 1) / 2;
            startY = centerPos.gridPosition.y - (rectHeight - 1) / 2;
            for (int i=0;i< rectWidth;i++)
            {
                for (int j = 0; j < rectHeight; j++)
                {
                    // проверка периметра прямоугольника
                    if(i==0 || i==(rectWidth-1) || j==0 || j==(rectHeight-1))
                    {
                        if(levelPresenter)
                        {
                            tempPos.Set(startX+i, startY+j);
                            if(levelPresenter.IsPlaceFree(tempPos,unitWidth, unitHeight))
                            {
                                if(levelPresenter.GetGridCellContainer(tempPos, out cell))
                                {                                    
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            if (iterationsCount > 100)
            {
                Debug.Log("Can not find position, so much iterations!");
                return false;
            }
        }
        return false;
    }
}
