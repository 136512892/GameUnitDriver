using UnityEngine;
using UnityEngine.AI;

public class EnemyUnitSample : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    //巡逻点集合
    [SerializeField] private Transform[] patrolPoints;
    //到达一个巡逻点后的休息时长
    [SerializeField] private float restTime = 3f;
    //警戒范围
    [SerializeField] private float guardingRange = 3f;
    //攻击范围
    [SerializeField] private float attackRange = 1.2f;

    //当前巡逻点的索引值
    private int index;
    //休息计时器
    private float restTimer;
    //攻击计时器
    private float attackTimer;
    //状态机
    private StateMachine machine;

    private void Start()
    {
        machine = new StateMachine()
            .Build<State>("巡逻状态")
                .OnEnter(s =>
                {
                    //进入巡逻状态 设置第一个巡逻点并开始巡逻
                    agent.isStopped = false;
                    agent.stoppingDistance = 0f;
                    agent.speed = 1f;
                    index = 0;
                    agent.SetDestination(
                        patrolPoints[index].position);
                    animator.SetBool("Move", true);
                })
                .OnStay(s =>
                {
                    //到达指定巡逻点
                    if (Vector3.Distance(transform.position,
                        patrolPoints[index].position) <= .1f)
                    {
                        animator.SetBool("Move", false);
                        restTimer += Time.deltaTime;
                        //休息指定时间 随后找到下一个巡逻点进行巡逻
                        if (restTimer >= restTime)
                        {
                            restTimer = 0f;
                            index++;
                            index %= patrolPoints.Length;
                            agent.SetDestination(
                                patrolPoints[index].position);
                            animator.SetBool("Move", true);
                        }
                    }
                })
                .OnExit(s =>
                {
                    agent.isStopped = true;
                    animator.SetBool("Move", false);
                })
                //假设警戒范围为3米 当与Player的距离小于3时 进入追击状态
                .SwitchWhen(() => Vector3.Distance(
                    transform.position, player.position) <= guardingRange,
                    "追击状态")
            .Complete();
        machine
            .Build<State>("追击状态")
                .OnEnter(s =>
                {
                    agent.isStopped = false;
                    agent.stoppingDistance = 0.8f;
                    //追击时的移动速度略高于巡逻时的移动速度
                    agent.speed = 1.5f;
                    animator.SetBool("Move", true);
                })
                .OnStay(s =>
                {
                    //追击过程是向Player位置寻路的过程，目的是到Player前进行攻击
                    //因此未到达Player位置前时不停的进行寻路，到达后切换至攻击状态
                    if (Vector3.Distance(
                            transform.position, player.position) > attackRange)
                        agent.SetDestination(player.position);
                    else
                        s.machine.Switch("攻击状态");
                })
                .OnExit(s =>
                {
                    animator.SetBool("Move", false);
                })
                //当Player离开警戒范围后 回到巡逻状态
                .SwitchWhen(() => Vector3.Distance(
                    transform.position, player.position) > guardingRange,
                    "巡逻状态")
            .Complete();
        machine
            .Build<State>("攻击状态")
                .OnEnter(s =>
                {
                    agent.isStopped = true;
                })
                .OnStay(s =>
                {
                    //朝向Player
                    transform.rotation = Quaternion.LookRotation(
                        player.position - transform.position);
                    if (attackTimer >= 0f)
                        attackTimer -= Time.deltaTime;
                    else
                    {
                        if (attackTimer <= 0f)
                        {
                            Debug.Log("攻击"); //TODO：攻击
                            attackTimer = 2f;
                        }
                    }
                })
                .SwitchWhen(() => Vector3.Distance(
                    transform.position, player.position) > attackRange, 
                    "追击状态")
            .Complete();

        //进入第一个状态
        machine.Switch2Next();
    }

    private void Update()
    {
        machine.OnUpdate();   
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, transform.right, 360f, guardingRange);
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, transform.right, 360f, attackRange);
    }
#endif
}

