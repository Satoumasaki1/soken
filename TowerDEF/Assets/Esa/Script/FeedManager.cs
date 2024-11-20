using System.Collections.Generic;
using UnityEngine;

public class FeedManager : MonoBehaviour
{
    // 餌の種類を列挙型で定義
    public enum FeedType { Plankton, Benthos, OkiaMi }

    // 魚の体力データを保持するクラス
    [System.Serializable]
    public class FishData
    {
        public int currentHealth; // 現在の体力
        public int maxHealth;     // 最大体力
    }

    // 魚オブジェクトとその体力データを関連付ける辞書
    private Dictionary<GameObject, FishData> fishDataMap;

    // 餌ごとの回復量を保持するディクショナリ
    private Dictionary<FeedType, int> feedHealAmounts;

    // 初期化処理
    private void Start()
    {
        // 魚のデータを管理するための辞書を初期化
        fishDataMap = new Dictionary<GameObject, FishData>();
        // 餌ごとの回復量を初期化
        InitializeFeedHealAmounts();
    }

    // 餌ごとの回復量を設定
    private void InitializeFeedHealAmounts()
    {
        feedHealAmounts = new Dictionary<FeedType, int>
        {
            { FeedType.Plankton, 2 }, // プランクトンで回復する量は2
            { FeedType.Benthos, 5 },  // ベントスで回復する量は5
            { FeedType.OkiaMi, 10 }   // オキアミで回復する量は10
        };
    }

    // 魚オブジェクトに対応するFishDataを取得するメソッド
    public FishData GetFishData(GameObject fish)
    {
        // fishDataMapから指定した魚のデータを取得
        if (fishDataMap.TryGetValue(fish, out FishData fishData))
        {
            return fishData; // 見つかった場合、そのデータを返す
        }

        // 魚が見つからなかった場合、デフォルトのFishDataを返す
        return null;
    }

    // 魚に餌をあげて回復処理を行うメソッド
    public void FeedFish(GameObject fishObject, FeedType feedType)
    {
        // 魚のデータを取得または初期化
        if (!fishDataMap.TryGetValue(fishObject, out FishData fishData))
        {
            // 魚の初期体力を設定
            fishData = new FishData { currentHealth = 10, maxHealth = 10 }; // 初期値設定
            fishDataMap[fishObject] = fishData; // 辞書に追加
        }

        // 餌ごとの回復量を取得
        if (feedHealAmounts.TryGetValue(feedType, out int healAmount))
        {
            // 現在の体力が最大体力未満の場合にのみ回復を実行
            if (fishData.currentHealth < fishData.maxHealth)
            {
                // 回復量を適用して体力を更新
                fishData.currentHealth = Mathf.Min(fishData.currentHealth + healAmount, fishData.maxHealth);
                Debug.Log($"{fishObject.name}に{feedType}をあげて、体力が{healAmount}回復しました。");
            }
            else
            {
                Debug.Log($"{fishObject.name}の体力は既に最大です。");
            }
        }
    }

    // 魚がクリックされた時に呼ばれるメソッド
    public void OnFishClicked(GameObject fishObject)
    {
        // 魚に餌をあげる処理を呼び出す（例: プランクトンを与える）
        FeedFish(fishObject, FeedType.Plankton);
    }
}
