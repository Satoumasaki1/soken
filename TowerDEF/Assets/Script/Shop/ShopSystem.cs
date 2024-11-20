using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public GameObject shopUI;
    public int pearls; // 現在の真珠数
    public Text pearlText;

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int cost;
        public Button button;
        public GameObject characterToUpgrade;
        public bool isUnlocked = false; // 初期状態はロック
    }

    public List<ShopItem> shopItems;

    void Start()
    {
        UpdateUI();

        // 最初のアイテム(テッポウエビ)のみ解放
        shopItems[0].isUnlocked = true;
        shopItems[0].button.interactable = true;
        UpdateButtonVisual(shopItems[0].button, true);
    }

    public void OpenShop()
    {
        shopUI.SetActive(!shopUI.activeSelf);
    }

    public void PurchaseItem(int index)
    {
        ShopItem item = shopItems[index];

        if (pearls >= item.cost && item.isUnlocked)
        {
            pearls -= item.cost;
            item.button.interactable = false;
            UpdateButtonVisual(item.button, false);

            // キャラクターの強化を実行
            item.characterToUpgrade.GetComponent<CharacterUpgrade>().ApplyUpgrade();

            // アイテムのロック解除処理 (上下左右)
            UnlockAdjacentItems(index);
            UpdateUI();
        }
    }

    private void UnlockAdjacentItems(int index)
    {
        int[] adjacentIndices = { index - 1, index + 1, index - 4, index + 4 }; // グリッドベース
        foreach (int i in adjacentIndices)
        {
            if (i >= 0 && i < shopItems.Count)
            {
                shopItems[i].isUnlocked = true;
                shopItems[i].button.interactable = true;
                UpdateButtonVisual(shopItems[i].button, true);
            }
        }
    }

    private void UpdateUI()
    {
        pearlText.text = $"Pearls: {pearls}";
    }

    private void UpdateButtonVisual(Button button, bool isUnlocked)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = isUnlocked ? Color.white : Color.gray;
        button.colors = colors;
    }
}
