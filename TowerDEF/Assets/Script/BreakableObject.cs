using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public int maxDurability = 15;          // �ő�ϋv�l
    private int currentDurability;          // ���݂̑ϋv�l
    public GameObject materialPrefab;       // �h���b�v����f�ނ�Prefab
    public Transform dropPosition;          // �h���b�v�ʒu
    public float dropRadius = 0.5f;         // �h���b�v����A�C�e���͈̔�

    // IsBroken�v���p�e�B��ǉ�
    public bool IsBroken
    {
        get { return currentDurability <= 0; } // �ϋv�l��0�ȉ��Ȃ�j��ς�
    }

    void Start()
    {
        currentDurability = maxDurability; // ���݂̑ϋv�l��������
    }

    public void TakeDamage()
    {
        if (currentDurability > 0)
        {
            currentDurability--;

            // �f�ނ��h���b�v
            DropMaterial();

            // �ϋv�l��0�ɂȂ�����I�u�W�F�N�g��j��
            if (currentDurability <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void DropMaterial()
    {
        if (materialPrefab != null)
        {
            // �h���b�v�ʒu���������炵�Ĕz�u
            Vector3 spawnPosition = dropPosition != null ? dropPosition.position : transform.position;
            spawnPosition += new Vector3(Random.Range(-dropRadius, dropRadius), 0, Random.Range(-dropRadius, dropRadius));

            // �h���b�v�A�C�e���̐���
            Instantiate(materialPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
