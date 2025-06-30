using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance { get; private set; }

    private Lobby connectedLobby;
    private float lobbyHeartbeatTimer;
    private float lobbyPollTimer;
    
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

    // The host needs to send a heartbeat every 15 seconds to keep the lobby from being deleted
    private async void HandleLobbyHeartbeat()
    {
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
        if (connectedLobby == null) return;

        lobbyPollTimer -= Time.deltaTime;
        if (lobbyPollTimer < 0f)
        {
            float lobbyPollMax = 1.1f;
            lobbyPollTimer = lobbyPollMax;
            
            connectedLobby = await LobbyService.Instance.GetLobbyAsync(connectedLobby.Id);
            LobbyUIManager.Instance.UpdatePlayerSlots(connectedLobby.Players);

            // If a client joins, the lobby UI should activate for them
            if (!IsLobbyHost()) {
                 LobbyUIManager.Instance.ShowLobbyUI();
            }
        }
    }

    public async Task CreateLobby(string lobbyName, bool isPrivate)
    {
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
            
            await LobbyService.Instance.UpdateLobbyAsync(connectedLobby.Id, new UpdateLobbyOptions{
                Data = new System.Collections.Generic.Dictionary<string, DataObject>{
                    {"RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });

            LobbyUIManager.Instance.ShowLobbyUI();
            LobbyUIManager.Instance.UpdateLobbyCode(connectedLobby.LobbyCode);
            LobbyUIManager.Instance.UpdatePlayerSlots(connectedLobby.Players);
            Debug.Log($"Created lobby with code: {connectedLobby.LobbyCode}");

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            LobbyUIManager.Instance.ShowMainMenuUI(); // Go back to main menu on failure
        }
    }

    public async Task<bool> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            connectedLobby = joinedLobby;

            string relayJoinCode = joinedLobby.Data["RELAY_CODE"].Value;
            await RelayManager.Instance.JoinRelay(relayJoinCode);
            
            LobbyUIManager.Instance.UpdatePlayerSlots(connectedLobby.Players);
            LobbyUIManager.Instance.UpdateLobbyCode(joinedLobby.LobbyCode);

            Debug.Log($"Joined lobby with code: {lobbyCode}");
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return false;
        }
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
        return connectedLobby != null && connectedLobby.HostId == Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
    }
}