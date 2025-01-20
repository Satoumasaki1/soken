using UnityEngine;
using UnityEngine.UI;

public class ToggleText : MonoBehaviour
{
    public GameObject textObject;  // 表示非表示を切り替えたいTextオブジェクトを指定


    // 一時停止ボタンがクリックされた時の処理

    public void PButton()
    {
        if(textObject != null)
        {
            textObject.SetActive(true);// テキストを表示
        }
    }

    // 再生ボタンがクリックされた時の処理
    public void PlayButton()
    {
        if (textObject != null)
        {
            textObject.SetActive(false);  // テキストを非表示
        }
    }
}
