using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public List<GameObject> enemyPrefabs; // �o������G�̎��
    public List<int> enemyCounts;         // �e�G�̏o�����ienemyPrefabs�ƑΉ��j
    public List<float> spawnIntervals;    // �e�G�̏o���Ԋu�ienemyPrefabs�ƑΉ��j
}
