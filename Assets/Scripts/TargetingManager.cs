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
    private DraggableItem casterDraggableItem; 
    private bool inputConsumedThisFrame = false;

    private readonly List<DraggableItem> highlightedTargets = new();
    private readonly List<PokemonStats> confirmedMultiTargets = new();
    
    public SpriteRenderer RangeIndicatorSprite => rangeIndicatorSprite;
    public Color AttackColor => attackColor;
    public Color OtherColor => otherColor;
    public float ZOffset => zOffset;
    
    public PokemonStats Caster => caster;
    public MoveBaseSO CurrentMove => currentMove;
    public DraggableItem CasterDraggableItem => casterDraggableItem;
    
    public float CurrentRange
    {
        get => currentRange;
        set => currentRange = value;
    }
    public List<DraggableItem> HighlightedTargets => highlightedTargets;
    public List<PokemonStats> ConfirmedMultiTargets => confirmedMultiTargets;

    public bool IsTargeting()
    {
        return isTargeting;
    }

    public bool DidConsumeInputThisFrame()
    {
        return inputConsumedThisFrame;
    }
    

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
        if (move.targetingStrategy == null)
        {
            Debug.LogError($"Move {move.moveNameCN} ({move.moveID}) is missing a TargetingStrategySO!", move);
            return;
        }
        
        CancelTargeting();

        this.caster = caster;
        currentMove = move;
        casterDraggableItem = caster.GetComponent<DraggableItem>();

        currentMove.targetingStrategy.Initiate(this, caster, move);
        
        isTargeting = true;
    }

    public void CancelTargeting()
    {
        ClearSelectedTargets(); 
        foreach (DraggableItem item in highlightedTargets)
        {
            if (item != null)
            {
                item.SetTargetingState(DraggableItem.TargetingState.None);
            }
        }

        isTargeting = false;
        caster = null;
        currentMove = null;
        casterDraggableItem = null; 
        currentRange = 0;
        highlightedTargets.Clear();
        confirmedMultiTargets.Clear();
        rangeIndicatorSprite.gameObject.SetActive(false);
    }

    public void FindAndCacheTargetsInRange()
    {
        highlightedTargets.Clear();
        float casterTotalRange = currentRange;
        
        DraggableItem[] allItems = FindObjectsByType<DraggableItem>(FindObjectsSortMode.None);

        foreach (DraggableItem item in allItems)
        {
            if (item == casterDraggableItem)
            {
                if (currentMove.targetingStrategy.target == Target.Self || currentMove.targetingStrategy.target == Target.AllyTeam)
                {
                     highlightedTargets.Add(item);
                }
                continue;
            }

            if (!item.TryGetComponent(out PokemonStats targetStats)) continue;
            
            float targetRadius = targetStats.pokemon.radius;
            float distance = Vector2.Distance(caster.transform.position, item.transform.position);

            if (distance <= casterTotalRange + targetRadius - 1f)
            {
                highlightedTargets.Add(item);
                
                item.SetTargetingState(DraggableItem.TargetingState.Targetable);
            }
        }
    }

    public void ClearSelectedTargets()
    {
        foreach (PokemonStats targetStats in confirmedMultiTargets)
        {
            if (targetStats != null)
            {
                DraggableItem item = targetStats.GetComponent<DraggableItem>();
                if (item != null)
                {
                    item.SetTargetingState(DraggableItem.TargetingState.None);
                }
            }
        }
    }

    void Update()
    {
        if (!isTargeting || currentMouse == null || currentMove == null || currentMove.targetingStrategy == null)
        {
            return;
        }
        
        if (currentMouse.leftButton.wasPressedThisFrame || currentMouse.rightButton.wasPressedThisFrame)
        {
            inputConsumedThisFrame = true;
        }
        
        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            HandleTargetingClick();
        }

        if (currentMouse.rightButton.wasPressedThisFrame)
        {
            currentMove.targetingStrategy.HandleRightClick(this, GetMouseWorldPos(), Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero));
        }
    }
    
    void LateUpdate()
    {
        if (isTargeting && caster != null && currentMove != null && currentMove.targetingStrategy != null)
        {
            currentMove.targetingStrategy.UpdateIndicator(this, caster);
        }
        inputConsumedThisFrame = false;
    }

    private void HandleTargetingClick()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        float distance = Vector2.Distance(caster.transform.position, mouseWorldPos);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (currentRange > 0 && distance > currentRange && hit.collider == null)
        {
            CancelTargeting();
            return;
        }

        currentMove.targetingStrategy.HandleClick(this, mouseWorldPos, hit);
    }

    public bool IsValidTarget(DraggableItem targetItem, Target moveTargetType)
    {
        bool isSelf = targetItem == casterDraggableItem;
        
        if (isSelf)
        {
            if (moveTargetType == Target.Self || moveTargetType == Target.AllyTeam || moveTargetType == Target.Creature)
            {
                return true;
            }
            else
            {
                Debug.LogError($"Invalid Target: Move target type is {moveTargetType}, cannot target Self.");
                return false;
            }
        }
        else
        {
            if (moveTargetType == Target.Self)
            {
                Debug.LogError($"Invalid Target: Move target type is {moveTargetType}, must target Self.");
                return false;
            }
            
            // TODO: Ally/Enemy checks here
        }
        
        return true;
    }

    public void ExecuteMove(PokemonStats caster, PokemonStats target, MoveBaseSO move)
    {
        if (target == null || caster == null || move == null) return;

        if (!caster.CanUseActionPoint(1))
        {
            Debug.Log($"[Info]: {caster.alias} has no action points left to use {move.moveNameCN}!", caster);
            return;
        }

        caster.UseActionPoint(1);
        Debug.Log($"[Info]: {caster.alias} used {move.moveNameCN}, consuming 1 action point.", caster);

        int damageDealt = 0;

        if (move is AttackMoveSO attackMove)
        {
            damageDealt = BattleCalculator.HandleAttack(caster, target, attackMove);
        }
        else
        {
            Debug.LogWarning($"Move {move.moveName} ({move.moveNameCN}) is not an AttackMove. Calculation logic not implemented.");
        }

        if (!target.IsFainted())
        {
            if (move.additionalEffects != null && move.additionalEffects.Count > 0)
            {
                foreach (MoveEffectSO effect in move.additionalEffects)
                {
                    if (effect != null)
                    {
                        if (effect is HealEffectSO healEffect && healEffect.healType == HealType.PercentageOfDamageDealt)
                        {
                            healEffect.SetDamageContext(damageDealt);
                        }
                        else if (effect is RecoilEffectSO recoilEffect)
                        {
                            recoilEffect.SetDamageContext(damageDealt);
                        }
                        effect.Execute(caster, target);
                    }
                }
            }
        }
    }

    public Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = currentMouse.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        worldPos.z = 0;
        return worldPos;
    }
}