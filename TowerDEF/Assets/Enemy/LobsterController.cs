using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobsterController : MonoBehaviour, IDamageable
{
    public int health = 20;
    public float speed = 2.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 5;
    public float attackCooldown = 1.5f;

    private Transform targetAlly;
    private Transform targetBase;
    private GameObject target;
    private float attackCooldownTimer;

    private void Update()
    {
        if (targetAlly == null && targetBase == null)
        {
            FindNearestTarget();
        }

        if (target != null)
        {
            MoveTowardsTarget();
            AttackTargetIfInRange();
        }
    }

    private void FindNearestTarget()
    {
        GameObject[] allyTargets = GameObject.FindGameObjectsWithTag("Ally");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (GameObject target in allyTargets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = target;
            }
        }

        if (nearestTarget != null)
        {
            targetAlly = nearestTarget.transform;
            target = nearestTarget;
        }
        else
        {
            GameObject baseTarget = GameObject.FindGameObjectWithTag("Base");
            if (baseTarget != null)
            {
                targetBase = baseTarget.transform;
                target = baseTarget;
            }
            else
            {
                targetAlly = null;
                targetBase = null;
                target = null;
            }
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
            attackCooldownTimer = attackCooldown;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " ‚Í“|‚³‚ê‚Ü‚µ‚½");
        Destroy(gameObject);
    }
}
