using UnityEngine;
using UnityEngine.AI;

public class SAME : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // 優先ターゲットのタグを設定
    public string fallbackTag = "Base"; // 最後に狩うターゲットのタグ

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

    // スタン関連の設定
    public bool isStunned = false; // スタン状態かどうか
    private float stunEndTime;

    // シーズン効果関連の設定
    private bool seasonEffectApplied = false;
    private GameManager.Season currentSeason;
    private int originalHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // 移動速度を設定
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        originalHealth = health;
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

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                // 攻撃範囲内にターゲットがいる場合、移動を停止して攻撃する
                agent.isStopped = true;
                if (Time.time > lastAttackTime + attackCooldown)
                {
                    PerformAreaAttack();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // 攻撃範囲外の場合はターゲットに向かって移動する
                agent.isStopped = false;
                agent.SetDestination(target.position);
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
        // SAMEが倒れた際の処理（例えば破壊など）
        Debug.Log($"{name} が倒れました。");
        Destroy(gameObject);
    }

    // シーズンの効果を適用するメソッド
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied && this.currentSeason == currentSeason) return;

        ResetSeasonEffect();
        this.currentSeason = currentSeason;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                moveSpeed = originalSpeed * 1.2f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は春のシーズンで強化されました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
                moveSpeed = originalSpeed * 1.4f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は夏のシーズンで大幅に強化されました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                moveSpeed = originalSpeed * 0.9f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は秋のシーズンで若干弱体化しました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                moveSpeed = originalSpeed * 0.6f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は冬のシーズンで大幅に弱体化しました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 80;
        moveSpeed = originalSpeed;
        agent.speed = moveSpeed;
        seasonEffectApplied = false;
    }
}
