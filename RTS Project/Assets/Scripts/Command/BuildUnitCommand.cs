using System;
using NUnit.Framework;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
    public class BuildUnitCommand : BaseCommand
    {

        [field: SerializeField] public UnitSO unitSO { get; private set; }
        public override bool CanHandle(CommandContext commandContext)
        {
            return commandContext.Commandable is CommandPost;
        }

        public override void Handle(CommandContext commandContext)
        {
            CommandPost commandPost = (CommandPost)commandContext.Commandable;
            commandPost.BuildUnit(unitSO);
        }
    }


}