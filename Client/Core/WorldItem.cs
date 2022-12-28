using UnityEngine;

namespace AOClient.Core
{
    public class WorldItem : MonoBehaviour, IPoolObject
    {
        public int InstanceId { get; private set; }
        public bool IsBeingUsed => gameObject.activeSelf;
    
        private SpriteRenderer spriteRenderer;
    
        public void Initialize(int id, Sprite sprite)
        {
            InstanceId = id;
            if (!spriteRenderer)
                spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            gameObject.SetActive(true);
        }
    
        public void ResetPoolObject()
        {
            gameObject.SetActive(false);
        }
    }
}
