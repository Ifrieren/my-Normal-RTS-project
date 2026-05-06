using System;
using System.Windows.Input;
using RTS.Commands;
using RTS.EventSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace RTS.Units
{
    public class BaseCommandable : MonoBehaviour, ISelectable
    {
        [Header("组件")]
        [SerializeField] protected DecalProjector DecalProjector;

        [Header("状态")]
        [SerializeField] protected bool isSelected;

        [field: Header("数据")]
        [SerializeField] protected UnitSO UnitAttribute;
        [field: SerializeField] public float currentHealth { get; protected set; }
        [field: SerializeField] public BaseCommand[] availableCommands{ get; protected set; }

        protected virtual void Start()
        {
            currentHealth = UnitAttribute.maxHealth;
            EventSystem.EventBus.Publish<UnitSpawnEvent>(new UnitSpawnEvent { unit = this });
        }

        protected virtual void OnEnable()
        {
            EventSystem.EventBus.Subscribe<UnitSelectEvent>(OnSelected);
            EventSystem.EventBus.Subscribe<UnitDeSelectEvent>(OndeSelected);
            //EventSystem.EventBus.Subscribe<UnitMoveToEvent>(MoveTo);
        }

        protected virtual void OnDisable()
        {
            EventSystem.EventBus.UnSubscribe<UnitSelectEvent>(OnSelected);
            EventSystem.EventBus.UnSubscribe<UnitDeSelectEvent>(OndeSelected);
            //EventSystem.EventBus.UnSubscribe<UnitMoveToEvent>(MoveTo);
        }

        public void OnSelected(UnitSelectEvent evt)
        {
            
            if (evt.Unit == (ISelectable)this)
            {
                Debug.Log($"OnSelected 被调用, evt.Unit: {evt.Unit}, this: {this.gameObject.name}");
                DecalProjector?.gameObject.SetActive(true);
                isSelected = true;
                //TODO
            }
        }

        public void OndeSelected(UnitDeSelectEvent evt)
        {
            
            if (evt.Unit == (ISelectable)this)
            {
                Debug.Log($"OndeSelected 被调用, evt.Unit: {evt.Unit}, this: {this.gameObject.name}");
                DecalProjector?.gameObject.SetActive(false);
                isSelected = false;
                //TODO
            }
        }
    }

}
