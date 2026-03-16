using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [Header("Speed")]
    [SerializeField] private float _walkingSpeed = 0.8f;
    [SerializeField] private float _runningSpeed = 1.2f;

    [Header("Range")]
    [SerializeField] private float _chaseDistance = 10.0f;
    [SerializeField] private float _forgetDistance = 15.0f;

    // Components
    private GameObject _player;
    private Animator _anim;

    // Enemy states
    private enum STATE
    {
        IDLE,
        WANDER,
        ATTACK,
        CHASE,
        DEAD
    }

    // Local variables
    private STATE _currentState;
    private DungeonGenerator _dungeonFloor;
    private Room _parentRoom;
    private int _roomNum;

    private void Awake()
    {
        // Initialize components
        _player = GameObject.FindWithTag("Player");
        _anim = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentState = STATE.IDLE;

        _dungeonFloor = this.transform.parent.gameObject.transform.parent.GetComponent<DungeonGenerator>();

        _roomNum = int.Parse(this.transform.name.Substring(5));

        _parentRoom = _dungeonFloor.DungeonRoom(_roomNum - 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentState == STATE.DEAD)
            return;

        switch (_currentState)
        {
            case STATE.IDLE:
                if (IsPlayerWithinRange())
                {
                    _currentState = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    _currentState = STATE.WANDER;
                }
                break;

            case STATE.WANDER:
                break;

            case STATE.CHASE:
                break;

            case STATE.ATTACK:
                break;
        }
    }

    // Set all anim bool states to false
    private void ResetStates()
    {
        _anim.SetBool("isWalking", false);
        _anim.SetBool("isAttacking", false);
        _anim.SetBool("isRunning", false);
        _anim.SetBool("isDead", false);
    }

    // Calculate distance to the player
    private float DistanceToPlayer()
    {
        return Vector3.Distance(_player.transform.position, this.transform.position);
    }

    // Check if the player is within visible range
    private bool IsPlayerWithinRange()
    {
        return DistanceToPlayer() <= _chaseDistance;
    }

    // Check if player is out of chase range
    private bool ForgetPlayer()
    {
        return DistanceToPlayer() > _forgetDistance;
    }
}
