using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tpuebi : MonoBehaviour, IDamageable
{
    public float attackRange = 10f;  // タレットの攻撃範囲
    public float attackCooldown = 1f; // 攻撃クールダウン
    public int damage = 1; // ダメージ量
    private float attackTimer = 0f;

    public int hp = 15; // 初期HP
    private Transform targetEnemy;

    void Update()
    {
        // タレットの攻撃タイミングを計測
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            FindTargetEnemy();

            if (targetEnemy != null)
            {
                Attack(targetEnemy);
                attackTimer = attackCooldown;  // 攻撃後にタイマーをリセット
            }
        }

        // HPが0以下ならタレットを破壊
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    // 敵を探索する
    void FindTargetEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= attackRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= attackRange)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // 敵を攻撃する
    void Attack(Transform enemy)
    {
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log(gameObject.name + " attacks " + enemy.name + " with " + damage + " damage.");
        }
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // HPが0になったら破壊
        }
    }
}

