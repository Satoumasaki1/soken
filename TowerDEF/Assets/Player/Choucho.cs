using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choucho : MonoBehaviour
{
    public float healRange = 10f;  // ヒーラーの回復範囲
    public int healerHP = 150;      // ヒーラーのHP
    public int healAmount = 50;     // ヒール量
    public float healCooldown = 3f; // ヒールのクールダウン
    private float healTimer = 0f;   // ヒールタイマー

    void Update()
    {
        // ヒーラーのHPが0なら破壊
        if (healerHP <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // ヒールタイマーを更新
        healTimer -= Time.deltaTime;
        if (healTimer <= 0f)
        {
            HealBarracksAndAllies();
            healTimer = healCooldown; // ヒールタイマーをリセット
        }
    }

    // 兵舎や味方を回復する関数
    void HealBarracksAndAllies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange);
        foreach (Collider collider in colliders)
        {
            Kaniheisya barracks = collider.GetComponent<Kaniheisya>();

            if (barracks != null && barracks.hp < 20)  // 兵舎のHPが最大でない場合
            {
                barracks.hp += healAmount;
                Debug.Log(gameObject.name + " healed " + barracks.name + " for " + healAmount + " HP.");
            }
            else if (collider.CompareTag("Ally") || collider.CompareTag("koukaku"))  // Allyまたはkoukakuタグを持つ生物のHPが最大でない場合
            {
                Debug.Log(gameObject.name + " healed " + collider.name + " for " + healAmount + " HP.");
            }
        }
    }
}
