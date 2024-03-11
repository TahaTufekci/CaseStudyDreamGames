using System;
using System.Collections.Generic;
using LevelScene.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainScene
{
    public class MainUIManager : MonoBehaviour
    {
        public TextMeshProUGUI buttonText;
        // Start is called before the first frame update
        void Start()
        {
            if (GameManager.instance.currentGameState == GameState.Finished)
            {
                buttonText.text = "Finished";
            }
            else
            {
                buttonText.text = $"Level {PlayerPrefs.GetInt("LevelIndex") + 1}";
            }
        }
    
        public void PlayGame()
        {
            if(GameManager.instance.currentGameState == GameState.Finished) return;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        private void OnApplicationQuit()
        {
            LevelManager.instance.CleanSavedData();
        }
    }
}
