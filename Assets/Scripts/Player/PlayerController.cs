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
    [SerializeField] private Vector3 _cameraOffset = new(0.2f, 1.0f, -2.0f);
    [SerializeField] private Vector3 _aimOffset = new(0.0f, 1.0f, 0.0f);

    [Header("Dungeon")]
    [SerializeField] private GameObject _environment;

    private CharacterController _characterController;
    private Animator _animator;

    private Vector3 _moveVec = Vector3.zero;
    private float _moveThreshold = 0.01f;
    private bool _isSprinting = false;
    private bool _isAiming;
    private bool _isInStairway = false;

    private int _playerFloor = 1;

    public int PlayerFloor
    {
        get { return _playerFloor; }
        set { _playerFloor = value; }
    }

    public bool IsInStairway
    {
        get { return _isInStairway; }
        set { _isInStairway = value; }
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        // set the direction to move based on the camera's Y-axis rotation
        Vector3 moveDir = _cameraController.PlanarRotation * _moveVec;

        // Movement keys depressed by user
        if (_moveVec.magnitude > _moveThreshold)
        {
            // set player speed based on move speed, sprint speerd and if the sprint trigger is pressed
            float playerSpeed = _isSprinting ? SprintSpeed : MoveSpeed;

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

    // Handles 'Player_Aim' context from InputSystem
    public void OnPlayerAim(InputAction.CallbackContext ctx)
    {
        _isAiming = ctx.ReadValue<float>() == 1;
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

    // Handles 'Player_Sprint' context from InputSystem
    public void OnPlayerSprint(InputAction.CallbackContext ctx)
    {
        _isSprinting = ctx.ReadValue<float>() == 1;
    }

    

    // Handles 'Player_Map' context from InputSystem
    public void OnPlayerToggleMap(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !_isInStairway)
        {
            ToggleMapVisibility();
        }
    }

    // Function to toggle the visibility of the map
    private void ToggleMapVisibility()
    {
        foreach (Transform child in _environment.transform)
        {
            if (child.name == "Dungeon Floor " + _playerFloor.ToString())
            {
                child.GetComponent<DungeonGenerator>().ToggleMap();
            }
        }
    }

    public void TurnActiveMapOff()
    {
        foreach (Transform child in _environment.transform)
        {
            if (child.name == "Dungeon Floor " + _playerFloor.ToString())
            {
                child.GetComponent<DungeonGenerator>().ShowMap = false;
            }
        }
    }
}
