using System.Collections;
using System.Collections.Generic;
using RTS.Units;
using Unity.Mathematics;
using UnityEngine;

public class CommandableBuilding : BaseBuilding
{
    [Header("状态")]
    [SerializeField] private bool isBuilding;// 仅用于debug

    [field: SerializeField] public UnitSO CurrentBuildingUnit { get; private set; }
    [field: SerializeField] public float CurrentQueneStartTime { get; private set; }

    public delegate void QueneUpdateEvent(UnitSO[] UnitQuene);
    public event QueneUpdateEvent OnQueneUpdate;
    private const int MAX_QUENE_LIST = 5;

    //供外部调用
    public int QueneSize => BuildingQuene.Count;

    public UnitSO[] Quene => BuildingQuene.ToArray();
    private List<UnitSO> BuildingQuene = new(MAX_QUENE_LIST);

    public void BuildUnit(UnitSO unitSO)
    {
        if (BuildingQuene.Count == MAX_QUENE_LIST)
        {
            Debug.Log("建造队列已满");
            return;
        }
        BuildingQuene.Add(unitSO);
        UpdateBuildingQuene();
    }
    private void UpdateBuildingQuene()
    {
        if (BuildingQuene.Count == 1)
        {
            StartCoroutine(DoBuildUnit());
        }
        else
        {
            OnQueneUpdate?.Invoke(BuildingQuene.ToArray());
        }

    }

    public void CancelBuildingUnit(int index)
    {
        if (index < 0 || index > BuildingQuene.Count)
        {
            Debug.LogError("取消的单位不在队列中");
            return;
        }
        BuildingQuene.RemoveAt(index);
        if (index == 0) // 索引为0时代表正在建造，需要取消携程
        {
            StopAllCoroutines();
            if (BuildingQuene.Count > 0)
            {
                StartCoroutine(DoBuildUnit());
            }
            else
            {
                OnQueneUpdate?.Invoke(BuildingQuene.ToArray());
            }
        }
        else
        {
            OnQueneUpdate?.Invoke(BuildingQuene.ToArray());
        }
    }
    private IEnumerator DoBuildUnit()
    {
        while (BuildingQuene.Count > 0)
        {
            Debug.Log("开始建造单位");

            CurrentBuildingUnit = BuildingQuene[0];
            CurrentQueneStartTime = Time.time;

            OnQueneUpdate?.Invoke(BuildingQuene.ToArray());// 更新建造UI事件

            yield return new WaitForSeconds(CurrentBuildingUnit.BuildTime);
            Instantiate(CurrentBuildingUnit.UnitPrefab, transform.position, quaternion.identity);

            BuildingQuene.RemoveAt(0);

            Debug.Log("单位建造完成");
            OnQueneUpdate?.Invoke(BuildingQuene.ToArray()); //清除建造的进度条状态
        }


    }


}
