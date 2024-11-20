using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject optionsPanel;

    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);  // パネルの表示/非表示を切り替え
    }
    public void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);  // オプションパネルを非表示にする
    }
}
