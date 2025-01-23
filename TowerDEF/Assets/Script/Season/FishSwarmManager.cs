using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSwarmManager : MonoBehaviour
{
    [Header("Fish Swarm Settings")]
    public GameObject fishSwarmPrefab; // 魚群のプレハブ
    public GameObject fishPrefab; // 魚個体のプレハブ
    public int fishCount = 10; // 魚の数
    public float fishSpacing = 1f; // 魚同士の距離
    public List<Transform> lines; // ラインリスト
    public float movementSpeed = 5f; // 魚群全体の移動速度
    public float swarmDuration = 10f; // 魚群がアクティブな期間
    public int damagePerSecond = 1; // 魚群が与えるダメージ量
    public float damageRadius = 2f; // ダメージ範囲

    [Header("Game State Settings")]
    public string currentSeason = "Autumn"; // 現在の季節
    public int currentWave; // 現在のウェーブ番号
    public int[] waveNumbers = { 5, 9, 13, 18, 22, 26, 31, 35, 39, 44, 48, 52 }; // 魚群を発生させるウェーブ番号

    private bool hasSwarmSpawnedThisWave = false; // このウェーブで魚群が発生したか

    private void Update()
    {
        // 季節が秋で指定ウェーブなら魚群を発生させる
        if (currentSeason == "Autumn" && ShouldSpawnSwarm())
        {
            StartCoroutine(SpawnFishSwarm());
            hasSwarmSpawnedThisWave = true; // このウェーブでは魚群を発生させた
        }
    }

    private bool ShouldSpawnSwarm()
    {
        // 指定されたウェーブ番号で魚群がまだ発生していない場合にtrueを返す
        foreach (int wave in waveNumbers)
        {
            if (wave == currentWave && !hasSwarmSpawnedThisWave)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator SpawnFishSwarm()
    {
        // ランダムなラインを選択
        Transform selectedLine = lines[Random.Range(0, lines.Count)];

        // 魚群オブジェクトを生成
        GameObject fishSwarm = new GameObject("FishSwarm");
        fishSwarm.tag = "FishSwarm";
        fishSwarm.transform.position = selectedLine.position;

        // プレハブがラインの進行方向を向くように回転
        fishSwarm.transform.rotation = Quaternion.LookRotation(selectedLine.forward);

        // 魚を生成して配置
        for (int i = 0; i < fishCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-fishSpacing, fishSpacing),
                Random.Range(-fishSpacing, fishSpacing),
                Random.Range(-fishSpacing, fishSpacing)
            );

            GameObject fish = Instantiate(fishPrefab, fishSwarm.transform.position + offset, Quaternion.identity, fishSwarm.transform);
        }

        // 魚群をラインに沿って移動
        float elapsedTime = 0f;
        while (elapsedTime < swarmDuration)
        {
            fishSwarm.transform.position += selectedLine.forward * movementSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 一定時間後に魚群を削除
        Destroy(fishSwarm);
        hasSwarmSpawnedThisWave = false; // 次のウェーブで再度魚群を発生可能にする
    }

    private void ApplyDamage(Vector3 swarmPosition)
    {
        // 魚群の周囲にある対象を検出
        Collider[] colliders = Physics.OverlapSphere(swarmPosition, damageRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Ally") || collider.CompareTag("Enemy"))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // フレームレートに依存しないダメージを与える
                    damageable.TakeDamage(Mathf.CeilToInt(damagePerSecond * Time.deltaTime));
                }
            }
        }
    }
}
