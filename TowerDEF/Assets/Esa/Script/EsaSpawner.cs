using System.Collections.Generic;
using UnityEngine;

public class EsaSpawner : MonoBehaviour
{
    public GameObject esa1Prefab; // OkiaMi用Prefab
    public GameObject esa2Prefab; // Benthos用Prefab
    public GameObject esa3Prefab; // Plankton用Prefab

    public Transform planeArea; // 餌を置くプレーンの領域
    public int maxEsaCount = 4; // 餌の最大設置数
    private int currentEsaCount = 0; // 現在設置されている餌の数

    private bool isEsaPlacingMode = false; // 餌を置くモードのトグル

    private GameManager gameManager; // GameManagerのインスタンス参照

    void Start()
    {
        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        // 餌を置くモードがオンの場合にクリック処理を実行
        if (isEsaPlacingMode && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == planeArea)
            {
                if (gameManager.SelectedFeedType == null)
                {
                    Debug.Log("餌が選択されていません");
                    return;
                }

                GameManager.ResourceType selectedFeedType = gameManager.SelectedFeedType.Value;

                // 在庫確認
                if (!gameManager.inventory.ContainsKey(selectedFeedType) || gameManager.inventory[selectedFeedType] <= 0)
                {
                    Debug.Log($"在庫が不足しています: {selectedFeedType}");
                    return;
                }

                // 餌の設置
                if (currentEsaCount < maxEsaCount)
                {
                    Vector3 position = GetRandomPositionInPlane(planeArea);
                    SpawnSelectedEsa(position, selectedFeedType);
                    gameManager.UpdateResourceUI();
                }
                else
                {
                    Debug.Log("既に最大数の餌が設置されています");
                }
            }
        }
    }

    // 餌を置くモードをトグルする関数
    public void ToggleEsaPlacingMode()
    {
        isEsaPlacingMode = !isEsaPlacingMode;
        Debug.Log("餌を置くモード: " + (isEsaPlacingMode ? "ON" : "OFF"));
    }

    // プレーン上のランダムな位置を計算する関数
    Vector3 GetRandomPositionInPlane(Transform plane)
    {
        Vector3 center = plane.position;
        float width = plane.localScale.x * 5f;  // プレーンの幅
        float length = plane.localScale.z * 5f; // プレーンの長さ

        float x = center.x + Random.Range(-width / 2f, width / 2f);
        float z = center.z + Random.Range(-length / 2f, length / 2f);

        float y = center.y; // プレーンの高さを固定

        return new Vector3(x, y, z);
    }

    // 選択された餌を生成する関数
    void SpawnSelectedEsa(Vector3 position, GameManager.ResourceType selectedFeedType)
    {
        GameObject esaPrefab = null;

        // 選択された餌タイプに応じたPrefabを設定
        switch (selectedFeedType)
        {
            case GameManager.ResourceType.OkiaMi:
                esaPrefab = esa1Prefab;
                break;
            case GameManager.ResourceType.Benthos:
                esaPrefab = esa2Prefab;
                break;
            case GameManager.ResourceType.Plankton:
                esaPrefab = esa3Prefab;
                break;
        }

        if (esaPrefab != null)
        {
            // 餌を生成
            Instantiate(esaPrefab, position, Quaternion.identity);
            currentEsaCount++;

            // 在庫を減少
            gameManager.inventory[selectedFeedType]--;
            Debug.Log($"{selectedFeedType} を設置しました。在庫: {gameManager.inventory[selectedFeedType]}");
        }
        else
        {
            Debug.Log("無効な餌タイプが選択されました");
        }
    }
}
