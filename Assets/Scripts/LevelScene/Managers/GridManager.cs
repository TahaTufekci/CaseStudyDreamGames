using System;
using System.Collections;
using System.Collections.Generic;
using LevelScene.Grid;
using LevelScene.Helpers;
using LevelScene.Obstacles;
using Obstacles;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelScene.Managers
{
    public class GridManager : MonoBehaviour
    {
        #region Lists

        private List<Tile> controlList = new List<Tile>(); //List of tiles to check the neighbours
        [SerializeField] private List<Tile> tiles = new List<Tile>(); //List of all tiles that we created
        public List<Tile> Tiles => tiles;
        private HashSet<Tile> activeTileList = new HashSet<Tile>(); //List of all neighbours including the tile itself
        private HashSet<Tile> obstacleList = new HashSet<Tile>(); //List of all neighbours including the tile itself
        private List<Tile> tempList = new List<Tile>();
        private List<Tile> nearbyTiles = new List<Tile>();
        #endregion

        private LevelReader _levelReader;
        private int _currentMoveCount;
        private int _controlIndex; // Index of the tile that should check the neighbours
        [SerializeField] private GridBoard gridBoard;
        private Level _currentLevel;

      
        // Start is called before the first frame update
        void Start()
        {
            LevelManager.instance.InitializeLevel();
            _currentLevel = LevelManager.instance.currentLevel;
            GameManager.instance.SetCurrentMoveCount(LevelManager.instance.currentLevel.move_count);
            InitilazeGrid();
        }

        public void InitilazeGrid()
        {
            gridBoard.CalculateTileScale(_currentLevel);
            tiles = gridBoard.GenerateTiles(_currentLevel);
            CheckNeighboursAndDeadlock();
        }
        private void TileClicked(Tile tile)
        {
            tile.SetNeighbours(CheckBoard(tile));
            SetObstacleList(tile);
            if (tile.Neighbours.Count >= 2) // If there is more than 2 neighbours, destroy them
            {
                GameManager.instance.OnValidTap?.Invoke();
                DestroyNeighboursAndRefill(tile);
            }
        }
          private void CheckTileGeneration()
        {
            for (int i = 0; i < _currentLevel.grid_height; i++)
            {
                for (int j = 0; j < _currentLevel.grid_width; j++)
                {
                    if(GetTile(j,i) == null) GenerateTileAtColumn(j,i); // Generate new tiles using the column info of destroyed tiles
                }
            }
        }
       private void GenerateTileAtColumn(int x, int y)
       {
           tempList = GetColumn(x);
           int checkIndex = y + 1;
           while (checkIndex < _currentLevel.grid_height && tempList[checkIndex] == null)
           {
               checkIndex++;
           }

           if ( checkIndex < _currentLevel.grid_height && tempList[checkIndex] != null)
           {
               if (MoveTile(x, y, checkIndex)) return;
           }

           if (checkIndex == _currentLevel.grid_height)
           {
               gridBoard.GenerateTileAtTop(x);
           }
           tempList.Clear();
       }
   
       private bool MoveTile(int x, int y, int checkIndex)
       {
           Tile tileAbove = GetTile(x, checkIndex).GetComponent<Tile>();
           if (tileAbove.TileType is TileType.Box or TileType.Stone) return true;
           Vector3 targetPos = new Vector3((x * gridBoard.NewTileWidth) - gridBoard.SpawnPointXOffset,
               (y * gridBoard.NewTileHeight) - gridBoard.SpawnPointYOffset,
               tileAbove.transform.position.z);
           tileAbove.MoveToTarget(targetPos);
           int removedTileIndex = tiles.IndexOf(tileAbove);
           tiles.Remove(tileAbove);
           tileAbove.SetCoordinates(x, y);
           SetTile(tileAbove, x, y);
           tiles.Insert(removedTileIndex, null);
           return false;
       }

      
       public int FindIndexOfLowestNull(int x)
       {
           int lowestNull = 99;

           for (int y = _currentLevel.grid_height - 1; y >= 0; y--)
           {
               Tile currentTile = GetTile(x, y);

               if (currentTile != null)
               {
                   return (y + 1);
               }
               else
               {
                   lowestNull = y;
               }
           }
           return lowestNull;
       }
       private void DestroyNeighboursAndRefill(Tile clickedTile)
        {
            foreach (Tile tile in clickedTile.Neighbours)
            {
                TryToDestroyObstaclesAndRefill(tile);
                int removedTileIndex = tiles.IndexOf(tile);
                tiles[removedTileIndex] = null;
                DestroyTile(tile);
            }

            if (clickedTile.Neighbours.Count >= 5)
            {
                gridBoard.CreateTnt(clickedTile);
            }
            CheckTileGeneration();
            CheckNeighboursAndDeadlock();
        }
       private void TryToDestroyObstaclesAndRefill(Tile neighbour)
       {
           foreach (Tile tile in neighbour.ObstacleNeighbours)
           {
               IObstacles obstacle = tile.GetComponent<IObstacles>(); // Get the IObstacles component
               if (obstacle != null && obstacle is not Stone)
               {
                   GameManager.instance.OnObstacleDamage?.Invoke(tile);

                   if (obstacle.Health == 0)
                   {
                       int removedTileIndex = tiles.IndexOf(tile);
                       tiles[removedTileIndex] = null;
                       GameManager.instance.OnObstacleDestroy?.Invoke(obstacle);
                       tile.gameObject.SetActive(false);
                   }
               }
           }
       }
       private void DestroyWithTnt(Tile clickedTnt, int effectedAreaSize)
        {
            GameManager.instance.OnValidTap?.Invoke();

            HashSet<Tile> affectedTiles = new HashSet<Tile>(); // Collection to store affected tiles

            // Perform TNT explosion and collect affected tiles
            GetAffectedTiles(clickedTnt, effectedAreaSize, affectedTiles);

            // Process affected tiles
            foreach (Tile affectedTile in affectedTiles)
            {
                IObstacles obstacle = affectedTile.GetComponent<IObstacles>(); // Get the IObstacles component

                if (obstacle != null)
                {
                    GameManager.instance.OnObstacleDamage?.Invoke(affectedTile);

                    if (obstacle.Health == 0)
                    {
                        int removedTileIndex = tiles.IndexOf(affectedTile);
                        tiles[removedTileIndex] = null;
                        GameManager.instance.OnObstacleDestroy?.Invoke(obstacle);
                        affectedTile.gameObject.SetActive(false);
                    }
                }
                else
                {
                    int removedTileIndex = tiles.IndexOf(affectedTile);
                    tiles[removedTileIndex] = null;
                    DestroyTile(affectedTile);
                }
            }

            CheckTileGeneration();
            CheckNeighboursAndDeadlock();
        }
       private void GetAffectedTiles(Tile clickedTnt, int effectedAreaSize, HashSet<Tile> affectedTiles)
        {
            Queue<Tile> queue = new Queue<Tile>(); // Queue for BFS traversal
            HashSet<Tile> visited = new HashSet<Tile>(); // Set to track visited tiles

            queue.Enqueue(clickedTnt); // Enqueue the clicked TNT tile
            visited.Add(clickedTnt); // Mark the clicked TNT tile as visited

            while (queue.Count > 0)
            {
                Tile currentTile = queue.Dequeue(); // Dequeue the current tile

                // Add the current tile to the affected tiles collection
                affectedTiles.Add(currentTile);

                // Check if the current tile is a TNT and if it's in the affectedTiles set
                if (currentTile.TileType == TileType.Tnt && affectedTiles.Contains(currentTile))
                {
                    // Get nearby tiles within the explosion radius using GetNearbyCells function
                    List<Tile> nearbyTiles = GetNearbyTiles(currentTile, effectedAreaSize);

                    foreach (Tile nearbyTile in nearbyTiles)
                    {
                        // Skip if the nearby tile is already visited
                        if (visited.Contains(nearbyTile))
                            continue;

                        queue.Enqueue(nearbyTile); // Enqueue the nearby tile
                        visited.Add(nearbyTile); // Mark the nearby tile as visited
                    }
                }
            }
        }

        public List<Tile> GetNearbyTiles(Tile tile, int gridSize)
       {
           nearbyTiles.Clear();
           int row = tile.PosX;
           int col = tile.PosY;

           // Define the size of the area to iterate around the cell
           int areaSize = gridSize - 3; // number of tiles in each direction

           for (int rowIndex = row - areaSize; rowIndex <= row + areaSize; rowIndex++)
           {
               if (rowIndex < 0 || rowIndex >= _currentLevel.grid_width)
               {
                   continue;
               }

               for (int colIndex = col - areaSize; colIndex <= col + areaSize; colIndex++)
               {
                   if (colIndex < 0 || colIndex >= _currentLevel.grid_height)
                   {
                       continue;
                   }

                   Tile tempTile = GetTile(rowIndex, colIndex);
                   if(tempTile != null) nearbyTiles.Add(tempTile);
               }
           }

           return nearbyTiles;
       }

       public bool CheckForTntCombo(Tile clickedTile)
       {
           for (int i = -1; i <= 1; i += 2) //Check for the X-axis
           {
               int targetPosX = clickedTile.PosX + i;
               if (targetPosX >= 0 && targetPosX < _currentLevel.grid_width)
               {
                   Tile targetTile = GetTile(targetPosX, clickedTile.PosY); // Get the target tile
                   
                   if (targetTile != null && targetTile.TileType is TileType.Tnt)
                   {
                       return true;
                   }
               }
           }
           for (int i = -1; i <= 1; i += 2) //Check for the Y-axis
           {
               int targetPosY = clickedTile.PosY + i;
               if (targetPosY >= 0 && targetPosY < _currentLevel.grid_height)
               {
                   Tile targetTile = GetTile(clickedTile.PosX, targetPosY); // Get the target tile
                   
                   if (targetTile != null && targetTile.TileType is TileType.Tnt)
                   {
                       return true;
                   }
               }
           }
           return false;
       }
       private void CheckNeighboursAndDeadlock()
        {
            foreach (Tile tile in tiles)
            {
                if(tile == null)continue;
                if(tile.TileType is TileType.Stone or TileType.Tnt or TileType.Vase or TileType.Box) continue;
                tile.SetNeighbours(CheckBoard(tile));
                CheckForSpriteChange(tile.Neighbours.Count, tile.Neighbours, tile.TileType);
                SetObstacleList(tile);
            }
        }
        private void SetObstacleList(Tile tile)
        {
            foreach (var neighbour in tile.Neighbours)
            {
                neighbour.SetObstacleNeighbours(CalculateObstacleNeighbours(neighbour));
            }
        }
        
        private HashSet<Tile> CalculateObstacleNeighbours(Tile tile)
        {
            obstacleList.Clear();
            for (int i = -1; i <= 1; i += 2) //Check for the X-axis
            {
                int targetPosX = tile.PosX + i;
                if (targetPosX >= 0 && targetPosX < _currentLevel.grid_width)
                {
                    Tile targetTile = GetTile(targetPosX, tile.PosY); // Get the target tile
                   
                    if (targetTile != null && targetTile.TileType is TileType.Box or TileType.Vase)
                    {
                        obstacleList.Add(targetTile);
                    }
                }
            }
            for (int i = -1; i <= 1; i += 2) //Check for the Y-axis
            {
                int targetPosY = tile.PosY + i;
                if (targetPosY >= 0 && targetPosY < _currentLevel.grid_height)
                {
                    Tile targetTile = GetTile(tile.PosX, targetPosY); // Get the target tile
                   
                    if (targetTile != null && targetTile.TileType is TileType.Box or TileType.Vase)
                    {
                        obstacleList.Add(targetTile);
                    }
                }

            }
            return obstacleList;
        }
        public HashSet<Tile> CheckBoard(Tile tile)
        {
            controlList.Clear();
            activeTileList.Clear();
            _controlIndex = 0;
            controlList.Add(tile);

            do
            {
                Tile tempTile = controlList[_controlIndex];
                CalculateNeighbours(tempTile);
                _controlIndex++; // Go to the next tile
            } while (_controlIndex < controlList.Count);

            return activeTileList;
        }
        private void CalculateNeighbours(Tile tile)
        {
            activeTileList.Add(tile);

            for (int i = -1; i <= 1; i += 2) //Check for the X-axis
            {
                int targetPosX = tile.PosX + i;
                if (targetPosX >= 0 && targetPosX < _currentLevel.grid_width)
                {
                    Tile targetTile = GetTile(targetPosX, tile.PosY); // Get the target tile
                    if (targetTile != null && tile.TileType == targetTile.TileType)
                    {

                        activeTileList.Add(tile);

                        if (!controlList.Contains(targetTile))
                        {
                            controlList.Add(targetTile);
                        }
                    }
                }
            }

            for (int i = -1; i <= 1; i += 2) //Check for the Y-axis
            {
                int targetPosY = tile.PosY + i;
                if (targetPosY >= 0 && targetPosY < _currentLevel.grid_height)
                {
                    Tile targetTile = GetTile(tile.PosX, targetPosY); // Get the target tile
                    if (targetTile != null && tile.TileType == targetTile.TileType)
                    {

                        activeTileList.Add(tile);

                        if (!controlList.Contains(targetTile))
                        {
                            controlList.Add(targetTile);
                        }
                    }
                }

            }
        }
        // Get the tile for neighbour checking
        private Tile GetTile(int x, int y)
        {
            int index = (y * _currentLevel.grid_width) + x;
            if (index >= 0 && index < tiles.Count)
            {
                return tiles[index];
            }
            return null;
        }
        // Set the tile for neighbour checking
        public void SetTile(Tile tile,int x, int y)
        {
            int index = (y * _currentLevel.grid_width) + x;
            if (index >= 0 && index < tiles.Count)
            {
                tiles[index] = tile;
            }
        }
        // Check the conditions and give the icons
        private void CheckForSpriteChange(int counter, HashSet<Tile> neighbours,TileType tileType)
        {
            if (counter < 5)
            {
                ChangeSprite(neighbours,GameManager.instance.ItemList[(int)tileType].sprite[0]); // default icon
            }
            else
            { 
                if(GameManager.instance.ItemList[(int)tileType].sprite.Length != 1){
                    ChangeSprite(neighbours,GameManager.instance.ItemList[(int)tileType].sprite[1]); // bomb icon
                }
            }
        }
             
        //Change the sprites of all neighbours
        private void ChangeSprite(HashSet<Tile> neighbours, Sprite sprite)
        {
            foreach (var tile in neighbours)
            {
                tile.SpriteRenderer.sprite = sprite;
            }
        }
        public List<Tile> GetColumn(int index)
        {
            tempList.Clear();
            for (int i = 0; i < _currentLevel.grid_height; i++)
            {
                tempList.Add(tiles[(_currentLevel.grid_width * i) + index]);
            }
            return tempList;
        }

        private void GiveDamage(Tile tile)
        {
            tile.TryGetComponent(out IObstacles obstacle);
            obstacle.TakeDamage();
            if (obstacle.Health == 0)
            {
                gridBoard.objectPools[(int)TileType.Tnt].Return(tile.gameObject);
            }
        }
        private void DestroyTile(Tile tile)
        {
            if (tile.TryGetComponent(out Tnt tnt))
            {
                gridBoard.objectPools[(int)TileType.Tnt].Return(tile.gameObject);
                tnt.DestroyTnt();
                tile.gameObject.SetActive(false);
            }
            else
            {
                gridBoard.objectPools[(int)tile.TileType].Return(tile.gameObject);
                tile.DestroyTile();
                tile.gameObject.SetActive(false);
            }
        }
        private void UseTnt(Tile clickedTile)
        {
            DestroyWithTnt(clickedTile, 5);
        }

        private void UseTntCombo(Tile clickedTile)
        {
            DestroyWithTnt(clickedTile, 7);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (GameManager.instance.GetGridManager() == null) GameManager.instance.SetGridManager(this);
        }

        private void OnEnable()
        {
            GameManager.instance.OnObstacleDamage += GiveDamage;
            GameManager.instance.OnUseTnt += UseTnt;
            GameManager.instance.OnUseTntCombo += UseTntCombo;
            GameManager.instance.OnTileClicked += TileClicked;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameManager.instance.OnTileClicked -= TileClicked;
            GameManager.instance.OnObstacleDamage -= GiveDamage;
            GameManager.instance.OnUseTnt -= UseTnt;
            GameManager.instance.OnUseTntCombo -= UseTntCombo;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
