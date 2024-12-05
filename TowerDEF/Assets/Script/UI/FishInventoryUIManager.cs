using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 魚の在庫を管理し、UIを更新するスクリプト。
/// </summary>
public class FishInventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] fishUIPrefabs; // 各魚種ごとのUIプレハブを格納する配列（8種類）
    [SerializeField] private Transform contentPanel;     // 魚UIを配置するコンテンツパネル
    [SerializeField] private GameObject changeButton;    // ChangeButtonオブジェクト（削除されない）
    [SerializeField] private Vector2 fishUIPanelSize = new Vector2(100, 100); // 魚UIのサイズ
    [SerializeField] private float spacing = 10f;        // 魚UIの間隔

    private GameManager gameManager;                     // GameManagerの参照
    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>(); // 魚の種類ごとのUIを管理

    /// <summary>
    /// 初期化処理。GameManagerを参照し、UIを初期化。
    /// </summary>
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>(); // GameManagerのインスタンスを取得
        InitializeFishInventoryUI();                  // 初回UI更新
    }

    /// <summary>
    /// 初回の魚在庫UIを設定する。
    /// 在庫が0でない魚のみ表示。
    /// </summary>
    private void InitializeFishInventoryUI()
    {
        foreach (Transform child in contentPanel)
        {
            if (child != changeButton.transform)
            {
                Destroy(child.gameObject); // ChangeButton以外の子オブジェクトを削除
            }
        }

        foreach (var fish in gameManager.finventory)
        {
            if (fish.Value > 0)
            {
                CreateFishUI(fish.Key, fish.Value);
            }
        }

        AdjustContentSize();
    }

    /// <summary>
    /// 魚のUIを生成する関数。
    /// </summary>
    private void CreateFishUI(GameManager.ResourceFishType fishType, int count)
    {
        int prefabIndex = (int)fishType;
        if (prefabIndex >= 0 && prefabIndex < fishUIPrefabs.Length)
        {
            GameObject fishUI = Instantiate(fishUIPrefabs[prefabIndex], contentPanel);
            //fishUI.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize;
            UpdateFishCountText(fishUI, count); // 数を更新
            fishUIInstances[fishType] = fishUI;
        }
    }

    /// <summary>
    /// 魚の在庫数テキストを更新する。
    /// </summary>

    private void UpdateFishCountText(GameObject fishUI, int count)
    {
        // TextMeshProUGUIコンポーネントを取得
        TextMeshProUGUI countText = fishUI.GetComponentInChildren<TextMeshProUGUI>();
        if (countText != null)
        {
            countText.text = $"x{count}";  // 在庫数を更新
        }
    }

    /// <summary>
    /// 在庫が増えたときに呼び出される関数。
    /// </summary>
    public void OnFishStockIncreased(GameManager.ResourceFishType fishType, int amount)
    {
        // 増加量が0以下なら何もしない
        if (amount <= 0) return;

        // 魚の在庫数が1になったときだけアイコンを表示する
        if (gameManager.finventory[fishType] == 1)
        {
            // すでにUIが存在する場合は更新しない
            if (!fishUIInstances.ContainsKey(fishType))
            {
                // 新しくUIを作成する
                CreateFishUI(fishType, gameManager.finventory[fishType]);
                // コンテンツパネルのサイズを調整
                AdjustContentSize();
                //在庫数を更新
                UpdateFishCountText(fishUIInstances[fishType], gameManager.finventory[fishType]);
            }
        }
        // 在庫が1以上のときは、アイコンを新たに作成しない
        else
        {
            // UIが既にあれば、数量を更新する処理を行う
            if (fishUIInstances.ContainsKey(fishType))
            {
                UpdateFishCountText(fishUIInstances[fishType], gameManager.finventory[fishType]);
            }
        }
    }


    /// <summary>
    /// 在庫が減ったときに呼び出される関数。
    /// </summary>
    public void OnFishStockDecreased(GameManager.ResourceFishType fishType, int amount)
    {
        // 減少量が0以下なら何もしない
        if (amount <= 0) return;

        // 在庫が0以下になった場合、アイコンを削除してパネルを調整
        if (gameManager.finventory[fishType] <= 0)
        {
            // アイコンが存在する場合、削除する
            if (fishUIInstances.ContainsKey(fishType))
            {
                Destroy(fishUIInstances[fishType]);  // アイコンを削除
                fishUIInstances.Remove(fishType);    // インスタンスリストから削除
                AdjustContentSize();                 // パネルのサイズを調整
            }
        }
        // 在庫が0でなければ、在庫数を更新
        UpdateFishCountText(fishUIInstances[fishType], gameManager.finventory[fishType]);
    }


    /// <summary>
    /// コンテンツパネルのサイズを調整する。
    /// </summary>
    private void AdjustContentSize()
    {
        int itemCount = fishUIInstances.Count;
        float totalHeight = 0f;

        // 各アイコンの高さを合計していく
        foreach (var fishUIInstance in fishUIInstances.Values)
        {
            RectTransform rectTransform = fishUIInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                totalHeight += rectTransform.rect.height;  // アイコンの高さを加算
            }
        }

        // アイコンの間隔を考慮して高さを調整
        totalHeight += (itemCount - 1) * spacing;  // アイコン間の間隔を加算

        // 最小高さを確保
        float newHeight = Mathf.Max(300, totalHeight); // 最小高さ300

        // コンテンツパネルの高さを更新
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
    }

}
