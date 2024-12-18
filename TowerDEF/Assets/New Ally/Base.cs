using UnityEngine;

public class Base : MonoBehaviour, IDamageable
{
    public int health = 100; // 拠点の体力

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
        // 拠点が破壊された際の処理（例えば破壊など）
        Destroy(gameObject);
    }
}
