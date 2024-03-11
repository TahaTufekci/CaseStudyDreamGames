using LevelScene.Obstacles;
using Obstacles;
using UnityEngine;

namespace LevelScene.Managers
{
    public class InputManager : MonoBehaviour
    {
        private Vector3 _position;
        private Camera _mainCamera;
        private RaycastHit2D[] _hit;

        private int _hitCount;
        private bool _checkResume = true;
        [SerializeField] private GridManager gridManager;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _hit = new RaycastHit2D[1];
        }

        private void Update()
        {
            if (!_checkResume) return; // Check if input can be processed

            GenerateInput();
        }

        private void GenerateInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _position = Input.mousePosition;
                _position.z = _mainCamera.transform.position.z;
                GameObject hitObject = GetHitObject();
                if (hitObject == null)
                {
                    return;
                }
                Tile hitTile = hitObject.GetComponent<Tile>();
                if (hitObject.TryGetComponent(out Stone stone) || hitObject.TryGetComponent(out Box box) || hitObject.TryGetComponent(out Vase vase))
                {
                    return;
                }
                if (hitObject.TryGetComponent(out Tnt tnt))
                {
                    GameManager.instance.OnMovePlayed?.Invoke();
                    if (gridManager.CheckForTntCombo(hitTile))
                    {
                        GameManager.instance.OnUseTntCombo?.Invoke(hitTile);
                    }
                    else
                    {
                        GameManager.instance.OnUseTnt?.Invoke(hitTile);
                    }
                    return;
                }

                if (!hitObject.TryGetComponent(out Tile tile)) return;
                GameManager.instance.OnMovePlayed?.Invoke();
                GameManager.instance.OnTileClicked?.Invoke(tile);
            }
        }

        private GameObject GetHitObject()
        {
            _hitCount = Physics2D.RaycastNonAlloc(_mainCamera.ScreenToWorldPoint(_position), Vector2.zero, _hit);
            if (_hitCount > 0 && _hit[0].collider != null)
            {
                return _hit[0].collider.gameObject;
            }
            return null;
        }

        private void ValidateInput(GameState gameState)
        {
            _checkResume = gameState.HasFlag(GameState.Playing);
        }

        private void OnEnable()
        {
            GameManager.instance.OnGameStateChanged += ValidateInput;
        }

        private void OnDisable()
        {
            GameManager.instance.OnGameStateChanged -= ValidateInput;
        }
    }
}
