using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class ONIDARUMA_OKOZE : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "koukaku"; // 優先ターゲットのタグを設定
    public string secondaryTargetTag = "Ally"; // 次に優先するターゲットのタグ
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 60; // オニダルマオコゼの体力
    public int maxHealth = 60; // 最大体力
    public int attackDamage = 30; // 攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 2f; // 攻撃クールダウン時間
    public float moveSpeed = 2f; // 移動速度が遅い
    public int thornDamage = 10; // 棘の反撃ダメージ
    public float poisonDamage = 2f; // 毒のダメージ
    public float poisonDuration = 5f; // 毒の持続時間
    public float poisonInterval = 1f; // 毒のダメージ間隔
    public float dashDistance = 6f; // 体当たり攻撃の距離
    public float dashDuration = 0.8f; // 体当たり攻撃の時間

    private float lastAttackTime;
    private NavMeshAgent agent;
    private bool isDashing = false;

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

    // ヘルスバー設定
    [Header("ヘルスバー設定")]
    public GameObject healthBarPrefab; // ヘルスバーのプレハブ
    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Transform cameraTransform;

    // 攻撃エフェクト＆サウンド
    [Header("攻撃エフェクト＆サウンド")]
    public GameObject attackEffectPrefab; // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint; // エフェクト生成位置
    public AudioClip dashSound; // 体当たり攻撃のサウンド
    private AudioSource audioSource;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // 移動速度を設定
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        originalHealth = health;

        // ヘルスバーを生成
        cameraTransform = Camera.main.transform;
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2.5f, 0); // 頭上に配置
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = health;
            }
        }

        // オーディオソースを設定
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

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

        if (target != null && !isDashing)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                agent.isStopped = true;

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    StartCoroutine(PerformDashAttack());
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

        healthSlider.value = health;
        healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - cameraTransform.position);
        healthBarInstance.SetActive(health < maxHealth);
    }

    private void OnDestroy()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
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

        // 体当たり攻撃の処理
        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        PlayAttackEffect(); // エフェクト＆サウンド再生

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 攻撃判定
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(secondaryTargetTag))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                    Debug.Log($"オニダルマオコゼが {collider.name} に攻撃を行いました。ダメージ: {attackDamage}");
                }
            }
        }

        isDashing = false;
    }

    private void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
        }

        if (dashSound != null && audioSource != null)
        {
            audioSource.clip = dashSound;
            audioSource.Play();
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
                    StartCoroutine(ApplyPoison(damageable));
                }
            }
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
        if (seasonEffectApplied && this.currentSeason == currentSeason) return;

        ResetSeasonEffect();
        this.currentSeason = currentSeason;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                health = Mathf.RoundToInt(health * 0.8f);
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health = Mathf.RoundToInt(health * 1.2f);
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                health = Mathf.RoundToInt(health * 0.8f);
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health = Mathf.RoundToInt(health * 1.2f);
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 30;
        health = originalHealth;
        seasonEffectApplied = false;
    }
}