using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// ���̍݌ɂ��Ǘ����AUI���X�V����X�N���v�g�B
/// </summary>
public class FishInventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] fishUIPrefabs; // �e���킲�Ƃ�UI�v���n�u���i�[����z��i8��ށj
    [SerializeField] private Transform contentPanel;     // ��UI��z�u����R���e���c�p�l��
    [SerializeField] private GameObject changeButton;    // ChangeButton�I�u�W�F�N�g�i�폜����Ȃ��j
    [SerializeField] private Vector2 fishUIPanelSize = new Vector2(100, 100); // ��UI�̃T�C�Y
    [SerializeField] private float spacing = 10f;        // ��UI�̊Ԋu

    private GameManager gameManager;                     // GameManager�̎Q��
    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>(); // ���̎�ނ��Ƃ�UI���Ǘ�

    /// <summary>
    /// �����������BGameManager���Q�Ƃ��AUI���������B
    /// </summary>
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>(); // GameManager�̃C���X�^���X���擾
        InitializeFishInventoryUI();                  // ����UI�X�V
    }

    /// <summary>
    /// ����̋��݌�UI��ݒ肷��B
    /// �݌ɂ�0�łȂ����̂ݕ\���B
    /// </summary>
    private void InitializeFishInventoryUI()
    {
        foreach (Transform child in contentPanel)
        {
            if (child != changeButton.transform)
            {
                Destroy(child.gameObject); // ChangeButton�ȊO�̎q�I�u�W�F�N�g���폜
            }
        }

        foreach (var fish in gameManager.finventory)
        {
            if (fish.Value > 0)
            {
                CreateFishUI(fish.Key, fish.Value);
            }
        }

        AdjustContentSize();
    }

    /// <summary>
    /// ����UI�𐶐�����֐��B
    /// </summary>
    private void CreateFishUI(GameManager.ResourceFishType fishType, int count)
    {
        int prefabIndex = (int)fishType;
        if (prefabIndex >= 0 && prefabIndex < fishUIPrefabs.Length)
        {
            GameObject fishUI = Instantiate(fishUIPrefabs[prefabIndex], contentPanel);
            //fishUI.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize;
            UpdateFishCountText(fishUI, count); // �����X�V
            fishUIInstances[fishType] = fishUI;
        }
    }

    /// <summary>
    /// ���̍݌ɐ��e�L�X�g���X�V����B
    /// </summary>

    private void UpdateFishCountText(GameObject fishUI, int count)
    {
        // TextMeshProUGUI�R���|�[�l���g���擾
        TextMeshProUGUI countText = fishUI.GetComponentInChildren<TextMeshProUGUI>();
        if (countText != null)
        {
            countText.text = $"x{count}";  // �݌ɐ����X�V
        }
    }

    /// <summary>
    /// �݌ɂ��������Ƃ��ɌĂяo�����֐��B
    /// </summary>
    public void OnFishStockIncreased(GameManager.ResourceFishType fishType, int amount)
    {
        // �����ʂ�0�ȉ��Ȃ牽�����Ȃ�
        if (amount <= 0) return;

        // ���̍݌ɐ���1�ɂȂ����Ƃ������A�C�R����\������
        if (gameManager.finventory[fishType] == 1)
        {
            // ���ł�UI�����݂���ꍇ�͍X�V���Ȃ�
            if (!fishUIInstances.ContainsKey(fishType))
            {
                // �V����UI���쐬����
                CreateFishUI(fishType, gameManager.finventory[fishType]);
                // �R���e���c�p�l���̃T�C�Y�𒲐�
                AdjustContentSize();
                //�݌ɐ����X�V
                UpdateFishCountText(fishUIInstances[fishType], gameManager.finventory[fishType]);
            }
        }
        // �݌ɂ�1�ȏ�̂Ƃ��́A�A�C�R����V���ɍ쐬���Ȃ�
        else
        {
            // UI�����ɂ���΁A���ʂ��X�V���鏈�����s��
            if (fishUIInstances.ContainsKey(fishType))
            {
                UpdateFishCountText(fishUIInstances[fishType], gameManager.finventory[fishType]);
            }
        }
    }


    /// <summary>
    /// �݌ɂ��������Ƃ��ɌĂяo�����֐��B
    /// </summary>
    public void OnFishStockDecreased(GameManager.ResourceFishType fishType, int amount)
    {
        // �����ʂ�0�ȉ��Ȃ牽�����Ȃ�
        if (amount <= 0) return;

        // �݌ɂ�0�ȉ��ɂȂ����ꍇ�A�A�C�R�����폜���ăp�l���𒲐�
        if (gameManager.finventory[fishType] <= 0)
        {
            // �A�C�R�������݂���ꍇ�A�폜����
            if (fishUIInstances.ContainsKey(fishType))
            {
                Destroy(fishUIInstances[fishType]);  // �A�C�R�����폜
                fishUIInstances.Remove(fishType);    // �C���X�^���X���X�g����폜
                AdjustContentSize();                 // �p�l���̃T�C�Y�𒲐�
            }
        }
        // �݌ɂ�0�łȂ���΁A�݌ɐ����X�V
        UpdateFishCountText(fishUIInstances[fishType], gameManager.finventory[fishType]);
    }


    /// <summary>
    /// �R���e���c�p�l���̃T�C�Y�𒲐�����B
    /// </summary>
    private void AdjustContentSize()
    {
        int itemCount = fishUIInstances.Count;
        float totalHeight = 0f;

        // �e�A�C�R���̍��������v���Ă���
        foreach (var fishUIInstance in fishUIInstances.Values)
        {
            RectTransform rectTransform = fishUIInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                totalHeight += rectTransform.rect.height;  // �A�C�R���̍��������Z
            }
        }

        // �A�C�R���̊Ԋu���l�����č����𒲐�
        totalHeight += (itemCount - 1) * spacing;  // �A�C�R���Ԃ̊Ԋu�����Z

        // �ŏ��������m��
        float newHeight = Mathf.Max(300, totalHeight); // �ŏ�����300

        // �R���e���c�p�l���̍������X�V
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
    }

}
