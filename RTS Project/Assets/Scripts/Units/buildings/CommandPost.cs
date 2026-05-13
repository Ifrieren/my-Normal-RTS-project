using System.Collections;
using System.Collections.Generic;
using RTS.Units;
using Unity.Mathematics;
using UnityEngine;

public class CommandPost : BaseBuilding
{
    [Header("状态")]
    [SerializeField] private bool isBuilding;
    private const int MAX_QUENE_LIST = 5;
    private Queue<UnitSO> BuildingQuene = new(MAX_QUENE_LIST);

    private void Update()
    {
        UpdateBuildingQuene();
    }

    public void BuildUnit(UnitSO unitSO)
    {
        if (BuildingQuene.Count == MAX_QUENE_LIST)
        {
            Debug.Log("建造队列已满");
            return;
        }
        BuildingQuene.Enqueue(unitSO);

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
        UnitSO unitSO = BuildingQuene.Peek();
        yield return new WaitForSeconds(unitSO.BuildTime);
        Instantiate(unitSO.UnitPrefab, transform.position, quaternion.identity);
        BuildingQuene.Dequeue();
        isBuilding = false;
        Debug.Log("单位建造完成");
    }
}
