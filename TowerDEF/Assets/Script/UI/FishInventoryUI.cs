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

    [SerializeField] private GameObject[] fishUIPrefabs; // 魚ごとのUIプレハブを格納する配列（種類ごとに割り当て）

    // 魚の種類とそのUIインスタンスを管理する辞書
    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>();

    /// <summary>
    /// シーン開始時にUIを更新するために呼び出される。
    /// </summary>
    private void Awake()
    {
        UpdateFishInventoryUI(); // インベントリの内容に基づいてUIを初期化
    }

    /// <summary>
    /// インベントリに基づいて魚のUIを更新する。
    /// </summary>
    public void UpdateFishInventoryUI()
    {
        ClearUI(); // 既存のUIをクリア
        int count = 0; // 表示する魚の数をカウント

        // インベントリから魚の情報を取得し、UIを生成
        foreach (var fish in GameManager.Instance.finventory)
        {
            if (fish.Value > 0) // 魚の数が0より大きい場合のみUIを生成
            {
                GameObject fishUIPrefabInstance = fishUIPrefabs[(int)fish.Key]; // 該当する魚のプレハブを取得
                GameObject uiInstance = Instantiate(fishUIPrefabInstance, contentPanel); // プレハブをコンテンツパネル内に生成
                uiInstance.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize; // サイズを設定

                // 魚の数をテキストに反映
                TextMeshProUGUI fishCountText = uiInstance.transform.Find("FishCountText").GetComponent<TextMeshProUGUI>();
                fishCountText.text = "x" + fish.Value;

                fishUIInstances.Add(fish.Key, uiInstance); // 辞書にUIインスタンスを登録
                count++;
            }
        }

        AdjustContentSize(count); // コンテンツパネルのサイズを調整
        AdjustSpacing(); // UIの間隔を調整
    }

    /// <summary>
    /// 表示中の魚のUIをすべて削除する。
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
    /// 魚の数に応じてコンテンツパネルのサイズを調整する。
    /// </summary>
    private void AdjustContentSize(int itemCount)
    {
        // 高さを最小サイズ以上に設定し、スクロールの有無を決定
        float newHeight = Mathf.Max(minContentSize.y, itemCount * (fishUIPanelSize.y + spacing) - spacing);
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(minContentSize.x, newHeight);
        scrollRect.vertical = newHeight > scrollRect.viewport.rect.height; // コンテンツが表示領域を超える場合にスクロールを有効化
    }

    /// <summary>
    /// 魚UI要素間のスペーシングを調整する。
    /// </summary>
    private void AdjustSpacing()
    {
        VerticalLayoutGroup layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing; // レイアウトグループの間隔を設定
        }
    }
}
