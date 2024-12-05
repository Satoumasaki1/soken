using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 魚のインベントリをUIに反映するクラス。
/// 在庫がある魚のUIを生成し、在庫が0になった魚のUIは削除する。
/// </summary>
public class FishInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject[] fishUIPrefabs;  // 各魚種ごとのUIプレハブ
    [SerializeField] private Transform contentPanel;      // UIを配置するコンテンツパネル
    [SerializeField] private ScrollRect scrollRect;       // スクロールビューのScrollRect
    [SerializeField] private Vector2 minContentSize = new Vector2(0, 300); // コンテンツパネルの最小サイズ
    [SerializeField] private Vector2 fishUIPanelSize = new Vector2(100, 100); // 各魚UIのサイズ
    [SerializeField] private float spacing = 10f;         // UI要素間のスペース

    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>(); // 魚UIのインスタンス管理用辞書

    public GameManager gameManager; // GameManagerの参照

    /// <summary>
    /// 初期化処理。GameManagerの参照を取得し、UIを初期化する。
    /// </summary>
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>(); // GameManagerをシーンから探して取得
        ClearUIF(); // 既存のUIをクリア
        UpdateFishInventoryUI(); // 初回のUI更新
    }

    /// <summary>
    /// 魚インベントリに基づいてUIを更新する。
    /// 在庫がある魚のUIを生成し、在庫が0の魚のUIは削除する。
    /// </summary>
    public void UpdateFishInventoryUI()
    {
        int count = 0; // 表示する魚の数をカウント

        // finventoryのデータをリスト形式に変換
        List<KeyValuePair<GameManager.ResourceFishType, int>> fishList = new List<KeyValuePair<GameManager.ResourceFishType, int>>(gameManager.finventory);

        for (int i = 0; i < fishList.Count; i++)
        {
            var fish = fishList[i];

            if (fish.Value > 0) // 在庫がある場合のみ処理
            {
                int prefabIndex = (int)fish.Key;

                if (prefabIndex >= 0 && prefabIndex < fishUIPrefabs.Length && fishUIPrefabs[prefabIndex] != null)
                {
                    if (fishUIInstances.ContainsKey(fish.Key))
                    {
                        // 既存のUIを更新
                        UpdateFishCountText(fishUIInstances[fish.Key], fish.Value);
                    }
                    else
                    {
                        // 新しいUIを生成
                        GameObject uiInstance = Instantiate(fishUIPrefabs[prefabIndex], contentPanel);
                        uiInstance.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize;
                        UpdateFishCountText(uiInstance, fish.Value);
                        fishUIInstances[fish.Key] = uiInstance;
                    }

                    count++; // 表示する魚の数をカウント
                }
                else
                {
                    Debug.LogError($"Invalid prefab index or missing prefab for {fish.Key}");
                }
            }
            else
            {
                // 在庫が0の場合はUIを削除
                RemoveFishUI(fish.Key);
            }
        }

        AdjustContentSize(count); // コンテンツパネルのサイズを調整
        AdjustSpacing();          // UI間のスペースを調整
    }

    /// <summary>
    /// 指定された魚UIの数を更新する。
    /// </summary>
    /// <param name="uiInstance">更新するUIインスタンス</param>
    /// <param name="count">魚の数</param>
    private void UpdateFishCountText(GameObject uiInstance, int count)
    {
        // FishCountTextオブジェクトを探してテキストを更新
        TextMeshProUGUI fishCountText = uiInstance.transform.Find("FishCountText").GetComponent<TextMeshProUGUI>();
        fishCountText.text = "x" + count;
    }

    /// <summary>
    /// 在庫が0の魚のUIを削除する。
    /// </summary>
    /// <param name="fishType">削除する魚の種類</param>
    private void RemoveFishUI(GameManager.ResourceFishType fishType)
    {
        if (fishUIInstances.ContainsKey(fishType))
        {
            Destroy(fishUIInstances[fishType]); // UIを削除
            fishUIInstances.Remove(fishType);  // 辞書から削除
        }
    }

    /// <summary>
    /// コンテンツパネルの子オブジェクトをすべて削除する（例外を除く）。
    /// </summary>
    private void ClearUIF()
    {
        foreach (Transform child in contentPanel)
        {
            if (child.name != "ChangeButton") // "ChangeButton"は削除しない
            {
                Destroy(child.gameObject);
            }
        }
        fishUIInstances.Clear(); // 辞書をクリア
    }

    /// <summary>
    /// 表示する魚の数に応じてコンテンツパネルのサイズを調整する。
    /// </summary>
    /// <param name="itemCount">表示する魚の数</param>
    private void AdjustContentSize(int itemCount)
    {
        float totalHeight = itemCount * (fishUIPanelSize.y + spacing) - spacing; // UIの総高さを計算
        float newHeight = Mathf.Max(minContentSize.y, totalHeight); // 最小サイズを下回らないようにする

        // コンテンツパネルの高さを設定
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(minContentSize.x, newHeight);

        // スクロールが必要か判定
        scrollRect.vertical = newHeight > scrollRect.viewport.rect.height;
    }

    /// <summary>
    /// UI要素間のスペーシングを調整する。
    /// </summary>
    private void AdjustSpacing()
    {
        VerticalLayoutGroup layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing; // スペーシングを設定
        }
    }
}
