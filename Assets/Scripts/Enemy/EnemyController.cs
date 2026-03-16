using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

    [Header("Speed")]
    [SerializeField] private float _walkingSpeed = 0.8f;
    [SerializeField] private float _runningSpeed = 1.2f;

    [Header("Range")]
    [SerializeField] private float _chaseDistance = 10.0f;
    [SerializeField] private float _forgetDistance = 15.0f;
    [SerializeField] private float _stoppingDistance = 1.0f;

    // Components
    private GameObject _player;
    private Animator _anim;
    private NavMeshAgent _agent;

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
    private int _dungeonFloorNum;
    private Room _parentRoom;
    private int _roomNum;

    private void Awake()
    {
        // Initialize components
        _player = GameObject.FindWithTag("Player");
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initial state
        _currentState = STATE.IDLE;

        // Parent dungeon floor
        _dungeonFloor = this.transform.parent.gameObject.transform.parent.GetComponent<DungeonGenerator>();

        // Parse the dungeon floor number from its name
        _dungeonFloorNum = int.Parse(_dungeonFloor.name.Substring(13));

        // Parse the room number from object name
        _roomNum = int.Parse(this.transform.name.Substring(5));

        // Obtain the room and its properties
        _parentRoom = _dungeonFloor.DungeonRoom(_roomNum - 1);
    }

    // Update is called once per frame
    void Update()
    {
        // Enemy is dead, do not update
        if (_currentState == STATE.DEAD)
            return;

        // Player is not on this floor
        if (_dungeonFloorNum != GameManager.Instance.PlayerFloor)
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
                // Only if agent has no path or reach end of current path
                if (!_agent.hasPath)
                {
                    // Define the limits of the room to randomly traverse
                    Vector3 pos1 = new Vector3(
                        (_parentRoom.StartX + 0.75f) * _dungeonFloor.DungeonScale,
                        this.transform.position.y,
                        (_parentRoom.StartZ + 0.75f) * _dungeonFloor.DungeonScale);
                    Vector3 pos2 = new Vector3(
                        (_parentRoom.StartX + _parentRoom.Width - 0.75f) * _dungeonFloor.DungeonScale,
                        this.transform.position.y,
                        (_parentRoom.StartZ + 0.75f) * _dungeonFloor.DungeonScale);
                    Vector3 pos3 = new Vector3(
                        (_parentRoom.StartX + _parentRoom.Width - 0.75f) * _dungeonFloor.DungeonScale,
                        this.transform.position.y,
                        (_parentRoom.StartZ + _parentRoom.Depth - 0.75f) * _dungeonFloor.DungeonScale);
                    Vector3 pos4 = new Vector3(
                        (_parentRoom.StartX + 0.75f) * _dungeonFloor.DungeonScale,
                        this.transform.position.y,
                        (_parentRoom.StartZ + _parentRoom.Depth - 0.75f) * _dungeonFloor.DungeonScale);

                    // Randomly select X & Z as a float
                    float newX = UnityEngine.Random.Range(pos1.x, pos3.x);
                    float newZ = UnityEngine.Random.Range(pos1.z, pos3.z);

                    Vector3 dest = new Vector3(newX, 0.0f, newZ);
                    _agent.SetDestination(dest);
                    _agent.stoppingDistance = 0;

                    ResetStates();
                    _agent.speed = _walkingSpeed;
                    _anim.SetBool("isWalking", true);
                    //_audioSource.PlayOneShot(_walking);
                }
                if (IsPlayerWithinRange())
                {
                    _currentState = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    _currentState = STATE.IDLE;

                    ResetStates();
                    _agent.ResetPath();
                }
                break;

            case STATE.CHASE:
                _agent.ResetPath();
                _agent.SetDestination(_player.transform.position);
                _agent.stoppingDistance = _stoppingDistance;

                ResetStates();
                _agent.speed = _runningSpeed;
                _anim.SetBool("isRunning", true);

                if (_agent.remainingDistance <= _agent.stoppingDistance + 1 && !_agent.pathPending)
                {
                    _currentState = STATE.ATTACK;
                }

                if (ForgetPlayer())
                {
                    _currentState = STATE.WANDER;
                    _agent.ResetPath();
                }
                break;

            case STATE.ATTACK:
                ResetStates();
                _anim.SetBool("isAttacking", true);

                // Set enemy to look at player
                transform.LookAt(_player.transform.position);

                if (DistanceToPlayer() > _agent.stoppingDistance + 1)
                    _currentState = STATE.CHASE;
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
