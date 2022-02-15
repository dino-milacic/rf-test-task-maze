using System.Collections;

namespace RFTestTaskMaze
{
    public interface IMazeGenerator
    {
        IEnumerator GenerateMaze(IMazeManager mazeManager, int width, int height);
    }
}