using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float SprintSpeed = 4.0f;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Vector3 _cameraOffset = new(0.2f, 1.0f, -4.0f);
    [SerializeField] private Vector3 _aimOffset = new(0.0f, 1.0f, 0.0f);

    private CharacterController _characterController;
    private Animator _animator;

    private Vector3 _moveVec = Vector3.zero;
    private bool _isSprinting = false;
    private bool _isAiming;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (_isAiming)
        {
            _mainCamera.transform.localPosition = _aimOffset;
        }
        else
        {
            _mainCamera.transform.localPosition = _cameraOffset;
        }
    }

    private void FixedUpdate()
    {
        // set the direction to move based on the camera's Y-axis rotation
        Vector3 moveDir = _cameraController.PlanarRotation * _moveVec;

        // Movement keys depressed by user
        if (_moveVec.magnitude > 0)
        {
            // set player speed based on move speed, sprint speerd and if the sprint trigger is pressed
            float playerSpeed = _isSprinting ? SprintSpeed : MoveSpeed;

            Debug.Log(_moveVec);
            Debug.Log(playerSpeed);

            // rotate player to 'forward' direction of camera
            transform.rotation = Quaternion.LookRotation(moveDir);

            Vector3 movement = new Vector3(moveDir.x, 0.0f, moveDir.z) * playerSpeed * Time.fixedDeltaTime;

            _characterController.Move(movement);

            // Update the animator 'speed' parameter
            _animator.SetFloat("speed", playerSpeed, 0.2f, Time.fixedDeltaTime);
        }
        else
        {
            _moveVec = Vector3.zero;
            _animator.SetFloat("speed", 0f);
        }
    }

    // Handles 'Player_Move' context from InputSystem
    public void OnPlayerMove(InputAction.CallbackContext ctx)
    {
        // 'Move' key has been pressed
        if (ctx.performed)
        {
            // read the value from the depressed key
            Vector2 xyInput = ctx.ReadValue<Vector2>();

            // update the 'player move vector'
            _moveVec = new Vector3(xyInput.x, 0.0f, xyInput.y).normalized;
        }
        else if (ctx.canceled)
        {
            // 'Move' key is not pressed
            // set vector to (0, 0, 0)
            _moveVec = Vector3.zero;
        }
    }

    public void OnPlayerSprint(InputAction.CallbackContext ctx)
    {
        _isSprinting = ctx.ReadValue<float>() == 1;
    }

    public void OnPlayerAim(InputAction.CallbackContext ctx)
    {
        _isAiming = ctx.ReadValue<float>() == 1;
    }
}
