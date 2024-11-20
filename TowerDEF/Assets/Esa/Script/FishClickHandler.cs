using UnityEngine;

public class FishClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        // FeedManager�̃C���X�^���X���擾���āAFeedFish���\�b�h���Ăяo��
        FeedManager feedManager = FindObjectOfType<FeedManager>();
        if (feedManager != null)
        {
            feedManager.OnFishClicked(gameObject);
        }
    }
}
