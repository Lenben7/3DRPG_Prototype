using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 30;
    public GameObject rockPrefab;
    public Transform handPos;

    // Animation Event;
    public void KickOff()
    {
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 hitDirection = (attackTarget.transform.position - transform.position).normalized;
            
            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = hitDirection * kickForce;
            targetStats.GetComponent<Animator>().SetTrigger("dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    // Animation Event;
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
