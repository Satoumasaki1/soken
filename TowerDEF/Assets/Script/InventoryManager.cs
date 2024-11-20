using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private List<GameObject> inventory = new List<GameObject>();

    public void AddItem(GameObject item)
    {
        inventory.Add(item);
        Debug.Log(item.name + "をインベントリに追加しました！");
    }

    // インベントリの内容を表示する（デバッグ用）
    public void ShowInventory()
    {
        foreach (var item in inventory)
        {
            Debug.Log(item.name);
        }
    }
}
