using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class VfxManager : MonoBehaviour
{
    // Singleton instance (can be optional for small games)
    public static VfxManager Instance { get; private set; }

   [System.Serializable]
    public class VFXPrefab
    {
        public string vfxName;
        public GameObject prefab;
        public int poolSize = 10;
    }

    public List<VFXPrefab> vfxPrefabs;
    private Dictionary<string, Queue<GameObject>> pools;
    private Dictionary<string, GameObject> vfxDict;

    void Awake()
    {
        Instance = this;
        vfxDict = new Dictionary<string, GameObject>();
        pools = new Dictionary<string, Queue<GameObject>>();
        foreach (var vfx in vfxPrefabs)
        {
            vfxDict[vfx.vfxName] = vfx.prefab;
            pools[vfx.vfxName] = new Queue<GameObject>();
            for (int i = 0; i < vfx.poolSize; i++)
            {
                GameObject obj = Instantiate(vfx.prefab);
                obj.SetActive(false);
                pools[vfx.vfxName].Enqueue(obj);
            }
        }
    }

    [CanBeNull]
    public GameObject PlayVFX(string vfxName, Vector3 position, Quaternion rotation, float scale = 1f, Color? color = null, bool? flipx = false, float? lifetime = null)
    {
        if (!pools.ContainsKey(vfxName)) { Debug.LogWarning($"VFX '{vfxName}' not found!"); return null; }

        GameObject vfx = pools[vfxName].Count > 0 ? pools[vfxName].Dequeue() : Instantiate(vfxDict[vfxName]);
        vfx.transform.position = position;
        vfx.transform.rotation = rotation;
        vfx.transform.localScale = Vector3.one * scale;
        vfx.SetActive(true);

        // Customization (color)
        var sr = vfx.GetComponent<SpriteRenderer>();
        if (sr && color.HasValue) sr.color = color.Value;
        if (sr) sr.flipX = flipx.HasValue ? flipx.Value : false;
        
        var ps = vfx.GetComponent<ParticleSystem>();
        if (ps && color.HasValue) {
            var main = ps.main;
            main.startColor = color.Value;
        }

        // Auto-deactivate after duration
        float life;

        if (lifetime.HasValue)
        {
            life = lifetime.Value; // caller override
        }
        else if (ps)
        {
            var main = ps.main;
            life = main.duration + main.startLifetime.constantMax; // particle-based
        }
        else
        {
            life = 2f;  // default
        }

        StartCoroutine(DeactivateVFX(vfxName, vfx, life));
        return vfx;
    }

    public void StopVFX(string vfxName, GameObject vfx)
    {
        if (vfx == null) return;

        if (!pools.ContainsKey(vfxName))
        {
            Debug.LogWarning($"VFX '{vfxName}' not found for StopVFX!");
            vfx.SetActive(false);
            return;
        }

        // Optional: stop particle system immediately
        var ps = vfx.GetComponent<ParticleSystem>();
        if (ps)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        vfx.SetActive(false);

        // Avoid double‑enqueue if some other code already returned it
        if (!pools[vfxName].Contains(vfx))
            pools[vfxName].Enqueue(vfx);
    }
    
    
    public void StopAllVFX(string vfxName)
    {
        if (!pools.ContainsKey(vfxName))
        {
            Debug.LogWarning($"VFX '{vfxName}' not found for StopAllVFX!");
            return;
        }

        // Find all objects in scene using this prefab name
        foreach (var kvp in pools)
        {
            foreach (var obj in kvp.Value)
            {
                // pooled ones are already inactive; only handle active ones in scene:
                if (obj.activeSelf && kvp.Key == vfxName)
                {
                    obj.SetActive(false);
                    pools[vfxName].Enqueue(obj);
                }
            }
        }
    }
    
    
    private System.Collections.IEnumerator DeactivateVFX(string vfxName, GameObject vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        vfx.SetActive(false);
        pools[vfxName].Enqueue(vfx);
    }

}
