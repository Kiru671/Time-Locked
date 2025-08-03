using UnityEngine;
using Unity.Netcode;

public class ClockInteraction : NetworkBehaviour
{
    [Header("Interaction Settings")]
    public ClockController clockController;
    public float interactionRange = 3f;
    
    private Transform player;
    private bool isInRange = false;
    
    void Start()
    {
        // Player'ı bul - NetworkManager'dan local player'ı al
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayerObject != null)
            {
                player = localPlayerObject.transform;
                Debug.Log($"ClockInteraction found local player: {player.name}");
            }
        }

        // Fallback: Tag ile ara
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"ClockInteraction found player by tag: {player.name}");
            }
        }
    }

    void Update()
    {
        CheckPlayerDistance();

        if (isInRange && Input.GetKeyDown(clockController.adjustKey))
        {
            Debug.Log($"Input detected! Adjusting clock {clockController.clockId}");
            clockController.AdjustTime();
        }
    }
    
    void CheckPlayerDistance()
    {
        if (player == null)
        {
            // Player'ı tekrar bulmaya çalış
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
            {
                var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (localPlayerObject != null)
                {
                    player = localPlayerObject.transform;
                }
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionRange;

        // Debug: Range değişikliklerini logla
        if (wasInRange != isInRange)
        {
            Debug.Log($"Clock {clockController.clockId} - Player {(isInRange ? "entered" : "left")} interaction range. Distance: {distance:F2}");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
