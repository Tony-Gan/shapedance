using UnityEngine;

public enum EffectTarget
{
    Caster,
    Target
}


[CreateAssetMenu(fileName = "New Modify Stat Effect", menuName = "Pokemon/Move Effect/Modify Stat")]
public class ModifyStatEffectSO : MoveEffectSO
{
    [Header("Modify Stat Details")]
    public EffectTarget effectTarget;
    public StatType statToModify;
    [Range(-6, 6)]
    public int stageChange;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        PokemonStats statsToChange = null;
        if (effectTarget == EffectTarget.Caster)
        {
            statsToChange = caster;
        }
        else if (effectTarget == EffectTarget.Target)
        {
            statsToChange = target;
        }

        if (statsToChange == null)
        {
            Debug.LogWarning($"ModifyStatEffectSO: Target Pokemon is null (EffectTarget: {effectTarget})");
            return;
        }
        
        switch (statToModify)
        {
            case StatType.Attack:
                statsToChange.AttackStage += stageChange;
                break;
            case StatType.Defense:
                statsToChange.DefenseStage += stageChange;
                break;
            case StatType.SpAttack:
                statsToChange.SpAttackStage += stageChange;
                break;
            case StatType.SpDefense:
                statsToChange.SpDefenseStage += stageChange;
                break;
            case StatType.Speed:
                statsToChange.SpeedStage += stageChange;
                break;
            case StatType.Accuracy: 
                statsToChange.AccuracyStage += stageChange;
                break;
             case StatType.Evasion:
                statsToChange.EvasionStage += stageChange;
                break;
            case StatType.HP:
                Debug.LogWarning("ModifyStatEffectSO: Cannot modify HP stage.");
                break;
        }
        
        string targetName = statsToChange.alias;
        string verb = (stageChange > 0) ? "rose" : "fell";
        Debug.Log($"[Effect]: {targetName}'s {statToModify} {verb} by {Mathf.Abs(stageChange)}!");
    }
}