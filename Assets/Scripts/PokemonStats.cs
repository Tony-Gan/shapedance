using UnityEngine;
using System.Collections.Generic;

public class PokemonStats : MonoBehaviour
{
    [Header("Pokemon")]
    public PokemonSO pokemon;
    public string alias;
    public Gender gender = Gender.NA;
    public PokemonNature nature = PokemonNature.Hardy;
    [SerializeField] private MoveBaseSO[] moves = new MoveBaseSO[6];

    [Range(1, 100)]
    public int level = 5;

    [Header("EV")]
    [Range(0, 252)]
    public int evHP;
    [Range(0, 252)]
    public int evAttack;
    [Range(0, 252)]
    public int evDefense;
    [Range(0, 252)]
    public int evSpAttack;
    [Range(0, 252)]
    public int evSpDefense;
    [Range(0, 252)]
    public int evSpeed;

    [Header("IV")]
    [Range(0, 31)]
    public int ivHP;
    [Range(0, 31)]
    public int ivAttack;
    [Range(0, 31)]
    public int ivDefense;
    [Range(0, 31)]
    public int ivSpAttack;
    [Range(0, 31)]
    public int ivSpDefense;
    [Range(0, 31)]
    public int ivSpeed;
    
    [Header("Current Status")]
    [SerializeField] private int _currentHP;
    [SerializeField] private int _currentEXP;
    [SerializeField][Range(0, 6)] private int _critStage = 0;
    [SerializeField][Range(0, 6)] private int _accuracyStage = 0;
    [SerializeField][Range(0, 6)] private int _evasionStage = 0;

    [Header("Calculated Stats")]
    [SerializeField] private Dictionary<StatType, int> currentStats = new();

    [Space(10)]
    [Header("Debug: Calculated Stats View")]
    [SerializeField] private List<StatType> _debugStatKeys = new();
    [SerializeField] private List<int> _debugStatValues = new();
    
    
    public int CurrentHP
    {
        get => _currentHP;
        set
        {
            _currentHP = Mathf.Clamp(value, 0, GetStat(StatType.HP));
        }
    }
    
    public int CurrentEXP
    {
        get => _currentEXP;
        set
        {
            if (level >= 100)
            {
                _currentEXP = Mathf.Clamp(value, 0, 100);
                return;
            }

            _currentEXP = Mathf.Max(0, value);

            while (_currentEXP >= 100 && level < 100)
            {
                _currentEXP -= 100;
                level++;
                RecalculateStats(); 
            }
        }
    }
    
    public int CritStage
    {
        get => _critStage;
        set
        {
            _critStage = Mathf.Clamp(value, 0, 6);
        }
    }
    
    public int AccuracyStage
    {
        get => _accuracyStage;
        set
        {
            _accuracyStage = Mathf.Clamp(value, 0, 6);
        }
    }
    
    public int EvasionStage
    {
        get => _evasionStage;
        set
        {
            _evasionStage = Mathf.Clamp(value, 0, 6);
        }
    }


    void Awake()
    {
        if (pokemon == null)
        {
            Debug.LogError("FATAL: No Pokemon Stats Found!", gameObject);

            enabled = false;
            return;
        }

        if (string.IsNullOrEmpty(alias))
        {
            alias = pokemon.monsterNameCN;
        }

        RecalculateStats();
        
        CurrentHP = GetStat(StatType.HP);
    }

    public int GetStat(StatType stat)
    {
        if (currentStats.TryGetValue(stat, out int value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"Can't find {stat}, recalculating...", this);
            RecalculateStats();
            
            if (currentStats.TryGetValue(stat, out int newValue))
            {
                return newValue;
            }
            else
            {
                Debug.LogError($"Recalculate FAILED to find {stat}!", this);
                return 0;
            }
        }
    }

    [ContextMenu("Recalculate Stats Now")]
    public void RecalculateStats()
    {
        if (pokemon == null) 
        {
            Debug.LogWarning("PokemonSO is null, cannot calculate stats.");
            return;
        }
        
        currentStats ??= new();

        bool hasOldHP = currentStats.TryGetValue(StatType.HP, out int oldMaxHP);

        int hpBase = pokemon.baseHP;
        int hpEV = Mathf.FloorToInt(evHP / 4f);
        int hpCalc = Mathf.FloorToInt((2 * hpBase + ivHP + hpEV) * level / 100f) + level + 10;
        currentStats[StatType.HP] = hpCalc;

        currentStats[StatType.Attack] = CalculateStat(StatType.Attack, pokemon.baseAttack, ivAttack, evAttack);
        currentStats[StatType.Defense] = CalculateStat(StatType.Defense, pokemon.baseDefense, ivDefense, evDefense);
        currentStats[StatType.SpAttack] = CalculateStat(StatType.SpAttack, pokemon.baseSpAttack, ivSpAttack, evSpAttack);
        currentStats[StatType.SpDefense] = CalculateStat(StatType.SpDefense, pokemon.baseSpDefense, ivSpDefense, evSpDefense);
        currentStats[StatType.Speed] = CalculateStat(StatType.Speed, pokemon.baseSpeed, ivSpeed, evSpeed);

        int newMaxHP = currentStats[StatType.HP];

        if (hasOldHP && newMaxHP != oldMaxHP)
        {
            int hpDelta = newMaxHP - oldMaxHP;
            _currentHP += hpDelta; 
        }
        
        CurrentHP = _currentHP; 

        UpdateDebugLists();
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    private void UpdateDebugLists()
    {
        _debugStatKeys ??= new();
        _debugStatValues ??= new();
            
        _debugStatKeys.Clear();
        _debugStatValues.Clear();

        if (currentStats == null) return;
        
        foreach (var pair in currentStats)
        {
            _debugStatKeys.Add(pair.Key);
            _debugStatValues.Add(pair.Value);
        }
    }

    private int CalculateStat(StatType statType, int baseStat, int iv, int ev)
    {
        int evCalc = Mathf.FloorToInt(ev / 4f);
        float natureMod = GetNatureModifier(statType);
        
        int calculatedStat = Mathf.FloorToInt(
            (Mathf.FloorToInt((2 * baseStat + iv + evCalc) * level / 100f) + 5) * natureMod
        );

        return calculatedStat;
    }

    private float GetNatureModifier(StatType stat)
    {
        switch (nature)
        {
            // +Atk
            case PokemonNature.Lonely:
                if (stat == StatType.Attack) return 1.1f;
                if (stat == StatType.Defense) return 0.9f;
                break;
            case PokemonNature.Brave:
                if (stat == StatType.Attack) return 1.1f;
                if (stat == StatType.Speed) return 0.9f;
                break;
            case PokemonNature.Adamant:
                if (stat == StatType.Attack) return 1.1f;
                if (stat == StatType.SpAttack) return 0.9f;
                break;
            case PokemonNature.Naughty:
                if (stat == StatType.Attack) return 1.1f;
                if (stat == StatType.SpDefense) return 0.9f;
                break;

            // +Def
            case PokemonNature.Bold:
                if (stat == StatType.Defense) return 1.1f;
                if (stat == StatType.Attack) return 0.9f;
                break;
            case PokemonNature.Relaxed:
                if (stat == StatType.Defense) return 1.1f;
                if (stat == StatType.Speed) return 0.9f;
                break;
            case PokemonNature.Impish:
                if (stat == StatType.Defense) return 1.1f;
                if (stat == StatType.SpAttack) return 0.9f;
                break;
            case PokemonNature.Lax:
                if (stat == StatType.Defense) return 1.1f;
                if (stat == StatType.SpDefense) return 0.9f;
                break;

            // +Speed
            case PokemonNature.Timid:
                if (stat == StatType.Speed) return 1.1f;
                if (stat == StatType.Attack) return 0.9f;
                break;
            case PokemonNature.Hasty:
                if (stat == StatType.Speed) return 1.1f;
                if (stat == StatType.Defense) return 0.9f;
                break;
            case PokemonNature.Jolly:
                if (stat == StatType.Speed) return 1.1f;
                if (stat == StatType.SpAttack) return 0.9f;
                break;
            case PokemonNature.Naive:
                if (stat == StatType.Speed) return 1.1f;
                if (stat == StatType.SpDefense) return 0.9f;
                break;

            // +SpAtk
            case PokemonNature.Modest:
                if (stat == StatType.SpAttack) return 1.1f;
                if (stat == StatType.Attack) return 0.9f;
                break;
            case PokemonNature.Mild:
                if (stat == StatType.SpAttack) return 1.1f;
                if (stat == StatType.Defense) return 0.9f;
                break;
            case PokemonNature.Quiet:
                if (stat == StatType.SpAttack) return 1.1f;
                if (stat == StatType.Speed) return 0.9f;
                break;
            case PokemonNature.Rash:
                if (stat == StatType.SpAttack) return 1.1f;
                if (stat == StatType.SpDefense) return 0.9f;
                break;

            // +SpDef
            case PokemonNature.Calm:
                if (stat == StatType.SpDefense) return 1.1f;
                if (stat == StatType.Attack) return 0.9f;
                break;
            case PokemonNature.Gentle:
                if (stat == StatType.SpDefense) return 1.1f;
                if (stat == StatType.Defense) return 0.9f;
                break;
            case PokemonNature.Sassy:
                if (stat == StatType.SpDefense) return 1.1f;
                if (stat == StatType.Speed) return 0.9f;
                break;
            case PokemonNature.Careful:
                if (stat == StatType.SpDefense) return 1.1f;
                if (stat == StatType.SpAttack) return 0.9f;
                break;
        }

        return 1.0f;
    }

    public MoveBaseSO[] GetMoves()
    {
        return moves;
    }
}