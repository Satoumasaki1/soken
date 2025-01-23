using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Haze : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Hazeの体力と最大体力
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 5;
    public int attackDamage = 5;

    // 攻撃関連の設定
    public float detectionRadius = 10f;
    public float attackCooldown = 3.0f;

    private Transform target;
    private float lastAttackTime;

    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    private Animator animator; // アニメーションを制御するためのAnimator

    // 攻撃エフェクト関連
    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab; // エフェクトのプレハブ (damage ef)
    public Transform effectSpawnPoint;   // エフェクトを生成する位置
    public AudioClip attackSound;           // 効果音のAudioClip
    private AudioSource audioSource;        // 効果音を再生するAudioSource

    // **新規追加: 体当たりの動きを制御するためのフィールド**
    [Header("体当たり設定")]
    public float dashDistance = 2.0f; // 前進する距離
    public float dashDuration = 0.2f; // 前進にかかる時間
    public float returnDuration = 0.2f; // 後退にかかる時間

    private bool isDashing = false; // 体当たり中かどうかを判定するフラグ

    // **体力バー関連の設定**
    [Header("体力バー関連")]
    public GameObject healthBarPrefab;      // 体力バーのプレハブ
    private GameObject healthBarInstance;   // 実際に生成された体力バー
    private Slider healthSlider;            // 体力バーのスライダーコンポーネント

    public void OnApplicationQuit()
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius)
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        detectionRadius += additionaRadius;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage + ", Radius: " + detectionRadius);
    }

    public void SaveState()
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()
    {
        if (PlayerPrefs.HasKey($"{gameObject.name}_HP"))
        {
            health = PlayerPrefs.GetInt($"{gameObject.name}_HP");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Damage"))
        {
            attackDamage = PlayerPrefs.GetInt($"{gameObject.name}_Damage");
        }

        Debug.Log($"{gameObject.name} state loaded! HP: {health}, Damage: {attackDamage}");
    }

    void Start()
    {
        LoadState();

        // GameManager参照の取得
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }

        animator = GetComponent<Animator>(); // Animatorの取得
        if (animator == null)
        {
            Debug.LogWarning("Animatorがアタッチされていません！");
        }

        // **体力バーの初期化**
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform); // 体力バーを生成
            healthBarInstance.transform.localPosition = new Vector3(0, 0.5f, 0); // キャラクターの頭上に配置

            healthSlider = healthBarInstance.GetComponentInChildren<Slider>(); // Slider コンポーネントを取得

            if (healthSlider != null)
            {
                healthSlider.maxValue = 1; // スライダーの最大値を 1 に設定
                healthSlider.value = (float)health / maxHealth; // 現在の体力に応じてスライダーの値を設定
            }
        }
        else
        {
            Debug.LogError("HealthBarPrefabが設定されていません！");
        }

        audioSource = gameObject.AddComponent<AudioSource>(); // AudioSourceを追加
        audioSource.playOnAwake = false; // 自動再生を無効化
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyBuffFromUdeppo();
        ApplyIrukaBuff();
        AttackOn();

        // **体力バーを更新**
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        float healthPercentage = (float)health / maxHealth;
        healthSlider.value = healthPercentage;

        // 体力が最大の場合は体力バーを非表示
        healthBarInstance.SetActive(health < maxHealth);

        // **体力バーの回転をカメラに合わせる**
        if (Camera.main != null)
        {
            healthBarInstance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    private void OnDestroy()
    {
        // キャラクターが削除された場合、体力バーも削除
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    private void OnMouseDown()
    {
        TryHeal();
    }

    public void TryHeal()
    {
        if (!gm.SelectedFeedType.HasValue) { Debug.Log("餌が選択されていません。"); return; }

        var selectedFeed = gm.SelectedFeedType.Value;
        if (selectedFeed == GameManager.ResourceType.OkiaMi ||
            selectedFeed == GameManager.ResourceType.Benthos ||
            selectedFeed == GameManager.ResourceType.Plankton)
        {
            if (gm.inventory[selectedFeed] > 0 && health < maxHealth)
            {
                health = Mathf.Min(health + 2, maxHealth);
                gm.inventory[selectedFeed]--;
                gm.UpdateResourceUI();

                // **回復時に体力バーを更新**
                UpdateHealthBar();

                Debug.Log($"{selectedFeed} で体力を回復しました。残り在庫: {gm.inventory[selectedFeed]}");
            }
            else
            {
                Debug.Log(health >= maxHealth ? "体力は既に最大です。" : $"{selectedFeed} の在庫が不足しています。");
            }
        }
        else
        {
            Debug.Log("この餌では回復できません。");
        }
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
            PlayAttackEffect(); // 攻撃エフェクトと効果音の再生
            AttackTarget();
            lastAttackTime = Time.time;
        }
    }

    void DetectEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Debug.Log("敵を検知しています...");
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                target = collider.transform;
                Debug.Log($"敵を検知しました: {target.name}");
                break;
            }
        }
    }

    void AttackTarget()
    {
        if (animator != null) animator.SetTrigger("Attack");

        if (!isDashing) StartCoroutine(PerformDash());

        PlayAttackEffect();

        if (ApplyBuffFromUdeppo())
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Debug.Log("範囲攻撃を実行中...");
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    IDamageable damageable = collider.GetComponent<IDamageable>();
                    damageable?.TakeDamage(attackDamage);
                }
            }
        }
        else if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;

        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(dashPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
        }
        else
        {
            Debug.LogWarning("攻撃エフェクトのプレハブまたは生成位置が設定されていません！");
        }
        // **効果音の再生**
        if (attackSound != null && audioSource != null)
        {
            audioSource.clip = attackSound; // 効果音を設定
            audioSource.Play(); // 効果音を再生
            Debug.Log("narase!!!!!!!!!");
        }
        else
        {
            Debug.LogWarning("攻撃効果音が設定されていません！");
        }

    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private bool ApplyBuffFromUdeppo()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool isUdeppoNearby = false;

        foreach (Collider collider in colliders)
        {
            Udeppo udeppo = collider.GetComponent<Udeppo>();
            if (udeppo != null && udeppo.gameObject != gameObject)
            {
                isUdeppoNearby = true;
                break;
            }
        }

        if (isUdeppoNearby && !maxHealthBuffApplied)
        {
            maxHealth += 20;
            health = Mathf.Min(health + 20, maxHealth);
            maxHealthBuffApplied = true;
        }
        else if (!isUdeppoNearby && maxHealthBuffApplied)
        {
            maxHealth -= 20;
            health = Mathf.Min(health, maxHealth);
            maxHealthBuffApplied = false;
        }

        return isUdeppoNearby;
    }

    private void ApplyIrukaBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool irukaNearby = false;

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Iruka iruka))
            {
                irukaNearby = true;
                if (!isBuffActive)
                {
                    isBuffActive = true;
                    attackDamage = Mathf.RoundToInt(originalAttackDamage * buffMultiplier);
                }
                break;
            }
        }

        if (!irukaNearby && isBuffActive)
        {
            isBuffActive = false;
            attackDamage = originalAttackDamage;
        }
    }

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.1f);
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.2f);
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 0.9f);
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
