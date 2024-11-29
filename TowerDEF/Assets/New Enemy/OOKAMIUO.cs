using UnityEngine;
using UnityEngine.AI;

public class OOKAMIUO : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string targetTag = "koukaku"; // 攻撃対象のタグを設定
    public string fallbackTag = "Base"; // ターゲットが見つからなかった場合の代替タグ

    private Transform target; // ターゲットのTransform
    public int health = 25; // OOKAMIUOの体力
    public int attackDamage = 8; // 攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 2f; // 攻撃クールダウン時間
    public float moveSpeed = 3.5f; // OOKAMIUOの移動速度

    private float lastAttackTime;
    private NavMeshAgent agent;

    // 麻痹毒関連の設定
    public bool isPoisoned = false; // 麻痹毒状態かどうか
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // 麻痹毒によるスピード減少率
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
            // 麻痹毒の効果が続く間、移動速度と攻撃クールダウンが減少する
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

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
        // 優先ターゲット（koukakuタグ）を探す
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(targetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        // koukakuが見つからない場合、Baseタグのターゲットを探す
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
            Debug.Log($"{name} が麻痹毒の効果を受けました。持続時間: {duration}秒、スロー効果: {slowEffect}");
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
        Debug.Log($"{name} の麻痹毒の効果が解除されました。");
    }

    private void Die()
    {
        // OOKAMIUOが倒れた際の処理（例えば破壊など）
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
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                moveSpeed = originalSpeed * 1.1f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は春のシーズンで強化されました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                moveSpeed = originalSpeed * 1.2f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は夏のシーズンで大幅に強化されました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.8f);
                moveSpeed = originalSpeed * 0.9f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は秋のシーズンで弱体化しました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.6f);
                moveSpeed = originalSpeed * 0.7f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} は冬のシーズンで大幅に弱体化しました。攻撃力: {attackDamage}, 移動速度: {moveSpeed}");
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 8;
        moveSpeed = originalSpeed;
        agent.speed = moveSpeed;
        seasonEffectApplied = false;
    }
}
