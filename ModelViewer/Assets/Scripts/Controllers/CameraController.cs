using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _pivot;

    [Header("Sensitivity and movespeeds")]
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _zoomSpeed;
    [SerializeField] private float _panSpeed;

    [Header("Limits")]
    [SerializeField] private Vector2 _panningLimits;
    [SerializeField] private Vector2 _zoomLimits;

    private Vector2 _mouseInput;
    private float _scrollInput;
    private bool _mmbInput;

    private float _targetZoomPos;

    private Vector3 _defaultCamPos;
    private Quaternion _defaultCamRot;


    void Start()
    {
        _defaultCamPos = transform.position;
        _defaultCamRot = transform.rotation;

        _targetZoomPos = transform.localPosition.z;
    }

    void Update()
    {
        HandleZoom();
    }

    private void OnEnable()
    {
        GetComponent<PlayerInput>().actions.FindAction("Zoom").performed += CalculateZoom;
    }

    private void OnDisable()
    {
        GetComponent<PlayerInput>().actions.FindAction("Zoom").performed -= CalculateZoom;
    }
    private void HandlePanning()
    {

    }

    private void HandleRotation()
    {

    }

    private void CalculateZoom(InputAction.CallbackContext ctx)
    {
        Vector3 newPos = new Vector3(0, 0, ctx.ReadValue<Vector2>().y + transform.position.z);
        _targetZoomPos = Mathf.Clamp(newPos.z, _zoomLimits.x, _zoomLimits.y);

    }

    private void HandleZoom()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, _targetZoomPos), _zoomSpeed * Time.deltaTime);
    }

    private void ResetCamera()
    {
        transform.position = _defaultCamPos;
        transform.rotation = _defaultCamRot;
    }

}
