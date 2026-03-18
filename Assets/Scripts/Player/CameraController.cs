using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.SceneView;

public class CameraController : MonoBehaviour
{
    
    [Header("Camera Movement Parameters")]
    [SerializeField] private float _moveSpeed = 4.0f;
    [SerializeField] public float _rotationSpeed = 2.0f;
    [SerializeField] private float _zoomSpeed = 5.0f;
    [SerializeField] private float _minVerticalAngle = -30.0f;
    [SerializeField] private float _maxVerticalAngle = 70.0f;
    [SerializeField] private float _mouseSensitivityX = 5.0f;
    [SerializeField] private float _mouseSensitivityY = 5.0f;

    [Header("Camera Collision")]
    [SerializeField] private Transform _cameraPosition;
    [SerializeField] LayerMask _camAvoidanceLayers;

    [Header("Player")]
    [SerializeField] private PlayerInput _playerInput;

    private Transform _focalPt;
    private Transform _target;
    private Vector3 _initialCamPos;

    // Camera rotation variables
    private Camera _mainCam;
    private float _rotationX;
    private float _rotationY;
    private Vector2 _lookDelta;
    private const float _lookThreshold = 0.01f;

    private RaycastHit _hit;

    public Quaternion PlanarRotation => Quaternion.Euler(0.0f, _rotationY, 0.0f);

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCam = Camera.main;

        // reference the focal point of the camera
        _focalPt = transform.GetChild(0);

        // initial local position of the main camera
        _initialCamPos = _mainCam.transform.localPosition;

        // reference the camera's target, i.e. the player
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        // Hide & lock the mouse cursor
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_target)
            return;
    }

    private void LateUpdate()
    {
        RotateCamera();
        FollowTarget();
        //HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        Vector3 cameraPos = Vector3.Lerp(transform.position, _target.transform.position, _moveSpeed * Time.deltaTime);

        transform.position = cameraPos;
    }

    public void OnCameraRotate(InputAction.CallbackContext ctx)
    {
        // Detect input from PlayerInput 'Look' actions
        _lookDelta = ctx.ReadValue<Vector2>();
    }

    // Update the 'Camera' rotation
    private void RotateCamera()
    {
        if (_lookDelta.sqrMagnitude >= _lookThreshold)
        {
            //Don't multiply mouse input by Time.deltaTime
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _rotationX += _lookDelta.y * (IsCurrentDeviceMouse ? _mouseSensitivityX : 1.0f);

            // restrict the horizontal axis rotation based on min/max vertical angles
            _rotationX = Mathf.Clamp(_rotationX, _minVerticalAngle, _maxVerticalAngle);

            _rotationY += _lookDelta.x * (IsCurrentDeviceMouse ? _mouseSensitivityY : 1.0f);

            // reset angles larger than 360 degrees
            _rotationY = Mathf.Repeat(_rotationY, 360.0f);

            // required rotation angle
            Vector3 rotationAngle = new(_rotationX, _rotationY, 0);

            // use 'slerp' for smooth rotation of camera
            Quaternion rotation = Quaternion.Slerp(_focalPt.transform.localRotation, Quaternion.Euler(rotationAngle), _rotationSpeed * deltaTimeMultiplier);

            // update the rotation of the 'camera' parent container
            _focalPt.transform.rotation = rotation;
        }
    }

    private void HandleCameraCollisions()
    {
        if (Physics.Linecast(_target.transform.position + _target.up, _cameraPosition.position, out _hit, _camAvoidanceLayers))
        {
            Debug.Log("Camera hit detected");
            Vector3 newCameraPos = new Vector3(_hit.point.x + _hit.normal.x * 0.2f, _hit.point.y + _hit.normal.y * 0.8f, _hit.point.z + _hit.normal.z * 0.2f);

            _mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, newCameraPos, _moveSpeed * Time.deltaTime);
        }
        else
        {
            _mainCam.transform.localPosition = Vector3.Lerp(_mainCam.transform.localPosition, _initialCamPos, _moveSpeed * Time.deltaTime);
        }

        //Debug.DrawLine(_target.transform.position + _target.transform.up, _cameraPosition.position, Color.blue);
    }
}
