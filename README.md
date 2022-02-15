# RF Test Task #2: Maze

* **Unity version:** 2020.3.16f1

Generates a 100x100 tile maze (can be changed on the `MazeManager` game object) using the specified `MazeGenerator`, then waits for user input before starting the `MazeSolver`.

## Implementations used
* `MazeGenerator`: **Recursive Division** with a higher amount of doors per division wall dependant on wall length.
* `MazeSolver`: **Depth-First Search** with fake vision to check corridors before moving on.

## User input
* **Return/Enter:** Start the `MazeSolver` using a random starting point and 4 exit points placed at a random location on each outer wall of the maze.
* **Backspace:** Resets a `MazeSolver` solution once it has been found.
* **Right Mouse Button:** Place an exit point.
* **Left Mouse Button:** Place a starting point and start the `MazeSolver`. If no exits have been defined manually, will place 4 exit points at a random location on each outer wall of the maze.

## Tile Colors  
* `Gray`: Empty
* `Green`: Starting position
* `Magenta`: Exit
* `Red`: Visited by `MazeSolver`
* `Orange`: Looked at by `MazeSolver`
