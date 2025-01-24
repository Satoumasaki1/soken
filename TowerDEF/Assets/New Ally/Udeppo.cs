using UnityEngine;
using UnityEngine.UI;

public class Udeppo : MonoBehaviour, IDamageable, IUpgradable
{
    // Udeppoの体力
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 10; // 元の攻撃力

    // 攻撃関連の設定
    public int attackDamage = 10;
    public float detectionRadius = 20f;     // 敵を検知する範囲（射程が長い）
    public float attackCooldown = 3.0f;     // 攻撃のクールダウン時間（攻撃頻度は遅い）

    // **たたきつけ攻撃の動き関連**
    public float slamDistance = 2.0f;      // たたきつけ攻撃で前進する距離
    public float slamDuration = 0.3f;      // 前進にかかる時間
    public float returnDuration = 0.3f;    // 後退にかかる時間
    private bool isAttacking = false;      // 攻撃中かどうかを判定するフラグ

    // **攻撃エフェクト＆サウンド設定**
    [Header("攻撃エフェクト設定")]
    public GameObject slamEffectPrefab;    // たたきつけ攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;     // エフェクトを生成する位置
    public AudioClip slamSound;            // たたきつけ攻撃時の効果音
    private AudioSource audioSource;       // 効果音を再生するためのAudioSource

    // **ヘルスバー関連**
    [Header("ヘルスバー設定")]
    public GameObject healthBarPrefab;     // ヘルスバーのプレハブ
    private GameObject healthBarInstance;  // 実際に生成されたヘルスバー
    private Slider healthSlider;           // ヘルスバーのスライダーコンポーネント
    private Transform cameraTransform;     // メインカメラのTransform

    // GameManagerの参照
    [SerializeField]
    private GameManager gm;

    private float lastAttackTime;          // 最後に攻撃した時間
    private Transform target;              // 攻撃対象の敵

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

        // GameManagerの参照を取得
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

        ApplyBuffFromHaze();
        ApplyBuffFromIruka();
        AttackOn();
        UpdateHealthBar();
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

    public void PlaySlamEffect()
    {
        if (slamEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(slamEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
            Debug.Log("たたきつけ攻撃エフェクトを生成しました！");
        }

        if (slamSound != null && audioSource != null)
        {
            audioSource.clip = slamSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("攻撃エフェクトまたはサウンドが設定されていません！");
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
            if (!isAttacking) StartCoroutine(PerformSlamAttack());
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

    private System.Collections.IEnumerator PerformSlamAttack()
    {
        isAttacking = true;

        // **前進フェーズ**
        Vector3 startPosition = transform.position;
        Vector3 slamPosition = transform.position + transform.forward * slamDistance;
        float elapsedTime = 0f;

        while (elapsedTime < slamDuration)
        {
            transform.position = Vector3.Lerp(startPosition, slamPosition, elapsedTime / slamDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // **攻撃判定**
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                damageable?.TakeDamage(attackDamage);
            }
        }

        // **エフェクト＆サウンドの再生**
        PlaySlamEffect();

        // **後退フェーズ**
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(slamPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void ApplyBuffFromHaze()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool isHazeNearby = false;
        foreach (Collider collider in colliders)
        {
            Haze haze = collider.GetComponent<Haze>();
            if (haze != null && haze.gameObject != gameObject)
            {
                isHazeNearby = true;
                break;
            }
        }

        if (isHazeNearby && !maxHealthBuffApplied)
        {
            maxHealth += 20;
            health = Mathf.Min(health + 20, maxHealth);
            detectionRadius *= 2;
            attackDamage *= 2;
            maxHealthBuffApplied = true;
        }
        else if (!isHazeNearby && maxHealthBuffApplied)
        {
            maxHealth -= 20;
            health = Mathf.Min(health, maxHealth);
            detectionRadius /= 2;
            attackDamage /= 2;
            maxHealthBuffApplied = false;
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
                    attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
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
}