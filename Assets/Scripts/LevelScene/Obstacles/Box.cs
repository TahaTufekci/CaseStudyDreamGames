using UnityEngine;

namespace Obstacles
{
    public class Box : MonoBehaviour, IObstacles
    {
        [SerializeField] private int health = 1;
        [SerializeField] private ParticleSystem particleSystemPrefab;
        public int Health => health;

        public void TakeDamage()
        {
            health--;
            if (health == 0) PlayParticle();
        }

        public void PlayParticle()
        {
            // Instantiate the particle system
            var particlesInstance = Instantiate(particleSystemPrefab, new Vector3(transform.position.x,transform.position.y,-10), Quaternion.identity);
            // Optionally parent it to the obstacle's parent
            particlesInstance.transform.parent = transform.parent;

            // Play the particle system
            particlesInstance.Play();

            // Subscribe to the particle system's finished event to destroy it after playing
            Destroy(particlesInstance.gameObject, particlesInstance.main.duration);
        }
    }
}