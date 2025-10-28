using UnityEngine;

[CreateAssetMenu(fileName = "New Recoil Effect", menuName = "Pokemon/Move Effect/Recoil")]
public class RecoilEffectSO : MoveEffectSO
{
    [Header("Recoil Details")]
    [Range(0f, 1f)]
    public float recoilPercentage = 0.25f;
    
    public int fixedRecoilAmount = 0;
    
    [Header("Damage Context")]
    [System.NonSerialized]
    public int lastDamageDealt = 0;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        if (caster == null)
        {
            Debug.LogWarning("RecoilEffectSO: Caster is null!");
            return;
        }

        if (lastDamageDealt <= 0)
        {
            return;
        }

        int recoilDamage = fixedRecoilAmount > 0 
            ? fixedRecoilAmount 
            : Mathf.FloorToInt(lastDamageDealt * recoilPercentage);

        if (recoilDamage <= 0)
        {
            return;
        }

        caster.TakeDamage(recoilDamage);
        
        Debug.Log($"[Effect] {caster.alias} is hit by recoil! ({recoilDamage} damage)");
        
        if (caster.IsFainted())
        {
            Debug.Log($"[Effect] {caster.alias} fainted from recoil!");
        }
    }

    public void SetDamageContext(int damageDealt)
    {
        lastDamageDealt = damageDealt;
    }
}