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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // 移動速度を設定
        FindTarget();
    }

    void Update()
    {
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
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // KAZIKIが倒れた際の処理（例えば破壊など）
        Destroy(gameObject);
    }
}
