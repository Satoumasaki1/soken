using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject optionsPanel;

    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);  // �p�l���̕\��/��\����؂�ւ�
    }
    public void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);  // �I�v�V�����p�l�����\���ɂ���
    }
}
