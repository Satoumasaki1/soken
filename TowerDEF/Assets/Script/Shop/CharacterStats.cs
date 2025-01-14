using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int health = 10;
    public int attackPower = 5;

    // ‹­‰»ˆ—
    public void Upgrade()
    {
        health *= 2; // ‘Ì—Í‚ğ2”{‚É
        attackPower *= 2; // UŒ‚—Í‚ğ2”{‚É
        Debug.Log($"{gameObject.name} has been upgraded! Health: {health}, Attack: {attackPower}");
    }
}
