using UnityEngine;
using UnityEngine.AI;

public class ISEEBI : MonoBehaviour, IDamageable
{
    public string targetTag = "koukaku"; // 攻撃対象のタグを設定
    public string fallbackTag = "Base"; // ターゲットが見つからなかった場合の代替タグ

    private Transform target; // ターゲットのTransform
    public int health = 15; // ISEEBIの体力
    public int attackDamage = 5; // 攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 2f; // 攻撃クールダウン時間

    private float lastAttackTime;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        FindTarget();
    }

    void Update()
    {
        if (target == null || !target.CompareTag(targetTag))
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
                AttackTarget();
                lastAttackTime = Time.time;
            }
        }
    }

    void FindTarget()
    {
        // 優先ターゲット（Allyタグ）を探す
        GameObject allyTarget = GameObject.FindGameObjectWithTag(targetTag);
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

    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
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

    private void Die()
    {
        // ISEEBIが倒れた際の処理（例えば破壊など）
        Destroy(gameObject);
    }
}
