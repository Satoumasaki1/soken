using UnityEngine;
using UnityEngine.AI;

public class ONIDARUMA_OKOZE : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "koukaku"; // 優先ターゲットのタグを設定
    public string secondaryTargetTag = "Ally"; // 次に優先するターゲットのタグ
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 60; // ONIDARUMA_OKOZEの体力
    public int attackDamage = 30; // 攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 2f; // 攻撃クールダウン時間
    public float moveSpeed = 2f; // 移動速度がとても遅い
    public int thornDamage = 10; // 棘の反撃ダメージ
    public float poisonDamage = 2f; // 毒のダメージ
    public float poisonDuration = 5f; // 毒の持続時間
    public float poisonInterval = 1f; // 毒のダメージ間隔

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
    private bool isStunned = false;
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
                return; // スタン中は何もしない
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
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                // 攻撃範囲内にターゲットがいる場合、移動を停止して攻撃する
                agent.isStopped = true;
                AttackTarget();
                lastAttackTime = Time.time;
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
        if (Time.time > lastAttackTime + attackCooldown)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
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
        else
        {
            ThornCounterAttack();
        }
    }

    private void ThornCounterAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(secondaryTargetTag))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(thornDamage);
                    Debug.Log("ONIDARUMA_OKOZEが棘の反撃を行いました: " + collider.name);
                    StartCoroutine(ApplyPoison(damageable));
                }
            }
        }
    }

    private System.Collections.IEnumerator ApplyPoison(IDamageable damageable)
    {
        float elapsedTime = 0f;
        MonoBehaviour damageableObject = damageable as MonoBehaviour;

        while (elapsedTime < poisonDuration)
        {
            if (damageableObject == null || damageableObject.gameObject == null)
            {
                // 対象が破壊されていたらコルーチンを終了する
                yield break;
            }

            damageable.TakeDamage((int)poisonDamage);
            elapsedTime += poisonInterval;
            yield return new WaitForSeconds(poisonInterval);
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
        agent.isStopped = true; // スタン中は移動を止める
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
        // ONIDARUMA_OKOZEが倒れた際の処理（例えば破壊など）
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
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                health = Mathf.RoundToInt(health * 0.8f);
                Debug.Log($"{name} は春のシーズンで弱体化しました。攻撃力: {attackDamage}, 体力: {health}");
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health = Mathf.RoundToInt(health * 1.2f);
                Debug.Log($"{name} は夏のシーズンで強化されました。攻撃力: {attackDamage}, 体力: {health}");
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                health = Mathf.RoundToInt(health * 0.8f);
                Debug.Log($"{name} は秋のシーズンで弱体化しました。攻撃力: {attackDamage}, 体力: {health}");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health = Mathf.RoundToInt(health * 1.2f);
                Debug.Log($"{name} は冬のシーズンで強化されました。攻撃力: {attackDamage}, 体力: {health}");
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 30;
        health = originalHealth;
        seasonEffectApplied = false;
        Debug.Log($"{name} のシーズン効果がリセットされました。攻撃力: {attackDamage}, 体力: {health}");
    }
}
