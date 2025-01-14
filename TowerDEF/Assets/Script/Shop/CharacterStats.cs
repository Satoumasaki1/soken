using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int health = 10;
    public int attackPower = 5;

    // ��������
    public void Upgrade()
    {
        health *= 2; // �̗͂�2�{��
        attackPower *= 2; // �U���͂�2�{��
        Debug.Log($"{gameObject.name} has been upgraded! Health: {health}, Attack: {attackPower}");
    }
}
