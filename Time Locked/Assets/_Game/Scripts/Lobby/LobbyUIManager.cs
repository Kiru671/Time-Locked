using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject lobbyCodeInputUI;
    [SerializeField] private GameObject lobbyUI;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;

    [Header("Lobby Code Input")]
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button backButton;

    [Header("Lobby View")]
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    private void Start()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
        joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
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

    public void UpdatePlayerCount(int count)
    {
        playerCountText.text = $"Count = {count}";
    }
}