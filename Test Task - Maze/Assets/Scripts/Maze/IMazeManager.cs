using RFTestTaskMaze.MazeSolver;
using UnityEngine;

namespace RFTestTaskMaze
{
    public interface IMazeManager : IGameService
    {
        int Width { get; }
        int Height { get; }
        
        void SetMazeGenerator(IMazeGenerator generator);
        void SetMazeSolver(IMazeSolver solver);

        void CreateMaze();
        void SolveMaze();
        void SolveMaze(Vector2Int startingPosition);

        void ResetSolution();
        
        void SpecifyExit(Vector2Int exitPosition);
        bool TryGetTile(int x, int y, out Tile tile);
        bool TryGetTile(Vector2Int position, out Tile tile);
        void HighlightSubfield(RectInt subfield);
    }
}