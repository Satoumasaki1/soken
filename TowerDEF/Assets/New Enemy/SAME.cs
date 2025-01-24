using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class SAME : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // 優先ターゲットのタグを設定
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 150; // SAMEの体力
    public int maxHealth = 150; // 最大体力
    public int attackDamage = 80; // SAMEの攻撃力
    public float attackRange = 4f; // 攻撃範囲
    public float attackCooldown = 5f; // 攻撃クールダウン時間
    public float moveSpeed = 5f; // 通常の移動速度
    public float dashDistance = 6f; // 体当たり攻撃の距離
    public float dashDuration = 0.5f; // 体当たり攻撃の時間

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
    public bool isStunned = false; // スタン状態かどうか
    private float stunEndTime;

    // シーズン効果関連の設定
    private bool seasonEffectApplied = false;
    private GameManager.Season currentSeason;
    private int originalHealth;

    // **ヘルスバー設定**
    [Header("ヘルスバー設定")]
    public GameObject healthBarPrefab; // ヘルスバーのプレハブ
    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Transform cameraTransform;

    // **エフェクト＆サウンド設定**
    [Header("攻撃エフェクト＆サウンド設定")]
    public GameObject attackEffectPrefab; // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint; // エフェクト生成位置
    public AudioClip dashSound; // 体当たりサウンド
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
        else
        {
            Debug.LogError("ヘルスバープレハブが設定されていません！");
        }

        // AudioSourceを設定
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
                return; // スタン中は行動しない
            }
        }

        if (isPoisoned)
        {
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
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
        GameObject allyTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
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
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(fallbackTag))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                    Debug.Log($"SAMEが {collider.name} に攻撃を行いました。ダメージ: {attackDamage}");
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
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                moveSpeed = originalSpeed * 1.2f;
                agent.speed = moveSpeed;
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
                moveSpeed = originalSpeed * 1.4f;
                agent.speed = moveSpeed;
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                moveSpeed = originalSpeed * 0.9f;
                agent.speed = moveSpeed;
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                moveSpeed = originalSpeed * 0.6f;
                agent.speed = moveSpeed;
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