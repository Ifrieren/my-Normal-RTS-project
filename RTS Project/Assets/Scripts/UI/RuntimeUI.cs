using UnityEngine;
using RTS.EventSystem;
using RTS.UI.Components;
using System.Collections.Generic;
using RTS.Units;
using System;
namespace RTS.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [Header("组件")]
        [SerializeField] private CommandsUI CommandsUI;
        [SerializeField] private HashSet<BaseCommandable> SelectedCommandables = new(20);
        void OnEnable()
        {
            EventBus.Subscribe<UnitDeSelectEvent>(HandleUnitDeSelected);
            EventBus.Subscribe<UnitSelectEvent>(HandleUnitSelected);
        }
        void OnDisable()
        {
            EventBus.Subscribe<UnitDeSelectEvent>(HandleUnitDeSelected);
            EventBus.Subscribe<UnitSelectEvent>(HandleUnitSelected);
        }
        private void HandleUnitSelected(UnitSelectEvent evt)
        {
            if(evt.Unit is BaseCommandable Commandable)
            {
                SelectedCommandables.Add(Commandable);
                CommandsUI.EnableFor(SelectedCommandables);
            }
        }
        private void HandleUnitDeSelected(UnitDeSelectEvent evt)
        {
            if (evt.Unit is BaseCommandable Commandable)
            {
                SelectedCommandables.Remove(Commandable);
                if (SelectedCommandables.Count > 0)
                {
                    CommandsUI.EnableFor(SelectedCommandables);
                }
                else
                {
                    CommandsUI.Disable();
                }
                
            }
        }

        
    }
}