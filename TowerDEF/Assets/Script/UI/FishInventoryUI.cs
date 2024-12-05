using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ���̃C���x���g����UI�ɔ��f����N���X�B
/// �݌ɂ����鋛��UI�𐶐����A�݌ɂ�0�ɂȂ�������UI�͍폜����B
/// </summary>
public class FishInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject[] fishUIPrefabs;  // �e���킲�Ƃ�UI�v���n�u
    [SerializeField] private Transform contentPanel;      // UI��z�u����R���e���c�p�l��
    [SerializeField] private ScrollRect scrollRect;       // �X�N���[���r���[��ScrollRect
    [SerializeField] private Vector2 minContentSize = new Vector2(0, 300); // �R���e���c�p�l���̍ŏ��T�C�Y
    [SerializeField] private Vector2 fishUIPanelSize = new Vector2(100, 100); // �e��UI�̃T�C�Y
    [SerializeField] private float spacing = 10f;         // UI�v�f�Ԃ̃X�y�[�X

    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>(); // ��UI�̃C���X�^���X�Ǘ��p����

    public GameManager gameManager; // GameManager�̎Q��

    /// <summary>
    /// �����������BGameManager�̎Q�Ƃ��擾���AUI������������B
    /// </summary>
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>(); // GameManager���V�[������T���Ď擾
        ClearUIF(); // ������UI���N���A
        UpdateFishInventoryUI(); // �����UI�X�V
    }

    /// <summary>
    /// ���C���x���g���Ɋ�Â���UI���X�V����B
    /// �݌ɂ����鋛��UI�𐶐����A�݌ɂ�0�̋���UI�͍폜����B
    /// </summary>
    public void UpdateFishInventoryUI()
    {
        int count = 0; // �\�����鋛�̐����J�E���g

        // finventory�̃f�[�^�����X�g�`���ɕϊ�
        List<KeyValuePair<GameManager.ResourceFishType, int>> fishList = new List<KeyValuePair<GameManager.ResourceFishType, int>>(gameManager.finventory);

        for (int i = 0; i < fishList.Count; i++)
        {
            var fish = fishList[i];

            if (fish.Value > 0) // �݌ɂ�����ꍇ�̂ݏ���
            {
                int prefabIndex = (int)fish.Key;

                if (prefabIndex >= 0 && prefabIndex < fishUIPrefabs.Length && fishUIPrefabs[prefabIndex] != null)
                {
                    if (fishUIInstances.ContainsKey(fish.Key))
                    {
                        // ������UI���X�V
                        UpdateFishCountText(fishUIInstances[fish.Key], fish.Value);
                    }
                    else
                    {
                        // �V����UI�𐶐�
                        GameObject uiInstance = Instantiate(fishUIPrefabs[prefabIndex], contentPanel);
                        uiInstance.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize;
                        UpdateFishCountText(uiInstance, fish.Value);
                        fishUIInstances[fish.Key] = uiInstance;
                    }

                    count++; // �\�����鋛�̐����J�E���g
                }
                else
                {
                    Debug.LogError($"Invalid prefab index or missing prefab for {fish.Key}");
                }
            }
            else
            {
                // �݌ɂ�0�̏ꍇ��UI���폜
                RemoveFishUI(fish.Key);
            }
        }

        AdjustContentSize(count); // �R���e���c�p�l���̃T�C�Y�𒲐�
        AdjustSpacing();          // UI�Ԃ̃X�y�[�X�𒲐�
    }

    /// <summary>
    /// �w�肳�ꂽ��UI�̐����X�V����B
    /// </summary>
    /// <param name="uiInstance">�X�V����UI�C���X�^���X</param>
    /// <param name="count">���̐�</param>
    private void UpdateFishCountText(GameObject uiInstance, int count)
    {
        // FishCountText�I�u�W�F�N�g��T���ăe�L�X�g���X�V
        TextMeshProUGUI fishCountText = uiInstance.transform.Find("FishCountText").GetComponent<TextMeshProUGUI>();
        fishCountText.text = "x" + count;
    }

    /// <summary>
    /// �݌ɂ�0�̋���UI���폜����B
    /// </summary>
    /// <param name="fishType">�폜���鋛�̎��</param>
    private void RemoveFishUI(GameManager.ResourceFishType fishType)
    {
        if (fishUIInstances.ContainsKey(fishType))
        {
            Destroy(fishUIInstances[fishType]); // UI���폜
            fishUIInstances.Remove(fishType);  // ��������폜
        }
    }

    /// <summary>
    /// �R���e���c�p�l���̎q�I�u�W�F�N�g�����ׂč폜����i��O�������j�B
    /// </summary>
    private void ClearUIF()
    {
        foreach (Transform child in contentPanel)
        {
            if (child.name != "ChangeButton") // "ChangeButton"�͍폜���Ȃ�
            {
                Destroy(child.gameObject);
            }
        }
        fishUIInstances.Clear(); // �������N���A
    }

    /// <summary>
    /// �\�����鋛�̐��ɉ����ăR���e���c�p�l���̃T�C�Y�𒲐�����B
    /// </summary>
    /// <param name="itemCount">�\�����鋛�̐�</param>
    private void AdjustContentSize(int itemCount)
    {
        float totalHeight = itemCount * (fishUIPanelSize.y + spacing) - spacing; // UI�̑��������v�Z
        float newHeight = Mathf.Max(minContentSize.y, totalHeight); // �ŏ��T�C�Y�������Ȃ��悤�ɂ���

        // �R���e���c�p�l���̍�����ݒ�
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(minContentSize.x, newHeight);

        // �X�N���[�����K�v������
        scrollRect.vertical = newHeight > scrollRect.viewport.rect.height;
    }

    /// <summary>
    /// UI�v�f�Ԃ̃X�y�[�V���O�𒲐�����B
    /// </summary>
    private void AdjustSpacing()
    {
        VerticalLayoutGroup layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing; // �X�y�[�V���O��ݒ�
        }
    }
}
