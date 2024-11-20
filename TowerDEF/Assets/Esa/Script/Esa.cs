using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Esa : MonoBehaviour
{
    // 魚など変換後のオブジェクト
    public GameObject transformedObject;

    private void OnEnable()
    {
        // GameManagerのWaveStartedイベントをリッスンする
        GameManager.WaveStarted += OnWaveStart;
    }

    private void OnDisable()
    {
        // リスナーを解除（不要なイベントを聞かないようにする）
        GameManager.WaveStarted -= OnWaveStart;
    }

    // ウェーブが始まった時に呼ばれる関数
    private void OnWaveStart()
    {
        // ウェーブが始まったら餌を別のオブジェクトに変換
        TransformToFish();
    }

    // 餌を魚に変換する処理
    private void TransformToFish()
    {
        // 変換後のオブジェクトが指定されている場合
        if (transformedObject != null)
        {

            Debug.Log("現在の位置: " + transform.position);
            Debug.Log("現在の回転: " + transform.rotation);

            // 現在の位置と回転を維持して変換後のオブジェクトを生成
            Instantiate(transformedObject, transform.position, transform.rotation);

            // 元の餌オブジェクトを削除
            Destroy(gameObject);
        }
    }
}
