using UnityEngine;

public class IkaiKa : MonoBehaviour
{
    public int attackPower = 5;
    public float maxHealth = 10f;
    private float currentHealth;
    public float moveSpeed = 2f;
    public float attackRange = 6f;
    private float targetSearchInterval = 0.5f;
    private float nextTargetSearchTime = 0f;
    private Collider[] nearbyTargets = new Collider[10];
    private Transform currentTarget;
    public Transform baseTarget;  // 味方の拠点を指定するターゲット
    public Transform[] waypoints; // 出現位置から拠点までのルートを示すウェイポイント
    private int currentWaypointIndex = 0;

    // 初期化
    void Start()
    {
        currentHealth = maxHealth;
        if (waypoints.Length > 0)
        {
            currentTarget = waypoints[currentWaypointIndex];  // 初期のターゲットを最初のウェイポイントに設定
        }
        else
        {
            currentTarget = baseTarget;  // ウェイポイントがない場合は拠点をターゲットに設定
        }
        Debug.Log(gameObject.name + " initialized with max health: " + maxHealth);
    }

    void Update()
    {
        Debug.Log(gameObject.name + " Update called. Current target: " + (currentTarget != null ? currentTarget.name : "None"));
        if (currentTarget != null)
        {
            MoveTowardsTarget();
        }
        else if (Time.time >= nextTargetSearchTime)
        {
            Debug.Log(gameObject.name + " Searching for new target.");
            FindNewTarget();
            nextTargetSearchTime = Time.time + targetSearchInterval;
        }
    }

    // ダメージを受けた時の処理
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took damage: " + damageAmount + ", current health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ユニットが死亡した際の処理
    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated.");
        Destroy(gameObject);
    }

    // 新しい攻撃対象を探す（優先順位: Ally）
    private void FindNewTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, 15f, nearbyTargets);
        Debug.Log(gameObject.name + " found " + hitCount + " potential targets.");
        Transform bestTarget = baseTarget;  // デフォルトで拠点をターゲットにする

        for (int i = 0; i < hitCount; i++)
        {
            Collider target = nearbyTargets[i];
            if (target.CompareTag("Ally"))
            {
                Debug.Log(gameObject.name + " evaluating target: " + target.name);
                bestTarget = target.transform;
                break;  // Allyタグが見つかったらそのターゲットにする
            }
        }

        currentTarget = bestTarget;
        Debug.Log(gameObject.name + " new target set to: " + (currentTarget != null ? currentTarget.name : "None"));
    }

    // 攻撃対象に向かって移動する
    private void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            Vector3 targetPosition = currentTarget.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            Debug.Log(gameObject.name + " moving towards target: " + currentTarget.name);

            // ウェイポイントに到達したかどうかをチェック
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                if (currentTarget == baseTarget)
                {
                    return;  // 現在のターゲットが拠点であれば、そのまま攻撃を続ける
                }
                if (currentWaypointIndex < waypoints.Length - 1)
                {
                    currentWaypointIndex++;
                    currentTarget = waypoints[currentWaypointIndex];
                    Debug.Log(gameObject.name + " moving to next waypoint: " + currentTarget.name);
                }
                else
                {
                    currentTarget = baseTarget;
                    Debug.Log(gameObject.name + " moving to base target: " + currentTarget.name);
                }
            }
        }
    }

    // 攻撃範囲に入った対象に攻撃する
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " triggered with: " + other.name);
        if (other.transform == currentTarget && Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackPower);
                Debug.Log(gameObject.name + " attacked " + other.gameObject.name + " for " + attackPower + " damage.");
            }
        }
    }

    // 攻撃範囲を可視化するための Gizmos 描画
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
