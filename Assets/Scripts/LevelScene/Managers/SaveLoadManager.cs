using System;
using System.Collections;
using System.Collections.Generic;
using LevelScene.Helpers;
using UnityEngine;

namespace LevelScene.Managers
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        [SerializeField] private Level levelData;
        
        public void SaveData(Level currentLevel)
        {
            string data = JsonUtility.ToJson(currentLevel);
            PlayerPrefs.SetString("Save", data);
            Debug.Log("Data saved. Data: \n" + data);
        }
        public void CleanData()
        {
            PlayerPrefs.SetString("Save", null);
            Debug.Log("Data null saved. Data: \n");
        }

        public Level LoadData()
        {
            string data = PlayerPrefs.GetString("Save", string.Empty);
            if (data == string.Empty)
            {
                Debug.Log("No save data found");
                return null;
            }

            Level savedLevel = JsonUtility.FromJson<Level>(data);
            
            levelData.level_number = savedLevel.level_number;
            levelData.grid_width = savedLevel.grid_width;
            levelData.grid_height = savedLevel.grid_height;
            levelData.move_count = savedLevel.move_count;
            levelData.grid = savedLevel.grid;
            return levelData;
        }
    }
}