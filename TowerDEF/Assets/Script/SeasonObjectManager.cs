using System.Collections.Generic;
using UnityEngine;

public class SeasonObjectManager : MonoBehaviour
{
    // �G�߂��Ƃ̃I�u�W�F�N�g���X�g��ێ����鎫��
    private Dictionary<GameManager.Season, List<GameObject>> seasonObjects = new Dictionary<GameManager.Season, List<GameObject>>();

    // �G�߂��Ƃ�SkyBox�}�e���A����ݒ肷��z��i�C���X�y�N�^�[����ݒ�\�j
    [SerializeField]
    private Material[] seasonSkyboxMaterials;

    private void Awake()
    {
        // �G�߂��Ƃ̃��X�g��������
        foreach (GameManager.Season season in System.Enum.GetValues(typeof(GameManager.Season)))
        {
            seasonObjects[season] = new List<GameObject>();
        }

        // �G�߂��Ƃ̃^�O�����I�u�W�F�N�g���������ă��X�g�ɓo�^
        RegisterSeasonObjects();
    }

    private void OnEnable()
    {
        // GameManager �̋G�ߕύX�C�x���g�ɓo�^
        GameManager.WaveStarted += OnWaveStarted;
    }

    private void OnDisable()
    {
        // GameManager �̋G�ߕύX�C�x���g����o�^����
        GameManager.WaveStarted -= OnWaveStarted;
    }

    private void OnWaveStarted()
    {
        // ���݂̋G�߂Ɋ�Â��ăI�u�W�F�N�g��؂�ւ�
        UpdateSeasonObjects(GameManager.Instance.currentSeason);
        UpdateSkybox(GameManager.Instance.currentSeason);
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

    private void UpdateSeasonObjects(GameManager.Season currentSeason)
    {
        // ���ׂẴI�u�W�F�N�g�𖳌���
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
}
