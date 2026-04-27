using System;
using RTS.EventSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseMobileUnit : MonoBehaviour, ISelectable, IMovable
    {
        [Header("组件")]
        [SerializeField] private Vector3 target;
        [SerializeField] private DecalProjector DecalProjector;

        [SerializeField] private NavMeshAgent agent;

        [Header("状态")]
        //[SerializeField] private bool isFindingPath;
        [SerializeField] private bool isSelected;

        [Header("参数")]

        public float AgentRadius => agent.radius;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            EventSystem.EventBus.Publish<UnitSpawnEvent>(new UnitSpawnEvent { unit = this });
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

        void OnEnable()
        {
            EventSystem.EventBus.Subscribe<UnitSelectEvent>(OnSelected);
            EventSystem.EventBus.Subscribe<UnitDeSelectEvent>(OndeSelected);
            //EventSystem.EventBus.Subscribe<UnitMoveToEvent>(MoveTo);
        }

        void OnDisable()
        {
            EventSystem.EventBus.UnSubscribe<UnitSelectEvent>(OnSelected);
            EventSystem.EventBus.UnSubscribe<UnitDeSelectEvent>(OndeSelected);
            //EventSystem.EventBus.UnSubscribe<UnitMoveToEvent>(MoveTo);
        }

        public void OnSelected(UnitSelectEvent evt)
        {
            Debug.Log($"Unit.OnSelected 被调用, evt.Unit: {evt.Unit}, this: {(ISelectable)this}");
            if (evt.Unit == (ISelectable)this)
            {
                Debug.Log("选中当前Unit，显示DecalProjector");
                DecalProjector?.gameObject.SetActive(true);
                isSelected = true;
                //TODO
            }
        }

        public void OndeSelected(UnitDeSelectEvent evt)
        {
            Debug.Log($"Unit.OndeSelected 被调用, evt.Unit: {evt.Unit}, this: {(ISelectable)this}");
            if (evt.Unit == (ISelectable)this)
            {
                Debug.Log("取消选中当前Unit，隐藏DecalProjector");
                DecalProjector?.gameObject.SetActive(false);
                isSelected = false;
                //TODO
            }
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
