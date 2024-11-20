using UnityEngine;
using UnityEngine.UI;

public class BackGroundMover : MonoBehaviour
{
    private const float k_maxLength = 1f;
    private const string k_propName = "_MainTex"; // "_MainTex" は通常のテクスチャオフセットのプロパティ名です

    [SerializeField]
    private Vector2 m_offsetSpeed = new Vector2(0.1f, 0.1f); // テクスチャのオフセットスピード

    private Material m_material;

    private void Start()
    {
        // 背景のImageコンポーネントを取得し、マテリアルをセット
        Image imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            // インスタンス化されたマテリアルを取得
            m_material = imageComponent.material;
        }
        else
        {
            Debug.LogWarning("Imageコンポーネントが見つかりません。");
        }
    }

    private void Update()
    {
        if (m_material != null)
        {
            // xとyの値が0 ～ 1でリピートするようにする
            float x = Mathf.Repeat(Time.time * m_offsetSpeed.x, k_maxLength);
            float y = Mathf.Repeat(Time.time * m_offsetSpeed.y, k_maxLength);
            Vector2 offset = new Vector2(x, y);

            // テクスチャオフセットを設定
            m_material.SetTextureOffset(k_propName, offset);
        }
    }

    private void OnDestroy()
    {
        // ゲーム終了時にマテリアルのOffsetを元に戻す
        if (m_material != null)
        {
            m_material.SetTextureOffset(k_propName, Vector2.zero);
        }
    }
}
