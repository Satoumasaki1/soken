using TMPro;  // TextMeshProを使用するための名前空間
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text okiaMiText;  // OkiaMiの在庫表示用
    [SerializeField] private TMP_Text benthosText;  // Benthosの在庫表示用
    [SerializeField] private TMP_Text planktonText;  // Planktonの在庫表示用

    void Update()
    {
        UpdateResourceDisplay();  // リソースの表示を更新
    }

    void UpdateResourceDisplay()
    {
        // GameManagerからリソースの在庫を取得し、TextMeshProに表示
        //okiaMiText.text = "OkiaMi: " + GameManager.Instance.resourceSlots[(int)GameManager.ResourceType.OkiaMi];
        //benthosText.text = "Benthos: " + GameManager.Instance.resourceSlots[(int)GameManager.ResourceType.Benthos];
        //planktonText.text = "Plankton: " + GameManager.Instance.resourceSlots[(int)GameManager.ResourceType.Plankton];
    }
}
