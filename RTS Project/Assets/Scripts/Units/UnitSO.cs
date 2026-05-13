using UnityEngine;

namespace RTS.Units
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Units/UnitAttribute")]
    public class UnitSO : ScriptableObject
    {
        [field: SerializeField] public float maxHealth { get; private set; }
        [field: SerializeField] public GameObject UnitPrefab { get; private set; }
        [field: SerializeField] public float BuildTime { get; private set; } = 3f;
    }



}