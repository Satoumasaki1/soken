using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int health = 10;
    public float speed = 3.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 5;
    public float attackCooldown = 1.5f;

    private GameObject target;
    private float attackCooldownTimer;

    private void Start()
    {
        // ターゲットを最初はプレイヤーの拠点に設定
        FindNearestTarget();
    }

    private void Update()
    {
        if (target != null)
        {
            MoveTowardsTarget();
            AttackTargetIfInRange();
        }
        else
        {
            // ターゲットがなくなったら新しいターゲットを探す
            FindNearestTarget();
        }
    }

    private void FindNearestTarget()
    {
        // 見方のタワーを優先してターゲットにする
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestAlly = null;

        foreach (GameObject ally in allies)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestAlly = ally;
            }
        }

        if (nearestAlly != null)
        {
            target = nearestAlly;
        }
        else
        {
            // 見方がいない場合はプレイヤーの拠点をターゲットにする
            target = GameObject.FindGameObjectWithTag("Base");
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            if (target.CompareTag("Base"))
            {
                // プレイヤーの拠点に攻撃する
                target.GetComponent<PlayerBaseController>().TakeDamage(attackDamage);
            }
            else if (target.CompareTag("Ally"))
            {
                // 見方のタワーに攻撃する
                target.GetComponent<Healt>().TakeDamage(attackDamage);
            }
            attackCooldownTimer = attackCooldown;
        }

        // クールダウンを減らす
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    // ダメージを受けた際の処理
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " は倒されました");
        Destroy(gameObject);
    }
}

public class Healt : MonoBehaviour
{
    public int currentHealth = 50;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
