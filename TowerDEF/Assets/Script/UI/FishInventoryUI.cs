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

    // ���̎�ނ��Ƃ�UI�v���n�u��ێ����邽�߂̔z��
    [SerializeField] private GameObject[] fishUIPrefabs;

    private Dictionary<GameManager.ResourceFishType, GameObject> fishUIInstances = new Dictionary<GameManager.ResourceFishType, GameObject>();

    private void Start()
    {
        UpdateFishInventoryUI(); // �V�[���J�n����UI���X�V
    }

    /// <summary>
    /// ���̃C���x���g���Ɋ�Â���UI���X�V���܂��B
    /// </summary>
    public void UpdateFishInventoryUI()
    {
        ClearUI(); // ������UI�����ׂč폜
        int count = 0; // �\�����鋛�̐����J�E���g

        // �C���x���g�����̋����m�F����UI���X�V
        foreach (var fish in GameManager.Instance.finventory)
        {
            if (fish.Value > 0) // ����0���傫���ꍇ�̂ݕ\��
            {
                // ���̎�ނɉ������v���n�u��I��
                GameObject fishUIPrefabInstance = fishUIPrefabs[(int)fish.Key];  // ���̎�ނɑΉ�����v���n�u��I��
                GameObject uiInstance = Instantiate(fishUIPrefabInstance, contentPanel);
                uiInstance.GetComponent<RectTransform>().sizeDelta = fishUIPanelSize;

                // �q�I�u�W�F�N�g�̃e�L�X�g���擾���ċ��̐���\��
                TextMeshProUGUI fishCountText = uiInstance.transform.Find("FishCountText").GetComponent<TextMeshProUGUI>();
                fishCountText.text = "x" + fish.Value;   // ���̐���\��

                fishUIInstances.Add(fish.Key, uiInstance); // �C���X�^���X�������ɓo�^
                count++;
            }
        }

        AdjustContentSize(count); // �R���e���c�p�l���̃T�C�Y�𒲐�
        AdjustSpacing();          // UI�Ԃ̃X�y�[�X��ݒ�
    }

    /// <summary>
    /// �\������UI�����ׂč폜���܂��B
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
    /// �R���e���c�p�l���̃T�C�Y�����̐��ɉ����Ē������܂��B
    /// </summary>
    private void AdjustContentSize(int itemCount)
    {
        float newHeight = Mathf.Max(minContentSize.y, itemCount * (fishUIPanelSize.y + spacing) - spacing);
        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(minContentSize.x, newHeight);
        scrollRect.vertical = newHeight > scrollRect.viewport.rect.height;
    }

    /// <summary>
    /// UI�v�f�Ԃ̃X�y�[�X��ݒ肵�܂��B
    /// </summary>
    private void AdjustSpacing()
    {
        VerticalLayoutGroup layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing; // �X�y�[�V���O��K�p
        }
    }
}
