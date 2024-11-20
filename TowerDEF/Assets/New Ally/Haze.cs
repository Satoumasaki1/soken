using UnityEngine;

public class Haze : MonoBehaviour, IDamageable
{
    // Hazeの体力と最大体力
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;

    // 攻撃関連の設定
    public int attackDamage = 5;
    public float detectionRadius = 10f;
    public float attackCooldown = 1.0f;

    private Transform target;
    private float lastAttackTime;

    [SerializeField]
    private GameManager gm;

    void Start()
    {
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManagerの参照が見つかりません。GameManagerが正しく設定されているか確認してください。");
        }
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
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
        if (maxHealthBuffApplied) return false;

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
                maxHealth += 20;
                health = Mathf.Min(health + 20, maxHealth);
                maxHealthBuffApplied = true;
                ApplyBuffToAllies(colliders);
                break;
            }
        }
        return isUdeppoNearby;
    }

    private void ApplyBuffToAllies(Collider[] colliders)
    {
        foreach (Collider nearbyCollider in colliders)
        {
            if (nearbyCollider.CompareTag("Ally"))
            {
                Haze nearbyHaze = nearbyCollider.GetComponent<Haze>();
                if (nearbyHaze != null && nearbyHaze.gameObject != gameObject && !nearbyHaze.maxHealthBuffApplied)
                {
                    nearbyHaze.maxHealth += 20;
                    nearbyHaze.health = Mathf.Min(nearbyHaze.health + 20, nearbyHaze.maxHealth);
                    nearbyHaze.maxHealthBuffApplied = true;
                    Debug.Log($"{nearbyHaze.name} の最大体力を20増加しました。");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
