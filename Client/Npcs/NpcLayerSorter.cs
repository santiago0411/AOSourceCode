using System.Collections.Generic;
using UnityEngine;
using AOClient.Core;
using AOClient.Core.Utils;

namespace AOClient.Npcs
{
    public class NpcLayerSorter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer head;
        [SerializeField] private SpriteRenderer body;

        private readonly List<Obstacle> obstacles = new();

        private const byte SORT_ORDER_ORIGINAL = 200;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == Layer.Obstacles.Id)
            {
                var obstacle = collision.GetComponent<Obstacle>();
                if (!obstacle)
                    obstacle = collision.GetComponentInParent<Obstacle>();

                if (obstacles.Count == 0 || obstacle.SpriteRenderers[0].sortingOrder - 1 < body.sortingOrder)
                {
                    head.sortingOrder = obstacle.SpriteRenderers[0].sortingOrder - 1;
                    body.sortingOrder = obstacle.SpriteRenderers[0].sortingOrder - 1;
                }

                obstacles.Add(obstacle);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == Layer.Obstacles.Id)
            {
                var obstacle = collision.GetComponent<Obstacle>();
                obstacles.Remove(obstacle);

                if (obstacles.Count == 0)
                {
                    head.sortingOrder = SORT_ORDER_ORIGINAL;
                    body.sortingOrder = SORT_ORDER_ORIGINAL;
                }
                else
                {
                    obstacles.Sort();
                    head.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
                    body.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
                }
            }
        }
    }
}

