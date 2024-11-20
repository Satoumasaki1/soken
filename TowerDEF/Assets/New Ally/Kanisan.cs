using UnityEngine;

public class Kanisan : MonoBehaviour, IDamageable
{
    // Kanisanの体力
    public int health = 10;
    public int maxHealth = 10;
    private bool maxHealthBuffApplied = false;

    // Kanisanの攻撃力と攻撃関連の設定
    public int attackDamage = 3;
    public float detectionRadius = 10f;     // 敵を検知する範囲
    public float attackCooldown = 1.5f;     // 攻撃のクールダウン時間

    private Transform target;               // 攻撃対象の敵
    private float lastAttackTime;           // 最後に攻撃した時間

    // GameManagerの参照をインスペクターから設定できるようにする
    [SerializeField]
    private GameManager gm;

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
    }

    private void OnMouseDown()
    {
        // 魚がクリックされたとき、回復を試みる
        TryHeal();
    }

    // 魚がクリックされたときに呼ばれる回復メソッド
    public void TryHeal()
    {
        // GameManagerで選択されている餃がある場合
        if (gm.SelectedFeedType.HasValue)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            // 餃がオキアミ、ベントス、またはプランクトンの場合のみ回復可能
            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                // 餃の在庫がある場合のみ回復処理を行う
                if (gm.inventory[selectedFeed] > 0)
                {
                    // 体力が最大値に達していない場合、回復
                    if (health < maxHealth)
                    {
                        health += 2; // 回復量を設定
                        health = Mathf.Min(health, maxHealth); // 最大体力を超えないように制限
                        gm.inventory[selectedFeed]--; // 在庫を減らす
                        gm.UpdateResourceUI(); // リソースUIを更新
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

    // 敵を検知して攻撃するメソッド
    public void AttackOn()
    {
        // ターゲットが設定されていない場合、敵を検知
        if (target == null)
        {
            DetectEnemy();
        }
        else
        {
            // ターゲットが範囲内におり、攻撃クールダウンが終了している場合に攻撃
            if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
            {
                AttackTarget();
                lastAttackTime = Time.time; // 攻撃時間を更新
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 検知範囲に入ったオブジェクトがEnemyタグを持つ場合、ターゲットに設定
        if (other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
        }
    }

    // 範囲内の敵を検知してターゲットに設定するメソッド
    void DetectEnemy()
    {
        // 検知範囲内にあるすべてのColliderを取得
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Debug.Log("敵を検知しています...");
        foreach (Collider collider in colliders)
        {
            // タグが"Enemy"と一致するオブジェクトをターゲットとして設定
            if (collider.CompareTag("Enemy"))
            {
                target = collider.transform;
                Debug.Log($"敵を検知しました: {target.name}");
                break;
            }
        }
    }

    // ターゲットを攻撃するメソッド
    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // ターゲットがIDamageableを持つ場合、ダメージを与える
            damageable.TakeDamage(attackDamage);
        }
    }

    // ダメージを受けたときの処理
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    // Kanisanが倒れたときの処理
    private void Die()
    {
        Destroy(gameObject); // オブジェクトを破壊
    }

    // 近くのカニさんの数に応じて攻撃力と体力を強化する
    private void BuffKanisan()
    {
        if (maxHealthBuffApplied) return;

        // 近くにいるカニさんを検索
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

        // 攻撃力と体力を設定する
        attackDamage = 3 + nearbyKanisanCount * 5; // 基本攻撃力に近くのカニさん数に応じて5ずつ加算（最大25）
        maxHealth = 10 + nearbyKanisanCount * 5; // 基本体力に近くのカニさん数に応じて5ずつ加算（最大25）
        health = Mathf.Min(health + nearbyKanisanCount * 5, maxHealth); // 通常の体力にも近くのカニさん数に応じて5ずつ加算

        maxHealthBuffApplied = true; // バフが一度だけ適用されるようにする
    }

    void OnDrawGizmosSelected()
    {
        // シーンビューで検知範囲を可視化
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
