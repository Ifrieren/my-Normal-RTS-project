using System.Collections.Generic;
using System.Collections;
using RTS.EventSystem;
using RTS.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CommandRingUIPool : MonoBehaviour
{
    [Header("对象池配置")]
    [SerializeField] private DecalProjector CommandRing;
    [Range(0, 4)][SerializeField] private int PoolSize = 4 ;
    [SerializeField] private float TimeToDestroyTheRing = 1.5f;
    [SerializeField] private Queue<DecalProjector> CommandRingQueue = new(5);
    void Awake()
    {
        PreWarmPool();
    }
    void OnEnable()
    {
        EventBus.Subscribe<MobileCommandRingSpawnEvent>(SpawnRing);
        //EventBus.Subscribe<MobileCommandRingDestroyEvent>(DestroyRing);
    }
    void OnDisable()
    {
        EventBus.UnSubscribe<MobileCommandRingSpawnEvent>(SpawnRing);
        //EventBus.UnSubscribe<MobileCommandRingDestroyEvent>(DestroyRing);
    }

    private void PreWarmPool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var ring = Instantiate(CommandRing);
            CommandRingQueue.Enqueue(ring);
            ring.gameObject.SetActive(false);
        }
    }
    private void SpawnRing(MobileCommandRingSpawnEvent evt)
    {
        if (CommandRingQueue.Count <= 0)
        {
            Debug.Log("Ring对象池已空");
            return;
        }
        DecalProjector newRing = CommandRingQueue.Dequeue();
        newRing.gameObject.SetActive(true);
        newRing.transform.position = new Vector3(evt.Pos.x, newRing.transform.position.y, evt.Pos.z);

        StartCoroutine(UpdateRingUI(newRing));
    }
    private IEnumerator UpdateRingUI(DecalProjector Ring)
    {
        //TODO消失动画
        yield return new WaitForSeconds(TimeToDestroyTheRing);
        DestroyRing(Ring);
    }
    private void DestroyRing(DecalProjector Ring)
    {
        CommandRingQueue.Enqueue(Ring);
        Ring.gameObject.SetActive(false);
        
    }

}

