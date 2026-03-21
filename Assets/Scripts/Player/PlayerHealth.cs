using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;

    // Components
    private Animator _animator;

    // Local Variables
    private int _currentHealth;

    // Public access
    public int PlayerCurrentHealth
    {
        get { return _currentHealth; }
        set {  _currentHealth = value; }
    }

    public int PlayerMaxHealth
    {
        get { return _maxHealth; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();

        // Initialse _currentHealth to _maxHealth to start game
        _currentHealth = _maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        // Update _currentHealth due to damage
        _currentHealth -= damage;

        // Check if player has died
        if (_currentHealth <= 0)
            Die();
    }

    public void Heal(int healAmount)
    {
        // Update _currentHealth with healAmount
        _currentHealth += healAmount;

        // Cannot exceed _maxHealth
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }

    private void Die()
    {
        _animator.SetBool("die", true);

        GameManager.Instance.CurrentGameState = GameManager.GameState.GameOver;
        GameManager.Instance.UpdateGameState(GameManager.GameState.GameOver);
        
    }
}
