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
    public bool isWaveActive; // 戦闘WAVEがアクティブかどうか
    public string currentSeason = "Autumn"; // 現在の季節

    private void Update()
    {
        // 季節が秋で戦闘WAVEがアクティブな場合に魚群を発生させる
        if (currentSeason == "Autumn" && isWaveActive && !IsSwarmActive())
        {
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
        Transform selectedLine = lines[Random.Range(0, lines.Count)];

        // デバッグ: ラインの進行方向を確認
        Debug.Log("Selected Line Forward: " + selectedLine.forward);

        // 魚群オブジェクトを生成
        GameObject fishSwarm = new GameObject("FishSwarm");
        fishSwarm.tag = "FishSwarm";
        fishSwarm.transform.position = selectedLine.position;

        // プレハブがラインの進行方向を向くように回転
        fishSwarm.transform.rotation = Quaternion.LookRotation(selectedLine.forward);

        Debug.Log("FishSwarm Rotation: " + fishSwarm.transform.rotation.eulerAngles);

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

            // 移動中の進行方向確認
            Debug.Log("FishSwarm Position: " + fishSwarm.transform.position);
            yield return null;
        }

        // 一定時間後に魚群を削除
        Destroy(fishSwarm);
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
