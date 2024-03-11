using System.Collections.Generic;
using LevelScene.Helpers;
using LevelScene.Managers;
using Obstacles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelScene.Grid
{
    public class GridBoard :MonoBehaviour
    {
        #region Lists
        [SerializeField] private List<Tile> tiles = new List<Tile>(); //List of all tiles that we created
        #endregion

        private Level _level;
        [SerializeField] private GameObject board;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private List<GameObject> tilePrefabs;
        [SerializeField] private SpriteRenderer boardRenderer;
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private GridManager gridManager;

        #region ScaleValues
        private float scaleRateWidth;
        private float scaleRateHeight;
        private float newTileWidth;
        private float newTileHeight;
        private float newBoardWidth;
        private float newBoardHeight;
        private float spawnPointXOffset; // Threshold number for X offset of spawn point 
        private float spawnPointYOffset; // Threshold number for Y offset of spawn point 
        public float NewTileWidth => newTileWidth;
        public float NewTileHeight => newTileHeight;
        public float SpawnPointYOffset => spawnPointYOffset;
        public float SpawnPointXOffset => spawnPointXOffset;
        #endregion
        #region Pools
        private ObjectGameObjectPool blueTilePool;
        private ObjectGameObjectPool redTilePool;
        private ObjectGameObjectPool greenTilePool;
        private ObjectGameObjectPool yellowTilePool;
        private ObjectGameObjectPool stonePool;
        private ObjectGameObjectPool boxPool;
        private ObjectGameObjectPool vasePool;
        private ObjectGameObjectPool tntPool;
        public List<ObjectGameObjectPool> objectPools;
        #endregion

 
        private void Awake()
        {
            blueTilePool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Blue], 75, transform);
            redTilePool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Red], 75, transform);
            greenTilePool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Green], 75, transform);
            yellowTilePool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Yellow], 75, transform);
            stonePool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Stone], 40, transform);
            boxPool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Box], 50, transform);
            vasePool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Vase], 30, transform);
            tntPool = new ObjectGameObjectPool(tilePrefabs[(int)TileType.Tnt], 50, transform);
            objectPools = new List<ObjectGameObjectPool>
            {
                redTilePool,
                greenTilePool,
                blueTilePool,
                yellowTilePool,
                boxPool,
                stonePool,
                vasePool,
                tntPool
            };
        }

        // Generate all tiles
        public List<Tile> GenerateTiles(Level level)
        {
            _level = level;
            List<TileType> tileTypes = GetTileTypeList();
            GameManager.instance.GoalTypes.Clear();
            spawnPointXOffset = (boardRenderer.size.x / 2) - (newTileWidth/2);
            spawnPointYOffset = (boardRenderer.size.y / 2) - (newTileHeight/2) - boardRenderer.transform.position.y;
            for (int i = 0; i < _level.grid_height; i++)
            {
                for (int j = 0; j < _level.grid_width; j++)
                {
                    float xPosition = (j * newTileWidth)- spawnPointXOffset;
                    float yPosition = (i * newTileHeight) - spawnPointYOffset; 
                    Vector2 tilePosition = new Vector2(xPosition,yPosition);
                    int index = (i * level.grid_width) + j;
                    CreateTiles(tileTypes[index],tilePosition,j,i);
                }
            }
            return tiles;
        }
        private Dictionary<string, TileType> stringToTypeMap = new Dictionary<string,TileType>
        {
            { "r", TileType.Red },
            { "b",TileType.Blue},
            { "g", TileType.Green },
            { "y", TileType.Yellow },
            { "bo",TileType.Box },
            { "s", TileType.Stone },
            { "v", TileType.Vase },
            { "t", TileType.Tnt }
        };

        // Function to create the string list based on mapped tile types
        private List<TileType> GetTileTypeList()
        {
            List<TileType> tileTypes = new List<TileType>();
            
            foreach (var stringValue in _level.grid)
            {
                // Check if the current tile type is contained in the list for the mapped string key
                if (stringToTypeMap.ContainsKey(stringValue))
                {
                    // Add the string key to the list
                    tileTypes.Add(stringToTypeMap[stringValue]);
                }
                else if (stringValue == "rand")
                {
                    // Generate a random tile type
                    tileTypes.Add(GenerateRandomTileType());
                }
                
            }
            return tileTypes;
        }

        // Function to generate a random tile type
        private TileType GenerateRandomTileType()
        {
            // Create an array of all tile types
            TileType[] allTileTypes = { TileType.Red, TileType.Blue, TileType.Green, TileType.Yellow };

            // Generate a random index to select a random tile type
            int randomIndex = Random.Range(0, allTileTypes.Length);

            return allTileTypes[randomIndex];
        }

        // Arrange and give some properties to the tiles
        private void CreateTiles(TileType tileType, Vector2 tilePosition, int xAxis, int yAxis)
        {
            Tile tile = objectPools[(int)tileType].Get().GetComponent<Tile>();
            InitializeTiles(tile,xAxis,yAxis,tileType,tilePosition);
            tiles.Add(tile);
            if (tileType is not (TileType.Box or TileType.Vase or TileType.Stone)) return;
            IObstacles obstacle = tile.GetComponent<IObstacles>();
            GameManager.instance.OnObstacleCount?.Invoke(obstacle);
            GameManager.instance.OnGoalType?.Invoke(tileType);
        }

        private void InitializeTiles(Tile tile, int xAxis, int yAxis,TileType tileType, Vector2 tilePosition)
        {
            tile.transform.position = tilePosition;
            tile.SpriteRenderer.sprite = GameManager.instance.ItemList[(int)tileType].sprite[0]; // Give default sprite
            tile.TileType = tileType;
            tile.SetCoordinates(xAxis, yAxis);
            tile.transform.localScale = new Vector2(scaleRateWidth, scaleRateHeight); // Adjust the scale of tile
            tile.gameObject.SetActive(true);
        }
        // Calculate the tile scale according to the board
        public void CalculateTileScale(Level level)
        {
            float originalTileWidth = tileRenderer.bounds.size.x;
            float originalTileHeight = tileRenderer.bounds.size.y;
            float boardWidth = boardRenderer.bounds.size.x;
            float boardHeight = boardRenderer.bounds.size.y;
            newTileWidth = boardWidth / level.grid_width;
            newTileHeight = boardHeight / level.grid_height;
            scaleRateWidth = newTileWidth / originalTileWidth / board.transform.localScale.x *
                             tilePrefab.transform.localScale.x;
            scaleRateHeight = newTileHeight / originalTileHeight / board.transform.localScale.y *
                              tilePrefab.transform.localScale.y;
        }

        public void CreateTnt(Tile clickedTile)
        {
            Tile tnt = objectPools[(int)TileType.Tnt].Get().GetComponent<Tile>();
            Vector2 clickedTilePosition = new Vector2((clickedTile.PosX * newTileWidth) - spawnPointXOffset,
                (clickedTile.PosY * newTileHeight) - spawnPointYOffset);
            InitializeTiles(tnt,clickedTile.PosX,clickedTile.PosY,TileType.Tnt,clickedTilePosition);
            gridManager.SetTile(tnt,clickedTile.PosX,clickedTile.PosY);
        }
        public void GenerateTileAtTop(int x)
        {
            int index = gridManager.FindIndexOfLowestNull(x);
            int newTileYOffset = 3;
            Vector2 newTilePosition = new Vector2((x * newTileWidth) - spawnPointXOffset, (LevelManager.instance.GetCurrentLevel().grid_height + newTileYOffset) - spawnPointYOffset);
            int randomIndex = Random.Range(0, 4);
            Tile topTile = objectPools[randomIndex].Get().GetComponent<Tile>();
            InitializeTiles(topTile,x,index,(TileType)randomIndex,newTilePosition);
            Vector3 targetPosition = new Vector3((x * newTileWidth)- spawnPointXOffset,
                (index * newTileHeight) - spawnPointYOffset, topTile.transform.position.z);
            gridManager.SetTile(topTile,x,index);
            topTile.MoveToTarget(targetPosition);
        }
        public List<Tile> Tiles
        {
            get => tiles;
            set => tiles = value;
        }
    }
}