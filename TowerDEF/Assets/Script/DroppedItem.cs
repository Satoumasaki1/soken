using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public string itemName = "Item";  // 入手するアイテムの名前（デバッグ用）

    void OnMouseDown()
    {
        // アイテムを入手（デバッグ用のログ表示）
        Debug.Log($"{itemName}を入手しました！");

        // アイテムをシーンから削除
        Destroy(gameObject);
    }
}
