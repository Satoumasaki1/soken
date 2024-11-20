using UnityEngine;
using UnityEngine.AI;

public class AKAEI : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "koukaku"; // 優先ターゲットのタグを設定
    public string secondaryTargetTag = "Ally"; // 次に優先するターゲットのタグ
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 35; // AKAEIの体力
    public int attackDamage = 8; // 攻撃力
    public float attackRange = 6f; // 攻撃範囲が広い
    public float attackCooldown = 2f; // 攻撃クールダウン時間
    public float poisonDamage = 2f; // 毒のダメージ
    public float poisonDuration = 5f; // 毒の持続時間
    public float poisonInterval = 1f; // 毒のダメージ間隔

    private float lastAttackTime;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        FindTarget();
    }

    void Update()
    {
        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(secondaryTargetTag) && !target.CompareTag(fallbackTag)))
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
        // 優先ターゲット（koukakuタグ）を探す
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        // 次に優先するターゲット（Allyタグ）を探す
        GameObject allyTarget = GameObject.FindGameObjectWithTag(secondaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // それでも見つからない場合、Baseタグのターゲットを探す
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
            StartCoroutine(ApplyPoison(damageable));
        }
    }

    private System.Collections.IEnumerator ApplyPoison(IDamageable damageable)
    {
        float elapsedTime = 0f;
        while (elapsedTime < poisonDuration)
        {
            damageable.TakeDamage((int)poisonDamage);
            elapsedTime += poisonInterval;
            yield return new WaitForSeconds(poisonInterval);
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
        // AKAEIが倒れた際の処理（例えば破壊など）
        Destroy(gameObject);
    }
}
