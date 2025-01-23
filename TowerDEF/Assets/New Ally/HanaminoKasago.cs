using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HanaminoKasago : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // HanaminoKasagoの体力と最大体力
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率
    public int health = 20;
    public int maxHealth = 20;

    // 麻痺毒関連の設定
    public float detectionRadius = 15f; // 麻痺毒の範囲
    public float poisonDamage = 1.0f;   // 継続ダメージ
    public float effectInterval = 1.0f; // 継続ダメージの間隔
    private float lastEffectTime;

    // 攻撃関連
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 10;
    public int attackDamage = 10; // 攻撃力

    // 体当たり関連の設定
    [Header("体当たり設定")]
    public float dashDistance = 2.0f; // 前進する距離
    public float dashDuration = 0.2f; // 前進にかかる時間
    public float returnDuration = 0.2f; // 後退にかかる時間
    private bool isDashing = false; // 体当たり中かどうかを判定するフラグ

    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    // **体力バー関連**
    [Header("体力バー関連")]
    public GameObject healthBarPrefab;      // 体力バーのプレハブ
    private GameObject healthBarInstance;   // 実際に生成された体力バー
    private Slider healthSlider;            // 体力バーのスライダーコンポーネント

    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab;   // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;      // エフェクトを生成する位置

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

        // **体力バーの初期化**
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform); // 体力バーを生成
            healthBarInstance.transform.localPosition = new Vector3(0, 0.5f, 0); // キャラクターの上（高さ 0.5f）に配置

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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;

        ApplyParalyticPoisonEffect();
        ApplyIrukaBuff();

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

    public void ApplyParalyticPoisonEffect()
    {
        if (Time.time > lastEffectTime + effectInterval)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    IDamageable enemy = collider.GetComponent<IDamageable>();
                    enemy?.TakeDamage((int)poisonDamage);
                }
            }
            lastEffectTime = Time.time;
        }
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

    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            // **エフェクトを生成**
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);

            // **一定時間後にエフェクトを削除**
            Destroy(effect, 2.0f);

            Debug.Log("攻撃エフェクトを生成しました！");
        }
        else
        {
            Debug.LogWarning("攻撃エフェクトのプレハブまたは生成位置が設定されていません！");
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

    private void Die()
    {
        Destroy(gameObject);
    }

    private IEnumerator PerformDash()
    {
        if (isDashing) yield break;

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
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
}
