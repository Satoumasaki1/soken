using UnityEngine;

public class Uni1 : MonoBehaviour, IUpgradable
{
    public int maxHealth = 10;
    private float currentHealth;
    public int attackPower = 5;
    public float attackRange = 2f;
    public float attackInterval = 1f;
    private float nextAttackTime = 0f;
    private float targetSearchInterval = 0.5f; // �G�̌����Ԋu
    private float nextTargetSearchTime = 0f;
    private Collider[] nearbyEnemies = new Collider[10];
    private Transform currentTarget;

    public void OnApplicationQuit()�@//�ǉ�
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius)//�ǉ�
    {
        maxHealth += additionalHp;
        attackPower += additionalDamage;
        Debug.Log(gameObject.name + " upgraded! HP: " + maxHealth + ", Damage: " + attackPower);
    }

    public void SaveState()//�ǉ�
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", maxHealth);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackPower);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()//�ǉ�
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
        LoadState();//�ǉ�

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

    // �_���[�W���󂯂����̏���
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // �񕜏���
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // ���j�b�g�����S�����ۂ̏���
    private void Die()
    {
        // ���j�b�g���|�ꂽ���̏���
        Debug.Log(gameObject.name + " has been defeated.");
        Destroy(gameObject);
    }

    // ���݂̗̑͂��擾���郁�\�b�h�i�K�v�ɉ����āj
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // �V�����U���Ώۂ�T��
    private void FindNewTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, nearbyEnemies, (int)LayerMask.GetMask("Enemy"));
        if (hitCount > 0)
        {
            currentTarget = nearbyEnemies[0].transform;
        }
    }

    // ���݂̍U���ΏۂɍU������
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
