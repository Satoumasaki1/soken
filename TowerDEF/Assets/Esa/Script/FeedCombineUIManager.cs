using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedCombineUIManager : MonoBehaviour
{
    [SerializeField] private GameObject popupMenu;           // �����|�b�v�A�b�v���j���[
    [SerializeField] private Button feedAButton;             // �R�}�Z�{�^��
    [SerializeField] private Button feedBButton;             // �~�b�N�X�y���b�g�{�^��
    [SerializeField] private Button feedCButton;             // �o�C�I�t�B�[�h�{�^��
    [SerializeField] private TextMeshProUGUI resultText;     // �������ʂ�\������UI
    [SerializeField] private Button closeButton;             // �|�b�v�A�b�v�����{�^��

    public GameManager gm;
    private GameManager.ResourceType selectedFeedType;       // �I�����ꂽ�����a�̃^�C�v

    private void Start()
    {
        // �{�^���ɃN���b�N�C�x���g��ݒ�
        feedAButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedA));
        feedBButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedB));
        feedCButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedC));
        closeButton.onClick.AddListener(HidePopup);

        gm = GameManager.Instance;

        // �|�b�v�A�b�v���ŏ��͔�\���ɂ���
        popupMenu.SetActive(false);
    }

    // �����a��I�����郁�\�b�h
    private void SelectFeedToCreate(GameManager.ResourceType feedType)
    {
        selectedFeedType = feedType;
        CreateFeed(selectedFeedType);
    }

    // �����a���쐬���郁�\�b�h
    private void CreateFeed(GameManager.ResourceType feedType)
    {
        var inventory = GameManager.Instance.inventory;

        switch (feedType)
        {
            case GameManager.ResourceType.FeedA:
                if (inventory[GameManager.ResourceType.OkiaMi] > 0 && inventory[GameManager.ResourceType.Plankton] > 0)
                {
                    inventory[GameManager.ResourceType.OkiaMi]--;
                    inventory[GameManager.ResourceType.Plankton]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "��������: �R�}�Z���쐬����܂����I";
                }
                else
                {
                    resultText.text = "�������s: �݌ɂ�����܂���";
                }
                break;

            case GameManager.ResourceType.FeedB:
                if (inventory[GameManager.ResourceType.OkiaMi] > 0 && inventory[GameManager.ResourceType.Benthos] > 0)
                {
                    inventory[GameManager.ResourceType.OkiaMi]--;
                    inventory[GameManager.ResourceType.Benthos]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "��������: �~�b�N�X�y���b�g���쐬����܂����I";
                }
                else
                {
                    resultText.text = "�������s: �݌ɂ�����܂���";
                }
                break;

            case GameManager.ResourceType.FeedC:
                if (inventory[GameManager.ResourceType.Plankton] > 0 && inventory[GameManager.ResourceType.Benthos] > 0)
                {
                    inventory[GameManager.ResourceType.Plankton]--;
                    inventory[GameManager.ResourceType.Benthos]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "��������: �o�C�I�t�B�[�h���쐬����܂����I";
                }
                else
                {
                    resultText.text = "�������s: �݌ɂ�����܂���";
                }
                break;

            default:
                resultText.text = "�����ȉa�^�C�v";
                break;
        }
    }

    // �|�b�v�A�b�v��\�����郁�\�b�h
    public void ShowPopup()
    {
        // �Q�[�����ꎞ��~
        //GameManager.Instance.PauseGame();

        popupMenu.SetActive(true);
        resultText.text = "��肽���a��I��ł�������";
    }

    // �|�b�v�A�b�v����郁�\�b�h
    public void HidePopup()
    {
        // �Q�[���̈ꎞ��~������
        //GameManager.Instance.ResumeGame();

        popupMenu.SetActive(false);
    }
}
