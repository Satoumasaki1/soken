using System.Collections;
using UnityEngine;

public class SquidController : MonoBehaviour
{
    public int health = 10;
    public float speed = 2.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 3;
    public float attackCooldown = 1.2f;

    public GameObject target;
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

    public GameObject GetTarget()
    {
        return target;
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
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            if (target.CompareTag("Base"))
            {
                // プレイヤーの拠点に攻撃する
                PlayerBaseController baseController = target.GetComponent<PlayerBaseController>();
                if (baseController != null)
                {
                    baseController.TakeDamage(attackDamage);
                }
            }
            else if (target.CompareTag("Ally"))
            {
                // 見方のタワーに攻撃する
                Health healthComponent = target.GetComponent<Health>();
                if (healthComponent != null)
                {
                    healthComponent.TakeDamage(attackDamage);
                }
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
