using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class UTUBO : MonoBehaviour, IDamageable, IStunnable
{
    public string primaryTargetTag = "koukaku"; // 優先ターゲットのタグを設定
    public string secondaryTargetTag = "Ally"; // 次に優先するターゲットのタグ
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 35; // UTUBOの体力
    public int attackDamage = 10; // 攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 3f; // 攻撃クールダウン時間
    public float biteChargeTime = 2f; // 連続噛みつきのチャージ時間

    private float lastAttackTime;
    private NavMeshAgent agent;
    private bool isChargingBite = false;

    // 麻痺毒関連の設定
    public bool isPoisoned = false; // 麻痺毒状態かどうか
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // 麻痺毒によるスピード減少率
    private float originalAttackCooldown;
    private float originalSpeed;
    private bool poisonEffectApplied = false;

    // スタン関連の設定
    public bool isStunned = false; // スタン状態かどうか
    private float stunEndTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        FindTarget();
    }

    void Update()
    {
        if (isStunned)
        {
            if (Time.time > stunEndTime)
            {
                RemoveStunEffect();
            }
            else
            {
                return; // スタン中は行動しない
            }
        }

        if (isPoisoned)
        {
            // 麻痺毒の効果が続く間、移動速度と攻撃クールダウンが減少する
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(secondaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            // ターゲットに向かって移動する
            agent.SetDestination(target.position);

            // 攻撃範囲内にターゲットがいる場合、攻撃の準備をする
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown && !isChargingBite)
            {
                StartCoroutine(ChargeAndBite());
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

    IEnumerator ChargeAndBite()
    {
        isChargingBite = true;
        yield return new WaitForSeconds(biteChargeTime);
        AttackTarget();
        lastAttackTime = Time.time;
        isChargingBite = false;
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

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.isStopped = true; // 移動を停止する
        Debug.Log($"{name} がスタン状態になりました。持続時間: {duration}秒");
    }

    private void RemoveStunEffect()
    {
        isStunned = false;
        agent.isStopped = false; // 移動を再開する
        Debug.Log($"{name} のスタン効果が解除されました。");
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
        // UTUBOが倒れた際の処理（例えば破壊など）
        Debug.Log($"{name} が倒れました。");
        Destroy(gameObject);
    }
}
