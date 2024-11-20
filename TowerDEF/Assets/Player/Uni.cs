using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Uni : MonoBehaviour, IDamageable
{
    public int hp = 15; // 初期HP
    public int damage = 2; // 初期ダメージ
    public int level = 1; // 初期レベル
    public int maxLevel = 3; // 最大レベル
    public float attackRange = 1.5f; // 攻撃範囲
    public float attackCooldown = 2f; // 攻撃クールダウン
    private float attackTimer = 0f;

    private Transform target;

    private void Start()
    {
        UpdateStats(); // レベルに応じたステータスを設定
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            FindNearestTarget();
            if (target != null)
            {
                AttackTargetIfInRange();
                attackTimer = attackCooldown;
            }
        }
    }

    // レベルアップ処理
    public void LevelUp()
    {
        if (level < maxLevel)
        {
            level++; // レベルを上げる
            UpdateStats(); // レベルに応じたHPとダメージを更新
            Debug.Log("Uni leveled up to level " + level + "!");
        }
        else
        {
            Debug.Log("Uni has already reached the max level.");
        }
    }

    // レベルに応じてステータスを更新
    private void UpdateStats()
    {
        switch (level)
        {
            case 2:
                hp = 20;
                damage = 5;
                break;
            case 3:
                hp = 30;
                damage = 7;
                break;
            default:
                hp = 15;
                damage = 2;
                break;
        }
    }

    // ターゲットを見つける関数
    private void FindNearestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < nearestDistance && distanceToEnemy <= attackRange)
            {
                nearestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && nearestDistance <= attackRange)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    // ターゲットが攻撃範囲内なら攻撃する
    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= attackRange)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log(gameObject.name + " attacks " + target.name + " with " + damage + " damage.");
            }
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

    // アイテムを使ってレベルアップする関数
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LevelUpItem"))
        {
            LevelUp(); // レベルアップ
            Destroy(other.gameObject); // アイテムを消去
        }
    }
}
