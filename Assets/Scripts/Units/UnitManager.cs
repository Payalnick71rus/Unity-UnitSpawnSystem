using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Класс - менеджер юнитов
/// </summary>
public class UnitManager : MonoBehaviour
{
    public static UnitManager main = null;
    [SerializeField] private Text redCountText, blueCountText;
    [SerializeField] private List<UnitSpawner> unitSpawners;
    [SerializeField] private float maximumUnitsPerSide = 5;
    
    private List<Unit> redUnits = new List<Unit>();
    private List<Unit> blueUnits = new List<Unit>();

    #region Unity Methods
    private void Awake()
    {
        if (main == null)
        {
            main = this;
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
        for (int i = 0; i < unitSpawners.Count; i++)
        {
            if(!unitSpawners[i].IsInited) unitSpawners[i].Init();
        }
    }
    
    #endregion

    #region private methods
    private void UpdateUIData()
    {
        redCountText.text = $"Количество красных юнитов: {redUnits.Count}";
        blueCountText.text = $"Количество красных юнитов: {blueUnits.Count}";
    }
    
    #endregion

    #region public variables
    /// <summary>
    /// Убирает юнита в пулл и создает эффект смерти
    /// </summary>
    /// <param name="unit"></param>
    public void DestroyUnit(Unit unit)
    {
        switch (unit.UnitSide)
        {
            case UnitSide.Blue:
                blueUnits.Remove(unit);
                break;
            case UnitSide.Red:
                redUnits.Remove(unit);
                break;
            case UnitSide.None:
                break;
        }
        LevelPresenter.main.SetCellEmpty(unit.GridCell.gridPosition);

        PoolObject effectObject = null;
        PoolManager.main.GetObject(ObjectType.DeathEffect, out effectObject);
        GameObject effect = effectObject.gameObject;
        effect.SetActive(true);
        effect.transform.position = unit.gameObject.transform.position;
        ParticleSystem ps = null;
        if(effect.TryGetComponent(out ps))
        {
            ParticleSystemRenderer rendererPS = ps.GetComponent<ParticleSystemRenderer>();
            if(rendererPS)
            {
                rendererPS.material = unit.GetCurrentMaterial();
            }
            ps.Play();
            
        }
        // возвращает эффект в пулл
        if (ps) effectObject.ReturnToPoolDelayed(ps.main.startDelay.constant+ps.main.startLifetime.constant);
        else effectObject.ReturnToPool();

        unit.DeactivateUnit();
        unit.gameObject.SetActive(false);
        
        UpdateUIData();
    }
    public float MaximumUnitsPerSide
    {
        get { return maximumUnitsPerSide; }
    }
    #endregion

    #region public methods
    public void OnGridGenerated()
    {        
        for(int i=0;i< unitSpawners.Count; i++)
        {
            unitSpawners[i].SnapToGrid();
        }
    }
    /// <summary>
    /// Возвращает список юнитов определенной стороны
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<Unit> GetUnitsOfSide(UnitSide side)
    {
        switch (side)
        {
            case UnitSide.Blue:
                return blueUnits;
            case UnitSide.Red:
                return redUnits;
            case UnitSide.None:
                return null;
        }
        return null;
    }
    /// <summary>
    /// Добавляет юнита 
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnit(Unit unit)
    {        
        switch(unit.UnitSide)
        {
            case UnitSide.Blue:
                blueUnits.Add(unit);
                break;
            case UnitSide.Red:
                redUnits.Add(unit);
                break;
        }
        UpdateUIData();
    }
    /// <summary>
    /// Убирает юнита
    /// </summary>
    /// <param name="unit"></param>
    public void RemoveUnit(Unit unit)
    {
        switch (unit.UnitSide)
        {
            case UnitSide.Blue:
                blueUnits.Remove(unit);
                break;
            case UnitSide.Red:
                redUnits.Remove(unit);
                break;
        }
        UpdateUIData();
    }
    #endregion
}
