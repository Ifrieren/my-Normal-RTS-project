using System.Collections.Generic;
using System.Collections;
using RTS.EventSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CommandRingUIPool : MonoBehaviour
{
    [Header("对象池配置")]
    [SerializeField] private DecalProjector CommandRingPrefab;
    [Range(1, 8)][SerializeField] private int PoolSize = 4;
    [SerializeField] private float ShrinkDuration = 1.5f;
    [SerializeField] private float StartRadius = 0.9f;
    [SerializeField] private float EndRadius = 0.1f;

    private Queue<DecalProjector> CommandRingQueue;
    private Material SourceMaterial;
    private static readonly int RingInnerRadiusID = Shader.PropertyToID("_RingInnerRdius");

    void Awake()
    {
        CommandRingQueue = new Queue<DecalProjector>(PoolSize);
        PreWarmPool();
        SourceMaterial = CommandRingPrefab.material;
    }

    void OnEnable()
    {
        EventBus.Subscribe<MobileCommandRingSpawnEvent>(SpawnRing);
    }

    void OnDisable()
    {
        EventBus.UnSubscribe<MobileCommandRingSpawnEvent>(SpawnRing);
    }

    private void PreWarmPool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var ring = Instantiate(CommandRingPrefab, transform);
            ring.gameObject.SetActive(false);
            CommandRingQueue.Enqueue(ring);
        }
    }

    private void SpawnRing(MobileCommandRingSpawnEvent evt)
    {
        if (!CommandRingQueue.TryDequeue(out DecalProjector ring))
        {
            Debug.LogWarning("Ring 对象池已空，考虑增大 PoolSize");
            return;
        }

        ring.transform.position = new Vector3(evt.Pos.x, 1f, evt.Pos.z);
        ring.gameObject.SetActive(true);

        StartCoroutine(ShrinkAndRecycle(ring));
    }

    private IEnumerator ShrinkAndRecycle(DecalProjector ring)
    {
        if (ring.material == SourceMaterial)
        {
            ring.material = new Material(SourceMaterial);
        }
        else
        {
            ring.material.CopyPropertiesFromMaterial(SourceMaterial);
        }
        Material mat = ring.material;
        float elapsed = 0f;

        while (elapsed < ShrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ShrinkDuration;
            float radius = Mathf.Lerp(StartRadius, EndRadius, t);
            mat.SetFloat(RingInnerRadiusID, radius);
            yield return null;
        }

        mat.SetFloat(RingInnerRadiusID, EndRadius);
        RecycleRing(ring);
    }

    private void RecycleRing(DecalProjector ring)
    {
        ring.gameObject.SetActive(false);
        CommandRingQueue.Enqueue(ring);
    }
}
