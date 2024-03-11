using DG.Tweening;
using LevelScene.Grid;
using LevelScene.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LosePanel : MonoBehaviour
{
    public void TryAgain()
    {
        DOTween.KillAll();
        LevelManager.instance.SetCurrentLevel(LevelManager.instance.currentLevel);
        SceneManager.LoadScene(1);
    }

    public void MainMenuButton()
    {
        LevelManager.instance.SetCurrentLevel(LevelManager.instance.currentLevel);
        DOTween.KillAll();
        SceneManager.LoadScene(0);
    }
    public void OnEnable()
    {
        EnableAnimation();
    }

    public void EnableAnimation()
    {
        gameObject.transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero);
    }

}

