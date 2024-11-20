using UnityEngine;

public class FishClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        // FeedManagerのインスタンスを取得して、FeedFishメソッドを呼び出し
        FeedManager feedManager = FindObjectOfType<FeedManager>();
        if (feedManager != null)
        {
            feedManager.OnFishClicked(gameObject);
        }
    }
}
