using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        public abstract bool CanHandle(BaseCommandable commandable, RaycastHit hit);

        public abstract void Handle(BaseCommandable commandable, RaycastHit hit);
    }


}