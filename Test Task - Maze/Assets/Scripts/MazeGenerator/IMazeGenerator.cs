using System.Collections;

namespace RFTestTaskMaze
{
    public interface IMazeGenerator
    {
        IEnumerator GenerateMaze(IGridManager gridManager, int width, int height);
    }
}