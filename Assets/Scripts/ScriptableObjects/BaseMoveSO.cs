using System.Collections.Generic;
using UnityEngine;

public abstract class MoveBaseSO : ScriptableObject
{
    [Header("Base Info")]
    public string moveID;

    public string moveName;
    public string moveNameCN;
    public int pp;
    
    public int range;
    public TargetType targetType;
    public Target target;

    [Header("Additional Effects")]
    public List<MoveEffectSO> additionalEffects;
}