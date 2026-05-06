using System;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Move", menuName = "AI/Action",order = 100)]
    public class MoveCommand : BaseCommand
    {
        
        public override bool CanHandle(BaseCommandable commandable, RaycastHit hit)
        {
            return commandable is IMovable;
        }

        public override void Handle(BaseCommandable commandable, RaycastHit hit)
        {
            //todo 
        }
    }
}