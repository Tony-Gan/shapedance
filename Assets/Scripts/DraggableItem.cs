using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; 

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PokemonStats))]
public class DraggableItem : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Vector3 offset;
    private bool isDragging = false;
    private Mouse currentMouse;

    [Header("Dragging Physics")]
    [SerializeField] private readonly float mouseDragSpeed = 15f; 
    
    private readonly HashSet<Rigidbody2D> collidingBodies = new();
    private readonly HashSet<DraggableItem> triggeredKnockbacks = new();
    private Collider2D coll;
    private PokemonStats pokemonStats;

    private LineRenderer lineRenderer;
    [Header("Visuals")]
    [SerializeField] private readonly bool showLineWhileDragging = false;
    private bool previousShowLineWhileDragging;
    private Vector3 dragStartPosition;

    private SpriteRenderer sr; 
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color targetColor = Color.red;
    
    private bool isSelected = false;
    private bool isTargeted = false;
    
    [Header("Jiggle Effect")]
    [Tooltip("Drag the 'VisualContainer' parent object here.")]
    [SerializeField] private readonly Transform visualTransform; 
    [SerializeField] private readonly float knockbackDistance = 0.4f;
    [SerializeField] private readonly float knockbackDuration = 0.3f;
    private Coroutine jiggleCoroutine;
    private Vector3 jiggleVelocity; 


    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        coll = GetComponent<Collider2D>();
        pokemonStats = GetComponent<PokemonStats>();
        
        if (visualTransform == null)
        {
            Debug.LogError("FATAL: 'Visual Transform' is not set in the Inspector. Please drag the 'VisualContainer' object into this slot.", this);
            return;
        }

        sr = visualTransform.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("Could not find a SpriteRenderer in the children of 'Visual Transform'.", this);
        }
        else
        {
            UpdateColor();
        }

        if (pokemonStats != null && pokemonStats.pokemon != null)
        {
            if (pokemonStats.pokemon.pokemonSprite != null)
            {
                sr.sprite = pokemonStats.pokemon.pokemonSprite;
            }
            else
            {
                Debug.LogWarning($"PokemonSO '{pokemonStats.pokemon.pokemonName}' is missing a Sprite.", this);
            }
        }
        else
        {
            Debug.LogError("FATAL: 'PokemonStats' component is missing or its PokemonSO is not set.", this);
            enabled = false;
            return;
        }
        
        ApplyPokemonRadius();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.freezeRotation = true; 

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentMouse = Mouse.current;

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        previousShowLineWhileDragging = showLineWhileDragging;
    }

    void Update()
    {
        if (currentMouse == null) return;

        HandleLineVisibility();
        HandleInput();
        UpdateLinePosition();
    }

    private void HandleLineVisibility()
    {
        if (previousShowLineWhileDragging == showLineWhileDragging) return;
        
        if (isDragging)
        {
            bool show = showLineWhileDragging;
            lineRenderer.enabled = show;

            if (show)
            {
                lineRenderer.SetPosition(0, dragStartPosition);
                UpdateLinePosition();
            }
        }
        else
        {
            lineRenderer.enabled = false;
        }
        
        previousShowLineWhileDragging = showLineWhileDragging;
    }

    private void HandleInput()
    {
        if (isDragging)
        {
            if (currentMouse.rightButton.wasPressedThisFrame)
            {
                CancelDrag();
                return;
            }

            if (currentMouse.leftButton.wasReleasedThisFrame)
            {
                ConfirmDrag();
                return;
            }
        }
        else
        {
            if (currentMouse.leftButton.wasPressedThisFrame)
            {
                Vector3 mouseWorldPos = GetMouseWorldPos();
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    StartDrag();
                    offset = transform.position - mouseWorldPos;
                }
            }
        }
    }

    private void UpdateLinePosition()
    {
        if (isDragging && showLineWhileDragging)
        {
            Vector3 currentEndPoint = transform.position;
            lineRenderer.SetPosition(1, currentEndPoint);
        }
    }

    private void StartDrag()
    {
        if (TargetingManager.Instance != null)
        {
            TargetingManager.Instance.CancelTargeting();
        }

        SelectionManager.SetSelected(this);

        isDragging = true;
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero; 
        
        dragStartPosition = transform.position;

        DetectInitialContacts();
        
        triggeredKnockbacks.Clear();

        lineRenderer.SetPosition(0, dragStartPosition);
        lineRenderer.SetPosition(1, dragStartPosition);
        
        if (showLineWhileDragging)
        {
            lineRenderer.enabled = true;
        }
    }

    private void ConfirmDrag()
    {
        isDragging = false;
        lineRenderer.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        collidingBodies.Clear();
        triggeredKnockbacks.Clear();
    }

    private void CancelDrag()
    {
        isDragging = false;
        lineRenderer.enabled = false;

        rb.position = dragStartPosition;
        rb.linearVelocity = Vector2.zero;

        rb.bodyType = RigidbodyType2D.Kinematic;

        collidingBodies.Clear();
        triggeredKnockbacks.Clear();
    }
    
    private void ApplyPokemonRadius()
    {
        float pokemonRadius = pokemonStats.pokemon.radius;

        if (coll is CircleCollider2D circleCollider)
        {
            circleCollider.radius = pokemonRadius;
        }
        else
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not a CircleCollider2D. Cannot apply radius.", this);
        }

        if (visualTransform != null && sr != null && sr.sprite != null)
        {
            float originalSpriteWidth = sr.sprite.bounds.size.x;

            if (originalSpriteWidth < 0.001f)
            {
                Debug.LogError($"Sprite {sr.sprite.name} has bounds.size.x of zero. Cannot scale correctly.", this);
                return;
            }
            
            float desiredDiameter = pokemonRadius * 2f;
            float scaleFactor = desiredDiameter / originalSpriteWidth;
            visualTransform.localScale = new Vector3(scaleFactor, scaleFactor, visualTransform.localScale.z);
        }
        else
        {
            Debug.LogWarning("Cannot apply visual scaling. SpriteRenderer, Sprite, or VisualTransform is missing.", this);
        }
    }
    
    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 desiredPos = GetMouseWorldPos() + offset;
            Vector2 moveDirection = (Vector2)desiredPos - rb.position;
            
            if (collidingBodies.Count > 0)
            {
                moveDirection = FilterMovementDirection(moveDirection);
            }
            
            rb.linearVelocity = moveDirection * mouseDragSpeed; 
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = currentMouse.position.ReadValue();
        float zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, zCoord));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<DraggableItem>(out var otherItem))
        {
            Rigidbody2D otherRb = collision.rigidbody;
            if (otherRb != null)
            {
                collidingBodies.Add(otherRb);
                
                if (isDragging && !triggeredKnockbacks.Contains(otherItem))
                {
                    Vector2 knockDirection = (otherRb.position - rb.position).normalized;
                    otherItem.TriggerKnockback(knockDirection);
                    
                    triggeredKnockbacks.Add(otherItem);
                }
            }
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null)
        {
            collidingBodies.Remove(collision.rigidbody);
        }
    }
    
    public void Highlight()
    {
        isSelected = true;
        UpdateColor();
    }

    public void Dehighlight()
    {
        isSelected = false;
        UpdateColor();
    }

    public void SetTargetHighlight(bool isTargeted)
    {
        this.isTargeted = isTargeted;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (sr == null) return;
        
        if (isSelected)
        {
            sr.color = highlightColor;
        }
        else if (isTargeted)
        {
            sr.color = targetColor;
        }
        else
        {
            sr.color = normalColor;
        }
    }
    
    
    private void DetectInitialContacts()
    {
        collidingBodies.Clear();

        ContactFilter2D filter = new()
        {
            useTriggers = false
        };
        filter.SetLayerMask(Physics2D.AllLayers);
        
        List<Collider2D> results = new();
        _ = Physics2D.OverlapCollider(coll, filter, results);

        foreach (Collider2D col in results)
        {
            if (col == coll) continue;
            if (col.TryGetComponent<DraggableItem>(out _))
            {
                Rigidbody2D otherRb = col.attachedRigidbody;
                if (otherRb != null && otherRb != rb)
                {
                    collidingBodies.Add(otherRb);
                }
            }
        }
    }
    
    private Vector2 FilterMovementDirection(Vector2 desiredMovement)
    {
        if (desiredMovement.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }
        
        Vector2 resultMovement = desiredMovement;
        
        foreach (Rigidbody2D collidingRb in collidingBodies)
        {
            if (collidingRb == null) continue;
            
            Vector2 toOther = collidingRb.position - rb.position;
            
            float dot = Vector2.Dot(desiredMovement.normalized, toOther.normalized);
            
            if (dot > 0.1f)
            {
                Vector2 perpendicular = Vector2.Perpendicular(toOther.normalized);
                float perpendicularComponent = Vector2.Dot(resultMovement, perpendicular);
                
                resultMovement = perpendicular * perpendicularComponent;
            }
        }
        
        return resultMovement;
    }

    public void TriggerKnockback(Vector2 direction)
    {
        if (jiggleCoroutine != null)
        {
            StopCoroutine(jiggleCoroutine);
            visualTransform.localPosition = Vector3.zero;
        }
        jiggleCoroutine = StartCoroutine(JiggleEffect(direction));
    }

    private IEnumerator JiggleEffect(Vector2 direction)
    {
        jiggleVelocity = Vector3.zero;
        Vector3 knockbackPosition = (Vector3)direction.normalized * knockbackDistance;
        
        float timer = 0;
        float halfDuration = knockbackDuration / 2f;
        
        while (timer < halfDuration)
        {
            visualTransform.localPosition = Vector3.SmoothDamp(
                visualTransform.localPosition, 
                knockbackPosition, 
                ref jiggleVelocity, 
                halfDuration
            );
            timer += Time.deltaTime;
            yield return null; 
        }

        timer = 0;
        jiggleVelocity = Vector3.zero; 
        
        while (timer < halfDuration)
        {
            visualTransform.localPosition = Vector3.SmoothDamp(
                visualTransform.localPosition, 
                Vector3.zero, 
                ref jiggleVelocity, 
                halfDuration
            );
            timer += Time.deltaTime;
            yield return null; 
        }

        visualTransform.localPosition = Vector3.zero;
        jiggleCoroutine = null;
    }
}