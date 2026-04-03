using UnityEngine;

public class HealthManager
{
    private int maxHealth;
    private int currentHealth;
    private bool isDead = false;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    public HealthManager(int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
    }

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

    public void Reset()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}