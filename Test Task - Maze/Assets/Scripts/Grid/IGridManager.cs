using UnityEngine;

namespace RFTestTaskMaze
{
    public interface IGridManager
    {
        int Width { get; }
        int Height { get; }
        
        bool TryGetTile(int x, int y, out Tile tile);
        bool TryGetTile(Vector2Int position, out Tile tile);
        void HighlightSubfield(RectInt subfield);

        void SetCameraTarget(Vector2 targetPos);
        void SetCameraZoom(float ortSize);
    }
}