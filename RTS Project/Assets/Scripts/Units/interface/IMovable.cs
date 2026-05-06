using UnityEngine;
using RTS.EventSystem;
using UnityEditor;

namespace RTS.Units
{
    public interface IMovable
    {
        void MoveTo(Vector3 Pos);
        void CheckIfArriveDes(Vector3 target, Vector3 unit);
    }
}