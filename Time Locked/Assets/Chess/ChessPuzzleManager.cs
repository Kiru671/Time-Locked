using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChessPuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleMove
    {
        public string from;
        public string to;
        public string description;
    }

    [System.Serializable]
    public class PuzzleLine
    {
        public List<PuzzleMove> moves = new List<PuzzleMove>();
        public string description;
    }

    public List<PuzzleLine> puzzleLines = new List<PuzzleLine>();

    private int currentMoveIndex = 0;
    private PuzzleLine currentLine;
    private bool isWhiteTurn = true;

    private ChessBoardManager boardManager;

    void Start()
    {
        boardManager = FindObjectOfType<ChessBoardManager>();
        SetupPuzzle();
    }

    void SetupPuzzle()
    {
        puzzleLines.Clear();

        // Ana varyant: 1.Şe4! Fxf4 2.Ad6 Fxd6 3.Vxd7#
        PuzzleLine mainLine = new PuzzleLine();
        mainLine.description = "Mate in 3 - Ana varyant";
        mainLine.moves = new List<PuzzleMove>()
        {
            new PuzzleMove { from = "f5", to = "e4", description = "1. Şe4!" },
            new PuzzleMove { from = "d6", to = "f4", description = "1... Fxf4" },
            new PuzzleMove { from = "c4", to = "d6", description = "2. Ad6" },
            new PuzzleMove { from = "f4", to = "d6", description = "2... Fxd6" },
            new PuzzleMove { from = "g4", to = "d7", description = "3. Vxd7#" },
        };

        puzzleLines.Add(mainLine);
        currentLine = puzzleLines[0];
        currentMoveIndex = 0;
        isWhiteTurn = true;

        Debug.Log($"Puzzle kuruldu: {mainLine.moves.Count} hamle");
        foreach (var move in mainLine.moves)
        {
            Debug.Log($"  {move.description}: {move.from} -> {move.to}");
        }
    }

    public bool ValidateMove(string from, string to, PieceColor color)
    {
        if (currentLine == null || currentMoveIndex >= currentLine.moves.Count)
        {
            Debug.Log("Puzzle validation failed: No current line or moves exhausted");
            return false;
        }

        PuzzleMove expected = currentLine.moves[currentMoveIndex];

        Debug.Log($"Puzzle validation - Expected: {expected.from} -> {expected.to}, Got: {from} -> {to}");
        Debug.Log($"Expected color: {(isWhiteTurn ? "White" : "Black")}, Got color: {color}");
        Debug.Log($"Current move index: {currentMoveIndex}, Is white turn: {isWhiteTurn}");

        if (from == expected.from && to == expected.to &&
            ((isWhiteTurn && color == PieceColor.White) || (!isWhiteTurn && color == PieceColor.Black)))
        {
            Debug.Log("✓ Puzzle validation SUCCESS");
            return true;
        }

        Debug.Log("✗ Puzzle validation FAILED");
        return false;
    }

    public void OnMoveExecuted(string from, string to, PieceColor color)
    {
        Debug.Log($"Hamle kabul edildi: {from} -> {to} ({color})");

        currentMoveIndex++;
        isWhiteTurn = !isWhiteTurn;

        Debug.Log($"Toplam hamle sayısı: {currentMoveIndex}");

        // Mat kontrolü - tüm hamleler tamamlandı mı?
        if (currentMoveIndex >= currentLine.moves.Count)
        {
            Debug.Log("🎉 ===== PUZZLE TAMAMLANDI! MAT! ===== 🎉");
            Debug.Log($"Toplam {currentMoveIndex} hamle ile puzzle çözüldü!");
            Debug.Log("Tebrikler! Şah mat başarıyla gerçekleştirildi!");
            return;
        }

        if (!isWhiteTurn && currentMoveIndex < currentLine.moves.Count)
        {
            StartCoroutine(PerformBlackMove());
        }
    }

    IEnumerator PerformBlackMove()
    {
        yield return new WaitForSeconds(0.8f);

        PuzzleMove move = currentLine.moves[currentMoveIndex];
        Debug.Log($"Siyah hamle yapılıyor: {move.from} -> {move.to} (Index: {currentMoveIndex})");

        ChessPieceController piece = boardManager.GetPieceAt(move.from);

        if (piece != null && piece.pieceColor == PieceColor.Black)
        {
            piece.AutoMoveTo(move.to);
            Debug.Log($"Siyah hamle başlatıldı: {move.from} -> {move.to}");
        }
        else
        {
            Debug.LogError("Siyah taş bulunamadı veya renk uyumsuzluğu: " + move.from);
        }
    }

    public void OnWrongMove(ChessPieceController piece)
    {
        Debug.Log("Yanlış hamle, taş geri dönüyor.");
        piece.ReturnToOriginalPosition();
    }
}
