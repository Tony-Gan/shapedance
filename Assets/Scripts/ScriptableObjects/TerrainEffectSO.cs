using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Effect", menuName = "Pokemon/Move Effect/Terrain")]
public class TerrainEffectSO : MoveEffectSO
{
    [Header("Terrain Details")]
    public TerrainType terrainType = TerrainType.None;
    public int duration = 5;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        // TODO: 实现 BattleFieldManager 后，调用设置场地的方法
        // BattleFieldManager.Instance.SetTerrain(terrainType, duration);
        
        Debug.Log($"[Effect] {GetTerrainMessage(terrainType)}");
    }

    private string GetTerrainMessage(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Electric => "An electric current ran across the battlefield!",
            TerrainType.Grassy => "Grass grew to cover the battlefield!",
            TerrainType.Misty => "Mist swirled around the battlefield!",
            TerrainType.Psychic => "The battlefield got weird!",
            TerrainType.None => "The terrain returned to normal!",
            _ => $"Terrain changed to {terrain}!"
        };
    }
}