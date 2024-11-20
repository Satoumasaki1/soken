using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : MonoBehaviour
{
    public float healRange = 10f;  // ヒーラーの回復範囲
    public int healAmount = 2;     // 1秒ごとの回復量
    public int healerHP = 10;      // ヒーラーのHP
    private float healInterval = 1f;  // 1秒に1回の回復
    private float healTimer = 0f;  // 回復用のタイマー

    void Update()
    {
        healTimer -= Time.deltaTime;

        if (healTimer <= 0f)
        {
            HealSoldiers();  // 範囲内の兵士を回復
            healTimer = healInterval;  // タイマーリセット
        }

        // ヒーラーのHPが0以下なら破壊
        if (healerHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    // 範囲内の兵士を回復する関数
    void HealSoldiers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange);
        foreach (Collider collider in colliders)
        {
            KaniHeisi soldier = collider.GetComponent<KaniHeisi>();
            if (soldier != null && soldier.hp < soldier.maxHP)
            {
                soldier.hp = Mathf.Min(soldier.hp + healAmount, soldier.maxHP);  // カニ兵士のHPを回復
            }

            Tpuebi soldier2 = collider.GetComponent<Tpuebi>();
            if (soldier != null && soldier.hp < soldier.maxHP)
            {
                soldier.hp = Mathf.Min(soldier.hp + healAmount, soldier.maxHP);  // 鉄砲エビのHPを回復
            }

            Uni soldier3 = collider.GetComponent<Uni>();
            if (soldier != null && soldier.hp < soldier.maxHP)
            {
                soldier.hp = Mathf.Min(soldier.hp + healAmount, soldier.maxHP);  // ウニのHPを回復
            }
        }
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        healerHP -= damageAmount;
    }
}
