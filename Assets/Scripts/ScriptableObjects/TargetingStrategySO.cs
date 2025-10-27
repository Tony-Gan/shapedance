using UnityEngine;

public abstract class TargetingStrategySO : ScriptableObject
{
    [Header("Target Validation")]
    public Target target;

    public abstract void Initiate(TargetingManager manager, PokemonStats caster, MoveBaseSO move);
    public abstract void HandleClick(TargetingManager manager, Vector3 mouseWorldPos, RaycastHit2D hit);
    public abstract void HandleRightClick(TargetingManager manager, Vector3 mouseWorldPos, RaycastHit2D hit);
    public abstract void UpdateIndicator(TargetingManager manager, PokemonStats caster);
}