using UnityEngine;
using UnityEngine.AI;

public class SAME : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "Ally"; // 優先ターゲットのタグを設定
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 150; // SAMEの体力が高い
    public int attackDamage = 80; // SAMEの攻撃力が高い
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 5f; // 攻撃クールダウン時間
    public float moveSpeed = 5f; // 通常の移動速度

    private float lastAttackTime;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // 移動速度を設定
        FindTarget();
    }

    void Update()
    {
        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            // ターゲットに向かって移動する
            agent.SetDestination(target.position);

            // 攻撃範囲内にターゲットがいる場合、攻撃する
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                PerformAreaAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    void FindTarget()
    {
        // 優先ターゲット（Allyタグ）を探す
        GameObject allyTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // Allyが見つからない場合、Baseタグのターゲットを探す
        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    void PerformAreaAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
        int targetCount = 0;
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(fallbackTag))
            {
                targetCount++;
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int totalDamage = attackDamage + (targetCount - 1) * 10; // 攻撃範囲にいるターゲット数に応じてダメージ増加
                    damageable.TakeDamage(totalDamage);
                    Debug.Log("SAMEが範囲攻撃を行いました: " + collider.name + " ダメージ: " + totalDamage);
                }
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    public int GetHealth()
    {
        return health;
    }

    private void Die()
    {
        // SAMEが倒れた際の処理（例えば破壊など）
        Destroy(gameObject);
    }
}
