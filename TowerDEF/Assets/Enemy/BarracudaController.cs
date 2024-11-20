using System.Collections;
using UnityEngine;

public class BarracudaController : MonoBehaviour
{
    public int health = 20;
    public float speed = 2.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 5;
    public float attackCooldown = 1.0f;

    private GameObject target;
    private float attackCooldownTimer;

    private void Start()
    {
        // ターゲットを最初に設定
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
        // Allyタグを持つオブジェクトを優先してターゲットにする
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
            // Allyタグのオブジェクトがいない場合はプレイヤーの拠点をターゲットにする
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
                // Allyタグのオブジェクトに攻撃する
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
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

    // ダメージを受ける関数
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
        Debug.Log(gameObject.name + " は倒されました");
        Destroy(gameObject);
    }
}
