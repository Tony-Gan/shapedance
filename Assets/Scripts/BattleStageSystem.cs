using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BattleStageSystem
{
    private Dictionary<StageType, int> stages = new();
    
    public int GetStage(StageType type)
    {
        return stages.TryGetValue(type, out int value) ? value : BattleConstants.NEUTRAL_STAT_STAGE;
    }
    
    public void SetStage(StageType type, int value)
    {
        stages[type] = Mathf.Clamp(value, BattleConstants.MIN_STAT_STAGE, BattleConstants.MAX_STAT_STAGE);
    }
    
    public void ModifyStage(StageType type, int delta)
    {
        int currentStage = GetStage(type);
        SetStage(type, currentStage + delta);
    }
    
    public void ResetAll()
    {
        stages.Clear();
    }
    
    public void Reset(StageType type)
    {
        if (stages.ContainsKey(type))
        {
            stages[type] = BattleConstants.NEUTRAL_STAT_STAGE;
        }
    }
    
    public bool IsMaxed(StageType type)
    {
        return GetStage(type) >= BattleConstants.MAX_STAT_STAGE;
    }
    
    public bool IsMinimized(StageType type)
    {
        return GetStage(type) <= BattleConstants.MIN_STAT_STAGE;
    }
    
    public float GetMultiplier(StageType type)
    {
        return BattleConstants.GetStatStageMultiplier(GetStage(type));
    }
    
    public Dictionary<StageType, int> GetModifiedStages()
    {
        Dictionary<StageType, int> modified = new();
        foreach (var kvp in stages)
        {
            if (kvp.Value != BattleConstants.NEUTRAL_STAT_STAGE)
            {
                modified[kvp.Key] = kvp.Value;
            }
        }
        return modified;
    }
    
    public bool TryModifyStage(StageType type, int delta, out string message)
    {
        int beforeStage = GetStage(type);
        ModifyStage(type, delta);
        int afterStage = GetStage(type);
        
        int actualChange = afterStage - beforeStage;
        
        if (actualChange == 0)
        {
            if (delta > 0)
                message = $"{type} won't go any higher!";
            else
                message = $"{type} won't go any lower!";
            return false;
        }
        
        string verb = delta > 0 ? "rose" : "fell";
        message = $"{type} {verb} by {Mathf.Abs(actualChange)}!";
        return true;
    }
    
    public override string ToString()
    {
        if (stages.Count == 0) return "All stages at 0";
        
        System.Text.StringBuilder sb = new();
        sb.Append("Stages: ");
        foreach (var kvp in stages)
        {
            if (kvp.Value != 0)
            {
                sb.Append($"{kvp.Key}:{kvp.Value:+0;-#} ");
            }
        }
        return sb.ToString().TrimEnd();
    }
}

public static class BattleStageExtensions
{
    public static void Increase(this BattleStageSystem system, StageType type, int amount = 1)
    {
        system.ModifyStage(type, Mathf.Abs(amount));
    }
    
    public static void Decrease(this BattleStageSystem system, StageType type, int amount = 1)
    {
        system.ModifyStage(type, -Mathf.Abs(amount));
    }
}