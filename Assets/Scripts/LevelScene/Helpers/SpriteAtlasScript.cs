using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace LevelScene.Helpers
{
    public class SpriteAtlasScript : MonoBehaviour
    {
        [SerializeField] private SpriteAtlas atlas;
        [SerializeField] private string spriteName;
        
        void Start()
        {
            GetComponent<Image>().sprite = atlas.GetSprite(spriteName);
        }
    }
}
