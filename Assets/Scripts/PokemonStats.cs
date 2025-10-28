using UnityEngine;
using System.Collections.Generic;

public class PokemonStats : MonoBehaviour
{
    #region Pokemon Base Info
    [Header("Pokemon")]
    public PokemonSO pokemon;
    public string alias;
    public Gender gender = Gender.NA;
    public PokemonNature nature = PokemonNature.Hardy;
    [SerializeField] private MoveBaseSO[] moves = new MoveBaseSO[4];

    [Range(1, 100)]
    public int level = 5;
    #endregion

    #region EVs and IVs
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
    #endregion

    #region Current Status
    [Header("Current Status")]
    [SerializeField] private int _currentHP;
    [SerializeField] private int _currentEXP;
    
    [Header("Battle Stages")]
    [SerializeField] private BattleStageSystem battleStages = new();
    
    public BattleStageSystem BattleStages => battleStages;
    #endregion

    #region Calculated Stats
    [Header("Calculated Stats")]
    private Dictionary<StatType, int> currentStats = new();

    [Space(10)]
    [Header("Debug: Calculated Stats View")]
    [SerializeField] private List<StatType> _debugStatKeys = new();
    [SerializeField] private List<int> _debugStatValues = new();
    #endregion

    #region Nature Modifier Lookup Table
    private static readonly Dictionary<(PokemonNature, StatType), float> NatureModifiers = new()
    {
        // +Atk Natures
        [(PokemonNature.Lonely, StatType.Attack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Lonely, StatType.Defense)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Brave, StatType.Attack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Brave, StatType.Speed)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Adamant, StatType.Attack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Adamant, StatType.SpAttack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Naughty, StatType.Attack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Naughty, StatType.SpDefense)] = BattleConstants.NATURE_PENALTY,

        // +Def Natures
        [(PokemonNature.Bold, StatType.Defense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Bold, StatType.Attack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Relaxed, StatType.Defense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Relaxed, StatType.Speed)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Impish, StatType.Defense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Impish, StatType.SpAttack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Lax, StatType.Defense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Lax, StatType.SpDefense)] = BattleConstants.NATURE_PENALTY,

        // +Speed Natures
        [(PokemonNature.Timid, StatType.Speed)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Timid, StatType.Attack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Hasty, StatType.Speed)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Hasty, StatType.Defense)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Jolly, StatType.Speed)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Jolly, StatType.SpAttack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Naive, StatType.Speed)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Naive, StatType.SpDefense)] = BattleConstants.NATURE_PENALTY,

        // +SpAtk Natures
        [(PokemonNature.Modest, StatType.SpAttack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Modest, StatType.Attack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Mild, StatType.SpAttack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Mild, StatType.Defense)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Quiet, StatType.SpAttack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Quiet, StatType.Speed)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Rash, StatType.SpAttack)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Rash, StatType.SpDefense)] = BattleConstants.NATURE_PENALTY,

        // +SpDef Natures
        [(PokemonNature.Calm, StatType.SpDefense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Calm, StatType.Attack)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Gentle, StatType.SpDefense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Gentle, StatType.Defense)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Sassy, StatType.SpDefense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Sassy, StatType.Speed)] = BattleConstants.NATURE_PENALTY,
        [(PokemonNature.Careful, StatType.SpDefense)] = BattleConstants.NATURE_BOOST,
        [(PokemonNature.Careful, StatType.SpAttack)] = BattleConstants.NATURE_PENALTY,
    };
    #endregion

    #region Properties - HP and EXP
    public int CurrentHP
    {
        get => _currentHP;
        set => _currentHP = Mathf.Clamp(value, 0, GetStat(StatType.HP));
    }
    
    public int CurrentEXP
    {
        get => _currentEXP;
        set
        {
            if (level >= BattleConstants.MAX_LEVEL)
            {
                _currentEXP = Mathf.Clamp(value, 0, BattleConstants.EXP_PER_LEVEL);
                return;
            }

            _currentEXP = Mathf.Max(0, value);

            while (_currentEXP >= BattleConstants.EXP_PER_LEVEL && level < BattleConstants.MAX_LEVEL)
            {
                _currentEXP -= BattleConstants.EXP_PER_LEVEL;
                LevelUp(); 
            }
        }
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int damage)
    {
        if (damage < 0) return;
        CurrentHP -= damage; 
    }
    
    public bool IsFainted()
    {
        return CurrentHP == 0;
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

    public MoveBaseSO[] GetMoves()
    {
        return moves;
    }
    #endregion

    #region Context Menu Commands
    [ContextMenu("Rest (Heal and Reset Stages)")]
    public void Rest()
    {
        CurrentHP = GetStat(StatType.HP);
        battleStages.ResetAll();
        
        Debug.Log($"[Debug]: {alias} has been fully rested. HP restored and stages reset.", this);
    }
    
    [ContextMenu("Level Up (+1)")]
    public void LevelUp()
    {
        if (level >= BattleConstants.MAX_LEVEL)
        {
            Debug.Log($"[Debug]: {alias} is already at max level ({BattleConstants.MAX_LEVEL}).", this);
            return;
        }

        level++;
        Debug.Log($"[Debug]: {alias} leveled up to Level {level}!", this);
        
        RecalculateStats();
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

        // HP 计算（特殊公式）
        int hpBase = pokemon.baseHP;
        int hpEV = Mathf.FloorToInt(evHP / 4f);
        int hpCalc = Mathf.FloorToInt((2 * hpBase + ivHP + hpEV) * level / 100f) + level + 10;
        currentStats[StatType.HP] = hpCalc;

        // 其他属性计算
        currentStats[StatType.Attack] = CalculateStat(StatType.Attack, pokemon.baseAttack, ivAttack, evAttack);
        currentStats[StatType.Defense] = CalculateStat(StatType.Defense, pokemon.baseDefense, ivDefense, evDefense);
        currentStats[StatType.SpAttack] = CalculateStat(StatType.SpAttack, pokemon.baseSpAttack, ivSpAttack, evSpAttack);
        currentStats[StatType.SpDefense] = CalculateStat(StatType.SpDefense, pokemon.baseSpDefense, ivSpDefense, evSpDefense);
        currentStats[StatType.Speed] = CalculateStat(StatType.Speed, pokemon.baseSpeed, ivSpeed, evSpeed);

        int newMaxHP = currentStats[StatType.HP];

        // 升级时调整当前HP
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
    #endregion

    #region Private Methods
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
            alias = pokemon.pokemonNameCN;
        }

        RecalculateStats();
        CurrentHP = GetStat(StatType.HP);
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

    /// <summary>
    /// ✅ 优化：使用查找表而不是巨大的 switch-case
    /// </summary>
    private float GetNatureModifier(StatType stat)
    {
        return NatureModifiers.TryGetValue((nature, stat), out float modifier) 
            ? modifier 
            : BattleConstants.NATURE_NEUTRAL;
    }
    #endregion
}