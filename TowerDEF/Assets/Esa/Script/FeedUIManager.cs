using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedUIManager : MonoBehaviour
{
    // UI�e�L�X�g�܂���Image�R���|�[�l���g�i�a�̎�ނ�\���j
    [SerializeField]
    public TextMeshProUGUI feedTypeText;

    // �a�̍݌ɂ��m�F���邽�߂�GameManager�̃C���X�^���X���Q��
    private GameManager gameManager;

    // UI�{�^���̃��X�g�i�a�̃{�^���j
    [SerializeField] private Button[] feedButtons;

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
        UpdateButtonStates();
    }

    // �a�̎�ނ�I�����AGameManager�ɒʒm���郁�\�b�h
    public void SetFeedType(GameManager.ResourceType feedType)
    {
        // GameManager�ɑI�΂ꂽ�a�^�C�v��ʒm����
        GameManager.Instance.SetSelectedFeedType(feedType);
        Debug.Log("�I�����ꂽ�a: " + feedType);
        UpdateFeedTypeUI(); // �a�^�C�v���ύX���ꂽ�ۂ�UI���X�V
    }

    // �a�̎�ނ�ێ����郁�\�b�h�i�{�^������Ăяo�����j
    /*public void SelectFeed(int feedTypeIndex)
    {
        GameManager.ResourceType selectedFeed = (GameManager.ResourceType)feedTypeIndex;

        // �݌ɂ�����ꍇ�̂݉a��ێ�
        if (gameManager.inventory[selectedFeed] > 0) // inventory���g�p
        {
            GameManager.Instance.SelectedFeedType = selectedFeed; // GameManager��SelectedFeedType���X�V
            UpdateFeedTypeUI();
        }
        else
        {
            Debug.Log($"{selectedFeed} �̍݌ɂ�����܂���");
        }
    }*/

    // UI�\�����X�V���郁�\�b�h
    private void UpdateFeedTypeUI()
    {
        // GameManager��SelectedFeedType���Q�Ƃ���UI���X�V
        feedTypeText.text = GameManager.Instance.SelectedFeedType.HasValue
            ? GameManager.Instance.SelectedFeedType.Value.ToString() // �I�����ꂽ�a�̖��O��\��
            : "No Feed Selected";
    }

    // �a�̃{�^���̏�ԁi�L��/�����j���݌ɂɊ�Â��čX�V
    private void UpdateButtonStates()
    {
        for (int i = 0; i < feedButtons.Length; i++)
        {
            GameManager.ResourceType feedType = (GameManager.ResourceType)i;
            feedButtons[i].interactable = gameManager.inventory[feedType] > 0; // inventory���g�p���ă{�^���̗L��/������ݒ�
        }
    }

    // �����N���b�N�����Ƃ��ɌĂяo����郁�\�b�h
    /*public void TryFeedFish(Fish fish)
    {
        if (SelectedFeedType.HasValue && gameManager.inventory[(int)SelectedFeedType.Value] > 0)
        {
            gameManager.GiveFoodToFish(fish, SelectedFeedType.Value); // ���ɉa��^����
            UpdateButtonStates(); // �݌ɂ��������̂Ń{�^�����X�V
            SelectedFeedType = null; // �a�����Z�b�g
            UpdateFeedTypeUI();
        }
    }*/
}
