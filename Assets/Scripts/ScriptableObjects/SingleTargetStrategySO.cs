using UnityEngine;

[CreateAssetMenu(fileName = "New Single Target Strategy", menuName = "Pokemon/Targeting/Single Target")]
public class SingleTargetStrategySO : RadiusTargetingSO
{
    public override void HandleRightClick(TargetingManager manager, Vector3 mouseWorldPos, RaycastHit2D hit)
    {
        manager.CancelTargeting();
    }

    public override void HandleClick(TargetingManager manager, Vector3 mouseWorldPos, RaycastHit2D hit)
    {
        if (hit.collider == null)
        {
            return;
        }
        
        if (hit.collider.TryGetComponent(out DraggableItem hitItem))
        {
            if (!manager.HighlightedTargets.Contains(hitItem))
            {
                return;
            }

            PokemonStats targetStats = hitItem.GetComponent<PokemonStats>();
            
            if (!manager.IsValidTarget(hitItem, target)) //
            {
                return; 
            }
            
            manager.ExecuteMove(manager.Caster, targetStats, manager.CurrentMove); //
            manager.CancelTargeting();
        }
    }
}