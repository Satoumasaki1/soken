using System.Collections.Generic;
using UnityEngine;

public class SeasonObjectManager : MonoBehaviour
{
    // 季節ごとのオブジェクトリストを保持する辞書
    private Dictionary<GameManager.Season, List<GameObject>> seasonObjects = new Dictionary<GameManager.Season, List<GameObject>>();

    // 季節ごとのSkyBoxマテリアルを設定する配列（インスペクターから設定可能）
    [SerializeField]
    private Material[] seasonSkyboxMaterials;

    private void Awake()
    {
        // 季節ごとのリストを初期化
        foreach (GameManager.Season season in System.Enum.GetValues(typeof(GameManager.Season)))
        {
            seasonObjects[season] = new List<GameObject>();
        }

        // 季節ごとのタグを持つオブジェクトを検索してリストに登録
        RegisterSeasonObjects();
    }

    private void OnEnable()
    {
        // GameManager の季節変更イベントに登録
        GameManager.WaveStarted += OnWaveStarted;
    }

    private void OnDisable()
    {
        // GameManager の季節変更イベントから登録解除
        GameManager.WaveStarted -= OnWaveStarted;
    }

    private void OnWaveStarted()
    {
        // 現在の季節に基づいてオブジェクトを切り替え
        UpdateSeasonObjects(GameManager.Instance.currentSeason);
        UpdateSkybox(GameManager.Instance.currentSeason);
    }

    private void RegisterSeasonObjects()
    {
        foreach (GameManager.Season season in System.Enum.GetValues(typeof(GameManager.Season)))
        {
            // 季節ごとのタグを検索
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(season.ToString());
            foreach (GameObject obj in objectsWithTag)
            {
                seasonObjects[season].Add(obj);
            }
        }
    }

    private void UpdateSeasonObjects(GameManager.Season currentSeason)
    {
        // すべてのオブジェクトを無効化
        foreach (var seasonList in seasonObjects.Values)
        {
            foreach (GameObject obj in seasonList)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // 現在の季節に対応するオブジェクトを有効化
        if (seasonObjects.ContainsKey(currentSeason))
        {
            foreach (GameObject obj in seasonObjects[currentSeason])
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        Debug.Log($"Season changed to {currentSeason}. Objects updated.");
    }

    private void UpdateSkybox(GameManager.Season currentSeason)
    {
        // 現在の季節に対応するSkyBoxマテリアルを設定
        int seasonIndex = (int)currentSeason;
        if (seasonSkyboxMaterials != null && seasonIndex >= 0 && seasonIndex < seasonSkyboxMaterials.Length)
        {
            RenderSettings.skybox = seasonSkyboxMaterials[seasonIndex];
            Debug.Log($"Skybox updated for season: {currentSeason}");
        }
        else
        {
            Debug.LogWarning($"No Skybox material set for season: {currentSeason}");
        }
    }
}
