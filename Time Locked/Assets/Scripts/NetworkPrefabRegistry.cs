using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabRegistry : MonoBehaviour
{
    private static NetworkPrefabRegistry instance;

    [SerializeField] private GameObject[] registeredPrefabs;
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

    void Awake()
    {
        instance = this;

        // Build lookup table
        foreach (var prefab in registeredPrefabs)
        {
            prefabLookup[prefab.name] = prefab;
        }
    }

    public static GameObject GetPrefabByName(string name)
    {
        // Remove (Clone) suffix if present
        name = name.Replace("(Clone)", "").Trim();

        if (instance.prefabLookup.TryGetValue(name, out GameObject prefab))
            return prefab;

        Debug.LogError($"Prefab {name} not found in registry!");
        return null;
    }
}