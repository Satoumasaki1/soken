using UnityEngine;
using System.Collections;

public class Kanisan : MonoBehaviour, IDamageable, ISeasonEffect
{
    // Kanisanの体力
    public int health = 10;
    public int maxHealth = 10;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // バフが有効かどうかのフラグ
    private int originalAttackDamage = 3;
    public int attackDamage = 3;
    public float buffMultiplier = 1.5f; // 攻撃バフの倍率

    // Kanisanの攻撃力と攻撃関連の設定
    public float detectionRadius = 10f;     // 敵を検知する範囲
    public float attackRange = 5f;          // 攻撃範囲
    public float attackCooldown = 1.5f;     // 攻撃のクールダウン時間

    private Transform target;               // 攻撃対象の敵
    private float lastAttackTime;           // 最後に攻撃した時間

    // GameManagerの参照をインスペクターから設定できるようにする
    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    void Start()
    {
        // GameManagerの参照がインスペクターで設定されていない場合、自動的に取得
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // GameManagerの参照が取得できなかった場合、エラーメッセージを表示
        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }
    }

    void Update()
    {
        // 一時停止中はAttackOn()を実行せずに戻る
        if (gm != null && gm.isPaused)
        {
            return;
        }

        // 一時停止されていない場合、攻撃処理を実行
        AttackOn();

        // 近くのカニさんの数に応じて攻撃力と体力を強化
        BuffKanisan();

        // イルカからのバフを適用
        ApplyIrukaBuff();
    }

    private void OnMouseDown()
    {
        // 魚がクリックされたとき、回復を試みる
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
                        health += 2;
                        health = Mathf.Min(health, maxHealth);
                        gm.inventory[selectedFeed]--;
                        gm.UpdateResourceUI();
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
                Debug.Log("この餃では回復できません。");
            }
        }
        else
        {
            Debug.Log("餃が選択されていません。");
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
                target = null; // ターゲットが範囲外に出た場合リセット
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
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
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
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
        Debug.Log("味方のカニさんを検知しています...");
        foreach (Collider collider in colliders)
        {
            Kanisan kanisan = collider.GetComponent<Kanisan>();
            if (kanisan != null && kanisan.gameObject != gameObject)
            {
                nearbyKanisanCount++;
                Debug.Log($"味方のカニさんを検知しました: {kanisan.name}");
                if (nearbyKanisanCount >= 5)
                {
                    nearbyKanisanCount = 5;
                    break;
                }
            }
        }

        attackDamage = 3 + nearbyKanisanCount * 5;
        maxHealth = 10 + nearbyKanisanCount * 5;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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
        maxHealth = 10;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("シーズン効果がリセットされました。");
    }
}
