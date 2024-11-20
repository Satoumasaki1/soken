using UnityEngine;

public class Base : MonoBehaviour, IDamageable
{
    public int health = 100; // ���_�̗̑�

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
        // ���_���j�󂳂ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }
}
