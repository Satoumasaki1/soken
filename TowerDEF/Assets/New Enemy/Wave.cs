using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public List<GameObject> enemyPrefabs; // 出現する敵の種類
    public List<int> enemyCounts;         // 各敵の出現数（enemyPrefabsと対応）
    public List<float> spawnIntervals;    // 各敵の出現間隔（enemyPrefabsと対応）
}
