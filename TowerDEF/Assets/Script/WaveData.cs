using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveData", order = 1)]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemyInfo
    {
        public GameObject enemyPrefab; // �X�|�[��������G�̃v���n�u
        public int count; // �X�|�[������G�̐�
    }

    public EnemyInfo[] enemies; // �E�F�[�u���Ƃ̓G���̔z��
}

// ���̃X�N���v�^�u���I�u�W�F�N�g���g�p���āA�e�E�F�[�u�ŃX�|�[�����������G��ݒ�ł��܂��B
// WaveData���v���W�F�N�g���ō쐬���A�e�E�F�[�u�ɑ΂��ĈقȂ�G�̎�ނ␔���w��ł��܂��B
