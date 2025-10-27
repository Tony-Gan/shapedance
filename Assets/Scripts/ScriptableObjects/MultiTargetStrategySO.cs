using UnityEngine;

[CreateAssetMenu(fileName = "New Multi Target Strategy", menuName = "Pokemon/Targeting/Multi Target")]
public class MultiTargetStrategySO : RadiusTargetingSO
{
    [Header("Multi-Target Details")]
    [Tooltip("For multi-target moves, the number of targets to select.")]
    public int maxTargets = 2;

    public override void HandleRightClick(TargetingManager manager, Vector3 mouseWorldPos, RaycastHit2D hit)
    {
        if (hit.collider != null && hit.collider.TryGetComponent(out DraggableItem hitItem))
        {
            PokemonStats targetStats = hitItem.GetComponent<PokemonStats>();
            if (targetStats != null && manager.ConfirmedMultiTargets.Contains(targetStats))
            {
                manager.ConfirmedMultiTargets.Remove(targetStats);
                hitItem.SetTargetHighlight(false);
            }
            else
            {
                manager.CancelTargeting();
            }
        }
        else
        {
            manager.CancelTargeting();
        }
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
            
            if (manager.ConfirmedMultiTargets.Contains(targetStats))
            {
                return;
            }
            
            if (!manager.IsValidTarget(hitItem, target))
            {
                return;
            }
            
            manager.ConfirmedMultiTargets.Add(targetStats);
            hitItem.SetTargetHighlight(true);
            
            if (manager.ConfirmedMultiTargets.Count == maxTargets)
            {
                foreach (PokemonStats target in manager.ConfirmedMultiTargets)
                {
                    manager.ExecuteMove(manager.Caster, target, manager.CurrentMove);
                }
                manager.CancelTargeting();
            }
        }
    }
}