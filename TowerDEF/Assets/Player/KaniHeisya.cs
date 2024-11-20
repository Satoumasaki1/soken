using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaniheisya : MonoBehaviour
{
    public GameObject soldierPrefab;  // 兵士のプレハブ
    public float spawnRange = 10f;    // 兵舎の出現範囲
    public float spawnCooldown = 3f;  // 兵士の出現クールダウン
    public int hp = 20;               // 兵舎のHP
    private float spawnTimer = 0f;

    private Transform targetEnemy;

    void Update()
    {
        // 兵舎の出現タイミングを計測
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            FindTargetEnemy();

            if (targetEnemy != null)
            {
                SpawnSoldier();
                spawnTimer = spawnCooldown;  // 兵士出現後にタイマーをリセット
            }
        }

        // HPが0以下なら兵舎を破壊
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
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= spawnRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= spawnRange)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // 兵士を出現させる
    void SpawnSoldier()
    {
        Instantiate(soldierPrefab, transform.position, Quaternion.identity);
    }

    // ダメージを受ける関数
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
    }
}
