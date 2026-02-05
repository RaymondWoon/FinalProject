using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // Camera movement parameters
    [Header("Camera")]
    [SerializeField] private float _rotationSpeed = 2.0f;
    [SerializeField] private float _moveSpeed = 4.0f;
    [SerializeField] private float _minVerticalAngle = -30.0f;
    [SerializeField] private float _maxVerticalAngle = 70.0f;

    [Header("Player")]
    [SerializeField] private PlayerInput _playerInput;

    private Transform _focalPt;
    private Transform _target;

    // Camera rotation variables
    private float _rotationX;
    private float _rotationY;
    private Vector2 _lookDelta;
    private const float _lookThreshold = 0.01f;

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
        // reference the focal point of the camera
        _focalPt = transform.GetChild(0);

        // reference the camera's target, i.e. the player
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        // Hide & lock the mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        RotateCamera();
        FollowTarget();
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

            _rotationX += _lookDelta.y * _rotationSpeed * deltaTimeMultiplier;

            // restrict the horizontal axis rotation based on min/max vertical angles
            _rotationX = Mathf.Clamp(_rotationX, _minVerticalAngle, _maxVerticalAngle);

            _rotationY += _lookDelta.x * _rotationSpeed * deltaTimeMultiplier;

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
}
