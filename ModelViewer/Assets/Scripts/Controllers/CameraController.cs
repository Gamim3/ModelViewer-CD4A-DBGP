using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _pivot;

    [Header("Sensitivity and movespeeds")]
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _panSensitivity;
    [SerializeField] private float _zoomSpeed;
    [SerializeField] private float _panSpeed;

    [Header("Limits")]
    [SerializeField] private Vector4 _panningLimits;
    [SerializeField] private Vector2 _zoomLimits;

    private PlayerInput _playerInput;
    private Vector2 _mouseInput;
    private float _scrollInput;
    private bool _mmbInput;

    private float _targetZoomPos = -5f;
    private Vector2 _targetPanPos;

    private Vector3 _defaultCamPos;
    private Quaternion _defaultRot;
    private float _defaultZoomPos;

    private float _xRotation;
    private float _yRotation;


    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();

        _defaultCamPos = transform.position;
        _defaultRot = transform.rotation;
        _defaultZoomPos = transform.localPosition.z;
    }

    void Update()
    {
        HandleInput();
        HandleZoom();
    }

    private void OnEnable()
    {
        _playerInput.actions.FindAction("Zoom").performed += CalculateZoom;
    }

    private void OnDisable()
    {
        _playerInput.actions.FindAction("Zoom").performed -= CalculateZoom;
    }

    private void HandleInput()
    {
        _mouseInput = _playerInput.actions.FindAction("Rotate").ReadValue<Vector2>();
        if (_playerInput.actions.FindAction("AllowRotate").IsPressed())
        {
            HandleRotation();
        }

        if (_playerInput.actions.FindAction("Panning").IsPressed())
        {
            CalculatePanning();
        }
    }

    private void CalculatePanning()
    {
        _targetPanPos.x -= _mouseInput.x * _panSensitivity;
        _targetPanPos.y -= _mouseInput.y * _panSensitivity;

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
        Vector3 newPos = new Vector3(0, 0, ctx.ReadValue<Vector2>().y + transform.localPosition.z);
        _targetZoomPos = Mathf.Clamp(newPos.z, _zoomLimits.x, _zoomLimits.y);
    }

    private void HandleZoom()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, _targetZoomPos), _zoomSpeed * Time.deltaTime);
    }

    public void ResetCamera()
    {
        _pivot.rotation = _defaultRot;
        transform.position = _defaultCamPos;
        _targetPanPos = new Vector2(0, 0);
        _targetZoomPos = _defaultZoomPos;
    }
}