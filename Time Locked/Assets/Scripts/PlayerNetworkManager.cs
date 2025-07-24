using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>()
                .Where(component => !(component is NetworkBehaviour))
                .ToArray();
            foreach (var component in components)
            {
                component.enabled = false;
            }
            return;
        }
        PlayerInventory inv = gameObject.AddComponent<PlayerInventory>();
        StartCoroutine(AssignUIWhenReady(inv));
    }
    private IEnumerator AssignUIWhenReady(PlayerInventory inv)
    {
        // Wait until InventoryUIController.Instance is assigned
        while (InventoryUIController.Instance == null)
            yield return null;

        InventoryUIController.Instance.AssignToInventory(inv);
    }
}
