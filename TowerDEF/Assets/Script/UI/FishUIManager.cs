using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishUIManager : MonoBehaviour
{

    // �a�̍݌ɂ��m�F���邽�߂�GameManager�̃C���X�^���X���Q��
    private GameManager gameManager;

    // UI�{�^���̃��X�g�i�a�̃{�^���j
    [SerializeField] private Button[] fishButtons;

    // Start is called before the first frame update
    void Start()
    {
        // GameManager�̃C���X�^���X���擾
        gameManager = GameManager.Instance;

        // �{�^���ɃN���b�N���X�i�[��ǉ�
        fishButtons[0].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Kani));
        fishButtons[1].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Tyoutyou));
        fishButtons[2].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Kaisou));
        fishButtons[3].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Syako));
        fishButtons[4].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Koban));
        fishButtons[5].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Teppou));
        fishButtons[6].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Manta));
        fishButtons[7].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Uni));
        // �ŏ��ɍ݌ɂ̍X�V���s��
        UpdateButtonStates();
    }

    // �a�̎�ނ�I�����AGameManager�ɒʒm���郁�\�b�h
    public void SetFishType(GameManager.ResourceFishType fishType)
    {
        // GameManager�ɑI�΂ꂽ�a�^�C�v��ʒm����
        GameManager.Instance.SetSelectedFishType(fishType);
        Debug.Log("�I�����ꂽ�a: " + fishType);
    }

    // �a�̃{�^���̏�ԁi�L��/�����j���݌ɂɊ�Â��čX�V
    private void UpdateButtonStates()
    {
        for (int i = 0; i < fishButtons.Length; i++)
        {
            GameManager.ResourceFishType fishType = (GameManager.ResourceFishType)i;
            fishButtons[i].interactable = gameManager.finventory[fishType] > 0; // inventory���g�p���ă{�^���̗L��/������ݒ�
        }
    }

}
