using UnityEngine;
using System.Collections.Generic;
using System.Text;

public static class BattleCalculator
{
    private static readonly Dictionary<ElementType, Dictionary<ElementType, float>> TypeChart;

    static BattleCalculator()
    {
        TypeChart = new Dictionary<ElementType, Dictionary<ElementType, float>>
        {
            [ElementType.Normal] = new Dictionary<ElementType, float>{{ ElementType.Rock, 0.5f },{ ElementType.Ghost, 0f },{ ElementType.Steel, 0.5f }},
            [ElementType.Fire] = new Dictionary<ElementType, float>{{ ElementType.Fire, 0.5f },{ ElementType.Water, 0.5f },{ ElementType.Grass, 2f },{ ElementType.Ice, 2f },{ ElementType.Bug, 2f },{ ElementType.Rock, 0.5f },{ ElementType.Dragon, 0.5f },{ ElementType.Steel, 2f }},
            [ElementType.Water] = new Dictionary<ElementType, float>{{ ElementType.Fire, 2f },{ ElementType.Water, 0.5f },{ ElementType.Grass, 0.5f },{ ElementType.Ground, 2f },{ ElementType.Rock, 2f },{ ElementType.Dragon, 0.5f }},
            [ElementType.Grass] = new Dictionary<ElementType, float>{{ ElementType.Fire, 0.5f },{ ElementType.Water, 2f },{ ElementType.Grass, 0.5f },{ ElementType.Poison, 0.5f },{ ElementType.Ground, 2f },{ ElementType.Flying, 0.5f },{ ElementType.Bug, 0.5f },{ ElementType.Rock, 2f },{ ElementType.Dragon, 0.5f },{ ElementType.Steel, 0.5f }},
            [ElementType.Electric] = new Dictionary<ElementType, float>{{ ElementType.Water, 2f },{ ElementType.Grass, 0.5f },{ ElementType.Electric, 0.5f },{ ElementType.Ground, 0f },{ ElementType.Flying, 2f },{ ElementType.Dragon, 0.5f }},
            [ElementType.Ice] = new Dictionary<ElementType, float>{{ ElementType.Fire, 0.5f },{ ElementType.Water, 0.5f },{ ElementType.Grass, 2f },{ ElementType.Ice, 0.5f },{ ElementType.Ground, 2f },{ ElementType.Flying, 2f },{ ElementType.Dragon, 2f },{ ElementType.Steel, 0.5f }},
            [ElementType.Fighting] = new Dictionary<ElementType, float>{{ ElementType.Normal, 2f },{ ElementType.Ice, 2f },{ ElementType.Poison, 0.5f },{ ElementType.Flying, 0.5f },{ ElementType.Psychic, 0.5f },{ ElementType.Bug, 0.5f },{ ElementType.Rock, 2f },{ ElementType.Ghost, 0f },{ ElementType.Dark, 2f },{ ElementType.Steel, 2f },{ ElementType.Fairy, 0.5f }},
            [ElementType.Poison] = new Dictionary<ElementType, float>{{ ElementType.Grass, 2f },{ ElementType.Poison, 0.5f },{ ElementType.Ground, 0.5f },{ ElementType.Rock, 0.5f },{ ElementType.Ghost, 0.5f },{ ElementType.Steel, 0f },{ ElementType.Fairy, 2f }},
            [ElementType.Ground] = new Dictionary<ElementType, float>{{ ElementType.Fire, 2f },{ ElementType.Grass, 0.5f },{ ElementType.Electric, 2f },{ ElementType.Poison, 2f },{ ElementType.Flying, 0f },{ ElementType.Bug, 0.5f },{ ElementType.Rock, 2f },{ ElementType.Steel, 2f }},
            [ElementType.Flying] = new Dictionary<ElementType, float>{{ ElementType.Grass, 2f },{ ElementType.Electric, 0.5f },{ ElementType.Fighting, 2f },{ ElementType.Bug, 2f },{ ElementType.Rock, 0.5f },{ ElementType.Steel, 0.5f }},
            [ElementType.Psychic] = new Dictionary<ElementType, float>{{ ElementType.Fighting, 2f },{ ElementType.Poison, 2f },{ ElementType.Psychic, 0.5f },{ ElementType.Dark, 0f },{ ElementType.Steel, 0.5f }},
            [ElementType.Bug] = new Dictionary<ElementType, float>{{ ElementType.Fire, 0.5f },{ ElementType.Grass, 2f },{ ElementType.Fighting, 0.5f },{ ElementType.Poison, 0.5f },{ ElementType.Flying, 0.5f },{ ElementType.Psychic, 2f },{ ElementType.Ghost, 0.5f },{ ElementType.Dark, 2f },{ ElementType.Steel, 0.5f },{ ElementType.Fairy, 0.5f }},
            [ElementType.Rock] = new Dictionary<ElementType, float>{{ ElementType.Fire, 2f },{ ElementType.Ice, 2f },{ ElementType.Fighting, 0.5f },{ ElementType.Ground, 0.5f },{ ElementType.Flying, 2f },{ ElementType.Bug, 2f },{ ElementType.Steel, 0.5f }},
            [ElementType.Ghost] = new Dictionary<ElementType, float>{{ ElementType.Normal, 0f },{ ElementType.Psychic, 2f },{ ElementType.Ghost, 2f },{ ElementType.Dark, 0.5f }},
            [ElementType.Dragon] = new Dictionary<ElementType, float>{{ ElementType.Dragon, 2f },{ ElementType.Steel, 0.5f },{ ElementType.Fairy, 0f }},
            [ElementType.Dark] = new Dictionary<ElementType, float>{{ ElementType.Fighting, 0.5f },{ ElementType.Psychic, 2f },{ ElementType.Ghost, 2f },{ ElementType.Dark, 0.5f },{ ElementType.Fairy, 0.5f }},
            [ElementType.Steel] = new Dictionary<ElementType, float>{{ ElementType.Fire, 0.5f },{ ElementType.Water, 0.5f },{ ElementType.Electric, 0.5f },{ ElementType.Ice, 2f },{ ElementType.Rock, 2f },{ ElementType.Steel, 0.5f },{ ElementType.Fairy, 2f }},
            [ElementType.Fairy] = new Dictionary<ElementType, float>{{ ElementType.Fire, 0.5f },{ ElementType.Fighting, 2f },{ ElementType.Poison, 0.5f },{ ElementType.Dragon, 2f },{ ElementType.Dark, 2f },{ ElementType.Steel, 0.5f }}
        };
    }

    public static int HandleAttack(PokemonStats caster, PokemonStats target, AttackMoveSO move)
    {
        if (!CalculateHit(caster, target, move))
        {
            Debug.Log($"[Attack Result]: {caster.alias}'s {move.moveName} {BattleConstants.MSG_MISSED} {target.alias}!");
            return 0;
        }

        StringBuilder logBuilder = new();
        logBuilder.AppendLine($"[Attack Result]: {caster.alias}'s {move.moveName}!");
        logBuilder.AppendLine("========================================");
        logBuilder.AppendLine($"Caster: {caster.alias} (Lvl {caster.level})");
        logBuilder.AppendLine($"Target: {target.alias} (Lvl {target.level})");
        logBuilder.AppendLine($"Move: {move.moveName} (Power: {move.power}, Acc: {move.accuracy}%, Type: {move.elementType}, Category: {move.attackType})");

        bool isCritical = CalculateCritical(caster, move);
        if (isCritical)
        {
            logBuilder.AppendLine(BattleConstants.MSG_CRITICAL_HIT);
        }

        float typeEffectiveness = CalculateTypeEffectiveness(move.elementType, target.pokemon);
        string effectivenessMsg = BattleConstants.GetEffectivenessMessage(typeEffectiveness);
        if (!string.IsNullOrEmpty(effectivenessMsg))
        {
            logBuilder.AppendLine($"{effectivenessMsg} (x{typeEffectiveness})");
        }
        else
        {
            logBuilder.AppendLine($"Type Effectiveness: x{typeEffectiveness}");
        }

        var (damage, finalAttack, finalDefense) = CalculateDamage(caster, target, move, isCritical, typeEffectiveness);

        LogStatInfo(logBuilder, caster, target, move, finalAttack, finalDefense);
        
        logBuilder.AppendLine("----------------------------------------");
        logBuilder.AppendLine($"Final Damage: {damage}");

        target.TakeDamage(damage);

        logBuilder.AppendLine($"Target HP: {target.CurrentHP} / {target.GetStat(StatType.HP)}"); 

        if (target.IsFainted())
        {
            logBuilder.AppendLine($"{target.alias} has {BattleConstants.MSG_FAINTED}"); 
        }

        Debug.Log(logBuilder.ToString());
        
        return damage;
    }

    private static bool CalculateHit(PokemonStats caster, PokemonStats target, AttackMoveSO move)
    {
        if (move.ignoreAccuracyEvasion)
        {
            return true;
        }
        
        float moveBaseAccuracy = move.accuracy;
        
        int totalAccuracyStage = caster.BattleStages.GetStage(StageType.Accuracy) - target.BattleStages.GetStage(StageType.Evasion);
        
        float modifier = BattleConstants.GetAccuracyStageMultiplier(totalAccuracyStage);
        float finalAccuracy = moveBaseAccuracy * modifier;
        
        // 命中判定
        return Random.Range(0f, 100f) < finalAccuracy;
    }

    private static bool CalculateCritical(PokemonStats caster, AttackMoveSO move)
    {
        int totalCritStage = caster.BattleStages.GetStage(StageType.Critical) + move.criticalLevel;
        totalCritStage = Mathf.Clamp(totalCritStage, 0, BattleConstants.MAX_CRIT_STAGE);
        
        float critChance = BattleConstants.GetCriticalHitRate(totalCritStage);
        return Random.Range(0f, 1f) < critChance;
    }

    private static (int finalDamage, float effectiveAttack, float effectiveDefense) CalculateDamage(
        PokemonStats caster, PokemonStats target, AttackMoveSO move, bool isCritical, float typeEffectiveness)
    {
        int level = caster.level;
        int power = move.power;

        float attackStat;
        float defenseStat;
        
        if (move.attackType == AttackType.Physical) 
        {
            attackStat = caster.GetStat(StatType.Attack);
            defenseStat = target.GetStat(StatType.Defense);
            
            attackStat *= BattleConstants.GetStatStageMultiplier(caster.BattleStages.GetStage(StageType.Attack));
            defenseStat *= BattleConstants.GetStatStageMultiplier(target.BattleStages.GetStage(StageType.Defense));
        }
        else
        {
            attackStat = caster.GetStat(StatType.SpAttack);
            defenseStat = target.GetStat(StatType.SpDefense);

            attackStat *= BattleConstants.GetStatStageMultiplier(caster.BattleStages.GetStage(StageType.SpAttack));
            defenseStat *= BattleConstants.GetStatStageMultiplier(target.BattleStages.GetStage(StageType.SpDefense));
        }

        float baseDamage = (((2f * level / 5f) + 2f) * power * (attackStat / defenseStat) / 50f) + 2f;
        
        float modifier = 1.0f;
        
        if (isCritical)
        {
            modifier *= BattleConstants.CRITICAL_MULTIPLIER;
        }

        if (caster.pokemon.type1 == move.elementType || caster.pokemon.type2 == move.elementType) 
        {
            modifier *= BattleConstants.STAB_MULTIPLIER;
        }

        modifier *= typeEffectiveness;
        modifier *= Random.Range(BattleConstants.DAMAGE_RANDOM_MIN, BattleConstants.DAMAGE_RANDOM_MAX);
        
        int finalDamage = Mathf.FloorToInt(baseDamage * modifier);

        if (typeEffectiveness == BattleConstants.NO_EFFECT)
        {
            return (0, attackStat, defenseStat);
        }
        
        return (Mathf.Max(BattleConstants.MIN_DAMAGE, finalDamage), attackStat, defenseStat);
    }
    
    private static float CalculateTypeEffectiveness(ElementType attackType, PokemonSO defender)
    {
        float effectiveness = GetTypeEffectiveness(attackType, defender.type1);
        
        if (defender.type2 != ElementType.None)
        {
            effectiveness *= GetTypeEffectiveness(attackType, defender.type2);
        }
        
        return effectiveness;
    }
    
    private static float GetTypeEffectiveness(ElementType attackType, ElementType defenseType)
    {
        if (attackType == ElementType.None || defenseType == ElementType.None)
        {
            return BattleConstants.NORMAL_EFFECTIVE;
        }

        if (TypeChart.TryGetValue(attackType, out var attackMap))
        {
            if (attackMap.TryGetValue(defenseType, out float multiplier))
            {
                return multiplier;
            }
        }

        return BattleConstants.NORMAL_EFFECTIVE;
    }
    
    private static void LogStatInfo(StringBuilder log, PokemonStats caster, PokemonStats target, 
        AttackMoveSO move, float finalAttack, float finalDefense)
    {
        string atkStatName = move.attackType == AttackType.Physical ? "Attack" : "Sp. Attack";
        string defStatName = move.attackType == AttackType.Physical ? "Defense" : "Sp. Defense";
        
        StageType atkStageType = move.attackType == AttackType.Physical ? StageType.Attack : StageType.SpAttack;
        StageType defStageType = move.attackType == AttackType.Physical ? StageType.Defense : StageType.SpDefense;
        
        int atkStage = caster.BattleStages.GetStage(atkStageType);
        int defStage = target.BattleStages.GetStage(defStageType);

        log.AppendLine($"Caster's {atkStatName} (Stage {atkStage:+0;-#}): {finalAttack:F2}");
        log.AppendLine($"Target's {defStatName} (Stage {defStage:+0;-#}): {finalDefense:F2}");
    }
}