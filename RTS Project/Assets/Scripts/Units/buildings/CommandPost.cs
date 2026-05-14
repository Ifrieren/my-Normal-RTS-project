using System.Collections;
using System.Collections.Generic;
using RTS.Units;
using Unity.Mathematics;
using UnityEngine;

public class CommandPost : BaseBuilding
{
    [Header("状态")]
    [SerializeField] private bool isBuilding;

    [field: SerializeField] public UnitSO CurrentBuildingUnit { get; private set; }
    [field: SerializeField] public float CurrentQueneStartTime { get; private set; }

    public delegate void QueneUpdateEvent(UnitSO[] UnitQuene);
    public event QueneUpdateEvent OnQueneUpdate;
    private const int MAX_QUENE_LIST = 5;

    public int QueneSize => BuildingQuene.Count;
    private Queue<UnitSO> BuildingQuene = new(MAX_QUENE_LIST);

    public void BuildUnit(UnitSO unitSO)
    {
        if (BuildingQuene.Count == MAX_QUENE_LIST)
        {
            Debug.Log("建造队列已满");
            return;
        }
        BuildingQuene.Enqueue(unitSO);
        UpdateBuildingQuene();
    }
    private void UpdateBuildingQuene()
    {
        if (BuildingQuene.Count >= 1 && !isBuilding)
        {
            StartCoroutine(DoBuildUnit());
            isBuilding = true;
        }
        
    }

    private IEnumerator DoBuildUnit()
    {

        Debug.Log("开始建造单位");

        CurrentBuildingUnit = BuildingQuene.Peek();
        CurrentQueneStartTime = Time.time;

        OnQueneUpdate?.Invoke(BuildingQuene.ToArray());// 更新建造UI事件

        yield return new WaitForSeconds(CurrentBuildingUnit.BuildTime);
        Instantiate(CurrentBuildingUnit.UnitPrefab, transform.position, quaternion.identity);

        BuildingQuene.Dequeue();

        Debug.Log("单位建造完成");

        isBuilding = false;
        UpdateBuildingQuene();

        OnQueneUpdate?.Invoke(BuildingQuene.ToArray()); //清除建造的进度条状态
    }
}
