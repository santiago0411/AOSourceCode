using AO.Core.Logging;
using UnityEngine;

namespace AO.Core.Utils
{
    public static class CollisionManager
    {
        public static readonly LayerMask PlayerAndNpcLayerMask = LayerMask.GetMask(Layer.Player.Name, Layer.Npc.Name);
        public static readonly LayerMask ClickCollisionLayerMask = LayerMask.GetMask(Layer.WorldItem.Name, Layer.Obstacles.Name);
        public static readonly LayerMask ObstaclesLayerMask = LayerMask.GetMask(Layer.Obstacles.Name);
        public static readonly LayerMask MapLayerMask = LayerMask.GetMask(Layer.Map.Name);
        public static readonly LayerMask BackgroundLayerMask = LayerMask.GetMask(Layer.Background.Name);

        private static readonly LoggerAdapter log = new(typeof(CollisionManager));

        /// <summary>
        /// Casts a RaycastHit2D to check for linear collisions towards where the direction on the specified LayerMask. Returns null if there was no collision.
        /// </summary>
        /// <param name="position">The position at where the player currently is.</param>
        /// <param name="direction">The direction towards where the ray will be casted.</param>
        /// <param name="layerMask">The LayerMask on which the collisions will be checked.</param>
        public static RaycastHit2D CheckLinearCollision(Vector2 position, Vector2 direction, LayerMask layerMask)
        {
            return Physics2D.Raycast(position, direction, .48f, layerMask);
        }

        /// <summary>
        /// Checks for collisions in a square around the given position and the specified LayerMasks. Returns null if there was no collision.
        /// </summary>
        /// <param name="position">The position at where the collisions will be checked.</param>
        /// <param name="layerMask">The layers at where the collision will be checked.</param>
        public static Collider2D CheckOverlapSquare(Vector2 position, LayerMask layerMask)
        {
            return Physics2D.OverlapBox(position, Vector2.one / 2, 0f, layerMask);
        }

        /// <summary>Checks for a collision where the player clicked and one position lower if it clicked on a player head.</summary>
        public static Collider2D CheckClickCollision(Players.Player player, Vector2 clickPosition)
        {
            clickPosition.Round(0);

            if (player.CurrentMap.Boundaries.Contains(clickPosition))
            {
                if (Mathf.Abs(clickPosition.x - player.CurrentTile.Position.x) > Constants.VISION_RANGE_X || Mathf.Abs(clickPosition.y - player.CurrentTile.Position.y) > Constants.VISION_RANGE_Y)
                {
                    //Ban
                    log.Error("Clicked on an invalid position");
                    return null;
                }
            }

            // TODO profile this part to see if it would be faster to get player or npc from tile
            Collider2D collision = CheckOverlapSquare(clickPosition, PlayerAndNpcLayerMask);

            if (!collision)
            {
                //If no collision was found on the tile the player clicked check one position below to see if they clicked on a player's head
                collision = CheckOverlapSquare(new Vector3(clickPosition.x, clickPosition.y - 1f, 0f), LayerMask.GetMask("Player"));
            }

            if (!collision)
            {
                //If no player or npc collision was found check for items and obstacles collisions
                collision = CheckOverlapSquare(clickPosition, ClickCollisionLayerMask);
            }

            return collision;
        }
    }
}
