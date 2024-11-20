using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    // This method will be implemented by any class that can take damage.
    void TakeDamage(int damageAmount);

    // Optionally, you can add more methods to handle other aspects of damageable entities.
    // For example:
    // void Heal(int healAmount);
    // void Die();
}
