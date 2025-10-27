using UnityEngine;

public abstract class RadiusTargetingSO : TargetingStrategySO
{
    [Header("Range Calculation")]
    public MoveRangeType rangeType = MoveRangeType.FixedValue;

    [Tooltip("Only apply when RangeType as FixedValue")]
    public int fixedRange = 1;
    
    
    public override void Initiate(TargetingManager manager, PokemonStats caster, MoveBaseSO move)
    {
        float casterRadius = caster.pokemon.radius;
        float moveBaseRange = GetBaseRange(caster, move);
        
        manager.CurrentRange = moveBaseRange + casterRadius;

        manager.RangeIndicatorSprite.color = (move is AttackMoveSO) ? manager.AttackColor : manager.OtherColor;

        float diameter = manager.CurrentRange * 2f;
        manager.RangeIndicatorSprite.transform.localScale = new Vector3(diameter, diameter, 1f);
        manager.RangeIndicatorSprite.gameObject.SetActive(true);
        
        manager.FindAndCacheTargetsInRange();
    }

    public override void UpdateIndicator(TargetingManager manager, PokemonStats caster)
    {
        if (caster == null) return;
        Vector3 casterPos = caster.transform.position;
        manager.RangeIndicatorSprite.transform.position = new Vector3(casterPos.x, casterPos.y, casterPos.z + manager.ZOffset);
    }
    
    public float GetBaseRange(PokemonStats caster, MoveBaseSO move)
    {
        switch (rangeType)
        {
            case MoveRangeType.FixedValue:
                return fixedRange;

            case MoveRangeType.CasterRadius:
                if (caster == null || caster.pokemon == null)
                {
                    Debug.LogWarning($"Move {move.moveNameCN} needs caster stats to calculate CasterRadius range. Defaulting to 0.");
                    return 0;
                }
                return caster.pokemon.radius;
            
            default:
                Debug.LogWarning($"Unknown rangeType {rangeType} on move {move.moveNameCN}. Defaulting to 0.");
                return 0;
        }
    }
}