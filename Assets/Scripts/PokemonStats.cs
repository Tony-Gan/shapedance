using UnityEngine;

public class PokemonStats : MonoBehaviour
{
    [Header("Pokemon")]
    public PokemonSO pokemon;
    public string alias;
    public Gender gender = Gender.NA;
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
    }

    public MoveBaseSO[] GetMoves()
    {
        return moves;
    }
}