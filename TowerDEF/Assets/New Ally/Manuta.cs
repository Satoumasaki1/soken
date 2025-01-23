using UnityEngine;
using UnityEngine.UI;

public class Manuta : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Manutaの体力
    public int health = 20;
    public int maxHealth = 20;
    private bool isBuffApplied = false;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 5;
    public int attackDamage = 5;
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率

    // 攻撃関連の設定
    public float detectionRadius = 10f;     // 敵を検知する範囲
    public float attackRange = 5f;          // 攻撃範囲
    public float attackCooldown = 1.0f;     // 攻撃のクールダウン時間
    private Transform target;               // 攻撃対象の敵
    private float lastAttackTime;           // 最後に攻撃した時間

    // **攻撃エフェクト＆サウンド設定**
    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab;   // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;      // エフェクトを生成する位置
    public AudioClip attackSound;           // 攻撃時の効果音
    private AudioSource audioSource;        // 効果音を再生するためのAudioSource

    // **ヘルスバー関連**
    [Header("ヘルスバー設定")]
    public GameObject healthBarPrefab;      // ヘルスバーのプレハブ
    private GameObject healthBarInstance;   // 実際に生成されたヘルスバー
    private Slider healthSlider;            // ヘルスバーのスライダーコンポーネント
    private Transform cameraTransform;      // メインカメラのTransform

    // たたきつけ処理関連
    private enum SmashState { Idle, Rising, Falling }
    private SmashState smashState = SmashState.Idle; // 現在のたたきつけ状態
    private Vector3 originalPosition;
    private Vector3 raisedPosition;
    private float smashProgress = 0f; // 上昇や下降の進行度
    private const float riseDuration = 0.2f; // 上昇にかかる時間
    private const float fallDuration = 0.1f; // 下降にかかる時間

    // GameManagerの参照
    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

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

        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }

        // **AudioSourceの初期化**
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // **カメラの参照を取得**
        cameraTransform = Camera.main.transform;

        // **ヘルスバーの初期化**
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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;

        ApplyBuffFromKobanuzame();
        ApplyBuffFromIruka();
        HandleSmashAttack(); // たたきつけ処理
        AttackOn();
        UpdateHealthBar();   // ヘルスバーの更新
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        // ヘルスバーのスライダー値を更新
        healthSlider.value = health;

        // ヘルスバーをカメラに向けて回転
        healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - cameraTransform.position);

        // ヘルスバーの表示/非表示
        healthBarInstance.SetActive(health < maxHealth);
    }

    private void OnDestroy()
    {
        // ヘルスバーのインスタンスを削除
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

    public void PlayAttackEffect()
    {
        // **エフェクトの生成**
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
            Debug.Log("攻撃エフェクトを生成しました！");
        }

        // **効果音の再生**
        if (attackSound != null && audioSource != null)
        {
            audioSource.clip = attackSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("攻撃エフェクトまたはサウンドが設定されていません！");
        }
    }

    public void PerformSmashAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown || smashState != SmashState.Idle) return;

        // たたきつけ処理を開始
        smashState = SmashState.Rising;
        smashProgress = 0f;
        originalPosition = transform.position;
        raisedPosition = originalPosition + Vector3.up * 2f;
    }

    private void HandleSmashAttack()
    {
        if (smashState == SmashState.Idle) return;

        smashProgress += Time.deltaTime;

        if (smashState == SmashState.Rising)
        {
            transform.position = Vector3.Lerp(originalPosition, raisedPosition, smashProgress / riseDuration);

            if (smashProgress >= riseDuration)
            {
                smashState = SmashState.Falling;
                smashProgress = 0f;

                // エフェクトと効果音の再生
                PlayAttackEffect();
            }
        }
        else if (smashState == SmashState.Falling)
        {
            transform.position = Vector3.Lerp(raisedPosition, originalPosition, smashProgress / fallDuration);

            if (smashProgress >= fallDuration)
            {
                smashState = SmashState.Idle;

                // 攻撃判定
                if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
                {
                    IDamageable damageable = target.GetComponent<IDamageable>();
                    damageable?.TakeDamage(attackDamage);
                    Debug.Log($"{target.name} に {attackDamage} のダメージを与えました。");
                }

                lastAttackTime = Time.time;
            }
        }
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            PerformSmashAttack(); // たたきつけ攻撃を実行
        }
    }

    void DetectEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                target = collider.transform;
                break;
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} が倒れました！");
        Destroy(gameObject);
    }

    private void ApplyBuffFromKobanuzame()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool kobanuzameNearby = false;
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Kobanuzame kobanuzame) && kobanuzame.gameObject != gameObject)
            {
                kobanuzameNearby = true;
                break;
            }
        }

        if (kobanuzameNearby && !isBuffApplied)
        {
            maxHealth *= 3;
            health = Mathf.Min(health * 3, maxHealth);
            attackDamage += 5;
            isBuffApplied = true;
        }
        else if (!kobanuzameNearby && isBuffApplied)
        {
            maxHealth /= 3;
            health = Mathf.Min(health, maxHealth);
            attackDamage -= 5;
            isBuffApplied = false;
        }
    }

    private void ApplyBuffFromIruka()
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
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
}
