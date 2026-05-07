using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: Range(0, 8)][field: SerializeField] public int slot { get; private set; }
        [field: SerializeField] public bool RequireClickToActivate { get; private set; } = true;
        public abstract bool CanHandle(CommandContext commandContext);

        public abstract void Handle(CommandContext commandContext);

    }


}