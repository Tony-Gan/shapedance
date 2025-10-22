using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class DraggableItem : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Vector3 offset;
    private bool isDragging = false;
    private Mouse currentMouse;

    [Header("Dragging Physics")]
    [SerializeField] private float mouseDragSpeed = 15f; 

    private LineRenderer lineRenderer;
    [Header("Visuals")]
    [SerializeField] private bool showLineWhileDragging = false;
    private bool previousShowLineWhileDragging;
    private Vector3 dragStartPosition;

    private SpriteRenderer sr; 
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    
    [Header("Jiggle Effect")]
    [Tooltip("Drag the 'VisualContainer' parent object here.")]
    [SerializeField] private Transform visualTransform; 
    [SerializeField] private float knockbackDistance = 0.4f;
    [SerializeField] private float knockbackDuration = 0.3f;
    private Coroutine jiggleCoroutine;
    private Vector3 jiggleVelocity; 


    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        
        if (visualTransform == null)
        {
            Debug.LogError("FATAL: 'Visual Transform' is not set in the Inspector. Please drag the 'VisualContainer' object into this slot.", this);
            return;
        }

        // Find SpriteRenderer within the VisualContainer
        sr = visualTransform.GetComponentInChildren<SpriteRenderer>(); 
        if (sr == null)
        {
            Debug.LogError("Could not find a SpriteRenderer in the children of 'Visual Transform'.", this);
        }
        else
        {
            sr.color = normalColor; 
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.freezeRotation = true; 

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
        SelectionManager.SetSelected(this);

        isDragging = true;
        rb.linearVelocity = Vector2.zero; 
        dragStartPosition = transform.position;

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
    }
    
    private void CancelDrag()
    {
        isDragging = false;
        lineRenderer.enabled = false;
        
        rb.position = dragStartPosition; 
        rb.linearVelocity = Vector2.zero;
    }
    
    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 desiredPos = GetMouseWorldPos() + offset;
            Vector2 moveDirection = new Vector2(desiredPos.x, desiredPos.y) - rb.position;
            
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
        if (isDragging)
        {
            if (collision.gameObject.TryGetComponent<DraggableItem>(out var otherItem))
            {
                Rigidbody2D otherRb = collision.rigidbody;
                if (otherRb != null)
                {
                    otherRb.linearVelocity = Vector2.zero;
                    Vector2 knockDirection = (otherRb.position - rb.position).normalized;
                    otherItem.TriggerKnockback(knockDirection);
                }
                ConfirmDrag();
            }
        }
    }
    
    public void Highlight()
    {
        if (sr != null)
        {
            sr.color = highlightColor;
        }
    }

    public void Dehighlight()
    {
        if (sr != null)
        {
            sr.color = normalColor;
        }
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