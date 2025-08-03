using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject chessCamera;
    public string triggerObjectName = "Chess_Board";

    private bool gameStarted = false;
    private ChessBoardManager boardManager;

    void Start()
    {
        // Cursor baştan açık ve serbest kalsın
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Oyuncu kamerası açık, chess kamera kapalı başlasın
        mainCamera.SetActive(true);
        chessCamera.SetActive(false);
        
        // Board manager'ı bul
        boardManager = FindObjectOfType<ChessBoardManager>();
    }

    void Update()
    {
        if (!gameStarted && Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject.name == "Chess_Board")
                {
                    // Kamera geçişi yap
                    mainCamera.SetActive(false);
                    chessCamera.SetActive(true);

                    // Cursor yine açık kalsın
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    gameStarted = true;

                    Debug.Log("Satranç oyunu başladı!");

                    // Tüm taşlara kameralarını yeniden bulmaları için haber ver
                    ChessPieceController.RefreshAllCameras();

                    // Board manager'da oyunu başlat
                    if (boardManager != null)
                    {
                        Debug.Log("Board Manager bulundu ve oyun başlatıldı");
                    }
                }
            }
        }

        // ESC’ye bassan da cursor kaybolmasın
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


}
