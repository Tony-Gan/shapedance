using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Move", menuName = "Pokemon/New Attack Move")]
public class AttackMoveSO : MoveBaseSO
{
    [Header("Attack Details")]
    public AttackType attackType;
    public ElementType elementType;
    public int power;
    public int accuracy;
    public int criticalLevel;
    public int range;
    public TargetType targetType;
}