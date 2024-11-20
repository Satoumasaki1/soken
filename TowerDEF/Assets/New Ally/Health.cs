using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // ダメージを受けた時の処理
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 体力の回復処理
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // 現在の体力を取得するメソッド
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // ユニットが死亡した際の処理
    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated.");
        Destroy(gameObject);
    }
}
