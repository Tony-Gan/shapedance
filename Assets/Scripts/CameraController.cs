using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float defaultSize = 50f;
    [SerializeField] private float minSize = 20f;
    [SerializeField] private float maxSize = 100f;
    [SerializeField] private float zoomSpeed = 0.05f; 

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f; 
    
    private Camera mainCamera;
    private Mouse currentMouse;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (!mainCamera.orthographic)
        {
            Debug.LogError("CameraController 仅适用于 Orthographic (正交) 摄影机。");
            enabled = false;
        }
    }

    void Start()
    {
        mainCamera.orthographicSize = defaultSize; 
        currentMouse = Mouse.current;
    }

    void Update()
    {
        if (currentMouse == null) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float scrollDelta = currentMouse.scroll.ReadValue().y;
        if (Mathf.Abs(scrollDelta) > 0.1f)
        {
            HandleZoom(scrollDelta);
        }

        ButtonControl panButton = currentMouse.middleButton;
        HandlePan(panButton);
    }

    private void HandleZoom(float scrollDelta)
    {
        float newSize = mainCamera.orthographicSize - (scrollDelta * zoomSpeed * mainCamera.orthographicSize);
        
        mainCamera.orthographicSize = Mathf.Clamp(newSize, minSize, maxSize);
    }

    private void HandlePan(ButtonControl panButton)
    {
        if (panButton.isPressed)
        {
            Vector2 mouseDelta = currentMouse.delta.ReadValue();

            if (mouseDelta.sqrMagnitude < 0.1f)
            {
                return;
            }
            
            float unitsPerPixel = mainCamera.orthographicSize * 2 / Screen.height;

            Vector3 worldDelta = new(-mouseDelta.x * unitsPerPixel, -mouseDelta.y * unitsPerPixel, 0);

            transform.position += worldDelta * panSpeed;
        }
    }
}