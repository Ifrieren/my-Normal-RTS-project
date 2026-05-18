using System;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand //, IComparable<BaseCommand>
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: Range(0, 8)][field: SerializeField] public int slot { get; private set; }
        [field: SerializeField] public bool RequireClickToActivate { get; private set; } = true;

        [field: SerializeField] public CommandSource Source { get; private set; } = CommandSource.PlayerInput;


        // // 显示的优先级越大则越先显示
        // [field: Range(0, 1)][field: SerializeField] public int priority { get; private set; } = 0;
        public abstract bool CanHandle(CommandContext commandContext);

        public abstract void Handle(CommandContext commandContext);

        // public int CompareTo(BaseCommand command)
        // {
        //     return command.priority.CompareTo(this.priority);
        // }

    }


}