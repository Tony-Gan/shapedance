using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Move", menuName = "Pokemon/New Attack Move")]
public class AttackMoveSO : MoveBaseSO
{
    [Header("Attack Details")]
    public AttackType attackType;
    public ElementType elementType;
    public int power;
    
    [Header("Critical Hit")]
    [Range(0, 3)]
    [Tooltip("Critical hit stage. 0=4.17%, 1=12.5%, 2=50%, 3=100%")]
    public int criticalLevel = 0;
}