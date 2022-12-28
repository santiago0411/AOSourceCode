using System;
using UnityEngine;
using AOClient.Core.Utils;

namespace AOClient.Core
{
    public class Obstacle : MonoBehaviour, IComparable<Obstacle>
    {
        public static int SpawnedObstacles { get; private set; }

        public SpriteRenderer[] SpriteRenderers { get; private set; }

        private Color defaultColor;
        private Color fadedColor;

        private void Start()
        {
            if (transform.parent.name == "ObstaclesTreesDoors")
                SpawnedObstacles++;
            
            var root = transform.Find("Root");
            
            if (root)
                Destroy(root.gameObject);
            
            var collision = Physics2D.OverlapBox(transform.position, Vector2.one, 0f, LayerMask.GetMask(Layer.Map.Name));
            if (!collision)
                return;
            
            var parentMap = collision.GetComponent<Map>();

            switch (tag)
            {
                case "Tree":
                case "ElficTree":
                    transform.SetParent(parentMap.Trees);
                    break;
                case "IronDeposit":
                case "SilverDeposit":
                case "GoldDeposit":
                case "Forge":
                case "Anvil":
                    transform.SetParent(parentMap.Obstacles);
                    break;
                case "Roof":
                    transform.SetParent(parentMap.Roofs);
                    break;
                default:
                    transform.SetParent(parentMap.Obstacles);
                    break;
            }

            SpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var spriteRenderer in SpriteRenderers)
            {
                spriteRenderer.sortingLayerName = "Foreground";
                spriteRenderer.sortingOrder += (int)(parentMap.Boundaries.max.y * 2 - spriteRenderer.transform.position.y * 2);
            }

            defaultColor = SpriteRenderers[0].color;
            fadedColor = SpriteRenderers[0].color;
            fadedColor.a = 0.5f;
        }

        /*Reset SpawnedObstacles to 0 after tilemap is destroy, 
        because if the player logs out the count will sometimes not reset back to 0 
        and the tilemap will get instantly destroyed instead of waiting for the gameobjects to be spawned.*/
        public static void ResetSpawnsCount()
        {
            SpawnedObstacles = 0;
        }

        public int CompareTo(Obstacle other)
        {
            if (SpriteRenderers[0].sortingOrder > other.SpriteRenderers[0].sortingOrder)
                return 1;
            else if (SpriteRenderers[0].sortingOrder < other.SpriteRenderers[0].sortingOrder)
                return -1;

            return 0;
        }

        public void FadeOut()
        {
            if (CompareTag("Tree"))
                foreach (var spriteRender in SpriteRenderers)
                    spriteRender.color = fadedColor;
        }

        public void FadeIn()
        {
            if (CompareTag("Tree"))
                foreach (var spriteRender in SpriteRenderers)
                    spriteRender.color = defaultColor;
        }
    }
}
