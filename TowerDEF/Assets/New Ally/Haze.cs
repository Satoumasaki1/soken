using UnityEngine;
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

    // **新しく追加するエフェクト関連のフィールド**
    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab; // エフェクトのプレハブ (damage ef)
    public Transform effectSpawnPoint;   // エフェクトを生成する位置

    public void OnApplicationQuit() //追加
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius) //追加
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        detectionRadius += additionaRadius;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage + ", Radius: " + detectionRadius);
    }

    public void SaveState() //追加
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState() //追加
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
        LoadState(); //追加

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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyBuffFromUdeppo();
        ApplyIrukaBuff();
        AttackOn();
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

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
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
        // 攻撃アニメーションを再生
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // エフェクトの再生（新規追加）
        PlayAttackEffect();

        // 範囲攻撃または単体攻撃
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
                    Debug.Log($"{collider.name} に {attackDamage} のダメージを与えました。");
                }
            }
        }
        else if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
            Debug.Log($"{target.name} に {attackDamage} のダメージを与えました。");
        }
    }

    // **新規追加: 攻撃エフェクトを再生するメソッド**
    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            // エフェクトを生成
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);

            // 一定時間後に削除（2秒後）
            Destroy(effect, 2.0f);

            Debug.Log("攻撃エフェクトを生成しました！");
        }
        else
        {
            Debug.LogWarning("attackEffectPrefab または effectSpawnPoint が設定されていません！");
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
        Debug.Log("近くにいるテッポウエビを検知しています...");
        foreach (Collider collider in colliders)
        {
            Udeppo udeppo = collider.GetComponent<Udeppo>();
            if (udeppo != null && udeppo.gameObject != gameObject)
            {
                isUdeppoNearby = true;
                Debug.Log($"テッポウエビを検知しました: {udeppo.name}");
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

    // イルカのバフが有効か確認して適用する処理
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
                    Debug.Log($"{name} の攻撃力が強化されました: {attackDamage}");
                }
                break;
            }
        }

        if (!irukaNearby && isBuffActive)
        {
            isBuffActive = false;
            attackDamage = originalAttackDamage;
            Debug.Log($"{name} の攻撃力強化が終了しました。元の攻撃力に戻りました: {attackDamage}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // シーズンの効果を適用するメソッド
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.1f);
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.2f);
                Debug.Log("秋のバフが適用されました: 体力と攻撃力が上昇");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 0.9f);
                Debug.Log("冬のデバフが適用されました: 体力と攻撃力が減少");
                break;
        }

        seasonEffectApplied = true;
    }

    // シーズンの効果をリセットするメソッド
    public void ResetSeasonEffect()
    {
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}
