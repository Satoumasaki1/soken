using UnityEngine;

// ウェーブデータを定義するためのScriptableObjectクラス
[System.Serializable]
public class EnemyInfo
{
    public GameObject enemyPrefab; // 出現させる敵のPrefab
    public int count; // 敵の出現数
}


