using System.Collections.Generic;
using UnityEngine;

public class EsaSpawner : MonoBehaviour
{
    public GameObject esa1Prefab; // OkiaMi�pPrefab
    public GameObject esa2Prefab; // Benthos�pPrefab
    public GameObject esa3Prefab; // Plankton�pPrefab

    public Transform planeArea; // �a��u���v���[���̗̈�
    public int maxEsaCount = 4; // �a�̍ő�ݒu��
    private int currentEsaCount = 0; // ���ݐݒu����Ă���a�̐�

    private bool isEsaPlacingMode = false; // �a��u�����[�h�̃g�O��

    private GameManager gameManager; // GameManager�̃C���X�^���X�Q��

    void Start()
    {
        // GameManager�̃C���X�^���X���擾
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        // �a��u�����[�h���I���̏ꍇ�ɃN���b�N���������s
        if (isEsaPlacingMode && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == planeArea)
            {
                if (gameManager.SelectedFeedType == null)
                {
                    Debug.Log("�a���I������Ă��܂���");
                    return;
                }

                GameManager.ResourceType selectedFeedType = gameManager.SelectedFeedType.Value;

                // �݌Ɋm�F
                if (!gameManager.inventory.ContainsKey(selectedFeedType) || gameManager.inventory[selectedFeedType] <= 0)
                {
                    Debug.Log($"�݌ɂ��s�����Ă��܂�: {selectedFeedType}");
                    return;
                }

                // �a�̐ݒu
                if (currentEsaCount < maxEsaCount)
                {
                    Vector3 position = GetRandomPositionInPlane(planeArea);
                    SpawnSelectedEsa(position, selectedFeedType);
                    gameManager.UpdateResourceUI();
                }
                else
                {
                    Debug.Log("���ɍő吔�̉a���ݒu����Ă��܂�");
                }
            }
        }
    }

    // �a��u�����[�h���g�O������֐�
    public void ToggleEsaPlacingMode()
    {
        isEsaPlacingMode = !isEsaPlacingMode;
        Debug.Log("�a��u�����[�h: " + (isEsaPlacingMode ? "ON" : "OFF"));
    }

    // �v���[����̃����_���Ȉʒu���v�Z����֐�
    Vector3 GetRandomPositionInPlane(Transform plane)
    {
        Vector3 center = plane.position;
        float width = plane.localScale.x * 5f;  // �v���[���̕�
        float length = plane.localScale.z * 5f; // �v���[���̒���

        float x = center.x + Random.Range(-width / 2f, width / 2f);
        float z = center.z + Random.Range(-length / 2f, length / 2f);

        float y = center.y; // �v���[���̍������Œ�

        return new Vector3(x, y, z);
    }

    // �I�����ꂽ�a�𐶐�����֐�
    void SpawnSelectedEsa(Vector3 position, GameManager.ResourceType selectedFeedType)
    {
        GameObject esaPrefab = null;

        // �I�����ꂽ�a�^�C�v�ɉ�����Prefab��ݒ�
        switch (selectedFeedType)
        {
            case GameManager.ResourceType.OkiaMi:
                esaPrefab = esa1Prefab;
                break;
            case GameManager.ResourceType.Benthos:
                esaPrefab = esa2Prefab;
                break;
            case GameManager.ResourceType.Plankton:
                esaPrefab = esa3Prefab;
                break;
        }

        if (esaPrefab != null)
        {
            // �a�𐶐�
            Instantiate(esaPrefab, position, Quaternion.identity);
            currentEsaCount++;

            // �݌ɂ�����
            gameManager.inventory[selectedFeedType]--;
            Debug.Log($"{selectedFeedType} ��ݒu���܂����B�݌�: {gameManager.inventory[selectedFeedType]}");
        }
        else
        {
            Debug.Log("�����ȉa�^�C�v���I������܂���");
        }
    }
}
