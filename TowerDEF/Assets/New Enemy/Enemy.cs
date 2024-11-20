using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 3f; // 敵の移動速度
    public int damage = 10; // 敵が与えるダメージ
    public Transform[] waypoints; // ウェイポイント配列

    private int currentWaypointIndex = 0; // 現在のウェイポイントのインデックス

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[currentWaypointIndex].position;
        }
    }

    void Update()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        MoveTowardsWaypoint();
    }

    void MoveTowardsWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // ウェイポイントに到達した場合
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;

            // 最後のウェイポイントに到達したら
            if (currentWaypointIndex >= waypoints.Length)
            {
                ReachBase();
            }
        }
    }

    void ReachBase()
    {
        // 自城に到達したときの処理（例: ダメージを与えるなど）
        Debug.Log("Enemy has reached the base!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ターゲットに接触したらダメージを与える
        if (collision.gameObject.CompareTag("Ally"))
        {
            Ally ally = collision.gameObject.GetComponent<Ally>();
            if (ally != null)
            {
                ally.TakeDamage(damage);
            }
        }
    }
}

public class Ally : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Ally health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 味方が死亡した際の処理
        Debug.Log("Ally has been defeated!");
        Destroy(gameObject);
    }
}
