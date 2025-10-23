using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance { get; private set; }

    [Header("Indicator Visuals")]
    [SerializeField] private SpriteRenderer rangeIndicatorSprite;
    [SerializeField] private Color attackColor = new(1f, 0f, 0f, 0.4f);
    [SerializeField] private Color otherColor = new(0.5f, 0.5f, 0.5f, 0.4f);

    [SerializeField] private float zOffset = 1f;

    private Camera mainCamera;
    private Mouse currentMouse;

    private bool isTargeting = false;
    private PokemonStats caster;
    private MoveBaseSO currentMove;
    private float currentRange;

    private List<DraggableItem> highlightedTargets = new();
    private DraggableItem casterDraggableItem; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        mainCamera = Camera.main;
        currentMouse = Mouse.current;

        if (rangeIndicatorSprite == null)
        {
            Debug.LogError("FATAL: 'Range Indicator Sprite' is not set in the Inspector!", this);
            enabled = false;
            return;
        }
        
        rangeIndicatorSprite.gameObject.SetActive(false);
    }

    public void StartTargeting(PokemonStats caster, MoveBaseSO move)
    {
        CancelTargeting();

        this.caster = caster;
        currentMove = move;
        casterDraggableItem = caster.GetComponent<DraggableItem>();

        switch (move.targetType)
        {
            case TargetType.SingleTarget:
                InitiateRadiusTargeting(caster, move);
                break;

            case TargetType.MultipleTarget:
                Debug.Log("Initiating MultipleTarget targeting (currently same as SingleTarget)");
                InitiateRadiusTargeting(caster, move);
                break;

            case TargetType.Circle:
                Debug.Log("Initiating Circle targeting (currently same as SingleTarget)");
                InitiateRadiusTargeting(caster, move);
                break;

            case TargetType.Cone:
                Debug.LogWarning($"Targeting UI for TargetType '{move.targetType}' is not yet implemented.");
                CancelTargeting();
                break;
            
            default:
                CancelTargeting();
                break;
        }
    }

    private void InitiateRadiusTargeting(PokemonStats caster, MoveBaseSO move)
    {
        float casterRadius = caster.pokemon.radius; 
        currentRange = move.range + casterRadius; 

        rangeIndicatorSprite.color = (move is AttackMoveSO) ? attackColor : otherColor;

        UpdateIndicatorPosition();

        float diameter = currentRange * 2f;
        rangeIndicatorSprite.transform.localScale = new Vector3(diameter, diameter, 1f);

        rangeIndicatorSprite.gameObject.SetActive(true);
        
        FindAndHighlightTargets();
        
        isTargeting = true;
    }

    public void CancelTargeting()
    {
        ClearHighlightedTargets();

        isTargeting = false;
        caster = null;
        currentMove = null;
        casterDraggableItem = null; 
        currentRange = 0;

        rangeIndicatorSprite.gameObject.SetActive(false);
    }

    private void FindAndHighlightTargets()
    {
        float casterTotalRange = currentRange;
        
        DraggableItem[] allItems = FindObjectsByType<DraggableItem>(FindObjectsSortMode.None);

        foreach (DraggableItem item in allItems)
        {
            if (item == casterDraggableItem) continue;
            if (!item.TryGetComponent(out PokemonStats targetStats)) continue;

            float distance = Vector2.Distance(caster.transform.position, item.transform.position);

            if (distance < casterTotalRange)
            {
                item.SetTargetHighlight(true); 
                highlightedTargets.Add(item); 
            }
        }
    }

    private void ClearHighlightedTargets()
    {
        foreach (DraggableItem item in highlightedTargets)
        {
            if (item != null)
            {
                item.SetTargetHighlight(false);
            }
        }
        highlightedTargets.Clear();
    }

    void Update()
    {
        if (!isTargeting || currentMouse == null)
        {
            return;
        }

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            HandleTargetingClick();
        }

        if (currentMouse.rightButton.wasPressedThisFrame)
        {
            CancelTargeting();
        }
    }
    
    void LateUpdate()
    {
        if (isTargeting && caster != null && currentMove.targetType != TargetType.Cone)
        {
            UpdateIndicatorPosition();
        }
    }
    
    private void UpdateIndicatorPosition()
    {
        Vector3 casterPos = caster.transform.position;
        rangeIndicatorSprite.transform.position = new Vector3(casterPos.x, casterPos.y, casterPos.z + zOffset);
    }

    private void HandleTargetingClick()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        float distance = Vector2.Distance(caster.transform.position, mouseWorldPos);

        if (currentRange > 0 && distance > currentRange)
        {
            CancelTargeting();
            return;
        }

        switch (currentMove.targetType)
        {
            case TargetType.SingleTarget:
                HandleClick_SingleTarget(mouseWorldPos);
                break;

            case TargetType.MultipleTarget:
                Debug.LogWarning($"Click logic for TargetType '{currentMove.targetType}' is not yet implemented.");
                CancelTargeting();
                break;

            case TargetType.Circle:
                Debug.LogWarning($"Click logic for TargetType '{currentMove.targetType}' is not yet implemented.");
                CancelTargeting();
                break;

            case TargetType.Cone:
                Debug.LogWarning($"Click logic for TargetType '{currentMove.targetType}' is not yet implemented.");
                CancelTargeting();
                break;
        }
    }

    private void HandleClick_SingleTarget(Vector3 mouseWorldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out DraggableItem hitItem))
            {
                if (hitItem == casterDraggableItem)
                {
                    return;
                }

                if (highlightedTargets.Contains(hitItem))
                {
                    Debug.Log("Move Target: " + hitItem.GetComponent<PokemonStats>().pokemon.pokemonNameCN);
                    CancelTargeting();
                    return;
                }
                
                CancelTargeting();
                return;
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = currentMouse.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        worldPos.z = 0; 
        return worldPos;
    }
}