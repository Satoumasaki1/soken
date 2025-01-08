using UnityEngine;

public class GathererCharacter : MonoBehaviour
{
    public float attackRange = 2f;         // 攻撃範囲
    public float attackInterval = 1f;     // 攻撃間隔
    private float attackTimer = 0f;
    public float moveSpeed = 3f;          // キャラクターの移動速度
    public int maxHP = 30;                // キャラクターの最大HP
    private int currentHP;                // 現在のHP
    private BreakableObject targetObject; // 現在のターゲット

    private void Start()
    {
        currentHP = maxHP; // HPを初期化
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        // ターゲットが設定されていない場合、新しいターゲットを探す
        if (targetObject == null || targetObject.IsBroken)
        {
            FindClosestTarget();
        }

        // ターゲットに接近して攻撃
        if (targetObject != null)
        {
            MoveTowardsTarget();

            // 攻撃範囲内に入ったら攻撃
            float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);
            if (distanceToTarget <= attackRange && attackTimer >= attackInterval)
            {
                Attack(targetObject);
                attackTimer = 0f;
            }
        }
    }

    private void FindClosestTarget()
    {
        // タグが"sozai"のオブジェクトを探す
        GameObject[] candidates = GameObject.FindGameObjectsWithTag("sozai");
        BreakableObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (var candidate in candidates)
        {
            BreakableObject breakable = candidate.GetComponent<BreakableObject>();
            if (breakable != null && !breakable.IsBroken)
            {
                float distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = breakable;
                }
            }
        }

        targetObject = closestObject; // 最も近いターゲットを設定
    }

    private void MoveTowardsTarget()
    {
        if (targetObject != null)
        {
            // ターゲットの方向を計算
            Vector3 direction = (targetObject.transform.position - transform.position).normalized;

            // キャラクターを移動
            transform.position += direction * moveSpeed * Time.deltaTime;

            // ターゲットに向けて回転
            transform.LookAt(new Vector3(targetObject.transform.position.x, transform.position.y, targetObject.transform.position.z));
        }
    }

    private void Attack(BreakableObject target)
    {
        target.TakeDamage();
        currentHP--;

        // HPが0以下の場合、キャラクターを削除
        if (currentHP <= 0)
        {
            Debug.Log("キャラクターは力尽きました");
            Destroy(gameObject);
        }
    }
}
