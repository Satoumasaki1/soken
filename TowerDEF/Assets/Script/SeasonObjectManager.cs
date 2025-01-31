using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonObjectManager : MonoBehaviour
{
    // 季節ごとのオブジェクトリストを保持する辞書
    private Dictionary<GameManager.Season, List<GameObject>> seasonObjects = new Dictionary<GameManager.Season, List<GameObject>>();

    // 季節ごとのSkyBoxマテリアルを設定する配列（インスペクターから設定可能）
    [SerializeField]
    private Material[] seasonSkyboxMaterials;

    // 季節ごとのFog設定
    [System.Serializable]
    public class FogSettings
    {
        public Color fogColor;
        public float fogDensity;
    }

    [SerializeField]
    private FogSettings[] seasonFogSettings;

    // フェードイン・フェードアウト用のキャンバスとイメージ
    [SerializeField]
    private CanvasGroup fadeCanvasGroup;
    [SerializeField]
    private float fadeDuration = 1.3f;

    // 現在の季節を記録（初期値はnull）
    private GameManager.Season? previousSeason = null;

    private void Awake()
    {
        // 季節ごとのリストを初期化
    foreach (GameManager.Season season in System.Enum.GetValues(typeof(GameManager.Season)))
        {
            seasonObjects[season] = new List<GameObject>();
        }

        // 季節ごとのタグを持つオブジェクトを検索してリストに登録
        RegisterSeasonObjects();

        // ゲーム開始時にすべての季節のオブジェクトを非表示にする
        HideAllSeasonObjects();

        // 現在の季節の初期化処理
        var currentSeason = GameManager.Instance.currentSeason;
        previousSeason = currentSeason;
        UpdateSeasonObjects(currentSeason);
        UpdateSkybox(currentSeason);
        UpdateFog(currentSeason);
    }

    private void Start()
    {
        // 現在の季節の初期化処理
        var currentSeason = GameManager.Instance.currentSeason;
        previousSeason = currentSeason;

        // 必要なオブジェクトを即時に表示し、SkyBoxとFogを設定
        UpdateSeasonObjects(currentSeason);
        UpdateSkybox(currentSeason);
        UpdateFog(currentSeason);

        // フェード処理を開始
        StartCoroutine(Fade(0.0f)); // 初期状態ではフェードインのみ実行
    }

    private void OnEnable()
    {
        // GameManager の季節変更イベントに登録
        GameManager.SeasonChanged += OnSeasonChanged;
    }

    private void OnDisable()
    {
        // GameManager の季節変更イベントから登録解除
        GameManager.SeasonChanged -= OnSeasonChanged;
    }

    private void OnSeasonChanged(GameManager.Season currentSeason)
    {
        // 季節が切り替わった場合のみフェード処理を行う
        if (previousSeason == null || currentSeason != previousSeason)
        {
            previousSeason = currentSeason; // 季節を更新
            StartCoroutine(FadeSeasonChange(currentSeason));
        }
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

    private void HideAllSeasonObjects()
    {
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
    }

    private IEnumerator FadeSeasonChange(GameManager.Season currentSeason)
    {
        // フェードアウト
        yield return StartCoroutine(Fade(1.0f));

        // 季節ごとのオブジェクトを更新
        UpdateSeasonObjects(currentSeason);
        UpdateSkybox(currentSeason);
        UpdateFog(currentSeason);

        // フェードイン
        yield return StartCoroutine(Fade(0.0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private void UpdateSeasonObjects(GameManager.Season currentSeason)
    {
        // すべてのオブジェクトを無効化
        HideAllSeasonObjects();

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

    private void UpdateFog(GameManager.Season currentSeason)
    {
        // 現在の季節に対応するFogの設定を適用
        int seasonIndex = (int)currentSeason;
        if (seasonFogSettings != null && seasonIndex >= 0 && seasonIndex < seasonFogSettings.Length)
        {
            RenderSettings.fogColor = seasonFogSettings[seasonIndex].fogColor;
            RenderSettings.fogDensity = seasonFogSettings[seasonIndex].fogDensity;
            Debug.Log($"Fog updated for season: {currentSeason}");
        }
        else
        {
            Debug.LogWarning($"No fog settings set for season: {currentSeason}");
        }
    }
}
