using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "Pokemon/Move Effect/Heal")]
public class HealEffectSO : MoveEffectSO
{
    [Header("Heal Details")]
    public EffectTarget effectTarget = EffectTarget.Caster;
    
    [Header("Heal Amount")]
    public HealType healType = HealType.PercentageOfMaxHP;
    
    public float healValue = 0.5f;
    
    [Header("Damage Context (for drain moves)")]
    [System.NonSerialized] public int lastDamageDealt = 0;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        PokemonStats healTarget = effectTarget == EffectTarget.Caster ? caster : target;

        if (healTarget == null)
        {
            Debug.LogWarning($"HealEffectSO: Heal target is null (EffectTarget: {effectTarget})");
            return;
        }

        int healAmount = CalculateHealAmount(healTarget);
        
        if (healAmount <= 0)
        {
            Debug.LogWarning($"HealEffectSO: Calculated heal amount is {healAmount}, skipping heal.");
            return;
        }

        int maxHP = healTarget.GetStat(StatType.HP);
        int beforeHP = healTarget.CurrentHP;
        healTarget.CurrentHP = Mathf.Min(healTarget.CurrentHP + healAmount, maxHP);
        int actualHealed = healTarget.CurrentHP - beforeHP;

        Debug.Log($"[Effect] {healTarget.alias} recovered {actualHealed} HP! ({healTarget.CurrentHP}/{maxHP})");
    }

    private int CalculateHealAmount(PokemonStats target)
    {
        return healType switch
        {
            HealType.FixedAmount => Mathf.FloorToInt(healValue),
            HealType.PercentageOfMaxHP => Mathf.FloorToInt(target.GetStat(StatType.HP) * healValue),
            HealType.PercentageOfDamageDealt => Mathf.FloorToInt(lastDamageDealt * healValue),
            _ => 0
        };
    }

    public void SetDamageContext(int damageDealt)
    {
        lastDamageDealt = damageDealt;
    }
}