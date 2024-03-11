using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LevelScene.UIPanels
{
    public class WinPanel : MonoBehaviour
    {
        [SerializeField] private Image star;
        [SerializeField] private ParticleSystem particleSystemPrefab;

        private void OnEnable()
        {
            // Set initial scale and rotation
            star.rectTransform.localScale = new Vector3(20f, 20f, 1f);
            star.rectTransform.rotation = Quaternion.Euler(0f, 0f, 540f);

            // Define the tween sequence
            Sequence sequence = DOTween.Sequence();
            
            // Add scale tween
            Tween scaleTween = star.rectTransform.DOScale(new Vector3(5f, 5f, 1f), 2f);
            sequence.Join(scaleTween);
            
            // Add rotation tween
            Tween rotationTween = star.rectTransform.DORotate(new Vector3(0f, 0f, 0), 2f);
            sequence.Join(rotationTween);

            // Callback when the sequence completes
            sequence.OnComplete(() => GoBackToMain());

            // Instantiate and play particle system
            InstantiateAndPlayParticleSystem();
        }
        
        private void InstantiateAndPlayParticleSystem()
        {
            // Instantiate particle system
            ParticleSystem particles = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity);
            
            // Optionally parent it to the obstacle's parent
            particles.transform.parent = transform.parent;
            
            // Play the particle system
            particles.Play();
        }

        private void GoBackToMain()
        {
            SceneManager.LoadScene(0);
        }
    }
}