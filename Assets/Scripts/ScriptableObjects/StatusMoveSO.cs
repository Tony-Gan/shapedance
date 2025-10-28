using UnityEngine;

[CreateAssetMenu(fileName = "New Status Move", menuName = "Pokemon/New Status Move")]
public class StatusMoveSO : MoveBaseSO
{    
    [Header("Type (Optional)")]
    public ElementType elementType = ElementType.None;
}