using UnityEngine;
using RTS.EventSystem;
using System.Collections.Generic;
using RTS.Units;
using RTS.Commands;
using System.Linq;
using UnityEngine.Events;
using RTS.UI.Containers;
namespace RTS.UI.Components
{
    public class CommandsUI : MonoBehaviour ,IUIElement<HashSet<BaseCommandable>>
    {
        [SerializeField] private UIActionButtons[] actionButtons;
        private HashSet<BaseCommandable> selectedUnit = new(30);

        void Start()
        {
            Disable();
        }

        public void EnableFor(HashSet<BaseCommandable> selectedUnit)
        {
            RefreshButtons(selectedUnit);
        }

        public void Disable()
        {
            foreach (UIActionButtons button in actionButtons)
            {
                button.Disable();
            }
        }


        private void RefreshButtons(HashSet<BaseCommandable> selectedUnit)
        {
            HashSet<BaseCommand> availableCommands = new(9);
            foreach (BaseCommandable commandable in selectedUnit)
            {
                availableCommands.UnionWith(commandable.availableCommands);
            }
            
            for (int i = 0; i < actionButtons.Length; i++)
            {
                BaseCommand actionForSlot = availableCommands.Where(action => action.slot == i).FirstOrDefault();
                
                if (actionForSlot != null)
                {
                    actionButtons[i].EnableFor(actionForSlot, HandleClick(actionForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }
        
        private UnityAction HandleClick(BaseCommand command)
        {
            return () => EventSystem.EventBus.Publish<CommandSelectedEvent>(
                new CommandSelectedEvent { Command = command });
        }

       
    }

}
