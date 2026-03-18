using DungeonEscape.Inventory;
using Unity.VisualScripting;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private InventoryItemData _arrowItemData;
    [SerializeField] private Rigidbody _arrowPrefab;
    [SerializeField] private Transform _arrowPos;
    [SerializeField] private Transform _arrowEquipParent;
    [SerializeField] private float _arrowForce = 120f;

    [Header("Bow Equip & Disarm Settings")]
    [SerializeField] private Transform _equipPos;
    [SerializeField] private Transform _disarmPos;
    [SerializeField] private Transform _equipParent;
    [SerializeField] private Transform _disarmParent;

    [Header("Bow-String Settings")]
    [SerializeField] private Transform _bowString;
    [SerializeField] private Transform _stringInitialPos;
    [SerializeField] private Transform _stringHandPullPos;
    [SerializeField] private Transform _stringInitialParent;

    [Header("Bow Audio Settings")]
    [SerializeField] private AudioClip _drawArrowClip;
    [SerializeField] private AudioClip _pullStringClip;
    [SerializeField] private AudioClip _releaseStringClip;

    [Header("Crosshair Settings")]
    [SerializeField] private GameObject _crosshairPrefab;

    // Local parameters
    private GameObject _currentCrosshair;
    private Rigidbody _currentArrow;
    private AudioSource _bowAudio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _bowAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawArrow()
    {
        _arrowPos.gameObject.SetActive(true);

        _bowAudio.PlayOneShot(_drawArrowClip);
    }

    public void DisableArrow()
    {
        _arrowPos.gameObject.SetActive(false);
    }


    public void EquipBow()
    {
        this.transform.position = _equipPos.position;
        this.transform.rotation = _equipPos.rotation;
        this.transform.parent = _equipParent;
    }

    public void DisarmBow()
    {
        this.transform.position = _disarmPos.position;
        this.transform.rotation = _disarmPos.rotation;
        this.transform.parent = _disarmParent;
    }

    public void PullString()
    {
        _bowString.transform.position = _stringHandPullPos.position;
        _bowString.transform.parent = _stringHandPullPos;
    }

    public void PlayPullStringSound()
    {
        _bowAudio.PlayOneShot(_pullStringClip);
    }

    public void ReleaseString()
    {
        _bowString.transform.position = _stringInitialPos.position;
        _bowString.transform.parent = _stringInitialParent;
    }

    public void ShowCrosshair(Vector3 crosshairPos)
    {
        if (!_currentCrosshair)
            _currentCrosshair = Instantiate(_crosshairPrefab) as GameObject;

        _currentCrosshair.transform.position = crosshairPos;
        _currentCrosshair.transform.LookAt(Camera.main.transform);
    }

    public void RemoveCrosshair()
    {
        if (_currentCrosshair)
            Destroy(_currentCrosshair);
    }

    public void FireArrow(Vector3 hitPoint)
    {
        Vector3 dir = hitPoint - _arrowPos.position;

        _bowAudio.PlayOneShot(_releaseStringClip);

        _currentArrow = Instantiate(_arrowPrefab, _arrowPos.position, _arrowPos.rotation) as Rigidbody;
        _currentArrow.AddForce(dir * _arrowForce, ForceMode.Force);
    }


}
