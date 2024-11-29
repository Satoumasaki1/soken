using UnityEngine;
using UnityEngine.AI;

public class Ikasan : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string targetTag = "Ally"; // 攻撃対象のタグを設定
    public string fallbackTag = "Base"; // ターゲットが見つからなかった場合の代替タグ

    private Transform target; // ターゲットのTransform
    public int health = 10; // Ikasanの体力
    public int attackDamage = 2; // 攻撃力
    public float attackRange = 3f; // 攻撃範囲
    public float attackCooldown = 1f; // 攻撃クールダウン時間

    private float lastAttackTime;
    private NavMeshAgent agent;

    // 麻痺毒関連の設定
    public bool isPoisoned = false; // 麻痺毒状態かどうか
    public float poisonDuration = 5f; // 麻痺毒の持続時間
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // 麻痺毒によるスピード減少率
    private float originalAttackCooldown;
    private float originalSpeed;
    private bool poisonEffectApplied = false;

    // スタン関連の設定
    private bool isStunned = false;
    private float stunEndTime;

    // シーズン効果適用フラグ
    private bool seasonEffectApplied = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        FindTarget();
        Debug.Log("Ikasanが初期化されました。ターゲットを探しています...");
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

        if (target == null)
        {
            FindTarget();
        }

        if (target != null)
        {
            // ターゲットに向かって移動する
            agent.SetDestination(target.position);
            Debug.Log($"ターゲットに向かって移動中: {target.name}");

            // 攻撃範囲内にターゲットがいる場合、攻撃する
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                Debug.Log("AttackTargetメソッドを呼び出します...");
                AttackTarget();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            Debug.Log("ターゲットが見つかりません。");
        }
    }

    void FindTarget()
    {
        // 優先ターゲット（Allyタグ）を探す
        GameObject allyTarget = GameObject.FindGameObjectWithTag(targetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            Debug.Log($"ターゲットを発見: {target.name} (Ally)");
            return;
        }

        // Allyが見つからない場合、Baseタグのターゲットを探す
        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
            Debug.Log($"ターゲットを発見: {target.name} (Base)");
        }
    }

    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log($"ターゲットに攻撃しました: {target.name}, ダメージ: {attackDamage}");
        }
        else
        {
            Debug.Log($"ターゲット {target.name} は攻撃対象としてIDamageableを持っていません。");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Ikasanがダメージを受けました: {damageAmount}, 残り体力: {health}");
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
        // Ikasanが倒れた際の処理（例えば破壊など）
        Debug.Log($"{name} が倒れました。");
        Destroy(gameObject);
    }

    // シーズンの効果を適用するメソッド
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.8f);
                health = Mathf.Max(health - 3, 1);
                Debug.Log("春のデバフが適用されました: 体力と攻撃力が減少");
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health += 5;
                Debug.Log("夏のバフが適用されました: 体力と攻撃力が増加");
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                health = Mathf.Max(health - 5, 1);
                Debug.Log("秋のデバフが適用されました: 体力と攻撃力が大幅に減少");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.4f);
                health += 10;
                Debug.Log("冬のバフが適用されました: 体力と攻撃力が大幅に増加");
                break;
        }

        seasonEffectApplied = true;
    }

    // シーズンの効果をリセットするメソッド
    public void ResetSeasonEffect()
    {
        attackDamage = 2;
        health = 10;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}
