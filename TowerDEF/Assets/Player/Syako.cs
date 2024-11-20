using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Syako : MonoBehaviour, IDamageable
{
    [Header("Syako Settings")]
    public int hp = 100; // Syakoの体力（高体力）
    public int attackPower = 20; // Syakoの攻撃力（高攻撃力）
    public float attackCooldown = 10f; // 攻撃クールダウン（長い攻撃間隔）
    private float attackTimer; // 攻撃タイマー

    private void Start()
    {
        attackTimer = attackCooldown; // タイマーを初期化
    }

    private void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown; // タイマーをリセット
        }
    }

    private void Attack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // 攻撃範囲内の敵を検出
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackPower);
                    Debug.Log("Syako attacks " + collider.name + " with " + attackPower + " damage!");
                }
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log(gameObject.name + " が攻撃を受けました。残り体力: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}


