using System;
using RTS.EventSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseMobileUnit : BaseCommandable, IMovable
    {
        [SerializeField] private Vector3 target;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        protected override void Start()
        {
            base.Start();
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            if (target != null && agent.hasPath)
            {
                CheckIfArriveDes(target, this.transform.position);
                // HandleBrake();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }


        public void MoveTo(Vector3 Pos)
        {
            //if (!isSelected)
            //    return;
            agent.ResetPath();
            agent.SetDestination(Pos);
            target = Pos;
            //isFindingPath = true;
        }

        public void CheckIfArriveDes(Vector3 target, Vector3 unit)
        {
            if (Vector3.Distance(target, unit) < agent.stoppingDistance)
            {
                //isFindingPath = false;
                agent.ResetPath();
                Debug.Log("寻路已完成，寻路目标置为空");

            }
        }

        // private void HandleBrake()
        // {
        //     if (agent.hasPath && !agent.pathPending)
        //     {
        //         // 判断是否已经处于大部队的聚拢范围内
        //         if (agent.remainingDistance < agent.stoppingDistance*2)
        //         {
        //             // 检查自己是不是走不动了（速度极慢，说明撞到了前面的队友）
        //             if (agent.velocity.sqrMagnitude < 0.3f)
        //             {
        //                 // 智能刹车：既然我前面有人挡着，我也算是到达集合点了，停止移动！
        //                 agent.ResetPath();
        //             }
        //         }
        //     }
        // }

    }
}
