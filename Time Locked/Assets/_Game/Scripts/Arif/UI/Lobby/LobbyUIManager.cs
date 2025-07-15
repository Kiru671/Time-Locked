using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [Header("UI Panels")] [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject lobbyCodeInputUI;
    [SerializeField] private GameObject lobbyUI;

    [Header("Main Menu Buttons")] [SerializeField]
    private Button createLobbyButton;

    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button startButton;

    [Header("Lobby Code Input")] [SerializeField]
    private TMP_InputField lobbyCodeInputField;
    [SerializeField] private LobbyCodeSegmentedInput segmentedInput;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button backButton;

    [Header("Lobby View")] [SerializeField]
    private TextMeshProUGUI lobbyCodeText;

    [SerializeField] private PlayerSlotUI[] playerSlots;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
        joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);
        confirmJoinButton.onClick.AddListener(OnConfirmJoinClicked);
        backButton.onClick.AddListener(OnBackClicked);

        ShowMainMenuUI();
    }

    // Button Actions
    private async void OnCreateLobbyClicked()
    {
        await LobbyController.Instance.CreateLobby("MyLobby", true);
    }

    private void OnJoinLobbyClicked()
    {
        ShowLobbyCodeInputUI();
    }

    private async void OnStartButtonClicked()
    {
        // Only the host should be able to start the game
        if (LobbyController.Instance != null)
        {
            await LobbyController.Instance.StartGame();
        }
        else
        {
            Debug.LogError("LobbyController instance not found!");
        }
    }

    private async void OnConfirmJoinClicked()
    {
        string code = lobbyCodeInputField.text;
        if (string.IsNullOrEmpty(code)) return;

        bool joined = await LobbyController.Instance.JoinLobbyByCode(code);
        if (joined)
        {
            // The polling in LobbyController will handle showing the Lobby UI
        }
        else
        {
            Debug.Log("Failed to join lobby.");
            // TODO: Show an error message to the user here
        }
    }

    private void OnBackClicked()
    {
        ShowMainMenuUI();
    }

    // UI State Changers
    public void ShowMainMenuUI()
    {
        mainMenuUI.SetActive(true);
        lobbyCodeInputUI.SetActive(false);
        lobbyUI.SetActive(false);
    }

    private void ShowLobbyCodeInputUI()
    {
        mainMenuUI.SetActive(false);
        lobbyCodeInputUI.SetActive(true);
        lobbyUI.SetActive(false);
        
        segmentedInput.Clear();
    }

    public void ShowLobbyUI()
    {
        mainMenuUI.SetActive(false);
        lobbyCodeInputUI.SetActive(false);
        lobbyUI.SetActive(true);
    }

    // UI Updaters
    public void UpdateLobbyCode(string code)
    {
        lobbyCodeText.text = $"Lobby Code: {code}";
    }

    public void UpdatePlayerSlots(
        System.Collections.Generic.IReadOnlyList<Player> players)
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            // Is this index occupied?
            if (i < players.Count)
            {
                var p = players[i];

                
                playerSlots[i].Refresh(true);
            }
            else
            {
                playerSlots[i].Refresh(false);
            }
        }
    }

    public void UpdateHostControls(bool isHost)
    {
        // Only show start button for the host
        if (startButton != null)
        {
            startButton.gameObject.SetActive(isHost);
        }
    }
}