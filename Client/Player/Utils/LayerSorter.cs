using System.Collections.Generic;
using AOClient.Core;
using AOClient.Network;
using UnityEngine;

namespace AOClient.Player.Utils
{ 
    public class LayerSorter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer helm;
        [SerializeField] private SpriteRenderer head;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer shield;
        [SerializeField] private SpriteRenderer weapon;
        [SerializeField] private Canvas nameTag;
        
        private PlayerManager thisPlayer;

        private readonly List<Obstacle> obstacles = new();

        private const byte SORT_ORDER_ORIGINAL = 200;

        private void Start()
        {
            thisPlayer = GetComponentInParent<PlayerManager>();
        }

        public void OnObstacleCollisionEnter(Collider2D collision)
        {
            var obstacle = collision.GetComponentInParent<Obstacle>();

            if (thisPlayer.Id == Client.Instance.MyId)
                obstacle.FadeOut();

            foreach (var spriteRenderer in obstacle.SpriteRenderers)
            {
                if (obstacles.Count == 0 || spriteRenderer.sortingOrder - 1 < body.sortingOrder)
                {
                    var sortingOrder = spriteRenderer.sortingOrder;
                    helm.sortingOrder = sortingOrder;
                    head.sortingOrder = sortingOrder - 1;
                    body.sortingOrder = sortingOrder - 1;
                    shield.sortingOrder = sortingOrder - 1;
                    weapon.sortingOrder = sortingOrder - 1;
                    nameTag.sortingOrder = sortingOrder - 1;
                }
            }


            obstacles.Add(obstacle);
        }

        public void OnObstacleCollisionExit(Collider2D collision)
        {
            var obstacle = collision.GetComponentInParent<Obstacle>();

            if (thisPlayer.Id == Client.Instance.MyId)
                obstacle.FadeIn();

            obstacles.Remove(obstacle);

            if (obstacles.Count == 0)
            {
                helm.sortingOrder = SORT_ORDER_ORIGINAL;
                head.sortingOrder = SORT_ORDER_ORIGINAL;
                body.sortingOrder = SORT_ORDER_ORIGINAL;
                shield.sortingOrder = SORT_ORDER_ORIGINAL;
                weapon.sortingOrder = SORT_ORDER_ORIGINAL;
                nameTag.sortingOrder = SORT_ORDER_ORIGINAL;
            }
            else
            {
                obstacles.Sort();
                helm.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder;
                head.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
                body.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
                shield.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
                weapon.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
                nameTag.sortingOrder = obstacles[0].SpriteRenderers[0].sortingOrder - 1;
            }
        }
    }
}