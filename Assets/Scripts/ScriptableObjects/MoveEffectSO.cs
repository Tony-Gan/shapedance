using UnityEngine;

public abstract class MoveEffectSO : ScriptableObject
{
    [Header("Effect Logic")]
    public string description;

    public abstract void ApplyEffect(PokemonStats caster, PokemonStats target);
}