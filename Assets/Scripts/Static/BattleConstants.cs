using UnityEngine;

public static class BattleConstants
{
    #region Damage Calculation
    public const float STAB_MULTIPLIER = 1.5f;
    
    public const float CRITICAL_MULTIPLIER = 1.5f;
    
    public const float DAMAGE_RANDOM_MIN = 0.85f;
    
    public const float DAMAGE_RANDOM_MAX = 1.0f;
    
    public const int MIN_DAMAGE = 1;
    #endregion

    #region Critical Hit Rates
    public const float CRIT_RATE_STAGE_0 = 1f / 24f;  // ~4.17%
    
    public const float CRIT_RATE_STAGE_1 = 1f / 8f;   // 12.5%
    
    public const float CRIT_RATE_STAGE_2 = 1f / 2f;   // 50%
    
    public const float CRIT_RATE_STAGE_3_PLUS = 1f;   // 100%
    
    public const int MAX_CRIT_STAGE = 4;
    #endregion

    #region Stat Stages
    public const int MIN_STAT_STAGE = -6;
    
    public const int MAX_STAT_STAGE = 6;
    
    public const int NEUTRAL_STAT_STAGE = 0;
    #endregion

    #region Level System
    public const int MIN_LEVEL = 1;
    
    public const int MAX_LEVEL = 100;
    
    public const int EXP_PER_LEVEL = 100;
    #endregion

    #region EV/IV Limits
    public const int MAX_EV_PER_STAT = 252;
    
    public const int MAX_TOTAL_EV = 510;
    
    public const int MAX_IV = 31;
    #endregion

    #region Accuracy System
    public const float DEFAULT_ACCURACY = 100f;
    #endregion

    #region Type Effectiveness
    public const float SUPER_EFFECTIVE = 2f;
    
    public const float NORMAL_EFFECTIVE = 1f;
    
    public const float NOT_VERY_EFFECTIVE = 0.5f;
    
    public const float NO_EFFECT = 0f;
    
    public const float SUPER_EFFECTIVE_THRESHOLD = 1.5f;
    
    public const float NOT_VERY_EFFECTIVE_THRESHOLD = 0.7f;
    #endregion

    #region Nature Modifiers
    public const float NATURE_BOOST = 1.1f;
    
    public const float NATURE_PENALTY = 0.9f;
    
    public const float NATURE_NEUTRAL = 1.0f;
    #endregion

    #region Battle Messages
    public const string MSG_CRITICAL_HIT = "A CRITICAL HIT!";
    public const string MSG_SUPER_EFFECTIVE = "It's super effective!";
    public const string MSG_NOT_VERY_EFFECTIVE = "It's not very effective...";
    public const string MSG_NO_EFFECT = "It had no effect!";
    public const string MSG_FAINTED = "fainted!";
    public const string MSG_MISSED = "missed!";
    #endregion

    #region Stage Multiplier Arrays
    public static readonly float[] AccuracyStageMultipliers = new float[]
    {
        3f/9f, 3f/8f, 3f/7f, 3f/6f, 3f/5f, 3f/4f, 3f/3f,  // -6 to 0
        4f/3f, 5f/3f, 6f/3f, 7f/3f, 8f/3f, 9f/3f           // +1 to +6
    };
    
    public static readonly float[] StatStageMultipliers = new float[]
    {
        2f/8f, 2f/7f, 2f/6f, 2f/5f, 2f/4f, 2f/3f, 2f/2f,  // -6 to 0
        3f/2f, 4f/2f, 5f/2f, 6f/2f, 7f/2f, 8f/2f           // +1 to +6
    };
    #endregion

    #region Helper Methods
    public static float GetCriticalHitRate(int critStage)
    {
        return critStage switch
        {
            0 => CRIT_RATE_STAGE_0,
            1 => CRIT_RATE_STAGE_1,
            2 => CRIT_RATE_STAGE_2,
            >= 3 => CRIT_RATE_STAGE_3_PLUS,
            _ => CRIT_RATE_STAGE_0
        };
    }
    
    public static float GetStatStageMultiplier(int stage)
    {
        int index = Mathf.Clamp(stage, MIN_STAT_STAGE, MAX_STAT_STAGE) + 6;
        return StatStageMultipliers[index];
    }
    
    public static float GetAccuracyStageMultiplier(int stage)
    {
        int index = Mathf.Clamp(stage, MIN_STAT_STAGE, MAX_STAT_STAGE) + 6;
        return AccuracyStageMultipliers[index];
    }
    
    public static string GetEffectivenessMessage(float effectiveness)
    {
        if (effectiveness >= SUPER_EFFECTIVE_THRESHOLD)
            return MSG_SUPER_EFFECTIVE;
        if (effectiveness <= NO_EFFECT)
            return MSG_NO_EFFECT;
        if (effectiveness <= NOT_VERY_EFFECTIVE_THRESHOLD)
            return MSG_NOT_VERY_EFFECTIVE;
        return string.Empty;
    }
    #endregion
}