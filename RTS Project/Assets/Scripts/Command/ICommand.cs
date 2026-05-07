using UnityEngine;
using RTS.Units;
namespace RTS.Commands
{
    public interface ICommand
    {
        bool CanHandle(CommandContext commandContext);
        void Handle(CommandContext commandContext);
    }

}
