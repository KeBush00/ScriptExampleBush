using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

/// <summary>
/// The Stat class defines what a stat is and will be used for algorithims for actions and movement. Also it will determine
/// the value of substats. Players CAN upgrade this stat directly via leveling points , items, or status effects.
/// </summary>
public class Stat 
    
{
    [SerializeField]
    int _baseValue; // the numerical value for stat

    [SerializeField]
    string _description;// the description of the stat that will show in menus
    //Getter//
    public int getBaseValue() {
        return _baseValue;
    }
    //Setter//
    public void setBaseValue(int _value)
    {
        _baseValue = _value;
    }

}
