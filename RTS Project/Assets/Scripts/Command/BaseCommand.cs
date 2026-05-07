using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        public abstract bool CanHandle(CommandContext commandContext);

        public abstract void Handle(CommandContext commandContext);

    }


}