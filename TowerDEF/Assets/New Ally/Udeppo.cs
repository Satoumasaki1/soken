using UnityEngine;

public class Udeppo : MonoBehaviour, IDamageable
{
    // Udeppoの体力
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;

    // TeppoEbiの攻撃力と攻撃関連の設定
    public int attackDamage = 10;
    public float detectionRadius = 20f;     // 敵を検知する範囲（射程が長い）
    public float attackCooldown = 3.0f;     // 攻撃のクールダウン時間（攻撃頻度は遅い）

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
        ApplyBuffFromHaze();
        AttackOn();
    }

    private void OnMouseDown()
    {
        // テッポウエビがクリックされたとき、回復を試みる
        TryHeal();
    }

    // テッポウエビがクリックされたときに呼ばれる回復メソッド
    public void TryHeal()
    {
        // GameManagerで選択されている餌がある場合
        if (gm.SelectedFeedType.HasValue)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            // 餌がオキアミ、ベントス、またはプランクトンの場合のみ回復可能
            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                // 餌の在庫がある場合のみ回復処理を行う
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
                Debug.Log("この餌では回復できません。");
            }
        }
        else
        {
            Debug.Log("餌が選択されていません。");
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
            Debug.Log($"{target.name} に {attackDamage} のダメージを与えました。");
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

    // TeppoEbiが倒れたときの処理
    private void Die()
    {
        Destroy(gameObject); // オブジェクトを破壊
    }

    // 近くにハゼがいる場合、体力と最大体力を増加させる追加効果
    private void ApplyBuffFromHaze()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool isHazeNearby = false;
        Debug.Log("近くにいるハゼを検知しています...");
        foreach (Collider collider in colliders)
        {
            Haze haze = collider.GetComponent<Haze>();
            if (haze != null && haze.gameObject != gameObject)
            {
                isHazeNearby = true;
                Debug.Log($"ハゼを検知しました: {haze.name}");
                break;
            }
        }

        // 体力と最大体力を設定する
        if (isHazeNearby && !maxHealthBuffApplied)
        {
            maxHealth += 20; // ハゼがいる場合、最大体力を20増加
            health = Mathf.Min(health + 20, maxHealth); // 現在の体力も20増加し、最大体力を超えないように制限
            detectionRadius *= 2; // ハゼがいる場合、検知範囲を2倍にする
            attackDamage *= 2; // ハゼがいる場合、射程を2倍にする
            maxHealthBuffApplied = true; // バフが適用されたことを記録
        }
        else if (!isHazeNearby && maxHealthBuffApplied)
        {
            maxHealth -= 20; // ハゼがいなくなった場合、最大体力を元に戻す
            health = Mathf.Min(health, maxHealth); // 現在の体力を最大体力に合わせる
            detectionRadius /= 2; // 検知範囲を元に戻す
            attackDamage /= 2; // 射程を元に戻す
            maxHealthBuffApplied = false; // バフを解除
        }
    }

    void OnDrawGizmosSelected()
    {
        // シーンビューで検知範囲を可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
