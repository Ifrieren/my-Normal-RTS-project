using System;
using System.Drawing;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Move Command", menuName = "AI/Action/Move", order = 100)]
    public class MoveCommand : BaseCommand
    {
        [SerializeField] private float RadiusMultiplier;
        int unitsOnLayer = 0;
        int maxUnitsOnLayer = 1;
        float circleRadius = 0;
        float radialOffset = 0;
        public override bool CanHandle(CommandContext commandContext)
        {
            return commandContext.Commandable is BaseMobileUnit;
        }
        public override void Handle(CommandContext commandContext)
        {
            if (commandContext.UnitIndex == 0)
            {
                ResetAllTheLayer();
            }
            BaseMobileUnit mobileUnit = (BaseMobileUnit)commandContext.Commandable;
            Vector3 targetPos = new(
                commandContext.Hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
                commandContext.Hit.point.y,
                commandContext.Hit.point.z + circleRadius * Mathf.Sin(radialOffset * unitsOnLayer)
            );
            unitsOnLayer++;
            
            if (unitsOnLayer >= maxUnitsOnLayer)
            {
                unitsOnLayer = 0;
                circleRadius += mobileUnit.AgentRadius * RadiusMultiplier;
                maxUnitsOnLayer = Mathf.FloorToInt(Mathf.PI * circleRadius / mobileUnit.AgentRadius);
                radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;//单位圆
            }
            // 实际指令发布
            mobileUnit.MoveTo(targetPos);
        }
        void ResetAllTheLayer()
        {
            unitsOnLayer = 0;
            maxUnitsOnLayer = 1;
            circleRadius = 0;
            radialOffset = 0;
        }
    }
}