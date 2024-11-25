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

    // 麻痺毒関連の設定
    public bool isPoisoned = false; // 麻痺毒状態かどうか
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // 麻痺毒によるスピード減少率
    private float originalAttackCooldown;
    private float originalSpeed;
    private bool poisonEffectApplied = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // 移動速度を設定
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        FindTarget();
    }

    void Update()
    {
        if (isPoisoned)
        {
            // 麻痺毒の効果が続く間、移動速度と攻撃クールダウンが減少する
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

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
        Debug.Log($"{name} がダメージを受けました: {damageAmount}, 残り体力: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    public void ApplyPoison(float duration, float slowEffect)
    {
        isPoisoned = true;
        poisonEndTime = Time.time + duration;
        if (!poisonEffectApplied)
        {
            agent.speed = originalSpeed * slowEffect; // 移動速度を減少させる
            attackCooldown = originalAttackCooldown * 2; // 攻撃クールダウンを長くする
            poisonEffectApplied = true;
            Debug.Log($"{name} が麻痺毒の効果を受けました。持続時間: {duration}秒、スロー効果: {slowEffect}");
        }
    }

    private void RemovePoisonEffect()
    {
        isPoisoned = false;
        agent.speed = originalSpeed; // 移動速度を元に戻す
        attackCooldown = originalAttackCooldown; // 攻撃クールダウンを元に戻す
        poisonEffectApplied = false;
        Debug.Log($"{name} の麻痺毒の効果が解除されました。");
    }

    private void Die()
    {
        // SAMEが倒れた際の処理（例えば破壊など）
        Debug.Log($"{name} が倒れました。");
        Destroy(gameObject);
    }
}
