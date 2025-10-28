using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Pokemon/Move Effect/Status Effect")]
public class StatusEffectSO : MoveEffectSO
{
    [Header("Status Details")]
    public StatusCondition statusType = StatusCondition.None;
    
    public EffectTarget effectTarget = EffectTarget.Target;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        PokemonStats affectedPokemon = effectTarget == EffectTarget.Caster ? caster : target;

        if (affectedPokemon == null)
        {
            Debug.LogWarning($"StatusEffectSO: Target Pokemon is null (EffectTarget: {effectTarget})");
            return;
        }

        // TODO: 实现状态异常系统后，调用相应的方法
        // affectedPokemon.StatusManager.ApplyStatus(statusType);
        
        Debug.Log($"[Effect] {affectedPokemon.alias} is now {GetStatusName(statusType)}!");
    }

    private string GetStatusName(StatusCondition status)
    {
        return status switch
        {
            StatusCondition.Burn => "burned",
            StatusCondition.Poison => "poisoned",
            StatusCondition.BadlyPoisoned => "badly poisoned",
            StatusCondition.Paralyze => "paralyzed",
            StatusCondition.Freeze => "frozen",
            StatusCondition.Sleep => "asleep",
            StatusCondition.Confusion => "confused",
            _ => "affected"
        };
    }
}