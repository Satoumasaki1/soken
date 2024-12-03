using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FishInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject fishUIPrefab;  // ���̐���\������UI�v���n�u
    [SerializeField] private Transform contentPanel;   // �X�N���[���r���[�̃R���e���c�p�l��
    [SerializeField] private ScrollRect scrollRect;    // �X�N���[���r���[����
    [SerializeField] private Vector2 minContentSize = new Vector2(0, 300); // �R���e���c�p�l���̍ŏ��T�C�Y�i�����̉����j
    [SerializeField] private Vector2 fishUIPanelSize = new Vector2(100, 100); // �e��UI�p�l���̃T�C�Y
    [SerializeField] private float spacing = 10f; // ��UI�̊Ԋu�iUI���m�̐����X�y�[�X�j

    [SerializeField] private GameObject[] fishUIPrefabs; // �����Ƃ�UI�v���n�u���i�[����z��i��ނ��ƂɊ��蓖�āj

    // ���̎�ނƂ���UI�C���X�^���X���Ǘ����鎫��
    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>();

    /// <summary>
    /// �V�[���J�n����UI���X�V���邽�߂ɌĂяo�����B
    /// </summary>
    private void Awake()
    {
        UpdateFishInventoryUI(); // �C���x���g���̓��e�Ɋ�Â���UI��������
    }

    /// <summary>
    /// �C���x���g���Ɋ�Â��ċ���UI���X�V����B
    /// </summary>
    public void UpdateFishInventoryUI()
    {
        ClearUI(); // ������UI���N���A
        int count = 0; // �\�����鋛�̐����J�E���g

        // �C���x���g�����狛�̏����擾���AUI�𐶐�
        foreach (var fish in GameManager.Instance.finventory)
        {
            if (fish.Value > 0) // ���̐���0���傫���ꍇ�̂�UI�𐶐�
            {
                GameObject fishUIPrefabInstance = fishUIPrefabs[(int)fish.Key]; // �Y�����鋛�̃v���n�u���擾
                GameObject uiInstance = Instantiate(fishUIPrefabInstance, contentPanel); // �v���n�u���R���e���c�p�l�����ɐ���
                uiInstance.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize; // �T�C�Y��ݒ�

                // ���̐����e�L�X�g�ɔ��f
                TextMeshProUGUI fishCountText = uiInstance.transform.Find("FishCountText").GetComponent<TextMeshProUGUI>();
                fishCountText.text = "x" + fish.Value;

                fishUIInstances.Add(fish.Key, uiInstance); // ������UI�C���X�^���X��o�^
                count++;
            }
        }

        AdjustContentSize(count); // �R���e���c�p�l���̃T�C�Y�𒲐�
        AdjustSpacing(); // UI�̊Ԋu�𒲐�
    }

    /// <summary>
    /// �\�����̋���UI�����ׂč폜����B
    /// </summary>
    private void ClearUI()
    {
        foreach (var instance in fishUIInstances.Values)
        {
            Destroy(instance); // UI�C���X�^���X���폜
        }
        fishUIInstances.Clear(); // �������N���A
    }

    /// <summary>
    /// ���̐��ɉ����ăR���e���c�p�l���̃T�C�Y�𒲐�����B
    /// </summary>
    private void AdjustContentSize(int itemCount)
    {
        // �������ŏ��T�C�Y�ȏ�ɐݒ肵�A�X�N���[���̗L��������
        float newHeight = Mathf.Max(minContentSize.y, itemCount * (fishUIPanelSize.y + spacing) - spacing);
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(minContentSize.x, newHeight);
        scrollRect.vertical = newHeight > scrollRect.viewport.rect.height; // �R���e���c���\���̈�𒴂���ꍇ�ɃX�N���[����L����
    }

    /// <summary>
    /// ��UI�v�f�Ԃ̃X�y�[�V���O�𒲐�����B
    /// </summary>
    private void AdjustSpacing()
    {
        VerticalLayoutGroup layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing; // ���C�A�E�g�O���[�v�̊Ԋu��ݒ�
        }
    }
}
