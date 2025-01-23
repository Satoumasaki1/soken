using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Kanisan : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Kanisanの基本プロパティ
    public int health = 10;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 3;
    public int attackDamage = 3;
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率

    // 攻撃関連の設定
    public float detectionRadius = 10f;     // 敵を検知する範囲
    public float attackRange = 5f;          // 攻撃範囲
    public float attackCooldown = 1.5f;     // 攻撃のクールダウン時間
    private Transform target;               // 攻撃対象の敵
    private float lastAttackTime;           // 最後に攻撃した時間

    // GameManagerの参照
    [SerializeField]
    private GameManager gm;

    // 体力バー関連
    public GameObject healthBarPrefab;      // 体力バーのプレハブ
    private GameObject healthBarInstance;   // 実際に生成された体力バー
    private Slider healthSlider;            // 体力バーのスライダーコンポーネント

    private bool seasonEffectApplied = false;

    // **攻撃エフェクト＆効果音関連**
    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab;   // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;      // エフェクトを生成する位置
    public AudioClip attackSound;           // 効果音のAudioClip
    private AudioSource audioSource;        // 効果音を再生するAudioSource

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

        // 体力バーを生成し、キャラクターの子オブジェクトとして配置
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2, 0); // 頭上に配置
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = 10;
                healthSlider.value = health;
            }
        }
        else
        {
            Debug.LogError("HealthBarPrefabが設定されていません！");
        }

        // **AudioSourceの初期化**
        audioSource = gameObject.AddComponent<AudioSource>(); // AudioSourceを追加
        audioSource.playOnAwake = false; // 自動再生を無効化
    }

    void Update()
    {
        // 一時停止中は処理をスキップ
        if (gm != null && gm.isPaused)
            return;

        // 攻撃処理
        AttackOn();

        // 周囲のカニさんによるバフ
        BuffKanisan();

        // イルカからのバフ適用
        ApplyIrukaBuff();

        // 体力バーを更新
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        float healthPercentage = (float)health / 10; // 健康状態を 0～1 に正規化
        healthSlider.value = healthPercentage;

        // 体力がマックスの場合は非表示
        healthBarInstance.SetActive(health < 10);

        // 体力バーの回転をカメラに合わせる
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
        // 餌で回復する処理
        TryHeal();
    }

    public void TryHeal()
    {
        if (gm.SelectedFeedType.HasValue)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                if (gm.inventory[selectedFeed] > 0)
                {
                    if (health < 10)
                    {
                        health += 2; // 回復量
                        health = Mathf.Min(health, 10);
                        gm.inventory[selectedFeed]--;
                        gm.UpdateResourceUI();
                        // 体力バーを更新
                        UpdateHealthBar();
                        Debug.Log($"{selectedFeed} で体力を回復しました。残り在庫: {gm.inventory[selectedFeed]}");
                    }
                    else
                    {
                        Debug.Log("体力は既に最大です。");
                    }
                }
                else
                {
                    Debug.Log($"{selectedFeed} の在庫が不足しています。");
                }
            }
            else
            {
                Debug.Log("この餌では回復できません。");
            }
        }
        else
        {
            Debug.Log("餌が選択されていません。");
        }
    }

    public void PlayAttackEffect()
    {
        // **エフェクトの生成**
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f); // エフェクトを一定時間後に削除

            Debug.Log("攻撃エフェクトを生成しました！");
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
        }
        else
        {
            Debug.LogWarning("攻撃効果音が設定されていません！");
        }
    }

    public void PerformSmashAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        StartCoroutine(SmashAttack());
    }

    private IEnumerator SmashAttack()
    {
        // 上昇モーション
        Vector3 originalPosition = transform.position;
        Vector3 raisedPosition = originalPosition + Vector3.up * 2f;
        float elapsedTime = 0f;

        while (elapsedTime < 0.2f)
        {
            transform.position = Vector3.Lerp(originalPosition, raisedPosition, elapsedTime / 0.2f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // **攻撃エフェクトと効果音を再生**
        PlayAttackEffect();

        // たたきつけモーション
        elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            transform.position = Vector3.Lerp(raisedPosition, originalPosition, elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 攻撃を適用
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
        }

        lastAttackTime = Time.time;
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            PerformSmashAttack(); // **たたきつけ攻撃を実行**
        }
    }

    private void DetectEnemy()
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
        UpdateHealthBar();

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} が倒れました！");
        Destroy(gameObject);
    }

    private void BuffKanisan()
    {
        if (maxHealthBuffApplied) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        int nearbyKanisanCount = 0;
        foreach (Collider collider in colliders)
        {
            Kanisan kanisan = collider.GetComponent<Kanisan>();
            if (kanisan != null && kanisan != this)
            {
                nearbyKanisanCount++;
                if (nearbyKanisanCount >= 5) break;
            }
        }

        attackDamage += nearbyKanisanCount * 5;
        health = Mathf.Min(health + nearbyKanisanCount * 5, 10);

        maxHealthBuffApplied = true;
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
                health = Mathf.Min(health + 10, 10);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                break;
            case GameManager.Season.Summer:
                health = Mathf.Min(health, 10);
                break;
            case GameManager.Season.Autumn:
                health = Mathf.Min(health + 15, 10);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                break;
            case GameManager.Season.Winter:
                health = Mathf.Min(health, 10);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        health = Mathf.Min(health, 10);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
    }
}
