using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float SprintSpeed = 4.0f;

    [Header("Camera")]
    //[SerializeField] private Camera _mainCamera;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Vector3 _cameraOffset = new(0.2f, 1.0f, -2.0f);
    [SerializeField] private Vector3 _aimOffset = new(0.0f, 1.0f, 0.0f);

    [Header("Head Rotation Settings")]
    public float _lookAtPoint = 2.8f;

    //[Header("Camera & Player Syncing")]
    //[SerializeField] private float _lookDistance = 5.0f;
    //[SerializeField] private float _lookSpeed = 5.0f;

    [Header("Audio")]
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Header("Dungeon")]
    [SerializeField] private GameObject _environment;

    [Header("Aiming Settings")]
    [SerializeField] private Bow _bow;
    [SerializeField] private LayerMask _aimLayers;

    private CharacterController _characterController;
    private PlayerInventorySystem _playerInventory;
    private Animator _animator;

    private Camera _mainCamera;
    private Transform _focalPt;
    private Ray _ray;
    private RaycastHit _hit;
    private bool _hitDetected;

    private Vector3 _moveVec = Vector3.zero;
    private float _moveThreshold = 0.01f;
    private bool _isSprinting = false;
    private bool _isFirstPerson;
    private bool _isAiming;
    private bool _isInStairway = false;

    private int _playerFloor = 1;

    private bool _playerHasArrows = false;

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

    // Start is called before the first frame update
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInventory = GetComponent<PlayerInventorySystem>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
        _focalPt = _mainCamera.transform.parent;
    }

    // Update is called once per frame
    private void Update()
    {
        _playerHasArrows = _playerInventory.HasArrows();

        if (_isAiming && _playerHasArrows)
        {
            _bow.EquipBow();

            _isFirstPerson = true;

            Aim();
        }
        else
        {
            _isFirstPerson = false;
            _bow.RemoveCrosshair();
            _bow.DisarmBow();
            _bow.DisableArrow();
        }

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
            //RotateToCameraView();

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
        if (_isFirstPerson)
        {
            _mainCamera.transform.localPosition = _aimOffset;
        }
        else
        {
            _mainCamera.transform.localPosition = _cameraOffset;
        }
    }

    //private void RotateToCameraView()
    //{
    //    Vector3 camFocalPt = _focalPt.position;
    //    Vector3 lookPoint = camFocalPt + (_focalPt.forward * _lookDistance);
    //    Vector3 dir = lookPoint - transform.position;

    //    Quaternion lookRotation = Quaternion.LookRotation(dir);
    //    // Rotate player in Y only
    //    lookRotation.x = 0;
    //    lookRotation.z = 0;

    //    Quaternion finalRotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * _lookSpeed);
    //    transform.rotation = finalRotation;
    //}

    // Handles PlayerFootstep via AnimationEvent
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_characterController.center), FootstepAudioVolume);
            }
        }
    }

    // Handles 'Player_Aim' context from InputSystem
    public void OnPlayerAim(InputAction.CallbackContext ctx)
    {
        _isAiming = ctx.ReadValue<float>() == 1;

        if (_playerHasArrows)
            _animator.SetBool("aim", _isAiming);
    }



    // Handles 'Player_FirstPersonView' context from InputSystem
    public void OnPlayerFirstPersonView(InputAction.CallbackContext ctx)
    {
        _isFirstPerson = ctx.ReadValue<float>() == 1;
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

    // Handles 'Player_Fire' context from InputSystem
    public void OnPlayerFire(InputAction.CallbackContext ctx)
    {
        if (_isAiming && _playerHasArrows)
            _animator.SetBool("pullString", true);
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

    private void Aim()
    {
        Vector3 camPosition = _mainCamera.transform.position;
        Vector3 dir = _mainCamera.transform.forward;

        // rotate player to 'forward' direction of camera
        transform.rotation = Quaternion.LookRotation(dir);

        _ray = new Ray(camPosition, dir);

        if (Physics.Raycast(_ray, out _hit, 500f, _aimLayers))
        {
            _hitDetected = true;
            Debug.DrawLine(_ray.origin, _hit.point, Color.green);

            _bow.ShowCrosshair(_hit.point);
        }
        else
        {
            _hitDetected = false;
            _bow.RemoveCrosshair();
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_isAiming && _playerHasArrows)
        {
            _animator.SetLookAtWeight(1f);
            _animator.SetLookAtPosition(_ray.GetPoint(_lookAtPoint));
        }
        else
        {
            _animator.SetLookAtWeight(0f);
        }
    }

    private void OnPlayerDrawArrow()
    {
        if (_playerHasArrows)
            _bow.DrawArrow();
    }

    private void OnPlayerPullString()
    {
        _bow.PullString();
    }
}
