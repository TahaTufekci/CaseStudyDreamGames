using System;
using System.Collections.Generic;
using DG.Tweening;
using LevelScene.Obstacles;
using Obstacles;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LevelScene.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup losePanel;
        [SerializeField] private CanvasGroup winPanel;
        [SerializeField] private Image mainMask;
        [SerializeField] private GameObject[] goalPrefabs; // Prefab for obstacle UI element
        [SerializeField] private RectTransform goalParent; // Parent object for obstacle UI elements
        [SerializeField] private Image goalCheckImage;
        [SerializeField] private TextMeshProUGUI moveText;
        [SerializeField] private TextMeshProUGUI boxText;
        [SerializeField] private TextMeshProUGUI stoneText;
        [SerializeField] private TextMeshProUGUI vaseText;

        private void Start()
        {
            ShowObstacleTypes();
            moveText.text = LevelManager.instance.currentLevel.move_count.ToString();
        }
        
        private void ShowObstacleTypes()
        {
            // Remove existing obstacle UI elements
            foreach (RectTransform child in goalParent)
            {
                Destroy(child.gameObject);
            }

            foreach (TileType goalType in GameManager.instance.GoalTypes)
            {
                switch (goalType)
                {
                    case TileType.Box:
                        GameObject box = Instantiate(goalPrefabs[0], goalParent);
                        AddObstacleUI(box,GameManager.instance.BoxCounter, goalType, boxText);
                        break;
                    case TileType.Stone:
                        GameObject stone = Instantiate(goalPrefabs[1], goalParent);
                        AddObstacleUI(stone, GameManager.instance.StoneCounter, goalType, stoneText);
                        break;
                    case TileType.Vase:
                        GameObject vase = Instantiate(goalPrefabs[2], goalParent);
                        AddObstacleUI(vase, GameManager.instance.VaseCounter, goalType, vaseText);
                        break;
                }
            }
        }

        private void AddObstacleUI(GameObject obstacle, int count, TileType tileType, TextMeshProUGUI obstacleCountText)
        {
            obstacleCountText = obstacle.GetComponentInChildren<TextMeshProUGUI>();
            obstacleCountText.text = count.ToString();
            
            var obstacleImage = obstacle.GetComponent<Image>();
            obstacleImage.sprite = GameManager.instance.ItemList[(int)tileType].sprite[0];
 
            switch (tileType)
            {
                case TileType.Box:
                    boxText = obstacleCountText;
                    obstacle.name = $"{tileType}";
                    break;
                case TileType.Stone:
                    stoneText = obstacleCountText;
                    obstacle.name = $"{tileType}";
                    break;
                case TileType.Vase:
                    vaseText = obstacleCountText;
                    obstacle.name = $"{tileType}";
                    break;
            }
        }
        private void UpdateObstacleCount(IObstacles obstacle)
        {
            switch (obstacle)
            {
                case Box:
                    boxText.text = GameManager.instance.BoxCounter.ToString();
                    ShowGoalCheck(GameManager.instance.BoxCounter,boxText);
                    break;
                case Stone:
                    stoneText.text = GameManager.instance.StoneCounter.ToString();
                    ShowGoalCheck(GameManager.instance.StoneCounter,stoneText);
                    break;
                case Vase:
                    vaseText.text = GameManager.instance.VaseCounter.ToString();
                    ShowGoalCheck(GameManager.instance.VaseCounter,vaseText);
                    break;
            }
        }

        private void ShowGoalCheck(int counter, TextMeshProUGUI counterText)
        {
            if (counter == 0)
            {
                counterText.gameObject.SetActive(false);
            }
        }

        private void SetMaskState(Image mask, bool isActive, Action onClickAction = null)
        {
            if (isActive)
            {
                SetMaskClickAction(mask, onClickAction);
                mask.gameObject.SetActive(true);
            }
            else
            {
                mask.gameObject.SetActive(false);
            }
        }

        private void SetMaskClickAction(Image mask, Action action)
        {
            EventTrigger trigger = mask.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            trigger.triggers.Clear();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { action?.Invoke(); });
            trigger.triggers.Add(entry);
        }

        private void ControlPanels(GameState gameState)
        {
            mainMask.DOFade(0.5f, 0.5f).SetDelay(0.8f).From(0f);
            SetMaskState(mainMask, true);

            Sequence sequence = DOTween.Sequence();
            float delay = 0.8f;

            if (gameState.HasFlag(GameState.Lose))
            {
                sequence.PrependInterval(delay).OnComplete(() => FadeInLosePanel());
            }
            else if (gameState.HasFlag(GameState.Win))
            {
                delay += 0.2f; // Add additional delay for win panel
                sequence.PrependInterval(delay).OnComplete(() => Celebration());
            }
        }

        private void Celebration()
        {
            winPanel.gameObject.SetActive(true);
        }

        public void FadeInLosePanel()
        {
            losePanel.gameObject.SetActive(true);
            losePanel.gameObject.transform.DOScale(new Vector3(0.6f,0.6f), 0.5f).From(Vector3.zero);
        }
        private void UpdateMoveCount()
        {
            moveText.text = GameManager.instance.GetCurrentMoveCount().ToString();
        }
        private void OnEnable()
        {
            GameManager.instance.OnValidTap += UpdateMoveCount;
            GameManager.instance.OnObstacleDestroy += UpdateObstacleCount;
            GameManager.instance.OnGameStateChanged += ControlPanels;
        }

        private void OnDisable()
        {
            GameManager.instance.OnValidTap -= UpdateMoveCount;
            GameManager.instance.OnObstacleDestroy -= UpdateObstacleCount;
            GameManager.instance.OnGameStateChanged -= ControlPanels;
        }
    }
}
