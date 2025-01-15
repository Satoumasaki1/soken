using UnityEngine;

public class Uni1 : MonoBehaviour, IUpgradable
{
    public int maxHealth = 10;
    private float currentHealth;
    public int attackPower = 5;
    public float attackRange = 2f;
    public float attackInterval = 1f;
    private float nextAttackTime = 0f;
    private float targetSearchInterval = 0.5f; // 敵の検索間隔
    private float nextTargetSearchTime = 0f;
    private Collider[] nearbyEnemies = new Collider[10];
    private Transform currentTarget;

    public void OnApplicationQuit()　//追加
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius)//追加
    {
        maxHealth += additionalHp;
        attackPower += additionalDamage;
        Debug.Log(gameObject.name + " upgraded! HP: " + maxHealth + ", Damage: " + attackPower);
    }

    public void SaveState()//追加
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", maxHealth);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackPower);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()//追加
    {
        if (PlayerPrefs.HasKey($"{gameObject.name}_HP"))
        {
            maxHealth = PlayerPrefs.GetInt($"{gameObject.name}_HP");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Damage"))
        {
            attackPower = PlayerPrefs.GetInt($"{gameObject.name}_Damage");
        }

        Debug.Log($"{gameObject.name} state loaded! HP: { maxHealth}, Damage: {attackPower}");
    }

    void Start()
    {
        LoadState();//追加

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > attackRange)
            {
                if (Time.time >= nextTargetSearchTime)
                {
                    FindNewTarget();
                    nextTargetSearchTime = Time.time + targetSearchInterval;
                }
            }
            else
            {
                AttackCurrentTarget();
            }
        }
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

    // 回復処理
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // ユニットが死亡した際の処理
    private void Die()
    {
        // ユニットが倒れた時の処理
        Debug.Log(gameObject.name + " has been defeated.");
        Destroy(gameObject);
    }

    // 現在の体力を取得するメソッド（必要に応じて）
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // 新しい攻撃対象を探す
    private void FindNewTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, nearbyEnemies, (int)LayerMask.GetMask("Enemy"));
        if (hitCount > 0)
        {
            currentTarget = nearbyEnemies[0].transform;
        }
    }

    // 現在の攻撃対象に攻撃する
    private void AttackCurrentTarget()
    {
        if (currentTarget != null)
        {
            Health enemyHealth = currentTarget.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackPower);
                Debug.Log(gameObject.name + " attacked " + currentTarget.gameObject.name + " for " + attackPower + " damage.");
                nextAttackTime = Time.time + attackInterval;
            }
        }
    }
}
