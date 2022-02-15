using System.Collections;
using UnityEngine;

namespace RFTestTaskMaze.MazeSolver
{
    public interface IMazeSolver
    {
        IEnumerator SolveMaze(IGridManager gridManager, Vector2Int startingPosition);
    }
}