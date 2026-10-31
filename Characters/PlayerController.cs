using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;
    private GameObject attackTarget;

    private float lastAttackTime;
    private float stopDistance;
    private bool isDead;

    public float attackAbleForce = 20;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }
    void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    void Update()
    {
        isDead = characterStats.currentHealth == 0;

        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
        }

        SwitchAnimation();

        lastAttackTime -= Time.fixedDeltaTime;
    }

    void OnDisable()
    {
        if(!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    private void SwitchAnimation() 
    {
        anim.SetFloat("speed", agent.velocity.sqrMagnitude);
        anim.SetBool("death", isDead);
    }

    public void MoveToTarget(Vector3 target) 
    {
        StopAllCoroutines();

        if(isDead) return;

        agent.stoppingDistance = stopDistance;

        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if(isDead) return;

        if(target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {   
        agent.isStopped = false;
       
        agent.stoppingDistance = characterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        // Chase
        while(Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;

        // Attack
        if(lastAttackTime < 0)
        {   
            anim.SetBool("critical", characterStats.isCritical);
            anim.SetTrigger("attack");

            // CD
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    //Animation Event;
    void Hit()
    {
        if(attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;

                // 防止石头因自身代码不销毁
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;

                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * attackAbleForce, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
