using UnityEngine;

namespace DungeonEscape.Inventory
{
    public class InventoryItemAnimation : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _rotateSpeed;
        [SerializeField] private float _bobSpeed;
        [SerializeField] private float _bobHeight;

        private Vector3 _startPos;
        private bool _isBobbingUp;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // the start position for the pickup item
            _startPos = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            // rotate the pickup
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);

            // bobbing range
            Vector3 offset = _isBobbingUp ? new Vector3(0, _bobHeight / 2, 0) : new Vector3(0, -_bobHeight / 2, 0);

            // move in the bobbing direction
            transform.position = Vector3.MoveTowards(transform.position, _startPos + offset, _bobSpeed * Time.deltaTime);

            // limit bobbing to the range
            if (transform.position == _startPos + offset)
                _isBobbingUp = !_isBobbingUp;
        }
    }
}