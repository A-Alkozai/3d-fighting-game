using UnityEngine;

// Tracks a player's health, handles damage and KO detection
public class HealthManager
{
    private int maxHealth;
    private int currentHealth;
    private bool isDead = false;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    // Set max health and start at full
    public HealthManager(int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
    }

    // Subtract damage from current health, trigger KO if health hits 0
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"[Health] Took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            Debug.Log("[Health] KO!");
        }
    }

    // Restore to full health and clear KO flag (used between rounds)
    public void Reset()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    // Returns health as a 0-1 float for UI fill bars
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}