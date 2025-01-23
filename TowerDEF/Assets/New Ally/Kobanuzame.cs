using UnityEngine;
using System.Collections;

public class Kobanuzame : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Kobanuzameの体力
    public int health = 20;
    public int maxHealth = 20;
    private bool isBuffApplied = false;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 5;
    public int attackDamage = 5;
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率

    // Kobanuzameの攻撃力と攻撃関連の設定
    public float detectionRadius = 10f;     // 敵を検知する範囲
    public float attackCooldown = 1.0f;     // 攻撃のクールダウン時間

    private Transform target;               // 攻撃対象の敵
    private float lastAttackTime;           // 最後に攻撃した時間

    // **体当たり処理関連**
    [Header("体当たり設定")]
    public float dashDistance = 3.0f;       // 体当たりの前進距離
    public float dashDuration = 0.2f;       // 前進にかかる時間
    public float returnDuration = 0.2f;     // 後退にかかる時間
    private bool isDashing = false;         // 現在体当たり中かどうか

    // **攻撃エフェクト＆サウンド設定**
    [Header("攻撃エフェクト設定")]
    public GameObject attackEffectPrefab;   // 攻撃エフェクトのプレハブ
    public Transform effectSpawnPoint;      // エフェクトを生成する位置
    public AudioClip attackSound;           // 攻撃時の効果音
    private AudioSource audioSource;        // 効果音を再生するためのAudioSource

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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;

        ApplyBuffFromManuta();
        ApplyBuffFromIruka();
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

    public void PerformDashAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
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

        // **攻撃エフェクトとサウンドの再生**
        PlayAttackEffect();

        // 攻撃判定
        if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRadius)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
            Debug.Log($"{target.name} に {attackDamage} のダメージを与えました。");
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
        lastAttackTime = Time.time;
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
            PerformDashAttack(); // **体当たり攻撃を実行**
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

    public void TakeDamage(int damageAmount)
    {
        if (!isBuffApplied)
        {
            health -= damageAmount;
        }
        else
        {
            int reducedDamage = Mathf.RoundToInt(damageAmount * 0.25f); // ダメージを75%軽減
            health -= reducedDamage;
            Debug.Log($"Manutaの共生効果によりダメージが軽減されました。受けたダメージ: {reducedDamage}");
        }

        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void ApplyBuffFromManuta()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool manutaNearby = false;
        Debug.Log("近くにいるManutaを検知しています...");
        foreach (Collider collider in colliders)
        {
            Manuta manuta = collider.GetComponent<Manuta>();
            if (manuta != null && manuta.gameObject != gameObject)
            {
                manutaNearby = true;
                Debug.Log($"Manutaを検知しました: {manuta.name}");
                break;
            }
        }

        if (manutaNearby && !isBuffApplied)
        {
            attackCooldown *= 0.5f; // 攻撃頻度が上がる（クールダウン時間を半分にする）
            isBuffApplied = true;  // バフが適用されたことを記録
        }
        else if (!manutaNearby && isBuffApplied)
        {
            attackCooldown *= 2.0f; // 攻撃頻度を元に戻す
            isBuffApplied = false; // バフを解除
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
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}
