using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;

// Attach this to the root of the Player prefab.
// Disables input & camera for remote players so each client only controls its own character.
public class PlayerNetworkSetup : NetworkBehaviour
{
    [Header("Components that should only be active for the local player")] 
    [SerializeField] private MonoBehaviour[] localOnlyScripts; // e.g. FirstPersonController, StarterAssetsInputs
    [SerializeField] private GameObject cameraRoot;            // parent GameObject that contains camera & audio listener
    [SerializeField] private float localMovementEnableDelay = 1f; // seconds fixed at 1

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            DisableForRemotePlayer();
        }
        else
        {
            StartCoroutine(EnableLocalScriptsAfterDelay());
        }
    }

    private void DisableForRemotePlayer()
    {
        // Disable any local-only scripts
        foreach (var mono in localOnlyScripts)
        {
            if (mono != null) mono.enabled = false;
        }

        // Disable the PlayerInput component if present so remote clients don't send input
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        // Turn off camera & audio for remote players
        if (cameraRoot != null)
        {
            cameraRoot.SetActive(false);
        }
        else
        {
            // Fallback: disable every Camera & AudioListener in children
            foreach (var cam in GetComponentsInChildren<Camera>(true))
            {
                cam.enabled = false;
                if (cam.CompareTag("MainCamera")) cam.tag = "Untagged";
            }
            foreach (var al in GetComponentsInChildren<AudioListener>(true))
            {
                al.enabled = false;
            }
        }
    }

    private void EnsureMainCameraTag()
    {
        // Make sure only the local player's camera is tagged as MainCamera
        var cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.tag = "MainCamera";
    }

    private IEnumerator EnableLocalScriptsAfterDelay()
    {
        // Disable scripts first
        foreach (var mono in localOnlyScripts)
        {
            if (mono != null) mono.enabled = false;
        }
        // Disable CharacterController
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        EnsureMainCameraTag();
        yield return new WaitForSeconds(localMovementEnableDelay);
        foreach (var mono in localOnlyScripts)
        {
            if (mono != null) mono.enabled = true;
        }
        if (cc != null) cc.enabled = true;
    }

    private void ApplyColor(bool isOrange)
    {
        Color targetColor = isOrange ? new Color(1f, 0.5f, 0f, 1f) : Color.blue; // Orange or Blue
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = targetColor;
        }
    }

    [ClientRpc]
    private void SetColorClientRpc(bool isOrange)
    {
        ApplyColor(isOrange);
    }

    // Called by the server immediately after the player object is spawned
    public void SetInitialColor(bool isOrange)
    {
        // Apply on the server so host sees the color instantly
        ApplyColor(isOrange);
        // Propagate to all clients
        SetColorClientRpc(isOrange);
    }
} 