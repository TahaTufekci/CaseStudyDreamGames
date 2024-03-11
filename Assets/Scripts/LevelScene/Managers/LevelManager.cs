using System;
using System.Collections.Generic;
using DG.Tweening;
using LevelScene.Helpers;
using UnityEngine;

namespace LevelScene.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        public Level currentLevel;
        public List<Level> levelList = new List<Level>();
        [SerializeField]private LevelReader levelReader;
        [SerializeField]private SaveLoadManager saveLoadManager;
        public bool isLevelSaved;
        public int CurrentLevelIndex { get; set; } = 0;


        void Start()
        {
            levelList = levelReader.GetAllLevels();
        }

        public void InitializeLevel()
        {
            Level levelData = saveLoadManager.LoadData();
            // For the first level
            if (levelData != null)
            {
                SetCurrentLevel(levelData);
            }
            else
            {
                LoadLevel();
            }
        }
        public void LoadLevel()
        {
            GameManager.instance.RefreshLevel();
            isLevelSaved = false;
            // Check if there's a next level
            int savedLevelIndex = PlayerPrefs.GetInt("LevelIndex");
            if (savedLevelIndex < levelList.Count)
            {
                Level level = levelList[savedLevelIndex];
                SetCurrentLevel(level);
            }
            else
            {
                Debug.Log("No more levels to load.");
            }

            DOTween.KillAll();
        }
     
        public void SetCurrentLevel(Level level)
        {
            currentLevel = level;
        }
        public Level GetCurrentLevel()
        {
            return currentLevel;
        }
        private Dictionary<TileType, string> typeToStringMap = new Dictionary<TileType, string>
        {
            //{ "rand", new List<TileType> { TileType.Red, TileType.Blue, TileType.Green, TileType.Yellow } },
            { TileType.Red, "r" },
            { TileType.Blue, "b"  },
            { TileType.Green,"g" },
            { TileType.Yellow ,"y"},
            { TileType.Box,"bo" },
            { TileType.Stone,"s" },
            { TileType.Tnt, "t" },
            { TileType.Vase, "v" }
        };

        private List<string> GetTileTypeList()
        {
            List<string> savedTiles = new List<string>();

            foreach (Tile tile in GameManager.instance.GetGridManager().Tiles)
            {
                // Check if the current tile type is contained in the list for the mapped string key
                if (typeToStringMap.ContainsKey(tile.TileType))
                {
                    // Add the string key to the list
                    savedTiles.Add(typeToStringMap[tile.TileType]);
                }
            }
            return savedTiles;
        }
        
        private Level SaveLevelData()
        {
            Level savedLevel = new Level
            {
                move_count = GameManager.instance.GetCurrentMoveCount(),
                grid = GetTileTypeList().ToArray(),
                grid_height = currentLevel.grid_height,
                grid_width = currentLevel.grid_width,
                level_number = currentLevel.level_number
            };
            return savedLevel;
        }

        public void SaveLevel()
        {
            saveLoadManager.SaveData(SaveLevelData());
        }
        public void CleanSavedData()
        {
            saveLoadManager.CleanData();
        }
        private void OnApplicationQuit()
        {
            if (GameManager.instance.currentGameState != GameState.Win)
            {
                SaveLevel();
            }
        }
    }
}
