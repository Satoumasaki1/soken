using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // �����̂��߂ɒǉ�

public class FeedUIManager : MonoBehaviour
{
    // UI�e�L�X�g�i�a�̎�ނ�\���j
    [SerializeField]
    public TextMeshProUGUI feedTypeText;

    // �a�̍݌ɂ��m�F���邽�߂�GameManager�̃C���X�^���X���Q��
    private GameManager gameManager;

    // UI�{�^���̃��X�g�i�a�̃{�^���j
    [SerializeField] private Button[] feedButtons;

    // �a�̖��O����{��ŕ\�����邽�߂̎���
    private Dictionary<GameManager.ResourceType, string> feedTypeNames = new Dictionary<GameManager.ResourceType, string>
    {
        { GameManager.ResourceType.OkiaMi, "�I�L�A�~" },
        { GameManager.ResourceType.Benthos, "�x���g�X" },
        { GameManager.ResourceType.Plankton, "�v�����N�g��" },
        { GameManager.ResourceType.FeedA, "�R�}�Z" },
        { GameManager.ResourceType.FeedB, "�~�b�N�X�y���b�g" },
        { GameManager.ResourceType.FeedC, "�o�C�I�t�B�[�h" }
    };

    private void Start()
    {
        // GameManager�̃C���X�^���X���擾
        gameManager = GameManager.Instance;

        // �{�^���ɃN���b�N���X�i�[��ǉ�
        feedButtons[0].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.OkiaMi));
        feedButtons[1].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.Benthos));
        feedButtons[2].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.Plankton));
        feedButtons[3].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.FeedA));
        feedButtons[4].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.FeedB));
        feedButtons[5].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.FeedC));

        // �ŏ��ɍ݌ɂ̍X�V���s��
        //UpdateButtonStates();
        //�ŏ��Ɏ����Ă���a�̏���GameManager�ɒʒm����
        SetFeedType(GameManager.ResourceType.OkiaMi);
    }

    // �a�̎�ނ�I�����AGameManager�ɒʒm���郁�\�b�h
    public void SetFeedType(GameManager.ResourceType feedType)
    {
        // GameManager�ɑI�΂ꂽ�a�^�C�v��ʒm����
        GameManager.Instance.SetSelectedFeedType(feedType);
        Debug.Log("�I�����ꂽ�a: " + feedType);
        UpdateFeedTypeUI(); // �a�^�C�v���ύX���ꂽ�ۂ�UI���X�V
    }

    // UI�\�����X�V���郁�\�b�h
    public void UpdateFeedTypeUI()
    {
        if (GameManager.Instance.SelectedFeedType.HasValue)
        {
            GameManager.ResourceType selectedFeed = GameManager.Instance.SelectedFeedType.Value;

            // ���{��̉a�����擾���ĕ\��
            if (feedTypeNames.TryGetValue(selectedFeed, out string feedName))
            {
                feedTypeText.text = feedName;
            }
            else
            {
                feedTypeText.text = "�s���ȉa"; // �����ɂȂ��ꍇ
            }
        }
        else
        {
            feedTypeText.text = "�a���I������Ă��܂���";
        }
    }

    // �a�̃{�^���̏�ԁi�L��/�����j���݌ɂɊ�Â��čX�V
    public void UpdateButtonStates()
    {
        for (int i = 0; i < feedButtons.Length; i++)
        {
            GameManager.ResourceType feedType = (GameManager.ResourceType)i;
            feedButtons[i].interactable = gameManager.inventory[feedType] > 0; // inventory���g�p���ă{�^���̗L��/������ݒ�
        }
    }
}
