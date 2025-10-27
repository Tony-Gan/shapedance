using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Move", menuName = "Pokemon/New Attack Move")]
public class AttackMoveSO : MoveBaseSO
{
    [Header("Attack Details")]
    public AttackType attackType;
    public ElementType elementType;
    public int power;
    [Range(-6, 6)]
    public int accuracyLevel = 0;
    [Range(-6, 6)]
    public int criticalLevel = 0;
}