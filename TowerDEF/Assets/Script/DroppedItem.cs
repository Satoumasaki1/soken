using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public string itemName = "Item";  // ���肷��A�C�e���̖��O�i�f�o�b�O�p�j

    void OnMouseDown()
    {
        // �A�C�e�������i�f�o�b�O�p�̃��O�\���j
        Debug.Log($"{itemName}����肵�܂����I");

        // �A�C�e�����V�[������폜
        Destroy(gameObject);
    }
}
