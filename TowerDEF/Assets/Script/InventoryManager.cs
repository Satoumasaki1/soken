using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private List<GameObject> inventory = new List<GameObject>();

    public void AddItem(GameObject item)
    {
        inventory.Add(item);
        Debug.Log(item.name + "���C���x���g���ɒǉ����܂����I");
    }

    // �C���x���g���̓��e��\������i�f�o�b�O�p�j
    public void ShowInventory()
    {
        foreach (var item in inventory)
        {
            Debug.Log(item.name);
        }
    }
}
