using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public int maxDurability = 15;          // 最大耐久値
    private int currentDurability;          // 現在の耐久値
    public GameObject materialPrefab;       // ドロップする素材のPrefab
    public Transform dropPosition;          // ドロップ位置
    public float dropRadius = 0.5f;         // ドロップするアイテムの範囲

    // IsBrokenプロパティを追加
    public bool IsBroken
    {
        get { return currentDurability <= 0; } // 耐久値が0以下なら破壊済み
    }

    void Start()
    {
        currentDurability = maxDurability; // 現在の耐久値を初期化
    }

    public void TakeDamage()
    {
        if (currentDurability > 0)
        {
            currentDurability--;

            // 素材をドロップ
            DropMaterial();

            // 耐久値が0になったらオブジェクトを破壊
            if (currentDurability <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void DropMaterial()
    {
        if (materialPrefab != null)
        {
            // ドロップ位置を少しずらして配置
            Vector3 spawnPosition = dropPosition != null ? dropPosition.position : transform.position;
            spawnPosition += new Vector3(Random.Range(-dropRadius, dropRadius), 0, Random.Range(-dropRadius, dropRadius));

            // ドロップアイテムの生成
            Instantiate(materialPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
