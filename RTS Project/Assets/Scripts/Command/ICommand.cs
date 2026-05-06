using UnityEngine;
using RTS.Units;
namespace RTS.Commands
{
    public interface ICommand
    {
        bool CanHandle(BaseCommandable commandable, RaycastHit hit);
        void Handle(BaseCommandable commandable, RaycastHit hit);
    }

}
