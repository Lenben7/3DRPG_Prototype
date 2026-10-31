using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates{ GUARD, PATROL, CHASE, DEAD }
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
   private EnemyStates enemyStates;
   private NavMeshAgent agent;
   private Animator anim;
   private Collider _collider;
   protected CharacterStats characterStats;

   [Header("Basic Settings")]
   public float sightRadius;
   public bool isGuard;
   private float speed;
   protected GameObject attackTarget;
   public float lookAtTime;
   private float remainLookAtTime;
   private float lastAttackTime;
   private Quaternion guardRotation;

   [Header("Patrol State")]
   public float patrolRange;
   private Vector3 wayPoint;
   private Vector3 guardPos;

   // bool动画触发
   bool isWalk;
   bool isChase;
   bool isFollow;
   bool isDead;
   bool playerDead;

   void Awake()
   {
      agent = GetComponent<NavMeshAgent>();
      anim = GetComponent<Animator>();
      characterStats = GetComponent<CharacterStats>();
      _collider = GetComponent<Collider>();

      speed = agent.speed;
      guardPos = transform.position;
      guardRotation = transform.rotation;
      remainLookAtTime = lookAtTime;
   }

   void OnEnable()
   {
      GameManager.Instance.AddObserver(this);
   }

   void Start()
   {
      if (isGuard)
      {
         enemyStates = EnemyStates.GUARD;
      }
      else
      {
         enemyStates = EnemyStates.PATROL;
         GetNewWayPoint();
      } 
   }

    void Update()
   {
      if(characterStats.currentHealth == 0)
      {
         isDead = true;
      }

      if (!playerDead)
      {
         SwitchStates();
         SwitchAnimation();
         lastAttackTime -= Time.deltaTime;
      }
   }

   void OnDisable()
   {
      if(!GameManager.IsInitialized) return;
      
      GameManager.Instance.RemoveObserver(this);
   }

    void SwitchAnimation()
   {
      anim.SetBool("walk", isWalk);
      anim.SetBool("chase", isChase);
      anim.SetBool("follow", isFollow);
      anim.SetBool("critical", characterStats.isCritical);
      anim.SetBool("death", isDead);
   }

   void SwitchStates()
   {
      if (isDead)
      {
         enemyStates = EnemyStates.DEAD;
      }
      else if (FoundPlayer()) // 发现Player 切换CHASE
      {
         enemyStates = EnemyStates.CHASE;
      }

      switch (enemyStates)
      {
         case EnemyStates.GUARD:
            isChase = false;
            
            if(transform.position != guardPos)
            {
               isWalk = true;
               agent.isStopped = false;
               agent.destination = guardPos;

               if(Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
               {
                  isWalk = false;
                  transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
               }
            }
            break;

         case EnemyStates.PATROL:
            isChase = false;
            agent.speed = speed * 0.5f;

            // 是否抵达巡逻点
            if(Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
            {
               isWalk = false;
               if(remainLookAtTime > 0)
               {
                  remainLookAtTime -= Time.deltaTime;
               }
               else
               {
                  GetNewWayPoint();
               }
            }
            else
            {
               isWalk = true;
               agent.destination = wayPoint;
            }
            break;

         case EnemyStates.CHASE:
            isWalk = false;
            isChase = true;

            agent.speed = speed;

            if (!FoundPlayer())
            {
               isFollow = false;
               if(remainLookAtTime > 0)
               {
                  agent.destination = transform.position;
                  remainLookAtTime -= Time.deltaTime;
               }
               else
               {
                  if (isGuard)
                  {
                     enemyStates = EnemyStates.GUARD;
                  }
                  else
                  {
                     enemyStates = EnemyStates.PATROL;
                  }
               }
            }
            else
            {
               isFollow = true;
               agent.isStopped = false;
               agent.destination = attackTarget.transform.position;
            }

            if(TargetInAttackRange() || TargetInSkillRange())
            {
               isFollow = false;
               agent.isStopped = true;
               
               if(lastAttackTime < 0)
               {
                  lastAttackTime = characterStats.attackData.coolDown;

                  // 暴击伤害
                  characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                  // 执行攻击
                  Attack();
               }
            }
            break;

         case EnemyStates.DEAD:
            this.gameObject.tag = "Untagged";
            _collider.enabled = false;
            //agent.enabled = false;
            agent.radius = 0;
            Destroy(gameObject, 2f);
            break;
      }
   }
   void Attack()
   {
      transform.LookAt(attackTarget.transform);
      if (TargetInAttackRange())
      {
         anim.SetTrigger("attack");
      }
      if (TargetInSkillRange())
      {
         anim.SetTrigger("skill");
      }
   }

   bool FoundPlayer()
   {
      var colliders = Physics.OverlapSphere(transform.position, sightRadius);

      foreach (var target in colliders)
      {
         if (target.CompareTag("Player"))
         {
            attackTarget = target.gameObject;
            return true;
         }
      }

      attackTarget = null;

      return false;
   }

   bool TargetInAttackRange()
   {
      if(attackTarget != null)
      {
         return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
      }
      else
      {
         return false;
      }
   }

   bool TargetInSkillRange()
   {
      if(attackTarget != null)
      {
         return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
      }
      else
      {
         return false;
      }
   }

   void GetNewWayPoint()
   {
      remainLookAtTime = lookAtTime;

      float randomX = Random.Range(-patrolRange, patrolRange);
      float randomZ = Random.Range(-patrolRange, patrolRange);

      Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
      
      NavMeshHit hit;
      wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
   }

   //Animation Event;
   void Hit()
   {
      if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
      {
         var targetStats = attackTarget.GetComponent<CharacterStats>();
         targetStats.TakeDamage(characterStats, targetStats);
      }
   }

   public void EndNotify()
   {
      anim.SetBool("win", true);
      playerDead = true;
      isChase = false;
      isWalk = false;
      attackTarget = null;
   }

   void OnDrawGizmosSelected()
   {
      Gizmos.color = Color.green;
      Gizmos.DrawWireSphere(transform.position, sightRadius);

      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, patrolRange);
   }

}