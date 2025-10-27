using UnityEngine;
using System.Collections.Generic;
using System.Text;

public static class BattleCalculator
{
    private static readonly float[] accuracyStageModifiers = new float[]
    {
        3f/9f, 3f/8f, 3f/7f, 3f/6f, 3f/5f, 3f/4f, 3f/3f,
        4f/3f, 5f/3f, 6f/3f, 7f/3f, 8f/3f, 9f/3f
    };
    
    private static readonly float[] statStageModifiers = new float[]
    {
        2f/8f, 2f/7f, 2f/6f, 2f/5f, 2f/4f, 2f/3f, 2f/2f,
        3f/2f, 4f/2f, 5f/2f, 6f/2f, 7f/2f, 8f/2f 
    };

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

    public static void HandleAttack(PokemonStats caster, PokemonStats target, AttackMoveSO move)
    {
        if (!CalculateHit(caster, target, move))
        {
           
            Debug.Log($"[Attack Result]: {caster.alias}'s {move.moveName} missed {target.alias}!");
            return;
        }

        StringBuilder logBuilder = new();
        logBuilder.AppendLine($"[Attack Result]: {caster.alias}'s {move.moveName}!");
        logBuilder.AppendLine("========================================");
        logBuilder.AppendLine($"Caster: {caster.alias} (Lvl {caster.level})");
        logBuilder.AppendLine($"Target: {target.alias} (Lvl {target.level})");
        logBuilder.AppendLine($"Move: {move.moveName} (Power: {move.power}, Type: {move.elementType}, Category: {move.attackType})");

        bool isCritical = CalculateCritical(caster, move);
        if (isCritical)
        {
            logBuilder.AppendLine("A CRITICAL HIT!");
        }

        float typeEffectiveness = GetTypeEffectiveness(move.elementType, target.pokemon.type1);
        if (target.pokemon.type2 != ElementType.None)
        {
            typeEffectiveness *= GetTypeEffectiveness(move.elementType, target.pokemon.type2);
        }

        if (typeEffectiveness > 1.5f) logBuilder.AppendLine($"It's super effective! (x{typeEffectiveness})");
        else if (typeEffectiveness > 0 && typeEffectiveness < 0.7f) logBuilder.AppendLine($"It's not very effective... (x{typeEffectiveness})");
        else if (typeEffectiveness == 0f) logBuilder.AppendLine($"It had no effect! (x{typeEffectiveness})");
        else logBuilder.AppendLine($"Type Effectiveness: x{typeEffectiveness}");

        var (damage, finalAttack, finalDefense) = CalculateDamage(caster, target, move, isCritical, typeEffectiveness);

        string atkStatName = move.attackType == AttackType.Physical ? "Attack" : "Sp. Attack";
        string defStatName = move.attackType == AttackType.Physical ? "Defense" : "Sp. Defense";
        int atkStage = move.attackType == AttackType.Physical ? caster.AttackStage : caster.SpAttackStage;
        int defStage = move.attackType == AttackType.Physical ? target.DefenseStage : target.SpDefenseStage;

        logBuilder.AppendLine($"Caster's {atkStatName} (Stage {atkStage}): {finalAttack:F2}");
        logBuilder.AppendLine($"Target's {defStatName} (Stage {defStage}): {finalDefense:F2}");
        
        logBuilder.AppendLine("----------------------------------------");
        logBuilder.AppendLine($"Final Damage: {damage}");

        target.TakeDamage(damage);

        logBuilder.AppendLine($"Target HP: {target.CurrentHP} / {target.GetStat(StatType.HP)}"); 

        if (target.IsFainted())
        {
            logBuilder.AppendLine($"{target.alias} has fainted!"); 
        }

        Debug.Log(logBuilder.ToString());
    }

    private static bool CalculateHit(PokemonStats caster, PokemonStats target, AttackMoveSO move)
    {
        float moveBaseAccuracy = 100f;
        
        int totalAccuracyStage = caster.AccuracyStage + move.accuracyLevel - target.EvasionStage;
        int stageIndex = Mathf.Clamp(totalAccuracyStage, -6, 6) + 6;
        float modifier = accuracyStageModifiers[stageIndex];
        float finalAccuracy = moveBaseAccuracy * modifier;
        
        return Random.Range(0f, 100f) < finalAccuracy;
    }

    private static bool CalculateCritical(PokemonStats caster, AttackMoveSO move)
    {
       
        int totalCritStage = caster.CritStage + move.criticalLevel;
        totalCritStage = Mathf.Clamp(totalCritStage, 0, 4);
        float critChance = totalCritStage switch
        {
            0 => 1f / 24f,
            1 => 1f / 8f,
            2 => 1f / 2f,
            3 => 1f,
            _ => 1f,
        };
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
            
           
            int attackStageIndex = Mathf.Clamp(caster.AttackStage, -6, 6) + 6;
            int defenseStageIndex = Mathf.Clamp(target.DefenseStage, -6, 6) + 6;

            attackStat *= statStageModifiers[attackStageIndex];
            defenseStat *= statStageModifiers[defenseStageIndex];
        }
        else
        {
            attackStat = caster.GetStat(StatType.SpAttack);
            defenseStat = target.GetStat(StatType.SpDefense);

            int attackStageIndex = Mathf.Clamp(caster.SpAttackStage, -6, 6) + 6;
            int defenseStageIndex = Mathf.Clamp(target.SpDefenseStage, -6, 6) + 6;

            attackStat *= statStageModifiers[attackStageIndex];
            defenseStat *= statStageModifiers[defenseStageIndex];
        }

        float baseDamage = (((2f * level / 5f) + 2f) * power * (attackStat / defenseStat)  / 50f) + 2f;
        float modifier = 1.0f;
        if (isCritical)
        {
            modifier *= 1.5f;
        }

       
        if (caster.pokemon.type1 == move.elementType || caster.pokemon.type2 == move.elementType) 
        {
            modifier *= 1.5f;
        }

        modifier *= typeEffectiveness;
        
        modifier *= Random.Range(0.85f, 1.0f);
        
        int finalDamage = Mathf.FloorToInt(baseDamage * modifier);

        if (typeEffectiveness == 0)
        {
            return (0, attackStat, defenseStat);
        }
        return (Mathf.Max(1, finalDamage), attackStat, defenseStat);
    }
    
    private static float GetTypeEffectiveness(ElementType attackType, ElementType defenseType)
    {
        if (attackType == ElementType.None || defenseType == ElementType.None)
        {
            return 1f;
        }

       
        if (TypeChart.TryGetValue(attackType, out var attackMap))
        {
            if (attackMap.TryGetValue(defenseType, out float multiplier))
            {
                return multiplier;
            }
        }

        return 1f;
    }
}