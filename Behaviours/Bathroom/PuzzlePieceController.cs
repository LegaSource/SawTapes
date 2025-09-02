using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SawTapes.Behaviours.Bathroom;

public class PuzzlePieceController : MonoBehaviour, IPointerClickHandler
{
    public int index;
    public int originalIndex; // Position correcte
    public bool isEmpty = false;

    public static int gridSize;
    public static Vector2[] gridPositions;
    public static bool[] occupiedSlots;
    public static PuzzlePieceController selectedPiece;

    private RectTransform rectTransform;
    private Image img;
    private Color baseColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        baseColor = img.color;
    }

    public void SetEmpty(bool enable)
    {
        isEmpty = enable;

        if (enable)
        {
            img.sprite = null;
            img.color = new Color(0, 0, 0, 0);
        }
        else
        {
            img.color = Color.white;
        }

        if (occupiedSlots != null && index < occupiedSlots.Length)
            occupiedSlots[index] = !enable;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEmpty && selectedPiece == null) return;

        if (selectedPiece == null)
        {
            if (isEmpty) return;
            selectedPiece = this;
            Highlight(true);
            return;
        }

        if (selectedPiece == this)
        {
            Highlight(false);
            selectedPiece = null;
            return;
        }

        int[] directions = [-1, 1, -gridSize, gridSize];
        foreach (int dir in directions)
        {
            int neighborIndex = selectedPiece.index + dir;

            if (neighborIndex < 0 || neighborIndex >= gridSize * gridSize)
                continue;

            if ((dir == -1 || dir == 1) &&
                selectedPiece.index / gridSize != neighborIndex / gridSize)
            {
                continue;
            }

            if (neighborIndex == index && isEmpty)
            {
                SwapPieces(selectedPiece, this);
                break;
            }
        }

        selectedPiece.Highlight(false);
        selectedPiece = null;
    }

    private void SwapPieces(PuzzlePieceController pieceA, PuzzlePieceController emptyB)
    {
        occupiedSlots[pieceA.index] = false;
        occupiedSlots[emptyB.index] = true;

        (emptyB.index, pieceA.index) = (pieceA.index, emptyB.index);

        Vector2 posA = gridPositions[pieceA.index];
        Vector2 posB = gridPositions[emptyB.index];

        pieceA.rectTransform.anchoredPosition = posA;
        emptyB.rectTransform.anchoredPosition = posB;

        pieceA.SetEmpty(false);
        emptyB.SetEmpty(true);
        CheckVictory();
    }

    private void Highlight(bool on)
        => img.color = on ? Color.yellow : baseColor;

    private void CheckVictory()
    {
        foreach (PuzzlePieceController piece in FindObjectsOfType<PuzzlePieceController>())
        {
            if (piece.isEmpty) continue;
            if (piece.index != piece.originalIndex) return;
        }

        if (SawTapes.bathroom.isRightSpot) SawTapes.bathroom.SpawnSawKeyServerRpc();
        SawTapes.bathroom.OpenPuzzleBoard(false);
        Destroy(transform.parent.gameObject);
    }
}