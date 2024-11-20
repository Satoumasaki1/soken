using UnityEngine;

public class Base : MonoBehaviour, IDamageable
{
    public int health = 100; // ‹’“_‚Ì‘Ì—Í

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // ‹’“_‚ª”j‰ó‚³‚ê‚½Û‚Ìˆ—i—á‚¦‚Î”j‰ó‚È‚Çj
        Destroy(gameObject);
    }
}
