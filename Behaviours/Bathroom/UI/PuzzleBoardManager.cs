using LegaFusionCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SawTapes.Behaviours.Bathroom.UI;

public class PuzzleBoardManager : MonoBehaviour
{
    public Texture2D puzzleTexture;
    public RectTransform gameTransform;
    public int size = 3;
    public int amountEmptyPieces = 2;

    public List<RectTransform> pieces = [];
    public List<int> emptyIndexes = [];

    private void Start()
    {
        int total = size * size;
        float texWidth = puzzleTexture.width / (float)size;
        float texHeight = puzzleTexture.height / (float)size;

        // Ordre d'affichage mélangé
        List<int> indexes = Enumerable.Range(0, total - amountEmptyPieces).ToList();
        LFCUtilities.Shuffle(indexes);
        // Emplacements vides sur les derniers index
        for (int i = 1; i <= amountEmptyPieces; i++) emptyIndexes.Add(total - i);

        // Données partagées
        PuzzlePieceController.gridSize = size;
        PuzzlePieceController.gridPositions = new Vector2[total];
        PuzzlePieceController.occupiedSlots = new bool[total];

        // Taille visuelle
        float pieceSize = 100f;
        gameTransform.sizeDelta = new Vector2(size * pieceSize, size * pieceSize);
        gameTransform.anchoredPosition = Vector2.zero;

        for (int i = 0; i < total; i++)
        {
            GameObject pieceObj = Instantiate(SawTapes.puzzlePiecePrefab, gameTransform);
            RectTransform piece = pieceObj.GetComponent<RectTransform>();
            piece.sizeDelta = new Vector2(pieceSize, pieceSize);
            piece.name = $"Piece_{i}";

            Image img = piece.GetComponent<Image>();

            int gridRow = size - 1 - (i / size); // Inverser l’ordre des lignes
            int gridCol = i % size;

            float x = gridCol * pieceSize;
            float y = -(size - 1 - gridRow) * pieceSize;

            float offsetX = -(size - 1) * pieceSize / 2f;
            float offsetY = (size - 1) * pieceSize / 2f;

            Vector2 anchoredPos = new Vector2(x + offsetX, y + offsetY);
            piece.anchoredPosition = anchoredPos;

            PuzzlePieceController.gridPositions[i] = anchoredPos;

            PuzzlePieceController controller = piece.GetComponent<PuzzlePieceController>();
            int displayIndex = indexes.Count > i ? indexes[i] : i;
            controller.index = i; // position actuelle
            controller.originalIndex = displayIndex; // position correcte (celle correspondant à l'image)

            bool isEmpty = emptyIndexes.Contains(i);
            controller.SetEmpty(isEmpty);

            if (!isEmpty)
            {
                int row = displayIndex / size;
                int col = displayIndex % size;
                int invertedRow = size - 1 - row;

                Rect rect = new Rect(col * texWidth, invertedRow * texHeight, texWidth, texHeight);
                Sprite sprite = Sprite.Create(puzzleTexture, rect, new Vector2(0.5f, 0.5f));
                img.sprite = sprite;
            }

            pieces.Add(piece);
        }
    }
}