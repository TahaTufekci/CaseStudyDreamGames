using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TileType tileType;

    private HashSet<Tile> _neighbours;
    [SerializeField] private HashSet<Tile> _obstacleNeighbours;
    private int _posX, _posY;
    [SerializeField] private ParticleSystem particleSystemPrefab;
    
    private void Awake()
    {
        _obstacleNeighbours = new HashSet<Tile>();
        _neighbours = new HashSet<Tile>();
    }

    public void MoveToTarget(Vector2 targetPos)
    {
        StartCoroutine(MoveCoroutine(targetPos));
    }

    private IEnumerator MoveCoroutine(Vector2 targetPos)
    {
        float duration = 0.2f;
        Vector2 starPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.position = Vector2.Lerp(starPosition, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
    // Set the X and Y coordinates of the tile
    public void SetCoordinates(int x, int y)
    {
        _posX = x;
        _posY = y;
        gameObject.name = "(" + x + ") (" + y + ")";
    }

    // Set the neighboring tiles of this tile
    public void SetNeighbours(HashSet<Tile> neighbourList)
    {
        _neighbours.Clear();
        foreach (var tile in neighbourList)
        {
            _neighbours.Add(tile);
        }
    }

    // Set the neighboring tiles of this tile
    public void SetObstacleNeighbours(HashSet<Tile> obstacleList)
    {
       _obstacleNeighbours.Clear();
        foreach (var tile in obstacleList)
        {
            _obstacleNeighbours.Add(tile);
        }
    }
    public void DestroyTile()
    {
        // Instantiate the particle system
        var particlesInstance = Instantiate(particleSystemPrefab, new Vector3(transform.position.x,transform.position.y,-10), Quaternion.identity);
        // Optionally parent it to the obstacle's parent
        particlesInstance.transform.parent = transform.parent;

        // Play the particle system
        particlesInstance.Play();
        
        // Destroy the tile
        //Destroy(gameObject);
        
        // Subscribe to the particle system's finished event to destroy it after playing
        Destroy(particlesInstance.gameObject, particlesInstance.main.duration);
    }

    public HashSet<Tile> Neighbours
    {
        get => _neighbours;
    }
    public HashSet<Tile> ObstacleNeighbours
    {
        get => _obstacleNeighbours;
    }

    public SpriteRenderer SpriteRenderer
    {
        get => spriteRenderer;
    }

    public TileType TileType
    {
        get => tileType;
        set => tileType = value;
    }

    public int PosX
    {
        get => _posX;
    }

    public int PosY
    {
        get => _posY;
    }
}