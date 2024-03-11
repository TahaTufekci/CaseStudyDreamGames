using System;
using System.Collections.Generic;
using LevelScene.Helpers;
using LevelScene.Obstacles;
using Obstacles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelScene.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GridManager gridManager;
        #region Actions
        public Action<Tile> OnUseTnt;
        public Action<Tile> OnUseTntCombo;
        public Action<Tile> OnObstacleDamage;
        public Action<Tile> OnTileClicked;
        public Action OnMovePlayed;
        public Action<IObstacles>OnObstacleCount;
        public Action<IObstacles> OnObstacleDestroy;
        public Action<GameState> OnGameStateChanged;
        public Action<TileType> OnGoalType;
        public Action OnValidTap;
        #endregion
        
        #region integers
        private int _boxCounter;
        private int _stoneCounter;
        private int _vaseCounter;
        public int BoxCounter => _boxCounter;
        public int StoneCounter => _stoneCounter;
        public int VaseCounter => _vaseCounter;
        private int _moveCount;
        public int MoveCount => _moveCount;
        #endregion

        public GameState currentGameState;
        
        private HashSet<TileType> goalTypes = new HashSet<TileType>();
        public HashSet<TileType> GoalTypes => goalTypes;
        [SerializeField] private List<Item> itemList = new List<Item>(); // Items that we use to change the sprites of tiles
        public List<Item> ItemList => itemList;

        public void ChangeGameState(GameState state)
        {
            if (currentGameState != state)
            {
                currentGameState = state;
                OnGameStateChanged?.Invoke(state);
            }
        }

        private void CountObstacles(IObstacles obstacle)
        {
            switch (obstacle)
            {
                case Box:
                    _boxCounter++;
                    break;
                case Stone:
                    _stoneCounter++;
                    break;
                case Vase:
                    _vaseCounter++;
                    break;
            }
        }

        private void DecrementObstacleCount(IObstacles obstacle)
        {
            switch (obstacle)
            {
                case Box:
                    _boxCounter--;
                    break;
                case Stone:
                    _stoneCounter--;
                    break;
                case Vase:
                    _vaseCounter--;
                    break;
            }

            CheckLevelFinished();
        }
        private void SetGoalType(TileType goalType)
        {
            goalTypes.Add(goalType);
        }

        private void CheckLevelFinished()
        {
            if (_boxCounter == 0 && _stoneCounter == 0 && _vaseCounter == 0)
            {
                var savedIndex = PlayerPrefs.GetInt("LevelIndex");
                if (savedIndex > 0)
                {
                    PlayerPrefs.SetInt("LevelIndex",savedIndex + 1);
                }
                else
                {
                    PlayerPrefs.SetInt("LevelIndex",++LevelManager.instance.CurrentLevelIndex);
                }
                if (savedIndex.Equals(LevelManager.instance.levelList.Count - 1))
                {
                    ChangeGameState(GameState.Finished);
                    PlayerPrefs.SetInt("LevelIndex",0);
                    LevelManager.instance.CleanSavedData();
                    SceneManager.LoadScene(0);
                }
                else
                {
                    ChangeGameState(GameState.Win);
                    LevelManager.instance.CleanSavedData();
                }
            }
        } 
        public void RefreshLevel()
        {
            _boxCounter = 0;
            _stoneCounter = 0;
            _vaseCounter = 0;
            currentGameState = GameState.Playing;
        }
  
        private void DecrementMoveCount()
        {
            SetCurrentMoveCount(GetCurrentMoveCount() - 1);
            if (_moveCount == 0)
            {
                ChangeGameState(GameState.Lose);
                LevelManager.instance.CleanSavedData();
            }
        }
        public int GetCurrentMoveCount()
        {
            return _moveCount;
        }

        public void SetCurrentMoveCount(int moveCount)
        {
            _moveCount = moveCount;
        }  
        public GridManager GetGridManager()
        {
            return gridManager;
        }
        public void SetGridManager(GridManager manager)
        {
            gridManager = manager;
        }
        private void OnEnable()
          {
              OnObstacleCount += CountObstacles;
              OnObstacleDestroy += DecrementObstacleCount;
              OnGoalType += SetGoalType;
              OnValidTap += DecrementMoveCount;
          }

        private void OnDisable()
          {
              OnObstacleCount -= CountObstacles;
              OnObstacleDestroy -= DecrementObstacleCount;
              OnGoalType -= SetGoalType;
              OnValidTap -= DecrementMoveCount;
          }
     
    }
}
