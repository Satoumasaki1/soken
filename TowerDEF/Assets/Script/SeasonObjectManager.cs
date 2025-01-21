using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonObjectManager : MonoBehaviour
{
    // �G�߂��Ƃ̃I�u�W�F�N�g���X�g��ێ����鎫��
    private Dictionary<GameManager.Season, List<GameObject>> seasonObjects = new Dictionary<GameManager.Season, List<GameObject>>();

    // �G�߂��Ƃ�SkyBox�}�e���A����ݒ肷��z��i�C���X�y�N�^�[����ݒ�\�j
    [SerializeField]
    private Material[] seasonSkyboxMaterials;

    // �G�߂��Ƃ�Fog�ݒ�
    [System.Serializable]
    public class FogSettings
    {
        public Color fogColor;
        public float fogDensity;
    }

    [SerializeField]
    private FogSettings[] seasonFogSettings;

    // �t�F�[�h�C���E�t�F�[�h�A�E�g�p�̃L�����o�X�ƃC���[�W
    [SerializeField]
    private CanvasGroup fadeCanvasGroup;
    [SerializeField]
    private float fadeDuration = 1.3f;

    // ���݂̋G�߂��L�^�i�����l��null�j
    private GameManager.Season? previousSeason = null;

    private void Awake()
    {
        // �G�߂��Ƃ̃��X�g��������
        foreach (GameManager.Season season in System.Enum.GetValues(typeof(GameManager.Season)))
        {
            seasonObjects[season] = new List<GameObject>();
        }

        // �G�߂��Ƃ̃^�O�����I�u�W�F�N�g���������ă��X�g�ɓo�^
        RegisterSeasonObjects();

        // �Q�[���J�n���ɂ��ׂĂ̋G�߂̃I�u�W�F�N�g���\���ɂ���
        HideAllSeasonObjects();

        // ���݂̋G�߂̏���������
        var currentSeason = GameManager.Instance.currentSeason;
        previousSeason = currentSeason;
        UpdateSeasonObjects(currentSeason);
        UpdateSkybox(currentSeason);
        UpdateFog(currentSeason);
    }

    private void OnEnable()
    {
        // GameManager �̋G�ߕύX�C�x���g�ɓo�^
        GameManager.SeasonChanged += OnSeasonChanged;
    }

    private void OnDisable()
    {
        // GameManager �̋G�ߕύX�C�x���g����o�^����
        GameManager.SeasonChanged -= OnSeasonChanged;
    }

    private void OnSeasonChanged(GameManager.Season currentSeason)
    {
        // �G�߂��؂�ւ�����ꍇ�̂݃t�F�[�h�������s��
        if (previousSeason == null || currentSeason != previousSeason)
        {
            previousSeason = currentSeason; // �G�߂��X�V
            StartCoroutine(FadeSeasonChange(currentSeason));
        }
    }

    private void RegisterSeasonObjects()
    {
        foreach (GameManager.Season season in System.Enum.GetValues(typeof(GameManager.Season)))
        {
            // �G�߂��Ƃ̃^�O������
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
        // �t�F�[�h�A�E�g
        yield return StartCoroutine(Fade(1.0f));

        // �G�߂��Ƃ̃I�u�W�F�N�g���X�V
        UpdateSeasonObjects(currentSeason);
        UpdateSkybox(currentSeason);
        UpdateFog(currentSeason);

        // �t�F�[�h�C��
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
        // ���ׂẴI�u�W�F�N�g�𖳌���
        HideAllSeasonObjects();

        // ���݂̋G�߂ɑΉ�����I�u�W�F�N�g��L����
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
        // ���݂̋G�߂ɑΉ�����SkyBox�}�e���A����ݒ�
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
        // ���݂̋G�߂ɑΉ�����Fog�̐ݒ��K�p
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
