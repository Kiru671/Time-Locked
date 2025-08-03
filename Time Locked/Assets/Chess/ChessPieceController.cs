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
    }    void CheckForMove()
    {
        Debug.Log("=== CheckForMove başladı ===");
        
        // Kamera kontrolü
        if (chessCamera == null)
        {
            Debug.LogError("Chess kamera bulunamadı!");
            return;
        }
        
        Ray ray = chessCamera.ScreenPointToRay(Input.mousePosition);
        
        // Raycast parametreleri
        float maxDistance = 100f;
        LayerMask layerMask = LayerMask.GetMask("BoardTrigger", "Tile"); // Tüm layerlar

        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask);
        
        Debug.Log($"Kamera: {chessCamera.name}");
        Debug.Log($"Mouse pozisyonu: {Input.mousePosition}");
        Debug.Log($"Ray origin: {ray.origin}");
        Debug.Log($"Ray direction: {ray.direction}");
        Debug.Log($"Toplam {hits.Length} objeye çarptı");
        
        if (hits.Length > 0)
        {
            // Tüm hit'leri mesafeye göre sırala
            System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
            
            // Tüm hit'leri listele
            for (int i = 0; i < hits.Length; i++)
            {
                Debug.Log($"Hit #{i}: {hits[i].collider.name}, Tag: {hits[i].collider.tag}, Distance: {hits[i].distance:F2}, Layer: {hits[i].collider.gameObject.layer}");
            }
            
            // Sadece Tile tag'ine sahip objeleri filtrele
            List<RaycastHit> tileHits = new List<RaycastHit>();
            
            foreach (RaycastHit hit in hits)
            {
                // Sadece Tile tag'ine sahip ve bu taş değil olan objeleri al
                if (hit.collider.CompareTag("Tile") && hit.collider.gameObject != this.gameObject)
                {
                    tileHits.Add(hit);
                    Debug.Log($"✓ Tile hit kabul edildi: {hit.collider.name}");
                }
                else
                {
                    Debug.Log($"✗ Tile hit reddedildi: {hit.collider.name} (Tag: {hit.collider.tag}, Bu taş mı: {hit.collider.gameObject == this.gameObject})");
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
                Debug.Log($"Seçilen hedef kare: {targetPosition}");
                
                // Bu kare geçerli bir hamle mi kontrol et
                if (validMoves.Contains(targetPosition))
                {
                    Debug.Log($"✓ Geçerli hamle: {currentPosition} -> {targetPosition}");
                    
                    // Puzzle manager'dan hamle doğruluğunu kontrol et
                    if (puzzleManager != null && !puzzleManager.ValidateMove(currentPosition, targetPosition, pieceColor))
                    {
                        Debug.Log("✗ Puzzle manager tarafından reddedildi");
                        // Yanlış hamle - taşı geri döndür
                        if (puzzleManager != null)
                            puzzleManager.OnWrongMove(this);
                        return;
                    }
                    
                    Debug.Log("✓ Puzzle manager onayladı, hamle yapılıyor...");
                    MoveTo(targetPosition, closestTileHit.collider.transform.position);
                }
                else
                {
                    Debug.Log($"✗ Geçersiz hamle: {targetPosition} (Geçerli olanlar: {string.Join(", ", validMoves)})");
                    DeselectPiece();
                }
            }
            else
            {
                Debug.Log("✗ Hiçbir geçerli Tile bulunamadı");
                DeselectPiece();
            }
        }
        else
        {
            Debug.Log("✗ Raycast hiçbir şeye çarpmadı - Alternatif yöntem deneniyor...");
            
            // Alternatif yöntem: Daha kısa mesafe ve tek raycast
            if (Physics.Raycast(ray, out RaycastHit singleHit, 50f))
            {
                Debug.Log($"Alternatif raycast çarptı: {singleHit.collider.name}, Tag: {singleHit.collider.tag}");
                
                if (singleHit.collider.CompareTag("Tile"))
                {
                    string targetPosition = singleHit.collider.gameObject.name;
                    Debug.Log($"Alternatif yöntemle hedef: {targetPosition}");
                    
                    if (validMoves.Contains(targetPosition))
                    {
                        Debug.Log($"✓ Alternatif yöntemle geçerli hamle: {currentPosition} -> {targetPosition}");
                        
                        if (puzzleManager != null && !puzzleManager.ValidateMove(currentPosition, targetPosition, pieceColor))
                        {
                            Debug.Log("✗ Alternatif yöntem - Puzzle manager reddetti");
                            if (puzzleManager != null)
                                puzzleManager.OnWrongMove(this);
                            return;
                        }
                        
                        MoveTo(targetPosition, singleHit.collider.transform.position);
                        return;
                    }
                }
            }
            
            // Son çare: Mouse pozisyonuna en yakın tile'ı bul
            Debug.Log("Son çare: En yakın tile aranıyor...");
            GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Tile");
            GameObject closestTile = null;
            float minScreenDistance = float.MaxValue;
            
            foreach (GameObject tile in allTiles)
            {
                Vector3 screenPos = chessCamera.WorldToScreenPoint(tile.transform.position);
                float screenDistance = Vector2.Distance(Input.mousePosition, new Vector2(screenPos.x, screenPos.y));
                
                if (screenDistance < minScreenDistance)
                {
                    minScreenDistance = screenDistance;
                    closestTile = tile;
                }
            }
            
            if (closestTile != null && minScreenDistance < 100f) // 100 pixel tolerans
            {
                string targetPosition = closestTile.name;
                Debug.Log($"En yakın tile bulundu: {targetPosition}, Mesafe: {minScreenDistance:F1}px");
                
                if (validMoves.Contains(targetPosition))
                {
                    Debug.Log($"✓ En yakın tile yöntemiyle geçerli hamle: {currentPosition} -> {targetPosition}");
                    
                    if (puzzleManager != null && !puzzleManager.ValidateMove(currentPosition, targetPosition, pieceColor))
                    {
                        Debug.Log("✗ En yakın tile yöntemi - Puzzle manager reddetti");
                        if (puzzleManager != null)
                            puzzleManager.OnWrongMove(this);
                        return;
                    }
                    
                    MoveTo(targetPosition, closestTile.transform.position);
                    return;
                }
            }
            
            DeselectPiece();
        }
        
        Debug.Log("=== CheckForMove bitti ===");
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
        
        Debug.Log($"=== {pieceType} ({pieceColor}) at {currentPosition} hamle hesaplaması ===");
        
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
        
        Debug.Log($"=== SONUÇ: {pieceType} için {validMoves.Count} geçerli hamle: {string.Join(", ", validMoves)} ===");
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
    if (boardManager == null) return;

    List<string> allMoves = new List<string>();

    CalculateRookMoves();
    allMoves.AddRange(validMoves);

    validMoves.Clear();

    CalculateBishopMoves();
    allMoves.AddRange(validMoves);

    validMoves = allMoves;
}

    
    void CalculateKingMoves()
    {
        if (boardManager == null) return;
        
        int file = currentPosition[0] - 'a';
        int rank = int.Parse(currentPosition[1].ToString()) - 1;
        
        Debug.Log($"Şah {currentPosition} için hamle hesaplama: file={file}, rank={rank}");
        
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
                        Debug.Log($"Şah geçerli hamle eklendi: {targetSquare}");
                    }
                    else
                    {
                        Debug.Log($"Şah kendi taşı var: {targetSquare}");
                    }
                }
                else
                {
                    Debug.Log($"Şah tahtadan çıkıyor: file={newFile}, rank={newRank}");
                }
            }
        }
        
        Debug.Log($"Şah toplam {validMoves.Count} geçerli hamle: {string.Join(", ", validMoves)}");
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
