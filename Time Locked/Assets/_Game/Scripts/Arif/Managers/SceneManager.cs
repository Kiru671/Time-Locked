using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Loading Settings")]
    [SerializeField] private float loadingDelay = 0.5f;

    private void Awake()
    {
        // Singleton
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
    
    // Load the game scene
    public void LoadGameScene()
    {
        StartCoroutine(LoadSceneAsync(gameSceneName));
    }
    
    // Load the menu scene
    public void LoadMenuScene()
    {
        StartCoroutine(LoadSceneAsync(menuSceneName));
    }
    
    // Load any scene by name
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    // Asynchronously load a scene with optional loading delay
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Optional loading delay for smoother transitions
        yield return new WaitForSeconds(loadingDelay);
        
        
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    
    /// Reload the current scene
    public void ReloadCurrentScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }
    
    /// Quit the application
    public void QuitGame()
    {
        Application.Quit();
    }
} 