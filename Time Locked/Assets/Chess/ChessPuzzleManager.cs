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

        PuzzleLine mainLine = new PuzzleLine();
        mainLine.description = "Mate in 3";
        mainLine.moves = new List<PuzzleMove>()
        {
            new PuzzleMove { from = "f5", to = "e4", description = "1. Şe4!" },
            new PuzzleMove { from = "d6", to = "f4", description = "1... Fxf4" },
            new PuzzleMove { from = "c4", to = "d6", description = "2. Ad6" },
            new PuzzleMove { from = "d6", to = "d6", description = "2... Fxd6" },
            new PuzzleMove { from = "g4", to = "d7", description = "3. Vxd7#" },
        };


        puzzleLines.Add(mainLine);
        currentLine = puzzleLines[0];
        currentMoveIndex = 0;
        isWhiteTurn = true;
    }

    public bool ValidateMove(string from, string to, PieceColor color)
    {
        if (currentLine == null || currentMoveIndex >= currentLine.moves.Count)
            return false;

        PuzzleMove expected = currentLine.moves[currentMoveIndex];

        if (from == expected.from && to == expected.to &&
            ((isWhiteTurn && color == PieceColor.White) || (!isWhiteTurn && color == PieceColor.Black)))
        {
            return true;
        }

        return false;
    }

    public void OnMoveExecuted(string from, string to, PieceColor color)
    {
        Debug.Log($"Hamle kabul edildi: {from} -> {to} ({color})");

        currentMoveIndex++;
        isWhiteTurn = !isWhiteTurn;

        Debug.Log($"Toplam hamle sayısı: {currentMoveIndex}");

        if (!isWhiteTurn && currentMoveIndex < currentLine.moves.Count)
        {
            StartCoroutine(PerformBlackMove());
        }
    }

    IEnumerator PerformBlackMove()
    {
        yield return new WaitForSeconds(0.8f);

        PuzzleMove move = currentLine.moves[currentMoveIndex];
        ChessPieceController piece = boardManager.GetPieceAt(move.from);

        if (piece != null && piece.pieceColor == PieceColor.Black)
        {
            piece.AutoMoveTo(move.to);
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
