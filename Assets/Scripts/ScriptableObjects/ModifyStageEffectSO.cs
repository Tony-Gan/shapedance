using UnityEngine;

public enum EffectTarget
{
    Caster,
    Target
}

[CreateAssetMenu(fileName = "New Modify Stage Effect", menuName = "Pokemon/Move Effect/Modify Stage")]
public class ModifyStageEffectSO : MoveEffectSO
{
    [Header("Modify Stage Details")]
    public EffectTarget effectTarget;
    public StageType stageToModify;
    
    [Range(-6, 6)]
    public int stageChange;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        PokemonStats statsToChange = effectTarget == EffectTarget.Caster ? caster : target;

        if (statsToChange == null)
        {
            Debug.LogWarning($"ModifyStageEffectSO: Target Pokemon is null (EffectTarget: {effectTarget})");
            return;
        }
        
        if (statsToChange.BattleStages.TryModifyStage(stageToModify, stageChange, out string message))
        {
            Debug.Log($"[Effect] {statsToChange.alias}'s {message}");
        }
        else
        {
            Debug.Log($"[Effect] {message}");
        }
    }
}