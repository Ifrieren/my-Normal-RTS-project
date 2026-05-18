using RTS.Units;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
namespace RTS.Commands
{
    public struct CommandContext
    {

        public BaseCommandable Commandable { get; private set; }
        public RaycastHit Hit { get; private set; }
        public int UnitIndex { get; private set; }
        public CommandSource CommandSource { get; private set; }

        public CommandContext(BaseCommandable commandable, RaycastHit hit, int unitIndex = 0,
        CommandSource commandSource = CommandSource.PlayerInput)
        {
            Commandable = commandable;
            Hit = hit;
            UnitIndex = unitIndex;
            CommandSource = commandSource;
        }

    }
}