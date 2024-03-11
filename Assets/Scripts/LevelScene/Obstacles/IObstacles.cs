namespace Obstacles
{
    public interface IObstacles
    {
        int Health { get; } // Property to get the health of the obstacle
        void TakeDamage();
        void PlayParticle();
    }
}