using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FishInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject fishUIPrefab;  // 魚の数を表示するUIプレハブ
    [SerializeField] private Transform contentPanel;   // スクロールビューのコンテンツパネル
    [SerializeField] private ScrollRect scrollRect;    // スクロールビュー自体
    [SerializeField] private Vector2 minContentSize = new Vector2(0, 300); // コンテンツパネルの最小サイズ（高さの下限）
    [SerializeField] private Vector2 fishUIPanelSize = new Vector2(100, 100); // 各魚UIパネルのサイズ
    [SerializeField] private float spacing = 10f; // 魚UIの間隔（UI同士の垂直スペース）

    // 魚の種類ごとのUIプレハブを保持するための配列
    [SerializeField] private GameObject[] fishUIPrefabs;

    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>();

    private void Start()
    {
        UpdateFishInventoryUI(); // シーン開始時にUIを更新
    }

    /// <summary>
    /// 魚のインベントリに基づいてUIを更新します。
    /// </summary>
    public void UpdateFishInventoryUI()
    {
        ClearUI(); // 既存のUIをすべて削除
        int count = 0; // 表示する魚の数をカウント

        // インベントリ内の魚を確認してUIを更新
        foreach (var fish in GameManager.Instance.finventory)
        {
            if (fish.Value > 0) // 数が0より大きい場合のみ表示
            {
                // 魚の種類に応じたプレハブを選択
                GameObject fishUIPrefabInstance = fishUIPrefabs[(int)fish.Key];  // 魚の種類に対応するプレハブを選択
                GameObject uiInstance = Instantiate(fishUIPrefabInstance, contentPanel);
                uiInstance.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize;

                // 子オブジェクトのテキストを取得して魚の数を表示
                TextMeshProUGUI fishCountText = uiInstance.transform.Find("FishCountText").GetComponent<TextMeshProUGUI>();
                fishCountText.text = "x" + fish.Value;   // 魚の数を表示

                fishUIInstances.Add(fish.Key, uiInstance); // インスタンスを辞書に登録
                count++;
            }
        }

        AdjustContentSize(count); // コンテンツパネルのサイズを調整
        AdjustSpacing();          // UI間のスペースを設定
    }

    /// <summary>
    /// 表示中のUIをすべて削除します。
    /// </summary>
    private void ClearUI()
    {
        foreach (var instance in fishUIInstances.Values)
        {
            Destroy(instance); // UIインスタンスを削除
        }
        fishUIInstances.Clear(); // 辞書をクリア
    }

    /// <summary>
    /// コンテンツパネルのサイズを魚の数に応じて調整します。
    /// </summary>
    private void AdjustContentSize(int itemCount)
    {
        float newHeight = Mathf.Max(minContentSize.y, itemCount * (fishUIPanelSize.y + spacing) - spacing);
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(minContentSize.x, newHeight);
        scrollRect.vertical = newHeight > scrollRect.viewport.rect.height;
    }

    /// <summary>
    /// UI要素間のスペースを設定します。
    /// </summary>
    private void AdjustSpacing()
    {
        VerticalLayoutGroup layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing; // スペーシングを適用
        }
    }
}
