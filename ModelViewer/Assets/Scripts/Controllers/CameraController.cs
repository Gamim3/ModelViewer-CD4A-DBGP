using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _pivot;

    private PlayerInput _playerInput;
    public PlayerInput PlayerInput => _playerInput ??= GetComponent<PlayerInput>();

    private Model _model => ModelManager.Instance.SelectedModel;

    [Header("Sensitivity and movespeeds")]
    [SerializeField] private float _mouseSensitivity = 0.25f;
    [SerializeField] private float _panSensitivity = 0.01f;
    [SerializeField] private float _zoomSensitivity = 3f;
    [SerializeField] private float _zoomSpeed = 3f;

    [Header("Limits")]
    [Tooltip("Automatically calculated:X and Y is for left and rigth. Z and W is for up and down")]
    [SerializeField] private Vector4 _panningLimits;
    [Tooltip("X is for the far limit Y is for the near limit")]
    [SerializeField] private Vector2 _zoomLimits;

    private Vector2 _mouseInput;

    private float _targetZoomPos = -5f;
    private Vector2 _targetPanPos;

    private float _currentZoomLevel;

    private Quaternion _defaultRot;

    private float _xRotation;
    private float _yRotation;

    private void Start()
    {
        _defaultRot = transform.rotation;
        ModelManager.Instance.OnModelChanged += _ => OnModelChanged();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        HandleInput();
    }

    private void OnEnable()
    {
        PlayerInput.actions.FindAction("Zoom").performed += CalculateZoom;
    }

    private void OnDisable()
    {
        PlayerInput.actions.FindAction("Zoom").performed -= CalculateZoom;
        ModelManager.Instance.OnModelChanged -= _ => OnModelChanged();
    }


    /// <summary>
    /// continusely reads mouse inputs and checks if rotate or panning button is pressed
    /// </summary>
    private void HandleInput()
    {
        _mouseInput = PlayerInput.actions.FindAction("Rotate").ReadValue<Vector2>();
        if (PlayerInput.actions.FindAction("AllowRotate").IsPressed())
        {
            HandleRotation();
        }

        if (PlayerInput.actions.FindAction("Panning").IsPressed())
        {
            CalculatePanning();
        }

        HandleZoom();

        if (PlayerInput.actions.FindAction("Focus").WasPressedThisFrame())
        {
            transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                UIManager.Instance.ToggleGallery(false);
            }
        }
    }

    /// <summary>
    /// The logic for panning in X and Y axis
    /// </summary>
    private void CalculatePanning()
    {
        _targetPanPos.x -= _mouseInput.x * _panSensitivity * Time.deltaTime;
        _targetPanPos.y -= _mouseInput.y * _panSensitivity * Time.deltaTime;
        _targetPanPos.x = Mathf.Clamp(_targetPanPos.x, _panningLimits.x, _panningLimits.y);
        _targetPanPos.y = Mathf.Clamp(_targetPanPos.y, _panningLimits.z, _panningLimits.w);
        transform.localPosition = new Vector3(_targetPanPos.x, _targetPanPos.y, transform.localPosition.z);
    }

    private void HandleRotation()
    {
        float mouseX = _mouseInput.x * _mouseSensitivity;
        float mouseY = _mouseInput.y * _mouseSensitivity;

        _xRotation -= mouseY;
        _yRotation += mouseX;

        _pivot.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
    }

    private void CalculateZoom(InputAction.CallbackContext ctx)
    {
        var zoomFactor = 1f;
        if (_model != null && _model.Renderer != null)
            zoomFactor = Mathf.Lerp(1, 5, Mathf.InverseLerp(1, 10, _model.Renderer.bounds.size.magnitude / 2));

        Vector3 newPos = new(0, 0, ctx.ReadValue<Vector2>().y * (_zoomSensitivity * zoomFactor) + transform.localPosition.z);
        _targetZoomPos = Mathf.Clamp(newPos.z, _zoomLimits.x, _zoomLimits.y);

        _targetPanPos.x = Mathf.Clamp(_targetPanPos.x, _panningLimits.x, _panningLimits.y);
        _targetPanPos.y = Mathf.Clamp(_targetPanPos.y, _panningLimits.z, _panningLimits.w);
        transform.localPosition = new Vector3(_targetPanPos.x, _targetPanPos.y, transform.localPosition.z);
    }

    private void HandleZoom()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, _targetZoomPos), _zoomSpeed * Time.deltaTime);
        _currentZoomLevel = Mathf.Abs(transform.localPosition.z);
        RecalculateBounds();
    }

    public void ResetCamera()
    {
        _pivot.rotation = _defaultRot;
        transform.position = new Vector3((_panningLimits.x + _panningLimits.y) / 2, (_panningLimits.z + _panningLimits.w) / 2, (_zoomLimits.x + _zoomLimits.y) / 2);
        _targetPanPos = new Vector2(0, 0);
        _targetZoomPos = transform.localPosition.z;

        _xRotation = 0;
        _yRotation = 0;
    }

    private void RecalculateBounds()
    {
        if (_model == null || _model.Renderer == null) return;

        var bounds = _model.Renderer.bounds;

        _panningLimits.x = -bounds.size.x * (_currentZoomLevel / 5);
        _panningLimits.y = bounds.size.x * (_currentZoomLevel / 5);
        _panningLimits.z = -bounds.size.y * (_currentZoomLevel / 5);
        _panningLimits.w = bounds.size.y * (_currentZoomLevel / 5);

        _zoomLimits.x = -bounds.size.magnitude * 2;
        _zoomLimits.y = -bounds.size.magnitude;
    }

    private void OnModelChanged()
    {
        RecalculateBounds();
        ResetCamera();
    }
}
