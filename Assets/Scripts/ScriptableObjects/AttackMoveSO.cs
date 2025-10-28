using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Move", menuName = "Pokemon/New Attack Move")]
public class AttackMoveSO : MoveBaseSO
{
    [Header("Attack Details")]
    public AttackType attackType;
    public ElementType elementType;
    public int power;
    
    [Header("Accuracy")]
    [Range(0, 100)]
    public int accuracy = 100;
    
    [Range(-6, 6)]
    public int accuracyLevel = 0;
    
    [Header("Critical Hit")]
    [Range(0, 3)]
    [Tooltip("Critical hit stage. 0=4.17%, 1=12.5%, 2=50%, 3=100%")]
    public int criticalLevel = 0;
}