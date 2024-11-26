using UnityEngine;
using UnityEngine.AI;

public class KAZIKI : MonoBehaviour, IDamageable
{
    public string primaryTargetTag = "Ally"; // 優先ターゲットのタグを設定
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 60; // KAZIKIの体力
    public int attackDamage = 40; // 突進攻撃の威力を上げる
    public float attackRange = 6f; // 攻撃範囲
    public float attackCooldown = 3f; // 攻撃クールダウン時間
    public float moveSpeed = 8f; // 通常の移動速度が高速

    private float lastAttackTime;
    private NavMeshAgent agent;
    public float dashChargeTime = 2.0f; // 突進攻撃のチャージ時間
    public float dashSpeed = 20f; // 突進攻撃の速度
    public float dashDistance = 15f; // 突進攻撃の距離

    private bool isCharging = false;
    private bool isDashing = false;

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

        if (isCharging || isDashing)
        {
            return; // チャージ中または突進中は動作を制御する
        }

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            // ターゲットに向かって移動する
            agent.SetDestination(target.position);

            // 攻撃範囲内にターゲットがいる場合、突進攻撃を準備する
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                StartCoroutine(ChargeAndDashAttack());
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

    System.Collections.IEnumerator ChargeAndDashAttack()
    {
        isCharging = true;
        agent.isStopped = true; // チャージ中は移動を停止する
        yield return new WaitForSeconds(dashChargeTime);

        isCharging = false;
        isDashing = true;
        agent.isStopped = false;

        Vector3 dashDirection = transform.forward; // 突進方向は現在の前方
        float dashStartTime = Time.time;

        while (Time.time < dashStartTime + (dashDistance / dashSpeed))
        {
            agent.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // 攻撃処理（貫通攻撃）
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1.0f, dashDirection, dashDistance);
        foreach (RaycastHit hit in hits)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null && (hit.collider.CompareTag(primaryTargetTag) || hit.collider.CompareTag(fallbackTag)))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log("KAZIKIが突進攻撃を行いました: " + hit.collider.name);
            }
        }

        // 突進後の反動でHPを10%失う
        TakeDamage((int)(health * 0.1f));
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
        // KAZIKIが倒れた際の処理（例えば破壊など）
        Debug.Log($"{name} が倒れました。");
        Destroy(gameObject);
    }
}
