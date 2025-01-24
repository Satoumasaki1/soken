using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class KAZIKI : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // 優先ターゲットのタグを設定
    public string fallbackTag = "Base"; // 最後に狙うターゲットのタグ

    private Transform target; // ターゲットのTransform
    public int health = 60; // KAZIKIの体力
    public int maxHealth = 60; // 最大体力
    public int attackDamage = 40; // 突進攻撃の威力
    public float attackRange = 6f; // 攻撃範囲
    public float attackCooldown = 3f; // 攻撃クールダウン時間
    public float moveSpeed = 8f; // 通常の移動速度

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

    // スタン関連の設定
    private bool isStunned = false;
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
    public GameObject attackEffectPrefab; // 突進攻撃のエフェクト
    public Transform effectSpawnPoint; // エフェクト生成位置
    public AudioClip dashSound; // 突進攻撃時のサウンド
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
            healthBarInstance.transform.localPosition = new Vector3(0, 2.0f, 0); // 頭上に配置
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

        if (isCharging || isDashing) return;

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
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
                    StartCoroutine(ChargeAndDashAttack());
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

    private IEnumerator ChargeAndDashAttack()
    {
        isCharging = true;
        agent.isStopped = true; // チャージ中は停止
        yield return new WaitForSeconds(dashChargeTime);

        isCharging = false;
        isDashing = true;
        agent.isStopped = false;

        Vector3 dashDirection = transform.forward; // 突進方向
        float dashStartTime = Time.time;

        PlayAttackEffect(); // エフェクト＆サウンド再生

        while (Time.time < dashStartTime + (dashDistance / dashSpeed))
        {
            agent.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // 突進攻撃処理
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

        TakeDamage((int)(health * 0.1f)); // 突進後の反動でHPを10%減少
        if (health <= 0)
        {
            Die();
        }
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
                moveSpeed = originalSpeed * 0.9f;
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                break;
            case GameManager.Season.Summer:
                health += 10;
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                break;
            case GameManager.Season.Autumn:
                moveSpeed = originalSpeed * 0.8f;
                attackDamage = Mathf.RoundToInt(attackDamage * 0.8f);
                health -= 10;
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
        moveSpeed = originalSpeed;
        attackDamage = 40;
        health = originalHealth;
        seasonEffectApplied = false;
    }
}