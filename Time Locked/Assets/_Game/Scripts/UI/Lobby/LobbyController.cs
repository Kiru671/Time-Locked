using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using System.Collections;
using Unity.Services.Authentication;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance { get; private set; }

    private Lobby connectedLobby;
    private float lobbyHeartbeatTimer;
    private float lobbyPollTimer;
    private bool inGame = false;

    private static bool servicesInitialized = false;

    private void OnApplicationQuit()
    {
        LeaveLobby();
    }

    /* (?)
    private void OnDestroy()
    {
        LeaveLobby();
    }
    */

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        // These methods handle keeping the lobby alive and updated
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    private async Task EnsureUnityServicesInitialized()
    {
        if (servicesInitialized) return;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            servicesInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e}");
        }
    }

    // The host needs to send a heartbeat every 15 seconds to keep the lobby from being deleted
    private async void HandleLobbyHeartbeat()
    {
        if (inGame) return;
        if (connectedLobby == null || !IsLobbyHost()) return;

        lobbyHeartbeatTimer -= Time.deltaTime;
        if (lobbyHeartbeatTimer < 0f)
        {
            float lobbyHeartbeatMax = 15f;
            lobbyHeartbeatTimer = lobbyHeartbeatMax;
            await LobbyService.Instance.SendHeartbeatPingAsync(connectedLobby.Id);
        }
    }

    // We poll the lobby every second to check for changes, like a new player joining
    private async void HandleLobbyPolling()
    {
        if (inGame) return;
        if (connectedLobby == null) return;

        // Check if LobbyUIManager still exists (might be destroyed during scene changes)
        if (LobbyUIManager.Instance == null) return;

        lobbyPollTimer -= Time.deltaTime;
        if (lobbyPollTimer < 0f)
        {
            float lobbyPollMax = 1.1f;
            lobbyPollTimer = lobbyPollMax;

            try
            {
                connectedLobby = await LobbyService.Instance.GetLobbyAsync(connectedLobby.Id);

                // Check if the game has started
                if (connectedLobby.Data.ContainsKey("GAME_STARTED") &&
                    connectedLobby.Data["GAME_STARTED"].Value == "true")
                {
                    TransitionToGameScene();
                    return; // Exit early since we're transitioning scenes
                }

                // Double-check that UI still exists after async operation
                if (LobbyUIManager.Instance != null)
                {
                    LobbyUIManager.Instance.UpdatePlayerSlots(connectedLobby.Players);
                    LobbyUIManager.Instance.UpdateHostControls(IsLobbyHost());

                    // If a client joins, the lobby UI should activate for them
                    if (!IsLobbyHost())
                    {
                        LobbyUIManager.Instance.ShowLobbyUI();
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Lobby polling failed: {e.Message}");
                // If we can't reach the lobby, clear the connection
                connectedLobby = null;
            }
        }
    }

    public async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        await EnsureUnityServicesInitialized();

        try
        {
            var createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { "RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, createLobbyOptions);
            connectedLobby = lobby;

            string relayJoinCode = await RelayManager.Instance.CreateRelay();

            await LobbyService.Instance.UpdateLobbyAsync(connectedLobby.Id, new UpdateLobbyOptions
            {
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { "RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            // Check if UI still exists after async operations
            if (LobbyUIManager.Instance != null)
            {
                LobbyUIManager.Instance.ShowLobbyUI();
                LobbyUIManager.Instance.UpdateLobbyCode(connectedLobby.LobbyCode);
                LobbyUIManager.Instance.UpdatePlayerSlots(connectedLobby.Players);
                LobbyUIManager.Instance.UpdateHostControls(true); // Host created the lobby
            }

            Debug.Log($"Created lobby with code: {connectedLobby.LobbyCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            // Check if UI still exists before trying to use it
            if (LobbyUIManager.Instance != null)
            {
                LobbyUIManager.Instance.ShowMainMenuUI(); // Go back to main menu on failure
            }
        }
    }

    public async Task<bool> JoinLobbyByCode(string lobbyCode)
    {
        await EnsureUnityServicesInitialized();

        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            connectedLobby = joinedLobby;

            string relayJoinCode = joinedLobby.Data["RELAY_CODE"].Value;
            await RelayManager.Instance.JoinRelay(relayJoinCode);

            // Check if UI still exists after async operations
            if (LobbyUIManager.Instance != null)
            {
                LobbyUIManager.Instance.UpdatePlayerSlots(connectedLobby.Players);
                LobbyUIManager.Instance.UpdateLobbyCode(joinedLobby.LobbyCode);
                LobbyUIManager.Instance.UpdateHostControls(false); // Client joined, not host
            }

            Debug.Log($"Joined lobby with code: {lobbyCode}");
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public async Task StartGame()
    {
        if (connectedLobby == null)
        {
            Debug.LogError("No lobby connected!");
            return;
        }

        if (!IsLobbyHost())
        {
            Debug.LogError("Only the host can start the game!");
            return;
        }

        // Launch coroutine that handles lobby update, delay, and scene switch
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        // Update lobby data to indicate game start
        var updateOptions = new UpdateLobbyOptions
        {
            Data = new System.Collections.Generic.Dictionary<string, DataObject>
            {
                { "GAME_STARTED", new DataObject(DataObject.VisibilityOptions.Member, "true") }
            }
        };
        var task = LobbyService.Instance.UpdateLobbyAsync(connectedLobby.Id, updateOptions);
        while (!task.IsCompleted) { yield return null; }
        if (task.IsFaulted)
        {
            Debug.LogError($"Failed to update lobby for game start: {task.Exception}");
            yield break;
        }

        // Give clients time to receive the flag via polling
        yield return new WaitForSeconds(2f);

        // Load the game scene (clients follow automatically)
        TransitionToGameScene();

        // After short delay, leave lobby (no more heartbeats needed)
        yield return new WaitForSeconds(1f);
        LeaveLobby();
    }

    private void TransitionToGameScene()
    {
        if (inGame) return;
        inGame = true;

        // Only the host loads the scene. Clients follow automatically via Netcode scene events.
        if (IsLobbyHost())
        {
            Debug.Log("Host is loading the game scene via NetworkManager.");
            Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Client detected GAME_STARTED flag; waiting for server scene event.");
        }
        // Do NOT leave lobby here; handled in coroutine for host
    }

    public async void LeaveLobby()
    {
        if (connectedLobby == null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(
                connectedLobby.Id,
                Unity.Services.Authentication.AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }

        connectedLobby = null;
    }

    private bool IsLobbyHost()
    {
        return connectedLobby != null && connectedLobby.HostId ==
            Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
    }
}