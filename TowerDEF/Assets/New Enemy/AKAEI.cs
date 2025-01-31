using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class AKAEI : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
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

    // シーズン効果適用フラグ
    private bool seasonEffectApplied = false;

    // **攻撃エフェクト＆サウンド**
    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab;  // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;     // エフェクトを生成する位置
    public AudioClip attackSound;          // 攻撃時のサウンド
    private AudioSource audioSource;       // サウンド再生用

    // **体当たり攻撃の設定**
    [Header("体当たり攻撃設定")]
    public float dashDistance = 3.0f;      // 体当たりの移動距離
    public float dashDuration = 0.3f;      // 体当たりの前進時間
    public float returnDuration = 0.3f;    // 体当たり後の後退時間
    private bool isDashing = false;        // 体当たり中かどうか

    // **ヘルスバー**
    [Header("ヘルスバー設定")]
    public GameObject healthBarPrefab;     // ヘルスバーのプレハブ
    private GameObject healthBarInstance;  // 実際に生成されたヘルスバー
    private Slider healthSlider;           // ヘルスバーのスライダー
    private Transform cameraTransform;     // カメラのTransform

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;

        // AudioSourceを設定
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ヘルスバーを設定
        cameraTransform = Camera.main.transform;
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2.0f, 0); // 頭上に配置
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = health;
                healthSlider.value = health;
            }
        }
        else
        {
            Debug.LogError("ヘルスバープレハブが設定されていません！");
        }

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
                agent.isStopped = true;

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    if (!isDashing) StartCoroutine(PerformDashAttack());
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        // ヘルスバーの値を更新
        healthSlider.value = health;

        // ヘルスバーをカメラに向ける
        healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - cameraTransform.position);

        // ヘルスバーを表示/非表示
        healthBarInstance.SetActive(health < healthSlider.maxValue);
    }

    void FindTarget()
    {
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        GameObject allyTarget = GameObject.FindGameObjectWithTag(secondaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    private IEnumerator PerformDashAttack()
    {
        isDashing = true;

        // 前進
        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 攻撃
        if (target != null)
        {
            AttackTarget();
        }

        PlayAttackEffect();

        // 後退
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(dashPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void PlayAttackEffect()
    {
        // 攻撃エフェクト
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
        }

        // 攻撃サウンド
        if (attackSound != null && audioSource != null)
        {
            audioSource.clip = attackSound;
            audioSource.Play();
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

    private IEnumerator ApplyPoison(IDamageable damageable)
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

    public void ApplyPoison(float duration, float slowEffect)
    {
        isPoisoned = true;
        poisonEndTime = Time.time + duration;
        if (!poisonEffectApplied)
        {
            agent.speed = originalSpeed * slowEffect;
            attackCooldown = originalAttackCooldown * 2;
            poisonEffectApplied = true;
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.isStopped = true;
    }

    private void RemoveStunEffect()
    {
        isStunned = false;
        agent.isStopped = false;
    }

    private void RemovePoisonEffect()
    {
        isPoisoned = false;
        agent.speed = originalSpeed;
        attackCooldown = originalAttackCooldown;
        poisonEffectApplied = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                health = Mathf.Max(health - 5, 1);
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                health += 10;
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.8f);
                health = Mathf.Max(health - 10, 1);
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health += 15;
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 8;
        health = 35;
        seasonEffectApplied = false;
    }
}