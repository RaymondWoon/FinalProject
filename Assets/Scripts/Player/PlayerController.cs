using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private float MoveSpeed = 5.0f;
    [SerializeField] private float SprintSpeed = 10.0f;

    private CharacterController _characterController;

    private Vector3 _moveVec = Vector3.zero;
    private bool _isSprinting = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Movement keys depressed by user
        if (_moveVec.magnitude > 0)
        {
            // set player speed based on move speed, sprint speerd and if the sprint trigger is pressed
            float playerSpeed = _isSprinting ? SprintSpeed : MoveSpeed;

            Debug.Log(_moveVec);
            Debug.Log(playerSpeed);

            Vector3 movement = new Vector3(_moveVec.x, 0.0f, _moveVec.z) * playerSpeed * Time.fixedDeltaTime;

            _characterController.Move(movement);
        }
        else
        {
            _moveVec = Vector3.zero;
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

            Debug.Log(xyInput);

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
}
