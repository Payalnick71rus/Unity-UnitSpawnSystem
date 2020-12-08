using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Конфиг файл для спавнера
/// </summary>
[CreateAssetMenu(fileName ="Unit data", menuName = "Create Units Config", order = 51)]
public class UnitsConfigData : ScriptableObject
{
    public UnitParameters rangeUnitData;
    public UnitParameters meleeUnitData;
}
