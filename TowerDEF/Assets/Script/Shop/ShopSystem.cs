using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public GameObject shopPanel; // ショップ全体のパネル
    public GameObject itemPrefab; // アイテム用のプレハブ
    public Transform gridParent; // Grid Layout Group の親オブジェクト
    public int gridSize = 5; // グリッドのサイズ (5x5)
    public ItemData[] items; // アイテムデータの配列

    private Button[] itemButtons; // 生成したアイテムボタンを管理
    private Image[] itemImages; // 各アイテムの見た目を管理
    private bool[] isPurchasable; // 各アイテムが購入可能かどうかを管理
    private bool[] isPurchased; // 各アイテムが購入済みかどうかを管理

    private int pearls = 50; // 初期真珠数


    void Start()
    {
        shopPanel.SetActive(false); // ショップを非表示にする

        int totalItems = gridSize * gridSize;
        itemButtons = new Button[totalItems];
        itemImages = new Image[totalItems];
        isPurchasable = new bool[totalItems];
        isPurchased = new bool[totalItems];

        GenerateShopItems();
        LoadPurchaseState();
    }

    // ショップの表示/非表示を切り替える
    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }

    // ショップアイテムを生成
    void GenerateShopItems()
    {
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            GameObject item = Instantiate(itemPrefab, gridParent);
            Button itemButton = item.GetComponent<Button>();
            Image itemImage = item.GetComponent<Image>();

            itemButtons[i] = itemButton;
            itemImages[i] = itemImage;

            int itemIndex = i; // ローカル変数にインデックスを保存

            if (itemIndex == gridSize * gridSize / 2)
            {
                // 中央のアイテムを購入可能にする
                isPurchasable[itemIndex] = true;
                UpdateItemAppearance(itemIndex);
                itemButton.onClick.AddListener(() => OnItemClicked(itemIndex));
            }
            else
            {
                // 初期状態では購入不可
                isPurchasable[itemIndex] = false;
                UpdateItemAppearance(itemIndex);
            }

            // アイテムのアイコンを設定
            if (i < items.Length)
            {
                itemImage.sprite = items[i].icon; // アイテムに設定されたアイコン
                itemButton.GetComponentInChildren<Text>().text = items[i].cost.ToString(); // アイテム価格を表示
            }
        }
    }

    // アイテムがクリックされたときの処理
    void OnItemClicked(int index)
    {
        if (!isPurchasable[index] || isPurchased[index]) return; // 購入不可または購入済みのアイテムは無視

        if (index < items.Length && pearls >= items[index].cost)
        {
            pearls -= items[index].cost; // 真珠を消費
            isPurchased[index] = true; // 購入済みにする
            Debug.Log($"Item {index} purchased for {items[index].cost} pearls!");

            // アイテムの効果を発動
            items[index].ApplyEffect();

            // 上下左右のアイテムを購入可能にする
            UnlockAdjacentItems(index);
            UpdateItemAppearance(index);
        }
        else
        {
            Debug.Log("Not enough pearls!");
        }
    }

    // 上下左右のアイテムを購入可能にする
    void UnlockAdjacentItems(int index)
    {
        int[] adjacentOffsets = { -gridSize, gridSize, -1, 1 }; // 上下左右のオフセット

        foreach (int offset in adjacentOffsets)
        {
            int adjacentIndex = index + offset;

            // グリッドの範囲外を無視
            if (adjacentIndex < 0 || adjacentIndex >= gridSize * gridSize)
                continue;

            // 左端と右端を跨がないようにする
            if ((index % gridSize == 0 && offset == -1) || (index % gridSize == gridSize - 1 && offset == 1))
                continue;

            if (!isPurchasable[adjacentIndex])
            {
                isPurchasable[adjacentIndex] = true;
                UpdateItemAppearance(adjacentIndex);
                itemButtons[adjacentIndex].onClick.AddListener(() => OnItemClicked(adjacentIndex));
            }
        }
    }

    // アイテムの見た目を更新
    void UpdateItemAppearance(int index)
    {
        if (isPurchased[index])
        {
            itemButtons[index].interactable = false; // クリック不可
            itemImages[index].color = Color.gray; // 購入済みを灰色で表示
        }
        else if (isPurchasable[index])
        {
            itemButtons[index].interactable = true; // クリック可能
            itemImages[index].color = Color.white; // 購入可能を白色で表示
        }
        else
        {
            itemButtons[index].interactable = false; // クリック不可
            itemImages[index].color = Color.black; // 購入不可を黒色で表示
        }
    }

    private void OnApplicationQuit()
    {
        SavePurchaseState();
    }

    private void SavePurchaseState()
    {
        for (int i = 0; i < isPurchased.Length; i++)
        {
            PlayerPrefs.SetInt($"ItemPurchased_{i}", isPurchased[i] ? 1 : 0); // 購入状態を保存
            PlayerPrefs.SetInt($"ItemPurchasable_{i}", isPurchasable[i] ? 1 : 0); // 解放状態を保存
        }

        PlayerPrefs.Save(); // データを保存
        Debug.Log("Purchase and unlock states saved!");
    }

    private void LoadPurchaseState()
    {
        for (int i = 0; i < isPurchased.Length; i++)
        {
            // 購入状態を復元
            if (PlayerPrefs.HasKey($"ItemPurchased_{i}"))
            {
                isPurchased[i] = PlayerPrefs.GetInt($"ItemPurchased_{i}") == 1;
            }

            // 解放状態を復元
            if (PlayerPrefs.HasKey($"ItemPurchasable_{i}"))
            {
                isPurchasable[i] = PlayerPrefs.GetInt($"ItemPurchasable_{i}") == 1;
            }
        }

        // 外見を更新
        for (int i = 0; i < isPurchased.Length; i++)
        {
            UpdateItemAppearance(i);
        }

        Debug.Log("Purchase and unlock states loaded!");
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All saved data reset!");
    }
}

