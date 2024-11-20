using UnityEngine;
using UnityEngine.UI;

public class ToggleText : MonoBehaviour
{
    public GameObject textObject;  // 表示非表示を切り替えたいTextオブジェクトを指定

    public void ToggleVisibility()
    {
        if (textObject != null)
        {
            textObject.SetActive(!textObject.activeSelf);  // 現在の状態を反転
        }
    }
}
