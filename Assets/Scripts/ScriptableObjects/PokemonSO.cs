using UnityEngine;

[CreateAssetMenu(fileName = "New Pokemon", menuName = "Pokemon/New Pokemon")]
public class PokemonSO : ScriptableObject
{
    public int pokedexNumber;

    [Header("Names")]
    public string monsterName = "English Name";
    public string monsterNameCN = "中文名";

    [Header("Types")]
    public ElementType type1 = ElementType.None;
    public ElementType type2 = ElementType.None;

    [Header("Base Stats")]
    public int baseHP;
    public int baseAttack;
    public int baseDefense;
    public int baseSpAttack;
    public int baseSpDefense;
    public int baseSpeed;
}