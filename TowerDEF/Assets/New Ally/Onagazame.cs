using UnityEngine;
using UnityEngine.UI;

public class Onagazame : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Onagazameの体力
    public int health = 25;
    public int maxHealth = 25;

    // Onagazameの攻撃力と攻撃関連の設定
    public int attackDamage = 10;
    public float detectionRadius = 15f;     // 敵を検知する範囲
    public float attackCooldown = 1.5f;    // 攻撃のクールダウン時間
    public float stunDuration = 12.0f;     // スタン効果の持続時間

    // しっぽ攻撃の動き関連
    public float tailSwingDistance = 2.0f; // しっぽ攻撃で前進する距離
    public float tailSwingDuration = 0.3f; // 前進にかかる時間
    public float tailReturnDuration = 0.3f; // 後退にかかる時間
    private bool isAttacking = false;      // 現在攻撃中かどうかを判定するフラグ

    private Transform target;               // 攻撃対象の敵
    private float lastAttackTime;           // 最後に攻撃した時間

    // **攻撃エフェクト＆サウンド設定**
    [Header("攻撃エフェクト設定")]
    public GameObject tailAttackEffectPrefab; // しっぽ攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;        // エフェクトを生成する位置
    public AudioClip tailAttackSound;         // しっぽ攻撃時の効果音
    private AudioSource audioSource;          // 効果音を再生するためのAudioSource

    // **ヘルスバー関連**
    [Header("ヘルスバー設定")]
    public GameObject healthBarPrefab;        // ヘルスバーのプレハブ
    private GameObject healthBarInstance;     // 実際に生成されたヘルスバー
    private Slider healthSlider;              // ヘルスバーのスライダーコンポーネント
    private Transform cameraTransform;        // メインカメラのTransform

    // GameManagerの参照をインスペクターから設定できるようにする
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

        AttackOn();        // 攻撃処理
        UpdateHealthBar(); // ヘルスバーの更新
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

    public void PlayTailAttackEffect()
    {
        // **エフェクトの生成**
        if (tailAttackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(tailAttackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
            Debug.Log("しっぽ攻撃エフェクトを生成しました！");
        }

        // **効果音の再生**
        if (tailAttackSound != null && audioSource != null)
        {
            audioSource.clip = tailAttackSound;
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
            if (!isAttacking) StartCoroutine(PerformTailAttack());
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

    private System.Collections.IEnumerator PerformTailAttack()
    {
        isAttacking = true;

        // **前進フェーズ**
        Vector3 startPosition = transform.position;
        Vector3 attackPosition = transform.position + transform.forward * tailSwingDistance;
        float elapsedTime = 0f;

        while (elapsedTime < tailSwingDuration)
        {
            transform.position = Vector3.Lerp(startPosition, attackPosition, elapsedTime / tailSwingDuration);
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

                IStunnable stunnable = collider.GetComponent<IStunnable>();
                stunnable?.Stun(stunDuration);
            }
        }

        // **エフェクト＆サウンドの再生**
        PlayTailAttackEffect();

        // **後退フェーズ**
        elapsedTime = 0f;
        while (elapsedTime < tailReturnDuration)
        {
            transform.position = Vector3.Lerp(attackPosition, startPosition, elapsedTime / tailReturnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
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

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                Debug.Log("春のバフが適用されました: 体力と攻撃力が少し上昇");
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                Debug.Log("夏のデバフが適用されました: 体力が減少");
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                Debug.Log("秋のバフが適用されました: 体力と攻撃力が上昇");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                Debug.Log("冬のデバフが適用されました: 体力と攻撃力が減少");
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        maxHealth = 25;
        health = Mathf.Min(health, maxHealth);
        attackDamage = 10;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}