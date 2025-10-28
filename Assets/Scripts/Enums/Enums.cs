public enum Gender
{
    M,
    F,
    NA
}

public enum ElementType
{
    None,
    Normal,
    Fire,
    Water,
    Grass,
    Electric,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum AttackType
{
    Physical,
    Special
}

public enum TargetType
{
    SingleTarget,
    MultipleTarget,
    Cone,
    Circle
}

public enum Target
{
    Self,
    Space,
    AllyOther,
    AllyTeam,
    Enemy,
    Creature
}

public enum StatusCondition
{
    None,
    Burn,
    Poison,
    BadlyPoisoned,
    Paralyze,
    Freeze,
    Sleep,
    Confusion
}

public enum WeatherType
{
    None,
    Sunny,
    Rain,
    Sandstorm,
    Hail, 
    HarshSunlight,
    HeavyRain,
    StrongWinds 
}

public enum TerrainType
{
    None,
    Electric,
    Grassy,
    Misty,
    Psychic
}

public enum HealType
{
    FixedAmount,
    PercentageOfMaxHP,
    PercentageOfDamageDealt
}

public enum StatType
{
    HP,
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}

public enum StageType
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Accuracy,
    Evasion,
    Critical
}

public enum PokemonNature
{
    Hardy,
    Docile,
    Serious,
    Bashful,
    Quirky,
    Lonely,
    Brave,
    Adamant,
    Naughty,
    Bold,
    Relaxed,
    Impish,
    Lax,
    Timid,
    Hasty,
    Jolly,
    Naive,
    Modest,
    Mild,
    Quiet,
    Rash,
    Calm,
    Gentle,
    Sassy,
    Careful
}