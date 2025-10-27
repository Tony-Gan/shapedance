using UnityEngine;

public abstract class MoveEffectSO : ScriptableObject
{
    [Header("Effect Base")]
    [Range(0f, 1f)]
    public float probability = 1.0f;
    public abstract void Execute(PokemonStats caster, PokemonStats target);
}