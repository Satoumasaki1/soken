using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveData", order = 1)]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemyInfo
    {
        public GameObject enemyPrefab; // スポーンさせる敵のプレハブ
        public int count; // スポーンする敵の数
    }

    public EnemyInfo[] enemies; // ウェーブごとの敵情報の配列
}

// このスクリプタブルオブジェクトを使用して、各ウェーブでスポーンさせたい敵を設定できます。
// WaveDataをプロジェクト内で作成し、各ウェーブに対して異なる敵の種類や数を指定できます。
