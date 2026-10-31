using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce = 15;

    public void KickOff()
    {
        if(attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);

            Vector3 hitDirection = attackTarget.transform.position-transform.position;
            hitDirection.Normalize();

            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = hitDirection * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("dizzy");
        }   
    }
}
