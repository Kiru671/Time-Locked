using UnityEngine;
using System.Collections.Generic;

public class ChessBoardManager : MonoBehaviour
{
    [Header("Board Setup")]
    public Transform[] squares = new Transform[64]; // a1'den h8'e kadar
    
    private Dictionary<string, ChessPieceController> piecePositions = new Dictionary<string, ChessPieceController>();
    private Dictionary<string, ChessTile> squareTiles = new Dictionary<string, ChessTile>();
    
    void Start()
    {
        SetupBoard();
    }
    
    void SetupBoard()
    {
        // Tüm kareleri bul ve dictionary'e ekle
        GameObject[] squareObjects = GameObject.FindGameObjectsWithTag("Tile");
        
        foreach (GameObject square in squareObjects)
        {
            string squareName = square.name;
            ChessTile tile = square.GetComponent<ChessTile>();
            
            if (tile == null)
            {
                tile = square.AddComponent<ChessTile>();
            }
            
            squareTiles[squareName] = tile;
        }
        
        Debug.Log($"Board setup complete. Found {squareTiles.Count} squares");
    }
    
    public void RegisterPiece(ChessPieceController piece, string position)
    {
        if (piecePositions.ContainsKey(position))
        {
            Debug.LogWarning($"Position {position} is already occupied!");
            return;
        }
        
        piecePositions[position] = piece;
        Debug.Log($"Registered {piece.pieceType} at {position}");
    }
    
    public void MovePiece(string fromPosition, string toPosition, ChessPieceController piece)
    {
        // Eski pozisyonu temizle
        if (piecePositions.ContainsKey(fromPosition))
        {
            piecePositions.Remove(fromPosition);
        }
        
        // Hedef pozisyonda taş varsa onu al (capture)
        if (piecePositions.ContainsKey(toPosition))
        {
            ChessPieceController capturedPiece = piecePositions[toPosition];
            Debug.Log($"{piece.pieceType} captured {capturedPiece.pieceType} at {toPosition}");
            
            // Alınan taşı oyun dışına çıkar
            capturedPiece.gameObject.SetActive(false);
        }
        
        // Yeni pozisyonu kaydet
        piecePositions[toPosition] = piece;
        
        Debug.Log($"Moved {piece.pieceType} from {fromPosition} to {toPosition}");
    }
    
    public bool IsSquareOccupied(string position)
    {
        return piecePositions.ContainsKey(position);
    }
    
    public bool IsSquareOccupiedByOpponent(string position, PieceColor myColor)
    {
        if (!piecePositions.ContainsKey(position))
            return false;
            
        ChessPieceController piece = piecePositions[position];
        return piece.pieceColor != myColor;
    }
    
    public bool IsSquareOccupiedBySameColor(string position, PieceColor myColor)
    {
        if (!piecePositions.ContainsKey(position))
            return false;
            
        ChessPieceController piece = piecePositions[position];
        return piece.pieceColor == myColor;
    }
    
    public void HighlightSquares(List<string> positions)
    {
        ClearHighlights();
        
        foreach (string position in positions)
        {
            if (squareTiles.ContainsKey(position))
            {
                squareTiles[position].Highlight(true);
            }
        }
    }
    
    public void ClearHighlights()
    {
        foreach (var tile in squareTiles.Values)
        {
            tile.Highlight(false);
        }
    }
    
    // Debug: Tüm taş pozisyonlarını yazdır
    [ContextMenu("Print All Piece Positions")]
    public void PrintAllPositions()
    {
        foreach (var kvp in piecePositions)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.pieceType} ({kvp.Value.pieceColor})");
        }
    }
    
    public ChessPieceController GetPieceAt(string position)
    {
        if (piecePositions.ContainsKey(position))
            return piecePositions[position];
        return null;
    }
}
