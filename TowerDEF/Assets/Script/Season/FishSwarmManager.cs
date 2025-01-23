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
    public List<int> swarmTriggerWaves = new List<int> { 5, 9, 13, 18, 22, 26, 31, 35, 39, 44, 48, 52 };

    private void Update()
    {
        Debug.Log($"Current Season: {currentSeason}, Current Wave: {currentWave}");

        // 季節が秋で指定されたウェーブの場合に魚群を発生
        if (currentSeason == "Autumn" && swarmTriggerWaves.Contains(currentWave) && !IsSwarmActive())
        {
            Debug.Log("Spawning Fish Swarm!");
            StartCoroutine(SpawnFishSwarm());
        }
    }

    private bool IsSwarmActive()
    {
        // 魚群が既にアクティブかどうかを確認
        return GameObject.FindGameObjectWithTag("FishSwarm") != null;
    }

    private IEnumerator SpawnFishSwarm()
    {
        // ランダムなラインを選択
        if (lines.Count == 0)
        {
            Debug.LogError("Lines list is empty! Please assign lines.");
            yield break;
        }

        Transform selectedLine = lines[Random.Range(0, lines.Count)];
        Debug.Log($"Selected Line: {selectedLine.name}");

        // 魚群オブジェクトを生成
        GameObject fishSwarm = new GameObject("FishSwarm");
        fishSwarm.tag = "FishSwarm";
        fishSwarm.transform.position = selectedLine.position;
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
    }
}
