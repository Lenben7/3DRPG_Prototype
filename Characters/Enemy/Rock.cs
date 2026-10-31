using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{

    public enum RockStates{ HitPlayer, HitEnemy, HitNothing }
    private Rigidbody rb;
    public RockStates rockStates;

    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject target;
    public GameObject breakEffect;
    private Vector3 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude < 1)
        {
            rockStates = RockStates.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        if(target == null) { target = FindObjectOfType<PlayerController>().gameObject; }

        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if(collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped=true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    
                    collision.gameObject.GetComponent<Animator>().SetTrigger("dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());

                    rockStates = RockStates.HitNothing;    
                }
                break;

            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Golem>())
                {
                    var collisionStates = collision.gameObject.GetComponent<CharacterStats>();
                    collisionStates.TakeDamage(damage, collisionStates);
                    
                    Instantiate(breakEffect, transform.position, Quaternion.identity);

                    Destroy(gameObject);
                }
                break;
        }
    }
}
