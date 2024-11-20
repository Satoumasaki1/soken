using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaniHeisi : MonoBehaviour
{
    public int maxHP = 5;
    public int hp = 5;    // 兵士のHP
    public int damage = 2; // 兵士のダメージ

    // 敵にダメージを与える関数
    void Attack(Transform enemy)
    {
        // IDamageable インターフェースを持つ敵にダメージを与える
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        if (hp <= 0)
        {
            Destroy(gameObject); // HPが0になったら兵士を破壊
        }
    }
}
