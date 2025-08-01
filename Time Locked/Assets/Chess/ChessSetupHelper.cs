using UnityEngine;

public class ChessSetupHelper : MonoBehaviour
{
    [Header("Setup")]
    public bool setupOnStart = true;
    
    [ContextMenu("Setup Chess Pieces")]
    public void SetupChessPieces()
    {
        // Tüm taşları bul
        ChessPieceController[] allPieces = FindObjectsOfType<ChessPieceController>();
        
        foreach (ChessPieceController piece in allPieces)
        {
            // Taş isminden pozisyon çıkarmaya çalış
            string pieceName = piece.gameObject.name.ToLower();
            
            // Örnek: "whiteRook_a1" -> a1 pozisyonu
            if (pieceName.Contains("_"))
            {
                string[] parts = pieceName.Split('_');
                if (parts.Length > 1)
                {
                    string position = parts[parts.Length - 1]; // Son kısmı al
                    
                    if (IsValidPosition(position))
                    {
                        piece.currentPosition = position;
                        Debug.Log($"{piece.gameObject.name} set to position {position}");
                    }
                }
            }
            
            // Taş tipini isimden çıkarmaya çalış
            if (pieceName.Contains("pawn"))
                piece.pieceType = PieceType.Pawn;
            else if (pieceName.Contains("rook"))
                piece.pieceType = PieceType.Rook;
            else if (pieceName.Contains("knight"))
                piece.pieceType = PieceType.Knight;
            else if (pieceName.Contains("bishop"))
                piece.pieceType = PieceType.Bishop;
            else if (pieceName.Contains("queen"))
                piece.pieceType = PieceType.Queen;
            else if (pieceName.Contains("king"))
                piece.pieceType = PieceType.King;
                
            // Rengi isimden çıkarmaya çalış
            if (pieceName.Contains("white"))
                piece.pieceColor = PieceColor.White;
            else if (pieceName.Contains("black"))
                piece.pieceColor = PieceColor.Black;
        }
        
        Debug.Log($"Setup completed for {allPieces.Length} pieces");
    }
    
    bool IsValidPosition(string position)
    {
        if (position.Length != 2) return false;
        
        char file = position[0];
        char rank = position[1];
        
        return (file >= 'a' && file <= 'h') && (rank >= '1' && rank <= '8');
    }
    
    [ContextMenu("Set Current Positions From World")]
    public void SetCurrentPositionsFromWorld()
    {
        // Taşların mevcut Unity dünya pozisyonlarından satranç pozisyonlarını hesapla
        ChessPieceController[] allPieces = FindObjectsOfType<ChessPieceController>();
        GameObject[] squares = GameObject.FindGameObjectsWithTag("Tile");
        
        foreach (ChessPieceController piece in allPieces)
        {
            // En yakın kareyi bul
            float minDistance = float.MaxValue;
            string closestSquare = "a1";
            
            foreach (GameObject square in squares)
            {
                float distance = Vector3.Distance(piece.transform.position, square.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSquare = square.name;
                }
            }
            
            // Pozisyonu güncelle
            piece.currentPosition = closestSquare;
            Debug.Log($"{piece.gameObject.name} is closest to {closestSquare}");
        }
        
        Debug.Log("Positions set from world positions!");
    }
    
    [ContextMenu("Fix Square Tags")]
    public void FixSquareTags()
    {
        // Tüm kareleri bul ve tag'lerini düzelt
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            string name = obj.name.ToLower();
            
            // Satranç karesi gibi görünen isimleri kontrol et
            if (IsValidPosition(name) || 
                name.Contains("square") || 
                name.Contains("tile") || 
                name.Contains("chess") ||
                (name.Length == 2 && char.IsLetter(name[0]) && char.IsDigit(name[1])))
            {
                obj.tag = "Tile";
                fixedCount++;
                Debug.Log($"Fixed tag for: {obj.name}");
            }
        }
        
        Debug.Log($"Fixed {fixedCount} square tags!");
    }
    
    [ContextMenu("Snap Pieces to Squares")]
    public void SnapPiecesToSquares()
    {
        // Taşları en yakın karelere yapıştır
        ChessPieceController[] allPieces = FindObjectsOfType<ChessPieceController>();
        GameObject[] squares = GameObject.FindGameObjectsWithTag("Tile");
        
        foreach (ChessPieceController piece in allPieces)
        {
            // En yakın kareyi bul
            float minDistance = float.MaxValue;
            GameObject closestSquare = null;
            
            foreach (GameObject square in squares)
            {
                float distance = Vector3.Distance(piece.transform.position, square.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSquare = square;
                }
            }
            
            if (closestSquare != null)
            {
                // Taşı kareye yapıştır (Y pozisyonunu koru)
                Vector3 targetPos = closestSquare.transform.position;
                piece.transform.position = new Vector3(targetPos.x, piece.transform.position.y, targetPos.z);
                piece.currentPosition = closestSquare.name;
            }
        }
        
        Debug.Log("Pieces snapped to nearest squares!");
    }
    
    void Start()
    {
        if (setupOnStart)
        {
            // Biraz bekle ki diğer scriptler start olsun
            Invoke("SetupChessPieces", 0.1f);
            // Dünya pozisyonlarından satranç pozisyonlarını ayarla
            Invoke("SetCurrentPositionsFromWorld", 0.2f);
        }
    }
}
