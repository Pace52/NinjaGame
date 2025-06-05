using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public TMP_Text healthText;
    public TMP_Text coinText;
    public TMP_Text snailText;

    void Start()
    {
        // Initialize UI
        UpdateHealth(PlayerHealth.OnHealthChanged != null ? FindObjectOfType<PlayerHealth>().health : 3);
        UpdateCoins(PlayerStateMachine.coinCount);
        UpdateSnails(SnailEnemy.DeadSnailCount);

        PlayerHealth.OnHealthChanged += UpdateHealth;
        PlayerStateMachine.OnCoinCountChanged += UpdateCoins;
    }

    void Update()
    {
        // Dead snail count is static, update every frame
        UpdateSnails(SnailEnemy.DeadSnailCount);
    }

    void UpdateHealth(int health)
    {
        healthText.text = $"Health: {health}";
    }

    void UpdateCoins(int coins)
    {
        coinText.text = $"Coins: {coins}";
    }

    void UpdateSnails(int snails)
    {
        snailText.text = $"Dead Snails: {snails}";
    }

    void OnDestroy()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealth;
        PlayerStateMachine.OnCoinCountChanged -= UpdateCoins;
    }
} 