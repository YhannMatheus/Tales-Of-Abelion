using System.Collections.Generic;
using UnityEngine;

public static class AbilityPool
{
    static Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    static GameObject poolRoot;

    static void EnsureRoot()
    {
        if (poolRoot == null)
        {
            poolRoot = new GameObject("AbilityPool_Root");
            Object.DontDestroyOnLoad(poolRoot);
        }
    }

    public static GameObject Get(GameObject prefab)
    {
        if (prefab == null) return null;
        EnsureRoot();
        Queue<GameObject> q;
        if (pools.TryGetValue(prefab, out q) && q.Count > 0)
        {
            var go = q.Dequeue();
            go.SetActive(true);
            go.transform.SetParent(null);
            return go;
        }
        var inst = Object.Instantiate(prefab);
        return inst;
    }

    public static void Release(GameObject instance, GameObject prefab = null)
    {
        if (instance == null) return;
        EnsureRoot();
        instance.SetActive(false);
        instance.transform.SetParent(poolRoot.transform);

        if (prefab == null)
        {
            // try to find original prefab by name (best-effort)
            // fallback: do not pool
            return;
        }

        Queue<GameObject> q;
        if (!pools.TryGetValue(prefab, out q))
        {
            q = new Queue<GameObject>();
            pools[prefab] = q;
        }
        q.Enqueue(instance);
    }
}
