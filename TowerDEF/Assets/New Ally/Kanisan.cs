using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Kanisan : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Kanisanの基本プロパティ
    public int health = 10;
    public int maxHealth = 10; //削除
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

    public void OnApplicationQuit()　//追加
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage)//追加
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage);
    }

    public void SaveState()//追加
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()//追加
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

        LoadState();//追加

        // GameManagerの参照を取得
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }

        // 現在の体力を初期化
        health = maxHealth;

        // 体力バーを生成し、キャラクターの子オブジェクトとして配置
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2, 0); // 頭上に配置
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = 1;
                healthSlider.value = health;
            }
        }
        else
        {
            Debug.LogError("HealthBarPrefabが設定されていません！");
        }
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

        float healthPercentage = (float)health / maxHealth;
        healthSlider.value = healthPercentage;

        // 体力がマックスの場合は非表示
        healthBarInstance.SetActive(health < maxHealth);

        // バーの色を更新
        Image fill = healthSlider.fillRect.GetComponent<Image>();
        if (fill != null)
        {
            fill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
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
                    if (health < maxHealth)
                    {
                        health += 2; // 回復量
                        health = Mathf.Min(health, maxHealth);
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

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackTarget();
                lastAttackTime = Time.time;
            }
            else if (distanceToTarget > detectionRadius)
            {
                target = null;
            }
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

    private void AttackTarget()
    {
        if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
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
        maxHealth += nearbyKanisanCount * 5;
        health = Mathf.Min(health + nearbyKanisanCount * 5, maxHealth);

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
        maxHealth = 10;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
    }
}
