using UnityEngine;
using System.Collections.Generic;

public enum PieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public enum PieceColor
{
    White,
    Black
}

[System.Serializable]
public class ChessSquare
{
    public string position; // a1, a2, etc.
    public Transform square;
    public bool isOccupied;
    public ChessPieceController occupyingPiece;
}

public class ChessPieceController : MonoBehaviour
{
    [Header("Piece Settings")]
    public PieceType pieceType;
    public PieceColor pieceColor;
    public string currentPosition = "a1"; // başlangıç pozisyonu
    
    [Header("Visual Settings")]
    public float hoverHeight = 0.2f; // 0.5'ten 0.2'ye düşür
    public float moveSpeed = 5f;
    
    private Vector3 originalPosition;
    private bool isSelected = false;
    private bool isMoving = false;
    private Camera chessCamera;
    private ChessBoardManager boardManager;
    private ChessPuzzleManager puzzleManager;
    private List<string> validMoves = new List<string>();
    private float lastClickTime = 0f;
    private float clickCooldown = 0.1f; // 100ms cooldown
    
    void Start()
    {
        originalPosition = transform.position;
        
        // Chess kamerasını bul
        GameObject chessCamObj = GameObject.Find("chessCamera");
        if (chessCamObj != null)
            chessCamera = chessCamObj.GetComponent<Camera>();
        else
            chessCamera = Camera.main;
            
        // Kamera kontrolü
        if (chessCamera == null)
        {
            Debug.LogError("Kamera bulunamadı! Chess camera veya main camera gerekli.");
            chessCamera = FindObjectOfType<Camera>(); // Herhangi bir kamera al
        }
        
        Debug.Log($"Taş {gameObject.name} kamera olarak {chessCamera.name} kullanıyor");
            
        // Board manager'ı bul
        boardManager = FindObjectOfType<ChessBoardManager>();
        
        // Puzzle manager'ı bul
        puzzleManager = FindObjectOfType<ChessPuzzleManager>();
        
        // Bu taşın pozisyonunu board manager'a kaydet
        if (boardManager != null)
            boardManager.RegisterPiece(this, currentPosition);
    }
    
    void OnMouseDown()
    {
        if (isMoving) return;
        
        if (isSelected)
        {
            DeselectPiece();
        }
        else
        {
            SelectPiece();
        }
    }
    
    void Update()
    {
        if (isSelected && !isMoving && Input.GetMouseButtonDown(0))
        {
            // Cooldown kontrol et
            if (Time.time - lastClickTime > clickCooldown)
            {
                CheckForMove();
            }
            else
            {
                // Cooldown aktif
            }
        }
    }
    
    void CheckForMove()
    {
        Ray ray = chessCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
        
        if (hits.Length > 0)
        {
            // Sadece Tile tag'ine sahip objeleri filtrele
            List<RaycastHit> tileHits = new List<RaycastHit>();
            
            foreach (RaycastHit hit in hits)
            {
                // Sadece Tile tag'ine sahip ve bu taş değil olan objeleri al
                if (hit.collider.CompareTag("Tile") && hit.collider.gameObject != this.gameObject)
                {
                    tileHits.Add(hit);
                }
            }
            
            if (tileHits.Count > 0)
            {
                // En yakın Tile'ı al
                RaycastHit closestTileHit = tileHits[0];
                float closestDistance = closestTileHit.distance;
                
                foreach (RaycastHit hit in tileHits)
                {
                    if (hit.distance < closestDistance)
                    {
                        closestTileHit = hit;
                        closestDistance = hit.distance;
                    }
                }
                
                string targetPosition = closestTileHit.collider.gameObject.name;
                
                // Bu kare geçerli bir hamle mi kontrol et
                if (validMoves.Contains(targetPosition))
                {
                    // Puzzle manager'dan hamle doğruluğunu kontrol et
                    if (puzzleManager != null && !puzzleManager.ValidateMove(currentPosition, targetPosition, pieceColor))
                    {
                        // Yanlış hamle - taşı geri döndür
                        if (puzzleManager != null)
                            puzzleManager.OnWrongMove(this);
                        return;
                    }
                    
                    MoveTo(targetPosition, closestTileHit.collider.transform.position);
                }
                else
                {
                    DeselectPiece();
                }
            }
            else
            {
                DeselectPiece();
            }
        }
        else
        {
            DeselectPiece();
        }
    }
    
    void SelectPiece()
    {
        isSelected = true;
        lastClickTime = Time.time; // Son tıklama zamanını kaydet
        
        // Taşı yukarı kaldır
        Vector3 targetPos = originalPosition + new Vector3(0, hoverHeight, 0);
        StartCoroutine(SmoothMove(transform.position, targetPos, 0.2f));
        
        // Geçerli hamleleri hesapla
        CalculateValidMoves();
        
        // Geçerli hamleleri görsel olarak göster
        if (boardManager != null)
            boardManager.HighlightSquares(validMoves);
    }
    
    void DeselectPiece()
    {
        isSelected = false;
        
        // Taşı normal yüksekliğe indir - originalPosition'ı kullan
        Vector3 targetPos = originalPosition;
        StartCoroutine(SmoothMove(transform.position, targetPos, 0.2f));
        
        // Tüm highlight'ları kapat
        if (boardManager != null)
            boardManager.ClearHighlights();
    }
    
    void MoveTo(string newPosition, Vector3 worldPosition)
    {
        isMoving = true;
        
        // Board manager'da pozisyon güncelle
        if (boardManager != null)
            boardManager.MovePiece(currentPosition, newPosition, this);
        
        // Pozisyonu güncelle
        string oldPosition = currentPosition;
        currentPosition = newPosition;
        
        // Yeni pozisyonu hesapla - worldPosition'ın Y'sini kullan
        Vector3 newTargetPos = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
        
        // ÖNCE hareket et, SONRA originalPosition'ı güncelle
        StartCoroutine(SmoothMove(transform.position, newTargetPos, 0.5f, () => {
            // Hareket tamamlandıktan SONRA originalPosition'ı güncelle
            originalPosition = newTargetPos;
            
            isMoving = false;
            DeselectPiece();
            
            // Puzzle manager'a hamleyi bildir
            if (puzzleManager != null)
                puzzleManager.OnMoveExecuted(oldPosition, newPosition, pieceColor);
        }));
    }
    
    void CalculateValidMoves()
    {
        validMoves.Clear();
        
        switch (pieceType)
        {
            case PieceType.Pawn:
                CalculatePawnMoves();
                break;
            case PieceType.Rook:
                CalculateRookMoves();
                break;
            case PieceType.Knight:
                CalculateKnightMoves();
                break;
            case PieceType.Bishop:
                CalculateBishopMoves();
                break;
            case PieceType.Queen:
                CalculateQueenMoves();
                break;
            case PieceType.King:
                CalculateKingMoves();
                break;
        }
        
        // CalculateValidMoves sonucu log kaldırıldı
    }
    
    void CalculatePawnMoves()
    {
        if (boardManager == null) return;
        
        int file = currentPosition[0] - 'a'; // 0-7
        int rank = int.Parse(currentPosition[1].ToString()) - 1; // 0-7
        
        int direction = (pieceColor == PieceColor.White) ? 1 : -1;
        
        // Bir kare ileri
        int newRank = rank + direction;
        if (newRank >= 0 && newRank <= 7)
        {
            string frontSquare = $"{(char)('a' + file)}{newRank + 1}";
            if (!boardManager.IsSquareOccupied(frontSquare))
            {
                validMoves.Add(frontSquare);
                
                // İlk hamle ise iki kare ileri de kontrol et
                bool isStartingPosition = (pieceColor == PieceColor.White && rank == 1) || 
                                        (pieceColor == PieceColor.Black && rank == 6);
                
                if (isStartingPosition)
                {
                    newRank += direction;
                    if (newRank >= 0 && newRank <= 7)
                    {
                        string twoSquaresFront = $"{(char)('a' + file)}{newRank + 1}";
                        if (!boardManager.IsSquareOccupied(twoSquaresFront))
                        {
                            validMoves.Add(twoSquaresFront);
                        }
                    }
                }
            }
        }
        
        // Çapraz saldırı (sadece düşman taş varsa)
        for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
        {
            int newFile = file + fileOffset;
            newRank = rank + direction;
            
            if (newFile >= 0 && newFile <= 7 && newRank >= 0 && newRank <= 7)
            {
                string diagonalSquare = $"{(char)('a' + newFile)}{newRank + 1}";
                if (boardManager.IsSquareOccupiedByOpponent(diagonalSquare, pieceColor))
                {
                    validMoves.Add(diagonalSquare);
                }
            }
        }
        
        // Piyon hamle detayı log kaldırıldı
    }
    
    void CalculateRookMoves()
    {
        if (boardManager == null) return;
        
        int file = currentPosition[0] - 'a';
        int rank = int.Parse(currentPosition[1].ToString()) - 1;
        
        // Dört yön: yukarı, aşağı, sağa, sola
        int[,] directions = {{0,1}, {0,-1}, {1,0}, {-1,0}};
        
        for (int d = 0; d < 4; d++)
        {
            int fileDir = directions[d,0];
            int rankDir = directions[d,1];
            
            for (int i = 1; i < 8; i++)
            {
                int newFile = file + (fileDir * i);
                int newRank = rank + (rankDir * i);
                
                if (newFile < 0 || newFile > 7 || newRank < 0 || newRank > 7)
                    break;
                
                string targetSquare = $"{(char)('a' + newFile)}{newRank + 1}";
                
                if (boardManager.IsSquareOccupied(targetSquare))
                {
                    if (boardManager.IsSquareOccupiedByOpponent(targetSquare, pieceColor))
                        validMoves.Add(targetSquare); // Düşman taşını yiyebilir
                    break; // Engel var, daha ileriye gidemez
                }
                
                validMoves.Add(targetSquare);
            }
        }
    }
    
    void CalculateKnightMoves()
    {
        if (boardManager == null) return;
        
        int file = currentPosition[0] - 'a';
        int rank = int.Parse(currentPosition[1].ToString()) - 1;
        
        // L şeklindeki 8 hamle
        int[,] knightMoves = {{2,1}, {2,-1}, {-2,1}, {-2,-1}, {1,2}, {1,-2}, {-1,2}, {-1,-2}};
        
        for (int i = 0; i < 8; i++)
        {
            int newFile = file + knightMoves[i,0];
            int newRank = rank + knightMoves[i,1];
            
            if (newFile >= 0 && newFile <= 7 && newRank >= 0 && newRank <= 7)
            {
                string targetSquare = $"{(char)('a' + newFile)}{newRank + 1}";
                
                if (!boardManager.IsSquareOccupiedBySameColor(targetSquare, pieceColor))
                {
                    validMoves.Add(targetSquare);
                }
            }
        }
    }
    
    void CalculateBishopMoves()
    {
        if (boardManager == null) return;
        
        int file = currentPosition[0] - 'a';
        int rank = int.Parse(currentPosition[1].ToString()) - 1;
        
        // Dört çapraz yön
        int[,] directions = {{1,1}, {1,-1}, {-1,1}, {-1,-1}};
        
        for (int d = 0; d < 4; d++)
        {
            int fileDir = directions[d,0];
            int rankDir = directions[d,1];
            
            for (int i = 1; i < 8; i++)
            {
                int newFile = file + (fileDir * i);
                int newRank = rank + (rankDir * i);
                
                if (newFile < 0 || newFile > 7 || newRank < 0 || newRank > 7)
                    break;
                
                string targetSquare = $"{(char)('a' + newFile)}{newRank + 1}";
                
                if (boardManager.IsSquareOccupied(targetSquare))
                {
                    if (boardManager.IsSquareOccupiedByOpponent(targetSquare, pieceColor))
                        validMoves.Add(targetSquare);
                    break;
                }
                
                validMoves.Add(targetSquare);
            }
        }
    }
    
    void CalculateQueenMoves()
    {
        // Vezir = Kale + Fil
        CalculateRookMoves();
        List<string> rookMoves = new List<string>(validMoves);
        validMoves.Clear();
        CalculateBishopMoves();
        validMoves.AddRange(rookMoves);
    }
    
    void CalculateKingMoves()
    {
        if (boardManager == null) return;
        
        int file = currentPosition[0] - 'a';
        int rank = int.Parse(currentPosition[1].ToString()) - 1;
        
        // 8 yön (1 kare)
        for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
        {
            for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
            {
                if (fileOffset == 0 && rankOffset == 0) continue;
                
                int newFile = file + fileOffset;
                int newRank = rank + rankOffset;
                
                if (newFile >= 0 && newFile <= 7 && newRank >= 0 && newRank <= 7)
                {
                    string targetSquare = $"{(char)('a' + newFile)}{newRank + 1}";
                    
                    if (!boardManager.IsSquareOccupiedBySameColor(targetSquare, pieceColor))
                    {
                        validMoves.Add(targetSquare);
                    }
                }
            }
        }
    }
    
    System.Collections.IEnumerator SmoothMove(Vector3 from, Vector3 to, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth curve
            t = Mathf.SmoothStep(0f, 1f, t);
            
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        
        transform.position = to;
        onComplete?.Invoke();
    }
    
    public void AutoMoveTo(string newPosition)
    {
        // Otomatik hamle için (siyah taşlar)
        if (boardManager == null) return;
        
        GameObject targetSquare = GameObject.Find(newPosition);
        if (targetSquare == null) 
        {
            Debug.LogError($"Hedef kare bulunamadı: {newPosition}");
            return;
        }
        
        isMoving = true;
        string oldPosition = currentPosition;
        
        // Board manager'da pozisyon güncelle
        boardManager.MovePiece(currentPosition, newPosition, this);
        
        currentPosition = newPosition;
        originalPosition = new Vector3(targetSquare.transform.position.x, originalPosition.y, targetSquare.transform.position.z);
        
        Vector3 targetPos = originalPosition;
        StartCoroutine(SmoothMove(transform.position, targetPos, 0.8f, () => {
            isMoving = false;
            
            // Puzzle manager'a hamleyi bildir
            if (puzzleManager != null)
                puzzleManager.OnMoveExecuted(oldPosition, newPosition, pieceColor);
        }));
    }
    
    public void ReturnToOriginalPosition()
    {
        // Yanlış hamle sonrası geri dönme
        isMoving = true;
        DeselectPiece();
        
        StartCoroutine(SmoothMove(transform.position, originalPosition, 0.3f, () => {
            isMoving = false;
        }));
    }
}
