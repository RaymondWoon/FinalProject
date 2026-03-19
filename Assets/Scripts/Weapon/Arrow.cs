using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private int _lifeSpan;
    [SerializeField] private int _damage;

    // Components
    private Rigidbody _rb;
    private BoxCollider _bc;

    // Local variables
    private bool _disableRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _bc = GetComponent<BoxCollider>();

        Destroy(this.gameObject, _lifeSpan);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_disableRotation && _rb.linearVelocity.magnitude > 0)
            transform.rotation = Quaternion.LookRotation(_rb.linearVelocity);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.isKinematic = true;
            _bc.isTrigger = true;
            _disableRotation = true;

            other.collider.gameObject.GetComponent<EnemyController>().TakeDamage(_damage);
        }
    }
}
